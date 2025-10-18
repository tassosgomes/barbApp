/**
 * Serviço para gestão de barbeiros
 * Compatível com Task 9.0 - Interface Admin Barbearia
 * 
 * Endpoints utilizados (backend em inglês):
 * - GET /api/barbers - Lista barbeiros (filtrado automaticamente por tenant via JWT)
 * - GET /api/barbers/:id - Busca barbeiro por ID
 * - POST /api/barbers - Cria novo barbeiro
 * - PUT /api/barbers/:id - Atualiza barbeiro
 * - PUT /api/barbers/:id/deactivate - Desativa barbeiro
 * - PUT /api/barbers/:id/reactivate - Reativa barbeiro
 * 
 * Nota: O isolamento por tenant é garantido automaticamente pelo backend via JWT
 */

import api from './api';
import type {
  Barber,
  CreateBarberRequest,
  UpdateBarberRequest,
  PaginatedResponse,
} from '@/types';

// Tipos compatíveis com Task 9.0 (aliases)
export type Barbeiro = Barber;
export type CreateBarbeiroInput = CreateBarberRequest;
export type UpdateBarbeiroInput = UpdateBarberRequest;

// Interface para parâmetros de listagem
export interface ListBarbeirosParams {
  page?: number;
  pageSize?: number;
  search?: string;
  searchName?: string; // Alias
  isActive?: boolean;
}

// Interface para resposta da API de listagem
interface BarbeirosListResponse {
  barbers: Barber[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/**
 * Normaliza resposta da API para o formato PaginatedResponse esperado
 */
function normalizePaginatedResponse(data: BarbeirosListResponse): PaginatedResponse<Barber> {
  return {
    items: data.barbers || [],
    pageNumber: data.page || 1,
    pageSize: data.pageSize || 20,
    totalPages: Math.ceil((data.totalCount || 0) / (data.pageSize || 20)),
    totalCount: data.totalCount || data.barbers?.length || 0,
    hasPreviousPage: (data.page || 1) > 1,
    hasNextPage: (data.page || 1) < Math.ceil((data.totalCount || 0) / (data.pageSize || 20)),
  };
}

/**
 * Serviço de gestão de barbeiros
 * Implementa todos os métodos CRUD necessários
 */
export const barbeiroService = {
  /**
   * Lista barbeiros com paginação e filtros
   * Backend filtra automaticamente por barbearia do token JWT
   * 
   * @param params - Parâmetros de filtro e paginação
   * @returns Resposta paginada com lista de barbeiros
   * 
   * @example
   * const result = await barbeiroService.list({ page: 1, pageSize: 10, isActive: true });
   */
  list: async (params: ListBarbeirosParams = {}): Promise<PaginatedResponse<Barbeiro>> => {
    // Normaliza parâmetros (search vs searchName)
    const queryParams = {
      page: params.page,
      pageSize: params.pageSize,
      searchName: params.searchName || params.search,
      isActive: params.isActive,
    };

    const { data } = await api.get<BarbeirosListResponse>('/barbers', {
      params: queryParams,
    });

    return normalizePaginatedResponse(data);
  },

  /**
   * Busca barbeiro por ID
   * 
   * @param id - UUID do barbeiro
   * @returns Dados completos do barbeiro
   * @throws {Error} 404 se barbeiro não encontrado ou não pertence à barbearia
   * 
   * @example
   * const barbeiro = await barbeiroService.getById('123e4567-e89b-12d3-a456-426614174000');
   */
  getById: async (id: string): Promise<Barbeiro> => {
    const { data } = await api.get<Barbeiro>(`/barbers/${id}`);
    return data;
  },

  /**
   * Cria novo barbeiro
   * Barbeiro é automaticamente associado à barbearia do token JWT
   * 
   * @param input - Dados do barbeiro (nome, email, telefone, senha, serviços)
   * @returns Barbeiro criado com ID gerado
   * @throws {Error} 409 se email já existe, 400 se validação falhar
   * 
   * @example
   * const novoBarbeiro = await barbeiroService.create({
   *   name: 'João Silva',
   *   email: 'joao@example.com',
   *   phone: '(11) 98765-4321',
   *   password: 'senha123',
   *   serviceIds: ['uuid-servico-1', 'uuid-servico-2']
   * });
   */
  create: async (input: CreateBarbeiroInput): Promise<Barbeiro> => {
    const { data } = await api.post<Barbeiro>('/barbers', input);
    return data;
  },

  /**
   * Atualiza dados de barbeiro existente
   * Apenas nome, telefone e serviços podem ser atualizados
   * 
   * @param id - UUID do barbeiro
   * @param input - Dados a serem atualizados
   * @returns Barbeiro atualizado
   * @throws {Error} 404 se barbeiro não encontrado, 403 se não pertence à barbearia
   * 
   * @example
   * const atualizado = await barbeiroService.update('uuid', {
   *   name: 'João da Silva',
   *   phone: '(11) 91234-5678'
   * });
   */
  update: async (id: string, input: UpdateBarbeiroInput): Promise<Barbeiro> => {
    const { data } = await api.put<Barbeiro>(`/barbers/${id}`, input);
    return data;
  },

  /**
   * Desativa barbeiro
   * Barbeiro desativado não pode receber novos agendamentos
   * 
   * @param id - UUID do barbeiro
   * @throws {Error} 404 se barbeiro não encontrado, 403 se não pertence à barbearia
   * 
   * @example
   * await barbeiroService.deactivate('uuid');
   */
  deactivate: async (id: string): Promise<void> => {
    await api.put(`/barbers/${id}/deactivate`);
  },

  /**
   * Reativa barbeiro previamente desativado
   * 
   * @param id - UUID do barbeiro
   * @throws {Error} 404 se barbeiro não encontrado, 403 se não pertence à barbearia
   * 
   * @example
   * await barbeiroService.reactivate('uuid');
   */
  reactivate: async (id: string): Promise<void> => {
    await api.put(`/barbers/${id}/reactivate`);
  },

  /**
   * Alterna status ativo/inativo do barbeiro
   * Método de compatibilidade com código existente
   * 
   * @param id - UUID do barbeiro
   * @param isActive - true para ativar, false para desativar
   * 
   * @example
   * await barbeiroService.toggleActive('uuid', false); // desativa
   * await barbeiroService.toggleActive('uuid', true);  // ativa
   */
  toggleActive: async (id: string, isActive: boolean): Promise<void> => {
    if (isActive) {
      await barbeiroService.reactivate(id);
    } else {
      await barbeiroService.deactivate(id);
    }
  },
};

// Export default para compatibilidade
export default barbeiroService;
