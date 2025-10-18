import { NavLink, useParams } from 'react-router-dom';
import { cn } from '@/lib/utils';
import { LayoutDashboard, Users, Scissors, Calendar, X } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface SidebarProps {
  isOpen?: boolean;
  onClose?: () => void;
}

/**
 * Sidebar navigation component for Admin Barbearia layout
 * Provides navigation links to main sections
 */
export function Sidebar({ isOpen = true, onClose }: SidebarProps) {
  const { codigo } = useParams<{ codigo: string }>();

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
    <>
      {/* Mobile overlay */}
      {isOpen && onClose && (
        <div
          className="fixed inset-0 z-40 bg-black/50 md:hidden"
          onClick={onClose}
          aria-hidden="true"
        />
      )}

      {/* Sidebar */}
      <aside
        className={cn(
          'fixed left-0 top-0 z-50 h-full w-64 border-r bg-background transition-transform duration-200 md:sticky md:top-16 md:h-[calc(100vh-4rem)] md:translate-x-0',
          isOpen ? 'translate-x-0' : '-translate-x-full'
        )}
      >
        {/* Mobile close button */}
        {onClose && (
          <div className="flex items-center justify-between border-b p-4 md:hidden">
            <h2 className="text-lg font-semibold">Menu</h2>
            <Button variant="ghost" size="icon" onClick={onClose}>
              <X className="h-5 w-5" />
            </Button>
          </div>
        )}

        {/* Navigation */}
        <nav className="space-y-1 p-4">
          {navItems.map((item) => {
            const Icon = item.icon;
            return (
              <NavLink
                key={item.path}
                to={item.path}
                onClick={onClose}
                className={({ isActive }) =>
                  cn(
                    'flex items-center gap-3 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                    isActive
                      ? 'bg-primary text-primary-foreground'
                      : 'hover:bg-muted'
                  )
                }
              >
                <Icon className="h-5 w-5" />
                {item.label}
              </NavLink>
            );
          })}
        </nav>
      </aside>
    </>
  );
}
