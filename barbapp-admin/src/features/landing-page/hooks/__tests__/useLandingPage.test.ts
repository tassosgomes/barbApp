/**
 * useLandingPage Hook Tests
 * 
 * Testes unitários para o hook useLandingPage.
 * Inclui testes para queries, mutations, cache e tratamento de erros.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { useLandingPage, LANDING_PAGE_QUERY_KEYS } from '../useLandingPage';
import { landingPageApi } from '@/services/api/landing-page.api';
import type { LandingPageConfigOutput, UpdateLandingPageInput } from '../../types/landing-page.types';

// ============================================================================
// Mocks
// ============================================================================

// Mock da API
vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    getConfig: vi.fn(),
    updateConfig: vi.fn(),
    createConfig: vi.fn(),
    togglePublish: vi.fn(),
  },
}));

// Mock do toast
vi.mock('@/components/ui/use-toast', () => ({
  toast: vi.fn(),
}));

// ============================================================================
// Test Data
// ============================================================================

const mockBarbershopId = 'barbershop-123';

const mockLandingPageConfig: LandingPageConfigOutput = {
  id: 'config-123',
  barbershopId: mockBarbershopId,
  templateId: 1,
  logoUrl: 'https://example.com/logo.png',
  aboutText: 'Test barbershop',
  openingHours: 'Mon-Fri 9-18',
  instagramUrl: 'https://instagram.com/test',
  facebookUrl: 'https://facebook.com/test',
  whatsappNumber: '+5511999999999',
  isPublished: true,
  services: [
    {
      serviceId: 'service-1',
      serviceName: 'Corte',
      description: 'Corte social',
      duration: 30,
      price: 35.00,
      displayOrder: 1,
      isVisible: true,
    },
  ],
  updatedAt: '2025-10-21T10:00:00Z',
};

// ============================================================================
// Test Utilities
// ============================================================================

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        gcTime: 0,
      },
      mutations: {
        retry: false,
      },
    },
  });

  return ({ children }: { children: React.ReactNode }) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

// ============================================================================
// Tests
// ============================================================================

describe('useLandingPage', () => {
  let mockGetConfig: ReturnType<typeof vi.fn>;
  let mockUpdateConfig: ReturnType<typeof vi.fn>;
  let mockCreateConfig: ReturnType<typeof vi.fn>;
  let mockTogglePublish: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockGetConfig = vi.mocked(landingPageApi.getConfig);
    mockUpdateConfig = vi.mocked(landingPageApi.updateConfig);
    mockCreateConfig = vi.mocked(landingPageApi.createConfig);
    mockTogglePublish = vi.mocked(landingPageApi.togglePublish);
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('Query - getConfig', () => {
    it('should fetch landing page config successfully', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      expect(result.current.isLoading).toBe(true);
      expect(result.current.config).toBeUndefined();

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.config).toEqual(mockLandingPageConfig);
      expect(result.current.isError).toBe(false);
      expect(result.current.hasConfig).toBe(true);
      expect(result.current.isReady).toBe(true);
      expect(mockGetConfig).toHaveBeenCalledWith(mockBarbershopId);
    });

    it('should handle fetch error', async () => {
      const error = new Error('API Error');
      mockGetConfig.mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.isError).toBe(true);
      expect(result.current.error).toEqual(error);
      expect(result.current.config).toBeUndefined();
      expect(result.current.isReady).toBe(false);
    });

    it('should not fetch when barbershopId is empty', () => {
      const { result } = renderHook(
        () => useLandingPage(''),
        { wrapper: createWrapper() }
      );

      expect(result.current.isLoading).toBe(false);
      expect(mockGetConfig).not.toHaveBeenCalled();
    });
  });

  describe('Mutation - updateConfig', () => {
    it('should update config successfully', async () => {
      const updatedConfig = { ...mockLandingPageConfig, aboutText: 'Updated text' };
      const updateData: UpdateLandingPageInput = { aboutText: 'Updated text' };

      mockGetConfig.mockResolvedValue(mockLandingPageConfig);
      mockUpdateConfig.mockResolvedValue(updatedConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Wait for initial load
      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      // Perform update
      act(() => {
        result.current.updateConfig(updateData);
      });

      expect(result.current.isUpdating).toBe(true);

      await waitFor(() => {
        expect(result.current.isUpdating).toBe(false);
      });

      expect(mockUpdateConfig).toHaveBeenCalledWith(mockBarbershopId, updateData);
    });

    it('should handle update error and revert optimistic update', async () => {
      const error = new Error('Update failed');
      const updateData: UpdateLandingPageInput = { aboutText: 'Updated text' };

      mockGetConfig.mockResolvedValue(mockLandingPageConfig);
      mockUpdateConfig.mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Wait for initial load
      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      const originalConfig = result.current.config;

      // Perform update
      act(() => {
        result.current.updateConfig(updateData);
      });

      await waitFor(() => {
        expect(result.current.isUpdating).toBe(false);
      });

      // Should revert to original config after error
      expect(result.current.config).toEqual(originalConfig);
    });
  });

  describe('Mutation - togglePublish', () => {
    it('should toggle publish status successfully', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);
      mockTogglePublish.mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Wait for initial load
      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.isPublished).toBe(true);

      // Toggle to unpublished
      act(() => {
        result.current.togglePublish(false);
      });

      expect(result.current.isToggling).toBe(true);

      await waitFor(() => {
        expect(result.current.isToggling).toBe(false);
      });

      expect(mockTogglePublish).toHaveBeenCalledWith(mockBarbershopId, false);
    });
  });

  describe('Utility functions', () => {
    it('should provide utility functions', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(typeof result.current.invalidateCache).toBe('function');
      expect(typeof result.current.clearCache).toBe('function');
      expect(typeof result.current.prefetchConfig).toBe('function');
      expect(typeof result.current.refetch).toBe('function');
    });

    it('should compute correct values', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.hasConfig).toBe(true);
      expect(result.current.isReady).toBe(true);
      expect(result.current.isPublished).toBe(true);
    });
  });

  describe('Query Keys', () => {
    it('should generate correct query keys', () => {
      const keys = LANDING_PAGE_QUERY_KEYS;

      expect(keys.all).toEqual(['landingPage']);
      expect(keys.configs()).toEqual(['landingPage', 'config']);
      expect(keys.config('test-id')).toEqual(['landingPage', 'config', 'test-id']);
      expect(keys.public('test-code')).toEqual(['landingPage', 'public', 'test-code']);
      expect(keys.analytics('test-id')).toEqual(['landingPage', 'analytics', 'test-id']);
    });
  });

  describe('Error handling', () => {
    it('should handle network errors gracefully', async () => {
      const networkError = new Error('Network Error');
      mockGetConfig.mockRejectedValue(networkError);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.isError).toBe(true);
      expect(result.current.error).toEqual(networkError);
      expect(result.current.isReady).toBe(false);
    });

    it('should handle API validation errors', async () => {
      const validationError = new Error('Validation failed');
      const updateData: UpdateLandingPageInput = { aboutText: '' };

      mockGetConfig.mockResolvedValue(mockLandingPageConfig);
      mockUpdateConfig.mockRejectedValue(validationError);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.updateConfig(updateData);
      });

      await waitFor(() => {
        expect(result.current.isUpdating).toBe(false);
      });

      // Should maintain original state after error
      expect(result.current.config?.aboutText).toBe(mockLandingPageConfig.aboutText);
    });
  });

  describe('Cache management', () => {
    it('should handle cache invalidation', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      // Should not throw when calling cache utilities
      expect(() => {
        result.current.invalidateCache();
        result.current.clearCache();
      }).not.toThrow();
    });

    it('should handle prefetch', async () => {
      mockGetConfig.mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current.prefetchConfig();
      });

      // Should call API for prefetch
      expect(mockGetConfig).toHaveBeenCalledTimes(2); // Initial + prefetch
    });
  });
});