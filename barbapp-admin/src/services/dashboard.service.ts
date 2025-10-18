import api from './api';

/**
 * Dashboard metrics interface
 */
export interface DashboardMetrics {
  totalBarbeiros: number;
  totalServicos: number;
  agendamentosHoje: number;
  proximosAgendamentos: ProximoAgendamento[];
}

/**
 * Upcoming appointment interface
 */
export interface ProximoAgendamento {
  id: string;
  clienteNome: string;
  barbeiro: string;
  servico: string;
  data: string;
  hora: string;
  status: string;
}

/**
 * Dashboard service for fetching metrics and aggregated data
 */
export const dashboardService = {
  /**
   * Get dashboard metrics for Admin Barbearia
   * Aggregates data from multiple endpoints
   * @returns Dashboard metrics with totals and upcoming appointments
   */
  getMetrics: async (): Promise<DashboardMetrics> => {
    // Fetch data in parallel for better performance
    const [barbeirosResponse, servicosResponse, agendamentosResponse] = await Promise.all([
      // Get total barbers count
      api.get('/barbeiros', { params: { pageSize: 1, page: 1 } }),
      // Get total services count
      api.get('/servicos', { params: { pageSize: 1, page: 1 } }),
      // Get today's appointments (first 10 for upcoming list)
      api.get('/agendamentos', {
        params: {
          pageSize: 10,
          page: 1,
          // Note: Backend should support date filtering
          // If not available, we'll filter on frontend
        },
      }),
    ]);

    // Extract totals from pagination metadata
    const totalBarbeiros = barbeirosResponse.data.totalCount || 0;
    const totalServicos = servicosResponse.data.totalCount || 0;

    // Process appointments
    const agendamentos = agendamentosResponse.data.items || [];
    const hoje = new Date().toISOString().split('T')[0];

    // Filter appointments for today
    const agendamentosHoje = agendamentos.filter((agendamento: any) => {
      const agendamentoData = agendamento.dataHora?.split('T')[0];
      return agendamentoData === hoje;
    });

    // Map to ProximoAgendamento format
    const proximosAgendamentos: ProximoAgendamento[] = agendamentosHoje.slice(0, 5).map((agendamento: any) => {
      const dataHora = new Date(agendamento.dataHora);
      return {
        id: agendamento.id,
        clienteNome: agendamento.clienteNome || 'Cliente não informado',
        barbeiro: agendamento.barbeiroNome || 'Barbeiro não informado',
        servico: agendamento.servicoNome || 'Serviço não informado',
        data: dataHora.toLocaleDateString('pt-BR'),
        hora: dataHora.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' }),
        status: agendamento.status || 'Pendente',
      };
    });

    return {
      totalBarbeiros,
      totalServicos,
      agendamentosHoje: agendamentosHoje.length,
      proximosAgendamentos,
    };
  },
};
