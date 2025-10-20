import { RouteObject, Outlet } from 'react-router-dom';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { LoginPage } from '@/pages/auth/LoginPage';
import { BarberSchedulePage } from '@/pages/barber/SchedulePage';
import { SelectBarbershopPage } from '@/pages/barber/SelectBarbershopPage';
import { AuthProvider } from '@/contexts/AuthContext';

/**
 * Layout wrapper com AuthProvider para rotas do barbeiro
 */
function BarberAuthLayout() {
  return (
    <AuthProvider>
      <Outlet />
    </AuthProvider>
  );
}

/**
 * Rotas do sistema de barbeiro
 * 
 * Define rotas públicas e protegidas para o módulo de barbeiro.
 * Todas as rotas protegidas requerem autenticação via AuthContext.
 * 
 * Estrutura:
 * - /login (pública)
 * - /barber/* (protegidas)
 */
export const barberRoutes: RouteObject = {
  element: <BarberAuthLayout />,
  children: [
    {
      path: '/login',
      element: <LoginPage />,
    },
    {
      path: '/barber',
      element: <ProtectedRoute />,
      children: [
        {
          path: 'schedule',
          element: <BarberSchedulePage />,
        },
        {
          path: 'select-barbershop',
          element: <SelectBarbershopPage />,
        },
        // Futuras rotas do barbeiro:
        // { path: 'profile', element: <BarberProfilePage /> },
        // { path: 'appointments', element: <AppointmentsPage /> },
        // { path: 'clients', element: <ClientsPage /> },
      ],
    },
  ],
};
