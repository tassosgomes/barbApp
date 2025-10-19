/**
 * Tipos relacionados à autenticação de barbeiros
 */

/**
 * Dados de entrada para login
 */
export interface LoginInput {
  email: string;
  password: string;
}

/**
 * Resposta da API de autenticação
 */
export interface AuthResponse {
  token: string;
  tipoUsuario: string;
  barbeariaId: string | null;
  nomeBarbearia: string;
  codigoBarbearia: string | null;
  expiresAt: string;
}

/**
 * Dados do usuário autenticado (Barbeiro)
 */
export interface User {
  id: string;
  name: string;
  email: string;
  role: 'Barbeiro';
  barbeariaId: string;
  nomeBarbearia: string;
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
