import { createBrowserRouter, Navigate } from 'react-router-dom';
import { Login } from '@/pages/Login/Login';
import { BarbershopList, BarbershopCreate, BarbershopEdit, BarbershopDetails } from '@/pages/Barbershops';
import { BarbersListPage } from '@/pages/Barbers';
import { ProtectedRoute } from '@/components/ProtectedRoute';

export const router = createBrowserRouter([
  {
    path: '/login',
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
    ],
  },
  {
    path: '*',
    element: <Navigate to="/barbearias" replace />,
  },
]);