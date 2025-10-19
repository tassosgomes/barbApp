using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class UpdateBarberInputValidator : AbstractValidator<UpdateBarberInput>
{
    public UpdateBarberInputValidator()
    {
        // Id vem da rota, não precisa validar no body

        When(x => x.Name != null, () =>
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nome não pode ser vazio")
                .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");
        });

        When(x => x.Phone != null, () =>
        {
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Telefone não pode ser vazio")
                .Must(BeValidPhone!).WithMessage("Telefone deve ser válido (formato brasileiro)");
        });
    }

    private bool BeValidPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;

        var cleaned = System.Text.RegularExpressions.Regex.Replace(phone, @"[^\d]", "");

        // Allow 10 or 11 digits (Brazilian format)
        return cleaned.Length == 10 || cleaned.Length == 11;
    }
}