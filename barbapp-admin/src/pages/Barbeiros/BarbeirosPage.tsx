import { Users } from 'lucide-react';

/**
 * Placeholder page for Barbeiros (Barbers) management
 * Will be implemented in future tasks
 */
export function BarbeirosPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Gerenciar Barbeiros</h1>
        <p className="mt-2 text-gray-600">
          Gerencie sua equipe de barbeiros.
        </p>
      </div>

      <div className="flex min-h-[400px] items-center justify-center rounded-lg border-2 border-dashed bg-gray-50">
        <div className="text-center">
          <Users className="mx-auto h-12 w-12 text-gray-400" />
          <h3 className="mt-4 text-lg font-semibold text-gray-900">
            Gestão de Barbeiros
          </h3>
          <p className="mt-2 text-sm text-gray-600">
            Esta funcionalidade será implementada nas próximas tarefas.
          </p>
          <p className="mt-1 text-sm text-gray-600">
            Você poderá cadastrar, editar e gerenciar sua equipe de barbeiros.
          </p>
        </div>
      </div>
    </div>
  );
}
