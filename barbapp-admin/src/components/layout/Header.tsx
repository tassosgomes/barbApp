import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';
import { BarbershopSelector } from './BarbershopSelector';

export function Header() {
  const { logout } = useAuth();

  return (
    <header className="border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <h1 className="text-xl font-bold">BarbApp Admin</h1>
        <div className="flex items-center gap-4">
          {/** Mostrar seletor apenas para barbeiros */}
          {/** eslint-disable-next-line @typescript-eslint/no-non-null-assertion */}
          {/** @ts-ignore - role pode não existir para outros tipos */}
          {useAuth().user?.role === 'Barbeiro' && <BarbershopSelector />}
          <Button variant="outline" onClick={logout} type="button">
            Sair
          </Button>
        </div>
      </div>
    </header>
  );
}