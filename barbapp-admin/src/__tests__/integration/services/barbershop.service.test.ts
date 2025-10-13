import { describe, it, expect, vi, beforeEach } from 'vitest';
import { barbershopService } from '@/services/barbershop.service';

// Mock the api module
vi.mock('@/services/api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  },
}));

describe('barbershopService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should fetch barbershops with pagination', async () => {
    const { default: mockApi } = await import('@/services/api');
    const mockResponse = {
      items: [
        {
          id: '1',
          name: 'Barbearia Mock',
          document: '12.345.678/0001-90',
          phone: '(11) 99999-9999',
          ownerName: 'João Silva',
          email: 'mock@email.com',
          code: 'ABC123XY',
          address: {
            street: 'Rua Mock',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    (mockApi.get as any).mockResolvedValueOnce({ data: mockResponse });

    const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

    expect(mockApi.get).toHaveBeenCalledWith('/barbearias', {
      params: { pageNumber: 1, pageSize: 20 },
    });
    expect(result.items).toHaveLength(1);
    expect(result.items[0].name).toBe('Barbearia Mock');
    expect(result.pageNumber).toBe(1);
    expect(result.totalCount).toBe(1);
  });

  it('should fetch barbershop by id', async () => {
    const { default: mockApi } = await import('@/services/api');
    const mockResponse = {
      id: '1',
      name: 'Barbearia Específica',
      document: '12.345.678/0001-90',
      phone: '(11) 98888-7777',
      ownerName: 'Maria Silva',
      email: 'especifica@email.com',
      code: 'DEF456ZW',
      address: {
        street: 'Rua Específica',
        number: '456',
        neighborhood: 'Vila',
        city: 'São Paulo',
        state: 'SP',
        zipCode: '02000-000',
      },
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    };

    (mockApi.get as any).mockResolvedValueOnce({ data: mockResponse });

    const result = await barbershopService.getById('1');

    expect(mockApi.get).toHaveBeenCalledWith('/barbearias/1');
    expect(result.name).toBe('Barbearia Específica');
    expect(result.email).toBe('especifica@email.com');
  });

  it('should create new barbershop', async () => {
    const { default: mockApi } = await import('@/services/api');
    const mockRequest = {
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

    const mockResponse = {
      id: 'new-id',
      ...mockRequest,
      code: 'NEW123AB',
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
      address: {
        zipCode: '01000-000',
        street: 'Rua Nova',
        number: '100',
        neighborhood: 'Novo Bairro',
        city: 'São Paulo',
        state: 'SP',
      },
    };

    (mockApi.post as any).mockResolvedValueOnce({ data: mockResponse });

    const result = await barbershopService.create(mockRequest);

    expect(mockApi.post).toHaveBeenCalledWith('/barbearias', mockRequest);
    expect(result.id).toBe('new-id');
    expect(result.name).toBe('Nova Barbearia');
  });

  it('should handle error responses', async () => {
    const { default: mockApi } = await import('@/services/api');
    const error = new Error('API Error');
    (error as any).response = {
      data: { message: 'Barbearia não encontrada' },
      status: 404,
    };

    (mockApi.get as any).mockRejectedValueOnce(error);

    await expect(barbershopService.getById('invalid-id')).rejects.toThrow();
  });
});