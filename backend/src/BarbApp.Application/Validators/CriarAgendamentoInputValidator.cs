// BarbApp.Application/Validators/CriarAgendamentoInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CriarAgendamentoInputValidator : AbstractValidator<DTOs.CriarAgendamentoInput>
{
    public CriarAgendamentoInputValidator()
    {
        RuleFor(x => x.BarbeiroId)
            .NotEmpty().WithMessage("ID do barbeiro é obrigatório");

        RuleFor(x => x.ServicosIds)
            .NotNull().WithMessage("Lista de serviços é obrigatória")
            .NotEmpty().WithMessage("Pelo menos um serviço deve ser selecionado")
            .ForEach(servicoId => servicoId.NotEmpty().WithMessage("ID do serviço não pode ser vazio"));

        RuleFor(x => x.DataHora)
            .NotEmpty().WithMessage("Data e hora são obrigatórias")
            .GreaterThan(DateTime.UtcNow).WithMessage("Data e hora devem ser futuras")
            .Must(BeValidBusinessHour).WithMessage("Horário deve estar entre 08:00 e 20:00");
    }

    private bool BeValidBusinessHour(DateTime dataHora)
    {
        var hora = dataHora.Hour;
        return hora >= 8 && hora < 20;
    }
}