# Especificação Técnica - Sistema Multi-tenant e Autenticação

## Resumo Executivo

O sistema multi-tenant é a fundação arquitetural do barbApp que garante isolamento total de dados entre barbearias através de identificação por código único e validação de contexto em todas as requisições. A autenticação é implementada com JWT (HS256) via cookies HttpOnly, com diferentes fluxos para cada perfil de usuário (Admin Central, Admin Barbearia, Barbeiro, Cliente). O isolamento é garantido através de Global Query Filters no Entity Framework Core, eliminando a necessidade de filtros manuais por `barbeariaId` em cada query. A arquitetura é construída sobre Clean Architecture com separação clara entre Domain, Application, Infrastructure e API, permitindo evolução incremental do sistema.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Domain Layer** (`BarbApp.Domain`)
- **Value Objects**: `BarbeariaCode` (código único de 8 caracteres)
- **Interfaces**: `ITenantContext` (contexto da barbearia atual)
- **Exceções**: `InvalidBarbeariaCodeException`, `UnauthorizedAccessException`, `BarbeariaInactiveException`

**Application Layer** (`BarbApp.Application`)
- **Use Cases de Autenticação**:
  - `AuthenticateAdminCentralUseCase`
  - `AuthenticateAdminBarbeariaUseCase`
  - `AuthenticateBarbeiroUseCase`
  - `AuthenticateClienteUseCase`
  - `ListBarbeirosBarbeariaUseCase` (listar barbearias do barbeiro)
  - `TrocarContextoBarbeiroUseCase` (trocar contexto de barbearia)
- **Use Cases de Gestão**:
  - `CreateAdminBarbeariaUseCase` (Admin Central cria Admin Barbearia)
- **DTOs**: `AuthenticationInput`, `AuthenticationOutput`, `BarbeariaContextOutput`, `CreateAdminBarbeariaInput`
- **Interfaces**: `IAuthenticationService`, `IJwtTokenGenerator`, `IPasswordHasher`

**Infrastructure Layer** (`BarbApp.Infrastructure`)
- **Implementações**:
  - `JwtTokenGenerator` (geração e validação de tokens)
  - `PasswordHasher` (hash com BCrypt)
  - `TenantContext` (extração de contexto do JWT)
- **Middleware**: `TenantMiddleware` (extrai contexto de cada requisição)
- **DbContext**: `BarbAppDbContext` com Global Query Filters

**API Layer** (`BarbApp.API`)
- **Controllers**: `AuthController` (endpoints de autenticação)
- **Filters**: `[RequireRole]` (autorização por perfil)
- **Middleware**: Pipeline com autenticação JWT + tenant extraction

### Fluxo de Dados

```
Cliente (React) 
    ↓ HTTP Request (Cookie com JWT)
AuthController ou outro Controller
    ↓ JWT Middleware (valida token)
TenantMiddleware (extrai barbeariaId do JWT)
    ↓ ITenantContext disponível
Use Case (Application)
    ↓ Business Logic
Repository (Infrastructure)
    ↓ EF Core com Global Query Filter (filtra por barbeariaId)
PostgreSQL Database
```

**Fluxo de Autenticação**:
1. Cliente envia credenciais para `/api/auth/{perfil}`
2. Use Case valida credenciais no banco
3. Se válido, gera JWT com claims (userId, role, barbeariaId)
4. JWT é retornado em cookie HttpOnly
5. Requisições subsequentes incluem cookie automaticamente
6. Middleware valida JWT e popula `ITenantContext`

## Design de Implementação

### Interfaces Principais

```csharp
// Domain - Tenant Context
public interface ITenantContext
{
    Guid? BarbeariaId { get; }
    string? BarbeariaCode { get; }
    bool IsAdminCentral { get; }
    string UserId { get; }
    string Role { get; }
}

// Application - Authentication Service
public interface IAuthenticationService
{
    Task<AuthenticationOutput> AuthenticateAdminCentralAsync(
        string email, 
        string password, 
        CancellationToken cancellationToken);
    
    Task<AuthenticationOutput> AuthenticateAdminBarbeariaAsync(
        string codigo, 
        string email, 
        string password, 
        CancellationToken cancellationToken);
    
    Task<AuthenticationOutput> AuthenticateBarbeiroAsync(
        string codigo, 
        string telefone, 
        CancellationToken cancellationToken);
    
    Task<AuthenticationOutput> AuthenticateClienteAsync(
        string codigo, 
        string telefone, 
        string nome, 
        CancellationToken cancellationToken);
}

// Application - JWT Token Generator
public interface IJwtTokenGenerator
{
    string Generate(TokenClaims claims);
    TokenClaims? Validate(string token);
}

public record TokenClaims(
    string UserId,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode
);

// Application - Password Hasher
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

### Modelos de Dados

**Value Object - BarbeariaCode (Domain)**

```csharp
public class BarbeariaCode
{
    public string Value { get; private set; }

    private BarbeariaCode() { } // EF Core

    public static BarbeariaCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidBarbeariaCodeException("Code cannot be empty");
        
        var upperValue = value.ToUpperInvariant().Trim();
        
        if (upperValue.Length != 8)
            throw new InvalidBarbeariaCodeException("Code must be 8 characters");
        
        if (!Regex.IsMatch(upperValue, @"^[A-Z2-9]{8}$"))
            throw new InvalidBarbeariaCodeException(
                "Code must contain only uppercase letters and numbers (excluding O, I, 0, 1)");
        
