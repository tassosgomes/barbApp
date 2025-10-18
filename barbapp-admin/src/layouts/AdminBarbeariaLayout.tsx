import { Outlet, Link, useNavigate, useParams } from 'react-router-dom';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { adminBarbeariaAuthService } from '@/services/adminBarbeariaAuth.service';
import { Button } from '@/components/ui/button';
import { LogOut, Scissors, Users, Calendar, LayoutDashboard } from 'lucide-react';

/**
 * Layout component for Admin Barbearia pages
 * Provides header, sidebar navigation, and content area
 */
export function AdminBarbeariaLayout() {
  const { codigo } = useParams<{ codigo: string }>();
  const { barbearia, clearBarbearia } = useBarbearia();
  const navigate = useNavigate();

  const handleLogout = () => {
    adminBarbeariaAuthService.logout();
    clearBarbearia();
    navigate(`/${codigo}/login`);
  };

  // Navigation items with icons
  const navItems = [
    {
      path: `/${codigo}/dashboard`,
      label: 'Dashboard',
      icon: LayoutDashboard,
    },
    {
      path: `/${codigo}/barbeiros`,
      label: 'Barbeiros',
      icon: Users,
    },
    {
      path: `/${codigo}/servicos`,
      label: 'Serviços',
      icon: Scissors,
    },
    {
      path: `/${codigo}/agenda`,
      label: 'Agenda',
      icon: Calendar,
    },
  ];

  return (
    <div className="flex min-h-screen flex-col">
      {/* Header */}
      <header className="sticky top-0 z-50 border-b bg-background">
        <div className="flex h-16 items-center justify-between px-6">
          <div>
            <h1 className="text-xl font-bold">{barbearia?.nome}</h1>
            <p className="text-sm text-muted-foreground">Código: {barbearia?.codigo}</p>
          </div>
          <Button variant="outline" size="sm" onClick={handleLogout}>
            <LogOut className="mr-2 h-4 w-4" />
            Sair
          </Button>
        </div>
      </header>

      <div className="flex flex-1">
        {/* Sidebar */}
        <aside className="w-64 border-r bg-muted/10">
          <nav className="space-y-1 p-4">
            {navItems.map((item) => {
              const Icon = item.icon;
              return (
                <Link
                  key={item.path}
                  to={item.path}
                  className="flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors hover:bg-muted"
                >
                  <Icon className="h-5 w-5" />
                  {item.label}
                </Link>
              );
            })}
          </nav>
        </aside>

        {/* Main content */}
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
