import { RouteObject } from 'react-router-dom';
import { LoginAdminBarbearia } from '@/pages/LoginAdminBarbearia';

/**
 * Temporary Dashboard component for testing
 */
function TempDashboard() {
  return (
    <div className="flex min-h-screen items-center justify-center">
      <div className="text-center">
        <h1 className="text-4xl font-bold">🎉 Dashboard Admin Barbearia</h1>
        <p className="mt-4 text-xl">Login realizado com sucesso!</p>
        <p className="mt-2 text-gray-600">Esta é uma página temporária para testes.</p>
      </div>
    </div>
  );
}

/**
 * Routes for Admin Barbearia
 * All routes are prefixed with /:codigo
 */
export const adminBarbeariaRoutes: RouteObject[] = [
  {
    path: '/:codigo/login',
    element: <LoginAdminBarbearia />,
  },
  {
    path: '/:codigo/dashboard',
    element: <TempDashboard />,
  },
  // TODO: Add protected routes for dashboard, barbers, services, schedule
  // These will be added in task 7.0
];