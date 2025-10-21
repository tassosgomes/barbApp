using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class UpdateLandingPageInputValidator : AbstractValidator<UpdateLandingPageInput>
{
    public UpdateLandingPageInputValidator()
    {
        RuleFor(x => x.TemplateId)
            .InclusiveBetween(1, 5).WithMessage("Template deve estar entre 1 e 5")
            .When(x => x.TemplateId.HasValue);
        
        RuleFor(x => x.WhatsappNumber)
            .NotEmpty().WithMessage("WhatsApp não pode ser vazio")
            .Matches(@"^\+55\d{11}$").WithMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX")
            .When(x => x.WhatsappNumber != null);
        
        RuleFor(x => x.LogoUrl)
            .NotEmpty().WithMessage("Logo URL não pode ser vazia")
            .MaximumLength(500).WithMessage("Logo URL deve ter no máximo 500 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Logo URL deve ser uma URL válida")
            .When(x => x.LogoUrl != null);
        
        RuleFor(x => x.AboutText)
            .NotEmpty().WithMessage("Texto 'Sobre' não pode ser vazio")
            .MaximumLength(2000).WithMessage("Texto 'Sobre' deve ter no máximo 2000 caracteres")
            .When(x => x.AboutText != null);
        
        RuleFor(x => x.OpeningHours)
            .NotEmpty().WithMessage("Horário não pode ser vazio")
            .MaximumLength(500).WithMessage("Horário deve ter no máximo 500 caracteres")
            .When(x => x.OpeningHours != null);
        
        RuleFor(x => x.InstagramUrl)
            .NotEmpty().WithMessage("Instagram URL não pode ser vazia")
            .MaximumLength(255).WithMessage("Instagram URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Instagram URL deve ser uma URL válida")
            .When(x => x.InstagramUrl != null);
        
        RuleFor(x => x.FacebookUrl)
            .NotEmpty().WithMessage("Facebook URL não pode ser vazia")
            .MaximumLength(255).WithMessage("Facebook URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Facebook URL deve ser uma URL válida")
            .When(x => x.FacebookUrl != null);
        
        RuleForEach(x => x.Services)
            .ChildRules(service => 
            {
                service.RuleFor(s => s.ServiceId).NotEmpty().WithMessage("Service ID é obrigatório");
                service.RuleFor(s => s.DisplayOrder).GreaterThanOrEqualTo(0).WithMessage("Display order deve ser maior ou igual a 0");
            })
            .When(x => x.Services != null);
    }
    
    private bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
