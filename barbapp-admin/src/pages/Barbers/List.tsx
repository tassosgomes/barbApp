import { useState, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useBarbers, useBarberMutations } from '@/hooks';
import { DataTable } from '@/components/ui/data-table';
import { FiltersBar } from '@/components/ui/filters-bar';
import { StatusBadge } from '@/components/ui/status-badge';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { BarberForm } from './BarberForm';
import type { Barber, CreateBarberRequest, UpdateBarberRequest } from '@/types';

export function BarbersListPage() {
  const { toast } = useToast();
  const [searchParams, setSearchParams] = useSearchParams();

  // Get filters from URL
  const searchName = searchParams.get('searchName') || '';
  const isActiveParam = searchParams.get('isActive');
  const isActive = isActiveParam === null ? undefined : isActiveParam === 'true';
  const page = parseInt(searchParams.get('page') || '1', 10);
  const pageSize = parseInt(searchParams.get('pageSize') || '20', 10);

  // Memoize filters
  const filters = useMemo(() => ({
    searchName: searchName || undefined,
    isActive,
    page,
    pageSize,
  }), [searchName, isActive, page, pageSize]);

  // Fetch data
  const { data, isLoading, error } = useBarbers(filters);
  const { createBarber, updateBarber, toggleBarberActive } = useBarberMutations();

  // State for confirm dialog
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [selectedBarber, setSelectedBarber] = useState<Barber | null>(null);
  const [isToggling, setIsToggling] = useState(false);

  // State for form modal
  const [formModalOpen, setFormModalOpen] = useState(false);
  const [editingBarber, setEditingBarber] = useState<Barber | null>(null);
  const [isSubmittingForm, setIsSubmittingForm] = useState(false);

  // Update URL params
  const updateFilters = (newFilters: Partial<typeof filters>) => {
    const params = new URLSearchParams(searchParams);
    if (newFilters.searchName !== undefined) {
      if (newFilters.searchName) {
        params.set('searchName', newFilters.searchName);
      } else {
        params.delete('searchName');
      }
    }
    if (newFilters.isActive !== undefined) {
      if (newFilters.isActive !== undefined) {
        params.set('isActive', newFilters.isActive.toString());
      } else {
        params.delete('isActive');
      }
    }
    if (newFilters.page !== undefined) {
      params.set('page', newFilters.page.toString());
    }
    if (newFilters.pageSize !== undefined) {
      params.set('pageSize', newFilters.pageSize.toString());
    }
    setSearchParams(params);
  };

  const handlePageChange = (newPage: number) => {
    updateFilters({ page: newPage });
  };

  // Handle toggle active
  const handleToggleActive = (barber: Barber) => {
    setSelectedBarber(barber);
    setConfirmDialogOpen(true);
  };

  const confirmToggleActive = async () => {
    if (!selectedBarber) return;

    setIsToggling(true);
    try {
      await toggleBarberActive.mutateAsync({
        id: selectedBarber.id,
        isActive: !selectedBarber.isActive,
      });
      toast({
        title: selectedBarber.isActive ? 'Barbeiro desativado!' : 'Barbeiro ativado!',
        description: `${selectedBarber.name} foi ${selectedBarber.isActive ? 'desativado' : 'ativado'} com sucesso.`,
      });
      setConfirmDialogOpen(false);
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      const message = axiosError?.response?.data?.message || 'Erro ao alterar status do barbeiro';
      toast({
        title: 'Erro',
        description: message,
        variant: 'destructive',
      });
    } finally {
      setIsToggling(false);
    }
  };

  // Handle create/edit
  const handleCreateBarber = () => {
    setEditingBarber(null);
    setFormModalOpen(true);
  };

  const handleEditBarber = (barber: Barber) => {
    setEditingBarber(barber);
    setFormModalOpen(true);
  };

  const handleFormSubmit = async (data: CreateBarberRequest | UpdateBarberRequest) => {
    setIsSubmittingForm(true);
    try {
      if (editingBarber) {
        await updateBarber.mutateAsync({
          id: editingBarber.id,
          request: data as UpdateBarberRequest,
        });
        toast({
          title: 'Barbeiro atualizado!',
          description: `${editingBarber.name} foi atualizado com sucesso.`,
        });
      } else {
        await createBarber.mutateAsync(data as CreateBarberRequest);
        toast({
          title: 'Barbeiro criado!',
          description: 'Novo barbeiro foi criado com sucesso.',
        });
      }
      setFormModalOpen(false);
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      const message = axiosError?.response?.data?.message || 'Erro ao salvar barbeiro';
      toast({
        title: 'Erro',
        description: message,
        variant: 'destructive',
      });
    } finally {
      setIsSubmittingForm(false);
    }
  };

  // Table columns
  const columns = [
    {
      key: 'name',
      header: 'Nome',
      render: (barber: Barber) => barber.name,
    },
    {
      key: 'email',
      header: 'Email',
      render: (barber: Barber) => barber.email,
    },
    {
      key: 'phone',
      header: 'Telefone',
      render: (barber: Barber) => barber.phoneFormatted,
    },
    {
      key: 'services',
      header: 'Serviços',
      render: (barber: Barber) => barber.services.map(s => s.name).join(', '),
    },
    {
      key: 'status',
      header: 'Status',
      render: (barber: Barber) => (
        <StatusBadge isActive={barber.isActive} />
      ),
    },
    {
      key: 'createdAt',
      header: 'Criado em',
      render: (barber: Barber) => new Date(barber.createdAt).toLocaleDateString('pt-BR'),
    },
    {
      key: 'actions',
      header: 'Ações',
      render: (barber: Barber) => (
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleEditBarber(barber)}
          >
            Editar
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleToggleActive(barber)}
          >
            {barber.isActive ? 'Desativar' : 'Ativar'}
          </Button>
        </div>
      ),
    },
  ];

  // FiltersBar fields
  const filterFields = [
    {
      key: 'searchName',
      label: 'Buscar',
      type: 'text' as const,
      placeholder: 'Buscar por nome...',
    },
    {
      key: 'isActive',
      label: 'Status',
      type: 'select' as const,
      placeholder: 'Todos',
      options: [
        { value: 'all', label: 'Todos' },
        { value: 'true', label: 'Ativos' },
        { value: 'false', label: 'Inativos' },
      ],
    },
  ];

  if (error) {
    return (
      <div className="p-6">
        <div className="text-center">
          <h2 className="text-xl font-semibold text-red-600">Erro ao carregar barbeiros</h2>
          <p className="text-gray-600 mt-2">Tente novamente mais tarde.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Barbeiros</h1>
        <Button onClick={handleCreateBarber}>+ Novo Barbeiro</Button>
      </div>

      <FiltersBar
        fields={filterFields}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        onPageChange={handlePageChange}
        emptyMessage="Nenhum barbeiro encontrado"
      />

      <ConfirmDialog
        open={confirmDialogOpen}
        onClose={() => setConfirmDialogOpen(false)}
        onConfirm={confirmToggleActive}
        title={selectedBarber?.isActive ? 'Desativar Barbeiro' : 'Ativar Barbeiro'}
        description={`Tem certeza que deseja ${selectedBarber?.isActive ? 'desativar' : 'ativar'} o barbeiro ${selectedBarber?.name}?`}
        confirmText={selectedBarber?.isActive ? 'Desativar' : 'Ativar'}
        isLoading={isToggling}
      />

      <Dialog open={formModalOpen} onOpenChange={setFormModalOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingBarber ? 'Editar Barbeiro' : 'Novo Barbeiro'}
            </DialogTitle>
          </DialogHeader>
          <BarberForm
            barber={editingBarber || undefined}
            onSubmit={handleFormSubmit}
            onCancel={() => setFormModalOpen(false)}
            isLoading={isSubmittingForm}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}