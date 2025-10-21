import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { showSuccessToast, showErrorToast } from '@/utils/toast';
import type {
  LandingPageConfig,
  CreateLandingPageInput,
  UpdateLandingPageInput,
  UseLandingPageResult,
} from '../types/landing-page.types';

const QUERY_KEYS = {
  landingPage: (barbershopId: string) => ['landing-page', barbershopId] as const,
  templates: ['landing-page-templates'] as const,
} as const;

export function useLandingPage(barbershopId: string): UseLandingPageResult {
  const queryClient = useQueryClient();

  const {
    data: config,
    isLoading,
    error,
    refetch,
  } = useQuery({
    queryKey: QUERY_KEYS.landingPage(barbershopId),
    queryFn: () => landingPageApi.getConfig(barbershopId),
    enabled: !!barbershopId,
    staleTime: 5 * 60 * 1000, // 5 minutes
    retry: (failureCount, error: unknown) => {
      if ((error as { response?: { status?: number } })?.response?.status === 404) return false;
      return failureCount < 2;
    },
  });

  const createMutation = useMutation({
    mutationFn: (payload: CreateLandingPageInput) =>
      landingPageApi.createConfig(barbershopId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QUERY_KEYS.landingPage(barbershopId),
      });
      showSuccessToast(
        'Landing page criada',
        'Sua landing page foi criada com sucesso!'
      );
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao criar landing page';
      showErrorToast('Erro na criação', message);
    },
  });

  const updateMutation = useMutation({
    mutationFn: (payload: UpdateLandingPageInput) =>
      landingPageApi.updateConfig(barbershopId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QUERY_KEYS.landingPage(barbershopId),
      });
      showSuccessToast(
        'Landing page atualizada',
        'Suas alterações foram salvas com sucesso!'
      );
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao atualizar landing page';
      showErrorToast('Erro na atualização', message);
    },
  });

  const publishMutation = useMutation({
    mutationFn: () => landingPageApi.publishConfig(barbershopId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QUERY_KEYS.landingPage(barbershopId),
      });
      showSuccessToast(
        'Landing page publicada',
        'Sua landing page está agora disponível publicamente!'
      );
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao publicar landing page';
      showErrorToast('Erro na publicação', message);
    },
  });

  const unpublishMutation = useMutation({
    mutationFn: () => landingPageApi.unpublishConfig(barbershopId),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: QUERY_KEYS.landingPage(barbershopId),
      });
      showSuccessToast(
        'Landing page despublicada',
        'Sua landing page não está mais pública'
      );
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao despublicar landing page';
      showErrorToast('Erro na despublicação', message);
    },
  });

  const uploadLogoMutation = useMutation({
    mutationFn: (file: File) => landingPageApi.uploadLogo(barbershopId, file),
    onSuccess: (logoUrl: string) => {
      queryClient.setQueryData(
        QUERY_KEYS.landingPage(barbershopId),
        (oldData: LandingPageConfig | undefined) => {
          if (!oldData) return oldData;
          return { ...oldData, logoUrl };
        }
      );
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

  const deleteLogoMutation = useMutation({
    mutationFn: () => landingPageApi.deleteLogo(barbershopId),
    onSuccess: () => {
      queryClient.setQueryData(
        QUERY_KEYS.landingPage(barbershopId),
        (oldData: LandingPageConfig | undefined) => {
          if (!oldData) return oldData;
          return { ...oldData, logoUrl: undefined };
        }
      );
      showSuccessToast('Logo removido', 'O logo foi removido da sua landing page');
    },
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao remover logo';
      showErrorToast('Erro na remoção', message);
    },
  });

  const previewMutation = useMutation({
    mutationFn: () => landingPageApi.generatePreview(barbershopId),
    onError: (error: unknown) => {
      const message = (error as { response?: { data?: { message?: string } } })?.response?.data?.message || 'Erro ao gerar preview';
      showErrorToast('Erro no preview', message);
    },
  });

  return {
    // Data
    config,
    
    // Loading states
    isLoading,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isPublishing: publishMutation.isPending,
    isUnpublishing: unpublishMutation.isPending,
    isUploadingLogo: uploadLogoMutation.isPending,
    isDeletingLogo: deleteLogoMutation.isPending,
    isGeneratingPreview: previewMutation.isPending,
    
    // Error states
    error,
    createError: createMutation.error,
    updateError: updateMutation.error,
    publishError: publishMutation.error,
    unpublishError: unpublishMutation.error,
    uploadLogoError: uploadLogoMutation.error,
    deleteLogoError: deleteLogoMutation.error,
    previewError: previewMutation.error,
    
    // Actions
    createConfig: createMutation.mutate,
    updateConfig: updateMutation.mutate,
    publishConfig: publishMutation.mutate,
    unpublishConfig: unpublishMutation.mutate,
    uploadLogo: uploadLogoMutation.mutate,
    deleteLogo: deleteLogoMutation.mutate,
    generatePreview: previewMutation.mutate,
    refetch,
    
    // Utils
    invalidateQueries: () => {
      queryClient.invalidateQueries({
        queryKey: QUERY_KEYS.landingPage(barbershopId),
      });
    },
  };
}

export { QUERY_KEYS };