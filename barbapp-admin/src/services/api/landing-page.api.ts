/**
 * Landing Page API Service
 * 
 * Serviço para integração com API backend do módulo de Landing Page.
 * Inclui endpoints para gerenciar configuração, upload de logo e operações CRUD.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import api from '../api';
import {
  LandingPageConfig,
  CreateLandingPageInput,
  UpdateLandingPageInput,
  ApiResponse,
  Template,
} from '@/features/landing-page/types/landing-page.types';

// ============================================================================
// API Endpoints
// ============================================================================

/**
 * Service para operações da Landing Page
 */
export const landingPageApi = {
  /**
   * Busca configuração da landing page por ID da barbearia
   */
  getConfig: async (barbershopId: string): Promise<LandingPageConfig> => {
    const { data } = await api.get<LandingPageConfig>(
      `/admin/landing-pages/${barbershopId}`
    );
    return data;
  },

  /**
   * Cria nova configuração de landing page
   */
  createConfig: async (barbershopId: string, payload: CreateLandingPageInput): Promise<LandingPageConfig> => {
    const { data } = await api.post<LandingPageConfig>(
      `/admin/landing-pages/${barbershopId}`,
      payload
    );
    return data;
  },

  /**
   * Atualiza configuração existente da landing page
   */
  updateConfig: async (
    barbershopId: string,
    payload: UpdateLandingPageInput
  ): Promise<void> => {
    await api.put(`/admin/landing-pages/${barbershopId}`, payload);
  },

  /**
   * Remove configuração da landing page
   */
  deleteConfig: async (barbershopId: string): Promise<void> => {
    await api.delete(`/admin/landing-pages/${barbershopId}`);
  },

  /**
   * Upload de logo da landing page
   */
  uploadLogo: async (barbershopId: string, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('file', file);

    const { data } = await api.post<ApiResponse<{ logoUrl: string }>>(
      `/admin/landing-pages/${barbershopId}/logo`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
        timeout: 30000, // Upload pode demorar mais
      }
    );

    return data.data!.logoUrl;
  },

  /**
   * Remove logo da landing page
   */
  removeLogo: async (barbershopId: string): Promise<void> => {
    await api.delete(`/admin/landing-pages/${barbershopId}/logo`);
  },

  /**
   * Alias for removeLogo (for consistency with hooks)
   */
  deleteLogo: async (barbershopId: string): Promise<void> => {
    await api.delete(`/admin/landing-pages/${barbershopId}/logo`);
  },

  /**
   * Publica/despublica landing page
   */
  togglePublish: async (barbershopId: string, isPublished: boolean): Promise<void> => {
    await api.patch(`/admin/landing-pages/${barbershopId}/publish`, {
      isPublished,
    });
  },

  /**
   * Publica landing page
   */
  publishConfig: async (barbershopId: string): Promise<void> => {
    await api.patch(`/admin/landing-pages/${barbershopId}/publish`, {
      isPublished: true,
    });
  },

  /**
   * Despublica landing page
   */
  unpublishConfig: async (barbershopId: string): Promise<void> => {
    await api.patch(`/admin/landing-pages/${barbershopId}/publish`, {
      isPublished: false,
    });
  },

  /**
   * Gera preview da landing page
   */
  generatePreview: async (barbershopId: string): Promise<string> => {
    const { data } = await api.post<ApiResponse<{ previewUrl: string }>>(
      `/admin/landing-pages/${barbershopId}/preview`
    );
    return data.data!.previewUrl;
  },

  /**
   * Busca URL pública da landing page
   */
  getPublicUrl: async (barbershopId: string): Promise<string> => {
    const { data } = await api.get<ApiResponse<{ publicUrl: string }>>(
      `/admin/landing-pages/${barbershopId}/public-url`
    );
    return data.data!.publicUrl;
  },

  /**
   * Valida configuração da landing page
   */
  validateConfig: async (
    barbershopId: string,
    config: UpdateLandingPageInput
  ): Promise<{ isValid: boolean; errors?: string[] }> => {
    const { data } = await api.post<ApiResponse<{ isValid: boolean; errors?: string[] }>>(
      `/admin/landing-pages/${barbershopId}/validate`,
      config
    );
    return data.data!;
  },

  /**
   * Busca templates disponíveis
   */
  getTemplates: async (): Promise<Template[]> => {
    const { data } = await api.get<ApiResponse<{ templates: Template[] }>>(
      '/admin/landing-pages/templates'
    );
    return data.data!.templates;
  },
};

// ============================================================================
// Query Keys para TanStack Query
// ============================================================================

/**
 * Chaves de query para uso com TanStack Query
 */
export const landingPageKeys = {
  all: ['landingPage'] as const,
  configs: () => [...landingPageKeys.all, 'configs'] as const,
  config: (barbershopId: string) => [...landingPageKeys.configs(), barbershopId] as const,
  publicUrl: (barbershopId: string) => [...landingPageKeys.all, 'publicUrl', barbershopId] as const,
} as const;

// ============================================================================
// Error Handling
// ============================================================================

/**
 * Tipos de erro específicos da Landing Page API
 */
export class LandingPageApiError extends Error {
  constructor(
    message: string,
    public statusCode?: number,
    public details?: unknown
  ) {
    super(message);
    this.name = 'LandingPageApiError';
  }
}

/**
 * Helper para tratar erros da API
 */
export const handleApiError = (error: unknown): LandingPageApiError => {
  if (error instanceof LandingPageApiError) {
    return error;
  }

  if (typeof error === 'object' && error !== null && 'response' in error) {
    const axiosError = error as { response?: { status?: number; data?: { message?: string } }; message?: string };
    const status = axiosError.response?.status;
    const message = axiosError.response?.data?.message || axiosError.message || 'Erro desconhecido';
    
    return new LandingPageApiError(message, status, axiosError.response?.data);
  }

  return new LandingPageApiError(
    error instanceof Error ? error.message : 'Erro desconhecido na API'
  );
};

// ============================================================================
// Default Export
// ============================================================================

export default landingPageApi;