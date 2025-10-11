// BarbApp.Application/Validators/LoginClienteInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class LoginClienteInputValidator : AbstractValidator<DTOs.LoginClienteInput>
{
    public LoginClienteInputValidator()
    {
        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");
    }
}