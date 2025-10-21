/**
 * useLandingPage Hook
 * 
 * Hook customizado para gerenciar estado e operações da landing page
 * no painel admin. Inclui integração com TanStack Query para cache,
 * invalidação automática, tratamento de erros e loading states.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from '@/components/ui/use-toast';
import { landingPageApi } from '@/services/api/landing-page.api';
import type {
  LandingPageConfigOutput,
  UpdateLandingPageInput,
  CreateLandingPageInput,
  UseLandingPageResult,
} from '../types/landing-page.types';

// ============================================================================
// Query Keys
// ============================================================================

export const LANDING_PAGE_QUERY_KEYS = {
  all: ['landingPage'] as const,
  configs: () => [...LANDING_PAGE_QUERY_KEYS.all, 'config'] as const,
  config: (barbershopId: string) => 
    [...LANDING_PAGE_QUERY_KEYS.configs(), barbershopId] as const,
  public: (code: string) =>
    [...LANDING_PAGE_QUERY_KEYS.all, 'public', code] as const,
  analytics: (barbershopId: string) =>
    [...LANDING_PAGE_QUERY_KEYS.all, 'analytics', barbershopId] as const,
} as const;

// ============================================================================
// Hook Implementation
// ============================================================================

export const useLandingPage = (barbershopId: string): UseLandingPageResult => {
  const queryClient = useQueryClient();

  // ============================================================================
  // Queries
  // ============================================================================

  /**
   * Buscar configuração da landing page
   */
  const {
    data: config,
    isLoading,
    isError,
    error,
    refetch,
  } = useQuery({
    queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
    queryFn: () => landingPageApi.getConfig(barbershopId),
    staleTime: 5 * 60 * 1000, // 5 minutos
    gcTime: 10 * 60 * 1000, // 10 minutos (anteriormente cacheTime)
    retry: 2,
    retryDelay: (attemptIndex) => Math.min(1000 * 2 ** attemptIndex, 30000),
    enabled: !!barbershopId,
  });

  // ============================================================================
  // Mutations
  // ============================================================================

  /**
   * Criar nova landing page
   */
  const createMutation = useMutation({
    mutationFn: (data: CreateLandingPageInput) => landingPageApi.createConfig(data),
    onSuccess: (data) => {
      // Invalidar queries relacionadas
      queryClient.invalidateQueries({
        queryKey: LANDING_PAGE_QUERY_KEYS.configs(),
      });
      
      // Adicionar aos dados em cache
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        data
      );

      toast({
        title: 'Landing page criada!',
        description: 'Sua landing page foi criada com sucesso.',
        variant: 'default',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao criar',
        description: error.message || 'Erro ao criar landing page.',
        variant: 'destructive',
      });
    },
  });

  /**
   * Atualizar configuração da landing page
   */
  const updateMutation = useMutation({
    mutationFn: (data: UpdateLandingPageInput) => 
      landingPageApi.updateConfig(barbershopId, data),
    onMutate: async (newData) => {
      // Cancelar queries em andamento
      await queryClient.cancelQueries({
        queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
      });

      // Snapshot do valor anterior
      const previousConfig = queryClient.getQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId)
      );

      // Atualização otimista
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        (old: LandingPageConfigOutput | undefined) => {
          if (!old) return old;
          return {
            ...old,
            ...newData,
            updatedAt: new Date().toISOString(),
          };
        }
      );

      return { previousConfig };
    },
    onSuccess: (data) => {
      // Atualizar cache com dados reais do servidor
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        data
      );

      toast({
        title: 'Alterações salvas!',
        description: 'Landing page atualizada com sucesso.',
        variant: 'default',
      });
    },
    onError: (error: any, newData, context) => {
      // Reverter para estado anterior em caso de erro
      if (context?.previousConfig) {
        queryClient.setQueryData(
          LANDING_PAGE_QUERY_KEYS.config(barbershopId),
          context.previousConfig
        );
      }

      toast({
        title: 'Erro ao salvar',
        description: error.message || 'Erro ao atualizar landing page.',
        variant: 'destructive',
      });
    },
    onSettled: () => {
      // Revalidar dados após mutação
      queryClient.invalidateQueries({
        queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
      });
    },
  });

  /**
   * Publicar/despublicar landing page
   */
  const togglePublishMutation = useMutation({
    mutationFn: (isPublished: boolean) => 
      landingPageApi.togglePublish(barbershopId, isPublished),
    onSuccess: (_, isPublished) => {
      // Atualizar cache
      queryClient.setQueryData(
        LANDING_PAGE_QUERY_KEYS.config(barbershopId),
        (old: LandingPageConfigOutput | undefined) => {
          if (!old) return old;
          return { ...old, isPublished };
        }
      );

      toast({
        title: isPublished ? 'Landing page publicada!' : 'Landing page despublicada!',
        description: isPublished 
          ? 'Sua landing page está agora online.' 
          : 'Sua landing page foi despublicada.',
        variant: 'default',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.message || 'Erro ao alterar status de publicação.',
        variant: 'destructive',
      });
    },
  });

  // ============================================================================
  // Utility Functions
  // ============================================================================

  /**
   * Invalidar cache manualmente
   */
  const invalidateCache = () => {
    queryClient.invalidateQueries({
      queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
    });
  };

  /**
   * Limpar cache da landing page
   */
  const clearCache = () => {
    queryClient.removeQueries({
      queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
    });
  };

  /**
   * Pré-carregar dados da landing page
   */
  const prefetchConfig = async () => {
    await queryClient.prefetchQuery({
      queryKey: LANDING_PAGE_QUERY_KEYS.config(barbershopId),
      queryFn: () => landingPageApi.getConfig(barbershopId),
      staleTime: 5 * 60 * 1000,
    });
  };

  // ============================================================================
  // Return Interface
  // ============================================================================

  return {
    // Data
    config,
    isLoading,
    isError,
    error,

    // Actions
    createConfig: createMutation.mutate,
    updateConfig: updateMutation.mutate,
    togglePublish: togglePublishMutation.mutate,
    refetch,

    // States
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isToggling: togglePublishMutation.isPending,

    // Utilities
    invalidateCache,
    clearCache,
    prefetchConfig,

    // Computed values
    isReady: !isLoading && !isError && !!config,
    hasConfig: !!config,
    isPublished: config?.isPublished ?? false,
  };
};

