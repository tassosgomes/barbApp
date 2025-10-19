# Especificação Técnica - Gestão de Barbeiros (Admin da Barbearia)

## Resumo Executivo

O módulo de Gestão de Barbeiros permite que Admins de Barbearia gerenciem sua equipe de profissionais e monitorem agendamentos de forma centralizada. A solução implementa CRUD completo de barbeiros com isolamento multi-tenant rigoroso, onde cada barbeiro é independente por barbearia (mesmo email pode existir em múltiplas barbearias com dados completamente isolados). A arquitetura segue Clean Architecture com Domain (entidades e regras), Application (use cases), Infrastructure (repositórios e DbContext) e API (controllers REST). O sistema inclui visualização consolidada de agenda com atualização via polling (30s), gestão de serviços por barbearia, e validações robustas. A remoção de um barbeiro (soft delete) aciona o cancelamento automático de todos os seus agendamentos futuros.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Domain Layer** (`BarbApp.Domain`)
- **Entidades**: `Barber` (barbeiro vinculado a barbearia), `BarbershopService` (serviços oferecidos pela barbearia)
- **Value Objects**: `PhoneNumber` (telefone de contato), `ServiceDuration` (duração do serviço)
- **Exceções**: `BarberNotFoundException`, `DuplicateBarberException` (por email)
- **Interfaces**: `IBarberRepository`, `IBarbershopServiceRepository`, `IAppointmentRepository`

**Application Layer** (`BarbApp.Application`)
- **Use Cases**:
  - `CreateBarberUseCase` (adicionar barbeiro com email/senha)
  - `UpdateBarberUseCase` (editar informações do barbeiro)
  - `RemoveBarberUseCase` (desativar barbeiro e cancelar agendamentos futuros)
  - `ListBarbersUseCase` (listar equipe com filtros)
  - `GetBarberByIdUseCase` (detalhes de barbeiro específico)
  - `GetTeamScheduleUseCase` (agenda consolidada de todos os barbeiros)
  - `CreateBarbershopServiceUseCase`, `ListBarbershopServicesUseCase`, etc.
- **DTOs**: Inputs/Outputs para todas as operações
- **Validators**: FluentValidation para validação de email, senha, nome, etc.

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
1. Admin preenche formulário (nome, email, senha, telefone, serviços)
2. Frontend envia `POST /api/barbers` com dados
3. Controller valida autorização (Admin da Barbearia)
4. `CreateBarberUseCase` valida dados e email único na barbearia, faz hash da senha
5. Cria entidade `Barber` com `barbeariaId` do contexto
6. Repository persiste no banco
7. `UnitOfWork.Commit()` confirma transação
8. Retorna `201 Created` com dados do barbeiro criado

**Fluxo de Remoção de Barbeiro**:
1. Admin clica em "Remover"
2. Frontend envia `DELETE /api/barbers/{id}`
3. `RemoveBarberUseCase` busca o barbeiro
4. Busca todos os agendamentos futuros do barbeiro via `IAppointmentRepository`
5. Atualiza o status desses agendamentos para "Cancelled"
6. Desativa o barbeiro (`IsActive = false`)
7. `UnitOfWork.Commit()` confirma todas as alterações atomicamente
8. Retorna `204 No Content`

## Design de Implementação

### Interfaces Principais

```csharp
// Domain - Repository Interfaces
public interface IBarberRepository
{
    Task<Barber?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Barber?> GetByEmailAsync(Guid barbeariaId, string email, CancellationToken cancellationToken);
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
    // ... (sem alterações)
}

public interface IAppointmentRepository
{
    Task<List<Appointment>> GetFutureAppointmentsByBarberAsync(Guid barberId, CancellationToken cancellationToken);
    Task UpdateStatusAsync(IEnumerable<Appointment> appointments, string newStatus, CancellationToken cancellationToken);
    // ... (outros métodos)
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
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Phone { get; private set; } // Contato, não login
    public List<Guid> ServiceIds { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Barber() { } // EF Core

    public static Barber Create(
        Guid barbeariaId,
        string name,
        string email,
        string passwordHash,
        string phone,
        List<Guid> serviceIds)
    {
        // Validações de nome, email, etc.
        // ...

        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Name = name.Trim(),
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            Phone = PhoneNumber.Normalize(phone),
            ServiceIds = serviceIds ?? new List<Guid>(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string phone, List<Guid> serviceIds)
    {
        // Validações
        // ...
        Name = name.Trim();
        Phone = PhoneNumber.Normalize(phone);
        ServiceIds = serviceIds ?? new List<Guid>();
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Outros métodos como UpdateEmail, ChangePassword, Deactivate, Activate...
    
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

**DTOs (Application)**

```csharp
// Inputs
public record CreateBarberInput(
    string Name,
    string Email,
    string Password,
    string Phone,
    List<Guid> ServiceIds
);

