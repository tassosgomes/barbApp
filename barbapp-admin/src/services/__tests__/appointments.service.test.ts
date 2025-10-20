import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { appointmentsService, AppointmentError } from '../appointments.service';
import api from '../api';
import { AppointmentStatus } from '@/types/appointment';

// Mock the API module
vi.mock('../api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
  },
}));

// Get typed references to the mocked functions
const mockApiGet = vi.mocked(api.get);
const mockApiPost = vi.mocked(api.post);

describe('Appointments Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.resetAllMocks();
  });

  describe('getDetails', () => {
    it('should fetch appointment details successfully', async () => {
      const mockAppointment = {
        id: '550e8400-e29b-41d4-a716-446655440001',
        customerName: 'Carlos Oliveira',
        customerPhone: '+5511999999999',
        serviceTitle: 'Corte de Cabelo',
        servicePrice: 50.0,
        serviceDurationMinutes: 30,
        startTime: '2025-10-20T14:00:00Z',
        endTime: '2025-10-20T14:30:00Z',
        status: AppointmentStatus.Confirmed,
        createdAt: '2025-10-19T10:00:00Z',
        confirmedAt: '2025-10-19T12:00:00Z',
      };

      mockApiGet.mockResolvedValueOnce({ data: mockAppointment });

      const result = await appointmentsService.getDetails('550e8400-e29b-41d4-a716-446655440001');

      expect(api.get).toHaveBeenCalledWith('/appointments/550e8400-e29b-41d4-a716-446655440001');
      expect(result).toEqual(mockAppointment);
    });

    it('should throw AppointmentError with 404 message when appointment not found', async () => {
      const axiosError = {
        response: { status: 404 },
      };

      mockApiGet.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.getDetails('non-existent-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Agendamento não encontrado');
        expect((error as AppointmentError).statusCode).toBe(404);
      }
    });

    it('should throw AppointmentError with 403 message when access forbidden', async () => {
      const axiosError = {
        response: { status: 403 },
      };

      mockApiGet.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.getDetails('forbidden-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Você não tem permissão para acessar este agendamento');
        expect((error as AppointmentError).statusCode).toBe(403);
      }
    });

    it('should throw generic AppointmentError for unknown errors', async () => {
      mockApiGet.mockRejectedValueOnce(new Error('Network error'));

      await expect(
        appointmentsService.getDetails('some-id')
      ).rejects.toThrow(AppointmentError);

      try {
        await appointmentsService.getDetails('some-id');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Erro inesperado ao processar agendamento');
        expect((error as AppointmentError).statusCode).toBe(500);
      }
    });
  });

  describe('confirm', () => {
    it('should confirm appointment successfully', async () => {
      mockApiPost.mockResolvedValueOnce({ data: {} });

      await appointmentsService.confirm('550e8400-e29b-41d4-a716-446655440001');

      expect(api.post).toHaveBeenCalledWith('/appointments/550e8400-e29b-41d4-a716-446655440001/confirm');
    });

    it('should throw AppointmentError with 409 message when conflict occurs', async () => {
      const axiosError = {
        response: { status: 409 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.confirm('conflict-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Este agendamento foi modificado. Atualize a página.');
        expect((error as AppointmentError).statusCode).toBe(409);
      }
    });

    it('should throw AppointmentError with 404 message when appointment not found', async () => {
      const axiosError = {
        response: { status: 404 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.confirm('non-existent-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Agendamento não encontrado');
        expect((error as AppointmentError).statusCode).toBe(404);
      }
    });

    it('should throw AppointmentError with 403 message when access forbidden', async () => {
      const axiosError = {
        response: { status: 403 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.confirm('forbidden-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Você não tem permissão para acessar este agendamento');
        expect((error as AppointmentError).statusCode).toBe(403);
      }
    });
  });

  describe('cancel', () => {
    it('should cancel appointment successfully', async () => {
      mockApiPost.mockResolvedValueOnce({ data: {} });

      await appointmentsService.cancel('550e8400-e29b-41d4-a716-446655440001');

      expect(api.post).toHaveBeenCalledWith('/appointments/550e8400-e29b-41d4-a716-446655440001/cancel');
    });

    it('should throw AppointmentError with 409 message when conflict occurs', async () => {
      const axiosError = {
        response: { status: 409 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.cancel('conflict-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Este agendamento foi modificado. Atualize a página.');
        expect((error as AppointmentError).statusCode).toBe(409);
      }
    });

    it('should throw AppointmentError with 404 message when appointment not found', async () => {
      const axiosError = {
        response: { status: 404 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.cancel('non-existent-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Agendamento não encontrado');
        expect((error as AppointmentError).statusCode).toBe(404);
      }
    });
  });

  describe('complete', () => {
    it('should complete appointment successfully', async () => {
      mockApiPost.mockResolvedValueOnce({ data: {} });

      await appointmentsService.complete('550e8400-e29b-41d4-a716-446655440001');

      expect(api.post).toHaveBeenCalledWith('/appointments/550e8400-e29b-41d4-a716-446655440001/complete');
    });

    it('should throw AppointmentError with 409 message when conflict occurs', async () => {
      const axiosError = {
        response: { status: 409 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.complete('conflict-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Este agendamento foi modificado. Atualize a página.');
        expect((error as AppointmentError).statusCode).toBe(409);
      }
    });

    it('should throw AppointmentError with 404 message when appointment not found', async () => {
      const axiosError = {
        response: { status: 404 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.complete('non-existent-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Agendamento não encontrado');
        expect((error as AppointmentError).statusCode).toBe(404);
      }
    });

    it('should throw AppointmentError with 403 message when access forbidden', async () => {
      const axiosError = {
        response: { status: 403 },
      };

      mockApiPost.mockRejectedValueOnce(axiosError);

      try {
        await appointmentsService.complete('forbidden-appointment-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).message).toBe('Você não tem permissão para acessar este agendamento');
        expect((error as AppointmentError).statusCode).toBe(403);
      }
    });
  });

  describe('Error handling consistency', () => {
    it('should handle errors consistently across all methods', async () => {
      const axiosError = {
        response: { status: 500 },
      };

      mockApiGet.mockRejectedValueOnce(axiosError);
      mockApiPost.mockRejectedValue(axiosError);

      const methods = [
        () => appointmentsService.getDetails('id'),
        () => appointmentsService.confirm('id'),
        () => appointmentsService.cancel('id'),
        () => appointmentsService.complete('id'),
      ];

      for (const method of methods) {
        await expect(method()).rejects.toThrow(AppointmentError);
      }
    });

    it('should preserve original error in AppointmentError', async () => {
      const originalError = {
        response: { status: 404 },
        message: 'Request failed with status code 404',
      };

      mockApiGet.mockRejectedValueOnce(originalError);

      try {
        await appointmentsService.getDetails('some-id');
        expect.fail('Should have thrown an error');
      } catch (error) {
        expect(error).toBeInstanceOf(AppointmentError);
        expect((error as AppointmentError).originalError).toBe(originalError);
      }
    });
  });
});
