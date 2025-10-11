# Especificação Técnica - Gestão de Barbearias (Admin Central)

## Resumo Executivo

Esta especificação define a implementação do módulo de Gestão de Barbearias, responsável pelo CRUD completo de estabelecimentos no sistema barbApp. A solução utiliza Entity Framework Core com PostgreSQL, implementando geração segura de códigos únicos alfanuméricos, validação de documentos (CNPJ/CPF), e hard delete com cascade para remoção de barbearias. A arquitetura segue Clean Architecture com separação clara entre domínio, aplicação e infraestrutura, garantindo testabilidade e manutenibilidade. O sistema é preparado para multi-tenancy mas Admin Central opera em contexto global, tendo acesso cross-tenant para gerenciar todas as barbearias.

## Arquitetura do Sistema

### Visão Geral dos Componentes

A implementação segue Clean Architecture com as seguintes camadas:

**Domain Layer** (`BarbApp.Domain`)
- **Entidades**: `Barbershop`, `Address`
- **Value Objects**: `Document` (CNPJ/CPF), `UniqueCode`
- **Interfaces de Repositório**: `IBarbershopRepository`, `IAddressRepository`
- **Exceções de Domínio**: `DuplicateDocumentException`, `InvalidDocumentException`, `BarbershopNotFoundException`

**Application Layer** (`BarbApp.Application`)
- **Use Cases**: `CreateBarbershop`, `UpdateBarbershop`, `DeleteBarbershop`, `GetBarbershop`, `ListBarbershops`
- **DTOs**: `CreateBarbershopInput`, `UpdateBarbershopInput`, `BarbershopOutput`, `PaginatedBarbershopsOutput`
- **Interfaces**: `IUniqueCodeGenerator`, `IUnitOfWork`
- **Validadores**: FluentValidation para inputs

**Infrastructure Layer** (`BarbApp.Infrastructure`)
- **DbContext**: `BarbAppDbContext` com EF Core
- **Repositórios**: Implementações concretas dos repositórios
- **Serviços**: `UniqueCodeGenerator` para geração de códigos
- **Migrations**: Migrations do EF Core para versionamento de schema

**API Layer** (`BarbApp.API`)
- **Controllers**: `BarbershopsController` com endpoints REST
- **Middlewares**: Autenticação JWT, Exception Handling
- **Filters**: Autorização para Admin Central apenas

### Fluxo de Dados

```
Cliente (React) 
    ↓ HTTP Request (JSON)
BarbershopsController
    ↓ Input Validation
Use Case (Application)
    ↓ Business Logic
Repository (Infrastructure)
    ↓ EF Core
PostgreSQL Database
```

## Design de Implementação

### Interfaces Principais

```csharp
// Domain - Repository Interface
public interface IBarbershopRepository
{
    Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Barbershop?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<Barbershop?> GetByDocumentAsync(string document, CancellationToken cancellationToken);
    Task<PaginatedResult<Barbershop>> ListAsync(
        int page, 
        int pageSize, 
        string? searchTerm, 
        bool? isActive, 
        string? sortBy, 
        CancellationToken cancellationToken);
    Task InsertAsync(Barbershop barbershop, CancellationToken cancellationToken);
    Task UpdateAsync(Barbershop barbershop, CancellationToken cancellationToken);
    Task DeleteAsync(Barbershop barbershop, CancellationToken cancellationToken);
}

// Application - Use Case Interface
public interface ICreateBarbershopUseCase
{
    Task<BarbershopOutput> ExecuteAsync(
        CreateBarbershopInput input, 
        CancellationToken cancellationToken);
}

// Application - Service Interface
public interface IUniqueCodeGenerator
{
    Task<string> GenerateAsync(CancellationToken cancellationToken);
}
```

### Modelos de Dados

**Entidade Barbershop (Domain)**

```csharp
public class Barbershop
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public Document Document { get; private set; } // Value Object
    public string Phone { get; private set; }
    public string OwnerName { get; private set; }
    public string Email { get; private set; }
    public UniqueCode Code { get; private set; } // Value Object
    public bool IsActive { get; private set; }
    public Guid AddressId { get; private set; }
    public Address Address { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public string CreatedBy { get; private set; }
    public string UpdatedBy { get; private set; }

    private Barbershop() { } // EF Core

    public static Barbershop Create(
        string name,
        Document document,
        string phone,
        string ownerName,
        string email,
        Address address,
        UniqueCode code,
        string createdBy)
    {
        // Validações de domínio
        var barbershop = new Barbershop
        {
            Id = Guid.NewGuid(),
            Name = name,
            Document = document,
            Phone = phone,
            OwnerName = ownerName,
            Email = email,
            Address = address,
            Code = code,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedBy = createdBy
        };
        return barbershop;
    }

    public void Update(
        string name,
        string phone,
        string ownerName,
        string email,
        Address address,
        string updatedBy)
    {
        Name = name;
        Phone = phone;
        OwnerName = ownerName;
        Email = email;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Entidade Address (Domain)**

```csharp
public class Address
{
    public Guid Id { get; private set; }
    public string ZipCode { get; private set; }
    public string Street { get; private set; }
    public string Number { get; private set; }
    public string? Complement { get; private set; }
    public string Neighborhood { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }

