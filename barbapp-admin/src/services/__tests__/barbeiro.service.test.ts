/**
 * Testes unitários para barbeiroService
 * Cobertura: métodos CRUD, paginação, filtros, tratamento de erros
 */

import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { barbeiroService } from '../barbeiro.service';
import api from '../api';
import type { Barbeiro, PaginatedResponse } from '@/types';

// Mock do módulo api
vi.mock('../api', () => ({
  default: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe('barbeiroService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('list', () => {
    it('deve listar barbeiros com paginação padrão', async () => {
      // Arrange
      const mockResponse = {
        barbers: [
          {
            id: '1',
            name: 'João Silva',
            email: 'joao@test.com',
            phoneFormatted: '(11) 98765-4321',
            services: [],
            isActive: true,
            createdAt: '2024-01-01T00:00:00Z',
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      // Act
      const result = await barbeiroService.list();

      // Assert
      expect(api.get).toHaveBeenCalledWith('/barbers', {
        params: {
          page: undefined,
          pageSize: undefined,
          searchName: undefined,
          isActive: undefined,
        },
      });
      expect(result.items).toHaveLength(1);
      expect(result.totalCount).toBe(1);
      expect(result.pageNumber).toBe(1);
    });

    it('deve listar barbeiros com filtros aplicados', async () => {
      // Arrange
      const mockResponse = {
        barbers: [],
        totalCount: 0,
        page: 2,
        pageSize: 10,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      // Act
      await barbeiroService.list({
        page: 2,
        pageSize: 10,
        search: 'João',
        isActive: true,
      });

      // Assert
      expect(api.get).toHaveBeenCalledWith('/barbers', {
        params: {
          page: 2,
          pageSize: 10,
          searchName: 'João',
          isActive: true,
        },
      });
    });

    it('deve usar searchName como alias para search', async () => {
      // Arrange
      const mockResponse = {
        barbers: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      // Act
      await barbeiroService.list({
        searchName: 'Maria',
      });

      // Assert
      expect(api.get).toHaveBeenCalledWith('/barbers', {
        params: {
          page: undefined,
          pageSize: undefined,
          searchName: 'Maria',
          isActive: undefined,
        },
      });
    });

    it('deve normalizar corretamente a resposta paginada', async () => {
      // Arrange
      const mockResponse = {
        barbers: Array(15).fill(null).map((_, i) => ({
          id: `${i + 1}`,
          name: `Barbeiro ${i + 1}`,
          email: `barbeiro${i + 1}@test.com`,
          phoneFormatted: '(11) 98765-4321',
          services: [],
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
        })),
        totalCount: 45,
        page: 2,
        pageSize: 15,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      // Act
      const result: PaginatedResponse<Barbeiro> = await barbeiroService.list({
        page: 2,
        pageSize: 15,
      });

      // Assert
      expect(result.items).toHaveLength(15);
      expect(result.totalCount).toBe(45);
      expect(result.totalPages).toBe(3); // 45 / 15 = 3
      expect(result.pageNumber).toBe(2);
      expect(result.hasPreviousPage).toBe(true);
      expect(result.hasNextPage).toBe(true);
    });

    it('deve tratar resposta vazia corretamente', async () => {
      // Arrange
      const mockResponse = {
        barbers: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      // Act
      const result = await barbeiroService.list();

      // Assert
      expect(result.items).toHaveLength(0);
      expect(result.totalCount).toBe(0);
      expect(result.totalPages).toBe(0);
      expect(result.hasPreviousPage).toBe(false);
      expect(result.hasNextPage).toBe(false);
    });
  });

  describe('getById', () => {
    it('deve buscar barbeiro por ID', async () => {
      // Arrange
      const mockBarbeiro: Barbeiro = {
        id: '123e4567-e89b-12d3-a456-426614174000',
        name: 'João Silva',
        email: 'joao@test.com',
        phoneFormatted: '(11) 98765-4321',
        services: [
          { id: 'service-1', name: 'Corte' },
          { id: 'service-2', name: 'Barba' },
        ],
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockBarbeiro });

      // Act
      const result = await barbeiroService.getById('123e4567-e89b-12d3-a456-426614174000');

      // Assert
      expect(api.get).toHaveBeenCalledWith('/barbers/123e4567-e89b-12d3-a456-426614174000');
      expect(result).toEqual(mockBarbeiro);
      expect(result.services).toHaveLength(2);
    });

    it('deve lançar erro quando barbeiro não encontrado', async () => {
      // Arrange
      vi.mocked(api.get).mockRejectedValueOnce({
        response: {
          status: 404,
          data: { message: 'Barbeiro não encontrado' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.getById('invalid-id')).rejects.toMatchObject({
        response: {
          status: 404,
        },
      });
    });
  });

  describe('create', () => {
    it('deve criar novo barbeiro', async () => {
      // Arrange
      const input = {
        name: 'João Silva',
        email: 'joao@test.com',
        phone: '(11) 98765-4321',
        password: 'senha123',
        serviceIds: ['service-1', 'service-2'],
      };

      const mockCreated: Barbeiro = {
        id: 'new-uuid',
        name: input.name,
        email: input.email,
        phoneFormatted: input.phone,
        services: [
          { id: 'service-1', name: 'Corte' },
          { id: 'service-2', name: 'Barba' },
        ],
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(api.post).mockResolvedValueOnce({ data: mockCreated });

      // Act
      const result = await barbeiroService.create(input);

      // Assert
      expect(api.post).toHaveBeenCalledWith('/barbers', input);
      expect(result).toEqual(mockCreated);
      expect(result.id).toBe('new-uuid');
    });

    it('deve lançar erro quando email já existe', async () => {
      // Arrange
      const input = {
        name: 'João Silva',
        email: 'joao@test.com',
        phone: '(11) 98765-4321',
        password: 'senha123',
        serviceIds: ['service-1'],
      };

      vi.mocked(api.post).mockRejectedValueOnce({
        response: {
          status: 409,
          data: { message: 'Email já cadastrado' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.create(input)).rejects.toMatchObject({
        response: {
          status: 409,
        },
      });
    });

    it('deve lançar erro quando validação falhar', async () => {
      // Arrange
      const input = {
        name: 'Jo', // muito curto
        email: 'invalid-email',
        phone: '123',
        password: '123',
        serviceIds: [],
      };

      vi.mocked(api.post).mockRejectedValueOnce({
        response: {
          status: 400,
          data: { errors: ['Nome inválido', 'Email inválido'] },
        },
      });

      // Act & Assert
      await expect(barbeiroService.create(input)).rejects.toMatchObject({
        response: {
          status: 400,
        },
      });
    });
  });

  describe('update', () => {
    it('deve atualizar barbeiro existente', async () => {
      // Arrange
      const id = '123e4567-e89b-12d3-a456-426614174000';
      const input = {
        name: 'João da Silva',
        phone: '(11) 91234-5678',
        serviceIds: ['service-1'],
      };

      const mockUpdated: Barbeiro = {
        id,
        name: input.name,
        email: 'joao@test.com',
        phoneFormatted: input.phone,
        services: [{ id: 'service-1', name: 'Corte' }],
        isActive: true,
        createdAt: '2024-01-01T00:00:00Z',
      };

      vi.mocked(api.put).mockResolvedValueOnce({ data: mockUpdated });

      // Act
      const result = await barbeiroService.update(id, input);

      // Assert
      expect(api.put).toHaveBeenCalledWith(`/barbers/${id}`, input);
      expect(result).toEqual(mockUpdated);
      expect(result.name).toBe('João da Silva');
    });

    it('deve lançar erro quando barbeiro não pertence à barbearia', async () => {
      // Arrange
      const id = 'other-barbershop-barber-id';
      const input = {
        name: 'João da Silva',
        phone: '(11) 91234-5678',
        serviceIds: ['service-1'],
      };

      vi.mocked(api.put).mockRejectedValueOnce({
        response: {
          status: 403,
          data: { message: 'Acesso negado' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.update(id, input)).rejects.toMatchObject({
        response: {
          status: 403,
        },
      });
    });
  });

  describe('deactivate', () => {
    it('deve desativar barbeiro', async () => {
      // Arrange
      const id = '123e4567-e89b-12d3-a456-426614174000';
      vi.mocked(api.put).mockResolvedValueOnce({ data: undefined });

      // Act
      await barbeiroService.deactivate(id);

      // Assert
      expect(api.put).toHaveBeenCalledWith(`/barbers/${id}/deactivate`);
    });

    it('deve lançar erro quando barbeiro não encontrado', async () => {
      // Arrange
      const id = 'invalid-id';
      vi.mocked(api.put).mockRejectedValueOnce({
        response: {
          status: 404,
          data: { message: 'Barbeiro não encontrado' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.deactivate(id)).rejects.toMatchObject({
        response: {
          status: 404,
        },
      });
    });
  });

  describe('reactivate', () => {
    it('deve reativar barbeiro', async () => {
      // Arrange
      const id = '123e4567-e89b-12d3-a456-426614174000';
      vi.mocked(api.put).mockResolvedValueOnce({ data: undefined });

      // Act
      await barbeiroService.reactivate(id);

      // Assert
      expect(api.put).toHaveBeenCalledWith(`/barbers/${id}/reactivate`);
    });

    it('deve lançar erro quando barbeiro não encontrado', async () => {
      // Arrange
      const id = 'invalid-id';
      vi.mocked(api.put).mockRejectedValueOnce({
        response: {
          status: 404,
          data: { message: 'Barbeiro não encontrado' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.reactivate(id)).rejects.toMatchObject({
        response: {
          status: 404,
        },
      });
    });
  });

  describe('toggleActive', () => {
    it('deve desativar barbeiro quando isActive é false', async () => {
      // Arrange
      const id = '123e4567-e89b-12d3-a456-426614174000';
      vi.mocked(api.put).mockResolvedValueOnce({ data: undefined });

      // Act
      await barbeiroService.toggleActive(id, false);

      // Assert
      expect(api.put).toHaveBeenCalledWith(`/barbers/${id}/deactivate`);
    });

    it('deve reativar barbeiro quando isActive é true', async () => {
      // Arrange
      const id = '123e4567-e89b-12d3-a456-426614174000';
      vi.mocked(api.put).mockResolvedValueOnce({ data: undefined });

      // Act
      await barbeiroService.toggleActive(id, true);

      // Assert
      expect(api.put).toHaveBeenCalledWith(`/barbers/${id}/reactivate`);
    });
  });

  describe('Tratamento de erros gerais', () => {
    it('deve propagar erro de rede', async () => {
      // Arrange
      vi.mocked(api.get).mockRejectedValueOnce(new Error('Network Error'));

      // Act & Assert
      await expect(barbeiroService.list()).rejects.toThrow('Network Error');
    });

    it('deve propagar erro 500 do servidor', async () => {
      // Arrange
      vi.mocked(api.get).mockRejectedValueOnce({
        response: {
          status: 500,
          data: { message: 'Internal Server Error' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.list()).rejects.toMatchObject({
        response: {
          status: 500,
        },
      });
    });

    it('deve propagar erro 401 de autenticação', async () => {
      // Arrange
      vi.mocked(api.get).mockRejectedValueOnce({
        response: {
          status: 401,
          data: { message: 'Token inválido' },
        },
      });

      // Act & Assert
      await expect(barbeiroService.list()).rejects.toMatchObject({
        response: {
          status: 401,
        },
      });
    });
  });
});
