/**
 * useLogoUpload Hook
 * 
 * Hook especializado para gerenciar upload de logo da barbearia.
 * Inclui validação de arquivo, preview local, upload progressivo
 * e integração com cache da landing page.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import { useState, useCallback } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from '@/hooks/use-toast';
import { landingPageApi, validateLogoFile } from '@/services/api/landing-page.api';
import { LANDING_PAGE_QUERY_KEYS } from './useLandingPage';
import type {
  LogoUploadResponse,
  LogoUploadState,
  LandingPageConfigOutput,
} from '../types/landing-page.types';

// ============================================================================
// Constants
// ============================================================================

const UPLOAD_CONFIG = {
  MAX_FILE_SIZE: 2 * 1024 * 1024, // 2MB
  ALLOWED_TYPES: ['image/jpeg', 'image/png', 'image/svg+xml'],
  RECOMMENDED_SIZE: '300x300px',
  PREVIEW_MAX_WIDTH: 400,
  PREVIEW_MAX_HEIGHT: 400,
} as const;

// ============================================================================
// Types
// ============================================================================

interface UseLogoUploadOptions {
  onSuccess?: (url: string) => void;
  onError?: (error: string) => void;
  autoUpload?: boolean;
}

interface UseLogoUploadResult {
  // State
  uploadState: LogoUploadState;
  preview: string | null;
  originalFile: File | null;
  
  // Actions
  selectFile: (file: File) => void;
  uploadFile: () => Promise<void>;
  removeFile: () => void;
  removeLogo: () => Promise<void>;
  resetState: () => void;
  
  // Status
  isUploading: boolean;
  canUpload: boolean;
  hasPreview: boolean;
  
  // Validation
  validationError: string | null;
}

// ============================================================================
// Hook Implementation
// ============================================================================

export const useLogoUpload = (
  barbershopId: string,
  options: UseLogoUploadOptions = {}
): UseLogoUploadResult => {
  const { onSuccess, onError, autoUpload = true } = options;
  const queryClient = useQueryClient();

  // ============================================================================
  // State
  // ============================================================================

  const [uploadState, setUploadState] = useState<LogoUploadState>({
    status: 'idle',
    progress: 0,
    url: null,
    error: null,
  });

  const [preview, setPreview] = useState<string | null>(null);
  const [originalFile, setOriginalFile] = useState<File | null>(null);
  const [validationError, setValidationError] = useState<string | null>(null);

  // ============================================================================
  // Mutations
  // ============================================================================

  /**
   * Upload do logo
   */
  const uploadMutation = useMutation({
    mutationFn: (file: File) => landingPageApi.uploadLogo(barbershopId, file),
    onMutate: () => {
      setUploadState(prev => ({
        ...prev,
        status: 'uploading',
        progress: 0,
        error: null,
      }));
    },
    onSuccess: (response: LogoUploadResponse) => {
      setUploadState(prev => ({
        ...prev,
        status: 'success',
        progress: 100,
        url: response.logoUrl,
      }));

      // Atualizar cache da landing page
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        (old: LandingPageConfigOutput | undefined) => {
          if (!old) return old;
          return {
            ...old,
            logoUrl: response.logoUrl,
            updatedAt: new Date().toISOString(),
          };
        }
      );

      // Limpar preview local após sucesso
      setPreview(null);
      setOriginalFile(null);

      toast({
        title: 'Logo atualizado!',
        description: 'Seu logo foi enviado com sucesso.',
        variant: 'default',
      });

      onSuccess?.(response.logoUrl);
    },
    onError: (error: Error) => {
      const errorMessage = error.message || 'Erro ao enviar logo.';
      
      setUploadState(prev => ({
        ...prev,
        status: 'error',
        error: errorMessage,
      }));

      toast({
        title: 'Erro no upload',
        description: errorMessage,
        variant: 'destructive',
      });

      onError?.(errorMessage);
    },
  });

  /**
   * Remoção do logo
   */
  const removeMutation = useMutation({
    mutationFn: () => landingPageApi.removeLogo(barbershopId),
    onSuccess: () => {
      // Atualizar cache
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        (old: LandingPageConfigOutput | undefined) => {
          if (!old) return old;
          return {
            ...old,
            logoUrl: undefined,
            updatedAt: new Date().toISOString(),
          };
        }
      );

      // Limpar estado local
      resetState();

      toast({
        title: 'Logo removido',
        description: 'Logo removido com sucesso.',
        variant: 'default',
      });
    },
    onError: (error: Error) => {
      toast({
        title: 'Erro ao remover',
        description: error.message || 'Erro ao remover logo.',
        variant: 'destructive',
      });
    },
  });

  // ============================================================================
  // Functions
  // ============================================================================

  /**
   * Criar preview do arquivo
   */
  const createPreview = useCallback((file: File) => {
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreview(reader.result as string);
    };
    reader.readAsDataURL(file);
  }, []);

  /**
   * Validar e selecionar arquivo
   */
  const selectFile = useCallback((file: File) => {
    // Limpar estado anterior
    setValidationError(null);
    setUploadState(prev => ({ ...prev, error: null }));

    // Validar arquivo
    const error = validateLogoFile(file);
    if (error) {
      setValidationError(error);
      toast({
        title: 'Arquivo inválido',
        description: error,
        variant: 'destructive',
      });
      return;
    }

    // Armazenar arquivo e criar preview
    setOriginalFile(file);
    createPreview(file);

    setUploadState(prev => ({
      ...prev,
      status: 'selected',
      progress: 0,
    }));

    // Auto upload se habilitado
    if (autoUpload) {
      uploadMutation.mutate(file);
    }
  }, [autoUpload, uploadMutation, createPreview]);

  /**
   * Upload manual (quando autoUpload = false)
   */
  const uploadFile = useCallback(async () => {
    if (!originalFile) {
      toast({
        title: 'Nenhum arquivo',
        description: 'Selecione um arquivo primeiro.',
        variant: 'destructive',
      });
      return;
    }

    uploadMutation.mutate(originalFile);
  }, [originalFile, uploadMutation]);

  /**
   * Remover arquivo local (cancelar upload)
   */
  const removeFile = useCallback(() => {
    setOriginalFile(null);
    setPreview(null);
    setValidationError(null);
    setUploadState({
      status: 'idle',
      progress: 0,
      url: null,
      error: null,
    });
  }, []);

  /**
   * Remover logo do servidor
   */
  const removeLogo = useCallback(async () => {
    removeMutation.mutate();
  }, [removeMutation]);

  /**
   * Reset completo do estado
   */
  const resetState = useCallback(() => {
    setOriginalFile(null);
    setPreview(null);
    setValidationError(null);
    setUploadState({
      status: 'idle',
      progress: 0,
      url: null,
      error: null,
    });
  }, []);

  // ============================================================================
  // Computed Values
  // ============================================================================

  const isUploading = uploadMutation.isPending;
  const canUpload = !!originalFile && !validationError && !isUploading;
  const hasPreview = !!preview;

  // ============================================================================
  // Return Interface
  // ============================================================================

  return {
    // State
    uploadState,
    preview,
    originalFile,
    
    // Actions
    selectFile,
    uploadFile,
    removeFile,
    removeLogo,
    resetState,
    
    // Status
    isUploading,
    canUpload,
    hasPreview,
    
    // Validation
    validationError,
  };
};