    private Address() { } // EF Core

    public static Address Create(
        string zipCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state)
    {
        return new Address
        {
            Id = Guid.NewGuid(),
            ZipCode = zipCode,
            Street = street,
            Number = number,
            Complement = complement,
            Neighborhood = neighborhood,
            City = city,
            State = state
        };
    }

    public void Update(
        string zipCode,
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state)
    {
        ZipCode = zipCode;
        Street = street;
        Number = number;
        Complement = complement;
        Neighborhood = neighborhood;
        City = city;
        State = state;
    }
}
```

**Value Objects (Domain)**

```csharp
public class Document
{
    public string Value { get; private set; }
    public DocumentType Type { get; private set; }

    private Document() { } // EF Core

    public static Document Create(string value)
    {
        var cleanValue = CleanDocument(value);
        
        if (IsCnpj(cleanValue))
            return new Document { Value = cleanValue, Type = DocumentType.CNPJ };
        
        if (IsCpf(cleanValue))
            return new Document { Value = cleanValue, Type = DocumentType.CPF };
        
        throw new InvalidDocumentException("Invalid CNPJ or CPF format");
    }

    private static string CleanDocument(string value)
    {
        return Regex.Replace(value, @"[^\d]", "");
    }

    private static bool IsCnpj(string value)
    {
        return value.Length == 14 && Regex.IsMatch(value, @"^\d{14}$");
    }

    private static bool IsCpf(string value)
    {
        return value.Length == 11 && Regex.IsMatch(value, @"^\d{11}$");
    }
}

public enum DocumentType
{
    CPF = 1,
    CNPJ = 2
}

public class UniqueCode
{
    public string Value { get; private set; }

    private UniqueCode() { } // EF Core

    public static UniqueCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Code cannot be empty");
        
        if (value.Length != 8)
            throw new ArgumentException("Code must be 8 characters");
        
        if (!Regex.IsMatch(value, @"^[A-Z2-9]{8}$"))
            throw new ArgumentException("Code must contain only uppercase letters and numbers (excluding O, I, 0, 1)");
        
        return new UniqueCode { Value = value.ToUpperInvariant() };
    }
}
```

**DTOs (Application)**

```csharp
public record CreateBarbershopInput(
    string Name,
    string Document,
    string Phone,
    string OwnerName,
    string Email,
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);

public record UpdateBarbershopInput(
    Guid Id,
    string Name,
    string Phone,
    string OwnerName,
    string Email,
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);

public record BarbershopOutput(
    Guid Id,
    string Name,
    string Document,
    string Phone,
    string OwnerName,
    string Email,
    string Code,
    bool IsActive,
    AddressOutput Address,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record AddressOutput(
    string ZipCode,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State
);

public record PaginatedBarbershopsOutput(
    List<BarbershopOutput> Items,
    int TotalCount,
    int Page,
    int PageSize
);
```

**Schema do Banco de Dados**

```sql
-- Tabela addresses
CREATE TABLE addresses (
    address_id UUID PRIMARY KEY,
    zip_code TEXT NOT NULL,
    street TEXT NOT NULL,
    number TEXT NOT NULL,
    complement TEXT,
    neighborhood TEXT NOT NULL,
    city TEXT NOT NULL,
    state TEXT NOT NULL
);

-- Tabela barbershops
CREATE TABLE barbershops (
    barbershop_id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    document TEXT NOT NULL,
    document_type INT NOT NULL,
    phone TEXT NOT NULL,
    owner_name TEXT NOT NULL,
    email TEXT NOT NULL,
    code TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    address_id UUID NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    created_by TEXT NOT NULL,
    updated_by TEXT NOT NULL,
    
    CONSTRAINT fk_barbershops_addresses 
        FOREIGN KEY (address_id) 
        REFERENCES addresses(address_id) 
        ON DELETE CASCADE,
    
    CONSTRAINT uk_barbershops_code UNIQUE (code),
    CONSTRAINT uk_barbershops_document UNIQUE (document)
);

-- Índices para performance
CREATE INDEX idx_barbershops_code ON barbershops(code);
CREATE INDEX idx_barbershops_document ON barbershops(document);
CREATE INDEX idx_barbershops_name ON barbershops(name);
CREATE INDEX idx_barbershops_is_active ON barbershops(is_active);
```

### Endpoints de API

**Base Path**: `/api/barbearias`

#### 1. Criar Barbearia
- **Método**: `POST /api/barbearias`
- **Autorização**: Admin Central apenas
- **Request Body**:
```json
{
  "name": "Barbearia do João",
  "document": "12.345.678/0001-90",
  "phone": "(11) 98765-4321",
  "ownerName": "João Silva",
  "email": "joao@barbearia.com",
  "zipCode": "01310-100",
  "street": "Av. Paulista",
  "number": "1000",
  "complement": "Sala 15",
  "neighborhood": "Bela Vista",
  "city": "São Paulo",
  "state": "SP"
}
```
- **Response**: `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Barbearia do João",
  "document": "12345678000190",
  "phone": "11987654321",
  "ownerName": "João Silva",
  "email": "joao@barbearia.com",
  "code": "XYZ123AB",
  "isActive": true,
  "address": {
    "zipCode": "01310100",
    "street": "Av. Paulista",
    "number": "1000",
    "complement": "Sala 15",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  },
  "createdAt": "2025-10-10T14:30:00Z",
  "updatedAt": "2025-10-10T14:30:00Z"
}
```

#### 2. Atualizar Barbearia
- **Método**: `PUT /api/barbearias/{id}`
- **Autorização**: Admin Central apenas
- **Request Body**: Similar ao POST (sem document)
- **Response**: `200 OK` (mesmo formato do POST)

#### 3. Buscar Barbearia por ID
- **Método**: `GET /api/barbearias/{id}`
- **Autorização**: Admin Central apenas
- **Response**: `200 OK` (formato BarbershopOutput)

#### 4. Listar Barbearias
- **Método**: `GET /api/barbearias`
- **Autorização**: Admin Central apenas
- **Query Parameters**:
  - `page` (int, default: 1)
  - `pageSize` (int, default: 20, max: 100)
  - `searchTerm` (string, opcional) - busca em name, code, document
  - `isActive` (boolean, opcional) - filtro por status
  - `sortBy` (string, opcional) - "name", "createdAt" (default: "name")
  - `sortOrder` (string, opcional) - "asc", "desc" (default: "asc")
- **Response**: `200 OK`
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20
}
```

