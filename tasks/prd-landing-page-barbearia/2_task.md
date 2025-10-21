---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/domain</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>3.0</unblocks>
</task_context>

# Tarefa 2.0: Entities e DTOs do Domínio Landing Page

## Visão Geral

Criar as entidades de domínio, DTOs de request/response e modelos de dados necessários para representar o sistema de landing pages na camada de domínio do backend.

<requirements>
- Entity `LandingPageConfig` mapeando tabela do banco
- Entity `LandingPageService` para relação com serviços
- DTOs de Request: `CreateLandingPageRequest`, `UpdateLandingPageRequest`
- DTOs de Response: `LandingPageConfigResponse`, `PublicLandingPageResponse`
- Validações nos DTOs (DataAnnotations ou FluentValidation)
- Mapeamento entre Entities e DTOs
</requirements>

## Subtarefas

- [ ] 2.1 Criar entidade `LandingPageConfig`
- [ ] 2.2 Criar entidade `LandingPageService`
- [ ] 2.3 Criar DTOs de Request com validações
- [ ] 2.4 Criar DTOs de Response
- [ ] 2.5 Configurar AutoMapper profiles
- [ ] 2.6 Adicionar validações customizadas (WhatsApp, URLs)
- [ ] 2.7 Criar testes unitários para validações

## Detalhes de Implementação

### Entity: LandingPageConfig.cs

```csharp
namespace BarbApp.Domain.Entities
{
    public class LandingPageConfig : BaseEntity
    {
        public Guid BarbershopId { get; set; }
        public int TemplateId { get; set; } // 1-5
        public string? LogoUrl { get; set; }
        public string? AboutText { get; set; }
        public string? OpeningHours { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string WhatsappNumber { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        
        // Navigation Properties
        public virtual Barbershop Barbershop { get; set; } = null!;
        public virtual ICollection<LandingPageService> Services { get; set; } = new List<LandingPageService>();
        
        // Validation Methods
        public bool IsValidTemplate() => TemplateId >= 1 && TemplateId <= 5;
    }
}
```

### Entity: LandingPageService.cs

```csharp
namespace BarbApp.Domain.Entities
{
    public class LandingPageService : BaseEntity
    {
        public Guid LandingPageConfigId { get; set; }
        public Guid ServiceId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; } = true;
        
        // Navigation Properties
        public virtual LandingPageConfig LandingPageConfig { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}
```

### DTO: UpdateLandingPageRequest.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace BarbApp.Application.DTOs.LandingPage
{
    public class UpdateLandingPageRequest
    {
        [Range(1, 5, ErrorMessage = "Template ID deve estar entre 1 e 5")]
        public int? TemplateId { get; set; }
        
        [MaxLength(1000, ErrorMessage = "Texto 'Sobre' deve ter no máximo 1000 caracteres")]
        public string? AboutText { get; set; }
        
        [MaxLength(500, ErrorMessage = "Horário deve ter no máximo 500 caracteres")]
        public string? OpeningHours { get; set; }
        
        [Url(ErrorMessage = "URL do Instagram inválida")]
        public string? InstagramUrl { get; set; }
        
        [Url(ErrorMessage = "URL do Facebook inválida")]
        public string? FacebookUrl { get; set; }
        
        [RegularExpression(@"^\+55\d{11}$", ErrorMessage = "WhatsApp deve estar no formato +55XXXXXXXXXXX")]
        public string? WhatsappNumber { get; set; }
        
        public List<ServiceDisplayRequest>? Services { get; set; }
    }
    
    public class ServiceDisplayRequest
    {
        [Required]
        public Guid ServiceId { get; set; }
        
        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }
        
        public bool IsVisible { get; set; }
    }
}
```

### DTO: LandingPageConfigResponse.cs

```csharp
namespace BarbApp.Application.DTOs.LandingPage
{
    public class LandingPageConfigResponse
    {
        public Guid Id { get; set; }
        public Guid BarbershopId { get; set; }
        public int TemplateId { get; set; }
        public string? LogoUrl { get; set; }
        public string? AboutText { get; set; }
        public string? OpeningHours { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string WhatsappNumber { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public BarbershopBasicInfo? Barbershop { get; set; }
        public List<LandingPageServiceResponse> Services { get; set; } = new();
    }
    
    public class LandingPageServiceResponse
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
    }
    
    public class BarbershopBasicInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
```

### DTO: PublicLandingPageResponse.cs

```csharp
namespace BarbApp.Application.DTOs.LandingPage
{
    public class PublicLandingPageResponse
    {
        public BarbershopPublicInfo Barbershop { get; set; } = null!;
        public LandingPagePublicInfo LandingPage { get; set; } = null!;
    }
    
    public class BarbershopPublicInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
    
    public class LandingPagePublicInfo
    {
        public int TemplateId { get; set; }
        public string? LogoUrl { get; set; }
        public string? AboutText { get; set; }
        public string? OpeningHours { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public string WhatsappNumber { get; set; } = string.Empty;
        public List<PublicServiceInfo> Services { get; set; } = new();
    }
    
    public class PublicServiceInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
    }
}
```

### AutoMapper Profile: LandingPageProfile.cs

```csharp
using AutoMapper;

namespace BarbApp.Application.Mappings
{
    public class LandingPageProfile : Profile
    {
        public LandingPageProfile()
        {
            CreateMap<LandingPageConfig, LandingPageConfigResponse>()
                .ForMember(dest => dest.Barbershop, opt => opt.MapFrom(src => src.Barbershop))
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));
            
            CreateMap<LandingPageService, LandingPageServiceResponse>()
                .ForMember(dest => dest.ServiceId, opt => opt.MapFrom(src => src.ServiceId))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Service.Duration))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Service.Price));
            
            CreateMap<Barbershop, BarbershopBasicInfo>();
            CreateMap<Barbershop, BarbershopPublicInfo>();
            
            // Public mapping
            CreateMap<LandingPageConfig, PublicLandingPageResponse>()
                .ForMember(dest => dest.Barbershop, opt => opt.MapFrom(src => src.Barbershop))
                .ForMember(dest => dest.LandingPage, opt => opt.MapFrom(src => src));
            
            CreateMap<LandingPageConfig, LandingPagePublicInfo>()
                .ForMember(dest => dest.Services, opt => opt.MapFrom(src => 
                    src.Services.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder)));
            
            CreateMap<LandingPageService, PublicServiceInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ServiceId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Service.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Service.Description))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Service.Duration))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Service.Price));
        }
    }
}
```

## Sequenciamento

- **Bloqueado por**: 1.0 (Banco de Dados)
- **Desbloqueia**: 3.0 (Repositórios)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Todas as entidades criadas e configuradas
- [ ] DTOs com validações funcionando
- [ ] AutoMapper configurado e testado
- [ ] Validações customizadas implementadas
- [ ] Testes unitários de validação passando
- [ ] Documentação XML nos tipos públicos
- [ ] Code review aprovado
