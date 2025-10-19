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
 * - Loading state durante validação inicial
 * - Redirect para /login preservando location
 * - Renderização de rotas filhas com Outlet
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
  
  // Exibe loading enquanto valida sessão
  if (isLoading) {
    return (
      <div 
        className="min-h-screen flex items-center justify-center bg-gray-50"
        data-testid="protected-route-loading"
      >
        <div className="text-center">
          <Loader2 
            className="h-8 w-8 animate-spin text-gray-600 mx-auto mb-4" 
            data-testid="loading-spinner"
          />
          <p className="text-gray-600 text-sm">Verificando autenticação...</p>
        </div>
      </div>
    );
  }
  
  // Redireciona para login se não autenticado
  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }
  
  // Renderiza rotas filhas se autenticado
  return <Outlet />;
}
