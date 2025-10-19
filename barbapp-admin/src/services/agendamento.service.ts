/**
 * Serviço para gerenciamento de agendamentos
 * 
 * Responsável por comunicação com a API de appointments.
 * Admin Barbearia: apenas visualização (sem criar/editar).
 */

import api from './api';
import type { Appointment, ListAppointmentsParams } from '@/types/agendamento';
import type { PaginatedResponse } from '@/types/servico';

/**
 * Serviço de agendamentos (appointments)
 */
export const appointmentService = {
  /**
   * Lista agendamentos com filtros e paginação
   * 
   * @param params - Parâmetros de filtro e paginação
   * @returns Lista paginada de agendamentos
   * 
   * @example
   * const result = await appointmentService.list({
   *   page: 1,
   *   pageSize: 10,
   *   barberId: 'uuid-barbeiro',
   *   startDate: '2024-01-01',
   *   status: 'Confirmed'
   * });
   */
  list: async (params: ListAppointmentsParams = {}): Promise<PaginatedResponse<Appointment>> => {
    const response = await api.get<PaginatedResponse<Appointment>>('/appointments', {
      params: {
        pageNumber: params.page,
        pageSize: params.pageSize,
        barberId: params.barberId,
        startDate: params.startDate,
        endDate: params.endDate,
        status: params.status,
      },
    });
    return response.data;
  },

  /**
   * Busca agendamento por ID
   * 
   * @param id - ID do agendamento
   * @returns Detalhes completos do agendamento
   * 
   * @example
   * const agendamento = await appointmentService.getById('uuid-agendamento');
   */
  getById: async (id: string): Promise<Appointment> => {
    const response = await api.get<Appointment>(`/appointments/${id}`);
    return response.data;
  },
};

/**
 * Alias em português para o service
 * Mantém consistência com nomenclatura do frontend
 */
export const agendamentoService = appointmentService;
