using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CreateBarberInputValidator : AbstractValidator<CreateBarberInput>
{
    public CreateBarberInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido")
            .MaximumLength(255).WithMessage("Email deve ter no máximo 255 caracteres");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Must(BeValidPhone).WithMessage("Telefone deve ser válido (formato brasileiro)");

        RuleFor(x => x.ServiceIds)
            .NotNull().WithMessage("Lista de serviços é obrigatória");
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