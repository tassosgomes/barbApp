# Especificação Técnica - Gestão de Barbeiros (Admin da Barbearia)

## Resumo Executivo

O módulo de Gestão de Barbeiros permite que Admins de Barbearia gerenciem sua equipe de profissionais e monitorem agendamentos de forma centralizada. A solução implementa CRUD completo de barbeiros com isolamento multi-tenant rigoroso, onde cada barbeiro é independente por barbearia (mesmo telefone pode existir em múltiplas barbearias com dados completamente isolados). A arquitetura segue Clean Architecture com Domain (entidades e regras), Application (use cases), Infrastructure (repositórios e DbContext) e API (controllers REST). O sistema inclui visualização consolidada de agenda com atualização via polling (30s), gestão de serviços por barbearia (texto livre com título, descrição, valor e duração) e validações robustas para garantir integridade de dados. Barbeiros removidos ficam bloqueados para novos agendamentos mas agendamentos futuros são mantidos.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Domain Layer** (`BarbApp.Domain`)
- **Entidades**: `Barber` (barbeiro vinculado a barbearia), `BarbershopService` (serviços oferecidos pela barbearia)
- **Value Objects**: `PhoneNumber` (telefone normalizado), `ServiceDuration` (duração do serviço)
- **Exceções**: `BarberNotFoundException`, `DuplicateBarberException`, `BarberHasFutureAppointmentsException`
- **Interfaces**: `IBarberRepository`, `IBarbershopServiceRepository`, `IAppointmentRepository`

**Application Layer** (`BarbApp.Application`)
- **Use Cases**:
  - `CreateBarberUseCase` (adicionar barbeiro à equipe)
  - `UpdateBarberUseCase` (editar informações do barbeiro)
  - `RemoveBarberUseCase` (remover barbeiro com validações)
  - `ListBarbersUseCase` (listar equipe com filtros)
  - `GetBarberByIdUseCase` (detalhes de barbeiro específico)
  - `GetTeamScheduleUseCase` (agenda consolidada de todos os barbeiros)
  - `CreateBarbershopServiceUseCase` (cadastrar serviço da barbearia)
  - `ListBarbershopServicesUseCase` (listar serviços disponíveis)
- **DTOs**: Inputs/Outputs para todas as operações
- **Validators**: FluentValidation para validação de telefone, nome, etc.

**Infrastructure Layer** (`BarbApp.Infrastructure`)
- **Repositories**: Implementações concretas com EF Core e Global Query Filters
- **DbContext**: Extensão de `BarbAppDbContext` com novas entidades
- **Mappings**: Entity Type Configurations para `Barber` e `BarbershopService`

**API Layer** (`BarbApp.API`)
- **Controllers**:
  - `BarbersController` (CRUD de barbeiros, agenda consolidada)
  - `BarbershopServicesController` (gestão de serviços)
- **Authorization**: `[Authorize(Roles = "AdminBarbearia")]` em todos os endpoints
- **Pagination**: Suporte a `limit` e `offset` para listagens

### Fluxo de Dados

```
Frontend React (Admin Barbearia)
    ↓ HTTP Request (Cookie JWT com barbeariaId)
BarbersController
    ↓ [Authorize] + TenantMiddleware (extrai barbeariaId)
Use Case (Application)
    ↓ Business Logic + Validations
Repository (Infrastructure)
    ↓ EF Core + Global Query Filter (WHERE barbearia_id = ?)
PostgreSQL Database
```

**Fluxo de Criação de Barbeiro**:
1. Admin preenche formulário (nome, telefone, serviços)
2. Frontend envia `POST /api/barbers` com dados
3. Controller valida autorização (Admin da Barbearia)
4. `CreateBarberUseCase` valida dados e telefone único na barbearia
5. Cria entidade `Barber` com `barbeariaId` do contexto
6. Repository persiste no banco
7. `UnitOfWork.Commit()` confirma transação
8. Retorna `201 Created` com dados do barbeiro criado

**Fluxo de Visualização de Agenda**:
1. Admin acessa tela de agenda
2. Frontend faz `GET /api/barbers/schedule?date=2025-10-11`
3. `GetTeamScheduleUseCase` busca todos os barbeiros da barbearia
4. Busca agendamentos do dia para cada barbeiro (via `IAppointmentRepository`)
5. Agrupa e organiza dados por barbeiro e horário
6. Retorna agenda consolidada com filtros aplicados
7. Frontend atualiza automaticamente via polling a cada 30s

## Design de Implementação

### Interfaces Principais

```csharp
// Domain - Repository Interfaces
public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Barber?> GetByPhoneAsync(Guid barbeariaId, string phone, CancellationToken cancellationToken);
    Task<List<Barber>> ListAsync(
        Guid barbeariaId,
        bool? isActive,
        string? searchName,
        int limit,
        int offset,
        CancellationToken cancellationToken);
    Task<int> CountAsync(Guid barbeariaId, bool? isActive, string? searchName, CancellationToken cancellationToken);
    Task InsertAsync(Barber barber, CancellationToken cancellationToken);
    Task UpdateAsync(Barber barber, CancellationToken cancellationToken);
}

public interface IBarbershopServiceRepository
{
    Task<BarbershopService?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<BarbershopService>> ListByBarbeariaAsync(Guid barbeariaId, CancellationToken cancellationToken);
    Task InsertAsync(BarbershopService service, CancellationToken cancellationToken);
    Task UpdateAsync(BarbershopService service, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetByDateRangeAsync(
        Guid barbeariaId,
        DateTime startDate,
        DateTime endDate,
        Guid? barberId,
        CancellationToken cancellationToken);
    Task<int> CountFutureAppointmentsByBarberAsync(Guid barberId, CancellationToken cancellationToken);
}
```

