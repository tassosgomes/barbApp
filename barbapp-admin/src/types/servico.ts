/**
 * Tipos para gestão de serviços
 * Aliases e extensões para compatibilidade com especificação da Task 11.0
 */

// Re-export tipos principais do service.ts para manter compatibilidade
export type {
  BarbershopService,
  CreateServiceRequest,
  UpdateServiceRequest,
} from './service';

// Aliases compatíveis com nomenclatura da task (português)
export type { BarbershopService as Servico } from './service';
export type { CreateServiceRequest as CreateServicoInput } from './service';
export type { UpdateServiceRequest as UpdateServicoInput } from './service';

// Tipos específicos para listagem com paginação
export interface ListServicosParams {
  page?: number;
  pageSize?: number;
  search?: string;
  searchName?: string; // Alias para search
  isActive?: boolean;
}

// Re-export tipo de resposta paginada
export type { PaginatedResponse } from './pagination';

// Tipos adicionais específicos para serviços
export interface ServicoSummary {
  id: string;
  nome: string;
  duracaoMinutos: number;
  preco: number;
}

// Interface para o contexto de serviços (caso necessário)
export interface ServicoFilters {
  search?: string;
  isActive?: boolean;
  minPrice?: number;
  maxPrice?: number;
  minDuration?: number;
  maxDuration?: number;
}