#### 5. Excluir Barbearia
- **Método**: `DELETE /api/barbearias/{id}`
- **Autorização**: Admin Central apenas
- **Response**: `204 No Content`

**Códigos de Status de Erro**:
- `400 Bad Request`: Validação de input falhou
- `401 Unauthorized`: Token inválido ou ausente
- `403 Forbidden`: Usuário não é Admin Central
- `404 Not Found`: Barbearia não encontrada
- `422 Unprocessable Entity`: Documento duplicado, código duplicado
- `500 Internal Server Error`: Erro inesperado

## Pontos de Integração

### Autenticação JWT (PRD-5)

A funcionalidade depende do sistema de autenticação multi-tenant já implementado:

- **Token JWT**: Deve conter claim `role` com valor `AdminCentral`
- **Middleware**: Validação automática de token em todas as requisições
- **Autorização**: Atributo `[Authorize(Roles = "AdminCentral")]` nos endpoints
- **UserId**: Claim `sub` ou `userId` usado para `CreatedBy` e `UpdatedBy`

### Serviço de Geração de Código Único

**Implementação (Infrastructure Layer)**:

```csharp
public class UniqueCodeGenerator : IUniqueCodeGenerator
{
    private readonly IBarbershopRepository _repository;
    private readonly ILogger<UniqueCodeGenerator> _logger;
    private const string AllowedChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sem O, I, 0, 1
    private const int CodeLength = 8;
    private const int MaxRetries = 10;

    public async Task<string> GenerateAsync(CancellationToken cancellationToken)
    {
        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            var code = GenerateRandomCode();
            
            var existing = await _repository.GetByCodeAsync(code, cancellationToken);
            if (existing == null)
            {
                _logger.LogInformation("Generated unique code {Code} on attempt {Attempt}", code, attempt + 1);
                return code;
            }
            
            _logger.LogWarning("Code collision detected: {Code}. Retrying...", code);
        }
        
        throw new InvalidOperationException("Failed to generate unique code after maximum retries");
    }

    private string GenerateRandomCode()
    {
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[CodeLength];
        random.GetBytes(bytes);
        
        var chars = new char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            chars[i] = AllowedChars[bytes[i] % AllowedChars.Length];
        }
        
        return new string(chars);
    }
}
```

**Tratamento de Erros**:
- Colisão após 10 tentativas: `500 Internal Server Error`
- Log de todas as tentativas para análise

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|----------------------------|----------------|
| `BarbAppDbContext` | Criação | Novas tabelas `barbershops` e `addresses`. **Baixo risco** - estrutura isolada. | Criar DbContext, DbSets e Configuration |
| Sistema Multi-tenant (PRD-5) | Dependência | Requer JWT authentication já implementado. **Risco médio** - bloqueante. | Coordenar ordem de implementação |
| Tabelas relacionadas futuras | Cascata | Exclusão de barbershop afetará `barbers`, `appointments`, `customers`. **Alto risco** futuro. | Documentar ON DELETE CASCADE em todas as FKs |
| Migrations | Versionamento | Primeira migration do sistema. **Baixo risco** - base limpa. | Criar migration inicial |
| Logging/Monitoring | Observabilidade | Necessidade de métricas de geração de código. **Baixo risco** - adicional. | Configurar logs estruturados |

**Dependências Bloqueantes**:
1. **PRD-5 (Multi-tenant)**: Autenticação JWT e middleware de autorização devem estar implementados
2. **Infraestrutura**: PostgreSQL configurado e acessível
3. **Projetos Base**: Estrutura de projetos (.NET solution) deve estar criada

**Componentes que Dependerão desta Funcionalidade**:
- PRD-2 (Gestão de Barbeiros): FK `barbershop_id` na tabela `barbers`
- PRD-3 (Agendamentos Barbeiro): Contexto de barbearia para isolamento
- PRD-4 (Cadastro Cliente): FK `barbershop_id` na tabela `customers`

## Abordagem de Testes

