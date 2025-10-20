import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { useAuth as useBarbeiroAuth } from '@/contexts/AuthContext';
import { BarbershopSelector } from './BarbershopSelector';

export function Header() {
  const { logout } = useAuth();
  const barbeiroAuth = useBarbeiroAuth();
  const isBarbeiro = barbeiroAuth?.user?.role === 'Barbeiro';

  return (
    <header className="border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <h1 className="text-xl font-bold">BarbApp Admin</h1>
        <div className="flex items-center gap-4">
          {isBarbeiro && <BarbershopSelector />}
          <Button variant="outline" onClick={logout} type="button">
            Sair
          </Button>
        </div>
      </div>
    </header>
  );
}