### Modelos de Dados

**Entidade Barber (Domain)**

```csharp
public class Barber
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Name { get; private set; }
    public string Phone { get; private set; } // Apenas números (normalizado)
    public List<Guid> ServiceIds { get; private set; } // IDs dos serviços que oferece
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Barber() { } // EF Core

    public static Barber Create(
        Guid barbeariaId,
        string name,
        string phone,
        List<Guid> serviceIds)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        var cleanPhone = PhoneNumber.Normalize(phone);
        if (!PhoneNumber.IsValid(cleanPhone))
            throw new ArgumentException("Invalid phone number", nameof(phone));

        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Name = name.Trim(),
            Phone = cleanPhone,
            ServiceIds = serviceIds ?? new List<Guid>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, List<Guid> serviceIds)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        Name = name.Trim();
        ServiceIds = serviceIds ?? new List<Guid>();
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhone(string phone)
    {
        var cleanPhone = PhoneNumber.Normalize(phone);
        if (!PhoneNumber.IsValid(cleanPhone))
            throw new ArgumentException("Invalid phone number", nameof(phone));

        Phone = cleanPhone;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Entidade BarbershopService (Domain)**

```csharp
public class BarbershopService
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Title { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationMinutes { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private BarbershopService() { } // EF Core

    public static BarbershopService Create(
        Guid barbeariaId,
        string title,
        string description,
        decimal price,
        int durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters", nameof(title));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (durationMinutes <= 0 || durationMinutes > 480)
            throw new ArgumentException("Duration must be between 1 and 480 minutes", nameof(durationMinutes));

        return new BarbershopService
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Title = title.Trim(),
            Description = description?.Trim() ?? string.Empty,
            Price = price,
            DurationMinutes = durationMinutes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string title, string description, decimal price, int durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (title.Length > 100)
            throw new ArgumentException("Title cannot exceed 100 characters", nameof(title));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (durationMinutes <= 0 || durationMinutes > 480)
            throw new ArgumentException("Duration must be between 1 and 480 minutes", nameof(durationMinutes));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Price = price;
        DurationMinutes = durationMinutes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Value Object - PhoneNumber (Domain)**

```csharp
public static class PhoneNumber
{
    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        return Regex.Replace(phone, @"[^\d]", "");
    }

    public static bool IsValid(string normalizedPhone)
    {
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            return false;

        // Brasil: 10 ou 11 dígitos (com ou sem 9 no celular)
        return normalizedPhone.Length >= 10 && normalizedPhone.Length <= 11;
    }

    public static string Format(string normalizedPhone)
    {
        if (normalizedPhone.Length == 11)
            return $"({normalizedPhone.Substring(0, 2)}) {normalizedPhone.Substring(2, 5)}-{normalizedPhone.Substring(7)}";

        if (normalizedPhone.Length == 10)
            return $"({normalizedPhone.Substring(0, 2)}) {normalizedPhone.Substring(2, 4)}-{normalizedPhone.Substring(6)}";

        return normalizedPhone;
    }
}
```

**DTOs (Application)**

```csharp
// Inputs
public record CreateBarberInput(
    string Name,
    string Phone,
    List<Guid> ServiceIds
);

public record UpdateBarberInput(
    Guid Id,
    string Name,
    List<Guid> ServiceIds
);

public record UpdateBarberPhoneInput(
    Guid Id,
    string Phone
);

public record ListBarbersInput(
    bool? IsActive,
    string? SearchName,
    int Limit = 50,
    int Offset = 0
);

public record GetTeamScheduleInput(
    DateTime Date,
    Guid? BarberId = null
);

public record CreateBarbershopServiceInput(
    string Title,
    string Description,
    decimal Price,
    int DurationMinutes
);

public record UpdateBarbershopServiceInput(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    int DurationMinutes
);

// Outputs
public record BarberOutput(
    Guid Id,
    string Name,
    string Phone,
    string PhoneFormatted,
    List<BarbershopServiceOutput> Services,
    bool IsActive,
    DateTime CreatedAt
);

public record BarbershopServiceOutput(
    Guid Id,
    string Title,
    string Description,
    decimal Price,
    int DurationMinutes,
    bool IsActive
);

public record BarberListOutput(
    List<BarberOutput> Items,
    int TotalCount,
    int Limit,
    int Offset
);

public record ScheduleOutput(
    DateTime Date,
    List<BarberScheduleOutput> Barbers
);

public record BarberScheduleOutput(
    Guid BarberId,
    string BarberName,
    List<AppointmentOutput> Appointments
);

public record AppointmentOutput(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid ServiceId,
    string ServiceTitle,
    DateTime StartTime,
    DateTime EndTime,
    string Status // Pending, Confirmed, Completed, Cancelled
);
```

**Schema do Banco de Dados**

```sql
-- Barbeiros (multi-tenant isolado)
CREATE TABLE barbers (
    barber_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    phone TEXT NOT NULL,
    service_ids UUID[] NOT NULL DEFAULT '{}', -- Array de UUIDs dos serviços
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    CONSTRAINT uq_barbers_barbearia_phone UNIQUE(barbearia_id, phone)
);

-- Serviços da Barbearia
CREATE TABLE barbershop_services (
    barbershop_service_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    title TEXT NOT NULL,
    description TEXT NOT NULL DEFAULT '',
    price NUMERIC(10, 2) NOT NULL CHECK (price >= 0),
    duration_minutes INT NOT NULL CHECK (duration_minutes > 0 AND duration_minutes <= 480),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Índices para performance
CREATE INDEX idx_barbers_barbearia_id ON barbers(barbearia_id);
CREATE INDEX idx_barbers_phone ON barbers(phone);
CREATE INDEX idx_barbers_is_active ON barbers(is_active);
CREATE INDEX idx_barbers_name ON barbers(name); -- Para busca por nome
CREATE INDEX idx_barbershop_services_barbearia_id ON barbershop_services(barbearia_id);
CREATE INDEX idx_barbershop_services_is_active ON barbershop_services(is_active);
```

### Endpoints de API

**Gestão de Barbeiros**

```
POST /api/barbers
Authorization: Bearer {token com role AdminBarbearia}
Body: {
    "name": "João Silva",
    "phone": "(11) 98765-4321",
    "serviceIds": ["uuid1", "uuid2"]
}
Response 201: {
    "id": "guid",
    "name": "João Silva",
    "phone": "11987654321",
    "phoneFormatted": "(11) 98765-4321",
    "services": [...],
    "isActive": true,
    "createdAt": "2025-10-11T10:00:00Z"
}
Response 400: Validation errors (invalid phone, duplicate, etc.)
Response 401: Unauthorized
Response 403: Forbidden (not Admin Barbearia)
Response 422: Business logic error (duplicate phone in this barbearia)
```

```
PUT /api/barbers/{id}
Authorization: Bearer {token com role AdminBarbearia}
Body: {
    "name": "João Silva",
    "serviceIds": ["uuid1", "uuid2"]
}
Response 200: BarberOutput
Response 400: Validation errors
Response 404: Barber not found
```

```
PUT /api/barbers/{id}/phone
Authorization: Bearer {token com role AdminBarbearia}
Body: { "phone": "(11) 98765-4321" }
Response 200: BarberOutput
Response 400: Invalid phone
Response 404: Barber not found
Response 422: Phone already in use in this barbearia
```

```
DELETE /api/barbers/{id}
Authorization: Bearer {token com role AdminBarbearia}
Response 204: Barber deactivated (soft delete)
Response 404: Barber not found
Response 409: Barber has future appointments (bloqueado)
```

```
GET /api/barbers
Authorization: Bearer {token com role AdminBarbearia}
Query: ?isActive=true&searchName=João&limit=50&offset=0
Response 200: {
    "items": [BarberOutput, ...],
    "totalCount": 100,
    "limit": 50,
    "offset": 0
}
```

```
GET /api/barbers/{id}
Authorization: Bearer {token com role AdminBarbearia}
Response 200: BarberOutput
Response 404: Barber not found
```

**Visualização de Agenda Consolidada**

```
GET /api/barbers/schedule
Authorization: Bearer {token com role AdminBarbearia}
Query: ?date=2025-10-11&barberId=guid (optional)
Response 200: {
    "date": "2025-10-11",
    "barbers": [
        {
            "barberId": "guid",
            "barberName": "João Silva",
            "appointments": [
                {
                    "id": "guid",
                    "customerId": "guid",
                    "customerName": "Cliente 1",
                    "customerPhone": "11999999999",
                    "serviceId": "guid",
                    "serviceTitle": "Corte + Barba",
                    "startTime": "2025-10-11T10:00:00Z",
                    "endTime": "2025-10-11T11:00:00Z",
                    "status": "Confirmed"
                },
                ...
            ]
        },
        ...
    ]
}
```

**Gestão de Serviços**

```
POST /api/barbershop-services
Authorization: Bearer {token com role AdminBarbearia}
Body: {
    "title": "Corte + Barba",
    "description": "Corte de cabelo masculino com acabamento de barba",
    "price": 50.00,
    "durationMinutes": 60
}
Response 201: BarbershopServiceOutput
Response 400: Validation errors
```

```
PUT /api/barbershop-services/{id}
Authorization: Bearer {token com role AdminBarbearia}
Body: { "title": "...", "description": "...", "price": 50.00, "durationMinutes": 60 }
Response 200: BarbershopServiceOutput
Response 404: Service not found
```

```
GET /api/barbershop-services
Authorization: Bearer {token com role AdminBarbearia}
Response 200: [BarbershopServiceOutput, ...]
```

```
DELETE /api/barbershop-services/{id}
Authorization: Bearer {token com role AdminBarbearia}
Response 204: Service deactivated
Response 404: Service not found
```

## Pontos de Integração

### Global Query Filters (Infrastructure)

```csharp
// BarbAppDbContext.OnModelCreating
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Isolamento multi-tenant automático
    modelBuilder.Entity<Barber>().HasQueryFilter(b =>
        _tenantContext.IsAdminCentral || b.BarbeariaId == _tenantContext.BarbeariaId);

    modelBuilder.Entity<BarbershopService>().HasQueryFilter(s =>
        _tenantContext.IsAdminCentral || s.BarbeariaId == _tenantContext.BarbeariaId);

    // Entity Type Configurations
    modelBuilder.ApplyConfiguration(new BarberConfiguration());
    modelBuilder.ApplyConfiguration(new BarbershopServiceConfiguration());
}
```

### Entity Type Configuration

```csharp
public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.ToTable("barbers");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).HasColumnName("barber_id");

        builder.Property(b => b.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(b => b.Phone)
            .HasColumnName("phone")
            .HasMaxLength(11)
            .IsRequired();

        builder.Property(b => b.ServiceIds)
            .HasColumnName("service_ids")
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(b => b.Barbearia)
            .WithMany()
            .HasForeignKey(b => b.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(b => new { b.BarbeariaId, b.Phone })
            .IsUnique()
            .HasDatabaseName("uq_barbers_barbearia_phone");
    }
}
```

### Validação com FluentValidation

```csharp
public class CreateBarberInputValidator : AbstractValidator<CreateBarberInput>
{
    public CreateBarberInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Must(BeValidPhone).WithMessage("Invalid phone format");

        RuleFor(x => x.ServiceIds)
            .NotNull().WithMessage("ServiceIds cannot be null");
    }

    private bool BeValidPhone(string phone)
    {
        var normalized = PhoneNumber.Normalize(phone);
        return PhoneNumber.IsValid(normalized);
    }
}
```

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|----------------------------|----------------|
| `BarbAppDbContext` | Extensão | Adicionar tabelas `barbers` e `barbershop_services` com Global Query Filters. **Risco médio** - pode afetar queries existentes. | Testar isolamento multi-tenant com testes de integração |
| `Barbershop` (Domain) | Extensão | Relacionamentos com `Barber` e `BarbershopService`. **Baixo risco** - apenas navegação. | Adicionar propriedades de navegação |
| `Appointment` (Domain) | Extensão | Adicionar FK `barber_id` para vincular agendamento ao barbeiro. **Médio risco** - PRD ainda não implementado. | Coordenar com PRD de Agendamentos |
| `IUnitOfWork` | Dependência | Use cases chamarão `Commit()` após operações de escrita. **Baixo risco** - padrão já estabelecido. | Seguir padrão existente |
| Frontend (React) | Dependência | Precisará implementar telas de gestão de barbeiros e agenda consolidada. **Médio risco** - nova funcionalidade. | Implementar componentes React + polling (30s) |
| `AuthController` | Dependência | Validação de autenticação JWT já implementada. **Baixo risco** - reutilizar. | Nenhuma ação necessária |

## Abordagem de Testes

### Testes Unitários

**Domain Layer** (`BarbApp.Domain.Tests`)

```csharp
public class BarberTests
{
    [Fact]
    public void Create_ValidData_ShouldSucceed()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var name = "João Silva";
        var phone = "11987654321";
        var serviceIds = new List<Guid> { Guid.NewGuid() };

        // Act
        var barber = Barber.Create(barbeariaId, name, phone, serviceIds);

        // Assert
        barber.Should().NotBeNull();
        barber.Id.Should().NotBeEmpty();
        barber.BarbeariaId.Should().Be(barbeariaId);
        barber.Name.Should().Be(name);
        barber.Phone.Should().Be(phone);
        barber.ServiceIds.Should().BeEquivalentTo(serviceIds);
        barber.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData("")] // Empty name
    [InlineData("   ")] // Whitespace only
    public void Create_InvalidName_ShouldThrowException(string invalidName)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var serviceIds = new List<Guid>();

        // Act & Assert
        Action act = () => Barber.Create(barbeariaId, invalidName, "11987654321", serviceIds);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Name cannot be empty*");
    }

    [Theory]
    [InlineData("123")] // Too short
    [InlineData("abc")] // Non-numeric
    [InlineData("123456789012")] // Too long
    public void Create_InvalidPhone_ShouldThrowException(string invalidPhone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var serviceIds = new List<Guid>();

        // Act & Assert
        Action act = () => Barber.Create(barbeariaId, "João Silva", invalidPhone, serviceIds);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Invalid phone number*");
    }

    [Fact]
    public void Update_ValidData_ShouldUpdateFields()
    {
        // Arrange
        var barber = Barber.Create(Guid.NewGuid(), "João Silva", "11987654321", new List<Guid>());
        var newName = "João Silva Santos";
        var newServiceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };

        // Act
        barber.Update(newName, newServiceIds);

        // Assert
        barber.Name.Should().Be(newName);
        barber.ServiceIds.Should().BeEquivalentTo(newServiceIds);
        barber.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var barber = Barber.Create(Guid.NewGuid(), "João Silva", "11987654321", new List<Guid>());

        // Act
        barber.Deactivate();

        // Assert
        barber.IsActive.Should().BeFalse();
    }
}

public class PhoneNumberTests
{
    [Theory]
    [InlineData("(11) 98765-4321", "11987654321")]
    [InlineData("11 98765-4321", "11987654321")]
    [InlineData("11987654321", "11987654321")]
    [InlineData("+55 11 98765-4321", "5511987654321")]
    public void Normalize_VariousFormats_ShouldReturnOnlyDigits(string input, string expected)
    {
        // Act
        var result = PhoneNumber.Normalize(input);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("11987654321")] // 11 digits
    [InlineData("1198765432")] // 10 digits
    public void IsValid_ValidPhone_ShouldReturnTrue(string phone)
    {
        // Act
        var result = PhoneNumber.IsValid(phone);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")] // Empty
    [InlineData("123")] // Too short
    [InlineData("123456789012")] // Too long
    public void IsValid_InvalidPhone_ShouldReturnFalse(string phone)
    {
        // Act
        var result = PhoneNumber.IsValid(phone);

        // Assert
        result.Should().BeFalse();
    }
}
```

**Application Layer** (`BarbApp.Application.Tests`)

```csharp
public class CreateBarberUseCaseTests
{
    private readonly Mock<IBarberRepository> _barberRepoMock;
    private readonly Mock<IBarbershopServiceRepository> _serviceRepoMock;
    private readonly Mock<ITenantContext> _tenantContextMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBarberUseCase _useCase;

    public CreateBarberUseCaseTests()
    {
        _barberRepoMock = new Mock<IBarberRepository>();
        _serviceRepoMock = new Mock<IBarbershopServiceRepository>();
        _tenantContextMock = new Mock<ITenantContext>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        var barbeariaId = Guid.NewGuid();
        _tenantContextMock.Setup(x => x.BarbeariaId).Returns(barbeariaId);

        _useCase = new CreateBarberUseCase(
            _barberRepoMock.Object,
            _serviceRepoMock.Object,
            _tenantContextMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidInput_ShouldCreateBarber()
    {
        // Arrange
        var input = new CreateBarberInput(
            Name: "João Silva",
            Phone: "(11) 98765-4321",
            ServiceIds: new List<Guid> { Guid.NewGuid() }
        );

        _barberRepoMock
            .Setup(x => x.GetByPhoneAsync(
                It.IsAny<Guid>(),
                "11987654321",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barber?)null); // Telefone não existe

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("João Silva");
        result.Phone.Should().Be("11987654321");
        result.IsActive.Should().BeTrue();

        _barberRepoMock.Verify(
            x => x.InsertAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Execute_DuplicatePhone_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarberInput(
            Name: "João Silva",
            Phone: "11987654321",
            ServiceIds: new List<Guid>()
        );

        var existingBarber = Barber.Create(
            _tenantContextMock.Object.BarbeariaId!.Value,
            "Outro Barbeiro",
            "11987654321",
            new List<Guid>()
        );

        _barberRepoMock
            .Setup(x => x.GetByPhoneAsync(
                It.IsAny<Guid>(),
                "11987654321",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBarber);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateBarberException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None)
        );

        _barberRepoMock.Verify(
            x => x.InsertAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}

public class RemoveBarberUseCaseTests
{
    private readonly Mock<IBarberRepository> _barberRepoMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly RemoveBarberUseCase _useCase;

    public RemoveBarberUseCaseTests()
    {
        _barberRepoMock = new Mock<IBarberRepository>();
        _appointmentRepoMock = new Mock<IAppointmentRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _useCase = new RemoveBarberUseCase(
            _barberRepoMock.Object,
            _appointmentRepoMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Execute_BarberWithFutureAppointments_ShouldThrowException()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var barber = Barber.Create(Guid.NewGuid(), "João Silva", "11987654321", new List<Guid>());

        _barberRepoMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _appointmentRepoMock
            .Setup(x => x.CountFutureAppointmentsByBarberAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // Tem agendamentos futuros

        // Act & Assert
        await Assert.ThrowsAsync<BarberHasFutureAppointmentsException>(
            () => _useCase.ExecuteAsync(barberId, CancellationToken.None)
        );

        _barberRepoMock.Verify(
            x => x.UpdateAsync(It.IsAny<Barber>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Execute_BarberWithoutFutureAppointments_ShouldDeactivate()
    {
        // Arrange
        var barberId = Guid.NewGuid();
        var barber = Barber.Create(Guid.NewGuid(), "João Silva", "11987654321", new List<Guid>());

        _barberRepoMock
            .Setup(x => x.GetByIdAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barber);

        _appointmentRepoMock
            .Setup(x => x.CountFutureAppointmentsByBarberAsync(barberId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0); // Sem agendamentos futuros

        // Act
        await _useCase.ExecuteAsync(barberId, CancellationToken.None);

        // Assert
        barber.IsActive.Should().BeFalse();
        _barberRepoMock.Verify(
            x => x.UpdateAsync(barber, It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(
            x => x.Commit(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

### Testes de Integração

**Configuração com TestContainers** (`BarbApp.IntegrationTests`)

```csharp
public class BarberIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public BarberIntegrationTests()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("barbapp_test")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    services.AddDbContext<BarbAppDbContext>(options =>
                    {
                        options.UseNpgsql(_dbContainer.GetConnectionString());
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
                    context.Database.Migrate();
                });
            });

        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    [Fact]
    public async Task CreateBarber_ValidData_ShouldReturn201()
    {
        // Arrange - Autenticar como Admin Barbearia
        var (barbeariaId, token) = await AuthenticateAsAdminBarbearia();

        var request = new CreateBarberInput(
            Name: "João Silva",
            Phone: "(11) 98765-4321",
            ServiceIds: new List<Guid>()
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<BarberOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("João Silva");
        result.Phone.Should().Be("11987654321");
    }

    [Fact]
    public async Task CreateBarber_DuplicatePhone_ShouldReturn422()
    {
        // Arrange
        var (barbeariaId, token) = await AuthenticateAsAdminBarbearia();

        // Criar barbeiro primeiro
        var barber1 = new CreateBarberInput("João Silva", "11987654321", new List<Guid>());
        await _client.PostAsJsonAsync("/api/barbers", barber1);

        // Tentar criar outro com mesmo telefone
        var barber2 = new CreateBarberInput("Maria Santos", "11987654321", new List<Guid>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/barbers", barber2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task MultiTenant_IsolationTest_BarbersShouldBeIsolatedByBarbearia()
    {
        // Arrange - Criar 2 barbearias com barbeiros
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var barbearia1 = Barbershop.Create("Barbearia 1", ...);
        var barbearia2 = Barbershop.Create("Barbearia 2", ...);
        var barbeiro1 = Barber.Create(barbearia1.Id, "Barbeiro 1", "11111111111", new List<Guid>());
        var barbeiro2 = Barber.Create(barbearia2.Id, "Barbeiro 2", "22222222222", new List<Guid>());

        context.Barbershops.AddRange(barbearia1, barbearia2);
        context.Barbers.AddRange(barbeiro1, barbeiro2);
        await context.SaveChangesAsync();

        // Autenticar como Admin da Barbearia 1
        var token1 = await AuthenticateAsAdminBarbearia(barbearia1.Code.Value);

        // Act - Listar barbeiros
        var response = await _client.GetAsync("/api/barbers");

        // Assert - Deve ver apenas barbeiros da barbearia1
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BarberListOutput>();
        result!.Items.Should().HaveCount(1);
        result.Items[0].Name.Should().Be("Barbeiro 1");
    }

    [Fact]
    public async Task GetTeamSchedule_ShouldReturnConsolidatedSchedule()
    {
        // Arrange - Criar barbearia, barbeiros e agendamentos
        var (barbeariaId, token) = await AuthenticateAsAdminBarbearia();

        // Criar barbeiros
        var barber1 = await CreateBarber("João Silva", "11111111111");
        var barber2 = await CreateBarber("Maria Santos", "22222222222");

        // Criar agendamentos para hoje (implementação depende do PRD de Agendamentos)

        // Act
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var response = await _client.GetAsync($"/api/barbers/schedule?date={today}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ScheduleOutput>();
        result.Should().NotBeNull();
        result!.Barbers.Should().HaveCount(2);
    }

    private async Task<(Guid barbeariaId, string token)> AuthenticateAsAdminBarbearia(string? code = null)
    {
        // Implementação de autenticação para testes
        // Retorna barbeariaId e JWT token
        throw new NotImplementedException("Implement auth helper");
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _client?.Dispose();
        await _factory.DisposeAsync();
    }
}
```

## Sequenciamento de Desenvolvimento

### Ordem de Construção

**Fase 1: Domain Layer (4h)**
1. Implementar Value Object `PhoneNumber` com normalização e validação
2. Implementar entidade `Barber` com regras de negócio
3. Implementar entidade `BarbershopService` com validações
4. Implementar exceções customizadas (`BarberNotFoundException`, `DuplicateBarberException`, `BarberHasFutureAppointmentsException`)
5. Implementar interfaces de repositório (`IBarberRepository`, `IBarbershopServiceRepository`)
6. Testes unitários de domínio (Value Objects + Entidades)

**Fase 2: Infrastructure - Banco de Dados (5h)**
7. Criar migrations para tabelas `barbers` e `barbershop_services`
8. Implementar Entity Type Configurations (mapeamento EF Core)
9. Estender `BarbAppDbContext` com novas entidades e Global Query Filters
10. Implementar repositórios (`BarberRepository`, `BarbershopServiceRepository`)
11. Testar migrations localmente

**Fase 3: Application Layer - Use Cases (8h)**
12. Implementar DTOs (Inputs/Outputs)
13. Implementar validators com FluentValidation
14. Implementar `CreateBarberUseCase` com validação de telefone único
15. Implementar `UpdateBarberUseCase` e `UpdateBarberPhoneUseCase`
16. Implementar `RemoveBarberUseCase` com validação de agendamentos futuros
17. Implementar `ListBarbersUseCase` com filtros e paginação
18. Implementar `GetBarberByIdUseCase`
19. Implementar `GetTeamScheduleUseCase` (depende de `IAppointmentRepository`)
20. Implementar use cases de serviços (`CreateBarbershopServiceUseCase`, `ListBarbershopServicesUseCase`)
21. Testes unitários de use cases com mocks

**Fase 4: API Layer (6h)**
22. Implementar `BarbersController` com todos os endpoints
23. Implementar `BarbershopServicesController`
24. Configurar autorização `[Authorize(Roles = "AdminBarbearia")]`
25. Implementar exception handling (middleware global)
26. Documentar endpoints com Swagger/OpenAPI
27. Testar manualmente via Swagger UI

**Fase 5: Testes de Integração (5h)**
28. Configurar TestContainers para PostgreSQL
29. Implementar testes de CRUD completo de barbeiros
30. Implementar testes de isolamento multi-tenant (crítico!)
31. Implementar testes de validação de telefone duplicado
32. Implementar testes de remoção com agendamentos futuros
33. Implementar testes de agenda consolidada

**Fase 6: Frontend Integration Preparation (2h)**
34. Documentar contratos de API (request/response examples)
35. Criar mock data para testes de frontend
36. Validar CORS configuration
37. Criar guia de integração para equipe de frontend

**Total Estimado**: ~30 horas

### Dependências Técnicas

**Bloqueantes**:
- ✅ Tabela `barbershops` já implementada (PRD Sistema Multi-tenant)
- ✅ Autenticação JWT e `ITenantContext` já implementados
- ⚠️ Tabela `appointments` (PRD Sistema de Agendamentos) - necessária para `GetTeamScheduleUseCase` e validação de remoção

**Desejáveis**:
- PostgreSQL rodando localmente ou via Docker
- Entity Framework Core CLI instalado (`dotnet tool install --global dotnet-ef`)

**Ordem de Implementação de PRDs**:
1. ✅ PRD Sistema Multi-tenant (concluído)
2. ✅ PRD Gestão de Barbearias Admin Central (concluído)
3. **→ PRD Gestão de Barbeiros** (este documento)
4. PRD Sistema de Agendamentos (necessário para agenda consolidada completa)

## Monitoramento e Observabilidade

### Logging Estruturado

**Pontos de Log** (seguindo `rules/logging.md`):

```csharp
// CreateBarberUseCase
_logger.LogInformation(
    "Creating barber {BarberName} for barbearia {BarbeariaId}",
    input.Name,
    _tenantContext.BarbeariaId);

_logger.LogWarning(
    "Duplicate phone {Phone} detected for barbearia {BarbeariaId}",
    "****" + input.Phone.Substring(input.Phone.Length - 4), // Mascarar telefone
    _tenantContext.BarbeariaId);

// RemoveBarberUseCase
_logger.LogWarning(
    "Cannot remove barber {BarberId} - has {Count} future appointments",
    barberId,
    futureAppointmentsCount);

_logger.LogInformation(
    "Barber {BarberId} deactivated for barbearia {BarbeariaId}",
    barberId,
    _tenantContext.BarbeariaId);

// GetTeamScheduleUseCase
_logger.LogInformation(
    "Fetching team schedule for barbearia {BarbeariaId} on {Date}",
    _tenantContext.BarbeariaId,
    input.Date);

// BarberRepository
_logger.LogDebug(
    "Listing barbers for barbearia {BarbeariaId} with filters: IsActive={IsActive}, SearchName={SearchName}",
    barbeariaId,
    isActive,
    searchName ?? "none");
```

**Níveis de Log**:
- `Information`: Operações bem-sucedidas (criação, atualização, listagem)
- `Warning`: Validações falhadas (telefone duplicado, barbeiro com agendamentos)
- `Error`: Exceções não previstas, falhas de banco de dados
- `Debug`: Queries detalhadas, filtros aplicados

**Conformidade**:
- ✅ Nunca logar telefone completo (mascarar: `****4321`)
- ✅ Usar logging estruturado com templates
- ✅ Injetar `ILogger<T>` via DI
- ✅ Logar exceções com stack trace: `_logger.LogError(ex, "Message")`

### Métricas (Prometheus)

```csharp
// Métricas customizadas
private static readonly Counter BarberCreations = Metrics.CreateCounter(
    "barbapp_barbers_created_total",
    "Total number of barbers created",
    new CounterConfiguration { LabelNames = new[] { "barbearia_id" } }
);

private static readonly Counter BarberRemovals = Metrics.CreateCounter(
    "barbapp_barbers_removed_total",
    "Total number of barbers removed",
    new CounterConfiguration { LabelNames = new[] { "barbearia_id", "reason" } }
);

private static readonly Gauge ActiveBarbers = Metrics.CreateGauge(
    "barbapp_active_barbers",
    "Number of active barbers per barbearia",
    new GaugeConfiguration { LabelNames = new[] { "barbearia_id" } }
);

private static readonly Histogram ScheduleLoadTime = Metrics.CreateHistogram(
    "barbapp_schedule_load_seconds",
    "Time to load team schedule",
    new HistogramConfiguration { LabelNames = new[] { "barber_count" } }
);
```

**Dashboard Grafana**:
- Número de barbeiros ativos por barbearia
- Taxa de criação/remoção de barbeiros ao longo do tempo
- Tempo médio de carregamento da agenda consolidada
- Tentativas de remoção bloqueadas (barbeiros com agendamentos futuros)

### Alertas

- **Warning**: Agenda consolidada demorando > 3s para carregar
- **Info**: Barbeiro removido com sucesso
- **Warning**: Tentativa de remover barbeiro com agendamentos futuros

## Considerações Técnicas

### Decisões Principais

**1. Autenticação e Primeiro Acesso do Barbeiro**
- **Escolha**: Login sem senha (passwordless) para o MVP.
- **Justificativa**: Alinhado com a especificação `prd-sistema-multi-tenant/techspec.md`, o MVP prioriza a simplicidade. O barbeiro fará login usando apenas o telefone e o código da barbearia.
- **Fluxo**:
  1. O Admin da Barbearia cadastra o barbeiro com nome e telefone.
  2. O Admin informa ao barbeiro o código da barbearia e a URL de acesso.
  3. O barbeiro acessa a URL, insere o código da barbearia e seu telefone para logar.
- **Evolução Futura**: A Fase 2 incluirá um fluxo de "primeiro acesso" onde o barbeiro poderá definir uma senha ou validar seu acesso via SMS, aumentando a segurança.

**2. Isolamento Multi-tenant: Dados Independentes por Barbearia**
- **Escolha**: Mesmo telefone pode existir em múltiplas barbearias com dados completamente independentes
- **Justificativa**:
  - Flexibilidade total para barbeiros trabalharem em múltiplos estabelecimentos
  - Dados isolados: nome, especialidades, serviços podem variar por barbearia
  - Constraint `UNIQUE(barbearia_id, phone)` garante unicidade dentro da barbearia
- **Trade-off**: Duplicação de dados (mesmo barbeiro = múltiplos registros)
- **Alternativa Rejeitada**: Tabela `BarbeiroBarbearia` com dados compartilhados (complexidade desnecessária, viola isolamento)

**3. Gestão de Serviços: Texto Livre com Estrutura**
- **Escolha**: Admin da Barbearia cadastra serviços com título, descrição, valor e duração (texto livre)
- **Justificativa**:
  - Flexibilidade para cada barbearia definir seus serviços
  - Não força catálogo pré-definido (cada barbearia é única)
  - Simples para MVP, evolui para catálogo compartilhado na Fase 2
- **Trade-off**: Sem padronização (mesmos serviços escritos de forma diferente)
- **Evolução Futura**: Catálogo sugerido com IA ou serviços mais populares

**4. Remoção de Barbeiros: Bloqueio com Agendamentos Futuros**
- **Escolha**: Bloquear remoção (Opção C) se houver agendamentos futuros
- **Justificativa**:
  - Evita cancelamento automático massivo (impacto negativo em clientes)
  - Força Admin a resolver agendamentos manualmente (decisão consciente)
  - Mantém integridade de dados e compromissos com clientes
- **Trade-off**: Admin precisa cancelar ou reagendar manualmente
- **Mitigação**: Modal de confirmação mostra número de agendamentos afetados

**5. Atualização de Agenda: Polling a cada 30s**
- **Escolha**: Frontend faz polling (requisição GET a cada 30s)
- **Justificativa**:
  - Suficiente para MVP (agenda não precisa ser real-time exato)
  - Simplicidade de implementação (sem WebSockets/SignalR)
  - Reduz carga no servidor vs polling muito frequente
- **Trade-off**: Delay de até 30s para novos agendamentos aparecerem
- **Evolução Futura**: Migrar para SignalR (push real-time) na Fase 2

**6. Soft Delete de Barbeiros**
- **Escolha**: Desativar barbeiro (flag `is_active = false`) ao invés de deletar
- **Justificativa**:
  - Mantém histórico de agendamentos passados (integridade referencial)
  - Permite reativação futura se barbeiro voltar
  - Não quebra relatórios e analytics
- **Trade-off**: Tabela cresce com registros inativos
- **Mitigação**: Filtro `is_active = true` nas listagens padrão

**7. Validação de Telefone: Normalização Automática**
- **Escolha**: Armazenar apenas números no banco, formatar no frontend
- **Justificativa**:
  - Facilita buscas e comparações (sem formatação variada)
  - Validação simples: 10 ou 11 dígitos
  - Frontend formata para exibição: `(11) 98765-4321`
- **Trade-off**: Frontend precisa implementar formatação
- **Mitigação**: DTO `BarberOutput` inclui `phoneFormatted` pronto

**8. Paginação: Limit/Offset**
- **Escolha**: Paginação baseada em `limit` e `offset` (query string)
- **Justificativa**:
  - Simples de implementar e entender
  - Suficiente para MVP (listas pequenas de barbeiros)
  - Padrão REST comum
- **Trade-off**: Performance degrada em offsets altos (raro no MVP)
- **Evolução Futura**: Cursor-based pagination se necessário

### Riscos Conhecidos

**1. Agendamentos Órfãos após Remoção**
- **Risco**: Se barbeiro for desativado, agendamentos futuros ficam sem barbeiro ativo
- **Mitigação**: Bloqueio de remoção (Opção C) força resolução manual antes da desativação
- **Detecção**: Query: `SELECT * FROM appointments WHERE barber_id IN (SELECT barber_id FROM barbers WHERE is_active = false)`

**2. Polling Overhead**
- **Risco**: Muitos Admins com polling ativo simultaneamente podem sobrecarregar servidor
- **Mitigação MVP**: Polling de 30s é conservador (120 req/hora por usuário)
- **Mitigação Futura**: Implementar cache Redis (TTL 10s) para agenda consolidada

**3. Serviços Duplicados**
- **Risco**: Admin pode criar serviços duplicados (ex: "Corte de Cabelo" e "corte cabelo")
- **Mitigação MVP**: Interface UI sugere serviços ao digitar (autocomplete)
- **Fase 2**: IA para detectar duplicatas e sugerir merge

**4. Telefone Inválido no Futuro**
- **Risco**: Barbeiro muda número e telefone antigo fica no sistema
- **Mitigação**: Permitir edição de telefone via `PUT /api/barbers/{id}/phone`
- **Validação**: Verificar se novo telefone não está em uso na mesma barbearia

**5. Global Query Filter Bypass**
- **Risco**: Desenvolvedor esquece de testar isolamento multi-tenant
- **Mitigação**: Testes de integração obrigatórios para isolamento (ver seção de Testes)

### Requisitos Especiais

**Performance**:
- Listagem de barbeiros: < 1s (requisito PRD)
- Agenda consolidada: < 3s (requisito PRD)
- Filtros na agenda: < 500ms (requisito PRD)
- Paginação com `limit=50`: < 500ms

**Segurança**:
- Todos os endpoints protegidos: `[Authorize(Roles = "AdminBarbearia")]`
- Global Query Filters garantem isolamento multi-tenant automático
- Telefones mascarados em logs: `****4321`
- Validação de telefone único por barbearia (previne duplicatas)

**LGPD**:
- Telefones armazenados sem formatação (apenas números)
- Logs mascarados para telefones
- Exclusão de barbearia cascadeia para barbeiros (ON DELETE CASCADE)
- Soft delete mantém histórico mas permite "esquecimento" via hard delete futuro

**Escalabilidade**:
- Índices em colunas de busca: `phone`, `name`, `is_active`, `barbearia_id`
- Paginação obrigatória em listagens (max 100 items por página)
- Global Query Filters compilados (overhead mínimo)

### Conformidade com Padrões

**✅ Clean Architecture** (`rules/code-standard.md`):
- Separação clara: Domain → Application → Infrastructure → API
- Inversão de dependências (interfaces no Domain)
- Entidades de domínio isoladas e ricas

**✅ Código** (`rules/code-standard.md`):
- camelCase: métodos, variáveis
- PascalCase: classes, interfaces
- kebab-case: arquivos, diretórios
- Métodos < 50 linhas
- Classes < 300 linhas
- Early returns (sem aninhamento > 2 níveis)
- Constantes para magic numbers

**✅ HTTP/REST** (`rules/http.md`):
- Endpoints RESTful: `/api/barbers`, `/api/barbershop-services`
- Métodos apropriados: POST (criar), PUT (atualizar), DELETE (remover), GET (listar/buscar)
- Status codes: 200, 201, 204, 400, 401, 403, 404, 422
- JSON payload
- OpenAPI documentation

**✅ SQL** (`rules/sql.md`):
- Tabelas em plural: `barbers`, `barbershop_services`
- snake_case: tabelas e colunas
- PKs: `barber_id`, `barbershop_service_id`
- FKs: `barbearia_id`
- Constraints: `UNIQUE(barbearia_id, phone)`
- Índices em colunas de busca
- `created_at`, `updated_at` em todas as tabelas

**✅ Testes** (`rules/tests.md`):
- xUnit framework
- Moq para mocks
- AAA pattern (Arrange, Act, Assert)
- FluentAssertions
- TestContainers para integração
- Projetos separados: `Domain.Tests`, `Application.Tests`, `IntegrationTests`

**✅ Logging** (`rules/logging.md`):
- ILogger<T> injetado via DI
- Logging estruturado com templates
- Níveis apropriados (Info, Warning, Error, Debug)
- Nunca logar dados sensíveis (telefones mascarados)
- Exceções logadas com stack trace

**✅ Unit of Work** (`rules/unit-of-work.md`):
- Interface `IUnitOfWork` com `Commit()` e `Rollback()`
- Use cases chamam `UnitOfWork.Commit()` após operações de escrita
- Transações coordenadas

---

**Data de Criação**: 2025-10-11
**Versão**: 1.0
**Status**: Pronto para Implementação