### Testes Unitários

**Domain Layer** (`BarbApp.Domain.Tests`):

```csharp
public class BarbershopTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateBarbershop()
    {
        // Arrange
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var code = UniqueCode.Create("ABC12345");
        
        // Act
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            code,
            "admin-user-id"
        );
        
        // Assert
        barbershop.Should().NotBeNull();
        barbershop.Name.Should().Be("Barbearia Teste");
        barbershop.IsActive.Should().BeTrue();
        barbershop.CreatedBy.Should().Be("admin-user-id");
    }

    [Theory]
    [InlineData("12345678000190")] // CNPJ válido
    [InlineData("12345678901")] // CPF válido
    public void Document_ValidFormat_ShouldCreate(string documentValue)
    {
        // Act
        var document = Document.Create(documentValue);
        
        // Assert
        document.Should().NotBeNull();
        document.Value.Should().Be(documentValue);
    }

    [Theory]
    [InlineData("123")] // Muito curto
    [InlineData("12345678901234")] // Comprimento inválido
    [InlineData("abcd1234")] // Não numérico
    public void Document_InvalidFormat_ShouldThrowException(string documentValue)
    {
        // Act & Assert
        Action act = () => Document.Create(documentValue);
        act.Should().Throw<InvalidDocumentException>();
    }
}

public class UniqueCodeTests
{
    [Fact]
    public void Create_ValidCode_ShouldCreate()
    {
        // Act
        var code = UniqueCode.Create("ABC12345");
        
        // Assert
        code.Value.Should().Be("ABC12345");
    }

    [Theory]
    [InlineData("ABC")] // Muito curto
    [InlineData("ABC123456")] // Muito longo
    [InlineData("ABC1234O")] // Contém O
    [InlineData("ABC1234I")] // Contém I
    [InlineData("ABC12340")] // Contém 0
    [InlineData("ABC12341")] // Contém 1
    public void Create_InvalidCode_ShouldThrowException(string codeValue)
    {
        // Act & Assert
        Action act = () => UniqueCode.Create(codeValue);
        act.Should().Throw<ArgumentException>();
    }
}
```

**Application Layer** (`BarbApp.Application.Tests`):

```csharp
public class CreateBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _repositoryMock;
    private readonly Mock<IUniqueCodeGenerator> _codeGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateBarbershopUseCase _useCase;

    public CreateBarbershopUseCaseTests()
    {
        _repositoryMock = new Mock<IBarbershopRepository>();
        _codeGeneratorMock = new Mock<IUniqueCodeGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _useCase = new CreateBarbershopUseCase(
            _repositoryMock.Object,
            _codeGeneratorMock.Object,
            _unitOfWorkMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidInput_ShouldCreateBarbershop()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "joao@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP"
        );
        
        _codeGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ABC12345");
        
        _repositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);
        
        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);
        
        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("ABC12345");
        _repositoryMock.Verify(x => x.InsertAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Execute_DuplicateDocument_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarbershopInput(...);
        
        var existingBarbershop = Barbershop.Create(...);
        _repositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBarbershop);
        
        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDocumentException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None)
        );
    }
}
```

**Infrastructure - Geração de Código** (`BarbApp.Infrastructure.Tests`):

```csharp
public class UniqueCodeGeneratorTests
{
    [Fact]
    public async Task GenerateAsync_NoCollision_ShouldReturnCode()
    {
        // Arrange
        var repositoryMock = new Mock<IBarbershopRepository>();
        repositoryMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);
        
        var generator = new UniqueCodeGenerator(repositoryMock.Object, Mock.Of<ILogger<UniqueCodeGenerator>>());
        
        // Act
        var code = await generator.GenerateAsync(CancellationToken.None);
        
        // Assert
        code.Should().NotBeNullOrEmpty();
        code.Should().HaveLength(8);
        code.Should().MatchRegex("^[A-Z2-9]{8}$");
    }

    [Fact]
    public async Task GenerateAsync_CollisionThenSuccess_ShouldRetryAndReturn()
    {
        // Arrange
        var repositoryMock = new Mock<IBarbershopRepository>();
        var existingBarbershop = Barbershop.Create(...);
        
        var callCount = 0;
        repositoryMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => callCount++ < 2 ? existingBarbershop : null);
        
        var generator = new UniqueCodeGenerator(repositoryMock.Object, Mock.Of<ILogger<UniqueCodeGenerator>>());
        
        // Act
        var code = await generator.GenerateAsync(CancellationToken.None);
        
        // Assert
        code.Should().NotBeNullOrEmpty();
        repositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }
}
```

### Testes de Integração

**Configuração com TestContainers** (`BarbApp.IntegrationTests`):

