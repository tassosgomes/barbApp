---
status: pending
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>domain/entities</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>"4.0", "6.0"</unblocks>
</task_context>

# Tarefa 3.0: Implementar Entidades de Usuários (Domain)

## Visão Geral

Implementar as 4 entidades de usuários do domínio: `AdminCentralUser`, `AdminBarbeariaUser`, `Barber` e `Customer`. Cada entidade possui lógica de criação, validação e relacionamentos apropriados com a entidade `Barbershop`. Foco em Rich Domain Model com factory methods e encapsulamento.

<requirements>
- Entidade AdminCentralUser (sem vínculo com barbearia)
- Entidade AdminBarbeariaUser (vinculada a uma barbearia)
- Entidade Barber (multi-tenant, pode estar em múltiplas barbearias)
- Entidade Customer (multi-tenant, pode estar em múltiplas barbearias)
- Validação de telefone brasileiro (10 ou 11 dígitos)
- Factory methods para criação segura
- Testes unitários para todas as entidades
</requirements>

## Subtarefas

- [ ] 3.1 Implementar entidade AdminCentralUser
- [ ] 3.2 Implementar entidade AdminBarbeariaUser
- [ ] 3.3 Implementar entidade Barber com validação de telefone
- [ ] 3.4 Implementar entidade Customer com validação de telefone
- [ ] 3.5 Adicionar propriedades de navegação para Barbershop
- [ ] 3.6 Criar testes unitários para validação de telefone
- [ ] 3.7 Criar testes unitários para factory methods
- [ ] 3.8 Validar que todos os testes passam

## Sequenciamento

- **Bloqueado por**: 2.0 (Domain Layer Base)
- **Desbloqueia**: 4.0 (DbContext), 6.0 (DTOs)
- **Paralelizável**: Sim (com tarefa 4.0 - DbContext pode ser preparado em paralelo)

## Detalhes de Implementação

### AdminCentralUser

```csharp
// BarbApp.Domain/Entities/AdminCentralUser.cs
public class AdminCentralUser
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private AdminCentralUser() { } // EF Core

    public static AdminCentralUser Create(
        string email,
        string passwordHash,
        string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new AdminCentralUser
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool VerifyPassword(string passwordHash)
    {
        return PasswordHash == passwordHash;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

### AdminBarbeariaUser

```csharp
// BarbApp.Domain/Entities/AdminBarbeariaUser.cs
public class AdminBarbeariaUser
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
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
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new AdminBarbeariaUser
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Email = email.ToLowerInvariant().Trim(),
            PasswordHash = passwordHash,
            Name = name.Trim(),
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
```

### Barber

```csharp
// BarbApp.Domain/Entities/Barber.cs
public class Barber
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
    public string Telefone { get; private set; }
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
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new Barber
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Telefone = CleanAndValidatePhone(telefone),
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string CleanAndValidatePhone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone is required");

        var cleaned = Regex.Replace(telefone, @"[^\d]", "");

        // Validar formato brasileiro: 10 ou 11 dígitos (DDD + número)
        if (!Regex.IsMatch(cleaned, @"^\d{10,11}$"))
            throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");

        return cleaned;
    }
}
```

### Customer

```csharp
// BarbApp.Domain/Entities/Customer.cs
public class Customer
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Barbershop Barbearia { get; private set; } = null!;
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
        if (barbeariaId == Guid.Empty)
            throw new ArgumentException("Barbearia ID is required");
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        return new Customer
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Telefone = CleanAndValidatePhone(telefone),
            Name = name.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static string CleanAndValidatePhone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("Telefone is required");

        var cleaned = Regex.Replace(telefone, @"[^\d]", "");

        // Validar formato brasileiro: 10 ou 11 dígitos (DDD + número)
        if (!Regex.IsMatch(cleaned, @"^\d{10,11}$"))
            throw new ArgumentException("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");

        return cleaned;
    }
}
```

### Testes Unitários

```csharp
// BarbApp.Domain.Tests/Entities/BarberTests.cs
public class BarberTests
{
    [Theory]
    [InlineData("11987654321")]
    [InlineData("1134567890")]
    [InlineData("(11) 98765-4321")] // Deve limpar formatação
    [InlineData("+55 11 98765-4321")] // Deve limpar +55 e formatação
    public void Create_ValidPhone_ShouldSucceed(string telefone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act
        var barber = Barber.Create(barbeariaId, telefone, "João Silva");

        // Assert
        barber.Should().NotBeNull();
        barber.Telefone.Should().MatchRegex(@"^\d{10,11}$");
    }

    [Theory]
    [InlineData("123")] // Muito curto
    [InlineData("123456789012")] // Muito longo
    [InlineData("")] // Vazio
    [InlineData(null)] // Null
    public void Create_InvalidPhone_ShouldThrowException(string telefone)
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();

        // Act & Assert
        Action act = () => Barber.Create(barbeariaId, telefone, "João Silva");
        act.Should().Throw<ArgumentException>();
    }
}
```

## Critérios de Sucesso

- ✅ Todas as 4 entidades implementadas com factory methods
- ✅ Validação de telefone brasileiro funcionando (10 ou 11 dígitos)
- ✅ Telefone armazenado sem formatação (apenas números)
- ✅ Email convertido para lowercase automaticamente
- ✅ Relacionamentos com Barbershop configurados
- ✅ Todos os testes unitários passando (mínimo 15 testes)
- ✅ Cobertura de código > 85% nas entidades
- ✅ Build executando sem warnings: `dotnet build`

## Tempo Estimado

**4 horas**

## Referências

- TechSpec: Seção "Modelos de Dados" - Entidades Domain
- TechSpec: Seção "Schema do Banco de Dados"
- PRD: Funcionalidade 7 - Cadastro Multi-vinculado
- PRD: Questões em Aberto #13 - Telefones apenas Brasil
