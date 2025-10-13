import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';

export function Header() {
  const { logout } = useAuth();

  return (
    <header className="border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <h1 className="text-xl font-bold">BarbApp Admin</h1>
        <Button variant="outline" onClick={logout} type="button">
          Sair
        </Button>
      </div>
    </header>
  );
}