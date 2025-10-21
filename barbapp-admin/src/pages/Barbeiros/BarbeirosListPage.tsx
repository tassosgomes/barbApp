import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { barbeiroService } from '@/services/barbeiro.service';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { DataTable } from '@/components/ui/data-table';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useToast } from '@/hooks/use-toast';
import { useDebounce } from '@/hooks';
import { Barber } from '@/types';
import { Badge } from '@/components/ui/badge';

/**
 * BarbeirosListPage - Página de listagem de barbeiros
 * 
 * Features:
 * - Listagem com DataTable e paginação
 * - Filtros por status (ativo/inativo)
 * - Busca por nome ou email (debounced)
 * - Ações: Editar, Desativar, Reativar
 * - Modal de confirmação para desativação
 * - Toast feedback para ações
 * - Loading states
 */
export function BarbeirosListPage() {
  const navigate = useNavigate();
  const { barbearia } = useBarbearia();
  const { toast } = useToast();
  const queryClient = useQueryClient();

  // Estado dos filtros
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // Debounce da busca para evitar requests excessivos
  const debouncedSearch = useDebounce(searchTerm, 300);

  // Memoizar filtros para evitar re-renders desnecessários
  const filters = useMemo(
    () => ({
      page: currentPage,
      pageSize,
      search: debouncedSearch,
      isActive: statusFilter === 'all' ? undefined : statusFilter === 'true',
    }),
    [debouncedSearch, statusFilter, currentPage]
  );

  // Query para buscar barbeiros
  const { data, isLoading } = useQuery({
    queryKey: ['barbeiros', barbearia?.barbeariaId, filters],
    queryFn: () => barbeiroService.list(filters),
    enabled: !!barbearia,
  });

  // Modal de confirmação de desativação
  const [deactivateModalOpen, setDeactivateModalOpen] = useState(false);
  const [selectedBarbeiro, setSelectedBarbeiro] = useState<{
    id: string;
    name: string;
    email?: string;
  } | null>(null);

  // Modal de confirmação de reset de senha
  const [resetPasswordModalOpen, setResetPasswordModalOpen] = useState(false);
  const [selectedBarberForReset, setSelectedBarberForReset] = useState<{
    id: string;
    name: string;
    email: string;
  } | null>(null);

  // Mutation para desativar barbeiro
  const deactivateMutation = useMutation({
    mutationFn: barbeiroService.deactivate,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barbeiros'] });
      toast({
        title: 'Barbeiro desativado com sucesso',
        description: `${selectedBarbeiro?.name} foi desativado.`,
      });
      setDeactivateModalOpen(false);
      setSelectedBarbeiro(null);
    },
    onError: () => {
      toast({
        title: 'Erro ao desativar barbeiro',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Mutation para reativar barbeiro
  const reactivateMutation = useMutation({
    mutationFn: barbeiroService.reactivate,
    onSuccess: (_, barberId) => {
      queryClient.invalidateQueries({ queryKey: ['barbeiros'] });
      const barbeiro = data?.items.find((b) => b.id === barberId);
      toast({
        title: 'Barbeiro reativado com sucesso',
        description: `${barbeiro?.name} foi reativado.`,
      });
    },
    onError: () => {
      toast({
        title: 'Erro ao reativar barbeiro',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Mutation para resetar senha do barbeiro
  const resetPasswordMutation = useMutation({
    mutationFn: barbeiroService.resetPassword,
    onSuccess: () => {
      toast({
        title: 'Senha redefinida com sucesso',
        description: `Uma nova senha foi enviada para o email de ${selectedBarberForReset?.name}.`,
      });
      setResetPasswordModalOpen(false);
      setSelectedBarberForReset(null);
    },
    onError: () => {
      toast({
        title: 'Erro ao redefinir senha',
        description: 'Não foi possível redefinir a senha. Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Handlers
  const handleCreateNew = () => {
    navigate(`/${barbearia?.codigo}/barbeiros/novo`);
  };

  const handleEdit = (id: string) => {
    navigate(`/${barbearia?.codigo}/barbeiros/${id}`);
  };

  const handleDeactivate = (barbeiro: Barber) => {
    setSelectedBarbeiro({ id: barbeiro.id, name: barbeiro.name });
    setDeactivateModalOpen(true);
  };

  const confirmDeactivate = () => {
    if (selectedBarbeiro) {
      deactivateMutation.mutate(selectedBarbeiro.id);
    }
  };

  const handleReactivate = (id: string) => {
    reactivateMutation.mutate(id);
  };

  const handleResetPassword = (barbeiro: Barber) => {
    setSelectedBarberForReset({
      id: barbeiro.id,
      name: barbeiro.name,
      email: barbeiro.email,
    });
    setResetPasswordModalOpen(true);
  };

  const confirmResetPassword = () => {
    if (selectedBarberForReset) {
      resetPasswordMutation.mutate(selectedBarberForReset.id);
    }
  };

  // Colunas da tabela
  const columns = [
    {
      key: 'name',
      header: 'Nome',
      render: (barbeiro: Barber) => (
        <div className="font-medium">{barbeiro.name}</div>
      ),
    },
    {
      key: 'email',
      header: 'Email',
      render: (barbeiro: Barber) => (
        <div className="text-sm text-gray-600">{barbeiro.email}</div>
      ),
    },
    {
      key: 'phoneFormatted',
      header: 'Telefone',
      render: (barbeiro: Barber) => (
        <div className="text-sm">{barbeiro.phoneFormatted}</div>
      ),
    },
    {
      key: 'services',
      header: 'Serviços',
      render: (barbeiro: Barber) => (
        <div className="flex flex-wrap gap-1">
          {barbeiro.services.length > 0 ? (
            barbeiro.services.slice(0, 2).map((service) => (
              <Badge key={service.id} variant="secondary" className="text-xs">
                {service.name}
              </Badge>
            ))
          ) : (
            <span className="text-sm text-gray-400">Nenhum serviço</span>
          )}
          {barbeiro.services.length > 2 && (
            <Badge variant="secondary" className="text-xs">
              +{barbeiro.services.length - 2}
            </Badge>
          )}
        </div>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (barbeiro: Barber) => (
        <Badge variant={barbeiro.isActive ? 'default' : 'secondary'}>
          {barbeiro.isActive ? 'Ativo' : 'Inativo'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'Ações',
      className: 'text-right',
      render: (barbeiro: Barber) => (
        <div className="flex justify-end gap-2">
          <Button
            size="sm"
            variant="outline"
            onClick={() => handleEdit(barbeiro.id)}
          >
            Editar
          </Button>
          {barbeiro.isActive && (
            <Button
              size="sm"
              variant="secondary"
              onClick={() => handleResetPassword(barbeiro)}
              disabled={resetPasswordMutation.isPending}
            >
              Redefinir Senha
            </Button>
          )}
          {barbeiro.isActive ? (
            <Button
              size="sm"
              variant="destructive"
              onClick={() => handleDeactivate(barbeiro)}
              disabled={deactivateMutation.isPending}
            >
              Desativar
            </Button>
          ) : (
            <Button
              size="sm"
              variant="default"
              onClick={() => handleReactivate(barbeiro.id)}
              disabled={reactivateMutation.isPending}
            >
              Reativar
            </Button>
          )}
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold">Barbeiros</h1>
          <p className="mt-2 text-gray-600">
            Gerencie sua equipe de barbeiros.
          </p>
        </div>
        <Button onClick={handleCreateNew}>+ Novo Barbeiro</Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <Input
          placeholder="Buscar por nome ou email..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-md"
        />
        <Select value={statusFilter} onValueChange={setStatusFilter}>
          <SelectTrigger className="w-48">
            <SelectValue placeholder="Filtrar por status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">Todos</SelectItem>
            <SelectItem value="true">Ativos</SelectItem>
            <SelectItem value="false">Inativos</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Tabela */}
      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        onPageChange={setCurrentPage}
        emptyMessage="Nenhum barbeiro encontrado. Clique em 'Novo Barbeiro' para cadastrar."
      />

      {/* Modal de confirmação de desativação */}
      <AlertDialog
        open={deactivateModalOpen}
        onOpenChange={setDeactivateModalOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Desativar barbeiro</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja desativar <strong>{selectedBarbeiro?.name}</strong>?
              <br />
              <br />
              O barbeiro não poderá receber novos agendamentos, mas os agendamentos
              existentes serão mantidos.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deactivateMutation.isPending}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmDeactivate}
              disabled={deactivateMutation.isPending}
              className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
            >
              {deactivateMutation.isPending ? 'Desativando...' : 'Desativar'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Modal de confirmação de reset de senha */}
      <AlertDialog
        open={resetPasswordModalOpen}
        onOpenChange={setResetPasswordModalOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Redefinir Senha</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja redefinir a senha de{' '}
              <strong>{selectedBarberForReset?.name}</strong>?
              <br />
              <br />
              Uma nova senha será gerada automaticamente e enviada para:{' '}
              <strong>{selectedBarberForReset?.email}</strong>
              <br />
              <br />
              ⚠️ O barbeiro precisará usar a nova senha no próximo login.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={resetPasswordMutation.isPending}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={confirmResetPassword}
              disabled={resetPasswordMutation.isPending}
            >
              {resetPasswordMutation.isPending ? 'Redefinindo...' : 'Confirmar'}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
