// BarbApp.Application/Validators/TrocarContextoInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class TrocarContextoInputValidator : AbstractValidator<DTOs.TrocarContextoInput>
{
    public TrocarContextoInputValidator()
    {
        RuleFor(x => x.NovaBarbeariaId)
            .NotEmpty().WithMessage("NovaBarbeariaId é obrigatório");
    }
}