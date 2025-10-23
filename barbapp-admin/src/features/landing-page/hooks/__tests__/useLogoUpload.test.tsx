/**
 * useLogoUpload Hook Tests
 * 
 * Testes unitários para o hook useLogoUpload.
 * Inclui testes para upload, validação, preview e tratamento de erros.
 * 
 * @version 2.0
 * @date 2025-10-23
 */

import React from 'react';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { useLogoUpload } from '../useLogoUpload';
import { LOGO_UPLOAD_CONFIG } from '../../constants/validation';

// ============================================================================
// Mocks
// ============================================================================

vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    uploadLogo: vi.fn(),
    deleteLogo: vi.fn(),
  },
}));

vi.mock('@/utils/toast', () => ({
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

// Import após mock para evitar problemas de hoisting
import { landingPageApi } from '@/services/api/landing-page.api';
import { showSuccessToast, showErrorToast } from '@/utils/toast';

// Mock URL.createObjectURL and URL.revokeObjectURL
global.URL.createObjectURL = vi.fn(() => 'mock-object-url');
global.URL.revokeObjectURL = vi.fn();

// ============================================================================
// Test Data
// ============================================================================

const mockBarbershopId = 'barbershop-123';

const mockFile = new File(['test'], 'logo.png', { type: 'image/png' });
Object.defineProperty(mockFile, 'size', { value: 1024 * 1024 }); // 1MB

const mockLargeFile = new File(['x'.repeat(3 * 1024 * 1024)], 'large.jpg', { 
  type: 'image/jpeg' 
});
Object.defineProperty(mockLargeFile, 'size', { value: 3 * 1024 * 1024 }); // 3MB

const mockInvalidFile = new File(['test'], 'test.txt', { type: 'text/plain' });

const mockUploadResponse = 'https://cdn.example.com/logo.jpg';

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

describe('useLogoUpload', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('Initial state', () => {
    it('should initialize with correct default state', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      expect(result.current.isUploading).toBe(false);
      expect(result.current.isDeleting).toBe(false);
      expect(result.current.uploadError).toBeNull();
      expect(result.current.deleteError).toBeNull();
      expect(result.current.validationError).toBeNull();
      expect(result.current.previewUrl).toBeNull();
    });
  });

  describe('File validation', () => {
    it('should accept valid file', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      const error = result.current.validateFile(mockFile);
      expect(error).toBeNull();
    });

    it('should reject file that is too large', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      const error = result.current.validateFile(mockLargeFile);
      expect(error).not.toBeNull();
      expect(error?.type).toBe('size');
      expect(error?.message).toContain('muito grande');
    });

    it('should reject invalid file type', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      const error = result.current.validateFile(mockInvalidFile);
      expect(error).not.toBeNull();
      expect(error?.type).toBe('type');
      expect(error?.message).toContain('não suportado');
    });
  });

  describe('Logo upload', () => {
    it('should upload valid file successfully', async () => {
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(mockUploadResponse);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockFile);
      });


      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(landingPageApi.uploadLogo).toHaveBeenCalledWith(mockBarbershopId, mockFile);
      expect(showSuccessToast).toHaveBeenCalled();
      expect(result.current.validationError).toBeNull();
    });

    it('should not upload invalid file', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockInvalidFile);
      });

      expect(landingPageApi.uploadLogo).not.toHaveBeenCalled();
      expect(showErrorToast).toHaveBeenCalled();
      expect(result.current.validationError).not.toBeNull();
    });

    it('should handle upload error', async () => {
      const error = new Error('Upload failed');
      vi.mocked(landingPageApi.uploadLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockFile);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('Logo deletion', () => {
    it('should delete logo successfully', async () => {
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.deleteLogo();
      });


      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      expect(landingPageApi.deleteLogo).toHaveBeenCalledWith(mockBarbershopId);
      expect(showSuccessToast).toHaveBeenCalled();
    });

    it('should handle delete error', async () => {
      const error = new Error('Delete failed');
      vi.mocked(landingPageApi.deleteLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      expect(showErrorToast).toHaveBeenCalled();
    });
  });

  describe('Preview management', () => {
    it('should create preview for valid file', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.createPreview(mockFile);
      });

      expect(result.current.previewUrl).toBe('mock-object-url');
      expect(result.current.validationError).toBeNull();
      expect(global.URL.createObjectURL).toHaveBeenCalledWith(mockFile);
    });

    it('should not create preview for invalid file', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.createPreview(mockInvalidFile);
      });

      expect(result.current.previewUrl).toBeNull();
      expect(result.current.validationError).not.toBeNull();
      expect(global.URL.createObjectURL).not.toHaveBeenCalled();
    });

    it('should clear preview', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Create preview first
      act(() => {
        result.current.createPreview(mockFile);
      });

      expect(result.current.previewUrl).toBe('mock-object-url');

      // Clear preview
      act(() => {
        result.current.clearPreview();
      });

      expect(result.current.previewUrl).toBeNull();
      expect(result.current.validationError).toBeNull();
      expect(global.URL.revokeObjectURL).toHaveBeenCalledWith('mock-object-url');
    });
  });

  describe('Configuration', () => {
    it('should have correct upload configuration', () => {
      expect(LOGO_UPLOAD_CONFIG.maxSize).toBe(2 * 1024 * 1024);
      expect(LOGO_UPLOAD_CONFIG.allowedTypes).toContain('image/jpeg');
      expect(LOGO_UPLOAD_CONFIG.allowedTypes).toContain('image/png');
      expect(LOGO_UPLOAD_CONFIG.allowedTypes).toContain('image/svg+xml');
      expect(LOGO_UPLOAD_CONFIG.recommendedSize).toEqual({
        width: 300,
        height: 300,
      });
    });
  });

  describe('Error states', () => {
    it('should track upload error', async () => {
      const error = { response: { data: { message: 'Upload failed' } } };
      vi.mocked(landingPageApi.uploadLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockFile);
      });

      await waitFor(() => {
        expect(result.current.uploadError).toBeTruthy();
      });
    });

    it('should track delete error', async () => {
      const error = { response: { data: { message: 'Delete failed' } } };
      vi.mocked(landingPageApi.deleteLogo).mockRejectedValue(error);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.deleteError).toBeTruthy();
      });
    });

    it('should track validation error', () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockLargeFile);
      });

      expect(result.current.validationError).not.toBeNull();
      expect(result.current.validationError?.type).toBe('size');
    });
  });

  describe('Integration with query cache', () => {
    it('should update query cache on successful upload', async () => {
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(mockUploadResponse);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.uploadLogo(mockFile);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(landingPageApi.uploadLogo).toHaveBeenCalledWith(mockBarbershopId, mockFile);
    });

    it('should update query cache on successful deletion', async () => {
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      expect(landingPageApi.deleteLogo).toHaveBeenCalledWith(mockBarbershopId);
    });
  });

  describe('Cleanup', () => {
    it('should clear preview and validation error on successful upload', async () => {
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(mockUploadResponse);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Create preview first
      act(() => {
        result.current.createPreview(mockFile);
      });

      expect(result.current.previewUrl).toBe('mock-object-url');

      // Upload logo
      act(() => {
        result.current.uploadLogo(mockFile);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      // Preview should be cleared
      expect(result.current.previewUrl).toBeNull();
      expect(result.current.validationError).toBeNull();
    });

    it('should clear preview on successful deletion', async () => {
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      // Create preview first
      act(() => {
        result.current.createPreview(mockFile);
      });

      expect(result.current.previewUrl).toBe('mock-object-url');

      // Delete logo
      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      // Preview should be cleared
      expect(result.current.previewUrl).toBeNull();
    });
  });
});
