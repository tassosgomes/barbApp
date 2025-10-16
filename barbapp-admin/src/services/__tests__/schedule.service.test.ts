import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { scheduleService } from '../schedule.service';
import api from '../api';
import { AppointmentStatus } from '@/types/schedule';

// Mock the API module
vi.mock('../api', () => ({
  default: {
    get: vi.fn(),
  },
}));

// Get typed references to the mocked functions
const mockApiGet = vi.mocked(api.get);

describe('Schedule Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('list', () => {
    it('should fetch schedule appointments with filters', async () => {
      const mockResponse = {
        appointments: [
          {
            id: '550e8400-e29b-41d4-a716-446655440001',
            barberId: '550e8400-e29b-41d4-a716-446655440000',
            barberName: 'João Silva',
            customerId: '550e8400-e29b-41d4-a716-446655440002',
            customerName: 'Carlos Oliveira',
            startTime: '2025-10-15T14:00:00Z',
            endTime: '2025-10-15T14:30:00Z',
            serviceName: 'Corte de Cabelo',
            status: 'Confirmed',
          },
        ],
      };

      mockApiGet.mockResolvedValueOnce({ data: mockResponse });

      const filters = {
        date: '2025-10-15',
        barberId: '550e8400-e29b-41d4-a716-446655440000',
        status: AppointmentStatus.Confirmed,
      };

      const result = await scheduleService.list(filters);

      expect(api.get).toHaveBeenCalledWith('/barbers/schedule', {
        params: filters,
      });
      expect(result).toEqual(mockResponse);
    });

    it('should fetch schedule appointments without filters', async () => {
      const mockResponse = {
        appointments: [],
      };

      mockApiGet.mockResolvedValueOnce({ data: mockResponse });

      const result = await scheduleService.list({});

      expect(api.get).toHaveBeenCalledWith('/barbers/schedule', {
        params: {},
      });
      expect(result).toEqual(mockResponse);
    });

    it('should fetch schedule appointments with partial filters', async () => {
      const mockResponse = {
        appointments: [
          {
            id: '550e8400-e29b-41d4-a716-446655440001',
            barberId: '550e8400-e29b-41d4-a716-446655440000',
            barberName: 'João Silva',
            customerId: '550e8400-e29b-41d4-a716-446655440002',
            customerName: 'Carlos Oliveira',
            startTime: '2025-10-15T14:00:00Z',
            endTime: '2025-10-15T14:30:00Z',
            serviceName: 'Corte de Cabelo',
            status: 'Pending',
          },
        ],
      };

      mockApiGet.mockResolvedValueOnce({ data: mockResponse });

      const filters = {
        date: '2025-10-15',
      };

      const result = await scheduleService.list(filters);

      expect(api.get).toHaveBeenCalledWith('/barbers/schedule', {
        params: filters,
      });
      expect(result).toEqual(mockResponse);
    });

    it('should handle API errors', async () => {
      const error = new Error('Network error');
      mockApiGet.mockRejectedValueOnce(error);

      const filters = {
        date: '2025-10-15',
      };

      await expect(scheduleService.list(filters)).rejects.toThrow('Network error');
      expect(api.get).toHaveBeenCalledWith('/barbers/schedule', {
        params: filters,
      });
    });

    it('should handle timeout errors', async () => {
      const timeoutError = new Error('Timeout');
      timeoutError.name = 'TimeoutError';
      mockApiGet.mockRejectedValueOnce(timeoutError);

      const filters = {
        date: '2025-10-15',
      };

      await expect(scheduleService.list(filters)).rejects.toThrow(timeoutError);
      expect(api.get).toHaveBeenCalledWith('/barbers/schedule', {
        params: filters,
      });
    });
  });
});