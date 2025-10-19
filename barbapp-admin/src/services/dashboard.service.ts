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
    try {
      // Fetch data in parallel for better performance
      const [barbeirosResponse, servicosResponse] = await Promise.all([
        // Get total barbers count
        api.get('/barbers', { params: { pageSize: 1, page: 1 } }),
        // Get total services count
        api.get('/barbershop-services', { params: { pageSize: 1, page: 1 } }),
      ]);

      // Extract totals from pagination metadata
      const totalBarbeiros = barbeirosResponse.data.totalCount || 0;
      const totalServicos = servicosResponse.data.totalCount || 0;

      // TODO: Implement appointments endpoint in backend
      // For now, return zero appointments
      return {
        totalBarbeiros,
        totalServicos,
        agendamentosHoje: 0,
        proximosAgendamentos: [],
      };
    } catch (error) {
      console.error('Error fetching dashboard metrics:', error);
      // Return empty metrics on error
      return {
        totalBarbeiros: 0,
        totalServicos: 0,
        agendamentosHoje: 0,
        proximosAgendamentos: [],
      };
    }
  },
};
