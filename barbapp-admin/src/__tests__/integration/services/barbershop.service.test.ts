import { describe, it, expect, vi, beforeEach } from 'vitest';
import { barbershopService } from '../../../services/barbershop.service';
import api from '../../../services/api';

// Mock the API module
vi.mock('../../../services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}));

// Mock data
const mockBarbershops = [
  {
    id: '1',
    name: 'Barbearia Central',
    document: '12345678000123',
    phone: '(11) 99999-9999',
    ownerName: 'João Silva',
    email: 'joao@barbearia.com',
    code: 'ABC123AB',
    address: {
      zipCode: '01234-567',
      street: 'Rua das Barbearias',
      number: '123',
      complement: '',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
    },
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
  {
    id: '2',
    name: 'Barbearia Moderna',
    document: '98765432000198',
    phone: '(11) 88888-8888',
    ownerName: 'Maria Santos',
    email: 'maria@moderna.com',
    code: 'XYZ789AB',
    address: {
      zipCode: '09876-543',
      street: 'Avenida Moderna',
      number: '456',
      complement: 'Sala 2',
      neighborhood: 'Jardins',
      city: 'São Paulo',
      state: 'SP',
    },
    isActive: false,
    createdAt: '2024-01-02T00:00:00Z',
    updatedAt: '2024-01-02T00:00:00Z',
  },
];

describe('barbershopService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  describe('getAll', () => {
    it('should fetch barbershops with pagination', async () => {
      const mockResponse = {
        items: mockBarbershops,
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 2,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      (api.get as any).mockResolvedValue({ data: mockResponse });

      const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

      expect(api.get).toHaveBeenCalledWith('/barbearias', {
        params: { pageNumber: 1, pageSize: 20 },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should filter barbershops by search term', async () => {
      const mockResponse = {
        items: [mockBarbershops[0]],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      (api.get as any).mockResolvedValue({ data: mockResponse });

      const result = await barbershopService.getAll({
        pageNumber: 1,
        pageSize: 20,
        searchTerm: 'Central'
      });

      expect(api.get).toHaveBeenCalledWith('/barbearias', {
        params: { pageNumber: 1, pageSize: 20, searchTerm: 'Central' },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should filter barbershops by active status', async () => {
      const mockResponse = {
        items: [mockBarbershops[0]],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      (api.get as any).mockResolvedValue({ data: mockResponse });

      const result = await barbershopService.getAll({
        pageNumber: 1,
        pageSize: 20,
        isActive: true
      });

      expect(api.get).toHaveBeenCalledWith('/barbearias', {
        params: { pageNumber: 1, pageSize: 20, isActive: true },
      });
      expect(result).toEqual(mockResponse);
    });

    it('should handle empty results', async () => {
      const mockResponse = {
        items: [],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 0,
        totalCount: 0,
        hasPreviousPage: false,
        hasNextPage: false,
      };

      (api.get as any).mockResolvedValue({ data: mockResponse });

      const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

      expect(api.get).toHaveBeenCalledWith('/barbearias', {
        params: { pageNumber: 1, pageSize: 20 },
      });
      expect(result).toEqual(mockResponse);
    });
  });

  describe('getById', () => {
    it('should fetch barbershop by id', async () => {
      (api.get as any).mockResolvedValue({ data: mockBarbershops[0] });

      const result = await barbershopService.getById('1');

      expect(api.get).toHaveBeenCalledWith('/barbearias/1');
      expect(result).toEqual(mockBarbershops[0]);
    });

    it('should throw error for non-existent barbershop', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.get as any).mockRejectedValue(error);

      await expect(barbershopService.getById('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('create', () => {
    it('should create new barbershop', async () => {
      const request = {
        name: 'Nova Barbearia',
        document: '11111111000111',
        phone: '(11) 77777-7777',
        ownerName: 'Carlos Silva',
        email: 'carlos@nova.com',
        zipCode: '03000-000',
        street: 'Rua Nova',
        number: '789',
        complement: '',
        neighborhood: 'Novo Bairro',
        city: 'São Paulo',
        state: 'SP',
      };

      const newBarbershop = {
        ...request,
        id: '3',
        code: 'NEW789AB',
        address: {
          zipCode: request.zipCode,
          street: request.street,
          number: request.number,
          complement: request.complement,
          neighborhood: request.neighborhood,
          city: request.city,
          state: request.state,
        },
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
        updatedAt: '2024-01-01T00:00:00Z',
      };

      (api.post as any).mockResolvedValue({ data: newBarbershop });

      const result = await barbershopService.create(request);

      expect(api.post).toHaveBeenCalledWith('/barbearias', request);
      expect(result).toEqual(newBarbershop);
    });
  });

  describe('update', () => {
    it('should update existing barbershop', async () => {
      const request = {
        id: '1',
        name: 'Barbearia Atualizada',
        phone: '(11) 66666-6666',
        ownerName: 'João Atualizado',
        email: 'joao@atualizada.com',
        zipCode: '04000-000',
        street: 'Rua Atualizada',
        number: '999',
        complement: '',
        neighborhood: 'Bairro Atualizado',
        city: 'São Paulo',
        state: 'SP',
      };

      const updatedBarbershop = {
        ...mockBarbershops[0],
        ...request,
        address: {
          zipCode: request.zipCode,
          street: request.street,
          number: request.number,
          complement: request.complement,
          neighborhood: request.neighborhood,
          city: request.city,
          state: request.state,
        },
        updatedAt: '2024-01-02T00:00:00Z',
      };

      (api.put as any).mockResolvedValue({ data: updatedBarbershop });

      const result = await barbershopService.update('1', request);

      expect(api.put).toHaveBeenCalledWith('/barbearias/1', request);
      expect(result).toEqual(updatedBarbershop);
    });

    it('should throw error when updating non-existent barbershop', async () => {
      const request = {
        id: '999',
        name: 'Barbearia Teste',
        phone: '(11) 99999-9999',
        ownerName: 'João Silva',
        email: 'joao@barbearia.com',
        zipCode: '01234-567',
        street: 'Rua das Barbearias',
        number: '123',
        complement: '',
        neighborhood: 'Centro',
        city: 'São Paulo',
        state: 'SP',
      };

      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.update('999', request)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('deactivate', () => {
    it('should deactivate barbershop', async () => {
      (api.put as any).mockResolvedValue({ data: null });

      await expect(barbershopService.deactivate('1')).resolves.toBeUndefined();

      expect(api.put).toHaveBeenCalledWith('/barbearias/1/desativar');
    });

    it('should throw error when deactivating non-existent barbershop', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.deactivate('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('reactivate', () => {
    it('should reactivate barbershop', async () => {
      (api.put as any).mockResolvedValue({ data: null });

      await expect(barbershopService.reactivate('2')).resolves.toBeUndefined();

      expect(api.put).toHaveBeenCalledWith('/barbearias/2/reativar');
    });

    it('should throw error when reactivating non-existent barbershop', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.reactivate('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });
  });

  describe('error scenarios', () => {
    it('should handle 404 errors for getById', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.get as any).mockRejectedValue(error);

      await expect(barbershopService.getById('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });

    it('should handle 404 errors for update', async () => {
      const request = {
        id: '999',
        name: 'Barbearia Teste',
        phone: '(11) 99999-9999',
        ownerName: 'João Silva',
        email: 'joao@barbearia.com',
        zipCode: '01234-567',
        street: 'Rua das Barbearias',
        number: '123',
        complement: '',
        neighborhood: 'Centro',
        city: 'São Paulo',
        state: 'SP',
      };

      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.update('999', request)).rejects.toMatchObject({
        response: { status: 404 },
      });
    });

    it('should handle 404 errors for deactivate', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.deactivate('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });

    it('should handle 404 errors for reactivate', async () => {
      const error = { response: { status: 404, data: 'Not found' } };
      (api.put as any).mockRejectedValue(error);

      await expect(barbershopService.reactivate('999')).rejects.toMatchObject({
        response: { status: 404 },
      });
    });

    it('should handle network errors', async () => {
      const networkError = new Error('Network Error');
      (api.get as any).mockRejectedValue(networkError);

      await expect(barbershopService.getAll({})).rejects.toThrow('Network Error');
    });

    it('should handle 500 server errors', async () => {
      const request = {
        name: 'Nova Barbearia',
        document: '11111111000111',
        phone: '(11) 77777-7777',
        ownerName: 'Carlos Silva',
        email: 'carlos@nova.com',
        zipCode: '03000-000',
        street: 'Rua Nova',
        number: '789',
        complement: '',
        neighborhood: 'Novo Bairro',
        city: 'São Paulo',
        state: 'SP',
      };

      const error = { response: { status: 500, data: 'Internal Server Error' } };
      (api.post as any).mockRejectedValue(error);

      await expect(barbershopService.create(request)).rejects.toMatchObject({
        response: { status: 500 },
      });
    });
  });
});