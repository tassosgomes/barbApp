/**
 * Exemplo de uso do AuthContext
 * 
 * Este arquivo demonstra como usar o AuthContext e useAuth
 * em diferentes cenários da aplicação.
 */

import { useAuth } from '@/contexts/AuthContext';

// ============================================================
// EXEMPLO 1: Uso básico em um componente
// ============================================================

export function LoginExample() {
  const { login, isLoading } = useAuth();

  const handleSubmit = async (data: { email: string; password: string }) => {
    try {
      await login(data);
      // Navegação automática para /barber/schedule
    } catch (error) {
      console.error('Erro no login:', error);
      // Exibir toast de erro
    }
  };

  return (
    <form onSubmit={(e) => {
      e.preventDefault();
      const formData = new FormData(e.currentTarget);
      handleSubmit({
        email: formData.get('email') as string,
        password: formData.get('password') as string
      });
    }}>
      <input name="email" type="email" required />
      <input name="password" type="password" required />
      <button type="submit" disabled={isLoading}>
        {isLoading ? 'Entrando...' : 'Entrar'}
      </button>
    </form>
  );
}

// ============================================================
// EXEMPLO 2: Exibir informações do usuário
// ============================================================

export function UserProfileExample() {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated || !user) {
    return <div>Você precisa fazer login</div>;
  }

  return (
    <div>
      <h1>Bem-vindo, {user.name}!</h1>
      <p>Email: {user.email}</p>
      <p>Barbearia: {user.nomeBarbearia}</p>
    </div>
  );
}

// ============================================================
// EXEMPLO 3: Botão de logout
// ============================================================

export function LogoutButtonExample() {
  const { logout } = useAuth();

  return (
    <button onClick={logout}>
      Sair
    </button>
  );
}

// ============================================================
// EXEMPLO 4: Rota protegida
// ============================================================

export function ProtectedRouteExample({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return <div>Carregando...</div>;
  }

  if (!isAuthenticated) {
    // Redirecionar para login ou mostrar mensagem
    return <div>Acesso negado. Faça login.</div>;
  }

  return <>{children}</>;
}

// ============================================================
// EXEMPLO 5: Validar sessão manualmente
// ============================================================

export function SessionValidationExample() {
  const { validateSession } = useAuth();

  const handleRefreshSession = async () => {
    const isValid = await validateSession();
    
    if (isValid) {
      console.log('Sessão válida');
    } else {
      console.log('Sessão inválida');
      // Redirecionar para login
    }
  };

  return (
    <button onClick={handleRefreshSession}>
      Validar Sessão
    </button>
  );
}

// ============================================================
// EXEMPLO 6: Uso no App.tsx
// ============================================================

/**
 * Configuração no App.tsx
 * 
 * ```tsx
 * import { BrowserRouter } from 'react-router-dom';
 * import { AuthProvider } from '@/contexts/AuthContext';
 * 
 * function App() {
 *   return (
 *     <BrowserRouter>
 *       <AuthProvider>
 *         <Routes>
 *           <Route path="/login" element={<LoginPage />} />
 *           <Route 
 *             path="/barber/*" 
 *             element={
 *               <ProtectedRoute>
 *                 <BarberLayout />
 *               </ProtectedRoute>
 *             } 
 *           />
 *         </Routes>
 *       </AuthProvider>
 *     </BrowserRouter>
 *   );
 * }
 * ```
 */
