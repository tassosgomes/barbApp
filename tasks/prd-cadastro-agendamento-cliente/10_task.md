---
status: pending
parallelizable: true
blocked_by: ["4.0", "8.0"]
---

<task_context>
<domain>backend/observability</domain>
<type>implementation</type>
<scope>monitoring</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
<unblocks>none</unblocks>
</task_context>

# Tarefa 10.0: Observabilidade - Logging Estruturado, Métricas Prometheus e Healthchecks

## Visão Geral

Implementar observabilidade completa para o módulo de Cadastro e Agendamento de Clientes, incluindo logging estruturado com Serilog, métricas de negócio e performance com Prometheus, e healthchecks para monitoramento de saúde da aplicação.

<requirements>
- Logging estruturado com Serilog (console + arquivo)
- Métricas Prometheus para: total de agendamentos, taxa de conflitos, cache hit rate
- Healthchecks para: database, cache, endpoints críticos
- Correlation ID para rastreamento de requisições
- Logs de auditoria para operações críticas
- Dashboard Grafana (opcional, mas recomendado)
- Alertas para: alta taxa de conflitos, database down, alta latência
</requirements>

## Subtarefas

- [ ] 10.1 Configurar Serilog com sinks (console, arquivo, seq opcional)
- [ ] 10.2 Adicionar Correlation ID middleware
- [ ] 10.3 Implementar logs estruturados em todos os use cases
- [ ] 10.4 Implementar logs de auditoria para operações críticas
- [ ] 10.5 Configurar prometheus-net para métricas
- [ ] 10.6 Criar métricas de negócio: counter de agendamentos, gauge de cache hit rate
- [ ] 10.7 Criar métricas de performance: histograma de latência de endpoints
- [ ] 10.8 Implementar healthchecks para PostgreSQL
- [ ] 10.9 Implementar healthchecks para IMemoryCache
- [ ] 10.10 Criar endpoint /health e /health/ready
- [ ] 10.11 Criar dashboard Grafana básico (arquivo JSON)
- [ ] 10.12 Documentar métricas e logs no README
- [ ] 10.13 Configurar alertas básicos (regras Prometheus)

## Detalhes de Implementação

### Configuração Serilog (Program.cs)

```csharp
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BarbApp.Cliente")
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/barbapp-cliente-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {CorrelationId} {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

// ... resto da configuração
```

### Correlation ID Middleware

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault() 
                          ?? Guid.NewGuid().ToString();

        context.Items[CorrelationIdHeader] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// No pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
```

### Logs Estruturados (Exemplo em Use Case)

```csharp
public class CriarAgendamentoUseCase
{
    private readonly ILogger<CriarAgendamentoUseCase> _logger;

    public async Task<AgendamentoOutput> Handle(...)
    {
        _logger.LogInformation(
            "Iniciando criação de agendamento. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}, BarbeiroId: {BarbeiroId}, DataHora: {DataHora}",
            clienteId, barbeariaId, input.BarbeiroId, input.DataHora);

        try
        {
            // ... lógica

            _logger.LogInformation(
                "Agendamento {AgendamentoId} criado com sucesso. Cliente: {ClienteId}, Barbeiro: {BarbeiroId}, Duração: {Duracao}min",
                agendamento.AgendamentoId, clienteId, input.BarbeiroId, duracaoTotal);

            return output;
        }
        catch (HorarioIndisponivelException ex)
        {
            _logger.LogWarning(
                "Conflito de horário detectado. BarbeiroId: {BarbeiroId}, DataHora: {DataHora}",
                input.BarbeiroId, input.DataHora);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro ao criar agendamento. ClienteId: {ClienteId}, BarbeariaId: {BarbeariaId}",
                clienteId, barbeariaId);
            throw;
        }
    }
}
```

### Métricas Prometheus

```csharp
using Prometheus;

public static class MetricsCollector
{
    // Counters
    public static readonly Counter AgendamentosCriados = Metrics.CreateCounter(
        "barbapp_agendamentos_criados_total",
        "Total de agendamentos criados",
        new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "status" }
        });

    public static readonly Counter AgendamentosConflito = Metrics.CreateCounter(
        "barbapp_agendamentos_conflito_total",
        "Total de tentativas de agendamento com conflito de horário",
        new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    public static readonly Counter AgendamentosCancelados = Metrics.CreateCounter(
        "barbapp_agendamentos_cancelados_total",
        "Total de agendamentos cancelados",
        new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    // Gauges
    public static readonly Gauge CacheHitRate = Metrics.CreateGauge(
        "barbapp_cache_hit_rate",
        "Taxa de acerto do cache de disponibilidade (0-1)",
        new GaugeConfiguration
        {
            LabelNames = new[] { "cache_type" }
        });

    // Histograms
    public static readonly Histogram LatenciaAgendamento = Metrics.CreateHistogram(
        "barbapp_agendamento_duracao_segundos",
        "Duração da operação de criar agendamento",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) // 10ms a 10s
        });
}

