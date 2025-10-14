import { describe, it, expect, beforeAll, afterEach, vi } from 'vitest';
import type { CreateBarbershopRequest, UpdateBarbershopRequest } from '@/types';

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

// Mock the api module
vi.mock('@/services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
  },
}));

// Now import the service after the mock
import { barbershopService } from '@/services/barbershop.service';
import api from '@/services/api';

// Mock browser APIs
Object.defineProperty(window, 'localStorage', {
  value: {
    getItem: vi.fn(() => null),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  },
  writable: true,
});

Object.defineProperty(window, 'sessionStorage', {
  value: {
    getItem: vi.fn(() => null),
    setItem: vi.fn(),
    removeItem: vi.fn(),
    clear: vi.fn(),
  },
  writable: true,
});

delete (window as any).location;
(window as any).location = {
  href: '',
};

beforeAll(() => {
  // Setup default mock implementations
  (api.get as any).mockImplementation((url: string, config?: any) => {
    if (url === '/barbearias') {
      const params = config?.params || {};
      let filteredBarbershops = [...mockBarbershops];

      // Filter by search term
      if (params.searchTerm) {
        filteredBarbershops = filteredBarbershops.filter(
          (barbershop) =>
            barbershop.name.toLowerCase().includes(params.searchTerm.toLowerCase()) ||
            barbershop.email.toLowerCase().includes(params.searchTerm.toLowerCase()) ||
            barbershop.address.city.toLowerCase().includes(params.searchTerm.toLowerCase())
        );
      }

      // Filter by active status
      if (params.isActive !== undefined) {
        filteredBarbershops = filteredBarbershops.filter(
          (barbershop) => barbershop.isActive === params.isActive
        );
      }

      // Pagination
      const pageNumber = params.pageNumber || 1;
      const pageSize = params.pageSize || 20;
      const totalCount = filteredBarbershops.length;
      const totalPages = Math.ceil(totalCount / pageSize);
      const startIndex = (pageNumber - 1) * pageSize;
      const paginatedItems = filteredBarbershops.slice(startIndex, startIndex + pageSize);

      return Promise.resolve({
        data: {
          items: paginatedItems,
          pageNumber,
          pageSize,
          totalPages,
          totalCount,
          hasPreviousPage: pageNumber > 1,
          hasNextPage: pageNumber < totalPages,
        },
      });
    }

    if (url.startsWith('/barbearias/') && !url.includes('/desativar') && !url.includes('/reativar')) {
      const id = url.split('/')[2];
      const barbershop = mockBarbershops.find((b) => b.id === id);
      if (barbershop) {
        return Promise.resolve({ data: barbershop });
      } else {
        return Promise.reject({ response: { status: 404 } });
      }
    }

    return Promise.reject(new Error('Unexpected URL'));
  });

  (api.post as any).mockImplementation((url: string, data: any) => {
    if (url === '/barbearias') {
      const newBarbershop = {
        id: Date.now().toString(),
        name: data.name,
        document: data.document,
        phone: data.phone,
        ownerName: data.ownerName,
        email: data.email,
        code: `NEW${Date.now().toString().slice(-6)}AB`,
        address: {
          zipCode: data.zipCode,
          street: data.street,
          number: data.number,
          complement: data.complement || '',
          neighborhood: data.neighborhood,
          city: data.city,
          state: data.state,
        },
        isActive: true,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString(),
      };
      mockBarbershops.push(newBarbershop);
      return Promise.resolve({ data: newBarbershop });
    }
    return Promise.reject(new Error('Unexpected URL'));
  });

  (api.put as any).mockImplementation((url: string, data?: any) => {
    if (url.startsWith('/barbearias/') && url.endsWith('/desativar')) {
      const id = url.split('/')[2];
      const barbershop = mockBarbershops.find((b) => b.id === id);
      if (barbershop) {
        barbershop.isActive = false;
        barbershop.updatedAt = new Date().toISOString();
        return Promise.resolve({ data: null });
      } else {
        return Promise.reject({ response: { status: 404 } });
      }
    }

    if (url.startsWith('/barbearias/') && url.endsWith('/reativar')) {
      const id = url.split('/')[2];
      const barbershop = mockBarbershops.find((b) => b.id === id);
      if (barbershop) {
        barbershop.isActive = true;
        barbershop.updatedAt = new Date().toISOString();
        return Promise.resolve({ data: null });
      } else {
        return Promise.reject({ response: { status: 404 } });
      }
    }

    if (url.startsWith('/barbearias/') && !url.includes('/desativar') && !url.includes('/reativar')) {
      const id = url.split('/')[2];
      const index = mockBarbershops.findIndex((b) => b.id === id);
      if (index !== -1) {
        mockBarbershops[index] = {
          ...mockBarbershops[index],
          name: data.name,
          phone: data.phone,
          ownerName: data.ownerName,
          email: data.email,
          address: {
            zipCode: data.zipCode,
            street: data.street,
            number: data.number,
            complement: data.complement || '',
            neighborhood: data.neighborhood,
            city: data.city,
            state: data.state,
          },
          updatedAt: new Date().toISOString(),
        };
        return Promise.resolve({ data: mockBarbershops[index] });
      } else {
        return Promise.reject({ response: { status: 404 } });
      }
    }

    return Promise.reject(new Error('Unexpected URL'));
  });
});

afterEach(() => {
  vi.clearAllMocks();
  // Reset mock data to initial state
  mockBarbershops.splice(2); // Keep only the first 2 items
  mockBarbershops[0].isActive = true;
  mockBarbershops[1].isActive = false;
});

