/**
 * BarberHeader Component
 * 
 * Header para o painel do barbeiro com:
 * - Nome do barbeiro
 * - Nome da barbearia
 * - Botão de logout
 */

import { LogOut } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAuth } from '@/contexts/AuthContext';

export function BarberHeader() {
  const { user, logout } = useAuth();

  if (!user) return null;

  return (
    <header className="border-b bg-white shadow-sm sticky top-0 z-10">
      <div className="container mx-auto px-4 py-4 max-w-4xl">
        <div className="flex items-center justify-between">
          {/* Informações do Barbeiro */}
          <div className="flex-1">
            <h1 className="text-xl font-semibold text-gray-900">
              {user.name}
            </h1>
            <p className="text-sm text-gray-600">
              {user.nomeBarbearia}
            </p>
          </div>

          {/* Botão de Logout */}
          <Button
            variant="ghost"
            size="sm"
            onClick={logout}
            className="flex items-center gap-2 text-gray-700 hover:text-gray-900 hover:bg-gray-100"
          >
            <LogOut className="h-4 w-4" />
            <span className="hidden sm:inline">Sair</span>
          </Button>
        </div>
      </div>
    </header>
  );
}
