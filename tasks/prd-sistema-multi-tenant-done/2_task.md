---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>domain/authentication</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>none</dependencies>
<unblocks>"3.0", "4.0"</unblocks>
</task_context>

# Tarefa 2.0: Implementar Domain Layer Base

## Visão Geral

Implementar componentes fundamentais da camada de domínio: Value Object `BarbeariaCode`, interfaces de contratos (`ITenantContext`, `IJwtTokenGenerator`, `IPasswordHasher`, `IAuthenticationService`) e exceções customizadas. Esta é a base conceitual do sistema multi-tenant.

<requirements>
- Value Object BarbeariaCode com validação completa (8 caracteres, uppercase, sem O/I/0/1)
- Interface ITenantContext para extração de contexto da barbearia
- Interfaces de serviços de autenticação
- Exceções customizadas: InvalidBarbeariaCodeException, UnauthorizedAccessException, BarbeariaInactiveException
- Testes unitários para BarbeariaCode
</requirements>

## Subtarefas

- [ ] 2.1 Criar Value Object BarbeariaCode com validação
- [ ] 2.2 Implementar interface ITenantContext
- [ ] 2.3 Criar interfaces: IJwtTokenGenerator, IPasswordHasher, IAuthenticationService
- [ ] 2.4 Implementar exceções customizadas do domínio
- [ ] 2.5 Criar testes unitários para BarbeariaCode (xUnit + FluentAssertions)
- [ ] 2.6 Validar que todos os testes passam

## Sequenciamento

- **Bloqueado por**: 1.0 (Setup e Dependências)
- **Desbloqueia**: 3.0 (Entidades de Usuários), 4.0 (DbContext)
- **Paralelizável**: Não (fundação conceitual)

## Detalhes de Implementação

### Value Object - BarbeariaCode

```csharp
// BarbApp.Domain/ValueObjects/BarbeariaCode.cs
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

    public override string ToString() => Value;

    public override bool Equals(object? obj) =>
        obj is BarbeariaCode other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}
```

### Interface ITenantContext

```csharp
// BarbApp.Domain/Interfaces/ITenantContext.cs
public interface ITenantContext
{
    Guid? BarbeariaId { get; }
    string? BarbeariaCode { get; }
    bool IsAdminCentral { get; }
    string UserId { get; }
    string Role { get; }
}
```

### Interfaces de Serviços (Application Layer)

```csharp
// BarbApp.Application/Interfaces/IAuthenticationService.cs
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

// BarbApp.Application/Interfaces/IJwtTokenGenerator.cs
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

// BarbApp.Application/Interfaces/IPasswordHasher.cs
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
```

### Exceções Customizadas

```csharp
// BarbApp.Domain/Exceptions/InvalidBarbeariaCodeException.cs
public class InvalidBarbeariaCodeException : DomainException
{
    public InvalidBarbeariaCodeException(string message)
        : base(message) { }
}

// BarbApp.Domain/Exceptions/DomainException.cs (base)
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
```

### Testes Unitários

```csharp
// BarbApp.Domain.Tests/ValueObjects/BarbeariaCodeTests.cs
public class BarbeariaCodeTests
{
    [Theory]
    [InlineData("ABC12345")]
    [InlineData("XYZ98765")]
    [InlineData("22334455")]
    [InlineData("abcd2345")] // Deve converter para uppercase
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
    [InlineData(null)] // Null
    public void Create_InvalidCode_ShouldThrowException(string codeValue)
    {
        // Act & Assert
        Action act = () => BarbeariaCode.Create(codeValue);
        act.Should().Throw<InvalidBarbeariaCodeException>();
    }

    [Fact]
    public void Equals_SameCodes_ShouldBeEqual()
    {
        // Arrange
        var code1 = BarbeariaCode.Create("ABC12345");
        var code2 = BarbeariaCode.Create("ABC12345");

        // Act & Assert
        code1.Should().Be(code2);
        (code1 == code2).Should().BeTrue();
    }
}
```

## Critérios de Sucesso

- ✅ BarbeariaCode valida corretamente códigos de 8 caracteres
- ✅ BarbeariaCode rejeita códigos com O, I, 0, 1
- ✅ BarbeariaCode converte para uppercase automaticamente
- ✅ Todas as interfaces definidas e compilando
- ✅ Exceções customizadas implementadas
- ✅ Todos os testes unitários passando (mínimo 10 testes)
- ✅ Cobertura de código > 90% no BarbeariaCode
- ✅ Build executando sem warnings: `dotnet build`

## Tempo Estimado

**3 horas**

## Referências

- TechSpec: Seção "Design de Implementação" - Interfaces Principais
- TechSpec: Seção "Modelos de Dados" - Value Object BarbeariaCode
- TechSpec: Seção "Abordagem de Testes" - Testes Unitários Domain Layer
- PRD: Funcionalidade 1 - Identificação de Contexto por Código/URL
