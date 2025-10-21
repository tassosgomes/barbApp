using BarbApp.Application.DTOs;
using FluentValidation;

namespace BarbApp.Application.Validators;

public class CreateLandingPageInputValidator : AbstractValidator<CreateLandingPageInput>
{
    public CreateLandingPageInputValidator()
    {
        RuleFor(x => x.BarbershopId)
            .NotEmpty().WithMessage("ID da barbearia é obrigatório");
        
        RuleFor(x => x.TemplateId)
            .InclusiveBetween(1, 5).WithMessage("Template deve estar entre 1 e 5");
        
        RuleFor(x => x.WhatsappNumber)
            .NotEmpty().WithMessage("WhatsApp é obrigatório")
            .Matches(@"^\+55\d{11}$").WithMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX");
        
        RuleFor(x => x.LogoUrl)
            .MaximumLength(500).WithMessage("Logo URL deve ter no máximo 500 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Logo URL deve ser uma URL válida")
            .When(x => !string.IsNullOrEmpty(x.LogoUrl));
        
        RuleFor(x => x.AboutText)
            .MaximumLength(2000).WithMessage("Texto 'Sobre' deve ter no máximo 2000 caracteres");
        
        RuleFor(x => x.OpeningHours)
            .MaximumLength(500).WithMessage("Horário deve ter no máximo 500 caracteres");
        
        RuleFor(x => x.InstagramUrl)
            .MaximumLength(255).WithMessage("Instagram URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Instagram URL deve ser uma URL válida")
            .When(x => !string.IsNullOrEmpty(x.InstagramUrl));
        
        RuleFor(x => x.FacebookUrl)
            .MaximumLength(255).WithMessage("Facebook URL deve ter no máximo 255 caracteres")
            .Must(BeValidUrlOrNull).WithMessage("Facebook URL deve ser uma URL válida")
            .When(x => !string.IsNullOrEmpty(x.FacebookUrl));
        
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
