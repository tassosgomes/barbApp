import { describe, it, expect, vi, beforeEach } from 'vitest';
import { dashboardService } from '../dashboard.service';
import api from '../api';

// Mock the api module
vi.mock('../api', () => ({
  default: {
    get: vi.fn(),
  },
}));

describe('dashboardService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getMetrics', () => {
    it('should fetch and aggregate metrics from multiple endpoints', async () => {
      const mockBarbeirosResponse = {
        data: {
          items: [],
          totalCount: 5,
        },
      };

      const mockServicosResponse = {
        data: {
          items: [],
          totalCount: 8,
        },
      };

      const mockAgendamentosResponse = {
        data: {
          items: [
            {
              id: '1',
              clienteNome: 'João Silva',
              barbeiroNome: 'Pedro Barbeiro',
              servicoNome: 'Corte',
              dataHora: new Date().toISOString(),
              status: 'Confirmado',
            },
            {
              id: '2',
              clienteNome: 'Maria Santos',
              barbeiroNome: 'Carlos Barbeiro',
              servicoNome: 'Barba',
              dataHora: new Date().toISOString(),
              status: 'Pendente',
            },
          ],
        },
      };

      vi.mocked(api.get)
        .mockResolvedValueOnce(mockBarbeirosResponse)
        .mockResolvedValueOnce(mockServicosResponse)
        .mockResolvedValueOnce(mockAgendamentosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.totalBarbeiros).toBe(5);
      expect(result.totalServicos).toBe(8);
      expect(result.agendamentosHoje).toBe(2);
      expect(result.proximosAgendamentos).toHaveLength(2);
    });

    it('should filter appointments for today only', async () => {
      const today = new Date();
      const tomorrow = new Date(today);
      tomorrow.setDate(tomorrow.getDate() + 1);

      const mockAgendamentosResponse = {
        data: {
          items: [
            {
              id: '1',
              clienteNome: 'João Silva',
              barbeiroNome: 'Pedro',
              servicoNome: 'Corte',
              dataHora: today.toISOString(),
              status: 'Confirmado',
            },
            {
              id: '2',
              clienteNome: 'Maria Santos',
              barbeiroNome: 'Carlos',
              servicoNome: 'Barba',
              dataHora: tomorrow.toISOString(),
              status: 'Pendente',
            },
          ],
        },
      };

      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce(mockAgendamentosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.agendamentosHoje).toBe(1);
      expect(result.proximosAgendamentos).toHaveLength(1);
      expect(result.proximosAgendamentos[0].clienteNome).toBe('João Silva');
    });

    it('should limit upcoming appointments to 5', async () => {
      const today = new Date().toISOString();
      const appointments = Array.from({ length: 10 }, (_, i) => ({
        id: `${i}`,
        clienteNome: `Cliente ${i}`,
        barbeiroNome: 'Barbeiro',
        servicoNome: 'Serviço',
        dataHora: today,
        status: 'Confirmado',
      }));

      const mockAgendamentosResponse = {
        data: {
          items: appointments,
        },
      };

      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce(mockAgendamentosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.proximosAgendamentos).toHaveLength(5);
    });

    it('should handle missing data gracefully', async () => {
      const mockAgendamentosResponse = {
        data: {
          items: [
            {
              id: '1',
              dataHora: new Date().toISOString(),
            },
          ],
        },
      };

      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce(mockAgendamentosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.proximosAgendamentos[0]).toMatchObject({
        id: '1',
        clienteNome: 'Cliente não informado',
        barbeiro: 'Barbeiro não informado',
        servico: 'Serviço não informado',
      });
    });

    it('should format date and time correctly', async () => {
      const testDate = new Date('2025-10-18T14:30:00');
      const mockAgendamentosResponse = {
        data: {
          items: [
            {
              id: '1',
              clienteNome: 'João',
              barbeiroNome: 'Pedro',
              servicoNome: 'Corte',
              dataHora: testDate.toISOString(),
              status: 'Confirmado',
            },
          ],
        },
      };

      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce(mockAgendamentosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.proximosAgendamentos[0].hora).toMatch(/\d{2}:\d{2}/);
      expect(result.proximosAgendamentos[0].data).toMatch(/\d{2}\/\d{2}\/\d{4}/);
    });
  });
});
