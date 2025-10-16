import api from './api';
import type {
  BarbershopService,
  CreateServiceRequest,
  UpdateServiceRequest,
  PaginatedResponse,
  ServiceFilters,
} from '@/types';

// Interface for API list response
interface ServicesListResponse {
  services: BarbershopService[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Helper function to normalize API response to expected format
function normalizePaginatedResponse(data: ServicesListResponse): PaginatedResponse<BarbershopService> {
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

export const servicesService = {
  /**
   * List services with pagination and filters
   * @param filters - Query parameters for filtering and pagination
   * @returns Paginated list of services
   */
  list: async (filters: ServiceFilters): Promise<PaginatedResponse<BarbershopService>> => {
    const { data } = await api.get<ServicesListResponse>('/barbershop-services', {
      params: filters,
    });
    return normalizePaginatedResponse(data);
  },

  /**
   * Get service by ID
   * @param id - Service unique identifier
   * @returns Service details
   */
  getById: async (id: string): Promise<BarbershopService> => {
    const { data } = await api.get<BarbershopService>(`/barbershop-services/${id}`);
    return data;
  },

  /**
   * Create new service
   * @param request - Service creation data
   * @returns Created service with generated ID
   */
  create: async (request: CreateServiceRequest): Promise<BarbershopService> => {
    const { data } = await api.post<BarbershopService>('/barbershop-services', request);
    return data;
  },

  /**
   * Update existing service
   * @param id - Service unique identifier
   * @param request - Updated service data
   * @returns Updated service
   */
  update: async (id: string, request: UpdateServiceRequest): Promise<BarbershopService> => {
    const { data } = await api.put<BarbershopService>(`/barbershop-services/${id}`, request);
    return data;
  },

  /**
   * Toggle active status of service (currently only deactivation via DELETE)
   * @param id - Service unique identifier
   * @param isActive - Desired active status (currently only false is supported)
   */
  toggleActive: async (id: string, isActive: boolean): Promise<void> => {
    if (!isActive) {
      // Deactivate via DELETE
      await api.delete(`/barbershop-services/${id}`);
    } else {
      // Activation not implemented yet
      return Promise.reject(new Error('Activation endpoint not implemented yet'));
    }
  },
};