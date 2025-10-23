/**
 * useLandingPage Hook Tests
 * 
 * Testes unitários para o hook useLandingPage.
 * Inclui testes para queries, mutations e tratamento de erros.
 * 
 * @version 2.0
 * @date 2025-10-23
 */

import React from 'react';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { useLandingPage } from '../useLandingPage';
import type { LandingPageConfig, UpdateLandingPageInput, CreateLandingPageInput } from '../../types/landing-page.types';

// ============================================================================
// Mocks
// ============================================================================

vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    getConfig: vi.fn(),
    updateConfig: vi.fn(),
    createConfig: vi.fn(),
    publishConfig: vi.fn(),
    unpublishConfig: vi.fn(),
    uploadLogo: vi.fn(),
    deleteLogo: vi.fn(),
    generatePreview: vi.fn(),
  },
}));

vi.mock('@/utils/toast', () => ({
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

// Import após mock para evitar problemas de hoisting
import { landingPageApi } from '@/services/api/landing-page.api';
import { showSuccessToast, showErrorToast } from '@/utils/toast';

// ============================================================================
// Test Data
// ============================================================================

const mockBarbershopId = 'barbershop-123';

const mockLandingPageConfig: LandingPageConfig = {
  id: 1,
  barbershopId: mockBarbershopId,
  templateId: 1,
  logoUrl: 'https://cdn.example.com/logo.jpg',
  aboutText: 'Sobre nós',
  openingHours: 'Seg-Sex: 9h-18h',
  instagramUrl: 'https://instagram.com/barbershop',
  facebookUrl: 'https://facebook.com/barbershop',
  whatsappNumber: '5511999999999',
  isPublished: true,
  services: [
    { id: '1', name: 'Corte', price: 50, duration: 30, enabled: true, displayOrder: 0 },
    { id: '2', name: 'Barba', price: 30, duration: 20, enabled: true, displayOrder: 1 },
  ],
  updatedAt: new Date().toISOString(),
  createdAt: new Date().toISOString(),
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

  function Wrapper({ children }: { children: React.ReactNode }) {
    return (
      <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
    );
  }
  return Wrapper;
};

// ============================================================================
// Tests
// ============================================================================

describe('useLandingPage', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('getConfig query', () => {
    it('should fetch config successfully', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );


      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.config).toEqual(mockLandingPageConfig);
      expect(result.current.error).toBeNull();
      expect(landingPageApi.getConfig).toHaveBeenCalledWith(mockBarbershopId);
    });


    it('should not fetch when barbershopId is empty', () => {
      const { result } = renderHook(
        () => useLandingPage(''),
        { wrapper: createWrapper() }
      );

      expect(result.current.config).toBeUndefined();
      expect(landingPageApi.getConfig).not.toHaveBeenCalled();
    });
  });

  describe('createConfig mutation', () => {
    it('should create config successfully', async () => {
      const createData: CreateLandingPageInput = {
        barbershopId: mockBarbershopId,
        templateId: 1,
        whatsappNumber: '5511999999999',
      };

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.createConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.createConfig(createData);
      });


      await waitFor(() => {
        expect(result.current.isCreating).toBe(false);
      });

      expect(landingPageApi.createConfig).toHaveBeenCalledWith(mockBarbershopId, createData);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle create error', async () => {
      const createData: CreateLandingPageInput = {
        barbershopId: mockBarbershopId,
        templateId: 1,
        whatsappNumber: '5511999999999',
      };

      const error = new Error('Create failed');
      vi.mocked(landingPageApi.createConfig).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.createConfig(createData);
      });

      await waitFor(() => {
        expect(result.current.isCreating).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('updateConfig mutation', () => {
    it('should update config successfully', async () => {
      const updateData: UpdateLandingPageInput = {
        aboutText: 'Novo texto',
      };

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.updateConfig).mockResolvedValue(undefined);

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

      expect(landingPageApi.updateConfig).toHaveBeenCalledWith(mockBarbershopId, updateData);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle update error', async () => {
      const updateData: UpdateLandingPageInput = {
        aboutText: 'Novo texto',
      };

      const error = new Error('Update failed');
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.updateConfig).mockRejectedValue(error);

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

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('publishConfig mutation', () => {
    it('should publish config successfully', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.publishConfig).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.publishConfig();
      });


      await waitFor(() => {
        expect(result.current.isPublishing).toBe(false);
      });

      expect(landingPageApi.publishConfig).toHaveBeenCalledWith(mockBarbershopId);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle publish error', async () => {
      const error = new Error('Publish failed');
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.publishConfig).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.publishConfig();
      });

      await waitFor(() => {
        expect(result.current.isPublishing).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('unpublishConfig mutation', () => {
    it('should unpublish config successfully', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.unpublishConfig).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.unpublishConfig();
      });


      await waitFor(() => {
        expect(result.current.isUnpublishing).toBe(false);
      });

      expect(landingPageApi.unpublishConfig).toHaveBeenCalledWith(mockBarbershopId);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle unpublish error', async () => {
      const error = new Error('Unpublish failed');
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.unpublishConfig).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.unpublishConfig();
      });

      await waitFor(() => {
        expect(result.current.isUnpublishing).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('uploadLogo mutation', () => {
    it('should upload logo successfully', async () => {
      const mockFile = new File(['logo'], 'logo.jpg', { type: 'image/jpeg' });
      const mockLogoUrl = 'https://cdn.example.com/new-logo.jpg';

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(mockLogoUrl);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.uploadLogo(mockFile);
      });


      await waitFor(() => {
        expect(result.current.isUploadingLogo).toBe(false);
      });

      expect(landingPageApi.uploadLogo).toHaveBeenCalledWith(mockBarbershopId, mockFile);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle upload error', async () => {
      const mockFile = new File(['logo'], 'logo.jpg', { type: 'image/jpeg' });
      const error = new Error('Upload failed');

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.uploadLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.uploadLogo(mockFile);
      });

      await waitFor(() => {
        expect(result.current.isUploadingLogo).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('deleteLogo mutation', () => {
    it('should delete logo successfully', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.deleteLogo();
      });


      await waitFor(() => {
        expect(result.current.isDeletingLogo).toBe(false);
      });

      expect(landingPageApi.deleteLogo).toHaveBeenCalledWith(mockBarbershopId);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle delete error', async () => {
      const error = new Error('Delete failed');
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.deleteLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeletingLogo).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('generatePreview mutation', () => {
    it('should generate preview successfully', async () => {
      const mockPreviewUrl = 'https://preview.example.com/123';

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.generatePreview).mockResolvedValue(mockPreviewUrl);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.generatePreview();
      });


      await waitFor(() => {
        expect(result.current.isGeneratingPreview).toBe(false);
      });

      expect(landingPageApi.generatePreview).toHaveBeenCalledWith(mockBarbershopId);
    });

    it('should handle preview generation error', async () => {
      const error = new Error('Preview failed');

      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);
      vi.mocked(landingPageApi.generatePreview).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      act(() => {
        result.current.generatePreview();
      });

      await waitFor(() => {
        expect(result.current.isGeneratingPreview).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('Utility functions', () => {
    it('should refetch config', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      vi.clearAllMocks();

      act(() => {
        result.current.refetch();
      });

      await waitFor(() => {
        expect(landingPageApi.getConfig).toHaveBeenCalled();
      });
    });

    it('should invalidate queries', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      await act(async () => {
        await result.current.invalidateQueries();
      });

      expect(result.current.invalidateQueries).toBeDefined();
    });
  });

  describe('Loading states', () => {
    it('should track loading states correctly', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(
        () => useLandingPage(mockBarbershopId),
        { wrapper: createWrapper() }
      );


      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.isCreating).toBe(false);
      expect(result.current.isUpdating).toBe(false);
      expect(result.current.isPublishing).toBe(false);
      expect(result.current.isUnpublishing).toBe(false);
      expect(result.current.isUploadingLogo).toBe(false);
      expect(result.current.isDeletingLogo).toBe(false);
      expect(result.current.isGeneratingPreview).toBe(false);
    });
  });

});
