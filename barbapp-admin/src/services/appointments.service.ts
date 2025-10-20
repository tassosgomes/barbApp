/**
 * Appointments Service - Schedule Actions
 * 
 * Service for barbers to manage their appointments:
 * - Get appointment details
 * - Confirm pending appointments
 * - Cancel appointments
 * - Complete appointments
 * 
 * Error handling:
 * - 403 Forbidden: User doesn't have permission to access/modify this appointment
 * - 404 Not Found: Appointment not found
 * - 409 Conflict: Appointment status was modified (optimistic concurrency)
 */

import api from './api';
import type { AppointmentDetails } from '@/types';
import { AxiosError } from 'axios';

/**
 * Custom error for appointment operations
 */
export class AppointmentError extends Error {
  constructor(
    message: string,
    public statusCode: number,
    public originalError?: unknown
  ) {
    super(message);
    this.name = 'AppointmentError';
  }
}

/**
 * Handle API errors and throw user-friendly messages
 */
const handleAppointmentError = (error: unknown): never => {
  if (error && typeof error === 'object' && 'response' in error) {
    const axiosError = error as AxiosError;
    const statusCode = axiosError.response?.status;

    switch (statusCode) {
      case 403:
        throw new AppointmentError(
          'Você não tem permissão para acessar este agendamento',
          403,
          error
        );
      case 404:
        throw new AppointmentError(
          'Agendamento não encontrado',
          404,
          error
        );
      case 409:
        throw new AppointmentError(
          'Este agendamento foi modificado. Atualize a página.',
          409,
          error
        );
      default:
        throw new AppointmentError(
          'Erro ao processar agendamento. Tente novamente.',
          statusCode || 500,
          error
        );
    }
  }

  throw new AppointmentError(
    'Erro inesperado ao processar agendamento',
    500,
    error
  );
};

export const appointmentsService = {
  /**
   * Get appointment details
   * 
   * @param id - Appointment ID
   * @returns Complete appointment details
   * @throws {AppointmentError} 403, 404, or 500
   * 
   * @example
   * try {
   *   const details = await appointmentsService.getDetails('uuid-appointment');
   *   console.log(details.customerName);
   * } catch (error) {
   *   if (error instanceof AppointmentError) {
   *     console.error(error.message); // User-friendly message
   *   }
   * }
   */
  getDetails: async (id: string): Promise<AppointmentDetails> => {
    try {
      const { data } = await api.get<AppointmentDetails>(`/appointments/${id}`);
      return data;
    } catch (error) {
      return handleAppointmentError(error);
    }
  },

  /**
   * Confirm a pending appointment
   * 
   * @param id - Appointment ID
   * @throws {AppointmentError} 403, 404, 409, or 500
   * 
   * @example
   * try {
   *   await appointmentsService.confirm('uuid-appointment');
   *   // Show success message
   * } catch (error) {
   *   if (error instanceof AppointmentError) {
   *     // Show error.message to user
   *   }
   * }
   */
  confirm: async (id: string): Promise<void> => {
    try {
      await api.post(`/appointments/${id}/confirm`);
    } catch (error) {
      return handleAppointmentError(error);
    }
  },

  /**
   * Cancel an appointment (pending or confirmed)
   * 
   * @param id - Appointment ID
   * @throws {AppointmentError} 403, 404, 409, or 500
   * 
   * @example
   * try {
   *   await appointmentsService.cancel('uuid-appointment');
   *   // Show success message
   * } catch (error) {
   *   if (error instanceof AppointmentError) {
   *     // Show error.message to user
   *   }
   * }
   */
  cancel: async (id: string): Promise<void> => {
    try {
      await api.post(`/appointments/${id}/cancel`);
    } catch (error) {
      return handleAppointmentError(error);
    }
  },

  /**
   * Complete a confirmed appointment
   * 
   * @param id - Appointment ID
   * @throws {AppointmentError} 403, 404, 409, or 500
   * 
   * @example
   * try {
   *   await appointmentsService.complete('uuid-appointment');
   *   // Show success message
   * } catch (error) {
   *   if (error instanceof AppointmentError) {
   *     // Show error.message to user
   *   }
   * }
   */
  complete: async (id: string): Promise<void> => {
    try {
      await api.post(`/appointments/${id}/complete`);
    } catch (error) {
      return handleAppointmentError(error);
    }
  },
};
