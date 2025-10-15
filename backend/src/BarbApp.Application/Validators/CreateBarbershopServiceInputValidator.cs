using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CreateBarbershopServiceInputValidator : AbstractValidator<CreateBarbershopServiceInput>
{
    public CreateBarbershopServiceInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome do serviço é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duração deve ser maior que zero");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Preço deve ser zero ou maior");
    }
}