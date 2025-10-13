import api from './api';
import type {
  Barbershop,
  CreateBarbershopRequest,
  UpdateBarbershopRequest,
  PaginatedResponse,
  BarbershopFilters,
} from '@/types';

export const barbershopService = {
  /**
   * List barbershops with pagination and filters
   * @param filters - Query parameters for filtering and pagination
   * @returns Paginated list of barbershops
   */
  getAll: async (filters: BarbershopFilters): Promise<PaginatedResponse<Barbershop>> => {
    const { data } = await api.get<PaginatedResponse<Barbershop>>('/barbearias', {
      params: filters,
    });
    return data;
  },

  /**
   * Get barbershop by ID
   * @param id - Barbershop unique identifier
   * @returns Barbershop details
   */
  getById: async (id: string): Promise<Barbershop> => {
    const { data } = await api.get<Barbershop>(`/barbearias/${id}`);
    return data;
  },

  /**
   * Create new barbershop
   * @param request - Barbershop creation data
   * @returns Created barbershop with generated ID
   */
  create: async (request: CreateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.post<Barbershop>('/barbearias', request);
    return data;
  },

  /**
   * Update existing barbershop
   * @param id - Barbershop unique identifier
   * @param request - Updated barbershop data
   * @returns Updated barbershop
   */
  update: async (id: string, request: UpdateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.put<Barbershop>(`/barbearias/${id}`, request);
    return data;
  },

  /**
   * Deactivate barbershop (soft delete)
   * @param id - Barbershop unique identifier
   */
  deactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/desativar`);
  },

  /**
   * Reactivate barbershop
   * @param id - Barbershop unique identifier
   */
  reactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/reativar`);
  },
};