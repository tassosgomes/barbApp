using Prometheus;

namespace BarbApp.Application;

/// <summary>
/// Coletor centralizado de métricas Prometheus para o BarbApp.
/// Todas as métricas seguem a convenção barbapp_{categoria}_{nome}_{unidade}_total|bucket|rate
/// </summary>
public static class BarbAppMetrics
{
    // ═══════════════════════════════════════════════════════════════
    // MÉTRICAS DE BARBEIROS
    // ═══════════════════════════════════════════════════════════════
    
    public static readonly Counter BarberCreatedCounter = Metrics
        .CreateCounter("barbapp_barber_created_total", "Total number of barbers created", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    public static readonly Counter BarberRemovedCounter = Metrics
        .CreateCounter("barbapp_barber_removed_total", "Total number of barbers removed", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    public static readonly Gauge ActiveBarbersGauge = Metrics
        .CreateGauge("barbapp_active_barbers", "Number of active barbers", new GaugeConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    public static readonly Histogram ScheduleRetrievalDuration = Metrics
        .CreateHistogram("barbapp_schedule_retrieval_duration_seconds", "Duration of schedule retrieval operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    public static readonly Histogram ListBarbersDuration = Metrics
        .CreateHistogram("barbapp_list_barbers_duration_seconds", "Duration of list barbers operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    // ═══════════════════════════════════════════════════════════════
    // MÉTRICAS DE AGENDAMENTOS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Contador de agendamentos criados (sucesso ou com status específico)
    /// </summary>
    public static readonly Counter AgendamentosCriadosCounter = Metrics
        .CreateCounter("barbapp_agendamentos_criados_total", "Total de agendamentos criados", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "status" }
        });

    /// <summary>
    /// Contador de conflitos de horário detectados durante criação de agendamento
    /// </summary>
    public static readonly Counter AgendamentosConflitoCounter = Metrics
        .CreateCounter("barbapp_agendamentos_conflito_total", "Total de tentativas de agendamento com conflito de horário", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    /// <summary>
    /// Contador de agendamentos cancelados
    /// </summary>
    public static readonly Counter AgendamentosCanceladosCounter = Metrics
        .CreateCounter("barbapp_agendamentos_cancelados_total", "Total de agendamentos cancelados", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    /// <summary>
    /// Histograma de latência para criação de agendamentos
    /// </summary>
    public static readonly Histogram AgendamentoLatenciaHistogram = Metrics
        .CreateHistogram("barbapp_agendamento_duracao_segundos", "Duração da operação de criar agendamento", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" },
            Buckets = new[] { 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0 }
        });

    public static readonly Counter AppointmentStatusChangedCounter = Metrics
        .CreateCounter("barbapp_appointments_status_changed_total", "Total number of appointment status changes", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "status" }
        });

    public static readonly Histogram ScheduleLoadDuration = Metrics
        .CreateHistogram("barbapp_schedule_load_duration_seconds", "Duration of barber schedule loading operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    // ═══════════════════════════════════════════════════════════════
    // MÉTRICAS DE DISPONIBILIDADE E CACHE
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Contador de consultas de disponibilidade (cache hit/miss)
    /// </summary>
    public static readonly Counter DisponibilidadeConsultasCounter = Metrics
        .CreateCounter("barbapp_disponibilidade_consultas_total", "Total number of availability consultations", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "cached" }
        });

    /// <summary>
    /// Histograma de tempo de cálculo de disponibilidade
    /// </summary>
    public static readonly Histogram DisponibilidadeCalculoTempo = Metrics
        .CreateHistogram("barbapp_disponibilidade_calculo_tempo_ms", "Time taken to calculate availability", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" },
            Buckets = new[] { 10.0, 25.0, 50.0, 100.0, 250.0, 500.0, 1000.0, 2500.0, 5000.0 }
        });

    /// <summary>
    /// Taxa de acerto do cache de disponibilidade (0-1)
    /// </summary>
    public static readonly Gauge DisponibilidadeCacheHitRate = Metrics
        .CreateGauge("barbapp_disponibilidade_cache_hit_rate", "Cache hit rate for availability consultations", new GaugeConfiguration
        {
            LabelNames = new[] { "cache_type" }
        });

    /// <summary>
    /// Contador de operações de cache (hit, miss, invalidate)
    /// </summary>
    public static readonly Counter CacheOperationsCounter = Metrics
        .CreateCounter("barbapp_cache_operations_total", "Total de operações de cache", new CounterConfiguration
        {
            LabelNames = new[] { "cache_type", "operation" }
        });

    // ═══════════════════════════════════════════════════════════════
    // MÉTRICAS DE CLIENTES E AUTENTICAÇÃO
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Contador de clientes cadastrados
    /// </summary>
    public static readonly Counter ClientesCadastradosCounter = Metrics
        .CreateCounter("barbapp_clientes_cadastrados_total", "Total de clientes cadastrados", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    /// <summary>
    /// Contador de logins de clientes (sucesso ou falha)
    /// </summary>
    public static readonly Counter LoginsClientesCounter = Metrics
        .CreateCounter("barbapp_logins_clientes_total", "Total de logins de clientes", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "resultado" }
        });

    /// <summary>
    /// Contador de tentativas de login falhadas (para rate limiting e segurança)
    /// </summary>
    public static readonly Counter LoginsFalhadosCounter = Metrics
        .CreateCounter("barbapp_logins_falhados_total", "Total de tentativas de login falhadas", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "motivo" }
        });

    // ═══════════════════════════════════════════════════════════════
    // MÉTRICAS DE PERFORMANCE DE ENDPOINTS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Histograma de latência por endpoint
    /// </summary>
    public static readonly Histogram EndpointLatenciaHistogram = Metrics
        .CreateHistogram("barbapp_endpoint_latencia_segundos", "Latência de endpoints por operação", new HistogramConfiguration
        {
            LabelNames = new[] { "endpoint", "method", "status_code" },
            Buckets = new[] { 0.01, 0.025, 0.05, 0.1, 0.25, 0.5, 1.0, 2.5, 5.0, 10.0 }
        });

    /// <summary>
    /// Contador de erros por tipo
    /// </summary>
    public static readonly Counter ErrosCounter = Metrics
        .CreateCounter("barbapp_erros_total", "Total de erros por tipo", new CounterConfiguration
        {
            LabelNames = new[] { "tipo", "endpoint" }
        });
}