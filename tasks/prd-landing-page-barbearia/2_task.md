---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>backend/domain</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>3.0</unblocks>
</task_context>

# Tarefa 2.0: Entities, DTOs, EntityTypeConfiguration e Migration

## Visão Geral

Criar as entidades de domínio, DTOs de request/response, configurações EF Core (EntityTypeConfiguration) e gerar a migration para o sistema de landing pages. Esta tarefa segue a abordagem Code-First do EF Core utilizada em todo o projeto.

<requirements>
- Entity `LandingPageConfig` mapeando tabela do banco
- Entity `LandingPageService` para relação com serviços
- EntityTypeConfiguration para ambas as entidades (padrão EF Core)
- DTOs de Request: `CreateLandingPageRequest`, `UpdateLandingPageRequest`
- DTOs de Response: `LandingPageConfigResponse`, `PublicLandingPageResponse`
- Validações nos DTOs (DataAnnotations)
- Mapeamento entre Entities e DTOs (AutoMapper)
- Migration gerada e aplicada no banco
</requirements>

## Subtarefas

- [ ] 2.1 Criar entidade `LandingPageConfig`
- [ ] 2.2 Criar entidade `LandingPageService`
- [ ] 2.3 Criar `LandingPageConfigConfiguration` (IEntityTypeConfiguration)
- [ ] 2.4 Criar `LandingPageServiceConfiguration` (IEntityTypeConfiguration)
- [ ] 2.5 Criar DTOs de Request com validações
- [ ] 2.6 Criar DTOs de Response
- [ ] 2.7 Configurar AutoMapper profiles
- [ ] 2.8 Adicionar validações customizadas (WhatsApp, URLs)
- [ ] 2.9 Gerar migration: `dotnet ef migrations add AddLandingPageEntities`
- [ ] 2.10 Aplicar migration: `dotnet ef database update`
- [ ] 2.11 Validar estrutura no banco (tabelas, FKs, índices, constraints)
- [ ] 2.12 Criar testes unitários para validações

## Detalhes de Implementação

### 1. Entities

#### Entity: LandingPageConfig.cs

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

#### Entity: LandingPageService.cs

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

### 2. EntityTypeConfiguration (EF Core)

#### LandingPageConfigConfiguration.cs

```csharp
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class LandingPageConfigConfiguration : IEntityTypeConfiguration<LandingPageConfig>
{
    public void Configure(EntityTypeBuilder<LandingPageConfig> builder)
    {
        builder.ToTable("landing_page_configs");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("landing_page_config_id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.BarbershopId)
            .HasColumnName("barbershop_id")
            .IsRequired();

        builder.Property(l => l.TemplateId)
            .HasColumnName("template_id")
            .IsRequired();

        builder.Property(l => l.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(500);

        builder.Property(l => l.AboutText)
            .HasColumnName("about_text")
            .HasMaxLength(2000);

        builder.Property(l => l.OpeningHours)
            .HasColumnName("opening_hours")
            .HasMaxLength(500);

        builder.Property(l => l.InstagramUrl)
            .HasColumnName("instagram_url")
            .HasMaxLength(255);

        builder.Property(l => l.FacebookUrl)
            .HasColumnName("facebook_url")
            .HasMaxLength(255);

        builder.Property(l => l.WhatsappNumber)
            .HasColumnName("whatsapp_number")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(l => l.IsPublished)
            .HasColumnName("is_published")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relationships
        builder.HasOne(l => l.Barbershop)
            .WithMany()
            .HasForeignKey(l => l.BarbershopId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes and Constraints
        builder.HasIndex(l => l.BarbershopId)
            .HasDatabaseName("ix_landing_page_configs_barbershop_id");

        builder.HasIndex(l => l.IsPublished)
            .HasDatabaseName("ix_landing_page_configs_is_published");

        builder.HasIndex(l => l.BarbershopId)
            .IsUnique()
            .HasDatabaseName("uq_landing_page_configs_barbershop");
    }
}
```

#### LandingPageServiceConfiguration.cs

```csharp
using BarbApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BarbApp.Infrastructure.Persistence.Configurations;

public class LandingPageServiceConfiguration : IEntityTypeConfiguration<LandingPageService>
{
    public void Configure(EntityTypeBuilder<LandingPageService> builder)
    {
        builder.ToTable("landing_page_services");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id)
            .HasColumnName("landing_page_service_id")
            .ValueGeneratedOnAdd();

        builder.Property(l => l.LandingPageConfigId)
            .HasColumnName("landing_page_config_id")
            .IsRequired();

        builder.Property(l => l.ServiceId)
            .HasColumnName("service_id")
            .IsRequired();

        builder.Property(l => l.DisplayOrder)
            .HasColumnName("display_order")
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.IsVisible)
            .HasColumnName("is_visible")
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasOne(l => l.LandingPageConfig)
            .WithMany(lp => lp.Services)
            .HasForeignKey(l => l.LandingPageConfigId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Service)
            .WithMany()
            .HasForeignKey(l => l.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes and Constraints
        builder.HasIndex(l => l.LandingPageConfigId)
            .HasDatabaseName("ix_landing_page_services_config_id");

        builder.HasIndex(l => l.ServiceId)
            .HasDatabaseName("ix_landing_page_services_service_id");

        builder.HasIndex(l => new { l.LandingPageConfigId, l.DisplayOrder })
            .HasDatabaseName("ix_landing_page_services_config_order");

        builder.HasIndex(l => new { l.LandingPageConfigId, l.ServiceId })
            .IsUnique()
            .HasDatabaseName("uq_landing_page_services_config_service");
    }
}
```

### 3. DTOs

#### DTO: UpdateLandingPageRequest.cs

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

#### DTO: LandingPageConfigResponse.cs

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

#### DTO: PublicLandingPageResponse.cs

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

### 4. AutoMapper Profile

#### AutoMapper Profile: LandingPageProfile.cs

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

### 5. Comandos de Migration

```bash
# Navegar para o diretório do projeto
cd backend/src/BarbApp.API

# Gerar a migration
dotnet ef migrations add AddLandingPageEntities --project ../BarbApp.Infrastructure --startup-project .

# Aplicar a migration no banco
dotnet ef database update --project ../BarbApp.Infrastructure --startup-project .

# Verificar a migration gerada em:
# backend/src/BarbApp.Infrastructure/Migrations/[timestamp]_AddLandingPageEntities.cs
```

### 6. Validação no Banco de Dados

Após aplicar a migration, validar:

```sql
-- Verificar tabelas criadas
\dt landing_page_*

-- Verificar estrutura de landing_page_configs
\d landing_page_configs

-- Verificar estrutura de landing_page_services
\d landing_page_services

-- Verificar índices
\di landing_page_*

-- Verificar constraints
SELECT conname, contype 
FROM pg_constraint 
WHERE conrelid::regclass::text LIKE 'landing_page_%';
```

## Sequenciamento

- **Bloqueado por**: Nenhuma (primeira tarefa agora)
- **Desbloqueia**: 3.0 (Repositórios)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Todas as entidades criadas e configuradas
- [ ] EntityTypeConfiguration completas com todos os mapeamentos
- [ ] DTOs com validações funcionando
- [ ] AutoMapper configurado e testado
- [ ] Migration gerada com sucesso
- [ ] Migration aplicada sem erros no banco
- [ ] Todas as tabelas, FKs, índices e constraints criados corretamente
- [ ] Constraint de unicidade (1 landing page por barbearia) funcionando
- [ ] Validações customizadas implementadas
- [ ] Testes unitários de validação passando
- [ ] Documentação XML nos tipos públicos
- [ ] Code review aprovado
