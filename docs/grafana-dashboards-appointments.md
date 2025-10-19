# Grafana Dashboards - Sistema de Agendamentos

## Visão Geral
Este documento descreve os painéis básicos do Grafana para monitoramento do sistema de agendamentos de barbeiros, incluindo métricas de mudanças de status e performance de carregamento de agenda.

## Métricas Disponíveis

### 1. Contador de Mudanças de Status
**Nome da Métrica:** `barbapp_appointments_status_changed_total`

**Descrição:** Contador total de mudanças de status de agendamentos.

**Labels:**
- `barbearia_id`: ID da barbearia
- `status`: Status para o qual o agendamento foi alterado (Confirmed, Cancelled, Completed)

**Tipo:** Counter

### 2. Histograma de Duração de Carregamento de Agenda
**Nome da Métrica:** `barbapp_schedule_load_duration_seconds`

**Descrição:** Histograma da duração de operações de carregamento de agenda do barbeiro.

**Labels:**
- `barbearia_id`: ID da barbearia

**Tipo:** Histogram

## Painéis Recomendados

### Painel 1: Visão Geral de Agendamentos

#### Panel 1.1: Total de Mudanças de Status (por Status)
```promql
# Query para visualizar total de mudanças por status
sum(barbapp_appointments_status_changed_total) by (status)
```

**Tipo de Visualização:** Graph/Time Series

**Descrição:** Mostra a evolução do número total de confirmações, cancelamentos e conclusões de agendamentos ao longo do tempo.

#### Panel 1.2: Taxa de Mudanças de Status (últimas 5 minutos)
```promql
# Taxa de mudanças de status por minuto
rate(barbapp_appointments_status_changed_total[5m]) * 60
```

**Tipo de Visualização:** Graph/Time Series

**Descrição:** Taxa de mudanças de status por minuto, útil para identificar picos de atividade.

#### Panel 1.3: Distribuição de Status por Barbearia
```promql
# Total de mudanças por barbearia e status
sum(barbapp_appointments_status_changed_total) by (barbearia_id, status)
```

**Tipo de Visualização:** Bar Chart

**Descrição:** Permite comparar a atividade de diferentes barbearias.

### Painel 2: Performance de Carregamento de Agenda

#### Panel 2.1: Tempo Médio de Carregamento
```promql
# Tempo médio de carregamento (últimos 5 minutos)
rate(barbapp_schedule_load_duration_seconds_sum[5m]) / rate(barbapp_schedule_load_duration_seconds_count[5m])
```

**Tipo de Visualização:** Gauge

**Descrição:** Tempo médio de carregamento de agenda em segundos.

**Thresholds:**
- Verde: < 1 segundo
- Amarelo: 1-2 segundos
- Vermelho: > 2 segundos

#### Panel 2.2: Distribuição de Latência (Percentis)
```promql
# P50, P90, P95, P99
histogram_quantile(0.50, rate(barbapp_schedule_load_duration_seconds_bucket[5m]))
histogram_quantile(0.90, rate(barbapp_schedule_load_duration_seconds_bucket[5m]))
histogram_quantile(0.95, rate(barbapp_schedule_load_duration_seconds_bucket[5m]))
histogram_quantile(0.99, rate(barbapp_schedule_load_duration_seconds_bucket[5m]))
```

**Tipo de Visualização:** Graph/Time Series

**Descrição:** Mostra os percentis de latência do carregamento de agenda, permitindo identificar degradação de performance.

#### Panel 2.3: Taxa de Carregamentos
```promql
# Número de carregamentos por minuto
rate(barbapp_schedule_load_duration_seconds_count[5m]) * 60
```

**Tipo de Visualização:** Graph/Time Series

**Descrição:** Quantidade de carregamentos de agenda por minuto.

#### Panel 2.4: Latência por Barbearia
```promql
# Tempo médio de carregamento por barbearia
rate(barbapp_schedule_load_duration_seconds_sum[5m]) / rate(barbapp_schedule_load_duration_seconds_count[5m]) by (barbearia_id)
```