public record UpdateBarberInput(
    Guid Id,
    string Name,
    string Phone,
    List<Guid> ServiceIds
);

// ... outros DTOs permanecem similares

// Outputs
public record BarberOutput(
    Guid Id,
    string Name,
    string Email,
    string PhoneFormatted,
    List<BarbershopServiceOutput> Services,
    bool IsActive,
    DateTime CreatedAt
);

// ... outros DTOs de output
```

**Schema do Banco de Dados**

```sql
-- Barbeiros (multi-tenant isolado)
CREATE TABLE barbers (
    barber_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    email TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    phone TEXT NOT NULL,
    service_ids UUID[] NOT NULL DEFAULT '{}',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    CONSTRAINT uq_barbers_barbearia_email UNIQUE(barbearia_id, email)
);

-- Índices para performance
CREATE INDEX idx_barbers_barbearia_id ON barbers(barbearia_id);
CREATE INDEX idx_barbers_email ON barbers(email);
-- ... outros índices
```

### Endpoints de API

**Gestão de Barbeiros**

```
POST /api/barbers
Authorization: Bearer {token com role AdminBarbearia}
Body: {
    "name": "João Silva",
    "email": "joao.silva@email.com",
    "password": "Password123!",
    "phone": "(11) 98765-4321",
    "serviceIds": ["uuid1", "uuid2"]
}
Response 201: BarberOutput (sem passwordHash)
Response 422: Business logic error (duplicate email in this barbearia)
```

```
DELETE /api/barbers/{id}
Authorization: Bearer {token com role AdminBarbearia}
Response 204: Barber deactivated and future appointments cancelled.
Response 404: Barber not found.
```
(Outros endpoints de CRUD são ajustados de forma similar)

## Pontos de Integração

### Entity Type Configuration

```csharp
public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        // ... outras configurações
        builder.Property(b => b.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired();
            
        builder.HasIndex(b => new { b.BarbeariaId, b.Email })
            .IsUnique()
            .HasDatabaseName("uq_barbers_barbearia_email");
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
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
            
        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(8);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required")
            .Must(BeValidPhone).WithMessage("Invalid phone format");
        // ...
    }
    // ...
}
```

## Considerações Técnicas

### Decisões Principais

**1. Autenticação e Primeiro Acesso do Barbeiro**
- **Escolha**: Login com `Email` e `Senha`.
- **Justificativa**: Mais seguro e padrão de mercado. Permite recuperação de senha futura. Separa o contato (telefone) da credencial de acesso (email).
- **Fluxo**:
  1. O Admin da Barbearia cadastra o barbeiro com nome, email, senha inicial, telefone e serviços.
  2. O Admin informa as credenciais ao barbeiro.
  3. O barbeiro acessa a URL de login, insere o código da barbearia, seu email e senha para logar.

**4. Remoção de Barbeiros: Cancelamento de Agendamentos Futuros**
- **Escolha**: Desativar o barbeiro (soft delete) e cancelar automaticamente todos os seus agendamentos futuros.
- **Justificativa**: Libera o Admin da Barbearia da tarefa manual de cancelamento, garante que a agenda reflita a realidade e evita que clientes compareçam a um atendimento que não ocorrerá.
- **Trade-off**: Ação de remoção se torna mais pesada (mais operações no banco).
- **Mitigação**: Executar todas as atualizações dentro de uma única transação do `UnitOfWork` para garantir atomicidade.

(Outras decisões como Soft Delete, Polling, etc., permanecem as mesmas)

### Riscos Conhecidos

**1. Agendamentos Órfãos após Remoção**
- **Risco**: Se a lógica de cancelamento falhar, agendamentos futuros podem ficar vinculados a um barbeiro inativo.
- **Mitigação**: A operação inteira (cancelar agendamentos + desativar barbeiro) deve ser atômica, envolvida por um `UnitOfWork.Commit()`. Testes de integração são essenciais para validar este cenário.

(Outros riscos permanecem os mesmos)

## Monitoramento e Observabilidade

### Logging Estruturado

**Bibliotecas**: Microsoft.Extensions.Logging + Serilog

**Padrões de Log Implementados:**
- **Criação de Barbeiro**: `LogInformation("Starting creation of new barber with email {Email} and phone {MaskedPhone}", input.Email, maskedPhone)`
- **Atualização de Barbeiro**: `LogInformation("Starting update of barber with ID {BarberId} and phone {MaskedPhone}", input.Id, maskedPhone)`
- **Remoção de Barbeiro**: `LogInformation("Starting removal of barber with ID {BarberId}", barberId)`
- **Consulta de Agenda**: `LogInformation("Getting team schedule for date {Date}, barberId: {BarberId}", date, barberId)`
- **Listagem de Barbeiros**: `LogInformation("Listing barbers with filters: isActive={IsActive}, searchName={SearchName}, page={Page}, pageSize={PageSize}", ...)`

**Mascaramento de Dados Sensíveis:**
- Telefones são mascarados nos logs: `MaskPhone()` converte "11987654321" para "11987****21"
- Emails não são mascarados (não considerados PII no contexto de barbearias)

### Métricas (Prometheus)

**Métricas Customizadas Implementadas:**

```csharp
// Counters
public static readonly Counter BarberCreatedCounter = Metrics
    .CreateCounter("barbapp_barber_created_total", "Total number of barbers created", new CounterConfiguration
    {
        LabelNames = new[] { "barbearia_id" }
    });

