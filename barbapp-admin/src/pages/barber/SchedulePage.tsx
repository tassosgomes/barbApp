import { useAuth } from '@/contexts/AuthContext';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Calendar, LogOut, User } from 'lucide-react';

/**
 * Página de agenda do barbeiro (Placeholder)
 * 
 * Esta é uma página temporária que será substituída pela
 * implementação completa do sistema de agendamentos.
 * 
 * Por enquanto, exibe informações do barbeiro autenticado
 * e permite fazer logout.
 */
export function BarberSchedulePage() {
  const { user, logout } = useAuth();
  
  return (
    <div className="min-h-screen bg-gray-50 p-4" data-testid="barber-schedule-page">
      {/* Header */}
      <header className="max-w-4xl mx-auto mb-6">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <Calendar className="h-8 w-8 text-blue-600" />
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Agenda do Barbeiro</h1>
              <p className="text-sm text-gray-600">
                Bem-vindo, {user?.name}!
              </p>
            </div>
          </div>
          
          <Button 
            onClick={logout} 
            variant="outline"
            data-testid="logout-button"
          >
            <LogOut className="h-4 w-4 mr-2" />
            Sair
          </Button>
        </div>
      </header>
      
      {/* Content */}
      <div className="max-w-4xl mx-auto space-y-6">
        {/* User Info Card */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <User className="h-5 w-5" />
              Informações do Usuário
            </CardTitle>
            <CardDescription>
              Dados do barbeiro autenticado
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-2">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm font-medium text-gray-500">Nome</p>
                <p className="text-base font-medium text-gray-900">{user?.name}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">E-mail</p>
                <p className="text-base font-medium text-gray-900">{user?.email}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">Cargo</p>
                <p className="text-base font-medium text-gray-900">{user?.role}</p>
              </div>
              <div>
                <p className="text-sm font-medium text-gray-500">Barbearia</p>
                <p className="text-base font-medium text-gray-900">{user?.nomeBarbearia}</p>
              </div>
            </div>
          </CardContent>
        </Card>
        
        {/* Placeholder Card */}
        <Card>
          <CardHeader>
            <CardTitle>Sistema de Agendamentos</CardTitle>
            <CardDescription>
              Em desenvolvimento
            </CardDescription>
          </CardHeader>
          <CardContent>
            <div className="text-center py-12">
              <Calendar className="h-16 w-16 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600 mb-2">
                A funcionalidade de agendamentos está em desenvolvimento.
              </p>
              <p className="text-sm text-gray-500">
                Em breve você poderá visualizar e gerenciar seus agendamentos aqui.
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