describe('barbershopService', () => {
  describe('getAll', () => {
    it('should fetch barbershops with pagination', async () => {
      const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

      expect(result.items).toHaveLength(2);
      expect(result.items[0].name).toBe('Barbearia Central');
      expect(result.items[0].document).toBe('12345678000123');
      expect(result.items[0].code).toBe('ABC123AB');
      expect(result.pageNumber).toBe(1);
      expect(result.totalCount).toBe(2);
      expect(result.hasNextPage).toBe(false);
    });

    it('should filter barbershops by search term', async () => {
      const result = await barbershopService.getAll({
        pageNumber: 1,
        pageSize: 20,
        searchTerm: 'Central'
      });

      expect(result.items).toHaveLength(1);
      expect(result.items[0].name).toBe('Barbearia Central');
      expect(result.totalCount).toBe(1);
    });

    it('should filter barbershops by active status', async () => {
      const result = await barbershopService.getAll({
        pageNumber: 1,
        pageSize: 20,
        isActive: false
      });

      expect(result.items).toHaveLength(1);
      expect(result.items[0].name).toBe('Barbearia Moderna');
      expect(result.items[0].isActive).toBe(false);
    });

    it('should handle empty results', async () => {
      const result = await barbershopService.getAll({
        pageNumber: 1,
        pageSize: 20,
        searchTerm: 'NonExistent'
      });

      expect(result.items).toHaveLength(0);
      expect(result.totalCount).toBe(0);
    });
  });

  describe('getById', () => {
    it('should fetch barbershop by id', async () => {
      const result = await barbershopService.getById('1');

      expect(result.id).toBe('1');
      expect(result.name).toBe('Barbearia Central');
      expect(result.document).toBe('12345678000123');
      expect(result.ownerName).toBe('João Silva');
      expect(result.code).toBe('ABC123AB');
      expect(result.isActive).toBe(true);
    });

    it('should throw error for non-existent barbershop', async () => {
      await expect(barbershopService.getById('999')).rejects.toThrow();
    });
  });

  describe('create', () => {
    it('should create new barbershop', async () => {
      const request: CreateBarbershopRequest = {
        name: 'Nova Barbearia',
        document: '12.345.678/0001-90',
        phone: '(11) 99999-9999',
        ownerName: 'José Silva',
        email: 'nova@email.com',
        zipCode: '01000-000',
        street: 'Rua Nova',
        number: '100',
        neighborhood: 'Novo Bairro',
        city: 'São Paulo',
        state: 'SP',
      };

      const result = await barbershopService.create(request);

      expect(result.name).toBe('Nova Barbearia');
      expect(result.document).toBe('12.345.678/0001-90');
      expect(result.ownerName).toBe('José Silva');
      expect(result.email).toBe('nova@email.com');
      expect(result.code).toMatch(/^NEW\d+AB$/);
      expect(result.isActive).toBe(true);
      expect(result.address.zipCode).toBe('01000-000');
      expect(result.address.street).toBe('Rua Nova');
    });
  });

  describe('update', () => {
    it('should update existing barbershop', async () => {
      const request: UpdateBarbershopRequest = {
        id: '1',
        name: 'Barbearia Atualizada',
        phone: '(11) 99999-9999',
        ownerName: 'João Silva Atualizado',
        email: 'atualizada@email.com',
        zipCode: '01000-000',
        street: 'Rua Atualizada',
        number: '123',
        neighborhood: 'Centro',
        city: 'São Paulo',
        state: 'SP',
      };

      const result = await barbershopService.update('1', request);

      expect(result.id).toBe('1');
      expect(result.name).toBe('Barbearia Atualizada');
      expect(result.ownerName).toBe('João Silva Atualizado');
      expect(result.email).toBe('atualizada@email.com');
      expect(result.address.street).toBe('Rua Atualizada');
    });

    it('should throw error when updating non-existent barbershop', async () => {
      const request: UpdateBarbershopRequest = {
        id: '999',
        name: 'Teste',
        phone: '(11) 99999-9999',
        ownerName: 'Teste',
        email: 'teste@email.com',
        zipCode: '01000-000',
        street: 'Rua Teste',
        number: '123',
        neighborhood: 'Centro',
        city: 'São Paulo',
        state: 'SP',
      };

      await expect(barbershopService.update('999', request)).rejects.toThrow();
    });
  });

  describe('deactivate', () => {
    it('should deactivate barbershop', async () => {
      await expect(barbershopService.deactivate('1')).resolves.toBeUndefined();
    });

    it('should throw error when deactivating non-existent barbershop', async () => {
      await expect(barbershopService.deactivate('999')).rejects.toThrow();
    });
  });

  describe('reactivate', () => {
    it('should reactivate barbershop', async () => {
      await expect(barbershopService.reactivate('2')).resolves.toBeUndefined();
    });

    it('should throw error when reactivating non-existent barbershop', async () => {
      await expect(barbershopService.reactivate('999')).rejects.toThrow();
    });
  });

  describe('error scenarios', () => {
    it('should handle network errors', async () => {
      // Mock network error
      (api.get as any).mockRejectedValueOnce(new Error('Network Error'));

      await expect(barbershopService.getAll({})).rejects.toThrow('Network Error');
    });

    it('should handle 404 errors for getById', async () => {
      // Mock 404 error for getById
      (api.get as any).mockRejectedValueOnce({ response: { status: 404 } });

      await expect(barbershopService.getById('999')).rejects.toThrow();
    });
  });
});