// BarbApp.Application/Validators/LoginBarbeiroInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class LoginBarbeiroInputValidator : AbstractValidator<DTOs.LoginBarbeiroInput>
{
    public LoginBarbeiroInputValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
    }
}