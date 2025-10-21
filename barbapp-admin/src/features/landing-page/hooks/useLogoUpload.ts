import { useState, useCallback } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { showSuccessToast, showErrorToast } from '@/utils/toast';
import { LOGO_UPLOAD_CONFIG } from '../constants/validation';
import type { 
  LandingPageConfig, 
  UseLogoUploadResult,
  LogoUploadValidationError,
} from '../types/landing-page.types';

const QUERY_KEYS = {
  landingPage: (barbershopId: string) => ['landing-page', barbershopId] as const,
} as const;

export function useLogoUpload(barbershopId: string): UseLogoUploadResult {
  const queryClient = useQueryClient();
  const [validationError, setValidationError] = useState<LogoUploadValidationError | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);

  const validateFile = useCallback((file: File): LogoUploadValidationError | null => {
    // Check file size
    if (file.size > LOGO_UPLOAD_CONFIG.maxSize) {
      return {
        type: 'size',
        message: `Arquivo muito grande. Tamanho máximo: ${LOGO_UPLOAD_CONFIG.maxSize / 1024 / 1024}MB`,
      };
    }

    // Check file type
    if (!LOGO_UPLOAD_CONFIG.allowedTypes.includes(file.type)) {
      return {
        type: 'type',
        message: `Tipo de arquivo não suportado. Use: ${LOGO_UPLOAD_CONFIG.allowedTypes.join(', ')}`,
      };
    }

    return null;
  }, []);

  const uploadMutation = useMutation({
    mutationFn: (file: File) => landingPageApi.uploadLogo(barbershopId, file),
    onSuccess: (logoUrl: string) => {
      queryClient.setQueryData(
        QUERY_KEYS.landingPage(barbershopId),
        (oldData: LandingPageConfig | undefined) => {
          if (!oldData) return oldData;
          return { ...oldData, logoUrl };
        }
      );
      setValidationError(null);
      setPreviewUrl(null);
      showSuccessToast(
        'Logo atualizado',
        'O logo da sua landing page foi atualizado com sucesso!'
      );
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao fazer upload do logo';
      showErrorToast('Erro no upload', message);
    },
  });

  const deleteMutation = useMutation({
    mutationFn: () => landingPageApi.deleteLogo(barbershopId),
    onSuccess: () => {
      queryClient.setQueryData(
        QUERY_KEYS.landingPage(barbershopId),
        (oldData: LandingPageConfig | undefined) => {
          if (!oldData) return oldData;
          return { ...oldData, logoUrl: undefined };
        }
      );
      setPreviewUrl(null);
      showSuccessToast('Logo removido', 'O logo foi removido da sua landing page');
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao remover logo';
      showErrorToast('Erro na remoção', message);
    },
  });

  const uploadLogo = useCallback((file: File) => {
    const error = validateFile(file);
    if (error) {
      setValidationError(error);
      showErrorToast('Arquivo inválido', error.message);
      return;
    }

    setValidationError(null);
    uploadMutation.mutate(file);
  }, [validateFile, uploadMutation]);

  const deleteLogo = useCallback(() => {
    deleteMutation.mutate();
  }, [deleteMutation]);

  const createPreview = useCallback((file: File) => {
    const error = validateFile(file);
    if (error) {
      setValidationError(error);
      setPreviewUrl(null);
      return;
    }

    setValidationError(null);
    const url = URL.createObjectURL(file);
    setPreviewUrl(url);

    // Cleanup previous preview URL
    return () => {
      if (previewUrl) {
        URL.revokeObjectURL(previewUrl);
      }
    };
  }, [validateFile, previewUrl]);

  const clearPreview = useCallback(() => {
    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
      setPreviewUrl(null);
    }
    setValidationError(null);
  }, [previewUrl]);

  return {
    // Loading states
    isUploading: uploadMutation.isPending,
    isDeleting: deleteMutation.isPending,
    
    // Error states
    uploadError: uploadMutation.error,
    deleteError: deleteMutation.error,
    validationError,
    
    // Preview state
    previewUrl,
    
    // Actions
    uploadLogo,
    deleteLogo,
    createPreview,
    clearPreview,
    validateFile,
  };
}

export { QUERY_KEYS };