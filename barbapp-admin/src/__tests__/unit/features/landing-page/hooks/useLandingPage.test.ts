import { describe, it, expect, vi, beforeEach } from 'vitest';
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useLandingPage } from '@/features/landing-page/hooks/useLandingPage';
import { landingPageApi } from '@/services/api/landing-page.api';
import * as toastUtils from '@/utils/toast';

// Mock the API
vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    getConfig: vi.fn(),
    createConfig: vi.fn(),
    updateConfig: vi.fn(),
    publishConfig: vi.fn(),
    unpublishConfig: vi.fn(),
    uploadLogo: vi.fn(),
    deleteLogo: vi.fn(),
    generatePreview: vi.fn(),
  },
}));

// Mock the toast utilities
vi.mock('@/utils/toast', () => ({
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

const mockLandingPageConfig = {
  id: '1',
  barbershopId: 'barbershop-1',
  templateId: 1,
  logoUrl: 'https://example.com/logo.png',
  aboutText: 'Test about text',
  openingHours: 'Mon-Fri 9-17',
  instagramUrl: 'https://instagram.com/test',
  facebookUrl: 'https://facebook.com/test',
  whatsappNumber: '+5511999999999',
  isPublished: false,
  services: [],
  updatedAt: '2024-01-01T00:00:00Z',
  createdAt: '2024-01-01T00:00:00Z',
};

describe('useLandingPage', () => {
  let queryClient: QueryClient;

  const createWrapper = ({ children }: { children: React.ReactNode }) => {
    return React.createElement(QueryClientProvider, { client: queryClient }, children);
  };

  beforeEach(() => {
    queryClient = new QueryClient({
      defaultOptions: {
        queries: { retry: false },
        mutations: { retry: false },
      },
    });
    vi.clearAllMocks();
  });

  describe('Data fetching', () => {
    it('should fetch landing page config successfully', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(result.current.config).toEqual(mockLandingPageConfig);
      expect(landingPageApi.getConfig).toHaveBeenCalledWith('barbershop-1');
    });

    it('should handle fetch errors gracefully', async () => {
      const error = new Error('Network error');
      vi.mocked(landingPageApi.getConfig).mockRejectedValue(error);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      }, { timeout: 5000 });

      expect(result.current.error).toBeTruthy();
      expect(result.current.config).toBeUndefined();
    });

    it('should not fetch when barbershopId is empty', () => {
      const { result } = renderHook(() => useLandingPage(''), {
        wrapper: createWrapper,
      });

      expect(landingPageApi.getConfig).not.toHaveBeenCalled();
      expect(result.current.isLoading).toBe(false);
    });
  });

  describe('Create config', () => {
    it('should create config successfully', async () => {
      vi.mocked(landingPageApi.createConfig).mockResolvedValue();

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      const createPayload = {
        templateId: 1,
        whatsappNumber: '+5511999999999',
        services: [],
      };

      result.current.createConfig(createPayload);

      await waitFor(() => {
        expect(result.current.isCreating).toBe(false);
      });

      expect(landingPageApi.createConfig).toHaveBeenCalledWith('barbershop-1', createPayload);
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Landing page criada',
        'Sua landing page foi criada com sucesso!'
      );
    });

    it('should handle create errors', async () => {
      const error = { response: { data: { message: 'Create failed' } } };
      vi.mocked(landingPageApi.createConfig).mockRejectedValue(error);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.createConfig({
        templateId: 1,
        whatsappNumber: '+5511999999999',
        services: [],
      });

      await waitFor(() => {
        expect(result.current.isCreating).toBe(false);
      });

      expect(toastUtils.showErrorToast).toHaveBeenCalledWith('Erro na criação', 'Create failed');
    });
  });

  describe('Update config', () => {
    it('should update config successfully', async () => {
      vi.mocked(landingPageApi.updateConfig).mockResolvedValue();

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      const updatePayload = { aboutText: 'Updated text' };
      result.current.updateConfig(updatePayload);

      await waitFor(() => {
        expect(result.current.isUpdating).toBe(false);
      });

      expect(landingPageApi.updateConfig).toHaveBeenCalledWith('barbershop-1', updatePayload);
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Landing page atualizada',
        'Suas alterações foram salvas com sucesso!'
      );
    });
  });

  describe('Publish/Unpublish', () => {
    it('should publish config successfully', async () => {
      vi.mocked(landingPageApi.publishConfig).mockResolvedValue();

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.publishConfig();

      await waitFor(() => {
        expect(result.current.isPublishing).toBe(false);
      });

      expect(landingPageApi.publishConfig).toHaveBeenCalledWith('barbershop-1');
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Landing page publicada',
        'Sua landing page está agora disponível publicamente!'
      );
    });

    it('should unpublish config successfully', async () => {
      vi.mocked(landingPageApi.unpublishConfig).mockResolvedValue();

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.unpublishConfig();

      await waitFor(() => {
        expect(result.current.isUnpublishing).toBe(false);
      });

      expect(landingPageApi.unpublishConfig).toHaveBeenCalledWith('barbershop-1');
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Landing page despublicada',
        'Sua landing page não está mais pública'
      );
    });
  });

  describe('Logo operations', () => {
    it('should upload logo successfully', async () => {
      const logoUrl = 'https://example.com/new-logo.png';
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(logoUrl);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      result.current.uploadLogo(file);

      await waitFor(() => {
        expect(result.current.isUploadingLogo).toBe(false);
      });

      expect(landingPageApi.uploadLogo).toHaveBeenCalledWith('barbershop-1', file);
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Logo atualizado',
        'O logo da sua landing page foi atualizado com sucesso!'
      );
    });

    it('should delete logo successfully', async () => {
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue();

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.deleteLogo();

      await waitFor(() => {
        expect(result.current.isDeletingLogo).toBe(false);
      });

      expect(landingPageApi.deleteLogo).toHaveBeenCalledWith('barbershop-1');
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Logo removido',
        'O logo foi removido da sua landing page'
      );
    });
  });

  describe('Preview generation', () => {
    it('should generate preview successfully', async () => {
      const previewUrl = 'https://example.com/preview';
      vi.mocked(landingPageApi.generatePreview).mockResolvedValue(previewUrl);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.generatePreview();

      await waitFor(() => {
        expect(result.current.isGeneratingPreview).toBe(false);
      });

      expect(landingPageApi.generatePreview).toHaveBeenCalledWith('barbershop-1');
    });

    it('should handle preview generation errors', async () => {
      const error = { response: { data: { message: 'Preview failed' } } };
      vi.mocked(landingPageApi.generatePreview).mockRejectedValue(error);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      result.current.generatePreview();

      await waitFor(() => {
        expect(result.current.isGeneratingPreview).toBe(false);
      });

      expect(toastUtils.showErrorToast).toHaveBeenCalledWith('Erro no preview', 'Preview failed');
    });
  });

  describe('Utility functions', () => {
    it('should provide invalidateQueries function', () => {
      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      expect(typeof result.current.invalidateQueries).toBe('function');
    });

    it('should provide refetch function', async () => {
      vi.mocked(landingPageApi.getConfig).mockResolvedValue(mockLandingPageConfig);

      const { result } = renderHook(() => useLandingPage('barbershop-1'), {
        wrapper: createWrapper,
      });

      await waitFor(() => {
        expect(result.current.isLoading).toBe(false);
      });

      expect(typeof result.current.refetch).toBe('function');
    });
  });
});