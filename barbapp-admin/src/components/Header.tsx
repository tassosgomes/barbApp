import { useNavigate, useParams } from 'react-router-dom';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { adminBarbeariaAuthService } from '@/services/adminBarbeariaAuth.service';
import { Button } from '@/components/ui/button';
import { LogOut, Menu } from 'lucide-react';

interface HeaderProps {
  onMenuToggle?: () => void;
}

/**
 * Header component for Admin Barbearia layout
 * Displays barbershop name, code, and logout button
 */
export function Header({ onMenuToggle }: HeaderProps) {
  const { codigo } = useParams<{ codigo: string }>();
  const { barbearia, clearBarbearia } = useBarbearia();
  const navigate = useNavigate();

  const handleLogout = () => {
    adminBarbeariaAuthService.logout();
    clearBarbearia();
    navigate(`/${codigo}/login`);
  };

  return (
    <header className="sticky top-0 z-50 border-b bg-background">
      <div className="flex h-16 items-center justify-between px-4 md:px-6">
        <div className="flex items-center gap-4">
          {/* Mobile menu toggle */}
          {onMenuToggle && (
            <Button
              variant="ghost"
              size="icon"
              className="md:hidden"
              onClick={onMenuToggle}
              aria-label="Toggle menu"
            >
              <Menu className="h-5 w-5" />
            </Button>
          )}
          
          <div>
            <h1 className="text-lg font-bold md:text-xl">{barbearia?.nome}</h1>
            <p className="text-xs text-muted-foreground md:text-sm">
              Código: {barbearia?.codigo}
            </p>
          </div>
        </div>

        <Button variant="outline" size="sm" onClick={handleLogout}>
          <LogOut className="mr-2 h-4 w-4" />
          <span className="hidden sm:inline">Sair</span>
        </Button>
      </div>
    </header>
  );
}
