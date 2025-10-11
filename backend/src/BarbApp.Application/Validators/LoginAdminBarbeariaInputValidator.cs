// BarbApp.Application/Validators/LoginAdminBarbeariaInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class LoginAdminBarbeariaInputValidator : AbstractValidator<DTOs.LoginAdminBarbeariaInput>
{
    public LoginAdminBarbeariaInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter no mínimo 6 caracteres");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório");
    }
}