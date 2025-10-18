import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { servicoService } from '@/services/servico.service';
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
import { BarbershopService } from '@/types';
import { Badge } from '@/components/ui/badge';
import { formatCurrency, formatDuration } from '@/utils/formatters';

/**
 * ServicosListPage - Página de listagem de serviços
 * 
 * Features:
 * - Listagem com DataTable e paginação
 * - Filtros por status (ativo/inativo)
 * - Busca por nome (debounced)
 * - Ações: Editar, Desativar
 * - Modal de confirmação para desativação
 * - Toast feedback para ações
 * - Loading states
 * - Formatação de preço (R$) e duração (horas/minutos)
 */
export function ServicosListPage() {
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

  // Query para buscar serviços
  const { data, isLoading } = useQuery({
    queryKey: ['servicos', barbearia?.barbeariaId, filters],
    queryFn: () => servicoService.list(filters),
    enabled: !!barbearia,
  });

  // Modal de confirmação de desativação
  const [deactivateModalOpen, setDeactivateModalOpen] = useState(false);
  const [selectedServico, setSelectedServico] = useState<{
    id: string;
    name: string;
  } | null>(null);

  // Mutation para desativar serviço
  const deactivateMutation = useMutation({
    mutationFn: servicoService.deactivate,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['servicos'] });
      toast({
        title: 'Serviço desativado com sucesso',
        description: `${selectedServico?.name} foi desativado.`,
      });
      setDeactivateModalOpen(false);
      setSelectedServico(null);
    },
    onError: () => {
      toast({
        title: 'Erro ao desativar serviço',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    },
  });

  // Handlers
  const handleCreateNew = () => {
    navigate(`/${barbearia?.codigo}/servicos/novo`);
  };

  const handleEdit = (id: string) => {
    navigate(`/${barbearia?.codigo}/servicos/${id}`);
  };

  const handleDeactivate = (servico: BarbershopService) => {
    setSelectedServico({ id: servico.id, name: servico.name });
    setDeactivateModalOpen(true);
  };

  const confirmDeactivate = () => {
    if (selectedServico) {
      deactivateMutation.mutate(selectedServico.id);
    }
  };

  // Colunas da tabela
  const columns = [
    {
      key: 'name',
      header: 'Nome',
      render: (servico: BarbershopService) => (
        <div className="font-medium">{servico.name}</div>
      ),
    },
    {
      key: 'description',
      header: 'Descrição',
      render: (servico: BarbershopService) => (
        <div className="text-sm text-gray-600 max-w-xs truncate">
          {servico.description || '-'}
        </div>
      ),
    },
    {
      key: 'durationMinutes',
      header: 'Duração',
      render: (servico: BarbershopService) => (
        <div className="text-sm">{formatDuration(servico.durationMinutes)}</div>
      ),
    },
    {
      key: 'price',
      header: 'Preço',
      render: (servico: BarbershopService) => (
        <div className="text-sm font-medium">{formatCurrency(servico.price)}</div>
      ),
    },
    {
      key: 'isActive',
      header: 'Status',
      render: (servico: BarbershopService) => (
        <Badge variant={servico.isActive ? 'default' : 'secondary'}>
          {servico.isActive ? 'Ativo' : 'Inativo'}
        </Badge>
      ),
    },
    {
      key: 'actions',
      header: 'Ações',
      className: 'text-right',
      render: (servico: BarbershopService) => (
        <div className="flex justify-end gap-2">
          <Button
            size="sm"
            variant="outline"
            onClick={() => handleEdit(servico.id)}
          >
            Editar
          </Button>
          {servico.isActive && (
            <Button
              size="sm"
              variant="destructive"
              onClick={() => handleDeactivate(servico)}
              disabled={deactivateMutation.isPending}
            >
              Desativar
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
          <h1 className="text-3xl font-bold">Serviços</h1>
          <p className="mt-2 text-gray-600">
            Gerencie os serviços oferecidos pela barbearia.
          </p>
        </div>
        <Button onClick={handleCreateNew}>+ Novo Serviço</Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <Input
          placeholder="Buscar por nome..."
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
        emptyMessage="Nenhum serviço encontrado. Clique em 'Novo Serviço' para cadastrar."
      />

      {/* Modal de confirmação de desativação */}
      <AlertDialog
        open={deactivateModalOpen}
        onOpenChange={setDeactivateModalOpen}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Desativar serviço</AlertDialogTitle>
            <AlertDialogDescription>
              Tem certeza que deseja desativar <strong>{selectedServico?.name}</strong>?
              <br />
              <br />
              O serviço não poderá ser usado em novos agendamentos, mas os agendamentos
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
    </div>
  );
}
