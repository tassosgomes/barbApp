import api from './api';
import type {
  Barber,
  CreateBarberRequest,
  UpdateBarberRequest,
  PaginatedResponse,
  BarberFilters,
} from '@/types';

// Interface for API list response
interface BarbersListResponse {
  barbers: Barber[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Helper function to normalize API response to expected format
function normalizePaginatedResponse(data: BarbersListResponse): PaginatedResponse<Barber> {
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

export const barbersService = {
  /**
   * List barbers with pagination and filters
   * @param filters - Query parameters for filtering and pagination
   * @returns Paginated list of barbers
   */
  list: async (filters: BarberFilters): Promise<PaginatedResponse<Barber>> => {
    const { data } = await api.get<BarbersListResponse>('/barbers', {
      params: filters,
    });
    return normalizePaginatedResponse(data);
  },

  /**
   * Get barber by ID
   * @param id - Barber unique identifier
   * @returns Barber details
   */
  getById: async (id: string): Promise<Barber> => {
    const { data } = await api.get<Barber>(`/barbers/${id}`);
    return data;
  },

  /**
   * Create new barber
   * @param request - Barber creation data
   * @returns Created barber with generated ID
   */
  create: async (request: CreateBarberRequest): Promise<Barber> => {
    const { data } = await api.post<Barber>('/barbers', request);
    return data;
  },

  /**
   * Update existing barber
   * @param id - Barber unique identifier
   * @param request - Updated barber data
   * @returns Updated barber
   */
  update: async (id: string, request: UpdateBarberRequest): Promise<Barber> => {
    const { data } = await api.put<Barber>(`/barbers/${id}`, request);
    return data;
  },

  /**
   * Toggle active status of barber (currently only deactivation via DELETE)
   * @param id - Barber unique identifier
   * @param isActive - Desired active status (currently only false is supported)
   */
  toggleActive: async (id: string, isActive: boolean): Promise<void> => {
    if (!isActive) {
      // Deactivate via DELETE
      await api.delete(`/barbers/${id}`);
    } else {
      // Activation not implemented yet
      return Promise.reject(new Error('Activation endpoint not implemented yet'));
    }
  },
};