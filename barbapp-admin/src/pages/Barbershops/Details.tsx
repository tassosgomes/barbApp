import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';
import { Button } from '@/components/ui/button';
import { StatusBadge } from '@/components/ui/status-badge';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/hooks';
import { formatDate, applyDocumentMask } from '@/utils/formatters';
import { copyToClipboard } from '@/lib/utils';
import { Copy, ArrowLeft, Edit, Power, PowerOff } from 'lucide-react';

export function BarbershopDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [barbershop, setBarbershop] = useState<Barbershop | null>(null);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  useEffect(() => {
    async function loadBarbershop() {
      if (!id) return;
      try {
        setLoading(true);
        const data = await barbershopService.getById(id);
        setBarbershop(data);
      } catch {
        toast({
          title: 'Erro ao carregar barbearia',
          description: 'Não foi possível carregar os dados da barbearia.',
          variant: 'destructive',
        });
        navigate('/barbearias');
      } finally {
        setLoading(false);
      }
    }
    loadBarbershop();
  }, [id, toast, navigate]);

  const handleCopyCode = async () => {
    if (!barbershop) return;
    const success = await copyToClipboard(barbershop.code);
    if (success) {
      toast({
        title: 'Código copiado!',
        description: 'O código da barbearia foi copiado para a área de transferência.',
      });
    } else {
      toast({
        title: 'Erro ao copiar',
        description: 'Não foi possível copiar o código.',
        variant: 'destructive',
      });
    }
  };

  const handleToggleStatus = async () => {
    if (!barbershop) return;
    setActionLoading(true);
    try {
      if (barbershop.isActive) {
        await barbershopService.deactivate(barbershop.id);
        toast({ title: 'Barbearia desativada com sucesso!' });
      } else {
        await barbershopService.reactivate(barbershop.id);
        toast({ title: 'Barbearia reativada com sucesso!' });
      }
      // Reload data
      const updated = await barbershopService.getById(barbershop.id);
      setBarbershop(updated);
    } catch {
      toast({
        title: 'Erro ao alterar status',
        description: 'Não foi possível alterar o status da barbearia.',
        variant: 'destructive',
      });
    } finally {
      setActionLoading(false);
    }
  };

  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-64" />
          <div className="flex gap-2">
            <Skeleton className="h-10 w-20" />
            <Skeleton className="h-10 w-20" />
            <Skeleton className="h-10 w-20" />
          </div>
        </div>
        <div className="rounded-lg border p-6 space-y-6">
          <Skeleton className="h-6 w-48" />
          <div className="grid grid-cols-2 gap-4">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
          </div>
          <Skeleton className="h-6 w-32" />
          <div className="grid grid-cols-2 gap-4">
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
            <Skeleton className="h-4 w-32" />
          </div>
        </div>
      </div>
    );
  }

  if (!barbershop) {
    return (
      <div className="flex flex-col items-center justify-center py-12">
        <h2 className="text-xl font-semibold text-gray-900 mb-2">
          Barbearia não encontrada
        </h2>
        <p className="text-gray-600 mb-6">
          A barbearia solicitada não existe ou foi removida.
        </p>
        <Button onClick={() => navigate('/barbearias')}>
          <ArrowLeft className="w-4 h-4 mr-2" />
          Voltar à lista
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <h1 className="text-3xl font-bold">{barbershop.name}</h1>
          <StatusBadge isActive={barbershop.isActive} />
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate(-1)}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Voltar
          </Button>
          <Button onClick={() => navigate(`/barbearias/${id}/editar`)}>
            <Edit className="w-4 h-4 mr-2" />
            Editar
          </Button>
          <Button
            variant={barbershop.isActive ? 'destructive' : 'default'}
            onClick={handleToggleStatus}
            disabled={actionLoading}
          >
            {barbershop.isActive ? (
              <>
                <PowerOff className="w-4 h-4 mr-2" />
                Desativar
              </>
            ) : (
              <>
                <Power className="w-4 h-4 mr-2" />
                Reativar
              </>
            )}
          </Button>
        </div>
      </div>

      {/* Content */}
      <div className="grid gap-6">
        {/* Code Section */}
        <div className="rounded-lg border bg-gray-50 p-6">
          <div className="flex items-center justify-between">
            <div>
              <h2 className="text-lg font-semibold mb-2">Código da Barbearia</h2>
              <p className="text-2xl font-mono font-bold text-gray-900">
                {barbershop.code}
              </p>
            </div>
            <Button onClick={handleCopyCode} variant="outline">
              <Copy className="w-4 h-4 mr-2" />
              Copiar Código
            </Button>
          </div>
        </div>

        {/* General Information */}
        <div className="rounded-lg border p-6">
          <h2 className="text-xl font-semibold mb-4">Informações Gerais</h2>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">Nome</dt>
              <dd className="mt-1">{barbershop.name}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Documento</dt>
              <dd className="mt-1 font-mono">{applyDocumentMask(barbershop.document)}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Proprietário</dt>
              <dd className="mt-1">{barbershop.ownerName}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Email</dt>
              <dd className="mt-1">{barbershop.email}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Telefone</dt>
              <dd className="mt-1">{barbershop.phone}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Status</dt>
              <dd className="mt-1">
                <StatusBadge isActive={barbershop.isActive} />
              </dd>
            </div>
          </dl>
        </div>

        {/* Address */}
        <div className="rounded-lg border p-6">
          <h2 className="text-xl font-semibold mb-4">Endereço</h2>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">CEP</dt>
              <dd className="mt-1">{barbershop.address.zipCode}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Logradouro</dt>
              <dd className="mt-1">
                {barbershop.address.street}, {barbershop.address.number}
                {barbershop.address.complement && ` - ${barbershop.address.complement}`}
              </dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Bairro</dt>
              <dd className="mt-1">{barbershop.address.neighborhood}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Cidade</dt>
              <dd className="mt-1">{barbershop.address.city} - {barbershop.address.state}</dd>
            </div>
          </dl>
        </div>

        {/* Audit Information */}
        <div className="rounded-lg border p-6">
          <h2 className="text-xl font-semibold mb-4">Informações de Auditoria</h2>
          <dl className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">Criado em</dt>
              <dd className="mt-1">{formatDate(barbershop.createdAt)}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Atualizado em</dt>
              <dd className="mt-1">{formatDate(barbershop.updatedAt)}</dd>
            </div>
          </dl>
        </div>
      </div>
    </div>
  );
}