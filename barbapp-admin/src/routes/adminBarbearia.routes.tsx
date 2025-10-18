import { RouteObject } from 'react-router-dom';
import { LoginAdminBarbearia } from '@/pages/LoginAdminBarbearia';
import { ProtectedBarbeariaRoute } from '@/components/ProtectedBarbeariaRoute';
import { AdminBarbeariaLayout } from '@/layouts/AdminBarbeariaLayout';
import { Dashboard } from '@/pages/Dashboard';
import { BarbeirosListPage, BarbeiroFormPage } from '@/pages/Barbeiros';
import { ServicosListPage, ServicoFormPage } from '@/pages/Servicos';
import { AgendaPage } from '@/pages/Agenda';

/**
 * Routes for Admin Barbearia
 * All routes are prefixed with /:codigo for tenant isolation
 *
 * Structure:
 * - /:codigo/login - Public login page
 * - /:codigo/* - Protected routes (require authentication)
 *   - dashboard - Main dashboard with metrics
 *   - barbeiros - Barbers management (placeholder)
 *   - servicos - Services management (placeholder)
 *   - agenda - Schedule view (placeholder)
 */
export const adminBarbeariaRoutes: RouteObject[] = [
  // Public login route
  {
    path: '/:codigo/login',
    element: <LoginAdminBarbearia />,
  },

  // Protected routes with layout
  {
    path: '/:codigo',
    element: (
      <ProtectedBarbeariaRoute>
        <AdminBarbeariaLayout />
      </ProtectedBarbeariaRoute>
    ),
    children: [
      {
        path: 'dashboard',
        element: <Dashboard />,
      },
      {
        path: 'barbeiros',
        element: <BarbeirosListPage />,
      },
      {
        path: 'barbeiros/novo',
        element: <BarbeiroFormPage />,
      },
      {
        path: 'barbeiros/:id',
        element: <BarbeiroFormPage />,
      },
      {
        path: 'servicos',
        element: <ServicosListPage />,
      },
      {
        path: 'servicos/novo',
        element: <ServicoFormPage />,
      },
      {
        path: 'servicos/:id',
        element: <ServicoFormPage />,
      },
      {
        path: 'agenda',
        element: <AgendaPage />,
      },
    ],
  },
];