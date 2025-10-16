import { describe, it, expect, vi, beforeEach } from 'vitest';
import { servicesService } from '../services.service';
import api from '../api';

// Mock the API module
vi.mock('../api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

// Get typed references to the mocked functions
const mockApiGet = vi.mocked(api.get);
const mockApiPost = vi.mocked(api.post);
const mockApiPut = vi.mocked(api.put);
const mockApiDelete = vi.mocked(api.delete);

// Mock data
const mockServices = [
  {
    id: '1',
    name: 'Corte de Cabelo',
    description: 'Corte masculino completo',
    durationMinutes: 30,
    price: 25.00,
    isActive: true,
  },
  {
    id: '2',
    name: 'Barba',
    description: 'Aparação e modelagem de barba',
    durationMinutes: 20,
    price: 15.00,
    isActive: true,
  },
  {
    id: '3',
    name: 'Corte Infantil',
    description: 'Corte para crianças',
    durationMinutes: 25,
    price: 20.00,
    isActive: false,
  },
];

describe('Services Service', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('list', () => {
    it('should list services with pagination', async () => {
      const mockResponse = {
        services: mockServices,
        totalCount: 3,
        page: 1,
        pageSize: 20,
      };

      mockApiGet.mockResolvedValue({ data: mockResponse });

      const filters = { page: 1, pageSize: 10 };
      const result = await servicesService.list(filters);

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: filters,
      });
      expect(result).toEqual({
        items: mockServices,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 3,
        hasPreviousPage: false,
        hasNextPage: false,
      });
    });

    it('should filter services by name', async () => {
      const mockResponse = {
        services: [mockServices[0]],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      };

      mockApiGet.mockResolvedValue({ data: mockResponse });

      const filters = { searchName: 'Corte' };
      const result = await servicesService.list(filters);

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: filters,
      });
      expect(result.items).toHaveLength(1);
      expect(result.items[0].name.toLowerCase()).toContain('corte');
    });

    it('should filter services by active status', async () => {
      const mockResponse = {
        services: [mockServices[0], mockServices[1]],
        totalCount: 2,
        page: 1,
        pageSize: 20,
      };

      mockApiGet.mockResolvedValue({ data: mockResponse });

      const filters = { isActive: true };
      const result = await servicesService.list(filters);

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: filters,
      });
      expect(result.items).toHaveLength(2);
      result.items.forEach((service) => {
        expect(service.isActive).toBe(true);
      });
    });
  });

  describe('getById', () => {
    it('should get service by ID', async () => {
      mockApiGet.mockResolvedValue({ data: mockServices[0] });

      const serviceId = '1';
      const result = await servicesService.getById(serviceId);

      expect(api.get).toHaveBeenCalledWith(`/barbershop-services/${serviceId}`);
      expect(result).toEqual(mockServices[0]);
    });

    it('should throw error for non-existent service', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      mockApiGet.mockRejectedValue(error);

      const serviceId = 'non-existent';
      await expect(servicesService.getById(serviceId)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('create', () => {
    it('should create a new service', async () => {
      const request = {
        name: 'Novo Serviço',
        description: 'Descrição do novo serviço',
        durationMinutes: 45,
        price: 35.00,
      };

      const newService = {
        id: '4',
        ...request,
        isActive: true,
      };

      mockApiPost.mockResolvedValue({ data: newService });

      const result = await servicesService.create(request);

      expect(api.post).toHaveBeenCalledWith('/barbershop-services', request);
      expect(result).toEqual(newService);
    });
  });

  describe('update', () => {
    it('should update an existing service', async () => {
      const serviceId = '1';
      const request = {
        name: 'Corte Atualizado',
        description: 'Descrição atualizada',
        durationMinutes: 35,
        price: 30.00,
      };

      const updatedService = {
        id: serviceId,
        ...request,
        isActive: true,
      };

      mockApiPut.mockResolvedValue({ data: updatedService });

      const result = await servicesService.update(serviceId, request);

      expect(api.put).toHaveBeenCalledWith(`/barbershop-services/${serviceId}`, request);
      expect(result).toEqual(updatedService);
    });

    it('should throw error when updating non-existent service', async () => {
      const serviceId = 'non-existent';
      const request = {
        name: 'Teste',
        description: 'Teste',
        durationMinutes: 30,
        price: 25.00,
      };

      const error = { response: { status: 404, data: 'Not found' } };
      mockApiPut.mockRejectedValue(error);

      await expect(servicesService.update(serviceId, request)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('toggleActive', () => {
    it('should deactivate service (currently maps to DELETE)', async () => {
      const serviceId = '1';
      mockApiDelete.mockResolvedValue({ data: null });

      await expect(servicesService.toggleActive(serviceId, false)).resolves.toBeUndefined();

      expect(api.delete).toHaveBeenCalledWith(`/barbershop-services/${serviceId}`);
    });

    it('should handle deactivation of non-existent service', async () => {
      const serviceId = 'non-existent';
      const error = { response: { status: 404, data: 'Not found' } };
      mockApiDelete.mockRejectedValue(error);

      await expect(servicesService.toggleActive(serviceId, false)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });
});