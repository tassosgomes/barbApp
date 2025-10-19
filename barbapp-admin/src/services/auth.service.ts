import api from './api';
import { formatPhoneToAPI } from '@/lib/phone-utils';
import type { LoginInput, AuthResponse, User } from '@/types/auth.types';

/**
 * Serviço de autenticação para barbeiros
 * 
 * Responsável por:
 * - Login de barbeiros usando telefone e código da barbearia
 * - Validação de token JWT
 * - Logout (remoção de token)
 */
export const authService = {
  /**
   * Realiza login de barbeiro
   * 
   * @param data - Dados de login (código da barbearia e telefone)
   * @returns Promise com token JWT e dados do usuário
   * 
   * @example
   * const response = await authService.login({
   *   barbershopCode: 'BARB001',
   *   phone: '(11) 99999-9999'
   * });
   * 
   * @throws {Error} 401 - Credenciais inválidas
   * @throws {Error} 400 - Dados de entrada inválidos
   * @throws {Error} 500 - Erro interno do servidor
   */
  login: async (data: LoginInput): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/barbeiro/login', {
      barbershopCode: data.barbershopCode.toUpperCase(),
      phone: formatPhoneToAPI(data.phone)
    });
    
    return response.data;
  },
  
  /**
   * Valida token JWT e retorna dados do usuário
   * Usado para verificar sessão ao carregar aplicação
   * 
   * @returns Promise com dados do usuário autenticado
   * 
   * @example
   * const user = await authService.validateToken();
   * 
   * @throws {Error} 401 - Token inválido ou expirado
   */
  validateToken: async (): Promise<User> => {
    // Endpoint para validar token e buscar dados do usuário
    const response = await api.get<User>('/barber/profile');
    return response.data;
  },
  
  /**
   * Realiza logout removendo token do localStorage
   * 
   * @example
   * authService.logout();
   */
  logout: () => {
    localStorage.removeItem('barbapp-barber-token');
  }
};
