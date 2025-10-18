/**
 * Tipos para gestão de barbeiros
 * Aliases e extensões para compatibilidade com especificação da Task 9.0
 */

// Re-export tipos principais do barber.ts para manter compatibilidade
export type {
  Barber,
  ServiceSummary,
  CreateBarberRequest,
  UpdateBarberRequest,
} from './barber';

// Aliases compatíveis com nomenclatura da task
export type { Barber as Barbeiro } from './barber';
export type { CreateBarberRequest as CreateBarbeiroInput } from './barber';
export type { UpdateBarberRequest as UpdateBarbeiroInput } from './barber';

// Tipos específicos para listagem com paginação
export interface ListBarbeirosParams {
  page?: number;
  pageSize?: number;
  search?: string;
  searchName?: string; // Alias para search
  isActive?: boolean;
}

// Re-export tipo de resposta paginada
export type { PaginatedResponse } from './pagination';