        return new BarbeariaCode { Value = upperValue };
    }
}
```

**DTOs (Application)**

```csharp
// Inputs
public record AuthenticateAdminCentralInput(
    string Email,
    string Password
);

public record AuthenticateAdminBarbeariaInput(
    string Codigo,
    string Email,
    string Password
);

public record AuthenticateBarbeiroInput(
    string Codigo,
    string Telefone
);

public record AuthenticateClienteInput(
    string Codigo,
    string Telefone,
    string Nome
);

public record TrocarContextoInput(
    Guid BarbeariaId
);

// Outputs
public record AuthenticationOutput(
    string UserId,
    string Name,
    string Role,
    Guid? BarbeariaId,
    string? BarbeariaCode,
    string? BarbeariaNome
);

public record BarbeariaContextOutput(
    Guid BarbeariaId,
    string Code,
    string Name
);
```

**Entidades (Domain) - Usuários**

```csharp
// Admin Central (sem vínculo com barbearia)
public class AdminCentralUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdminCentralUser() { }

    public static AdminCentralUser Create(
        string email,
        string passwordHash,
        string name)
    {
        return new AdminCentralUser
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }
}

// Admin da Barbearia (vinculado a uma barbearia)
public class AdminBarbeariaUser
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdminBarbeariaUser() { }

    public static AdminBarbeariaUser Create(
        Guid barbeariaId,
        string email,
        string passwordHash,
        string name)
    {
        return new AdminBarbeariaUser
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Email = email.ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }
}

// Barbeiro (pode estar em múltiplas barbearias)
public class Barber
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; }
    public string Telefone { get; private set; } // Sem formatação, apenas números
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Barber() { }

    public static Barber Create(
        Guid barbeariaId,
        string telefone,
        string name)
    {
        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Telefone = CleanPhone(telefone),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string CleanPhone(string telefone)
    {
        var cleaned = Regex.Replace(telefone, @"[^\d]", "");
        
        // Validar formato brasileiro: 10 ou 11 dígitos (DDD + número)
        if (!Regex.IsMatch(cleaned, @"^\d{10,11}$"))
            throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
        
        return cleaned;
    }
}

