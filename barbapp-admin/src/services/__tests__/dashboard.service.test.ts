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

      vi.mocked(api.get)
        .mockResolvedValueOnce(mockBarbeirosResponse)
        .mockResolvedValueOnce(mockServicosResponse);

      const result = await dashboardService.getMetrics();

      expect(result.totalBarbeiros).toBe(5);
      expect(result.totalServicos).toBe(8);
      // TODO: agendamentos not yet implemented in backend
      expect(result.agendamentosHoje).toBe(0);
      expect(result.proximosAgendamentos).toHaveLength(0);
    });

    it('should call correct API endpoints', async () => {
      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 3 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 7 } });

      await dashboardService.getMetrics();

      expect(api.get).toHaveBeenCalledWith('/barbers', { params: { pageSize: 1, page: 1 } });
      expect(api.get).toHaveBeenCalledWith('/barbershop-services', { params: { pageSize: 1, page: 1 } });
    });

    it('should handle zero counts gracefully', async () => {
      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } })
        .mockResolvedValueOnce({ data: { items: [], totalCount: 0 } });

      const result = await dashboardService.getMetrics();

      expect(result.totalBarbeiros).toBe(0);
      expect(result.totalServicos).toBe(0);
      expect(result.agendamentosHoje).toBe(0);
      expect(result.proximosAgendamentos).toHaveLength(0);
    });

    it('should handle missing totalCount gracefully', async () => {
      vi.mocked(api.get)
        .mockResolvedValueOnce({ data: { items: [] } })
        .mockResolvedValueOnce({ data: { items: [] } });

      const result = await dashboardService.getMetrics();

      expect(result.totalBarbeiros).toBe(0);
      expect(result.totalServicos).toBe(0);
    });

    it('should return empty metrics on API error', async () => {
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network error'));

      // Mock console.error to avoid test output pollution
      const consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});

      const result = await dashboardService.getMetrics();

      expect(result).toEqual({
        totalBarbeiros: 0,
        totalServicos: 0,
        agendamentosHoje: 0,
        proximosAgendamentos: [],
      });

      consoleErrorSpy.mockRestore();
    });
  });
});
