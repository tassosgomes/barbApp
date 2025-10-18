import api from './api';

/**
 * Request payload for Admin Barbearia login
 */
export interface LoginAdminBarbeariaRequest {
  codigo: string;
  email: string;
  senha: string;
}

/**
 * Response from Admin Barbearia login endpoint
 */
export interface LoginAdminBarbeariaResponse {
  token: string;
  barbeariaId: string;
  nome: string;
  codigo: string;
  expiresAt: string;
}

/**
 * Local storage key for Admin Barbearia token
 */
const TOKEN_KEY = 'admin_barbearia_token';

/**
 * Service for Admin Barbearia authentication operations
 */
export const adminBarbeariaAuthService = {
  /**
   * Authenticate Admin Barbearia with email, password and barbershop code
   * @param request - Login credentials and barbershop code
   * @returns Authentication response with token and barbershop info
   */
  login: async (request: LoginAdminBarbeariaRequest): Promise<LoginAdminBarbeariaResponse> => {
    const response = await api.post<LoginAdminBarbeariaResponse>(
      '/api/auth/admin-barbearia/login',
      request
    );

    // Store token in localStorage
    localStorage.setItem(TOKEN_KEY, response.data.token);

    return response.data;
  },

  /**
   * Logout Admin Barbearia by clearing stored token
   */
  logout: (): void => {
    localStorage.removeItem(TOKEN_KEY);
  },

  /**
   * Get stored Admin Barbearia token
   * @returns JWT token or null if not authenticated
   */
  getToken: (): string | null => {
    return localStorage.getItem(TOKEN_KEY);
  },

  /**
   * Check if Admin Barbearia is authenticated
   * @returns true if token exists in localStorage
   */
  isAuthenticated: (): boolean => {
    return !!localStorage.getItem(TOKEN_KEY);
  },
};