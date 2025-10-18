import { Scissors } from 'lucide-react';

/**
 * Placeholder page for Servicos (Services) management
 * Will be implemented in future tasks
 */
export function ServicosPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Gerenciar Serviços</h1>
        <p className="mt-2 text-gray-600">
          Gerencie os serviços oferecidos pela sua barbearia.
        </p>
      </div>

      <div className="flex min-h-[400px] items-center justify-center rounded-lg border-2 border-dashed bg-gray-50">
        <div className="text-center">
          <Scissors className="mx-auto h-12 w-12 text-gray-400" />
          <h3 className="mt-4 text-lg font-semibold text-gray-900">
            Gestão de Serviços
          </h3>
          <p className="mt-2 text-sm text-gray-600">
            Esta funcionalidade será implementada nas próximas tarefas.
          </p>
          <p className="mt-1 text-sm text-gray-600">
            Você poderá cadastrar serviços, definir preços e durações.
          </p>
        </div>
      </div>
    </div>
  );
}
