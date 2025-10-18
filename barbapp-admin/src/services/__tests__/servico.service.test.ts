/**
 * Testes unitários para servicoService
 * Cobertura: métodos CRUD, validações, tratamento de erros
 */

import { describe, it, expect, vi, beforeEach } from 'vitest';
import { servicoService } from '../servico.service';
import api from '../api';
import type { BarbershopService } from '@/types';

// Mock do módulo api
vi.mock('../api');

describe('servicoService', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  const mockServico: BarbershopService = {
    id: '123e4567-e89b-12d3-a456-426614174000',
    name: 'Corte de Cabelo',
    description: 'Corte masculino tradicional',
    durationMinutes: 30,
    price: 35.0,
    isActive: true,
  };

  describe('list', () => {
    it('should list services with default parameters', async () => {
      const mockResponse = {
        services: [mockServico],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      const result = await servicoService.list();

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: {
          page: undefined,
          pageSize: undefined,
          searchName: undefined,
          isActive: undefined,
        },
      });

      expect(result).toEqual({
        items: [mockServico],
        pageNumber: 1,
        pageSize: 20,
        totalPages: 1,
        totalCount: 1,
        hasPreviousPage: false,
        hasNextPage: false,
      });
    });

    it('should list services with custom parameters', async () => {
      const mockResponse = {
        services: [mockServico],
        totalCount: 50,
        page: 2,
        pageSize: 10,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      const result = await servicoService.list({
        page: 2,
        pageSize: 10,
        isActive: true,
        search: 'Corte',
      });

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: {
          page: 2,
          pageSize: 10,
          searchName: 'Corte',
          isActive: true,
        },
      });

      expect(result.pageNumber).toBe(2);
      expect(result.totalPages).toBe(5); // 50 / 10
      expect(result.hasPreviousPage).toBe(true);
      expect(result.hasNextPage).toBe(true);
    });

    it('should handle searchName alias', async () => {
      const mockResponse = {
        services: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      await servicoService.list({ searchName: 'Barba' });

      expect(api.get).toHaveBeenCalledWith('/barbershop-services', {
        params: {
          page: undefined,
          pageSize: undefined,
          searchName: 'Barba',
          isActive: undefined,
        },
      });
    });

    it('should calculate pagination correctly for last page', async () => {
      const mockResponse = {
        services: [mockServico],
        totalCount: 21,
        page: 3,
        pageSize: 10,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      const result = await servicoService.list({ page: 3, pageSize: 10 });

      expect(result.totalPages).toBe(3); // ceil(21 / 10)
      expect(result.hasPreviousPage).toBe(true);
      expect(result.hasNextPage).toBe(false);
    });
  });

  describe('getById', () => {
    it('should get service by id', async () => {
      vi.mocked(api.get).mockResolvedValueOnce({ data: mockServico });

      const result = await servicoService.getById(mockServico.id);

      expect(api.get).toHaveBeenCalledWith(`/barbershop-services/${mockServico.id}`);
      expect(result).toEqual(mockServico);
    });

    it('should throw error when service not found', async () => {
      vi.mocked(api.get).mockRejectedValueOnce({
        response: { status: 404, data: { message: 'Service not found' } },
      });

      await expect(servicoService.getById('invalid-id')).rejects.toThrow();
    });
  });

  describe('create', () => {
    it('should create new service', async () => {
      const input = {
        name: 'Corte + Barba',
        description: 'Combo completo',
        durationMinutes: 60,
        price: 50.0,
      };

      const createdServico = { ...mockServico, ...input };
      vi.mocked(api.post).mockResolvedValueOnce({ data: createdServico });

      const result = await servicoService.create(input);

      expect(api.post).toHaveBeenCalledWith('/barbershop-services', input);
      expect(result).toEqual(createdServico);
    });

    it('should throw error when name already exists', async () => {
      const input = {
        name: 'Corte de Cabelo',
        description: '',
        durationMinutes: 30,
        price: 35.0,
      };

      vi.mocked(api.post).mockRejectedValueOnce({
        response: { status: 409, data: { message: 'Service name already exists' } },
      });

      await expect(servicoService.create(input)).rejects.toThrow();
    });
  });

  describe('update', () => {
    it('should update service', async () => {
      const input = {
        name: 'Corte Premium',
        description: 'Corte masculino premium',
        durationMinutes: 30,
        price: 45.0,
      };

      const updatedServico = { ...mockServico, ...input };
      vi.mocked(api.put).mockResolvedValueOnce({ data: updatedServico });

      const result = await servicoService.update(mockServico.id, input);

      expect(api.put).toHaveBeenCalledWith(`/barbershop-services/${mockServico.id}`, input);
      expect(result).toEqual(updatedServico);
    });

    it('should throw error when service not found', async () => {
      const input = {
        name: 'Test',
        description: 'Test description',
        durationMinutes: 30,
        price: 35.0,
      };

      vi.mocked(api.put).mockRejectedValueOnce({
        response: { status: 404, data: { message: 'Service not found' } },
      });

      await expect(servicoService.update('invalid-id', input)).rejects.toThrow();
    });
  });

  describe('deactivate', () => {
    it('should deactivate service', async () => {
      vi.mocked(api.delete).mockResolvedValueOnce({ data: null });

      await servicoService.deactivate(mockServico.id);

      expect(api.delete).toHaveBeenCalledWith(`/barbershop-services/${mockServico.id}`);
    });

    it('should throw error when service not found', async () => {
      vi.mocked(api.delete).mockRejectedValueOnce({
        response: { status: 404, data: { message: 'Service not found' } },
      });

      await expect(servicoService.deactivate('invalid-id')).rejects.toThrow();
    });
  });

  describe('reactivate', () => {
    it('should throw error as endpoint is not implemented', async () => {
      await expect(servicoService.reactivate(mockServico.id)).rejects.toThrow(
        /Endpoint de reativação não implementado/
      );
    });
  });

  describe('toggleActive', () => {
    it('should deactivate service when isActive is false', async () => {
      vi.mocked(api.delete).mockResolvedValueOnce({ data: null });

      await servicoService.toggleActive(mockServico.id, false);

      expect(api.delete).toHaveBeenCalledWith(`/barbershop-services/${mockServico.id}`);
    });

    it('should throw error when trying to activate (isActive is true)', async () => {
      await expect(servicoService.toggleActive(mockServico.id, true)).rejects.toThrow(
        /Endpoint de reativação não implementado/
      );
    });
  });

  describe('edge cases', () => {
    it('should handle empty list response', async () => {
      const mockResponse = {
        services: [],
        totalCount: 0,
        page: 1,
        pageSize: 20,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      const result = await servicoService.list();

      expect(result.items).toEqual([]);
      expect(result.totalCount).toBe(0);
      expect(result.totalPages).toBe(0);
    });

    it('should handle missing optional fields in response', async () => {
      const mockResponse = {
        services: [mockServico],
        totalCount: 0,
        page: 0,
        pageSize: 0,
      };

      vi.mocked(api.get).mockResolvedValueOnce({ data: mockResponse });

      const result = await servicoService.list();

      expect(result.pageNumber).toBe(1); // Default fallback
      expect(result.pageSize).toBe(20); // Default fallback
    });
  });
});
