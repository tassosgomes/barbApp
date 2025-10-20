import { createContext, useState, useEffect, useContext, ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '@/services/auth.service';
import { barbershopService } from '@/services/barbershop.service';
import { TokenManager, UserType } from '@/services/tokenManager';
import type { AuthContextType, User, LoginInput } from '@/types/auth.types';

export const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Provider do contexto de autenticação
 * 
 * Responsável por:
 * - Gerenciar estado global do usuário autenticado
 * - Validar sessão ao carregar aplicação
 * - Realizar login e logout
 * - Navegar automaticamente após login/logout
 * 
 * @example
 * ```tsx
 * <AuthProvider>
 *   <App />
 * </AuthProvider>
 * ```
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();
  
  /**
   * Valida sessão ao carregar app
   */
  useEffect(() => {
    validateSession();
  }, []);
  
  /**
   * Valida se há sessão ativa verificando token no localStorage
   * 
   * @returns Promise<boolean> - true se sessão válida, false caso contrário
   */
  const validateSession = async (): Promise<boolean> => {
    const token = TokenManager.getToken(UserType.BARBEIRO);
    
    if (!token) {
      setIsLoading(false);
      return false;
    }
    
    try {
      const userData = await authService.validateToken();
      setUser(userData);
      setIsLoading(false);
      return true;
    } catch (error) {
      // Token inválido ou expirado - limpar completamente
      TokenManager.logout(UserType.BARBEIRO);
      setUser(null);
      setIsLoading(false);
      return false;
    }
  };
  
  /**
   * Realiza login do barbeiro
   * IMPORTANTE: Limpa automaticamente tokens de outros tipos de usuários
   * 
   * @param data - Dados de login (email e senha)
   * @throws Error quando credenciais são inválidas
   */
  const login = async (data: LoginInput) => {
    try {
      const response = await authService.login(data);
      
      // Armazenar token usando TokenManager (limpa tokens conflitantes automaticamente)
      TokenManager.setToken(UserType.BARBEIRO, response.token);
      
      // Criar objeto User com dados da resposta
      // Nota: Como não temos todos os dados do usuário na resposta de login,
      // vamos validar o token para buscar os dados completos
      const userData = await authService.validateToken();
      setUser(userData);
      
      // Após login, buscar as barbearias vinculadas ao barbeiro
      try {
        const myBarbershops = await barbershopService.getMyBarbershops();
        if (myBarbershops.length <= 1) {
          // Se apenas 1 barbearia, redireciona direto para agenda
          navigate('/barber/schedule');
        } else {
          // Se múltiplas barbearias, redireciona para página de seleção
          navigate('/barber/select-barbershop');
        }
      } catch (err) {
        // Se falhar ao buscar barbearias, fallback para agenda
        navigate('/barber/schedule');
      }
    } catch (error) {
      // Propagar erro para ser tratado no componente
      throw error;
    }
  };
  
  /**
   * Realiza logout do barbeiro
   */
  const logout = () => {
    authService.logout();
    setUser(null);
    navigate('/login');
  };
  
  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        validateSession
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

/**
 * Hook para acessar o contexto de autenticação
 * 
 * @returns AuthContextType - Contexto de autenticação
 * @throws Error se usado fora do AuthProvider
 * 
 * @example
 * ```tsx
 * function MyComponent() {
 *   const { user, isAuthenticated, login, logout } = useAuth();
 *   
 *   if (!isAuthenticated) {
 *     return <LoginForm onSubmit={login} />;
 *   }
 *   
 *   return <div>Welcome, {user.name}!</div>;
 * }
 * ```
 */
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);
  
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}
