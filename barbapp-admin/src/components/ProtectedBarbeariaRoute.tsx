import { Navigate, useParams } from 'react-router-dom';
import { adminBarbeariaAuthService } from '@/services/adminBarbeariaAuth.service';
import { useBarbearia } from '@/contexts/BarbeariaContext';

interface ProtectedBarbeariaRouteProps {
  children: React.ReactNode;
}

/**
 * Protected route component for Admin Barbearia
 * Validates authentication and user type before rendering protected routes
 * 
 * Security checks:
 * 1. User must be authenticated (valid token)
 * 2. User type must be AdminBarbearia
 * 3. URL codigo must match barbearia context codigo
 * 
 * @example
 * ```tsx
 * <Route
 *   path="/:codigo/dashboard"
 *   element={
 *     <ProtectedBarbeariaRoute>
 *       <Dashboard />
 *     </ProtectedBarbeariaRoute>
 *   }
 * />
 * ```
 */
export function ProtectedBarbeariaRoute({ children }: ProtectedBarbeariaRouteProps) {
  const { codigo } = useParams<{ codigo: string }>();
  const { barbearia, isLoaded } = useBarbearia();
  const isAuthenticated = adminBarbeariaAuthService.isAuthenticated();

  // Wait for context to load from localStorage
  if (!isLoaded) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <div className="text-center">
          <div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent"></div>
          <p className="mt-4 text-sm text-muted-foreground">Carregando...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated or no barbearia context
  if (!isAuthenticated || !barbearia) {
    return <Navigate to={`/${codigo}/login`} replace />;
  }

  // Validate that URL codigo matches context codigo
  // This prevents users from accessing other barbershops by changing the URL
  if (barbearia.codigo !== codigo) {
    console.warn(
      `[ProtectedBarbeariaRoute] URL codigo mismatch: URL=${codigo}, Context=${barbearia.codigo}`
    );
    return <Navigate to={`/${barbearia.codigo}/dashboard`} replace />;
  }

  // All checks passed, render protected content
  return <>{children}</>;
}
