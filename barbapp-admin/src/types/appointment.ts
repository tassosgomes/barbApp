/**
 * Tipos TypeScript para o sistema de agendamentos do barbeiro
 * Alinhados com os DTOs do backend (BarberScheduleOutput, AppointmentDetailsOutput)
 */

export enum AppointmentStatus {
  Pending = 0,
  Confirmed = 1,
  Completed = 2,
  Cancelled = 3
}

export interface Appointment {
  id: string;
  customerName: string;
  serviceTitle: string;
  startTime: string; // ISO 8601
  endTime: string; // ISO 8601
  status: AppointmentStatus;
}

export interface AppointmentDetails extends Appointment {
  customerPhone: string;
  servicePrice: number;
  serviceDurationMinutes: number;
  createdAt: string;
  confirmedAt?: string;
  cancelledAt?: string;
  completedAt?: string;
}

export interface BarberSchedule {
  date: string;
  barberId: string;
  barberName: string;
  appointments: Appointment[];
}