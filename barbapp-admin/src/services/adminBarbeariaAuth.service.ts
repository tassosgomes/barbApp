import api from './api';
import { TokenManager, UserType } from './tokenManager';

/**
 * Request payload for Admin Barbearia login
 */
export interface LoginAdminBarbeariaRequest {
  codigo: string;
  email: string;
  senha: string;
}

/**
 * Response from Admin Barbearia login endpoint (from backend)
 */
interface BackendAuthResponse {
  token: string;
  barbeariaId: string;
  nomeBarbearia: string;
  codigoBarbearia: string;
  expiresAt: string;
}

/**
 * Response from Admin Barbearia login endpoint (mapped to frontend format)
 */
export interface LoginAdminBarbeariaResponse {
  token: string;
  barbeariaId: string;
  nome: string;
  codigo: string;
  expiresAt: string;
}

/**
 * Service for Admin Barbearia authentication operations
 * IMPORTANTE: Usa TokenManager para evitar conflitos com outros tipos de usuários
 */
export const adminBarbeariaAuthService = {
  /**
   * Authenticate Admin Barbearia with email, password and barbershop code
   * IMPORTANTE: Limpa automaticamente tokens de outros tipos de usuários
   * 
   * @param request - Login credentials and barbershop code
   * @returns Authentication response with token and barbershop info
   */
  login: async (request: LoginAdminBarbeariaRequest): Promise<LoginAdminBarbeariaResponse> => {
    const response = await api.post<BackendAuthResponse>(
      '/auth/admin-barbearia/login',
      request
    );

    // Store token using TokenManager (clears conflicting tokens automatically)
    TokenManager.setToken(UserType.ADMIN_BARBEARIA, response.data.token);

    // Map backend response to frontend format
    return {
      token: response.data.token,
      barbeariaId: response.data.barbeariaId,
      nome: response.data.nomeBarbearia,
      codigo: response.data.codigoBarbearia,
      expiresAt: response.data.expiresAt,
    };
  },

  /**
   * Logout Admin Barbearia by clearing stored token and context
   * Usa TokenManager para limpeza completa
   */
  logout: (): void => {
    TokenManager.logout(UserType.ADMIN_BARBEARIA);
  },

  /**
   * Get stored Admin Barbearia token
   * @returns JWT token or null if not authenticated
   */
  getToken: (): string | null => {
    return TokenManager.getToken(UserType.ADMIN_BARBEARIA);
  },

  /**
   * Check if Admin Barbearia is authenticated
   * @returns true if token exists in localStorage
   */
  isAuthenticated: (): boolean => {
    return !!TokenManager.getToken(UserType.ADMIN_BARBEARIA);
  },
};