---
status: completed
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>Contratos de API</domain>
<type>Implementação</type>
<scope>DTOs e Validação</scope>
<complexity>baixa</complexity>
<dependencies>FluentValidation</dependencies>
<unblocks>"7.0", "10.0"</unblocks>
</task_context>

# Tarefa 6.0: Criar DTOs e Validadores

## Visão Geral
Criar os Data Transfer Objects (DTOs) para input e output da API, incluindo validadores com FluentValidation para garantir integridade dos dados de entrada.

<requirements>
- DTOs de Input para cada endpoint de autenticação
- DTOs de Output padronizados com informações de token
- Validadores FluentValidation para todos os inputs
- Mensagens de erro claras e consistentes
- Suporte a múltiplos idiomas (pt-BR, en-US)
</requirements>

## Subtarefas
- [x] 6.1 Criar DTOs de Input (Login, TrocarContexto)
- [x] 6.2 Criar DTOs de Output padronizados com informações de token
- [x] 6.3 Implementar validadores FluentValidation
- [x] 6.4 Configurar mensagens de validação
- [x] 6.5 Criar testes de validação

## Sequenciamento
- **Bloqueado por**: 4.0 (Domínio, Entidades e Interfaces)
- **Desbloqueia**: 7.0 (Use Cases), 10.0 (Controllers)
- **Paralelizável**: Sim (pode ser desenvolvido em paralelo com 5.0)

## Detalhes de Implementação

### DTOs de Input

```csharp
// LoginAdminCentralInput.cs
public record LoginAdminCentralInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

// LoginAdminBarbeariaInput.cs
public record LoginAdminBarbeariaInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public Guid BarbeariaId { get; init; }
}

// LoginBarbeiroInput.cs
public record LoginBarbeiroInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
    public Guid BarbeariaId { get; init; }
}

// LoginClienteInput.cs
public record LoginClienteInput
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

// TrocarContextoInput.cs
public record TrocarContextoInput
{
    public Guid NovaBarbeariaId { get; init; }
}
```

### DTOs de Output

```csharp
// AuthResponse.cs
public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public string TipoUsuario { get; init; } = string.Empty;
    public Guid? BarbeariaId { get; init; }
    public string NomeBarbearia { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}

// BarberInfo.cs
public record BarberInfo
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Guid BarbeariaId { get; init; }
    public string NomeBarbearia { get; init; } = string.Empty;
}
```

### Validadores FluentValidation

```csharp
// LoginAdminCentralInputValidator.cs
public class LoginAdminCentralInputValidator : AbstractValidator<LoginAdminCentralInput>
{
    public LoginAdminCentralInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}

// LoginAdminBarbeariaInputValidator.cs
public class LoginAdminBarbeariaInputValidator : AbstractValidator<LoginAdminBarbeariaInput>
{
    public LoginAdminBarbeariaInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

        RuleFor(x => x.BarbeariaId)
            .NotEmpty().WithMessage("BarbeariaId é obrigatório");
    }
}

// LoginBarbeiroInputValidator.cs
public class LoginBarbeiroInputValidator : AbstractValidator<LoginBarbeiroInput>
{
    public LoginBarbeiroInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

        RuleFor(x => x.BarbeariaId)
            .NotEmpty().WithMessage("BarbeariaId é obrigatório");
    }
}

// LoginClienteInputValidator.cs
public class LoginClienteInputValidator : AbstractValidator<LoginClienteInput>
{
    public LoginClienteInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}

// TrocarContextoInputValidator.cs
public class TrocarContextoInputValidator : AbstractValidator<TrocarContextoInput>
{
    public TrocarContextoInputValidator()
    {
        RuleFor(x => x.NovaBarbeariaId)
            .NotEmpty().WithMessage("NovaBarbeariaId é obrigatório");
    }
}
```

## Critérios de Sucesso
- ✅ Todos os DTOs criados com propriedades apropriadas
- ✅ Validadores implementados para todos os inputs
- ✅ Mensagens de erro claras e consistentes
- ✅ Testes de validação cobrem cenários válidos e inválidos
- ✅ DTOs utilizam records para imutabilidade
- ✅ Validação integrada com pipeline ASP.NET Core

## Tempo Estimado
**2 horas**

## Referências
- TechSpec: Seção "4.2 Fase 1.2: DTOs e Validação"
- PRD: Seção "Requisitos de API"
- FluentValidation Documentation
