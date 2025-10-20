import { createBrowserRouter, Navigate, Outlet } from 'react-router-dom';
import { Login } from '@/pages/Login/Login';
import { BarbershopList, BarbershopCreate, BarbershopEdit, BarbershopDetails } from '@/pages/Barbershops';
import { BarbersListPage } from '@/pages/Barbers';
import { ServicesListPage } from '@/pages/Services';
import { SchedulePage } from '@/pages/Schedule';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { adminBarbeariaRoutes } from './adminBarbearia.routes';
import { barberRoutes } from './barber.routes';
import { AuthProvider } from '@/contexts/AuthContext';

/**
 * Layout wrapper com AuthProvider para rotas do Admin Central
 */
function AdminCentralAuthLayout() {
  return (
    <AuthProvider>
      <Outlet />
    </AuthProvider>
  );
}

export const router = createBrowserRouter([
  // Barber routes (login and /barber/*)
  barberRoutes,
  
  // Admin Barbearia routes (must come after barber to avoid conflicts)
  ...adminBarbeariaRoutes,
  
  // Admin Central routes (wrapped with AuthProvider)
  {
    element: <AdminCentralAuthLayout />,
    children: [
      {
        path: '/admin/login',
        element: <Login />,
      },
      {
        path: '/',
        element: <ProtectedRoute />,
        children: [
          {
            index: true,
            element: <Navigate to="/barbearias" replace />,
          },
          {
            path: 'barbearias',
            children: [
              { index: true, element: <BarbershopList /> },
              { path: 'nova', element: <BarbershopCreate /> },
              { path: ':id', element: <BarbershopDetails /> },
              { path: ':id/editar', element: <BarbershopEdit /> },
            ],
          },
          {
            path: 'barbeiros',
            element: <BarbersListPage />,
          },
          {
            path: 'servicos',
            element: <ServicesListPage />,
          },
          {
            path: 'agenda',
            element: <SchedulePage />,
          },
        ],
      },
      {
        path: '*',
        element: <Navigate to="/barbearias" replace />,
      },
    ],
  },
]);