/**
 * useLogoUpload Hook Tests
 * 
 * Testes unitários para o hook useLogoUpload.
 * Inclui testes para upload, validação, preview e tratamento de erros.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import React from 'react';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { useLogoUpload, UPLOAD_CONFIG } from '../useLogoUpload';
import { landingPageApi, validateLogoFile } from '@/services/api/landing-page.api';
import type { LogoUploadResponse } from '../../types/landing-page.types';

// ============================================================================
// Mocks
// ============================================================================

// Mock da API
vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    uploadLogo: vi.fn(),
    removeLogo: vi.fn(),
  },
  validateLogoFile: vi.fn(),
}));

// Mock do toast
vi.mock('@/hooks/use-toast', () => ({
  toast: vi.fn(),
}));

// Mock FileReader
class MockFileReader {
  onloadend: ((this: FileReader, ev: ProgressEvent<FileReader>) => unknown) | null = null;
  result: string | ArrayBuffer | null = null;

  readAsDataURL() {
    this.result = `data:image/jpeg;base64,mockbase64data`;
    setTimeout(() => {
      if (this.onloadend) {
        this.onloadend.call(this, {} as ProgressEvent<FileReader>);
      }
    }, 0);
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
global.FileReader = MockFileReader as any;

// ============================================================================
// Test Data
// ============================================================================

const mockBarbershopId = 'barbershop-123';

const mockFile = new File(['test'], 'test.jpg', { type: 'image/jpeg' });
const mockLargeFile = new File(['x'.repeat(3 * 1024 * 1024)], 'large.jpg', { 
  type: 'image/jpeg' 
});
const mockInvalidFile = new File(['test'], 'test.txt', { type: 'text/plain' });

const mockUploadResponse: LogoUploadResponse = {
  logoUrl: 'https://cdn.example.com/logo.jpg',
  message: 'Logo uploaded successfully',
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

describe('useLogoUpload', () => {
  let mockUploadLogo: ReturnType<typeof vi.fn>;
  let mockRemoveLogo: ReturnType<typeof vi.fn>;
  let mockValidateLogoFile: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    mockUploadLogo = vi.mocked(landingPageApi.uploadLogo);
    mockRemoveLogo = vi.mocked(landingPageApi.removeLogo);
    mockValidateLogoFile = vi.mocked(validateLogoFile);
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

      expect(result.current.uploadState).toEqual({
        status: 'idle',
        progress: 0,
        url: null,
        error: null,
      });
      expect(result.current.preview).toBeNull();
      expect(result.current.originalFile).toBeNull();
      expect(result.current.isUploading).toBe(false);
      expect(result.current.canUpload).toBe(false);
      expect(result.current.hasPreview).toBe(false);
      expect(result.current.validationError).toBeNull();
    });
  });

  describe('File selection and validation', () => {
    it('should select valid file and create preview', async () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      expect(result.current.originalFile).toBe(mockFile);
      expect(result.current.validationError).toBeNull();

      // Wait for preview to be created
      await waitFor(() => {
        expect(result.current.hasPreview).toBe(true);
      });

      expect(result.current.preview).toBe('data:image/jpeg;base64,mockbase64data');
      expect(result.current.canUpload).toBe(true);
      expect(result.current.uploadState.status).toBe('selected');
    });

    it('should reject invalid file', () => {
      const errorMessage = 'Formato inválido. Use JPG, PNG ou SVG.';
      mockValidateLogoFile.mockReturnValue(errorMessage);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockInvalidFile);
      });

      expect(result.current.validationError).toBe(errorMessage);
      expect(result.current.originalFile).toBeNull();
      expect(result.current.preview).toBeNull();
      expect(result.current.canUpload).toBe(false);
    });

    it('should reject file that is too large', () => {
      const errorMessage = 'Arquivo muito grande. Tamanho máximo: 2MB.';
      mockValidateLogoFile.mockReturnValue(errorMessage);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockLargeFile);
      });

      expect(result.current.validationError).toBe(errorMessage);
      expect(mockValidateLogoFile).toHaveBeenCalledWith(mockLargeFile);
    });
  });

  describe('Auto upload', () => {
    it('should auto upload when autoUpload is true', async () => {
      mockValidateLogoFile.mockReturnValue(null);
      mockUploadLogo.mockResolvedValue(mockUploadResponse);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: true }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(mockUploadLogo).toHaveBeenCalledWith(mockBarbershopId, mockFile);
      expect(result.current.uploadState.status).toBe('success');
      expect(result.current.uploadState.url).toBe(mockUploadResponse.logoUrl);
    });

    it('should not auto upload when autoUpload is false', () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      expect(mockUploadLogo).not.toHaveBeenCalled();
      expect(result.current.uploadState.status).toBe('selected');
      expect(result.current.canUpload).toBe(true);
    });
  });

  describe('Manual upload', () => {
    it('should upload file manually', async () => {
      mockValidateLogoFile.mockReturnValue(null);
      mockUploadLogo.mockResolvedValue(mockUploadResponse);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await act(async () => {
        await result.current.uploadFile();
      });

      expect(mockUploadLogo).toHaveBeenCalledWith(mockBarbershopId, mockFile);
    });

    it('should handle upload without file', async () => {
      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      await act(async () => {
        await result.current.uploadFile();
      });

      expect(mockUploadLogo).not.toHaveBeenCalled();
    });
  });

  describe('Upload states', () => {
    it('should handle upload success', async () => {
      mockValidateLogoFile.mockReturnValue(null);
      mockUploadLogo.mockResolvedValue(mockUploadResponse);

      const onSuccess = vi.fn();

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: true, onSuccess }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.uploadState.status).toBe('success');
      });

      expect(result.current.uploadState.url).toBe(mockUploadResponse.logoUrl);
      expect(result.current.uploadState.progress).toBe(100);
      expect(result.current.preview).toBeNull(); // Should clear after success
      expect(result.current.originalFile).toBeNull();
      expect(onSuccess).toHaveBeenCalledWith(mockUploadResponse.logoUrl);
    });

    it('should handle upload error', async () => {
      const errorMessage = 'Upload failed';
      const error = new Error(errorMessage);

      mockValidateLogoFile.mockReturnValue(null);
      mockUploadLogo.mockRejectedValue(error);

      const onError = vi.fn();

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: true, onError }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.uploadState.status).toBe('error');
      });

      expect(result.current.uploadState.error).toBe(errorMessage);
      expect(onError).toHaveBeenCalledWith(errorMessage);
    });

    it('should track uploading state', async () => {
      mockValidateLogoFile.mockReturnValue(null);
      
      // Mock a slow upload
      mockUploadLogo.mockImplementation(() => 
        new Promise(resolve => setTimeout(() => resolve(mockUploadResponse), 100))
      );

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: true }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      // Should be uploading initially
      expect(result.current.isUploading).toBe(true);
      expect(result.current.uploadState.status).toBe('uploading');

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(result.current.uploadState.status).toBe('success');
    });
  });

  describe('File removal', () => {
    it('should remove local file', async () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.hasPreview).toBe(true);
      });

      act(() => {
        result.current.removeFile();
      });

      expect(result.current.originalFile).toBeNull();
      expect(result.current.preview).toBeNull();
      expect(result.current.validationError).toBeNull();
      expect(result.current.uploadState).toEqual({
        status: 'idle',
        progress: 0,
        url: null,
        error: null,
      });
    });

    it('should remove logo from server', async () => {
      mockRemoveLogo.mockResolvedValue(undefined);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await act(async () => {
        await result.current.removeLogo();
      });

      expect(mockRemoveLogo).toHaveBeenCalledWith(mockBarbershopId);
    });

    it('should handle remove logo error', async () => {
      const error = new Error('Remove failed');
      mockRemoveLogo.mockRejectedValue(error);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId),
        { wrapper: createWrapper() }
      );

      await act(async () => {
        await result.current.removeLogo();
      });

      expect(mockRemoveLogo).toHaveBeenCalledWith(mockBarbershopId);
    });
  });

  describe('State reset', () => {
    it('should reset all state', async () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.hasPreview).toBe(true);
      });

      act(() => {
        result.current.resetState();
      });

      expect(result.current.uploadState).toEqual({
        status: 'idle',
        progress: 0,
        url: null,
        error: null,
      });
      expect(result.current.preview).toBeNull();
      expect(result.current.originalFile).toBeNull();
      expect(result.current.validationError).toBeNull();
    });
  });

  describe('Configuration', () => {
    it('should respect upload configuration', () => {
      expect(UPLOAD_CONFIG.MAX_FILE_SIZE).toBe(2 * 1024 * 1024);
      expect(UPLOAD_CONFIG.ALLOWED_TYPES).toContain('image/jpeg');
      expect(UPLOAD_CONFIG.ALLOWED_TYPES).toContain('image/png');
      expect(UPLOAD_CONFIG.ALLOWED_TYPES).toContain('image/svg+xml');
      expect(UPLOAD_CONFIG.RECOMMENDED_SIZE).toBe('300x300px');
    });
  });

  describe('Computed values', () => {
    it('should compute canUpload correctly', async () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      // Initially can't upload
      expect(result.current.canUpload).toBe(false);

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.canUpload).toBe(true);
      });

      // Can't upload with validation error
      act(() => {
        result.current.removeFile();
      });

      mockValidateLogoFile.mockReturnValue('Invalid file');

      act(() => {
        result.current.selectFile(mockInvalidFile);
      });

      expect(result.current.canUpload).toBe(false);
    });

    it('should compute hasPreview correctly', async () => {
      mockValidateLogoFile.mockReturnValue(null);

      const { result } = renderHook(
        () => useLogoUpload(mockBarbershopId, { autoUpload: false }),
        { wrapper: createWrapper() }
      );

      expect(result.current.hasPreview).toBe(false);

      act(() => {
        result.current.selectFile(mockFile);
      });

      await waitFor(() => {
        expect(result.current.hasPreview).toBe(true);
      });
    });
  });
});