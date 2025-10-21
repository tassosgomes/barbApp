---
status: pending
parallelizable: false
blocked_by: ["4.0"]
---

<task_context>
<domain>backend/integration</domain>
<type>integration</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>none</unblocks>
</task_context>

# Tarefa 8.0: Criação Automática no Cadastro da Barbearia

## Visão Geral

Integrar a criação automática de landing page quando uma nova barbearia é cadastrada. Implementar como evento/hook após criação bem-sucedida da barbearia.

<requirements>
- Criar landing page automaticamente após cadastro de barbearia
- Usar padrão de eventos ou hooks
- Execução assíncrona (não bloquear cadastro)
- Tratamento de falhas (registrar mas não falhar cadastro)
- Retry em caso de erro
- Logging completo
</requirements>

## Subtarefas

- [ ] 8.1 Criar evento `BarbershopCreatedEvent`
- [ ] 8.2 Criar handler `CreateLandingPageHandler`
- [ ] 8.3 Integrar com serviço de cadastro de barbearias
- [ ] 8.4 Implementar retry policy
- [ ] 8.5 Adicionar logging e monitoramento
- [ ] 8.6 Criar testes de integração end-to-end

## Detalhes de Implementação

### Event: BarbershopCreatedEvent.cs

```csharp
namespace BarbApp.Domain.Events
{
    public class BarbershopCreatedEvent : INotification
    {
        public Guid BarbershopId { get; set; }
        public string BarbershopName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
```

### Handler: CreateLandingPageHandler.cs

```csharp
using MediatR;
using Polly;

namespace BarbApp.Application.EventHandlers
{
    public class CreateLandingPageHandler : INotificationHandler<BarbershopCreatedEvent>
    {
        private readonly ILandingPageService _landingPageService;
        private readonly ILogger<CreateLandingPageHandler> _logger;
        
        public CreateLandingPageHandler(
            ILandingPageService landingPageService,
            ILogger<CreateLandingPageHandler> logger)
        {
            _landingPageService = landingPageService;
            _logger = logger;
        }
        
        public async Task Handle(BarbershopCreatedEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Iniciando criação automática de landing page para barbearia {BarbershopId}",
                notification.BarbershopId);
            
            // Retry policy: 3 tentativas com intervalo exponencial
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    3,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(
                            exception,
                            "Tentativa {RetryCount} de criar landing page falhou. Aguardando {TimeSpan} antes de tentar novamente",
                            retryCount,
                            timeSpan);
                    });
            
            await retryPolicy.ExecuteAsync(async () =>
            {
                var result = await _landingPageService.CreateAsync(notification.BarbershopId, cancellationToken);
                
                if (!result.IsSuccess)
                {
                    _logger.LogError(
                        "Falha ao criar landing page automaticamente para barbearia {BarbershopId}. Erro: {Error}",
                        notification.BarbershopId,
                        result.Error);
                    
                    throw new Exception($"Falha ao criar landing page: {result.Error}");
                }
                
                _logger.LogInformation(
                    "Landing page criada automaticamente com sucesso para barbearia {BarbershopId}",
                    notification.BarbershopId);
            });
        }
    }
}
```

### Integração no Serviço de Barbershop

```csharp
// BarbershopService.cs
public async Task<Result<BarbershopResponse>> CreateAsync(
    CreateBarbershopRequest request,
    CancellationToken cancellationToken = default)
{
    try
    {
        // ... lógica de criação da barbearia ...
        
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        
        // Publicar evento
        await _mediator.Publish(new BarbershopCreatedEvent
        {
            BarbershopId = barbershop.Id,
            BarbershopName = barbershop.Name,
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);
        
        return Result<BarbershopResponse>.Success(response);
    }
    catch (Exception ex)
    {
        // ... tratamento de erro ...
    }
}
```

## Sequenciamento

- **Bloqueado por**: 4.0 (Serviços de Domínio)
- **Desbloqueia**: Nenhuma (feature completa)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Landing page criada automaticamente em 100% dos cadastros
- [ ] Falhas não bloqueiam cadastro de barbearia
- [ ] Retry funcionando corretamente
- [ ] Logging completo de eventos e erros
- [ ] Testes E2E validando fluxo completo
- [ ] Monitoramento de taxa de sucesso
