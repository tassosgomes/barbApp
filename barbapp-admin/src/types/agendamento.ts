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
  serviceTitle: string;
  status: 0 | 1 | 2 | 3;
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
export function translateAppointmentStatus(status: number): string {
  const statusMap: Record<number, string> = {
    0: 'Pendente',
    1: 'Confirmado',
    2: 'Concluído',
    3: 'Cancelado',
  };
  return statusMap[status] || 'Desconhecido';
}

/**
 * Helper para obter variante do Badge por status
 */
export function getAppointmentStatusVariant(status: number): 'default' | 'secondary' | 'destructive' | 'outline' {
  const variants: Record<number, 'default' | 'secondary' | 'destructive' | 'outline'> = {
    0: 'secondary',
    1: 'default',
    2: 'outline',
    3: 'destructive',
  };
  return variants[status] || 'default';
}

/**
 * Helper para obter classe CSS personalizada por status
 */
export function getAppointmentStatusClass(status: number): string {
  const classes: Record<number, string> = {
    0: 'bg-yellow-100 text-yellow-800 border-yellow-300',
    1: 'bg-blue-100 text-blue-800 border-blue-300',
    2: 'bg-green-100 text-green-800 border-green-300',
    3: 'bg-red-100 text-red-800 border-red-300',
  };
  return classes[status] || '';
}
