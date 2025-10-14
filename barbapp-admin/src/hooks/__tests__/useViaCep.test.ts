import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { useViaCep } from '../useViaCep';
import { ViaCepError } from '@/services/viacep.service';
import * as viaCepService from '@/services/viacep.service';

vi.mock('@/services/viacep.service', async () => {
  const actual = await vi.importActual<typeof viaCepService>('@/services/viacep.service');
  return {
    ...actual,
    fetchAddressByCep: vi.fn(),
  };
});

describe('useViaCep Hook', () => {
  const mockFetchAddressByCep = vi.fn();

  beforeEach(() => {
    vi.clearAllMocks();
    // Reset the mock implementation
    mockFetchAddressByCep.mockReset();
    // Mock the service function
    vi.mocked(viaCepService.fetchAddressByCep).mockImplementation(mockFetchAddressByCep);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it('should initialize with default values', () => {
    const { result } = renderHook(() => useViaCep());

    expect(result.current.loading).toBe(false);
    expect(result.current.error).toBeNull();
    expect(result.current.data).toBeNull();
  });

  it('should fetch address successfully', async () => {
    const mockAddressData = {
      street: 'Avenida Paulista',
      neighborhood: 'Bela Vista',
      city: 'São Paulo',
      state: 'SP',
      cep: '01310-100',
    };

    mockFetchAddressByCep.mockResolvedValueOnce(mockAddressData);

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('01310-100');

    // Wait for the operation to complete
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.data).toEqual(mockAddressData);
      expect(result.current.error).toBeNull();
    });
  });

  it('should handle errors from service', async () => {
    const mockError = new ViaCepError('CEP não encontrado.', 'NOT_FOUND');
    mockFetchAddressByCep.mockRejectedValueOnce(mockError);

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('99999-999');

    // Wait for the operation to complete with error
    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe('CEP não encontrado.');
      expect(result.current.data).toBeNull();
    });
  });

  it('should handle generic errors', async () => {
    mockFetchAddressByCep.mockRejectedValueOnce(new Error('Network error'));

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('01310-100');

    await waitFor(() => {
      expect(result.current.loading).toBe(false);
      expect(result.current.error).toBe('Erro ao buscar CEP. Tente novamente.');
      expect(result.current.data).toBeNull();
    });
  });

  it('should not search with incomplete CEP', async () => {
    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('01310');

    expect(mockFetchAddressByCep).not.toHaveBeenCalled();
    expect(result.current.loading).toBe(false);
  });

  it('should not search with empty CEP', async () => {
    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('');

    expect(mockFetchAddressByCep).not.toHaveBeenCalled();
  });

  it('should clear error', async () => {
    const mockError = new ViaCepError('CEP não encontrado.', 'NOT_FOUND');
    mockFetchAddressByCep.mockRejectedValueOnce(mockError);

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('99999-999');

    // Wait for error to be set first
    await waitFor(() => {
      expect(result.current.error).toBe('CEP não encontrado.');
    });

    result.current.clearError();

    // Wait for error to be cleared
    await waitFor(() => {
      expect(result.current.error).toBeNull();
    });
  });

  it('should clear data', async () => {
    const mockAddressData = {
      street: 'Avenida Paulista',
      neighborhood: 'Bela Vista',
      city: 'São Paulo',
      state: 'SP',
      cep: '01310-100',
    };

    mockFetchAddressByCep.mockResolvedValueOnce(mockAddressData);

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('01310-100');

    await waitFor(() => {
      expect(result.current.data).toEqual(mockAddressData);
    });

    result.current.clearData();

    await waitFor(() => {
      expect(result.current.data).toBeNull();
    });
  });

  it('should clear previous data when searching again', async () => {
    const mockAddressData = {
      street: 'Avenida Paulista',
      neighborhood: 'Bela Vista',
      city: 'São Paulo',
      state: 'SP',
      cep: '01310-100',
    };

    mockFetchAddressByCep.mockResolvedValueOnce(mockAddressData);

    const { result } = renderHook(() => useViaCep());

    result.current.searchCep('01310-100');

    // Wait for first search to complete
    await waitFor(() => {
      expect(result.current.data).toEqual(mockAddressData);
    });

    mockFetchAddressByCep.mockResolvedValueOnce({
      ...mockAddressData,
      street: 'Rua Augusta',
    });

    result.current.searchCep('01310-200');

    // Wait for second search to complete
    await waitFor(() => {
      expect(result.current.data).toEqual({
        ...mockAddressData,
        street: 'Rua Augusta',
      });
    });
  });
});
