---
status: completed
parallelizable: false
blocked_by: ["5.0", "6.0"]
completed_at: 2025-01-27
---

<task_context>
<domain>Lógica de Negócio</domain>
<type>Implementação</type>
<scope>Use Cases</scope>
<complexity>alta</complexity>
<dependencies>Repositories, DTOs, JWT, PasswordHasher</dependencies>
<unblocks>"10.0"</unblocks>
</task_context>

# Tarefa 7.0: Implementar Use Cases de Autenticação

## Visão Geral
Implementar os 6 use cases de autenticação que encapsulam a lógica de negócio para login de diferentes tipos de usuários, listagem de barbeiros e troca de contexto de tenant.

<requirements>
- AuthenticateAdminCentralUseCase
- AuthenticateAdminBarbeariaUseCase
- AuthenticateBarbeiroUseCase
- AuthenticateClienteUseCase
- ListBarbeirosBarbeariaUseCase
- TrocarContextoBarbeiroUseCase
- Validação de credenciais e hash de senha
- Geração de tokens JWT
- Isolamento de tenant
- Tratamento de erros apropriado
</requirements>

## Subtarefas
- [x] 7.1 Implementar AuthenticateAdminCentralUseCase
- [x] 7.2 Implementar AuthenticateAdminBarbeariaUseCase
- [x] 7.3 Implementar AuthenticateBarbeiroUseCase
- [x] 7.4 Implementar AuthenticateClienteUseCase
- [x] 7.5 Implementar ListBarbeirosBarbeariaUseCase
- [x] 7.6 Implementar TrocarContextoBarbeiroUseCase
- [x] 7.7 Criar testes unitários para cada use case
- [x] 7.8 Implementar tratamento de erros e exceções customizadas

## Sequenciamento
- **Bloqueado por**: 5.0 (Repositories), 6.0 (DTOs)
- **Desbloqueia**: 10.0 (Controllers)
- **Paralelizável**: Não (depende de repositórios e DTOs)

## Detalhes de Implementação

### AuthenticateAdminCentralUseCase
```csharp
public class AuthenticateAdminCentralUseCase
{
    private readonly IAdminCentralUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateAdminCentralUseCase(
        IAdminCentralUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginAdminCentralInput input)
    {
        var user = await _repository.GetByEmailAsync(input.Email);

        if (user == null || !_passwordHasher.VerifyPassword(input.Senha, user.SenhaHash))
        {
            throw new UnauthorizedException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: user.Id,
            userType: "AdminCentral",
            email: user.Email,
            barbeariaId: null
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "AdminCentral",
            BarbeariaId = null,
            NomeBarbearia = string.Empty,
            ExpiresAt = token.ExpiresAt
        };
    }
}
```

### AuthenticateAdminBarbeariaUseCase
```csharp
public class AuthenticateAdminBarbeariaUseCase
{
    private readonly IAdminBarbeariaUserRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateAdminBarbeariaUseCase(
        IAdminBarbeariaUserRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginAdminBarbeariaInput input)
    {
        var user = await _repository.GetByEmailAndBarbeariaIdAsync(
            input.Email,
            input.BarbeariaId);

        if (user == null || !_passwordHasher.VerifyPassword(input.Senha, user.SenhaHash))
        {
            throw new UnauthorizedException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: user.Id,
            userType: "AdminBarbearia",
            email: user.Email,
            barbeariaId: user.BarbeariaId
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "AdminBarbearia",
            BarbeariaId = user.BarbeariaId,
            NomeBarbearia = user.Barbearia.Nome,
            ExpiresAt = token.ExpiresAt
        };
    }
}
```

### AuthenticateBarbeiroUseCase
```csharp
public class AuthenticateBarbeiroUseCase
{
    private readonly IBarberRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateBarbeiroUseCase(
        IBarberRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginBarbeiroInput input)
    {
        var barber = await _repository.GetByEmailAndBarbeariaIdAsync(
            input.Email,
            input.BarbeariaId);

        if (barber == null || !_passwordHasher.VerifyPassword(input.Senha, barber.SenhaHash))
        {
            throw new UnauthorizedException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: barber.Id,
            userType: "Barbeiro",
            email: barber.Email,
            barbeariaId: barber.BarbeariaId
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Barbeiro",
            BarbeariaId = barber.BarbeariaId,
            NomeBarbearia = barber.Barbearia.Nome,
            ExpiresAt = token.ExpiresAt
        };
    }
}
```

### AuthenticateClienteUseCase
```csharp
public class AuthenticateClienteUseCase
{
    private readonly ICustomerRepository _repository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;

    public AuthenticateClienteUseCase(
        ICustomerRepository repository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator)
    {
        _repository = repository;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
    }

    public async Task<AuthResponse> ExecuteAsync(LoginClienteInput input)
    {
        var customer = await _repository.GetByEmailAsync(input.Email);

        if (customer == null || !_passwordHasher.VerifyPassword(input.Senha, customer.SenhaHash))
        {
            throw new UnauthorizedException("Credenciais inválidas");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: customer.Id,
            userType: "Cliente",
            email: customer.Email,
            barbeariaId: null
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Cliente",
            BarbeariaId = null,
            NomeBarbearia = string.Empty,
            ExpiresAt = token.ExpiresAt
        };
    }
}
```

