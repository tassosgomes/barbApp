export enum AppointmentStatus {
  Pending = 'Pending',
  Confirmed = 'Confirmed',
  Cancelled = 'Cancelled',
  Completed = 'Completed',
}

export interface Appointment {
  id: string;
  barberId: string;
  barberName: string;
  customerId: string;
  customerName: string;
  startTime: string;
  endTime: string;
  serviceName: string;
  status: AppointmentStatus;
}