import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Loader2 } from 'lucide-react';

/**
 * Componente de rota protegida para área do barbeiro
 * 
 * Protege rotas que requerem autenticação de barbeiro.
 * Exibe loading durante validação de sessão e redireciona
 * para login se usuário não estiver autenticado.
 * 
 * Features:
 * - Verificação de autenticação via useAuth
 * - Loading state aprimorado durante validação inicial
 * - Redirect para /login preservando location
 * - Renderização de rotas filhas com Outlet
 * - Animações suaves e feedback visual claro
 * 
 * @example
 * ```tsx
 * <Route element={<ProtectedRoute />}>
 *   <Route path="/barber/schedule" element={<SchedulePage />} />
 * </Route>
 * ```
 */
export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  const location = useLocation();
  
  // Exibe loading state aprimorado enquanto valida sessão
  if (isLoading) {
    return (
      <div 
        className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100"
        data-testid="protected-route-loading"
      >
        <div className="text-center space-y-4 fade-in">
          {/* Spinner com anel decorativo */}
          <div className="relative mx-auto w-16 h-16">
            <Loader2 
              className="h-16 w-16 animate-spin text-blue-600 mx-auto" 
              data-testid="loading-spinner"
              aria-label="Carregando"
            />
            <div className="absolute inset-0 h-16 w-16 rounded-full border-4 border-blue-200 opacity-25" aria-hidden="true" />
          </div>
          
          {/* Mensagens de loading */}
          <div className="space-y-1">
            <p className="text-gray-700 font-medium text-base">Verificando autenticação</p>
            <p className="text-gray-500 text-sm">Aguarde um momento...</p>
          </div>
        </div>
      </div>
    );
  }
  
  // Redireciona para login se não autenticado
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  
  // Renderiza rotas filhas se autenticado com fade in
  return (
    <div className="fade-in">
      <Outlet />
    </div>
  );
}
