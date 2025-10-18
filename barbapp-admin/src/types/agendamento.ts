/**
 * Tipos para Agendamentos
 * 
 * Alias PT → EN para manter consistência do frontend em português
 * enquanto a API usa inglês.
 */

/**
 * Tipo base do backend (inglês)
 */
export interface Appointment {
  id: string;
  barberId: string;
  barberName: string;
  customerId: string;
  customerName: string;
  startTime: string;
  endTime: string;
  serviceName: string;
  status: 'Pending' | 'Confirmed' | 'Completed' | 'Cancelled';
}

/**
 * Alias em português para uso no frontend
 */
export interface Agendamento extends Appointment {
  // Mesmos campos, apenas alias
}

/**
 * Parâmetros para listagem de agendamentos
 */
export interface ListAppointmentsParams {
  page?: number;
  pageSize?: number;
  barberId?: string;
  startDate?: string;  // ISO 8601 format
  endDate?: string;    // ISO 8601 format
  status?: string;
}

/**
 * Alias em português
 */
export interface ListAgendamentosParams extends ListAppointmentsParams {}

/**
 * Status traduzidos para PT
 */
export const AppointmentStatus = {
  Pending: 'Pendente',
  Confirmed: 'Confirmado',
  Completed: 'Concluído',
  Cancelled: 'Cancelado',
} as const;

export type AppointmentStatusKey = keyof typeof AppointmentStatus;
export type AppointmentStatusValue = typeof AppointmentStatus[AppointmentStatusKey];

/**
 * Helper para traduzir status EN → PT
 */
export function translateAppointmentStatus(status: string): string {
  return AppointmentStatus[status as AppointmentStatusKey] || status;
}

/**
 * Helper para obter variante do Badge por status
 */
export function getAppointmentStatusVariant(status: string): 'default' | 'secondary' | 'destructive' | 'outline' {
  const variants: Record<string, 'default' | 'secondary' | 'destructive' | 'outline'> = {
    Pending: 'secondary',
    Confirmed: 'default',
    Completed: 'outline',
    Cancelled: 'destructive',
  };
  return variants[status] || 'default';
}

/**
 * Helper para obter classe CSS personalizada por status
 */
export function getAppointmentStatusClass(status: string): string {
  const classes: Record<string, string> = {
    Pending: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    Confirmed: 'bg-blue-100 text-blue-800 border-blue-300',
    Completed: 'bg-green-100 text-green-800 border-green-300',
    Cancelled: 'bg-red-100 text-red-800 border-red-300',
  };
  return classes[status] || '';
}
