import api from './api';
import type { Appointment, ScheduleFilters, BarberSchedule } from '@/types';

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

  /**
   * Get barber's schedule for a specific date
   * @param date - Date in ISO format (YYYY-MM-DD)
   * @returns Barber's schedule with appointments for the specified date
   * 
   * @example
   * const schedule = await scheduleService.getMySchedule('2025-10-20');
   */
  getMySchedule: async (date: string): Promise<BarberSchedule> => {
    const { data } = await api.get<BarberSchedule>('/schedule/my-schedule', {
      params: { date },
    });
    return data;
  },
};