```csharp
public class BarbershopsControllerTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public BarbershopsControllerTests()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithDatabase("barbapp_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
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
                    // Remove o DbContext registrado
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<BarbAppDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);
                    
                    // Adiciona DbContext com TestContainer
                    services.AddDbContext<BarbAppDbContext>(options =>
                    {
                        options.UseNpgsql(_dbContainer.GetConnectionString());
                    });
                });
                
                builder.ConfigureTestServices(services =>
                {
                    // Aplicar migrations
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
                    context.Database.Migrate();
                });
            });
        
        _client = _factory.CreateClient();
        
        // Adicionar token JWT de Admin Central
        var token = GenerateAdminCentralToken();
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    [Fact]
    public async Task CreateBarbershop_ValidData_ShouldReturn201()
    {
        // Arrange
        var input = new
        {
            name = "Barbearia Teste",
            document = "12345678000190",
            phone = "11987654321",
            ownerName = "João Silva",
            email = "joao@test.com",
            zipCode = "01310100",
            street = "Av. Paulista",
            number = "1000",
            complement = (string?)null,
            neighborhood = "Bela Vista",
            city = "São Paulo",
            state = "SP"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/barbearias", input);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<BarbershopOutput>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Barbearia Teste");
        result.Code.Should().NotBeNullOrEmpty();
        result.Code.Should().HaveLength(8);
    }

    [Fact]
    public async Task CreateBarbershop_DuplicateDocument_ShouldReturn422()
    {
        // Arrange - Criar primeira barbearia
        var input = new { ... };
        await _client.PostAsJsonAsync("/api/barbearias", input);
        
        // Act - Tentar criar com mesmo documento
        var response = await _client.PostAsJsonAsync("/api/barbearias", input);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task ListBarbershops_WithPagination_ShouldReturnPaginatedResults()
    {
        // Arrange - Criar 25 barbearias
        for (int i = 0; i < 25; i++)
        {
            var input = new { name = $"Barbearia {i}", ... };
            await _client.PostAsJsonAsync("/api/barbearias", input);
        }
        
        // Act
        var response = await _client.GetAsync("/api/barbearias?page=1&pageSize=10");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PaginatedBarbershopsOutput>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task DeleteBarbershop_ExistingId_ShouldReturn204AndCascadeDelete()
    {
        // Arrange - Criar barbearia
        var createResponse = await _client.PostAsJsonAsync("/api/barbearias", new { ... });
        var created = await createResponse.Content.ReadFromJsonAsync<BarbershopOutput>();
        
        // Act - Deletar
        var deleteResponse = await _client.DeleteAsync($"/api/barbearias/{created.Id}");
        
        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verificar que foi deletada
        var getResponse = await _client.GetAsync($"/api/barbearias/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.DisposeAsync();
        _client?.Dispose();
        _factory?.Dispose();
    }

    private string GenerateAdminCentralToken()
    {
        // Gerar JWT com role AdminCentral para testes
        // Implementação específica do projeto
    }
}
```

**Cenários de Teste Críticos**:

1. **Criação com sucesso**: Documento válido, geração de código único
2. **Duplicação de documento**: CNPJ/CPF já cadastrado
3. **Validação de formato**: Documento inválido, campos obrigatórios vazios
4. **Busca e filtros**: Busca por nome/código, filtro por status
5. **Paginação**: Diferentes tamanhos de página, ordenação
6. **Atualização**: Modificar dados (exceto documento e código)
7. **Exclusão**: Hard delete com cascade
8. **Autorização**: Rejeitar usuários não-Admin Central
9. **Colisão de código**: Retry logic do gerador

## Sequenciamento de Desenvolvimento

### Ordem de Construção

**Fase 1: Fundação (Domain + Infrastructure Base)**
1. **Setup de Projetos** (2h)
   - Criar solution e projetos (Domain, Application, Infrastructure, API, Tests)
   - Configurar dependências (EF Core, FluentValidation, xUnit, TestContainers)
   - Configurar PostgreSQL connection string

2. **Domain Layer** (4h)
   - Implementar Value Objects: `Document`, `UniqueCode`
   - Implementar Entidades: `Address`, `Barbershop`
   - Implementar Exceções de Domínio
   - Testes unitários de domínio

**Fase 2: Infraestrutura de Dados**
3. **Infrastructure - DbContext** (4h)
   - Criar `BarbAppDbContext`
   - Configurar Entity Type Configurations
   - Configurar relacionamentos e constraints
   - Criar migration inicial
   - Testar migration em PostgreSQL local

4. **Infrastructure - Repositórios** (3h)
   - Implementar `BarbershopRepository`
   - Implementar `AddressRepository`
   - Implementar queries com filtros e paginação
   - Configurar índices no EF Core

**Fase 3: Application Layer**
5. **Application - DTOs e Validadores** (2h)
   - Criar Input/Output DTOs
   - Implementar validadores FluentValidation
   - Testes de validação

6. **Application - Use Cases** (6h)
   - Implementar `CreateBarbershopUseCase`
   - Implementar `UpdateBarbershopUseCase`
   - Implementar `DeleteBarbershopUseCase`
   - Implementar `GetBarbershopUseCase`
   - Implementar `ListBarbershopsUseCase`
   - Testes unitários de use cases (com mocks)

7. **Infrastructure - Serviços** (3h)
   - Implementar `UniqueCodeGenerator`
   - Implementar `UnitOfWork`
   - Testes de geração de código

**Fase 4: API Layer**
8. **API - Controller** (4h)
   - Implementar `BarbershopsController`
   - Configurar routing e autorização
   - Implementar exception handling middleware
   - Configurar DI no `Program.cs`

9. **API - Documentação** (2h)
   - Configurar Swagger/OpenAPI
   - Documentar endpoints com XML comments
   - Testar manualmente via Swagger UI

