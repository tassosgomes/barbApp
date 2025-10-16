import { AppointmentStatus } from './schedule';

export interface BarberFilters {
  searchName?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ServiceFilters {
  searchName?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface ScheduleFilters {
  date?: string;
  barberId?: string;
  status?: AppointmentStatus;
}