// Uso em Use Case
public async Task<AgendamentoOutput> Handle(...)
{
    using (MetricsCollector.LatenciaAgendamento.NewTimer())
    {
        try
        {
            // ... lógica de criação
            
            MetricsCollector.AgendamentosCriados.WithLabels(barbeariaId.ToString(), "sucesso").Inc();
            return output;
        }
        catch (HorarioIndisponivelException)
        {
            MetricsCollector.AgendamentosConflito.WithLabels(barbeariaId.ToString()).Inc();
            throw;
        }
    }
}
```

### Cache Metrics (DisponibilidadeCache)

```csharp
public class DisponibilidadeCache : IDisponibilidadeCache
{
    private long _totalRequests = 0;
    private long _cacheHits = 0;

    public Task<DisponibilidadeOutput?> GetAsync(...)
    {
        Interlocked.Increment(ref _totalRequests);
        
        _cache.TryGetValue(key, out DisponibilidadeOutput? disponibilidade);
        
        if (disponibilidade != null)
        {
            Interlocked.Increment(ref _cacheHits);
        }

        // Atualizar métrica a cada 100 requisições
        if (_totalRequests % 100 == 0)
        {
            var hitRate = _totalRequests > 0 ? (double)_cacheHits / _totalRequests : 0;
            MetricsCollector.CacheHitRate.WithLabels("disponibilidade").Set(hitRate);
        }

        return Task.FromResult(disponibilidade);
    }
}
```

### Healthchecks Configuration

```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: new[] { "db", "ready" })
    .AddCheck<MemoryCacheHealthCheck>("memory-cache", tags: new[] { "cache", "ready" })
    .AddCheck<AgendamentosEndpointHealthCheck>("agendamentos-endpoint", tags: new[] { "api", "ready" });

// Healthcheck customizado para cache
public class MemoryCacheHealthCheck : IHealthCheck
{
    private readonly IMemoryCache _cache;

    public MemoryCacheHealthCheck(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Testar operação básica de cache
            var testKey = "healthcheck_test";
            _cache.Set(testKey, "test", TimeSpan.FromSeconds(1));
            _cache.TryGetValue(testKey, out _);
            
            return Task.FromResult(HealthCheckResult.Healthy("Cache está funcionando"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("Cache com problema", ex));
        }
    }
}

// Endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapMetrics(); // Prometheus endpoint /metrics
```

### Dashboard Grafana (grafana-dashboard.json)

```json
{
  "dashboard": {
    "title": "BarbApp - Cadastro e Agendamento Cliente",
    "panels": [
      {
        "title": "Agendamentos Criados (Total)",
        "targets": [
          {
            "expr": "sum(barbapp_agendamentos_criados_total)"
          }
        ]
      },
      {
        "title": "Taxa de Conflito de Horários",
        "targets": [
          {
            "expr": "sum(rate(barbapp_agendamentos_conflito_total[5m])) / sum(rate(barbapp_agendamentos_criados_total[5m]))"
          }
        ]
      },
      {
        "title": "Cache Hit Rate",
        "targets": [
          {
            "expr": "barbapp_cache_hit_rate"
          }
        ]
      },
      {
        "title": "Latência de Agendamento (p95)",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(barbapp_agendamento_duracao_segundos_bucket[5m]))"
          }
        ]
      }
    ]
  }
}
```

### Alertas Prometheus (alerts.yml)

```yaml
groups:
  - name: barbapp_cliente
    interval: 30s
    rules:
      - alert: AltaTaxaDeConflitos
        expr: sum(rate(barbapp_agendamentos_conflito_total[5m])) / sum(rate(barbapp_agendamentos_criados_total[5m])) > 0.1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Taxa de conflito de horários > 10%"
          description: "Mais de 10% dos agendamentos estão falhando por conflito de horário"

      - alert: DatabaseDown
        expr: up{job="barbapp-postgresql"} == 0
        for: 1m
        labels:
          severity: critical
        annotations:
          summary: "PostgreSQL está down"

      - alert: AltaLatenciaAgendamento
        expr: histogram_quantile(0.95, rate(barbapp_agendamento_duracao_segundos_bucket[5m])) > 2
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "Latência p95 de agendamentos > 2s"
```

## Critérios de Sucesso

- ✅ Serilog configurado com logs estruturados em todos os use cases
- ✅ Correlation ID funcionando em todas as requisições
- ✅ Métricas Prometheus expostas em /metrics
- ✅ Healthchecks funcionando em /health e /health/ready
- ✅ Cache hit rate sendo coletado e exposto
- ✅ Métricas de negócio (agendamentos, conflitos) funcionando
- ✅ Dashboard Grafana criado e testado
- ✅ Alertas Prometheus configurados
- ✅ Documentação de observabilidade criada
- ✅ Logs não contêm informações sensíveis (telefones mascarados)
