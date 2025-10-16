import api from './api';
import type { Appointment, ScheduleFilters } from '@/types';

export interface ScheduleServiceResponse {
  appointments: Appointment[];
}

export const scheduleService = {
  /**
   * List schedule appointments with filters
   * @param filters - Query parameters for filtering by date, barber, and status
   * @returns List of appointments for the specified filters
   */
  list: async (filters: ScheduleFilters): Promise<ScheduleServiceResponse> => {
    const { data } = await api.get<ScheduleServiceResponse>('/barbers/schedule', {
      params: filters,
    });
    return data;
  },
};