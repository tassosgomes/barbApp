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
            .Matches(@"^\+55\d{11}$").WithMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX")
            .When(x => !string.IsNullOrWhiteSpace(x.WhatsappNumber));
        
        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL deve ter no máximo 500 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Logo URL deve ser uma URL válida")
            .When(x => !string.IsNullOrWhiteSpace(x.LogoUrl));
        
        RuleFor(x => x.AboutText)
            .MaximumLength(2000).WithMessage("Texto 'Sobre' deve ter no máximo 2000 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.AboutText));
        
        RuleFor(x => x.OpeningHours)
            .MaximumLength(500).WithMessage("Horário deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.OpeningHours));
        
        RuleFor(x => x.InstagramUrl)
            .MaximumLength(255).WithMessage("Instagram URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Instagram URL deve ser uma URL válida")
            .When(x => !string.IsNullOrWhiteSpace(x.InstagramUrl));
        
        RuleFor(x => x.FacebookUrl)
            .MaximumLength(255).WithMessage("Facebook URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Facebook URL deve ser uma URL válida")
            .When(x => !string.IsNullOrWhiteSpace(x.FacebookUrl));
        
        RuleForEach(x => x.Services)
            .ChildRules(service => 
            {
                service.RuleFor(s => s.ServiceId).NotEmpty().WithMessage("Service ID é obrigatório");
                service.RuleFor(s => s.DisplayOrder).GreaterThanOrEqualTo(0).WithMessage("Display order deve ser maior ou igual a 0");
            })
            .When(x => x.Services != null && x.Services.Any());
    }
    
    private bool BeValidUrlOrNull(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
