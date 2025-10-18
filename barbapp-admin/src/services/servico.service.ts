/**
 * Serviço para gestão de serviços da barbearia
 * Compatível com Task 11.0 - Interface Admin Barbearia
 * 
 * Endpoints utilizados (backend em inglês):
 * - GET /api/barbershop-services - Lista serviços (filtrado automaticamente por tenant via JWT)
 * - GET /api/barbershop-services/:id - Busca serviço por ID
 * - POST /api/barbershop-services - Cria novo serviço
 * - PUT /api/barbershop-services/:id - Atualiza serviço
 * - DELETE /api/barbershop-services/:id - Desativa serviço (soft delete)
 * 
 * Nota: O isolamento por tenant é garantido automaticamente pelo backend via JWT
 * Nota: Backend NÃO implementa endpoint de reativação, apenas DELETE (desativação)
 */

import api from './api';
import type {
  BarbershopService,
  CreateServiceRequest,
  UpdateServiceRequest,
  PaginatedResponse,
} from '@/types';

// Tipos compatíveis com Task 11.0 (aliases em português)
export type Servico = BarbershopService;
export type CreateServicoInput = CreateServiceRequest;
export type UpdateServicoInput = UpdateServiceRequest;

// Interface para parâmetros de listagem
export interface ListServicosParams {
  page?: number;
  pageSize?: number;
  search?: string;
  searchName?: string; // Alias
  isActive?: boolean;
}

// Interface para resposta da API de listagem
interface ServicosListResponse {
  services: BarbershopService[];
  totalCount: number;
  page: number;
  pageSize: number;
}

/**
 * Normaliza resposta da API para o formato PaginatedResponse esperado
 */
function normalizePaginatedResponse(data: ServicosListResponse): PaginatedResponse<BarbershopService> {
  return {
    items: data.services || [],
    pageNumber: data.page || 1,
    pageSize: data.pageSize || 20,
    totalPages: Math.ceil((data.totalCount || 0) / (data.pageSize || 20)),
    totalCount: data.totalCount || data.services?.length || 0,
    hasPreviousPage: (data.page || 1) > 1,
    hasNextPage: (data.page || 1) < Math.ceil((data.totalCount || 0) / (data.pageSize || 20)),
  };
}

/**
 * Serviço de gestão de serviços da barbearia
 * Implementa todos os métodos CRUD necessários
 */
export const servicoService = {
  /**
   * Lista serviços com paginação e filtros
   * Backend filtra automaticamente por barbearia do token JWT
   * 
   * @param params - Parâmetros de filtro e paginação
   * @returns Resposta paginada com lista de serviços
   * 
   * @example
   * const result = await servicoService.list({ page: 1, pageSize: 10, isActive: true });
   */
  list: async (params: ListServicosParams = {}): Promise<PaginatedResponse<Servico>> => {
    // Normaliza parâmetros (search vs searchName)
    const queryParams = {
      page: params.page,
      pageSize: params.pageSize,
      searchName: params.searchName || params.search,
      isActive: params.isActive,
    };

    const { data } = await api.get<ServicosListResponse>('/barbershop-services', {
      params: queryParams,
    });

    return normalizePaginatedResponse(data);
  },

  /**
   * Busca serviço por ID
   * 
   * @param id - UUID do serviço
   * @returns Dados completos do serviço
   * @throws {Error} 404 se serviço não encontrado ou não pertence à barbearia
   * 
   * @example
   * const servico = await servicoService.getById('123e4567-e89b-12d3-a456-426614174000');
   */
  getById: async (id: string): Promise<Servico> => {
    const { data } = await api.get<Servico>(`/barbershop-services/${id}`);
    return data;
  },

  /**
   * Cria novo serviço
   * Serviço é automaticamente associado à barbearia do token JWT
   * 
   * @param input - Dados do serviço (nome, descrição, duração, preço)
   * @returns Serviço criado com ID gerado
   * @throws {Error} 409 se nome já existe, 400 se validação falhar
   * 
   * @example
   * const novoServico = await servicoService.create({
   *   name: 'Corte de Cabelo',
   *   description: 'Corte masculino tradicional',
   *   durationMinutes: 30,
   *   price: 35.00
   * });
   */
  create: async (input: CreateServicoInput): Promise<Servico> => {
    const { data } = await api.post<Servico>('/barbershop-services', input);
    return data;
  },

  /**
   * Atualiza dados de serviço existente
   * Todos os campos podem ser atualizados
   * 
   * @param id - UUID do serviço
   * @param input - Dados a serem atualizados
   * @returns Serviço atualizado
   * @throws {Error} 404 se serviço não encontrado, 403 se não pertence à barbearia
   * 
   * @example
   * const atualizado = await servicoService.update('uuid', {
   *   name: 'Corte + Barba',
   *   price: 50.00
   * });
   */
  update: async (id: string, input: UpdateServicoInput): Promise<Servico> => {
    const { data } = await api.put<Servico>(`/barbershop-services/${id}`, input);
    return data;
  },

  /**
   * Desativa serviço (soft delete)
   * Serviço desativado não pode ser associado a novos agendamentos
   * 
   * @param id - UUID do serviço
   * @throws {Error} 404 se serviço não encontrado, 403 se não pertence à barbearia
   * 
   * @example
   * await servicoService.deactivate('uuid');
   */
  deactivate: async (id: string): Promise<void> => {
    await api.delete(`/barbershop-services/${id}`);
  },

  /**
   * Reativa serviço previamente desativado
   * 
   * ATENÇÃO: Backend não implementa endpoint de reativação ainda.
   * Este método retorna erro informando que a funcionalidade não está disponível.
   * 
   * @param _id - UUID do serviço (não utilizado, mantido para compatibilidade de interface)
   * @throws {Error} Endpoint não implementado no backend
   * 
   * @example
   * await servicoService.reactivate('uuid'); // Lança erro
   */
  reactivate: async (_id: string): Promise<void> => {
    // Backend não implementa reativação via endpoint específico
    // DELETE é usado apenas para desativação
    return Promise.reject(
      new Error(
        'Endpoint de reativação não implementado. ' +
        'Backend usa DELETE apenas para desativação (soft delete). ' +
        'Para reativar, será necessário implementar PUT /api/barbershop-services/{id}/reactivate no backend.'
      )
    );
  },

  /**
   * Alterna status ativo/inativo do serviço
   * Método de compatibilidade com código existente
   * 
   * ATENÇÃO: Apenas desativação (isActive=false) está disponível.
   * Reativação (isActive=true) retornará erro.
   * 
   * @param id - UUID do serviço
   * @param isActive - true para ativar, false para desativar
   * 
   * @example
   * await servicoService.toggleActive('uuid', false); // desativa (funciona)
   * await servicoService.toggleActive('uuid', true);  // ativa (lança erro)
   */
  toggleActive: async (id: string, isActive: boolean): Promise<void> => {
    if (isActive) {
      await servicoService.reactivate(id); // Lançará erro
    } else {
      await servicoService.deactivate(id);
    }
  },
};

// Export default para compatibilidade
export default servicoService;