**Fase 5: Testes de Integração**
10. **Testes de Integração** (6h)
    - Configurar TestContainers
    - Implementar cenários de teste end-to-end
    - Testar fluxos completos (CRUD)
    - Testar validações e casos de erro

**Fase 6: Refinamento**
11. **Logging e Observabilidade** (2h)
    - Adicionar logs estruturados em pontos críticos
    - Configurar métricas (tempo de geração de código, etc.)
    - Testar logs em diferentes cenários

12. **Revisão e Documentação** (2h)
    - Code review interno
    - Atualizar README com instruções
    - Documentar decisões técnicas

**Total Estimado**: ~40 horas de desenvolvimento

### Dependências Técnicas

**Bloqueantes (Devem Existir Antes)**:
- ✅ PostgreSQL instalado e configurado
- ✅ .NET 8 SDK instalado
- ⚠️ **PRD-5 (Multi-tenant)**: JWT authentication middleware e autorização
  - Claims structure definida
  - Middleware de validação de token
  - Role-based authorization configurado

**Não-Bloqueantes (Podem ser Implementadas Paralelamente)**:
- Logging infrastructure (Serilog, etc.)
- Monitoring dashboard (Grafana)
- CI/CD pipeline

**Entregas de Outras Features**:
- Nenhuma outra feature depende desta para iniciar desenvolvimento
- Esta feature é independente e pode ser desenvolvida isoladamente após PRD-5

## Monitoramento e Observabilidade

### Logging Estruturado

**Bibliotecas**: Microsoft.Extensions.Logging + Serilog (recomendado)

**Pontos de Log**:

```csharp
// UniqueCodeGenerator
_logger.LogInformation("Generating unique code for new barbershop");
_logger.LogWarning("Code collision detected: {Code}. Attempt {Attempt}/{MaxAttempts}", code, attempt, maxAttempts);
_logger.LogError("Failed to generate unique code after {MaxAttempts} attempts", maxAttempts);

// CreateBarbershopUseCase
_logger.LogInformation("Creating barbershop with document {Document}", input.Document);
_logger.LogWarning("Attempt to create barbershop with duplicate document {Document}", input.Document);
_logger.LogInformation("Barbershop created successfully with code {Code}", barbershop.Code);

// DeleteBarbershopUseCase
_logger.LogInformation("Deleting barbershop {BarbershopId}", id);
_logger.LogWarning("Barbershop {BarbershopId} not found for deletion", id);
_logger.LogInformation("Barbershop {BarbershopId} deleted successfully", id);
```

**Níveis de Log**:
- **Information**: Operações bem-sucedidas, início/fim de processos
- **Warning**: Colisões de código, tentativas de duplicação, barbearia não encontrada
- **Error**: Falhas inesperadas, exceções não tratadas
- **Critical**: Falha total do sistema (não esperado nesta feature)

**Formato de Log** (JSON estruturado):
```json
{
  "timestamp": "2025-10-10T14:30:00Z",
  "level": "Information",
  "message": "Barbershop created successfully with code {Code}",
  "properties": {
    "Code": "ABC12345",
    "UserId": "admin-user-123",
    "RequestId": "req-456"
  }
}
```

### Métricas

**Métricas a Expor** (formato Prometheus):

```csharp
// Contadores
- barbershops_created_total (Counter)
- barbershops_deleted_total (Counter)
- code_generation_collisions_total (Counter)
- code_generation_failures_total (Counter)

// Histogramas (duração)
- barbershop_creation_duration_seconds (Histogram)
- code_generation_duration_seconds (Histogram)
- barbershop_list_query_duration_seconds (Histogram)

// Gauges
- barbershops_active_total (Gauge)
- barbershops_inactive_total (Gauge)
```

**Implementação**:
```csharp
public class BarbershopMetrics
{
    private static readonly Counter CreatedCounter = Metrics.CreateCounter(
        "barbershops_created_total",
        "Total number of barbershops created"
    );

    private static readonly Histogram CreationDuration = Metrics.CreateHistogram(
        "barbershop_creation_duration_seconds",
        "Duration of barbershop creation in seconds"
    );

    public static void RecordCreation(TimeSpan duration)
    {
        CreatedCounter.Inc();
        CreationDuration.Observe(duration.TotalSeconds);
    }
}
```

### Dashboards Grafana

**Dashboard: Barbershop Management**

1. **Panel: Criações por Período**
   - Query: `rate(barbershops_created_total[5m])`
   - Tipo: Time Series

2. **Panel: Tempo Médio de Criação**
   - Query: `histogram_quantile(0.95, barbershop_creation_duration_seconds)`
   - Tipo: Gauge (95th percentile)

3. **Panel: Colisões de Código**
   - Query: `rate(code_generation_collisions_total[5m])`
   - Tipo: Time Series
   - Alert: > 10 colisões/min (indica problema)

4. **Panel: Status das Barbearias**
   - Query: `barbershops_active_total` vs `barbershops_inactive_total`
   - Tipo: Pie Chart

5. **Panel: Taxa de Erro**
   - Query: `rate(barbershop_creation_errors_total[5m])`
   - Tipo: Time Series
   - Alert: > 5 erros/min