// ============================================================================
// Utility Hooks
// ============================================================================

/**
 * Hook simplificado para upload com drag & drop
 */
export const useLogoDropzone = (
  barbershopId: string,
  options: UseLogoUploadOptions = {}
) => {
  const logoUpload = useLogoUpload(barbershopId, options);
  const [isDragOver, setIsDragOver] = useState(false);

  const handleDragEnter = useCallback((e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(true);
  }, []);

  const handleDragLeave = useCallback((e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);
  }, []);

  const handleDragOver = useCallback((e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
  }, []);

  const handleDrop = useCallback((e: DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setIsDragOver(false);

    const files = Array.from(e.dataTransfer?.files || []);
    const file = files[0];

    if (file) {
      logoUpload.selectFile(file);
    }
  }, [logoUpload]);

  const handleFileInput = useCallback((e: Event) => {
    const target = e.target as HTMLInputElement;
    const file = target.files?.[0];

    if (file) {
      logoUpload.selectFile(file);
    }
  }, [logoUpload]);

  return {
    ...logoUpload,
    isDragOver,
    dragHandlers: {
      onDragEnter: handleDragEnter,
      onDragLeave: handleDragLeave,
      onDragOver: handleDragOver,
      onDrop: handleDrop,
    },
    handleFileInput,
  };
};

// ============================================================================
// Validation Utilities
// ============================================================================

/**
 * Validar dimensões da imagem
 */
export const validateImageDimensions = (
  file: File,
  maxWidth: number = 1000,
  maxHeight: number = 1000
): Promise<boolean> => {
  return new Promise((resolve) => {
    const img = new Image();
    const url = URL.createObjectURL(file);

    img.onload = () => {
      URL.revokeObjectURL(url);
      resolve(img.width <= maxWidth && img.height <= maxHeight);
    };

    img.onerror = () => {
      URL.revokeObjectURL(url);
      resolve(false);
    };

    img.src = url;
  });
};

/**
 * Redimensionar imagem no client-side
 */
export const resizeImage = (
  file: File,
  maxWidth: number = 300,
  maxHeight: number = 300,
  quality: number = 0.8
): Promise<Blob> => {
  return new Promise((resolve, reject) => {
    const canvas = document.createElement('canvas');
    const ctx = canvas.getContext('2d');
    const img = new Image();

    img.onload = () => {
      const { width, height } = img;
      
      // Calcular novas dimensões mantendo proporção
      let newWidth = width;
      let newHeight = height;

      if (width > height) {
        if (width > maxWidth) {
          newHeight = (height * maxWidth) / width;
          newWidth = maxWidth;
        }
      } else {
        if (height > maxHeight) {
          newWidth = (width * maxHeight) / height;
          newHeight = maxHeight;
        }
      }

      canvas.width = newWidth;
      canvas.height = newHeight;

      ctx?.drawImage(img, 0, 0, newWidth, newHeight);

      canvas.toBlob(
        (blob) => {
          if (blob) {
            resolve(blob);
          } else {
            reject(new Error('Erro ao redimensionar imagem'));
          }
        },
        file.type,
        quality
      );
    };

    img.onerror = () => reject(new Error('Erro ao carregar imagem'));
    img.src = URL.createObjectURL(file);
  });
};

// ============================================================================
// Export
// ============================================================================

export type { UseLogoUploadResult, UseLogoUploadOptions };
export { UPLOAD_CONFIG };
export default useLogoUpload;