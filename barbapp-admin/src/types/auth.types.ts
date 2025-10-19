/**
 * Tipos relacionados à autenticação de barbeiros
 */

/**
 * Dados de entrada para login
 */
export interface LoginInput {
  barbershopCode: string;
  phone: string;
}

/**
 * Resposta da API de autenticação
 */
export interface AuthResponse {
  token: string;
  user: User;
}

/**
 * Dados do usuário autenticado (Barbeiro)
 */
export interface User {
  id: string;
  name: string;
  phone: string;
  role: 'Barbeiro';
  barbershopId?: string;
}

/**
 * Contexto de autenticação disponível na aplicação
 */
export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginInput) => Promise<void>;
  logout: () => void;
  validateSession: () => Promise<boolean>;
}
