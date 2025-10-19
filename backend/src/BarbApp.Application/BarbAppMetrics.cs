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

    // Histogram for list barbers time
    public static readonly Histogram ListBarbersDuration = Metrics
        .CreateHistogram("barbapp_list_barbers_duration_seconds", "Duration of list barbers operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });

    // Counter for appointment status changes
    public static readonly Counter AppointmentStatusChangedCounter = Metrics
        .CreateCounter("barbapp_appointments_status_changed_total", "Total number of appointment status changes", new CounterConfiguration
        {
            LabelNames = new[] { "barbearia_id", "status" }
        });

    // Histogram for schedule load duration
    public static readonly Histogram ScheduleLoadDuration = Metrics
        .CreateHistogram("barbapp_schedule_load_duration_seconds", "Duration of barber schedule loading operations", new HistogramConfiguration
        {
            LabelNames = new[] { "barbearia_id" }
        });
}