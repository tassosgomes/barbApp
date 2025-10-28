// BarbApp.Application/Validators/EditarAgendamentoInputValidator.cs
using FluentValidation;

namespace BarbApp.Application.Validators;

public class EditarAgendamentoInputValidator : AbstractValidator<DTOs.EditarAgendamentoInput>
{
    public EditarAgendamentoInputValidator()
    {
        RuleFor(x => x.AgendamentoId)
            .NotEmpty().WithMessage("ID do agendamento é obrigatório");

        When(x => x.BarbeiroId.HasValue, () =>
        {
            RuleFor(x => x.BarbeiroId!.Value)
                .NotEmpty().WithMessage("ID do barbeiro não pode ser vazio");
        });

        When(x => x.ServicosIds != null, () =>
        {
            RuleFor(x => x.ServicosIds)
                .NotEmpty().WithMessage("Lista de serviços não pode ser vazia")
                .ForEach(servicoId => servicoId.NotEmpty().WithMessage("ID do serviço não pode ser vazio"));
        });

        When(x => x.DataHora.HasValue, () =>
        {
            RuleFor(x => x.DataHora!.Value)
                .GreaterThan(DateTime.UtcNow).WithMessage("Data e hora devem ser futuras")
                .Must(BeValidBusinessHour).WithMessage("Horário deve estar entre 08:00 e 20:00");
        });
    }

    private bool BeValidBusinessHour(DateTime dataHora)
    {
        var hora = dataHora.Hour;
        return hora >= 8 && hora < 20;
    }
}