// ============================================================================
// Helper Hooks
// ============================================================================

/**
 * Hook para buscar landing page pública (para preview)
 */
export const usePublicLandingPage = (barbershopCode: string) => {
  return useQuery({
    queryKey: LANDING_PAGE_QUERY_KEYS.public(barbershopCode),
    queryFn: () => landingPageApi.getPublicData(barbershopCode),
    staleTime: 2 * 60 * 1000, // 2 minutos
    gcTime: 5 * 60 * 1000, // 5 minutos
    enabled: !!barbershopCode,
    retry: 1,
  });
};

/**
 * Hook para duplicar template
 */
export const useDuplicateTemplate = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ 
      sourceBarbershopId, 
      targetBarbershopId 
    }: {
      sourceBarbershopId: string;
      targetBarbershopId: string;
    }) => landingPageApi.duplicateTemplate(sourceBarbershopId, targetBarbershopId),
    onSuccess: (data, variables) => {
      // Invalidar cache do target
      queryClient.invalidateQueries({
        queryKey: LANDING_PAGE_QUERY_KEYS.config(variables.targetBarbershopId),
      });

      toast({
        title: 'Template duplicado!',
        description: 'Template copiado com sucesso.',
        variant: 'default',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro ao duplicar',
        description: error.message || 'Erro ao duplicar template.',
        variant: 'destructive',
      });
    },
  });
};

// ============================================================================
// Export
// ============================================================================

export type { UseLandingPageResult };
export default useLandingPage;