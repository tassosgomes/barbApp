using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CreateBarbershopInputValidator : AbstractValidator<CreateBarbershopInput>
{
    public CreateBarbershopInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da barbearia é obrigatório")
            .MaximumLength(255).WithMessage("Nome deve ter no máximo 255 caracteres");

        RuleFor(x => x.Document)
            .NotEmpty().WithMessage("Documento é obrigatório")
            .Must(BeValidDocument).WithMessage("Documento deve ser um CNPJ ou CPF válido");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\(\d{2}\)\s\d{4,5}-\d{4}$").WithMessage("Telefone deve estar no formato (XX) XXXXX-XXXX");

        RuleFor(x => x.OwnerName)
            .NotEmpty().WithMessage("Nome do proprietário é obrigatório")
            .MaximumLength(255).WithMessage("Nome do proprietário deve ter no máximo 255 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ser válido");

        RuleFor(x => x.ZipCode)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .Matches(@"^\d{5}-\d{3}$").WithMessage("CEP deve estar no formato XXXXX-XXX");

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Rua é obrigatória")
            .MaximumLength(255).WithMessage("Rua deve ter no máximo 255 caracteres");

        RuleFor(x => x.Number)
            .NotEmpty().WithMessage("Número é obrigatório")
            .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");

        RuleFor(x => x.Complement)
            .MaximumLength(255).WithMessage("Complemento deve ter no máximo 255 caracteres");

        RuleFor(x => x.Neighborhood)
            .NotEmpty().WithMessage("Bairro é obrigatório")
            .MaximumLength(255).WithMessage("Bairro deve ter no máximo 255 caracteres");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MaximumLength(255).WithMessage("Cidade deve ter no máximo 255 caracteres");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres")
            .Must(BeValidState).WithMessage("Estado deve ser uma sigla válida");
    }

    private bool BeValidDocument(string document)
    {
        if (string.IsNullOrWhiteSpace(document))
            return false;

        var cleanDoc = document.Replace(".", "").Replace("-", "").Replace("/", "");

        return (cleanDoc.Length == 11 || cleanDoc.Length == 14) && cleanDoc.All(char.IsDigit);
    }

    private bool BeValidState(string state)
    {
        var validStates = new[] { "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO" };
        return validStates.Contains(state.ToUpper());
    }
}