**Tipo de Visualização:** Bar Chart

**Descrição:** Permite identificar barbearias com performance degradada.

### Painel 3: Alertas e SLOs

#### Panel 3.1: SLO - Carregamento < 2s (95% das requisições)
```promql
# Percentual de requisições abaixo de 2 segundos
sum(rate(barbapp_schedule_load_duration_seconds_bucket{le="2"}[5m])) / sum(rate(barbapp_schedule_load_duration_seconds_count[5m]))
```

**Tipo de Visualização:** Gauge

**Descrição:** Indica se o SLO de 95% das requisições em menos de 2 segundos está sendo cumprido.

**Thresholds:**
- Verde: > 0.95
- Amarelo: 0.90-0.95
- Vermelho: < 0.90

## Alertas Sugeridos

### Alerta 1: Latência Alta no Carregamento de Agenda
```yaml
alert: HighScheduleLoadLatency
expr: histogram_quantile(0.95, rate(barbapp_schedule_load_duration_seconds_bucket[5m])) > 2
for: 5m
labels:
  severity: warning
annotations:
  summary: "Latência alta no carregamento de agenda"
  description: "95% das requisições de carregamento de agenda estão demorando mais de 2 segundos."
```

### Alerta 2: Taxa Anormalmente Alta de Cancelamentos
```yaml
alert: HighCancellationRate
expr: rate(barbapp_appointments_status_changed_total{status="Cancelled"}[15m]) > 0.5
for: 10m
labels:
  severity: warning
annotations:
  summary: "Taxa alta de cancelamentos"
  description: "Taxa de cancelamentos acima de 0.5 por segundo nos últimos 15 minutos."
```

### Alerta 3: Falha em Carregamento de Agenda
```yaml
alert: ScheduleLoadFailure
expr: rate(barbapp_schedule_load_duration_seconds_count[5m]) == 0 and time() - timestamp(barbapp_schedule_load_duration_seconds_count) < 300
for: 5m
labels:
  severity: critical
annotations:
  summary: "Falha no carregamento de agenda"
  description: "Nenhum carregamento de agenda nos últimos 5 minutos, mas deveria haver atividade."
```

## Configuração do Prometheus

Certifique-se de que o Prometheus está configurado para coletar as métricas do endpoint:

```yaml
scrape_configs:
  - job_name: 'barbapp'
    static_configs:
      - targets: ['localhost:5000']
    metrics_path: '/metrics'
    scrape_interval: 15s
```

## Exemplo de Dashboard JSON

Um exemplo completo de dashboard Grafana pode ser importado através do arquivo JSON:

```json
{
  "dashboard": {
    "title": "BarbApp - Agendamentos",
    "panels": [
      {
        "title": "Total de Mudanças de Status",
        "targets": [
          {
            "expr": "sum(barbapp_appointments_status_changed_total) by (status)"
          }
        ],
        "type": "graph"
      },
      {
        "title": "Tempo Médio de Carregamento",
        "targets": [
          {
            "expr": "rate(barbapp_schedule_load_duration_seconds_sum[5m]) / rate(barbapp_schedule_load_duration_seconds_count[5m])"
          }
        ],
        "type": "gauge"
      }
    ]
  }
}
```

## Acesso ao Grafana

- **URL:** http://localhost:3000 (ambiente local)
- **Dashboard:** BarbApp - Sistema de Agendamentos
- **Refresh Interval:** 30s (recomendado)

## Manutenção

1. Revisar dashboards mensalmente para garantir que refletem as necessidades atuais
2. Ajustar thresholds de alertas baseado em dados históricos
3. Adicionar novos painéis conforme novas métricas forem implementadas
4. Documentar qualquer mudança significativa neste arquivo

## Referências

- [Prometheus Query Language (PromQL)](https://prometheus.io/docs/prometheus/latest/querying/basics/)
- [Grafana Dashboard Best Practices](https://grafana.com/docs/grafana/latest/dashboards/build-dashboards/best-practices/)
- [Histogram Quantiles in Prometheus](https://prometheus.io/docs/practices/histograms/)