// Cliente (pode estar em múltiplas barbearias)
public class Customer
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; }
    public string Telefone { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Customer() { }

    public static Customer Create(
        Guid barbeariaId,
        string telefone,
        string name)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Telefone = CleanPhone(telefone),
            Name = name,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string CleanPhone(string telefone)
    {
        var cleaned = Regex.Replace(telefone, @"[^\d]", "");
        
        // Validar formato brasileiro: 10 ou 11 dígitos (DDD + número)
        if (!Regex.IsMatch(cleaned, @"^\d{10,11}$"))
            throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
        
        return cleaned;
    }
}
```

**Schema do Banco de Dados**

```sql
-- Admin Central (sem barbearia)
CREATE TABLE admin_central_users (
    admin_central_user_id UUID PRIMARY KEY,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    name TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- Admin Barbearia (vinculado a uma barbearia)
CREATE TABLE admin_barbearia_users (
    admin_barbearia_user_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    email TEXT NOT NULL,
    password_hash TEXT NOT NULL,
    name TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    UNIQUE(barbearia_id, email) -- Admin único por barbearia/email
);

-- Barbeiros (multi-tenant)
CREATE TABLE barbers (
    barber_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    telefone TEXT NOT NULL,
    name TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    UNIQUE(barbearia_id, telefone) -- Barbeiro pode ter mesmo telefone em múltiplas barbearias
);

-- Clientes (multi-tenant)
CREATE TABLE customers (
    customer_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    telefone TEXT NOT NULL,
    name TEXT NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    UNIQUE(barbearia_id, telefone)
);

-- Índices para performance
CREATE INDEX idx_admin_barbearia_users_barbearia_id ON admin_barbearia_users(barbearia_id);
CREATE INDEX idx_admin_barbearia_users_email ON admin_barbearia_users(email);
CREATE INDEX idx_barbers_barbearia_id ON barbers(barbearia_id);
CREATE INDEX idx_barbers_telefone ON barbers(telefone);
CREATE INDEX idx_customers_barbearia_id ON customers(barbearia_id);
CREATE INDEX idx_customers_telefone ON customers(telefone);
```

### Endpoints de API

**Autenticação**

```
POST /api/auth/admin-central
Body: { "email": "string", "password": "string" }
Response 200: { "userId": "guid", "name": "string", "role": "AdminCentral" }
Response 400: Invalid credentials
Response 401: Unauthorized
```

```
POST /api/auth/admin-barbearia
Body: { "codigo": "string", "email": "string", "password": "string" }
Response 200: { "userId": "guid", "name": "string", "role": "AdminBarbearia", 
                "barbeariaId": "guid", "barbeariaCode": "string", "barbeariaNome": "string" }
Response 400: Invalid code or credentials
Response 401: Unauthorized
Response 404: Barbearia not found or inactive
```

```
POST /api/auth/barbeiro
Body: { "codigo": "string", "telefone": "string" }
Response 200: { "userId": "guid", "name": "string", "role": "Barbeiro", 
                "barbeariaId": "guid", "barbeariaCode": "string", "barbeariaNome": "string" }
Response 400: Invalid code or phone
Response 401: Unauthorized
Response 404: Barbearia or barber not found
```

```
POST /api/auth/cliente
Body: { "codigo": "string", "telefone": "string", "nome": "string" }
Response 200: { "userId": "guid", "name": "string", "role": "Cliente", 
                "barbeariaId": "guid", "barbeariaCode": "string", "barbeariaNome": "string" }
Response 201: Cliente criado e autenticado (primeiro acesso)
Response 400: Invalid code, phone or name mismatch
Response 404: Barbearia not found
```

```
POST /api/auth/logout
Response 204: Cookie removido
```

**Gestão de Admin Barbearia (Admin Central apenas)**

```
POST /api/admin-barbearia
Authorization: Bearer {token com role AdminCentral}
Body: { 
  "barbeariaId": "guid", 
  "email": "string", 
  "password": "string", 
  "name": "string" 
}
Response 201: { 
  "userId": "guid", 
  "name": "string", 
  "email": "string",
  "barbeariaId": "guid" 
}
Response 400: Invalid data or email already exists
Response 401: Unauthorized
Response 403: Forbidden (apenas Admin Central)
Response 404: Barbearia not found
```

**Troca de Contexto (Barbeiro)**

```
GET /api/barbeiro/barbearias
Authorization: Bearer {token com role Barbeiro}
Response 200: [
  { "barbeariaId": "guid", "code": "string", "name": "string" },
  ...
]
Response 401: Unauthorized
```

```
POST /api/barbeiro/trocar-contexto
Authorization: Bearer {token com role Barbeiro}
Body: { "barbeariaId": "guid" }
Response 200: { "userId": "guid", "name": "string", "role": "Barbeiro", 
                "barbeariaId": "guid", "barbeariaCode": "string", "barbeariaNome": "string" }
Response 400: Invalid barbeariaId
Response 403: Barbeiro não vinculado a essa barbearia
```

## Pontos de Integração

### JWT Token Generator (Infrastructure)

**Implementação**:

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtTokenGenerator> _logger;

    public string Generate(TokenClaims claims)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));
        
        var credentials = new SigningCredentials(
            securityKey, 
            SecurityAlgorithms.HmacSha256);

        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, claims.UserId),
            new(ClaimTypes.Role, claims.Role)
        };

        if (claims.BarbeariaId.HasValue)
        {
            claimsList.Add(new("barbeariaId", claims.BarbeariaId.Value.ToString()));
            claimsList.Add(new("barbeariaCode", claims.BarbeariaCode!));
        }

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claimsList,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public TokenClaims? Validate(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(
                token, 
                validationParameters, 
                out var validatedToken);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;
            var barbeariaIdStr = principal.FindFirst("barbeariaId")?.Value;
            var barbeariaCode = principal.FindFirst("barbeariaCode")?.Value;

            Guid? barbeariaId = null;
            if (!string.IsNullOrEmpty(barbeariaIdStr))
                barbeariaId = Guid.Parse(barbeariaIdStr);

            return new TokenClaims(userId!, role!, barbeariaId, barbeariaCode);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate JWT token");
            return null;
        }
    }
}
```

### Password Hasher (Infrastructure)

```csharp
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

### Tenant Middleware (Infrastructure)

```csharp
public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var user = context.User;
        
        if (user.Identity?.IsAuthenticated == true)
        {
            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            var barbeariaIdStr = user.FindFirst("barbeariaId")?.Value;
            var barbeariaCode = user.FindFirst("barbeariaCode")?.Value;

            Guid? barbeariaId = null;
            if (!string.IsNullOrEmpty(barbeariaIdStr))
                barbeariaId = Guid.Parse(barbeariaIdStr);

            ((TenantContext)tenantContext).SetContext(
                userId!, 
                role!, 
                barbeariaId, 
                barbeariaCode);
        }

        await _next(context);
    }
}

