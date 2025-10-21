/**
 * Landing Page API Service
 * 
 * Serviço para integração com API do backend para gerenciamento
 * de landing pages. Inclui operações CRUD, upload de logo e
 * busca de dados públicos.
 * 
 * @version 1.0
 * @date 2025-10-21
 */

import api from '@/services/api';
import type {
  LandingPageConfigOutput,
  CreateLandingPageInput,
  UpdateLandingPageInput,
  PublicLandingPageOutput,
  LogoUploadResponse,
} from '@/features/landing-page';

// ============================================================================
// Types
// ============================================================================

interface ApiResponse<T> {
  data: T;
  message?: string;
  status: number;
}

// ============================================================================
// API Service Implementation
// ============================================================================

export const landingPageApi = {
  /**
   * Buscar configuração da landing page (Admin)
   */
  getConfig: async (barbershopId: string): Promise<LandingPageConfigOutput> => {
    const response = await api.get<LandingPageConfigOutput>(
      `/admin/landing-pages/${barbershopId}`
    );
    return response.data;
  },

  /**
   * Criar nova landing page (Admin)
   */
  createConfig: async (data: CreateLandingPageInput): Promise<LandingPageConfigOutput> => {
    const response = await api.post<LandingPageConfigOutput>(
      '/admin/landing-pages',
      data
    );
    return response.data;
  },

  /**
   * Atualizar configuração da landing page (Admin)
   */
  updateConfig: async (
    barbershopId: string,
    data: UpdateLandingPageInput
  ): Promise<LandingPageConfigOutput> => {
    const response = await api.put<LandingPageConfigOutput>(
      `/admin/landing-pages/${barbershopId}`,
      data
    );
    return response.data;
  },

  /**
   * Upload de logo da barbearia
   */
  uploadLogo: async (barbershopId: string, file: File): Promise<LogoUploadResponse> => {
    const formData = new FormData();
    formData.append('logo', file);

    const response = await api.post<LogoUploadResponse>(
      `/admin/landing-pages/${barbershopId}/logo`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  },

  /**
   * Remover logo da barbearia
   */
  removeLogo: async (barbershopId: string): Promise<void> => {
    await api.delete(`/admin/landing-pages/${barbershopId}/logo`);
  },

  /**
   * Buscar landing page pública (sem autenticação)
   */
  getPublicData: async (barbershopCode: string): Promise<PublicLandingPageOutput> => {
    const response = await api.get<PublicLandingPageOutput>(
      `/public/barbershops/${barbershopCode}/landing-page`
    );
    return response.data;
  },

  /**
   * Publicar/despublicar landing page
   */
  togglePublish: async (barbershopId: string, isPublished: boolean): Promise<void> => {
    await api.patch(`/admin/landing-pages/${barbershopId}/publish`, {
      isPublished,
    });
  },

  /**
   * Duplicar template de landing page
   */
  duplicateTemplate: async (
    sourceBarbershopId: string,
    targetBarbershopId: string
  ): Promise<LandingPageConfigOutput> => {
    const response = await api.post<LandingPageConfigOutput>(
      `/admin/landing-pages/${targetBarbershopId}/duplicate`,
      {
        sourceBarbershopId,
      }
    );
    return response.data;
  },

  /**
   * Buscar estatísticas da landing page (futuro)
   */
  getAnalytics: async (barbershopId: string, period?: string) => {
    const response = await api.get(
      `/admin/landing-pages/${barbershopId}/analytics`,
      {
        params: { period },
      }
    );
    return response.data;
  },
};

// ============================================================================
// Utility Functions
// ============================================================================

/**
 * Validar arquivo de logo antes do upload
 */
export const validateLogoFile = (file: File): string | null => {
  const MAX_SIZE = 2 * 1024 * 1024; // 2MB
  const ALLOWED_TYPES = ['image/jpeg', 'image/png', 'image/svg+xml'];

  if (!ALLOWED_TYPES.includes(file.type)) {
    return 'Formato inválido. Use JPG, PNG ou SVG.';
  }

  if (file.size > MAX_SIZE) {
    return 'Arquivo muito grande. Tamanho máximo: 2MB.';
  }

  return null;
};

/**
 * Gerar URL de preview da landing page
 */
export const generatePreviewUrl = (barbershopCode: string): string => {
  const baseUrl = import.meta.env.VITE_PUBLIC_URL || 'https://app.barbapp.com';
  return `${baseUrl}/barbearia/${barbershopCode}`;
};

/**
 * Gerar URL de compartilhamento para WhatsApp
 */
export const generateWhatsAppShareUrl = (landingPageUrl: string): string => {
  const message = encodeURIComponent(
    `Confira nossa barbearia! Agende seu horário: ${landingPageUrl}`
  );
  return `https://wa.me/?text=${message}`;
};

// ============================================================================
// Export Default
// ============================================================================

export default landingPageApi;