## Considerações Técnicas

### Decisões Principais

**1. Entity Framework Core vs Dapper**
- **Escolha**: Entity Framework Core
- **Justificativa**:
  - Change tracking automático para Unit of Work
  - Migrations para versionamento de schema
  - Global Query Filters para multi-tenancy futura
  - Suporte a relacionamentos complexos
- **Trade-off**: Performance um pouco inferior, mas aceitável para MVP
- **Alternativa Rejeitada**: Dapper (mais performático mas requer código SQL manual)

**2. Hard Delete vs Soft Delete**
- **Escolha**: Soft Delete (marcar como inativo)
- **Justificativa**:
  - **Segurança**: Previne exclusão acidental e perda de dados irrecuperável.
  - **Reversibilidade**: Permite reativar uma barbearia facilmente.
  - **Integridade Histórica**: Mantém o registro da barbearia para relatórios e análises futuras, mesmo que ela não esteja mais ativa.
  - **Conformidade**: Alinha-se melhor com práticas de retenção de dados e auditoria.
- **Trade-off**: Requer que todas as queries de listagem filtrem por `IsActive = true` para não exibir barbearias inativas. Esse comportamento já é suportado pelo `Global Query Filter`.
- **Implementação**: A entidade `Barbershop` já possui a flag `IsActive` e os métodos `Activate()` e `Deactivate()`. A operação de exclusão (`DELETE /api/barbearias/{id}`) deve chamar o método `Deactivate()` em vez de remover o registro do banco de dados.

**3. Entidade Address Separada**
- **Escolha**: Tabela `addresses` separada com FK
- **Justificativa**:
  - Normalização de dados
  - Reutilização futura (se cliente tiver endereço)
  - Facilita validações e formatação
- **Trade-off**: Join adicional em queries
- **Alternativa Rejeitada**: JSON column ou campos embutidos na tabela barbershops

**4. Geração de Código com Retry**
- **Escolha**: 10 tentativas com geração criptograficamente segura
- **Justificativa**:
  - RandomNumberGenerator garante aleatoriedade forte
  - 10 tentativas é suficiente (probabilidade de colisão muito baixa)
  - 32^8 = 1,099,511,627,776 combinações possíveis
- **Trade-off**: Overhead de múltiplas queries em caso de colisão
- **Alternativa Rejeitada**: UUID (não user-friendly), Sequential (previsível)

**5. Global Query Filter para Multi-tenancy**
- **Escolha**: Implementar Global Query Filter no DbContext
- **Justificativa**:
  - Proteção automática contra vazamento de dados
  - DRY - não repetir filtro em cada query
  - Admin Central pode desabilitar filtro via `.IgnoreQueryFilters()`
- **Trade-off**: Performance overhead mínimo
- **Alternativa Rejeitada**: Filtro manual em cada repository (erro-propenso)

