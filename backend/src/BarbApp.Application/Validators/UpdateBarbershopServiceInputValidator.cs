using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class UpdateBarbershopServiceInputValidator : AbstractValidator<UpdateBarbershopServiceInput>
{
    public UpdateBarbershopServiceInputValidator()
    {
        // Id vem da rota, não precisa validar no body

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome do serviço não pode ser vazio")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");
        });

        When(x => x.Description != null, () =>
        {
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        });

        When(x => x.DurationMinutes.HasValue, () =>
        {
            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("Duração deve ser maior que zero");
        });

        When(x => x.Price.HasValue, () =>
        {
            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("Preço deve ser zero ou maior");
        });
    }
}