---
status: pending
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>backend/business-logic</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>5.0, 6.0, 8.0</unblocks>
</task_context>

# Tarefa 4.0: Serviços de Domínio (Business Logic)

## Visão Geral

Implementar a camada de serviços de domínio contendo toda a lógica de negócio para gerenciamento de landing pages. Inclui criação, atualização, validações de regras de negócio e orquestração de operações complexas.

<requirements>
- Interface `ILandingPageService`
- Implementação `LandingPageService`
- Lógica de criação com validações
- Lógica de atualização de configuração
- Lógica de gerenciamento de serviços exibidos
- Validações de regras de negócio
- Tratamento de erros e exceções customizadas
- Logging de operações
</requirements>

## Subtarefas

- [ ] 4.1 Criar interface `ILandingPageService`
- [ ] 4.2 Implementar método `CreateAsync`
- [ ] 4.3 Implementar método `UpdateConfigAsync`
- [ ] 4.4 Implementar método `UpdateServicesAsync`
- [ ] 4.5 Implementar método `GetByBarbershopIdAsync`
- [ ] 4.6 Implementar método `GetPublicByCodeAsync`
- [ ] 4.7 Adicionar validações de regras de negócio
- [ ] 4.8 Implementar tratamento de erros
- [ ] 4.9 Adicionar logging estruturado
- [ ] 4.10 Criar testes unitários do serviço

## Detalhes de Implementação

### Interface: ILandingPageService.cs

```csharp
namespace BarbApp.Application.Interfaces
{
    public interface ILandingPageService
    {
        Task<Result<LandingPageConfigResponse>> CreateAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default);
        
        Task<Result<LandingPageConfigResponse>> GetByBarbershopIdAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default);
        
        Task<Result<PublicLandingPageResponse>> GetPublicByCodeAsync(
            string code, 
            CancellationToken cancellationToken = default);
        
        Task<Result> UpdateConfigAsync(
            Guid barbershopId, 
            UpdateLandingPageRequest request, 
            CancellationToken cancellationToken = default);
        
        Task<Result> UpdateServicesAsync(
            Guid barbershopId, 
            List<ServiceDisplayRequest> services, 
            CancellationToken cancellationToken = default);
        
        Task<Result<bool>> ExistsForBarbershopAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default);
    }
}
```

### Implementação: LandingPageService.cs