**Implementação**:
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global Query Filter para multi-tenancy
    modelBuilder.Entity<Barbershop>().HasQueryFilter(b => 
        !_tenantContext.IsAdminCentral || b.Id != Guid.Empty
    );
    
    // Admin Central ignora filtro nos seus repositórios
    // context.Barbershops.IgnoreQueryFilters().ToListAsync()
}
```

**6. Validação de CNPJ/CPF**
- **Escolha**: Validação de formato apenas (regex)
- **Justificativa**:
  - Decisão de negócio (MVP sem integração externa)
  - Evita dependência de API externa
  - Reduz latência e complexidade
- **Trade-off**: Aceita documentos com formato válido mas não existentes
- **Futura Evolução**: Integrar com ReceitaWS ou similar na Fase 2

### Riscos Conhecidos

**1. Colisão de Código Único**
- **Descrição**: Múltiplas instâncias gerando código simultaneamente podem colidir
- **Probabilidade**: Muito baixa (32^8 combinações)
- **Impacto**: Médio (atraso na criação, possível falha após 10 tentativas)
- **Mitigação**:
  - Retry logic com 10 tentativas
  - Constraint UNIQUE no banco (garantia final)
  - Monitoramento de colisões via métricas
  - Alert se colisões > 10/min

**2. Exclusão Acidental de Barbearia**
- **Descrição**: Admin Central pode excluir barbearia com dados importantes
- **Probabilidade**: Média (erro humano)
- **Impacto**: Alto (perda de dados de clientes, barbeiros, agendamentos)
- **Mitigação**:
  - Modal de confirmação no frontend (especificado no PRD)
  - Backups automáticos do banco
  - Sugestão: Adicionar período de "quarentena" na Fase 2

**3. Performance de Listagem com Muitas Barbearias**
- **Descrição**: Query de listagem pode ficar lenta com milhares de registros
- **Probabilidade**: Baixa no MVP, aumenta com crescimento
- **Impacto**: Médio (experiência do admin degradada)
- **Mitigação**:
  - Índices em campos de busca (name, code, document)
  - Paginação obrigatória (max 100 por página)
  - Filtros para reduzir resultado
  - Monitorar duração de queries

**4. Dependência de JWT Authentication (PRD-5)**
- **Descrição**: Funcionalidade não pode ser testada sem autenticação
- **Probabilidade**: Alta se PRD-5 atrasar
- **Impacto**: Alto (bloqueante)
- **Mitigação**:
  - Coordenar cronograma com time do PRD-5
  - Criar mock de authentication para desenvolvimento local
  - Testes de integração com token fake

**5. Migração de Schema em Produção**
- **Descrição**: Primeira migration pode falhar em produção
- **Probabilidade**: Baixa (schema simples)
- **Impacto**: Alto (sistema indisponível)
- **Mitigação**:
  - Testar migration em ambiente staging
  - Script de rollback preparado
  - Executar durante janela de manutenção

### Requisitos Especiais

**Performance**:
- Listagem com até 1.000 registros deve responder em < 2 segundos (requisito do PRD)
- Criação de barbearia deve responder em < 1 segundo (exceto em caso de múltiplas colisões)
- Índices obrigatórios: `code`, `document`, `name`, `is_active`

**Segurança**:
- Autorização obrigatória em todos os endpoints (Admin Central apenas)
- Validação de inputs para prevenir SQL Injection (EF Core parametriza automaticamente)
- Sanitização de outputs para prevenir XSS no frontend
- Documentos armazenados sem formatação (apenas números)
- Logs não devem conter documentos completos (LGPD)

**Escalabilidade**:
- Sistema deve suportar até 10.000 barbearias no MVP
- Geração de código deve escalar linearmente
- Paginação obrigatória para prevenir queries grandes

**Observabilidade**:
- Todas as operações CRUD devem ser logadas
- Métricas de performance devem ser expostas
- Colisões de código devem gerar warning logs
- Falhas de validação devem ser logadas (sem dados sensíveis)

### Conformidade com Padrões

**✅ Clean Architecture** (`rules/code-standard.md`):
- Separação clara entre Domain, Application, Infrastructure, API
- Inversão de dependências (interfaces no Domain/Application)
- Domínio isolado e independente de frameworks

**✅ Código** (`rules/code-standard.md`):
- camelCase para métodos/funções/variáveis
- PascalCase para classes/interfaces
- kebab-case para arquivos/diretórios
- Métodos < 50 linhas
- Classes < 300 linhas
- Early returns, sem aninhamento > 2 níveis
- Sem magic numbers (constantes nomeadas)

**✅ HTTP/REST** (`rules/http.md`):
- Endpoints RESTful: `/api/barbearias`
- Métodos HTTP apropriados (GET, POST, PUT, DELETE)
- Status codes corretos (200, 201, 204, 400, 401, 403, 404, 422, 500)
- JSON como formato de dados
- Autorização via JWT Bearer token
- Paginação via query params (page, pageSize)
- Documentação OpenAPI/Swagger

**✅ SQL/Banco** (`rules/sql.md`):
- Tabelas e colunas em snake_case
- Chaves primárias: `<table>_id` (barbershop_id, address_id)
- Uppercase para SQL keywords (SELECT, FROM, WHERE)
- Índices em colunas de busca
- Prepared statements (EF Core)
- `created_at`, `updated_at` em todas as tabelas
- Migrations para versionamento

**✅ Testes** (`rules/tests.md`):
- xUnit para estrutura de testes
- Moq/NSubstitute para mocks
- AAA pattern (Arrange, Act, Assert)
- FluentAssertions para asserções legíveis
- TestContainers para testes de integração
- Cobertura de: Domain (100%), Application (>90%), Integration (cenários críticos)

**✅ Logging** (`rules/logging.md`):
- ILogger<T> injetado via DI
- Logging estruturado com templates
- Níveis apropriados (Info, Warning, Error)
- Nunca logar dados sensíveis (PII)
- Logs de exceções com stack trace

**✅ Unit of Work** (`rules/unit-of-work.md`):
- Interface `IUnitOfWork` com `Commit` e `Rollback`
- Transações coordenadas
- SaveChanges no UnitOfWork, não nos repositórios
- Use cases chamam `UnitOfWork.Commit()` após operações

**✅ React** (`rules/react.md`):
- Componentes funcionais (TSX)
- React Query para API calls
- Tailwind para estilização
- Shadcn UI para componentes
- TypeScript strict mode

---

## Próximos Passos

1. **Aprovação da Tech Spec**: Revisão com stakeholders técnicos e de negócio
2. **Criação de Tasks**: Quebrar desenvolvimento nas fases definidas
3. **Setup de Ambiente**: Configurar repositório, CI/CD, banco de dados
4. **Coordenação com PRD-5**: Garantir que autenticação estará disponível
5. **Início do Desenvolvimento**: Seguir sequenciamento definido

---

**Questões em Aberto para Stakeholders**:

1. **Período de Retenção**: Se mudarmos para soft delete no futuro, qual o período de retenção de barbearias inativas?
2. **Notificação de Exclusão**: Admin Central deve receber confirmação por email ao excluir barbearia?
3. **Auditoria**: Logs de criação/edição/exclusão são suficientes ou precisamos de tabela de auditoria separada?
4. **Limite de Barbearias**: Há limite máximo de barbearias no plano do MVP? Isso afeta arquitetura?

---

**Documento Gerado**: 2025-10-10  
**Versão**: 1.0  
**Status**: Pronto para Revisão e Aprovação  
**Próxima Revisão**: Após feedback de stakeholders
