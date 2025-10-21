import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { renderHook, waitFor, act } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import React from 'react';
import { useLogoUpload } from '@/features/landing-page/hooks/useLogoUpload';
import { landingPageApi } from '@/services/api/landing-page.api';
import * as toastUtils from '@/utils/toast';
import { VALIDATION_RULES } from '@/features/landing-page/constants/validation';

// Mock the API
vi.mock('@/services/api/landing-page.api', () => ({
  landingPageApi: {
    uploadLogo: vi.fn(),
    deleteLogo: vi.fn(),
  },
}));

// Mock the toast utilities
vi.mock('@/utils/toast', () => ({
  showSuccessToast: vi.fn(),
  showErrorToast: vi.fn(),
}));

// Mock URL.createObjectURL and revokeObjectURL
const mockCreateObjectURL = vi.fn(() => 'blob:mock-url');
const mockRevokeObjectURL = vi.fn();

Object.defineProperty(window, 'URL', {
  value: {
    createObjectURL: mockCreateObjectURL,
    revokeObjectURL: mockRevokeObjectURL,
  },
  writable: true,
});

describe('useLogoUpload', () => {
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
    mockCreateObjectURL.mockClear();
    mockRevokeObjectURL.mockClear();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  describe('File validation', () => {
    it('should validate file size correctly', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      // Valid file size
      const validFile = new File([''], 'logo.png', { 
        type: 'image/png',
      });
      Object.defineProperty(validFile, 'size', { value: 1024 * 1024 }); // 1MB

      const validResult = result.current.validateFile(validFile);
      expect(validResult).toBeNull();

      // Invalid file size (too large)
      const invalidFile = new File([''], 'logo.png', { 
        type: 'image/png',
      });
      Object.defineProperty(invalidFile, 'size', { value: 3 * 1024 * 1024 }); // 3MB

      const invalidResult = result.current.validateFile(invalidFile);
      expect(invalidResult).toEqual({
        type: 'size',
        message: `Arquivo muito grande. Tamanho máximo: ${VALIDATION_RULES.LOGO_MAX_SIZE / 1024 / 1024}MB`,
      });
    });

    it('should validate file type correctly', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      // Valid file types
      const validTypes = ['image/png', 'image/jpeg', 'image/svg+xml'];
      validTypes.forEach(type => {
        const file = new File([''], 'logo', { type });
        Object.defineProperty(file, 'size', { value: 1024 }); // Small size
        
        const result_validation = result.current.validateFile(file);
        expect(result_validation).toBeNull();
      });

      // Invalid file type
      const invalidFile = new File([''], 'logo.txt', { type: 'text/plain' });
      Object.defineProperty(invalidFile, 'size', { value: 1024 });

      const invalidResult = result.current.validateFile(invalidFile);
      expect(invalidResult).toEqual({
        type: 'type',
        message: `Tipo de arquivo não suportado. Use: ${VALIDATION_RULES.LOGO_ALLOWED_TYPES.join(', ')}`,
      });
    });
  });

  describe('Logo upload', () => {
    it('should upload logo successfully', async () => {
      const logoUrl = 'https://example.com/logo.png';
      vi.mocked(landingPageApi.uploadLogo).mockResolvedValue(logoUrl);

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      Object.defineProperty(file, 'size', { value: 1024 });

      act(() => {
        result.current.uploadLogo(file);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(landingPageApi.uploadLogo).toHaveBeenCalledWith('barbershop-1', file);
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Logo atualizado',
        'O logo da sua landing page foi atualizado com sucesso!'
      );
    });

    it('should not upload invalid file', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const invalidFile = new File([''], 'logo.txt', { type: 'text/plain' });
      Object.defineProperty(invalidFile, 'size', { value: 1024 });

      act(() => {
        result.current.uploadLogo(invalidFile);
      });

      expect(landingPageApi.uploadLogo).not.toHaveBeenCalled();
      expect(toastUtils.showErrorToast).toHaveBeenCalledWith(
        'Arquivo inválido',
        expect.stringContaining('Tipo de arquivo não suportado')
      );
    });

    it('should handle upload errors', async () => {
      const error = { response: { data: { message: 'Upload failed' } } };
      vi.mocked(landingPageApi.uploadLogo).mockRejectedValue(error);

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      Object.defineProperty(file, 'size', { value: 1024 });

      act(() => {
        result.current.uploadLogo(file);
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });

      expect(toastUtils.showErrorToast).toHaveBeenCalledWith(
        'Erro no upload',
        'Upload failed'
      );
    });
  });

  describe('Logo deletion', () => {
    it('should delete logo successfully', async () => {
      vi.mocked(landingPageApi.deleteLogo).mockResolvedValue();

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      expect(landingPageApi.deleteLogo).toHaveBeenCalledWith('barbershop-1');
      expect(toastUtils.showSuccessToast).toHaveBeenCalledWith(
        'Logo removido',
        'O logo foi removido da sua landing page'
      );
    });

    it('should handle delete errors', async () => {
      const error = { response: { data: { message: 'Delete failed' } } };
      vi.mocked(landingPageApi.deleteLogo).mockRejectedValue(error);

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      act(() => {
        result.current.deleteLogo();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });

      expect(toastUtils.showErrorToast).toHaveBeenCalledWith(
        'Erro na remoção',
        'Delete failed'
      );
    });
  });

  describe('Preview functionality', () => {
    it('should create preview for valid file', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      Object.defineProperty(file, 'size', { value: 1024 });

      act(() => {
        result.current.createPreview(file);
      });

      expect(mockCreateObjectURL).toHaveBeenCalledWith(file);
      expect(result.current.previewUrl).toBe('blob:mock-url');
      expect(result.current.validationError).toBeNull();
    });

    it('should not create preview for invalid file', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const invalidFile = new File([''], 'logo.txt', { type: 'text/plain' });
      Object.defineProperty(invalidFile, 'size', { value: 1024 });

      act(() => {
        result.current.createPreview(invalidFile);
      });

      expect(mockCreateObjectURL).not.toHaveBeenCalled();
      expect(result.current.previewUrl).toBeNull();
      expect(result.current.validationError).not.toBeNull();
    });

    it('should clear preview correctly', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      Object.defineProperty(file, 'size', { value: 1024 });

      // Create preview first
      act(() => {
        result.current.createPreview(file);
      });

      expect(result.current.previewUrl).toBe('blob:mock-url');

      // Clear preview
      act(() => {
        result.current.clearPreview();
      });

      expect(mockRevokeObjectURL).toHaveBeenCalledWith('blob:mock-url');
      expect(result.current.previewUrl).toBeNull();
      expect(result.current.validationError).toBeNull();
    });

    it('should cleanup previous preview when creating new one', () => {
      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file1 = new File([''], 'logo1.png', { type: 'image/png' });
      const file2 = new File([''], 'logo2.png', { type: 'image/png' });
      Object.defineProperty(file1, 'size', { value: 1024 });
      Object.defineProperty(file2, 'size', { value: 1024 });

      // Create first preview
      act(() => {
        result.current.createPreview(file1);
      });

      const firstPreviewUrl = result.current.previewUrl;
      expect(firstPreviewUrl).toBe('blob:mock-url');

      // Create second preview (should cleanup first one)
      mockCreateObjectURL.mockReturnValue('blob:mock-url-2');
      act(() => {
        result.current.createPreview(file2);
      });

      expect(result.current.previewUrl).toBe('blob:mock-url-2');
    });
  });

  describe('Loading states', () => {
    it('should track upload loading state', async () => {
      let resolveUpload: (value: string) => void;
      const uploadPromise = new Promise<string>((resolve) => {
        resolveUpload = resolve;
      });
      vi.mocked(landingPageApi.uploadLogo).mockReturnValue(uploadPromise);

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      const file = new File([''], 'logo.png', { type: 'image/png' });
      Object.defineProperty(file, 'size', { value: 1024 });

      act(() => {
        result.current.uploadLogo(file);
      });

      expect(result.current.isUploading).toBe(true);

      act(() => {
        resolveUpload!('https://example.com/logo.png');
      });

      await waitFor(() => {
        expect(result.current.isUploading).toBe(false);
      });
    });

    it('should track delete loading state', async () => {
      let resolveDelete: () => void;
      const deletePromise = new Promise<void>((resolve) => {
        resolveDelete = resolve;
      });
      vi.mocked(landingPageApi.deleteLogo).mockReturnValue(deletePromise);

      const { result } = renderHook(() => useLogoUpload('barbershop-1'), {
        wrapper: createWrapper,
      });

      act(() => {
        result.current.deleteLogo();
      });

      expect(result.current.isDeleting).toBe(true);

      act(() => {
        resolveDelete!();
      });

      await waitFor(() => {
        expect(result.current.isDeleting).toBe(false);
      });
    });
  });
});