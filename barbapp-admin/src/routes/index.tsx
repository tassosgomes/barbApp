import { createBrowserRouter, Navigate } from 'react-router-dom';
import { Login } from '@/pages/Login/Login';
import { BarbershopList, BarbershopCreate, BarbershopEdit, BarbershopDetails } from '@/pages/Barbershops';
import { BarbersListPage } from '@/pages/Barbers';
import { ServicesListPage } from '@/pages/Services';
import { SchedulePage } from '@/pages/Schedule';
import { ProtectedRoute } from '@/components/ProtectedRoute';
import { adminBarbeariaRoutes } from './adminBarbearia.routes';
import { barberRoutes } from './barber.routes';

export const router = createBrowserRouter([
  // Barber routes (login and /barber/*)
  barberRoutes,
  
  // Admin Barbearia routes (must come after barber to avoid conflicts)
  ...adminBarbeariaRoutes,
  
  // Admin Central routes
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
]);