public static readonly Counter BarberRemovedCounter = Metrics
    .CreateCounter("barbapp_barber_removed_total", "Total number of barbers removed", new CounterConfiguration
    {
        LabelNames = new[] { "barbearia_id" }
    });

// Gauge for active barbers
public static readonly Gauge ActiveBarbersGauge = Metrics
    .CreateGauge("barbapp_active_barbers", "Number of active barbers", new GaugeConfiguration
    {
        LabelNames = new[] { "barbearia_id" }
    });

// Histogram for schedule retrieval time
public static readonly Histogram ScheduleRetrievalDuration = Metrics
    .CreateHistogram("barbapp_schedule_retrieval_duration_seconds", "Duration of schedule retrieval operations", new HistogramConfiguration
    {
        LabelNames = new[] { "barbearia_id" }
    });
```

**Pontos de Coleta:**
- `BarberCreatedCounter.Inc()`: Quando barbeiro é criado com sucesso
- `ActiveBarbersGauge.Inc()`: Quando barbeiro é criado
- `BarberRemovedCounter.Inc()`: Quando barbeiro é removido
- `ActiveBarbersGauge.Dec()`: Quando barbeiro é removido
- `ActiveBarbersGauge.Set(totalCount)`: Durante listagem de barbeiros ativos
- `ScheduleRetrievalDuration.Observe(stopwatch.Elapsed.TotalSeconds)`: Após consulta de agenda da equipe

### Dashboards Grafana

**Dashboard: Gestão de Barbeiros por Barbearia**
- **Gráfico de Linha**: Barbeiros criados/removidos por dia
- **Gauge**: Número atual de barbeiros ativos
- **Counter**: Total de operações de gestão (criação + remoção)
- **Tabela**: Top barbearias por número de barbeiros

**Dashboard: Performance da Agenda**
- **Histograma**: Tempo de consulta da agenda da equipe (p50, p95, p99)
- **Counter**: Total de consultas de agenda por barbearia
- **Gráfico de Área**: Consultas de agenda por hora do dia

**Dashboard: Isolamento Multi-tenant**
- **Alert**: Detecção de tentativas de acesso cross-tenant (deve ser 0)
- **Counter**: Operações de gestão por barbearia
- **Tabela**: Distribuição de barbeiros por barbearia

**Alertas Sugeridos:**
- 🚨 `barbapp_schedule_retrieval_duration_seconds p95 > 2.0` → Performance degradada na consulta de agenda
- 🚨 `Tentativa de acesso cross-tenant detectada` → Falha crítica de isolamento
- 🔶 `barbapp_active_barbers{barbearia_id=""} > 50` → Barbearia com muitos barbeiros (verificar se justificado)
- ℹ️ `barbapp_barber_created_total increase(1d) > 10` → Dia com muitas criações de barbeiro

### Logs de Auditoria

**Eventos Auditados:**
- Criação de barbeiro: ID, barbearia, email, timestamp
- Remoção de barbeiro: ID, barbearia, motivo (se aplicável), agendamentos cancelados
- Atualização de barbeiro: ID, campos alterados, timestamp
- Consultas de agenda: barbearia, filtros aplicados, resultado count

**Formato Estruturado:**
```json
{
  "timestamp": "2025-10-15T14:30:00Z",
  "level": "Information",
  "message": "Barber created successfully with ID {BarberId} in barbearia {BarbeariaId}",
  "barberId": "uuid-here",
  "barbeariaId": "uuid-here",
  "email": "barbeiro@email.com",
  "operation": "barber_created"
}
```
