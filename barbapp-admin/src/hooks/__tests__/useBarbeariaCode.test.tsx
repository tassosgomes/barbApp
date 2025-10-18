import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useBarbeariaCode } from '../useBarbeariaCode';
import type { BarbeariaInfo } from '@/types/adminBarbearia';

// Mock react-router-dom
const mockUseParams = vi.fn();
vi.mock('react-router-dom', () => ({
  useParams: () => mockUseParams(),
}));

// Mock the barbershop service
vi.mock('@/services/barbershop.service', () => ({
  barbershopService: {
    validateCode: vi.fn(),
  },
}));

import { barbershopService } from '@/services/barbershop.service';

const mockBarbershopService = vi.mocked(barbershopService);

describe('useBarbeariaCode Hook', () => {
  let queryClient: QueryClient;

  beforeEach(() => {
    vi.clearAllMocks();
    queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });
  });

  const wrapper = ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );

  it('should extract codigo from URL params', () => {
    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    expect(result.current.codigo).toBe('6SJJRFPD');
  });

  it('should return undefined codigo when not in params', () => {
    mockUseParams.mockReturnValue({});

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    expect(result.current.codigo).toBeUndefined();
  });

  it('should not fetch when codigo is undefined', () => {
    mockUseParams.mockReturnValue({});

    renderHook(() => useBarbeariaCode(), { wrapper });

    expect(mockBarbershopService.validateCode).not.toHaveBeenCalled();
  });

  it('should not fetch when codigo has invalid format', () => {
    mockUseParams.mockReturnValue({ codigo: 'invalid' });

    renderHook(() => useBarbeariaCode(), { wrapper });

    expect(mockBarbershopService.validateCode).not.toHaveBeenCalled();
  });

  it('should fetch barbearia info for valid codigo', async () => {
    const mockBarbeariaInfo: BarbeariaInfo = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      nome: 'Barbearia do Tasso Zé',
      codigo: '6SJJRFPD',
      isActive: true,
    };

    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });
    mockBarbershopService.validateCode.mockResolvedValue(mockBarbeariaInfo);

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    expect(result.current.isLoading).toBe(true);
    expect(result.current.barbeariaInfo).toBeNull();

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.barbeariaInfo).toEqual(mockBarbeariaInfo);
      expect(result.current.error).toBeNull();
    });

    expect(mockBarbershopService.validateCode).toHaveBeenCalledWith('6SJJRFPD');
  });

  it('should handle 404 error (barbearia not found)', async () => {
    const error = new Error('Barbearia não encontrada');
    (error as any).status = 404;

    mockUseParams.mockReturnValue({ codigo: 'INVALIDX' }); // Valid format (8 chars, uppercase) but will fail API call
    mockBarbershopService.validateCode.mockRejectedValue(error);

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
    });

    expect(result.current.barbeariaInfo).toBeNull();
    expect(result.current.error).toEqual(error);
  });

  it('should handle 403 error (barbearia inactive)', async () => {
    const error = new Error('Barbearia temporariamente indisponível');
    (error as any).status = 403;

    mockUseParams.mockReturnValue({ codigo: 'INACTIVE' });
    mockBarbershopService.validateCode.mockRejectedValue(error);

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.barbeariaInfo).toBeNull();
      expect(result.current.error).toEqual(error);
    });
  });

  it('should handle network error', async () => {
    const error = new Error('Network Error');

    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });
    mockBarbershopService.validateCode.mockRejectedValue(error);

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    await waitFor(() => {
      expect(result.current.isLoading).toBe(false);
      expect(result.current.barbeariaInfo).toBeNull();
      expect(result.current.error).toEqual(error);
    });
  });

  it('should set isValidating to true during fetch', async () => {
    const mockBarbeariaInfo: BarbeariaInfo = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      nome: 'Barbearia do Tasso Zé',
      codigo: '6SJJRFPD',
      isActive: true,
    };

    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });
    mockBarbershopService.validateCode.mockResolvedValue(mockBarbeariaInfo);

    const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

    // Initially should be validating
    expect(result.current.isValidating).toBe(true);

    await waitFor(() => {
      expect(result.current.isValidating).toBe(false);
    });
  });

  it('should use correct query key', async () => {
    const mockBarbeariaInfo: BarbeariaInfo = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      nome: 'Barbearia do Tasso Zé',
      codigo: '6SJJRFPD',
      isActive: true,
    };

    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });
    mockBarbershopService.validateCode.mockResolvedValue(mockBarbeariaInfo);

    renderHook(() => useBarbeariaCode(), { wrapper });

    await waitFor(() => {
      expect(mockBarbershopService.validateCode).toHaveBeenCalledWith('6SJJRFPD');
    });
  });

  it('should validate codigo format locally', () => {
    // Valid formats - should attempt to fetch
    const validCodes = ['6SJJRFPD', 'ABCDEFGH'];

    validCodes.forEach((code) => {
      vi.clearAllMocks();
      mockUseParams.mockReturnValue({ codigo: code });
      mockBarbershopService.validateCode.mockResolvedValue({
        id: '123e4567-e89b-12d3-a456-426614174000',
        nome: 'Test Barbershop',
        codigo: code,
        isActive: true,
      });

      const { result } = renderHook(() => useBarbeariaCode(), { wrapper });

      expect(result.current.codigo).toBe(code);
      // Should attempt to fetch for valid codes
      expect(mockBarbershopService.validateCode).toHaveBeenCalledWith(code);
    });

    // Invalid formats - should not attempt to fetch
    const invalidCodes = ['invalid', '123', 'ABCDEFGHI'];

    invalidCodes.forEach((code) => {
      vi.clearAllMocks();
      mockUseParams.mockReturnValue({ codigo: code });

      renderHook(() => useBarbeariaCode(), { wrapper });

      // Should not attempt to fetch for invalid codes
      expect(mockBarbershopService.validateCode).not.toHaveBeenCalled();
    });
  });

  it('should cache results with correct staleTime', async () => {
    const mockBarbeariaInfo: BarbeariaInfo = {
      id: '123e4567-e89b-12d3-a456-426614174000',
      nome: 'Barbearia do Tasso Zé',
      codigo: '6SJJRFPD',
      isActive: true,
    };

    mockUseParams.mockReturnValue({ codigo: '6SJJRFPD' });
    mockBarbershopService.validateCode.mockResolvedValue(mockBarbeariaInfo);

    // First render
    const { result, rerender } = renderHook(() => useBarbeariaCode(), { wrapper });

    await waitFor(() => {
      expect(result.current.barbeariaInfo).toEqual(mockBarbeariaInfo);
    });

    // Should have been called once
    expect(mockBarbershopService.validateCode).toHaveBeenCalledTimes(1);

    // Re-render with same params (should use cache)
    rerender();

    // Should still be cached (not called again immediately)
    expect(mockBarbershopService.validateCode).toHaveBeenCalledTimes(1);
  });
});