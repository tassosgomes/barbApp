# 📊 Observabilidade - BarbApp

Este documento descreve a estratégia de observabilidade implementada no módulo de Cadastro e Agendamento de Clientes do BarbApp.

## Índice

- [Visão Geral](#visão-geral)
- [Logging Estruturado](#logging-estruturado)
- [Métricas Prometheus](#métricas-prometheus)
- [Healthchecks](#healthchecks)
- [Dashboard Grafana](#dashboard-grafana)
- [Alertas](#alertas)
- [Boas Práticas](#boas-práticas)

---

## Visão Geral

A observabilidade do BarbApp é baseada em três pilares:

1. **Logs** - Serilog com logging estruturado em JSON
2. **Métricas** - Prometheus para coleta e monitoramento
3. **Traces** - Correlation ID para rastreamento de requisições

### Arquitetura

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   BarbApp   │────▶│  Prometheus │────▶│   Grafana   │
│     API     │     │   /metrics  │     │  Dashboard  │
└─────────────┘     └─────────────┘     └─────────────┘
       │
       ▼
┌─────────────┐
│   Serilog   │────▶ Console / Arquivo / Seq
│    Logs     │
└─────────────┘
```

---

## Logging Estruturado

### Configuração

O logging é configurado no `Program.cs` usando Serilog:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BarbApp.API")
    .WriteTo.Console()
    .WriteTo.File("logs/barbapp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
```

### Correlation ID

Todas as requisições recebem um Correlation ID único que é propagado através do sistema:

- **Header**: `X-Correlation-Id`
- **Geração**: Automática se não fornecido
- **Propagação**: Incluído em todos os logs via `LogContext`

Exemplo de log estruturado:
```json
{
  "Timestamp": "2025-11-24T10:30:00.000Z",
  "Level": "Information",
  "Message": "Agendamento criado com sucesso",
  "CorrelationId": "abc123def456",
  "RequestPath": "/api/agendamentos",
  "AgendamentoId": "guid-123",
  "ClienteId": "guid-456",
  "BarbeariaId": "guid-789"
}
```

### Níveis de Log

| Nível | Uso |
|-------|-----|
| `Debug` | Informações detalhadas para debugging (cache hits, queries) |
| `Information` | Operações normais (agendamento criado, login realizado) |
| `Warning` | Situações anormais mas recuperáveis (conflito de horário, login falho) |
| `Error` | Erros que afetam operação mas sistema continua funcionando |
| `Fatal` | Erros críticos que impedem funcionamento |

### Mascaramento de Dados Sensíveis (LGPD)

Dados sensíveis são mascarados antes de serem logados:

```csharp
// Telefone: 11987654321 → 11987****21
private static string MascararTelefone(string telefone)
{
    var inicio = telefone.Substring(0, 5);
    var fim = telefone.Substring(telefone.Length - 2);
    return $"{inicio}****{fim}";
}
```

---

## Métricas Prometheus

### Endpoint

As métricas estão disponíveis em: `GET /metrics`

### Métricas Disponíveis

#### Agendamentos

| Métrica | Tipo | Labels | Descrição |
|---------|------|--------|-----------|
| `barbapp_agendamentos_criados_total` | Counter | barbearia_id, status | Total de agendamentos criados |
| `barbapp_agendamentos_cancelados_total` | Counter | barbearia_id | Total de agendamentos cancelados |
| `barbapp_agendamentos_conflito_total` | Counter | barbearia_id | Tentativas com conflito de horário |
| `barbapp_agendamento_duracao_segundos` | Histogram | barbearia_id | Latência de criação de agendamentos |

#### Cache

| Métrica | Tipo | Labels | Descrição |
|---------|------|--------|-----------|
| `barbapp_disponibilidade_cache_hit_rate` | Gauge | cache_type | Taxa de acerto do cache (0-1) |
| `barbapp_cache_operations_total` | Counter | cache_type, operation | Operações de cache (hit/miss/set/invalidate) |

#### Autenticação

| Métrica | Tipo | Labels | Descrição |
|---------|------|--------|-----------|
| `barbapp_clientes_cadastrados_total` | Counter | barbearia_id | Total de clientes cadastrados |
| `barbapp_logins_clientes_total` | Counter | barbearia_id, resultado | Logins de clientes |
| `barbapp_logins_falhados_total` | Counter | barbearia_id, motivo | Tentativas de login falhadas |

#### Erros

| Métrica | Tipo | Labels | Descrição |
|---------|------|--------|-----------|
| `barbapp_erros_total` | Counter | tipo, endpoint | Total de erros por tipo |

### Exemplo de Uso em Código

```csharp
// Incrementar contador
BarbAppMetrics.AgendamentosCriadosCounter
    .WithLabels(barbeariaId.ToString(), "sucesso")
    .Inc();

// Observar latência
using (BarbAppMetrics.AgendamentoLatenciaHistogram
    .WithLabels(barbeariaId.ToString())
    .NewTimer())
{
    // operação...
}

// Atualizar gauge
BarbAppMetrics.DisponibilidadeCacheHitRate
    .WithLabels("disponibilidade")
    .Set(hitRate);
```

---

## Healthchecks

### Endpoints

| Endpoint | Descrição |
|----------|-----------|
| `GET /health` | Status geral da aplicação |
| `GET /health/ready` | Status de prontidão (ready checks) |
| `GET /health/live` | Status de vida (liveness check) |

### Checks Configurados

1. **PostgreSQL** (`postgresql`)
   - Tags: `db`, `ready`
   - Timeout: 5 segundos
   - Verifica conexão com banco de dados

2. **Memory Cache** (`memory-cache`)
   - Tags: `cache`, `ready`
   - Timeout: 2 segundos
   - Verifica operações básicas de cache (set/get/remove)

### Formato de Resposta

```json
{
  "status": "Healthy",
  "totalDuration": 45.5,
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "duration": 42.3,
      "description": null,
      "data": {}
    },
    {
      "name": "memory-cache",
      "status": "Healthy",
      "duration": 3.2,
      "description": "Cache de memória está funcionando corretamente",
      "data": {
        "operationsChecked": ["Set", "Get", "Remove"]
      }
    }
  ]
}
```

---

## Dashboard Grafana

O dashboard Grafana está disponível em: `observability/grafana-dashboard.json`

### Painéis Incluídos

1. **Visão Geral - Agendamentos**
   - Total de agendamentos criados
   - Total de agendamentos cancelados
   - Taxa de conflito de horários (gauge)
   - Cache hit rate (gauge)

2. **Métricas de Performance**
   - Latência de criação de agendamento (p50, p95, p99)
   - Agendamentos por barbearia (rate/5m)

3. **Autenticação e Clientes**
   - Total de clientes cadastrados
   - Total de logins de clientes
   - Operações de cache (hits vs misses)

4. **Erros e Alertas**
   - Erros por tipo
   - Taxa de conflito histórica

### Importação

1. Acesse Grafana → Dashboards → Import
2. Faça upload do arquivo `grafana-dashboard.json`
3. Configure o datasource Prometheus

---

## Alertas

As regras de alerta estão em: `observability/prometheus-alerts.yml`

### Alertas Configurados

#### Críticos (severity: critical)

| Alerta | Condição | Descrição |
|--------|----------|-----------|
| `LatenciaCriticaAgendamento` | p99 > 5s por 3m | Latência extremamente alta |
| `TaxaCriticaErros` | erros > 1/s por 2m | Sistema em estado degradado |
| `DatabaseDown` | banco inacessível por 1m | PostgreSQL não responde |

#### Warnings (severity: warning)

| Alerta | Condição | Descrição |
|--------|----------|-----------|
| `AltaTaxaDeConflitos` | conflitos > 10% por 5m | Muitos agendamentos falhando |
| `AltaLatenciaAgendamento` | p95 > 2s por 5m | Performance degradada |
| `BaixaTaxaHitCache` | hit rate < 50% por 10m | Cache ineficiente |
| `MuitasLoginsFalhas` | > 50 falhas em 10m | Possível ataque |

### Configuração no Prometheus

```yaml
# prometheus.yml
rule_files:
  - 'observability/prometheus-alerts.yml'

alerting:
  alertmanagers:
    - static_configs:
        - targets: ['alertmanager:9093']
```

---

## Boas Práticas

### Logging

1. **Sempre use logging estruturado** com propriedades nomeadas
   ```csharp
   // ✅ Correto
   _logger.LogInformation("Agendamento {AgendamentoId} criado", id);
   
   // ❌ Evitar
   _logger.LogInformation($"Agendamento {id} criado");
   ```

2. **Mascare dados sensíveis** (telefone, email, etc.)

3. **Use níveis apropriados**
   - `Information` para operações bem-sucedidas
   - `Warning` para falhas esperadas (validação, conflito)
   - `Error` para exceções inesperadas

4. **Inclua contexto suficiente** para debugging
   ```csharp
   _logger.LogWarning(
       "Conflito de horário. BarbeiroId: {BarbeiroId}, DataHora: {DataHora}",
       barbeiroId, dataHora);
   ```

### Métricas

1. **Use labels com cuidado** - muitos labels = alta cardinalidade

2. **Prefira contadores para taxas** - use `rate()` ou `increase()` em queries

3. **Histogramas para latência** - permitem cálculo de percentis

4. **Atualize métricas de gauge periodicamente** - não a cada requisição

### Alertas

1. **Defina for adequado** - evita alertas por picos momentâneos

2. **Inclua runbook_url** - facilita resposta a incidentes

3. **Escalone severidades** - warning → critical

4. **Agrupe alertas relacionados** - use labels comuns

---

## Manutenção

### Adicionando Novas Métricas

1. Defina a métrica em `BarbAppMetrics.cs`
2. Instrumentalize o código com a métrica
3. Atualize o dashboard Grafana
4. Adicione alertas se necessário
5. Atualize esta documentação

### Verificando Saúde do Sistema

```bash
# Health check básico
curl http://localhost:5000/health

# Ready check (dependências)
curl http://localhost:5000/health/ready

# Métricas Prometheus
curl http://localhost:5000/metrics | grep barbapp_
```

---

**Data de Criação**: 2025-11-24  
**Versão**: 1.0  
**Autor**: Task 10.0 - Observabilidade