```csharp
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.Services
{
    public class LandingPageService : ILandingPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<LandingPageService> _logger;
        
        public LandingPageService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<LandingPageService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<Result<LandingPageConfigResponse>> CreateAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Criando landing page para barbearia {BarbershopId}", barbershopId);
                
                // Verificar se já existe
                var exists = await _unitOfWork.LandingPageConfigs
                    .ExistsForBarbershopAsync(barbershopId, cancellationToken);
                
                if (exists)
                {
                    _logger.LogWarning("Landing page já existe para barbearia {BarbershopId}", barbershopId);
                    return Result<LandingPageConfigResponse>.Failure("Landing page já existe para esta barbearia");
                }
                
                // Buscar barbearia
                var barbershop = await _unitOfWork.Barbershops
                    .GetByIdAsync(barbershopId, cancellationToken);
                
                if (barbershop == null)
                {
                    return Result<LandingPageConfigResponse>.Failure("Barbearia não encontrada");
                }
                
                // Criar configuração padrão
                var config = new LandingPageConfig
                {
                    BarbershopId = barbershopId,
                    TemplateId = 1, // Template Clássico por padrão
                    WhatsappNumber = barbershop.PhoneNumber ?? string.Empty,
                    IsPublished = true,
                    AboutText = null,
                    OpeningHours = null
                };
                
                await _unitOfWork.LandingPageConfigs.AddAsync(config, cancellationToken);
                
                // Adicionar todos os serviços da barbearia como visíveis
                var barbershopServices = await _unitOfWork.Services
                    .GetByBarbershopIdAsync(barbershopId, cancellationToken);
                
                int displayOrder = 1;
                foreach (var service in barbershopServices)
                {
                    var landingPageService = new LandingPageService
                    {
                        LandingPageConfigId = config.Id,
                        ServiceId = service.Id,
                        DisplayOrder = displayOrder++,
                        IsVisible = true
                    };
                    
                    await _unitOfWork.LandingPageServices.AddAsync(landingPageService, cancellationToken);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation(
                    "Landing page criada com sucesso para barbearia {BarbershopId}. ID: {ConfigId}",
                    barbershopId, 
                    config.Id);
                
                // Buscar configuração completa
                var result = await GetByBarbershopIdAsync(barbershopId, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar landing page para barbearia {BarbershopId}", barbershopId);
                return Result<LandingPageConfigResponse>.Failure("Erro ao criar landing page");
            }
        }
        
        public async Task<Result<LandingPageConfigResponse>> GetByBarbershopIdAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _unitOfWork.LandingPageConfigs
                    .GetByBarbershopIdAsync(barbershopId, cancellationToken);
                
                if (config == null)
                {
                    return Result<LandingPageConfigResponse>.Failure("Landing page não encontrada");
                }
                
                var response = _mapper.Map<LandingPageConfigResponse>(config);
                return Result<LandingPageConfigResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar landing page da barbearia {BarbershopId}", barbershopId);
                return Result<LandingPageConfigResponse>.Failure("Erro ao buscar landing page");
            }
        }
        
        public async Task<Result<PublicLandingPageResponse>> GetPublicByCodeAsync(
            string code, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _unitOfWork.LandingPageConfigs
                    .GetPublicByCodeAsync(code, cancellationToken);
                
                if (config == null)
                {
                    return Result<PublicLandingPageResponse>.Failure("Landing page não encontrada");
                }
                
                var response = _mapper.Map<PublicLandingPageResponse>(config);
                return Result<PublicLandingPageResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar landing page pública. Code: {Code}", code);
                return Result<PublicLandingPageResponse>.Failure("Erro ao buscar landing page");
            }
        }
        
        public async Task<Result> UpdateConfigAsync(
            Guid barbershopId, 
            UpdateLandingPageRequest request, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Atualizando configuração da landing page. Barbearia: {BarbershopId}", barbershopId);
                
                var config = await _unitOfWork.LandingPageConfigs
                    .GetByBarbershopIdAsync(barbershopId, cancellationToken);
                
                if (config == null)
                {
                    return Result.Failure("Landing page não encontrada");
                }
                
                // Atualizar campos
                if (request.TemplateId.HasValue)
                {
                    if (request.TemplateId < 1 || request.TemplateId > 5)
                    {
                        return Result.Failure("Template ID inválido");
                    }
                    config.TemplateId = request.TemplateId.Value;
                }
                
                if (request.AboutText != null)
                    config.AboutText = request.AboutText;
                
                if (request.OpeningHours != null)
                    config.OpeningHours = request.OpeningHours;
                
                if (request.InstagramUrl != null)
                    config.InstagramUrl = request.InstagramUrl;
                
                if (request.FacebookUrl != null)
                    config.FacebookUrl = request.FacebookUrl;
                
                if (request.WhatsappNumber != null)
                    config.WhatsappNumber = request.WhatsappNumber;
                
                config.UpdatedAt = DateTime.UtcNow;
                
                _unitOfWork.LandingPageConfigs.Update(config);
                
                // Atualizar serviços se fornecidos
                if (request.Services != null && request.Services.Any())
                {
                    var servicesResult = await UpdateServicesAsync(barbershopId, request.Services, cancellationToken);
                    if (!servicesResult.IsSuccess)
                    {
                        return servicesResult;
                    }
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Landing page atualizada com sucesso. Barbearia: {BarbershopId}", barbershopId);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar landing page. Barbearia: {BarbershopId}", barbershopId);
                return Result.Failure("Erro ao atualizar landing page");
            }
        }
        
        public async Task<Result> UpdateServicesAsync(
            Guid barbershopId, 
            List<ServiceDisplayRequest> services, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var config = await _unitOfWork.LandingPageConfigs
                    .GetByBarbershopIdAsync(barbershopId, cancellationToken);
                
                if (config == null)
                {
                    return Result.Failure("Landing page não encontrada");
                }
                
                // Validar que ao menos 1 serviço está visível
                if (!services.Any(s => s.IsVisible))
                {
                    return Result.Failure("Ao menos um serviço deve estar visível");
                }
                
                // Remover todos os serviços atuais
                await _unitOfWork.LandingPageServices.DeleteByLandingPageIdAsync(config.Id, cancellationToken);
                
                // Adicionar novos serviços
                foreach (var serviceRequest in services)
                {
                    var landingPageService = new LandingPageService
                    {
                        LandingPageConfigId = config.Id,
                        ServiceId = serviceRequest.ServiceId,
                        DisplayOrder = serviceRequest.DisplayOrder,
                        IsVisible = serviceRequest.IsVisible
                    };
                    
                    await _unitOfWork.LandingPageServices.AddAsync(landingPageService, cancellationToken);
                }
                
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar serviços da landing page. Barbearia: {BarbershopId}", barbershopId);
                return Result.Failure("Erro ao atualizar serviços");
            }
        }
        
        public async Task<Result<bool>> ExistsForBarbershopAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            try
            {
                var exists = await _unitOfWork.LandingPageConfigs
                    .ExistsForBarbershopAsync(barbershopId, cancellationToken);
                
                return Result<bool>.Success(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de landing page. Barbearia: {BarbershopId}", barbershopId);
                return Result<bool>.Failure("Erro ao verificar landing page");
            }
        }
    }
}
```

## Sequenciamento

- **Bloqueado por**: 3.0 (Repositórios)
- **Desbloqueia**: 5.0 (Endpoints Admin), 6.0 (Endpoint Público), 8.0 (Criação Automática)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Todas as operações CRUD implementadas
- [ ] Validações de regras de negócio funcionando
- [ ] Logging estruturado em todas as operações
- [ ] Tratamento de erros adequado
- [ ] Transações garantindo consistência
- [ ] Testes unitários com coverage > 85%
- [ ] Performance adequada (< 200ms por operação)
