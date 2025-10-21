// BarbApp.Application/Validators/LoginBarbeiroInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class LoginBarbeiroInputValidator : AbstractValidator<DTOs.LoginBarbeiroInput>
{
    public LoginBarbeiroInputValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("A senha deve ter no mínimo 6 caracteres");
    }
}