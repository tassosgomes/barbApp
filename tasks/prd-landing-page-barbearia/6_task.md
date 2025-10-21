---
status: pending
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>backend/api</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>http_server</dependencies>
<unblocks>22.0</unblocks>
</task_context>

# Tarefa 6.0: API Endpoint - Landing Page Pública

## Visão Geral

Criar endpoint público (sem autenticação) para buscar dados da landing page de uma barbearia pelo código. Este endpoint será consumido pela landing page pública.

<requirements>
- GET /api/public/barbershops/{code}/landing-page - Buscar landing page pública
- Sem autenticação (endpoint público)
- Cache agressivo (5 minutos)
- Otimização de performance
- CORS configurado
- Rate limiting mais permissivo
</requirements>

## Subtarefas

- [ ] 6.1 Criar PublicLandingPageController
- [ ] 6.2 Implementar endpoint GET público
- [ ] 6.3 Configurar cache de resposta
- [ ] 6.4 Configurar CORS
- [ ] 6.5 Adicionar compressão de resposta
- [ ] 6.6 Criar testes de integração

## Detalhes de Implementação

### Controller: PublicLandingPageController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace BarbApp.API.Controllers
{
    [ApiController]
    [Route("api/public/barbershops")]
    public class PublicLandingPageController : ControllerBase
    {
        private readonly ILandingPageService _landingPageService;
        private readonly ILogger<PublicLandingPageController> _logger;
        
        public PublicLandingPageController(
            ILandingPageService landingPageService,
            ILogger<PublicLandingPageController> logger)
        {
            _landingPageService = landingPageService;
            _logger = logger;
        }
        
        /// <summary>
        /// Busca landing page pública de uma barbearia pelo código
        /// </summary>
        /// <param name="code">Código único da barbearia</param>
        /// <returns>Dados públicos da landing page</returns>
        [HttpGet("{code}/landing-page")]
        [OutputCache(Duration = 300)] // 5 minutos
        [ProducesResponseType(typeof(PublicLandingPageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPublicLandingPage(string code, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Buscando landing page pública para código: {Code}", code);
            
            var result = await _landingPageService.GetPublicByCodeAsync(code, cancellationToken);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Landing page não encontrada para código: {Code}", code);
                return NotFound(new { error = "Landing page não encontrada" });
            }
            
            return Ok(result.Data);
        }
    }
}
```

### Configuração: Program.cs

```csharp
// Adicionar Output Cache
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("PublicLandingPage", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Adicionar compressão
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

var app = builder.Build();

app.UseResponseCompression();
app.UseCors("PublicLandingPage");
app.UseOutputCache();
```

## Sequenciamento

- **Bloqueado por**: 4.0 (Serviços de Domínio)
- **Desbloqueia**: 22.0 (Types e Hooks do Public)
- **Paralelizável**: Sim (com tarefa 5.0)

## Critérios de Sucesso

- [ ] Endpoint funcionando e retornando dados corretos
- [ ] Cache configurado e funcionando
- [ ] CORS permitindo acesso de qualquer origem
- [ ] Compressão de resposta ativa
- [ ] Performance < 50ms (com cache)
- [ ] Testes de integração passando