### ListBarbeirosBarbeariaUseCase
```csharp
public class ListBarbeirosBarbeariaUseCase
{
    private readonly IBarberRepository _repository;
    private readonly ITenantContext _tenantContext;

    public ListBarbeirosBarbeariaUseCase(
        IBarberRepository repository,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tenantContext = tenantContext;
    }

    public async Task<IEnumerable<BarberInfo>> ExecuteAsync()
    {
        var barbeariaId = _tenantContext.GetCurrentBarbeariaId();

        if (barbeariaId == null)
        {
            throw new UnauthorizedException("Contexto de barbearia não definido");
        }

        var barbers = await _repository.GetByBarbeariaIdAsync(barbeariaId.Value);

        return barbers.Select(b => new BarberInfo
        {
            Id = b.Id,
            Nome = b.Nome,
            Email = b.Email,
            BarbeariaId = b.BarbeariaId,
            NomeBarbearia = b.Barbearia.Nome
        });
    }
}
```

### TrocarContextoBarbeiroUseCase
```csharp
public class TrocarContextoBarbeiroUseCase
{
    private readonly IBarberRepository _repository;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ITenantContext _tenantContext;

    public TrocarContextoBarbeiroUseCase(
        IBarberRepository repository,
        IJwtTokenGenerator tokenGenerator,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _tokenGenerator = tokenGenerator;
        _tenantContext = tenantContext;
    }

    public async Task<AuthResponse> ExecuteAsync(TrocarContextoInput input)
    {
        var userId = _tenantContext.GetCurrentUserId();
        var email = _tenantContext.GetCurrentUserEmail();

        if (userId == null || email == null)
        {
            throw new UnauthorizedException("Usuário não autenticado");
        }

        var barber = await _repository.GetByEmailAndBarbeariaIdAsync(
            email,
            input.NovaBarbeariaId);

        if (barber == null)
        {
            throw new NotFoundException("Barbeiro não encontrado na barbearia especificada");
        }

        var token = _tokenGenerator.GenerateToken(
            userId: barber.Id,
            userType: "Barbeiro",
            email: barber.Email,
            barbeariaId: barber.BarbeariaId
        );

        return new AuthResponse
        {
            Token = token.Value,
            TipoUsuario = "Barbeiro",
            BarbeariaId = barber.BarbeariaId,
            NomeBarbearia = barber.Barbearia.Nome,
            ExpiresAt = token.ExpiresAt
        };
    }
}
```

## Critérios de Sucesso
- ✅ Todos os 6 use cases implementados corretamente
- ✅ Validação de credenciais funciona apropriadamente
- ✅ Tokens JWT gerados com claims corretos
- ✅ Isolamento de tenant mantido em operações com contexto
- ✅ Exceções customizadas lançadas em cenários de erro
- ✅ Testes unitários cobrem cenários de sucesso e falha
- ✅ Código segue princípios SOLID e Clean Architecture

## Tempo Estimado
**8 horas**

## Referências
- TechSpec: Seção "4.3 Fase 1.3: Use Cases de Autenticação"
- PRD: Seção "Casos de Uso de Autenticação"
- Clean Architecture Patterns

## Resumo de Implementação

### ✅ Status: CONCLUÍDO

**Data de Conclusão**: 2025-01-27

**Implementação Realizada**:
- ✅ **6 Use Cases Implementados**: AuthenticateAdminCentralUseCase, AuthenticateAdminBarbeariaUseCase, AuthenticateBarbeiroUseCase, AuthenticateClienteUseCase, ListBarbeirosBarbeariaUseCase, TrocarContextoBarbeiroUseCase
- ✅ **Serviços de Infraestrutura**: JwtTokenGenerator (HS256), PasswordHasher (BCrypt), TenantContext
- ✅ **Isolamento Multi-tenant**: Tokens JWT incluem barbeariaCode, filtros automáticos por tenant
- ✅ **Testes Unitários**: 155 testes passando, cobertura completa de cenários sucesso/falha
- ✅ **Validação PRD**: Conformidade total com requisitos de autenticação multi-perfil
- ✅ **Padrões de Código**: Clean Architecture, SOLID, early returns, injeção de dependência

**Arquivos Criados/Modificados**:
- `src/BarbApp.Application/UseCases/` - 6 use cases atualizados
- `src/BarbApp.Infrastructure/Services/` - 3 novos serviços
- `tests/BarbApp.Application.Tests/UseCases/` - 6 arquivos de teste
- `src/BarbApp.Application/Interfaces/IJwtTokenGenerator.cs` - interface atualizada

**Validações Realizadas**:
- ✅ Build e compilação sem erros
- ✅ Todos os testes passando (155/155)
- ✅ Conformidade com regras de código (code-standard.md)
- ✅ Validação contra PRD de autenticação
- ✅ Commit seguindo padrões git-commit.md

**Dependências Desbloqueadas**: Task 10.0 (Controllers) pode ser iniciada.