public class TenantContext : ITenantContext
{
    public Guid? BarbeariaId { get; private set; }
    public string? BarbeariaCode { get; private set; }
    public bool IsAdminCentral => string.IsNullOrEmpty(BarbeariaCode);
    public string UserId { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public void SetContext(string userId, string role, Guid? barbeariaId, string? barbeariaCode)
    {
        UserId = userId;
        Role = role;
        BarbeariaId = barbeariaId;
        BarbeariaCode = barbeariaCode;
    }
}
```

### Global Query Filters (Infrastructure)

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Global Query Filters para isolamento multi-tenant
    modelBuilder.Entity<Barber>().HasQueryFilter(b => 
        _tenantContext.IsAdminCentral || b.BarbeariaId == _tenantContext.BarbeariaId);
    
    modelBuilder.Entity<Customer>().HasQueryFilter(c => 
        _tenantContext.IsAdminCentral || c.BarbeariaId == _tenantContext.BarbeariaId);
    
    modelBuilder.Entity<AdminBarbeariaUser>().HasQueryFilter(a => 
        _tenantContext.IsAdminCentral || a.BarbeariaId == _tenantContext.BarbeariaId);

    // Configurações de entidades...
}
```

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|----------------------------|----------------|
| `BarbAppDbContext` | Extensão | Adicionar novas tabelas de usuários e Global Query Filters. **Risco médio** - pode afetar queries existentes. | Testar queries com `IgnoreQueryFilters()` para Admin Central |
| `Barbershop` (Domain) | Extensão | Relacionamentos com `AdminBarbeariaUser`, `Barber`, `Customer`. **Baixo risco** - apenas navegação. | Adicionar propriedades de navegação |
| Todos os Controllers futuros | Dependência | Todos precisarão validar autenticação JWT. **Alto impacto** - transversal. | Configurar `[Authorize]` globalmente |
| Todos os Repositories futuros | Dependência | Todos serão filtrados por `barbeariaId` automaticamente. **Alto risco** - pode causar bugs se não testado. | Testes de integração obrigatórios para isolamento |
| Frontend (React) | Dependência | Precisará gerenciar cookies e estado de autenticação. **Médio risco** - nova complexidade. | Implementar context de autenticação e interceptors |
| `Program.cs` (API) | Configuração | Adicionar middlewares de autenticação e tenant. **Baixo risco** - configuração padrão. | Configurar pipeline correto |

## Abordagem de Testes

### Testes Unitários

**Domain Layer** (`BarbApp.Domain.Tests`):

```csharp
public class BarbeariaCodeTests
{
    [Theory]
    [InlineData("ABC12345")]
    [InlineData("XYZ98765")]
    [InlineData("22334455")]
    public void Create_ValidCode_ShouldSucceed(string codeValue)
    {
        // Act
        var code = BarbeariaCode.Create(codeValue);

        // Assert
        code.Value.Should().Be(codeValue.ToUpperInvariant());
    }

    [Theory]
    [InlineData("ABC")] // Muito curto
    [InlineData("ABCDEFGHIJ")] // Muito longo
    [InlineData("ABC1234O")] // Contém O
    [InlineData("ABC1234I")] // Contém I
    [InlineData("ABC12340")] // Contém 0
    [InlineData("ABC12341")] // Contém 1
    [InlineData("")] // Vazio
    public void Create_InvalidCode_ShouldThrowException(string codeValue)
    {
        // Act & Assert
        Action act = () => BarbeariaCode.Create(codeValue);
        act.Should().Throw<InvalidBarbeariaCodeException>();
    }
}
```

**Application Layer** (`BarbApp.Application.Tests`):

```csharp
public class AuthenticateBarbeiroUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepoMock;
    private readonly Mock<IBarberRepository> _barberRepoMock;
    private readonly Mock<IJwtTokenGenerator> _jwtGeneratorMock;
    private readonly AuthenticateBarbeiroUseCase _useCase;

    public AuthenticateBarbeiroUseCaseTests()
    {
        _barbershopRepoMock = new Mock<IBarbershopRepository>();
        _barberRepoMock = new Mock<IBarberRepository>();
        _jwtGeneratorMock = new Mock<IJwtTokenGenerator>();
        _useCase = new AuthenticateBarbeiroUseCase(
            _barbershopRepoMock.Object,
            _barberRepoMock.Object,
            _jwtGeneratorMock.Object
        );
    }

    [Fact]
    public async Task Execute_ValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var codigo = "ABC12345";
        var telefone = "11987654321";
        var barbeariaId = Guid.NewGuid();
        
        var barbearia = Barbershop.Create("Barbearia Teste", ...);
        var barbeiro = Barber.Create(barbeariaId, telefone, "João Silva");

        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);
        
        _barberRepoMock
            .Setup(x => x.GetByTelefoneAsync(barbeariaId, telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbeiro);
        
        _jwtGeneratorMock
            .Setup(x => x.Generate(It.IsAny<TokenClaims>()))
            .Returns("mock-jwt-token");

        var input = new AuthenticateBarbeiroInput(codigo, telefone);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Role.Should().Be("Barbeiro");
        result.BarbeariaId.Should().Be(barbeariaId);
        _jwtGeneratorMock.Verify(
            x => x.Generate(It.Is<TokenClaims>(c => 
                c.UserId == barbeiro.Id.ToString() && 
                c.Role == "Barbeiro" &&
                c.BarbeariaId == barbeariaId
            )), 
            Times.Once
        );
    }

    [Fact]
    public async Task Execute_InvalidCode_ShouldThrowException()
    {
        // Arrange
        _barbershopRepoMock
            .Setup(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        var input = new AuthenticateBarbeiroInput("INVALID", "11987654321");

        // Act & Assert
        await Assert.ThrowsAsync<BarbershopNotFoundException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None)
        );
    }
}
```

**Infrastructure - JWT Generator** (`BarbApp.Infrastructure.Tests`):

```csharp
public class JwtTokenGeneratorTests
{
    private readonly IConfiguration _configuration;
    private readonly JwtTokenGenerator _generator;

    public JwtTokenGeneratorTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "my-super-secret-key-with-at-least-32-characters"},
            {"Jwt:Issuer", "barbapp"},
            {"Jwt:Audience", "barbapp-api"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _generator = new JwtTokenGenerator(_configuration, Mock.Of<ILogger<JwtTokenGenerator>>());
    }

    [Fact]
    public void Generate_ValidClaims_ShouldReturnToken()
    {
        // Arrange
        var claims = new TokenClaims(
            UserId: Guid.NewGuid().ToString(),
            Role: "Barbeiro",
            BarbeariaId: Guid.NewGuid(),
            BarbeariaCode: "ABC12345"
        );

        // Act
        var token = _generator.Generate(claims);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT tem 3 partes
    }

    [Fact]
    public void Validate_ValidToken_ShouldReturnClaims()
    {
        // Arrange
        var originalClaims = new TokenClaims(
            UserId: Guid.NewGuid().ToString(),
            Role: "AdminCentral",
            BarbeariaId: null,
            BarbeariaCode: null
        );
        var token = _generator.Generate(originalClaims);

        // Act
        var validatedClaims = _generator.Validate(token);

        // Assert
        validatedClaims.Should().NotBeNull();
        validatedClaims!.UserId.Should().Be(originalClaims.UserId);
        validatedClaims.Role.Should().Be(originalClaims.Role);
    }

    [Fact]
    public void Validate_InvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var result = _generator.Validate(invalidToken);

        // Assert
        result.Should().BeNull();
    }
}
```

### Testes de Integração

**Configuração com TestContainers** (`BarbApp.IntegrationTests`):

```csharp
public class AuthenticationIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer;
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    public AuthenticationIntegrationTests()
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
                    // Substituir DbContext para usar TestContainer
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
            HandleCookies = true // Importante para cookies
        });
    }

    [Fact]
    public async Task AuthenticateBarbeiro_ValidCredentials_ShouldReturnCookieAndData()
    {
        // Arrange - Criar barbearia e barbeiro
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        
        var barbearia = Barbershop.Create("Barbearia Teste", ...);
        var barbeiro = Barber.Create(barbearia.Id, "11987654321", "João Silva");
        
        context.Barbershops.Add(barbearia);
        context.Barbers.Add(barbeiro);
        await context.SaveChangesAsync();

        var request = new AuthenticateBarbeiroInput("ABC12345", "11987654321");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/barbeiro", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<AuthenticationOutput>();
        result.Should().NotBeNull();
        result!.Role.Should().Be("Barbeiro");
        result.BarbeariaId.Should().Be(barbearia.Id);
        
        // Verificar cookie
        response.Headers.Should().ContainKey("Set-Cookie");
        var cookie = response.Headers.GetValues("Set-Cookie").First();
        cookie.Should().Contain("barbapp-token");
        cookie.Should().Contain("HttpOnly");
        cookie.Should().Contain("Secure");
    }

    [Fact]
    public async Task MultiTenant_IsolationTest_BarberShouldOnlySeeOwnBarbeariaData()
    {
        // Arrange - Criar 2 barbearias com barbeiros
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        
        var barbearia1 = Barbershop.Create("Barbearia 1", ...);
        var barbearia2 = Barbershop.Create("Barbearia 2", ...);
        var barbeiro1 = Barber.Create(barbearia1.Id, "11111111111", "Barbeiro 1");
        var barbeiro2 = Barber.Create(barbearia2.Id, "22222222222", "Barbeiro 2");
        
        context.Barbershops.AddRange(barbearia1, barbearia2);
        context.Barbers.AddRange(barbeiro1, barbeiro2);
        await context.SaveChangesAsync();

        // Act - Autenticar como barbeiro1
        var authResponse = await _client.PostAsJsonAsync("/api/auth/barbeiro", 
            new AuthenticateBarbeiroInput(barbearia1.Code.Value, "11111111111"));
        authResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Tentar listar barbeiros (endpoint futuro que usa Global Query Filter)
        var listResponse = await _client.GetAsync("/api/barbeiros");

        // Assert - Deve ver apenas barbeiros da barbearia1
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var barbers = await listResponse.Content.ReadFromJsonAsync<List<BarberOutput>>();
        barbers.Should().HaveCount(1);
        barbers![0].Name.Should().Be("Barbeiro 1");
    }

    [Fact]
    public async Task TrocarContexto_ValidBarbeariaId_ShouldReturnNewToken()
    {
        // Arrange - Criar barbeiro em 2 barbearias
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        
        var telefone = "11987654321";
        var barbearia1 = Barbershop.Create("Barbearia 1", ...);
        var barbearia2 = Barbershop.Create("Barbearia 2", ...);
        var barbeiro1 = Barber.Create(barbearia1.Id, telefone, "João Silva");
        var barbeiro2 = Barber.Create(barbearia2.Id, telefone, "João Silva");
        
        context.AddRange(barbearia1, barbearia2, barbeiro1, barbeiro2);
        await context.SaveChangesAsync();

        // Autenticar inicialmente na barbearia1
        await _client.PostAsJsonAsync("/api/auth/barbeiro", 
            new AuthenticateBarbeiroInput(barbearia1.Code.Value, telefone));

        // Act - Trocar para barbearia2
        var response = await _client.PostAsJsonAsync("/api/barbeiro/trocar-contexto", 
            new TrocarContextoInput(barbearia2.Id));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<AuthenticationOutput>();
        result!.BarbeariaId.Should().Be(barbearia2.Id);
        result.BarbeariaCode.Should().Be(barbearia2.Code.Value);
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

**Fase 1: Fundação (Domain + Infrastructure Base)**
1. **Setup e Dependências** (2h)
   - Adicionar pacotes: `System.IdentityModel.Tokens.Jwt`, `BCrypt.Net-Next`
   - Configurar variáveis de ambiente (appsettings.json)

2. **Domain Layer** (3h)
   - Implementar Value Object: `BarbeariaCode`
   - Implementar Exceções: `InvalidBarbeariaCodeException`, etc.
   - Implementar interfaces: `ITenantContext`, `IAuthenticationService`, `IJwtTokenGenerator`, `IPasswordHasher`
   - Testes unitários de domínio

**Fase 2: Entidades e Infraestrutura de Dados**
3. **Domain - Entidades de Usuários** (4h)
   - Implementar entidades: `AdminCentralUser`, `AdminBarbeariaUser`, `Barber`, `Customer`
   - Adicionar relacionamentos com `Barbershop`
   - Testes unitários de entidades

4. **Infrastructure - DbContext** (5h)
   - Estender `BarbAppDbContext` com novas tabelas
   - Configurar Entity Type Configurations
   - Implementar Global Query Filters
   - Criar migrations
   - Testar migrations localmente

5. **Infrastructure - Repositórios** (3h)
   - Implementar `AdminCentralUserRepository`
   - Implementar `AdminBarbeariaUserRepository`
   - Implementar `BarberRepository`
   - Implementar `CustomerRepository`

**Fase 3: Application Layer**
6. **Application - DTOs** (2h)
   - Criar Input/Output DTOs de autenticação
   - Implementar validadores FluentValidation

7. **Application - Use Cases de Autenticação** (8h)
   - Implementar `AuthenticateAdminCentralUseCase`
   - Implementar `AuthenticateAdminBarbeariaUseCase`
   - Implementar `AuthenticateBarbeiroUseCase`
   - Implementar `AuthenticateClienteUseCase`
   - Implementar `ListBarbeirosBarbeariaUseCase`
   - Implementar `TrocarContextoBarbeiroUseCase`
   - Testes unitários de use cases (com mocks)

**Fase 4: Infrastructure - Serviços**
8. **Infrastructure - JWT e Segurança** (4h)
   - Implementar `JwtTokenGenerator`
   - Implementar `PasswordHasher`
   - Implementar `TenantContext`
   - Testes unitários de serviços

9. **Infrastructure - Middlewares** (3h)
   - Implementar `TenantMiddleware`
   - Configurar pipeline de autenticação
   - Testar extração de contexto

**Fase 5: API Layer**
10. **API - Controller de Autenticação** (4h)
    - Implementar `AuthController` com todos os endpoints
    - Configurar cookie HttpOnly
    - Implementar exception handling

11. **API - Configuração** (3h)
    - Configurar JWT no `Program.cs`
    - Configurar DI (todos os serviços)
    - Configurar middlewares no pipeline
    - Configurar autorização por roles

12. **API - Documentação** (2h)
    - Documentar endpoints com Swagger
    - Adicionar exemplos de requisição/resposta
    - Testar manualmente via Swagger UI

**Fase 6: Testes de Integração**
13. **Testes de Integração Completos** (6h)
    - Configurar TestContainers
    - Implementar testes de autenticação (todos os perfis)
    - Implementar testes de isolamento multi-tenant
    - Implementar testes de troca de contexto
    - Implementar testes de autorização

**Fase 7: Validação e Ajustes**
14. **Testes End-to-End e Ajustes** (4h)
    - Validar fluxos completos
    - Corrigir bugs encontrados
    - Refatorar código se necessário
    - Atualizar documentação

**Total Estimado**: ~53 horas

### Dependências Técnicas

- **Bloqueante**: Criação da entidade `Barbershop` e tabela `barbershops` (PRD Gestão de Barbearias)
- **Desejável**: PostgreSQL rodando localmente ou via Docker
- **Configuração**: Secret key JWT (gerada e armazenada em variáveis de ambiente)

## Monitoramento e Observabilidade

### Logging Estruturado

**Bibliotecas**: Microsoft.Extensions.Logging + Serilog

**Pontos de Log**:

```csharp
// AuthenticationService
_logger.LogInformation("Authenticating user with email {Email}", email);
_logger.LogWarning("Failed authentication attempt for email {Email}", email);
_logger.LogInformation("User {UserId} authenticated successfully as {Role}", userId, role);

// TenantMiddleware
_logger.LogDebug("Extracting tenant context from JWT for user {UserId}", userId);
_logger.LogWarning("Request without valid tenant context for endpoint {Endpoint}", endpoint);

// JwtTokenGenerator
_logger.LogInformation("Generating JWT token for user {UserId} with role {Role}", userId, role);
_logger.LogError(ex, "Failed to validate JWT token");

// Use Cases
_logger.LogInformation("Barbeiro {BarbeiroId} listing available barbearias");
_logger.LogInformation("Barbeiro {BarbeiroId} switching context to barbearia {BarbeariaId}", barbeiroId, barbeariaId);
_logger.LogWarning("Barbeiro {BarbeiroId} attempted to switch to unauthorized barbearia {BarbeariaId}", barbeiroId, barbeariaId);
```

**Níveis de Log**:
- `Information`: Operações bem-sucedidas de autenticação, troca de contexto
- `Warning`: Tentativas de autenticação falhadas, acesso não autorizado
- `Error`: Erros na geração/validação de JWT, exceções inesperadas
- `Debug`: Extração de contexto, validações intermediárias

### Métricas (Prometheus)

```csharp
// Métricas customizadas
private static readonly Counter AuthenticationAttempts = Metrics.CreateCounter(
    "barbapp_auth_attempts_total", 
    "Total authentication attempts",
    new CounterConfiguration { LabelNames = new[] { "role", "status" } }
);

private static readonly Histogram AuthenticationDuration = Metrics.CreateHistogram(
    "barbapp_auth_duration_seconds",
    "Authentication duration in seconds",
    new HistogramConfiguration { LabelNames = new[] { "role" } }
);

private static readonly Gauge ActiveSessions = Metrics.CreateGauge(
    "barbapp_active_sessions",
    "Number of active authenticated sessions",
    new GaugeConfiguration { LabelNames = new[] { "role" } }
);
```

**Dashboard Grafana**:
- Taxa de autenticação bem-sucedida vs falhada por perfil
- Tempo médio de autenticação
- Número de sessões ativas por perfil
- Tentativas de acesso não autorizado (403)

### Alertas

- **Crítico**: Taxa de falha de autenticação > 50% em 5 minutos
- **Warning**: Mais de 10 tentativas falhadas do mesmo IP em 1 minuto (possível brute force)
- **Info**: Barbeiro troca de contexto mais de 20 vezes por dia (comportamento anormal?)

## Considerações Técnicas

### Decisões Principais

**1. JWT HS256 vs RS256**
- **Escolha**: HS256 (symmetric)
- **Justificativa**:
  - Suficiente para MVP com backend monolítico
  - Secret compartilhado entre instâncias via variável de ambiente
  - Mais simples de implementar e debugar
- **Trade-off**: Não permite validação de token por terceiros sem compartilhar secret
- **Alternativa Rejeitada**: RS256 (complexidade desnecessária para MVP)

**2. Cookie HttpOnly vs LocalStorage**
- **Escolha**: Cookie HttpOnly com Secure flag
- **Justificativa**:
  - Proteção contra XSS (JavaScript não pode acessar)
  - Enviado automaticamente em requisições
  - SameSite=Strict previne CSRF
- **Trade-off**: Requer CORS configurado corretamente
- **Alternativa Rejeitada**: LocalStorage (vulnerável a XSS)

**3. Refresh Token**
- **Escolha**: Não implementar no MVP
- **Justificativa**:
  - Expiração de 24h é aceitável para MVP
  - Reduz complexidade (sem storage de refresh tokens)
  - Usuário pode fazer login novamente se necessário
- **Evolução Futura**: Implementar em Fase 2 com Redis

**4. Token Blacklist**
- **Escolha**: Não implementar no MVP
- **Justificativa**:
  - Logout remove token do cliente (suficiente para MVP)
  - Evita dependência de Redis
  - Token expira naturalmente em 24h
- **Trade-off**: Token permanece válido até expirar mesmo após logout
- **Mitigação**: Tokens de curta duração (24h)
- **Evolução Futura**: Blacklist em Redis para revogação imediata

**5. Tabelas de Usuários Separadas vs Tabela Unificada**
- **Escolha**: Tabelas separadas por perfil
- **Justificativa**:
  - Schemas diferentes (Admin tem senha, Cliente não tem no MVP)
  - Relacionamentos diferentes (Admin Barbearia vinculado a 1 barbearia, Barbeiro a múltiplas)
  - Queries mais simples e performáticas
- **Trade-off**: Mais tabelas para gerenciar
- **Alternativa Rejeitada**: Tabela `users` com discriminator (complexidade de relacionamentos)

**6. Global Query Filters vs Filtros Manuais**
- **Escolha**: Global Query Filters no EF Core
- **Justificativa**:
  - Garante isolamento automático (impossível esquecer filtro)
  - Código mais limpo (sem `Where(x => x.BarbeariaId == ...)` em todas as queries)
  - Segurança por padrão
- **Trade-off**: Pode ocultar bugs se não testado adequadamente
- **Mitigação**: Testes de integração obrigatórios para isolamento
- **Nota**: Admin Central usa `.IgnoreQueryFilters()` quando necessário

**7. ASP.NET Core Identity vs Implementação Custom**
- **Escolha**: Implementação custom
- **Justificativa**:
  - Identity é complexo para necessidades simples do MVP
  - Barbeiro e Cliente não têm senha no MVP (Identity assume senha)
  - Controle total sobre schema e fluxo
  - Multi-tenancy com filtros customizados é mais simples
- **Trade-off**: Sem features prontas (recuperação de senha, 2FA, etc.)
- **Evolução Futura**: Migrar para Identity Passwordless em Fase 2 se necessário

**8. Primeiro Acesso Admin Barbearia**
- **Escolha**: Admin Central cria Admin Barbearia com senha temporária
- **Justificativa**:
  - Fluxo controlado (Admin Central gerencia acesso)
  - Senha pode ser enviada por email (fora do escopo MVP)
- **MVP**: Admin Central cria e informa senha manualmente
- **Fase 2**: Endpoint de "first access" com token temporário

### Riscos Conhecidos

**1. Colisão de Telefone entre Barbearias**
- **Risco**: Mesmo telefone pode estar em múltiplas barbearias (by design)
- **Mitigação**: Constraint `UNIQUE(telefone, barbearia_id)` + validação de código obrigatória no login

**2. Token Expiration sem Refresh**
- **Risco**: Usuário perde sessão após 24h sem aviso
- **Mitigação**: Frontend exibe warning 1h antes da expiração + redireciona para login automaticamente

**3. Global Query Filter Bypass**
- **Risco**: Desenvolvedor esquece de testar isolamento e vaza dados
- **Mitigação**: Testes de integração obrigatórios + code review focado em isolamento

**4. Brute Force Attack**
- **Risco**: Atacante pode tentar múltiplas senhas sem limitação
- **Mitigação MVP**: Logging de tentativas falhadas (detecção manual)
- **Fase 2**: Rate limiting (5 tentativas por 15 minutos)

**5. CORS Misconfiguration**
- **Risco**: Frontend de domínio diferente não consegue enviar cookies
- **Mitigação MVP**: Configurar CORS com `AllowAnyOrigin()` + `AllowCredentials()` para facilitar desenvolvimento
- **Produção**: Restringir para domínios específicos (ex: `https://app.barbapp.com`)
- **Nota**: Revisar configuração antes de deploy em produção

### Requisitos Especiais

**Performance**:
- Autenticação deve responder em < 1 segundo (requisito do PRD)
- Validação de JWT deve ser < 100ms
- Global Query Filters devem adicionar < 50ms às queries

**Segurança**:
- **HTTPS obrigatório em produção** (cookies Secure)
- **Secret Key**: Mínimo 32 caracteres, armazenado em variável de ambiente
- **Password Hashing**: BCrypt com work factor 12
- **CORS**: Configurado inicialmente para permitir qualquer origem no MVP (desenvolvimento), restringir em produção
- **Logs**: Nunca logar senhas ou tokens completos (apenas primeiros 10 caracteres para debug)
- **Validação de Telefone**: Apenas Brasil (+55), formato validado mas sem verificação por SMS no MVP

**Escalabilidade**:
- Sistema deve suportar 10.000 usuários ativos simultaneamente
- JWT stateless permite scaling horizontal sem shared session
- Global Query Filters têm overhead mínimo (compiled queries)

**LGPD**:
- Telefones armazenados sem formatação (apenas números)
- Logs não devem conter telefones completos (mascarar: `119****4321`)
- Exclusão de barbearia deve cascadear para todos os usuários vinculados

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
- Sem magic numbers (constantes: `WorkFactor = 12`, `TokenExpirationHours = 24`)

**✅ HTTP/REST** (`rules/http.md`):
- Endpoints RESTful: `/api/auth/{perfil}`
- Métodos HTTP apropriados (POST para autenticação)
- Status codes corretos: 200 (sucesso), 400 (bad request), 401 (unauthorized), 403 (forbidden), 404 (not found)
- Payload JSON
- Documentação OpenAPI

**✅ SQL** (`rules/sql.md`):
- Tabelas em plural: `admin_central_users`, `barbers`, `customers`
- snake_case para tabelas e colunas
- PKs: `admin_central_user_id`, `barber_id`, `customer_id`
- FKs: `barbearia_id`
- Constraints: `UNIQUE(barbearia_id, telefone)`
- Índices em colunas de busca: `telefone`, `email`, `barbearia_id`
- `created_at`, `updated_at` em todas as tabelas

**✅ Testes** (`rules/tests.md`):
- xUnit para estrutura de testes
- Moq para mocks
- AAA pattern (Arrange, Act, Assert)
- FluentAssertions para asserções legíveis
- TestContainers para testes de integração
- Projetos separados: `BarbApp.Domain.Tests`, `BarbApp.Application.Tests`, `BarbApp.IntegrationTests`

**✅ Logging** (`rules/logging.md`):
- ILogger<T> injetado via DI
- Logging estruturado com templates
- Níveis apropriados (Info, Warning, Error)
- Nunca logar dados sensíveis (senhas, tokens completos)
- Logs de exceções com stack trace

**✅ Unit of Work** (`rules/unit-of-work.md`):
- Interface `IUnitOfWork` com `Commit` e `Rollback`
- Use cases chamam `UnitOfWork.Commit()` após operações
- Transações coordenadas

---

## Decisões Finalizadas (Questões Abertas Resolvidas)

### ✅ Confirmações de Stakeholders (2025-10-11)

**1. Primeiro Acesso Admin Barbearia**
- **Decisão Confirmada**: Admin Central criará Admin Barbearia manualmente no MVP
- **Fluxo**: Admin Central envia credenciais por email/WhatsApp (fora do sistema)
- **Implementação**: Endpoint `POST /api/admin-barbearia` (uso exclusivo do Admin Central)

**2. Expiração de Token (24h)**
- **Decisão Confirmada**: Expiração fixa de 24h sem refresh token é aceitável para MVP
- **UX**: Frontend exibirá warning 1h antes da expiração
- **Comportamento**: Usuário faz login novamente após expiração

**3. Logout sem Invalidação Imediata**
- **Decisão Confirmada**: Token permanece válido até expiração natural após logout
- **Justificativa**: Aceitável para MVP, token removido do cliente é suficiente
- **Mitigação**: Tokens de curta duração (24h) limitam janela de risco
- **Evolução Futura**: Blacklist com Redis em Fase 2

**4. Taxa de Falha de Autenticação**
- **Decisão Confirmada**: Alerta quando taxa de falha > 50% em 5 minutos
- **Implementação**: Métricas Prometheus + alerta no Grafana
- **Monitoramento**: Dashboard com taxa de sucesso/falha por perfil

**5. Suporte de Telefones**
- **Decisão Confirmada**: MVP suporta apenas Brasil (+55)
- **Validação**: Regex `^\d{10,11}$` (10 ou 11 dígitos sem formatação)
- **Formato Aceito**: `11987654321` ou `1134567890`
- **Evolução Futura**: Telefones internacionais em versões posteriores

**6. CORS Origins**
- **Decisão Confirmada**: Permitir qualquer origem inicialmente para facilitar desenvolvimento
- **Configuração MVP**: `AllowAnyOrigin()` + `AllowCredentials()` (desenvolvimento)
- **Produção**: Restringir para domínios específicos quando deploy
- **Nota**: Revisar antes de produção para segurança

### Impacto das Decisões

**Simplificações Aprovadas**:
- ✅ Sem refresh token (reduz complexidade em ~8h de desenvolvimento)
- ✅ Sem token blacklist (evita dependência de Redis)
- ✅ Apenas telefones brasileiros (valida com regex simples)
- ✅ CORS permissivo no MVP (facilita desenvolvimento frontend)

**Próximas Ações**:
1. Implementar validação de telefone brasileiro (regex)
2. Configurar CORS permissivo para desenvolvimento
3. Adicionar endpoint para Admin Central criar Admin Barbearia
4. Implementar warning de expiração no frontend
5. Documentar processo manual de envio de credenciais

---

**Data de Criação**: 2025-10-11  
**Última Atualização**: 2025-10-11 (Questões abertas resolvidas)
**Versão**: 1.1  
**Status**: Pronto para Implementação
