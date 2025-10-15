using Prometheus;

namespace BarbApp.Application;

public static class BarbAppMetrics
{
    // Counters
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

    // Gauge for active barbers
    public static readonly Gauge ActiveBarbersGauge = Metrics
        .CreateGauge("barbapp_active_barbers", "Number of active barbers", new GaugeConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    // Histogram for schedule retrieval time
    public static readonly Histogram ScheduleRetrievalDuration = Metrics
        .CreateHistogram("barbapp_schedule_retrieval_duration_seconds", "Duration of schedule retrieval operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });
}