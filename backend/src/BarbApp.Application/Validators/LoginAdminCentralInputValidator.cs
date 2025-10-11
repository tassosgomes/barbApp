// BarbApp.Application/Validators/LoginAdminCentralInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class LoginAdminCentralInputValidator : AbstractValidator<DTOs.LoginAdminCentralInput>
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