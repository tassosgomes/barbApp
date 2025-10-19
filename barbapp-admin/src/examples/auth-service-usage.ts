/**
 * Exemplo de uso do Auth Service
 * 
 * Este arquivo demonstra como usar o authService em componentes React
 */

import { authService } from '@/services/auth.service';
import { LoginInput } from '@/types/auth.types';

// ============================================================================
// Exemplo 1: Login de Barbeiro
// ============================================================================

async function exemploLogin() {
  const loginData: LoginInput = {
    barbershopCode: 'BARB001',
    phone: '(11) 99999-9999'
  };

  try {
    const response = await authService.login(loginData);
    
    // Token já foi armazenado no localStorage pelo service
    console.log('Login bem-sucedido!');
    console.log('Token:', response.token);
    console.log('Usuário:', response.user);
    
    // Navegar para agenda
    // navigate('/barber/schedule');
    
  } catch (error: any) {
    // Tratamento de erros
    if (error.response?.status === 401) {
      console.error('Credenciais inválidas');
      // Mostrar toast: "Código ou telefone inválidos"
    } else if (error.response?.status === 400) {
      console.error('Dados inválidos:', error.response.data);
      // Mostrar erros de validação no formulário
    } else if (error.response?.status === 500) {
      console.error('Erro no servidor');
      // Mostrar toast: "Erro ao conectar. Tente novamente"
    } else {
      console.error('Erro desconhecido:', error);
      // Mostrar toast: "Erro ao conectar. Verifique sua internet"
    }
  }
}

// ============================================================================
// Exemplo 2: Validação de Token ao Carregar App
// ============================================================================

async function exemploValidarSessao() {
  // Verificar se existe token
  const token = localStorage.getItem('barbapp-barber-token');
  
  if (!token) {
    console.log('Sem token, redirecionar para login');
    // navigate('/login');
    return;
  }
  
  try {
    // Validar token com backend
    const user = await authService.validateToken();
    
    console.log('Sessão válida!');
    console.log('Usuário:', user);
    
    // Atualizar estado global (AuthContext)
    // setUser(user);
    
    // Continuar navegação normal
    // navigate(requestedRoute);
    
  } catch (error: any) {
    if (error.response?.status === 401) {
      console.log('Token expirado ou inválido');
      
      // Limpar token
      authService.logout();
      
      // Redirecionar para login
      // navigate('/login');
    } else {
      console.error('Erro ao validar sessão:', error);
      // Pode tentar novamente ou redirecionar para login
    }
  }
}

// ============================================================================
// Exemplo 3: Logout
// ============================================================================

function exemploLogout() {
  // Remover token do localStorage
  authService.logout();
  
  console.log('Logout realizado');
  
  // Limpar estado global
  // setUser(null);
  
  // Redirecionar para login
  // navigate('/login');
}

// ============================================================================
// Exemplo 4: Uso em Componente React com Hook Personalizado
// ============================================================================

/* 
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '@/services/auth.service';
import { LoginInput, User } from '@/types/auth.types';
import { toast } from 'sonner';

function useAuth() {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();

  const login = async (data: LoginInput) => {
    setIsLoading(true);
    
    try {
      const response = await authService.login(data);
      
      // Armazenar token
      localStorage.setItem('barbapp-barber-token', response.token);
      
      // Atualizar estado
      setUser(response.user);
      
      // Mostrar sucesso
      toast.success(`Bem-vindo, ${response.user.name}!`);
      
      // Redirecionar
      navigate('/barber/schedule');
      
    } catch (error: any) {
      if (error.response?.status === 401) {
        toast.error('Código ou telefone inválidos. Verifique e tente novamente.');
      } else {
        toast.error('Erro ao conectar. Tente novamente em instantes.');
      }
      
      throw error;
    } finally {
      setIsLoading(false);
    }
  };

  const logout = () => {
    authService.logout();
    setUser(null);
    navigate('/login');
  };

  const validateSession = async (): Promise<boolean> => {
    const token = localStorage.getItem('barbapp-barber-token');
    
    if (!token) {
      return false;
    }
    
    try {
      const userData = await authService.validateToken();
      setUser(userData);
      return true;
    } catch (error) {
      authService.logout();
      return false;
    }
  };

  return { user, isLoading, login, logout, validateSession };
}

export default useAuth;
*/

// ============================================================================
// Exemplo 5: Uso em LoginForm
// ============================================================================

/*
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, LoginFormData } from '@/schemas/login.schema';
import { authService } from '@/services/auth.service';

function LoginForm() {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting }
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema)
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      const response = await authService.login(data);
      
      // Armazenar token
      localStorage.setItem('barbapp-barber-token', response.token);
      
      // Navegar
      window.location.href = '/barber/schedule';
      
    } catch (error: any) {
      if (error.response?.status === 401) {
        toast.error('Credenciais inválidas');
      } else {
        toast.error('Erro ao conectar');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)}>
      <input {...register('barbershopCode')} />
      <input {...register('phone')} />
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  );
}
*/

// ============================================================================
// Notas Importantes
// ============================================================================

/*
1. FORMATO DE TELEFONE:
   - UI aceita: (11) 99999-9999
   - API espera: +5511999999999
   - Conversão automática pelo formatPhoneToAPI()

2. CÓDIGO DA BARBEARIA:
   - Sempre convertido para UPPERCASE
   - Exemplo: 'barb001' → 'BARB001'

3. TOKEN JWT:
   - Armazenado em: localStorage['barbapp-barber-token']
   - Expiração: 24 horas (backend)
   - Adicionado automaticamente em requisições por interceptor

4. INTERCEPTOR AUTOMÁTICO:
   - Detecta rota /barber/* automaticamente
   - Adiciona header: Authorization: Bearer {token}
   - Redireciona em 401 para /login

5. ERROS COMUNS:
   - 400: Dados inválidos (mostrar erros no form)
   - 401: Credenciais inválidas (mostrar mensagem específica)
   - 500: Erro servidor (mostrar mensagem genérica)
   - Network: Sem conexão (verificar internet)
*/
