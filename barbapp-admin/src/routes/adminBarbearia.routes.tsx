import { RouteObject } from 'react-router-dom';
import { LoginAdminBarbearia } from '@/pages/LoginAdminBarbearia';

/**
 * Routes for Admin Barbearia
 * All routes are prefixed with /:codigo
 */
export const adminBarbeariaRoutes: RouteObject[] = [
  {
    path: '/:codigo/login',
    element: <LoginAdminBarbearia />,
  },
  // TODO: Add protected routes for dashboard, barbers, services, schedule
  // These will be added in task 7.0
];