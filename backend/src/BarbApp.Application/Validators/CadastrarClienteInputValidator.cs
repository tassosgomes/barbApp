// BarbApp.Application/Validators/CadastrarClienteInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CadastrarClienteInputValidator : AbstractValidator<DTOs.CadastrarClienteInput>
{
    public CadastrarClienteInputValidator()
    {
        RuleFor(x => x.CodigoBarbearia)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório")
            .Length(8).WithMessage("Código da barbearia deve ter 8 caracteres");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos (formato brasileiro)");
    }
}