import { useQuery } from '@tanstack/react-query';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { dashboardService } from '@/services/dashboard.service';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { Users, Scissors, Calendar, Clock } from 'lucide-react';
import { Alert, AlertDescription } from '@/components/ui/alert';

/**
 * Dashboard page for Admin Barbearia
 * Displays overview metrics and upcoming appointments
 */
export function Dashboard() {
  const { barbearia } = useBarbearia();

  // Fetch dashboard metrics
  const { data: metrics, isLoading, isError } = useQuery({
    queryKey: ['dashboard-metrics', barbearia?.barbeariaId],
    queryFn: () => dashboardService.getMetrics(),
    enabled: !!barbearia,
    refetchInterval: 60000, // Refetch every minute
  });

  // Loading state
  if (isLoading) {
    return (
      <div className="space-y-6">
        <div>
          <Skeleton className="h-10 w-96" />
          <Skeleton className="mt-2 h-6 w-64" />
        </div>
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {[...Array(3)].map((_, i) => (
            <Skeleton key={i} className="h-32" />
          ))}
        </div>
        <Skeleton className="h-64" />
      </div>
    );
  }

  // Error state
  if (isError) {
    return (
      <Alert variant="destructive">
        <AlertDescription>
          Erro ao carregar métricas do dashboard. Tente novamente mais tarde.
        </AlertDescription>
      </Alert>
    );
  }

  const metricCards = [
    {
      title: 'Total de Barbeiros',
      value: metrics?.totalBarbeiros ?? 0,
      icon: Users,
      description: 'Barbeiros cadastrados',
    },
    {
      title: 'Serviços Ativos',
      value: metrics?.totalServicos ?? 0,
      icon: Scissors,
      description: 'Serviços disponíveis',
    },
    {
      title: 'Agendamentos Hoje',
      value: metrics?.agendamentosHoje ?? 0,
      icon: Calendar,
      description: 'Agendamentos para hoje',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Welcome header */}
      <div>
        <h1 className="text-2xl font-bold md:text-3xl">
          Bem-vindo ao painel!
        </h1>
        <p className="mt-2 text-sm text-muted-foreground md:text-base">
          Gerencie sua barbearia de forma simples e eficiente.
        </p>
      </div>

      {/* Metrics grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {metricCards.map((metric) => {
          const Icon = metric.icon;
          return (
            <Card key={metric.title}>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  {metric.title}
                </CardTitle>
                <Icon className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">{metric.value}</div>
                <p className="text-xs text-muted-foreground">
                  {metric.description}
                </p>
              </CardContent>
            </Card>
          );
        })}
      </div>

      {/* Upcoming appointments */}
      <Card>
        <CardHeader>
          <CardTitle>Próximos Agendamentos</CardTitle>
        </CardHeader>
        <CardContent>
          {!metrics?.proximosAgendamentos || metrics.proximosAgendamentos.length === 0 ? (
            <p className="text-sm text-muted-foreground">
              Nenhum agendamento próximo para hoje.
            </p>
          ) : (
            <div className="space-y-3">
              {metrics.proximosAgendamentos.map((agendamento) => (
                <div
                  key={agendamento.id}
                  className="flex flex-col justify-between gap-2 rounded-lg border p-3 sm:flex-row sm:items-center"
                >
                  <div className="flex-1 space-y-1">
                    <p className="font-medium">{agendamento.clienteNome}</p>
                    <p className="text-sm text-muted-foreground">
                      {agendamento.barbeiro} • {agendamento.servico}
                    </p>
                  </div>
                  <div className="flex items-center gap-2 text-right text-sm">
                    <Clock className="h-4 w-4 text-muted-foreground" />
                    <div>
                      <p className="font-medium">{agendamento.hora}</p>
                      <p className="text-muted-foreground">{agendamento.data}</p>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
