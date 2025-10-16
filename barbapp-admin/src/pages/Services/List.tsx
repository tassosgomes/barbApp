import { useState, useMemo } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useServices, useServiceMutations } from '@/hooks';
import { DataTable } from '@/components/ui/data-table';
import { FiltersBar } from '@/components/ui/filters-bar';
import { StatusBadge } from '@/components/ui/status-badge';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { ServiceForm } from './ServiceForm';
import type { BarbershopService, CreateServiceRequest, UpdateServiceRequest } from '@/types';

export function ServicesListPage() {
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
  const { data, isLoading, error } = useServices(filters);
  const { createService, updateService, toggleServiceActive } = useServiceMutations();

  // State for confirm dialog
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [selectedService, setSelectedService] = useState<BarbershopService | null>(null);
  const [isToggling, setIsToggling] = useState(false);

  // State for form modal
  const [formModalOpen, setFormModalOpen] = useState(false);
  const [editingService, setEditingService] = useState<BarbershopService | null>(null);
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
  const handleToggleActive = (service: BarbershopService) => {
    setSelectedService(service);
    setConfirmDialogOpen(true);
  };

  const confirmToggleActive = async () => {
    if (!selectedService) return;

    setIsToggling(true);
    try {
      await toggleServiceActive.mutateAsync({
        id: selectedService.id,
        isActive: !selectedService.isActive,
      });
      toast({
        title: selectedService.isActive ? 'Serviço desativado!' : 'Serviço ativado!',
        description: `${selectedService.name} foi ${selectedService.isActive ? 'desativado' : 'ativado'} com sucesso.`,
      });
      setConfirmDialogOpen(false);
    } catch (error: unknown) {
      const axiosError = error as { response?: { data?: { message?: string } } };
      const message = axiosError?.response?.data?.message || 'Erro ao alterar status do serviço';
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
  const handleCreateService = () => {
    setEditingService(null);
    setFormModalOpen(true);
  };

  const handleEditService = (service: BarbershopService) => {
    setEditingService(service);
    setFormModalOpen(true);
  };

  const handleFormSubmit = async (data: CreateServiceRequest | UpdateServiceRequest) => {
    setIsSubmittingForm(true);
    try {
      if (editingService) {
        await updateService.mutateAsync({
          id: editingService.id,
          request: data as UpdateServiceRequest,
        });
        toast({
          title: 'Serviço atualizado!',
          description: `${editingService.name} foi atualizado com sucesso.`,
        });
      } else {
        await createService.mutateAsync(data as CreateServiceRequest);
        toast({
          title: 'Serviço criado!',
          description: 'Novo serviço foi criado com sucesso.',
        });
      }
      setFormModalOpen(false);
    } catch (error: unknown) {
      const axiosError = error as { response?: { status?: number; data?: { message?: string } } };
      const status = axiosError?.response?.status;
      const message = axiosError?.response?.data?.message || 'Erro ao salvar serviço';
      
      if (status === 409) {
        toast({
          title: 'Nome duplicado',
          description: 'Já existe um serviço com este nome.',
          variant: 'destructive',
        });
      } else if (status === 422) {
        toast({
          title: 'Dados inválidos',
          description: message,
          variant: 'destructive',
        });
      } else {
        toast({
          title: 'Erro',
          description: message,
          variant: 'destructive',
        });
      }
    } finally {
      setIsSubmittingForm(false);
    }
  };

  // Format currency for display
  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  // Table columns
  const columns = [
    {
      key: 'name',
      header: 'Nome',
      render: (service: BarbershopService) => service.name,
    },
    {
      key: 'description',
      header: 'Descrição',
      render: (service: BarbershopService) => service.description,
    },
    {
      key: 'durationMinutes',
      header: 'Duração',
      render: (service: BarbershopService) => `${service.durationMinutes} min`,
    },
    {
      key: 'price',
      header: 'Preço',
      render: (service: BarbershopService) => formatCurrency(service.price),
    },
    {
      key: 'status',
      header: 'Status',
      render: (service: BarbershopService) => (
        <StatusBadge isActive={service.isActive} />
      ),
    },
    {
      key: 'actions',
      header: 'Ações',
      render: (service: BarbershopService) => (
        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleEditService(service)}
          >
            Editar
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={() => handleToggleActive(service)}
          >
            {service.isActive ? 'Desativar' : 'Ativar'}
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
          <h2 className="text-xl font-semibold text-red-600">Erro ao carregar serviços</h2>
          <p className="text-gray-600 mt-2">Tente novamente mais tarde.</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Serviços</h1>
        <Button onClick={handleCreateService}>+ Novo Serviço</Button>
      </div>

      <FiltersBar
        fields={filterFields}
      />

      <DataTable
        data={data}
        columns={columns}
        isLoading={isLoading}
        onPageChange={handlePageChange}
        emptyMessage="Nenhum serviço encontrado"
      />

      <ConfirmDialog
        open={confirmDialogOpen}
        onClose={() => setConfirmDialogOpen(false)}
        onConfirm={confirmToggleActive}
        title={selectedService?.isActive ? 'Desativar Serviço' : 'Ativar Serviço'}
        description={`Tem certeza que deseja ${selectedService?.isActive ? 'desativar' : 'ativar'} o serviço ${selectedService?.name}?`}
        confirmText={selectedService?.isActive ? 'Desativar' : 'Ativar'}
        isLoading={isToggling}
      />

      <Dialog open={formModalOpen} onOpenChange={setFormModalOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingService ? 'Editar Serviço' : 'Novo Serviço'}
            </DialogTitle>
          </DialogHeader>
          <ServiceForm
            service={editingService || undefined}
            onSubmit={handleFormSubmit}
            onCancel={() => setFormModalOpen(false)}
            isLoading={isSubmittingForm}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
