import { RouteObject } from 'react-router-dom';
import { LoginAdminBarbearia } from '@/pages/LoginAdminBarbearia';
import { ProtectedBarbeariaRoute } from '@/components/ProtectedBarbeariaRoute';
import { AdminBarbeariaLayout } from '@/layouts/AdminBarbeariaLayout';
import { Dashboard } from '@/pages/Dashboard';
import { BarbeirosPage } from '@/pages/Barbeiros';
import { ServicosPage } from '@/pages/Servicos';
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
        element: <BarbeirosPage />,
      },
      {
        path: 'servicos',
        element: <ServicosPage />,
      },
      {
        path: 'agenda',
        element: <AgendaPage />,
      },
    ],
  },
];