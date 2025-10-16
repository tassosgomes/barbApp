import api from './api';
import type {
  Barbershop,
  CreateBarbershopRequest,
  UpdateBarbershopRequest,
  PaginatedResponse,
  BarbershopFilters,
} from '@/types';

// Helper function to normalize API response to expected format
function normalizePaginatedResponse<T>(data: {
  items?: T[];
  pageNumber?: number;
  page?: number;
  pageSize?: number;
  totalPages?: number;
  totalCount?: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}): PaginatedResponse<T> {
  return {
    items: data.items || [],
    pageNumber: data.pageNumber || data.page || 1,
    pageSize: data.pageSize || 20,
    totalPages: data.totalPages || Math.ceil((data.totalCount || 0) / (data.pageSize || 20)),
    totalCount: data.totalCount || data.items?.length || 0,
    hasPreviousPage: data.hasPreviousPage !== undefined ? data.hasPreviousPage : (data.page || 1) > 1,
    hasNextPage: data.hasNextPage !== undefined ? data.hasNextPage : (data.page || 1) < (data.totalPages || Math.ceil((data.totalCount || 0) / (data.pageSize || 20))),
  };
}

export const barbershopService = {
  /**
   * List barbershops with pagination and filters
   * @param filters - Query parameters for filtering and pagination
   * @returns Paginated list of barbershops
   */
  getAll: async (filters: BarbershopFilters): Promise<PaginatedResponse<Barbershop>> => {
    const { data } = await api.get<{
      items?: Barbershop[];
      pageNumber?: number;
      page?: number;
      pageSize?: number;
      totalPages?: number;
      totalCount?: number;
      hasPreviousPage?: boolean;
      hasNextPage?: boolean;
    }>('/barbearias', {
      params: filters,
    });
    return normalizePaginatedResponse<Barbershop>(data);
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