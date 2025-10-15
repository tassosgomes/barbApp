import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBarbershops, useDebounce } from '@/hooks';
import { BarbershopTable } from '@/components/barbershop/BarbershopTable';
import { BarbershopTableSkeleton } from '@/components/barbershop/BarbershopTableSkeleton';
import { EmptyState } from '@/components/barbershop/EmptyState';
import { DeactivateModal } from '@/components/barbershop/DeactivateModal';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Pagination } from '@/components/ui/pagination';
import { useToast } from '@/hooks/use-toast';
import { barbershopService } from '@/services/barbershop.service';

export function BarbershopList() {
  const navigate = useNavigate();
  const { toast } = useToast();

  // Estado dos filtros
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // Debounce da busca
  const debouncedSearch = useDebounce(searchTerm, 300);

  // Memoizar filtros para evitar re-renders desnecessários
  const filters = useMemo(() => ({
    searchTerm: debouncedSearch,
    isActive: statusFilter === 'all' ? undefined : statusFilter === 'true',
    pageNumber: currentPage,
    pageSize,
  }), [debouncedSearch, statusFilter, currentPage]);

  // Buscar dados
  const { data, loading, error, refetch } = useBarbershops(filters);

  // Modal de confirmação
  const [deactivateModalOpen, setDeactivateModalOpen] = useState(false);
  const [selectedBarbershop, setSelectedBarbershop] = useState<{
    id: string;
    name: string;
    code: string;
  } | null>(null);
  const [isDeactivating, setIsDeactivating] = useState(false);

  const handleDeactivate = (id: string) => {
    const barbershop = data?.items.find(b => b.id === id);
    if (barbershop) {
      setSelectedBarbershop({
        id: barbershop.id,
        name: barbershop.name,
        code: barbershop.code,
      });
      setDeactivateModalOpen(true);
    }
  };

  const confirmDeactivate = async () => {
    if (!selectedBarbershop) return;

    setIsDeactivating(true);
    try {
      await barbershopService.deactivate(selectedBarbershop.id);
      toast({
        title: 'Barbearia desativada com sucesso!',
        description: `${selectedBarbershop.name} foi desativada.`,
      });
      setDeactivateModalOpen(false);
      // Refetch data after successful deactivation
      refetch();
    } catch {
      toast({
        title: 'Erro ao desativar barbearia',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    } finally {
      setIsDeactivating(false);
    }
  };

  const handleReactivate = async (id: string) => {
    try {
      await barbershopService.reactivate(id);
      const barbershop = data?.items.find(b => b.id === id);
      toast({
        title: 'Barbearia reativada com sucesso!',
        description: `${barbershop?.name} foi reativada.`,
      });
      // Refetch data after successful reactivation
      refetch();
    } catch {
      toast({
        title: 'Erro ao reativar barbearia',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    }
  };

  const handleCopyCode = (code: string) => {
    navigator.clipboard.writeText(code);
    toast({
      title: 'Código copiado!',
      description: `O código ${code} foi copiado para a área de transferência.`,
    });
  };

  const handleCreateNew = () => {
    navigate('/barbearias/nova');
  };

  if (loading) return <BarbershopTableSkeleton />;
  if (error) return (
    <EmptyState
      title="Erro ao carregar dados"
      description="Não foi possível carregar a lista de barbearias. Tente novamente mais tarde."
      actionLabel="Tentar Novamente"
      onAction={() => window.location.reload()}
    />
  );
  if (!data || data.items.length === 0) return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Gestão de Barbearias</h1>
        <Button onClick={handleCreateNew}>
          + Nova Barbearia
        </Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <Input
          placeholder="Buscar por nome, email ou cidade..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-md"
        />
        <Select
          value={statusFilter}
          onValueChange={setStatusFilter}
        >
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

      <EmptyState
        title={searchTerm ? "Nenhuma barbearia encontrada" : "Nenhuma barbearia encontrada"}
        description={searchTerm ? `Nenhum resultado para "${searchTerm}".` : "Comece cadastrando a primeira barbearia do sistema."}
        actionLabel={searchTerm ? "Limpar Busca" : "+ Nova Barbearia"}
        onAction={searchTerm ? () => {
          setSearchTerm('');
          setStatusFilter('all');
          setCurrentPage(1);
        } : handleCreateNew}
      />
    </div>
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Gestão de Barbearias</h1>
        <Button onClick={handleCreateNew}>
          + Nova Barbearia
        </Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <Input
          placeholder="Buscar por nome, email ou cidade..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-md"
        />
        <Select
          value={statusFilter}
          onValueChange={setStatusFilter}
        >
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
      <BarbershopTable
        barbershops={data.items}
        onView={(id) => navigate(`/barbearias/${id}`)}
        onEdit={(id) => navigate(`/barbearias/${id}/editar`)}
        onDeactivate={handleDeactivate}
        onReactivate={handleReactivate}
        onCopyCode={handleCopyCode}
      />

      {/* Paginação */}
      <Pagination
        currentPage={data.pageNumber}
        totalPages={data.totalPages}
        onPageChange={setCurrentPage}
        hasPreviousPage={data.hasPreviousPage}
        hasNextPage={data.hasNextPage}
      />

      {/* Modal de Confirmação */}
      <DeactivateModal
        open={deactivateModalOpen}
        onClose={() => setDeactivateModalOpen(false)}
        onConfirm={confirmDeactivate}
        barbershopName={selectedBarbershop?.name}
        barbershopCode={selectedBarbershop?.code}
        isLoading={isDeactivating}
      />
    </div>
  );
}