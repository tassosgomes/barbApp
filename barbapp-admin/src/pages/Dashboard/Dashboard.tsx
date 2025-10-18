import { useBarbearia } from '@/contexts/BarbeariaContext';
import { Users, Scissors, Calendar, TrendingUp } from 'lucide-react';

/**
 * Dashboard page for Admin Barbearia
 * Displays overview metrics and quick access to main features
 */
export function Dashboard() {
  const { barbearia } = useBarbearia();

  // Placeholder metrics - will be replaced with real data
  const metrics = [
    {
      title: 'Barbeiros Ativos',
      value: '5',
      icon: Users,
      description: 'Total de barbeiros cadastrados',
    },
    {
      title: 'Serviços',
      value: '8',
      icon: Scissors,
      description: 'Serviços disponíveis',
    },
    {
      title: 'Agendamentos Hoje',
      value: '12',
      icon: Calendar,
      description: 'Agendamentos para hoje',
    },
    {
      title: 'Taxa de Ocupação',
      value: '75%',
      icon: TrendingUp,
      description: 'Média dos últimos 7 dias',
    },
  ];

  return (
    <div className="space-y-6">
      {/* Welcome header */}
      <div>
        <h1 className="text-3xl font-bold">Bem-vindo ao {barbearia?.nome}!</h1>
        <p className="mt-2 text-gray-600">
          Gerencie sua barbearia de forma simples e eficiente.
        </p>
      </div>

      {/* Metrics grid */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {metrics.map((metric) => {
          const Icon = metric.icon;
          return (
            <div
              key={metric.title}
              className="rounded-lg border bg-white p-6 shadow-sm"
            >
              <div className="flex items-center justify-between">
                <h3 className="text-sm font-medium text-gray-600">{metric.title}</h3>
                <Icon className="h-4 w-4 text-gray-400" />
              </div>
              <div className="mt-2">
                <div className="text-2xl font-bold">{metric.value}</div>
                <p className="text-xs text-gray-500">{metric.description}</p>
              </div>
            </div>
          );
        })}
      </div>

      {/* Quick actions or upcoming appointments placeholder */}
      <div className="rounded-lg border bg-white p-6 shadow-sm">
        <h2 className="text-lg font-semibold">Próximos Agendamentos</h2>
        <div className="mt-4">
          <p className="text-sm text-gray-600">
            Esta seção exibirá os próximos agendamentos do dia.
          </p>
          <p className="mt-2 text-sm text-gray-600">
            Funcionalidade será implementada nas próximas tarefas.
          </p>
        </div>
      </div>
    </div>
  );
}
