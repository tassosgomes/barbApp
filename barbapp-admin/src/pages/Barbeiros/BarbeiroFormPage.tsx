import { useEffect, useId } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { barbeiroService } from '@/services/barbeiro.service';
import {
  createBarbeiroSchema,
  updateBarbeiroSchema,
  type CreateBarbeiroFormData,
  type UpdateBarbeiroFormData,
} from '@/schemas/barbeiro.schema';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { Skeleton } from '@/components/ui/skeleton';
import { Checkbox } from '@/components/ui/checkbox';
import { ArrowLeft } from 'lucide-react';
import { useServices } from '@/hooks';

/**
 * BarbeiroFormPage - Página de criação/edição de barbeiro
 * 
 * Features:
 * - Formulário com validação Zod
 * - Suporte para criação e edição
 * - Carrega dados existentes no modo edição
 * - Seleção múltipla de serviços com checkboxes
 * - Máscara de telefone
 * - Toast feedback
 * - Loading states
 */
export function BarbeiroFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const { barbearia } = useBarbearia();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const isEditing = !!id;

  // Buscar barbeiro existente (modo edição)
  const { data: barbeiro, isLoading: isLoadingBarbeiro } = useQuery({
    queryKey: ['barbeiro', id],
    queryFn: () => barbeiroService.getById(id!),
    enabled: isEditing,
  });

  // Buscar serviços disponíveis
  const { data: servicesData, isLoading: isLoadingServices } = useServices({
    page: 1,
    pageSize: 100,
    isActive: true,
  });

  // Setup do formulário
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<CreateBarbeiroFormData | UpdateBarbeiroFormData>({
    resolver: zodResolver(isEditing ? updateBarbeiroSchema : createBarbeiroSchema),
    defaultValues: {
      serviceIds: [],
    },
  });

  // Watch para serviceIds
  const selectedServiceIds = watch('serviceIds') || [];
  const servicesHelperId = useId();
  const servicesErrorId = errors.serviceIds ? `${servicesHelperId}-error` : undefined;
  const servicesDescribedBy = [servicesHelperId, servicesErrorId].filter(Boolean).join(' ');

  // Carregar dados do barbeiro no formulário (modo edição)
  useEffect(() => {
    if (barbeiro && isEditing) {
      reset({
        nome: barbeiro.name,
        telefone: barbeiro.phoneFormatted,
        serviceIds: barbeiro.services.map(s => s.id),
      });
    }
  }, [barbeiro, isEditing, reset]);

  // Mutation para criar/atualizar barbeiro
  const mutation = useMutation({
    mutationFn: (data: CreateBarbeiroFormData | UpdateBarbeiroFormData) => {
      if (isEditing) {
        // Transform para formato da API
        const updateData = data as UpdateBarbeiroFormData;
        return barbeiroService.update(id!, {
          name: updateData.nome!,
          phone: updateData.telefone!.replace(/\D/g, ''),
          serviceIds: updateData.serviceIds!,
        });
      }
      // Transform para formato da API
      const createData = data as CreateBarbeiroFormData;
      return barbeiroService.create({
        name: createData.nome,
        email: createData.email,
        password: createData.senha,
        phone: createData.telefone.replace(/\D/g, ''),
        serviceIds: createData.serviceIds,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['barbeiros'] });
      toast({
        title: `Barbeiro ${isEditing ? 'atualizado' : 'criado'} com sucesso`,
        description: `O barbeiro foi ${isEditing ? 'atualizado' : 'cadastrado'} com sucesso.`,
      });
      navigate(`/${barbearia?.codigo}/barbeiros`);
    },
    onError: (error: any) => {
      const errorMessage = error?.response?.data?.message || 'Tente novamente mais tarde.';
      toast({
        title: `Erro ao ${isEditing ? 'atualizar' : 'criar'} barbeiro`,
        description: errorMessage,
        variant: 'destructive',
      });
    },
  });

  // Handlers
  const onSubmit = (data: CreateBarbeiroFormData | UpdateBarbeiroFormData) => {
    mutation.mutate(data);
  };

  const handleCancel = () => {
    navigate(`/${barbearia?.codigo}/barbeiros`);
  };

  const handleServiceToggle = (serviceId: string, checked: boolean) => {
    const currentIds = selectedServiceIds || [];
    const nextIds = checked
      ? Array.from(new Set([...currentIds, serviceId]))
      : currentIds.filter(id => id !== serviceId);

    setValue('serviceIds', nextIds, {
      shouldValidate: true,
      shouldDirty: true,
    });
  };

  // Formatação de telefone
  const formatPhone = (value: string) => {
    const cleaned = value.replace(/\D/g, '');
    if (cleaned.length <= 10) {
      return cleaned.replace(/(\d{2})(\d{4})(\d{4})/, '($1) $2-$3');
    }
    return cleaned.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3');
  };

  const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const formatted = formatPhone(e.target.value);
    setValue('telefone', formatted, {
      shouldValidate: true,
      shouldDirty: true,
    });
  };

  // Loading state
  if (isLoadingBarbeiro || isLoadingServices) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Skeleton className="h-10 w-10 rounded-md" />
          <div>
            <Skeleton className="h-8 w-64" />
            <Skeleton className="mt-2 h-4 w-96" />
          </div>
        </div>
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-24 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center gap-4">
        <Button
          variant="outline"
          size="icon"
          onClick={handleCancel}
          aria-label="Voltar para lista de barbeiros"
        >
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold">
            {isEditing ? 'Editar Barbeiro' : 'Novo Barbeiro'}
          </h1>
          <p className="mt-2 text-gray-600">
            {isEditing
              ? 'Atualize as informações do barbeiro.'
              : 'Cadastre um novo barbeiro na sua equipe.'}
          </p>
        </div>
      </div>

      {/* Formulário */}
      <Card>
        <CardHeader>
          <CardTitle>Informações do Barbeiro</CardTitle>
          <CardDescription>
            Preencha os dados do barbeiro e selecione os serviços que ele pode realizar.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Nome */}
            <div className="space-y-2">
              <Label htmlFor="nome">Nome *</Label>
              <Input
                id="nome"
                {...register('nome')}
                placeholder="Ex: João Silva"
                disabled={mutation.isPending}
              />
              {errors.nome && (
                <p className="text-sm text-red-600">{errors.nome.message}</p>
              )}
            </div>

            {/* Email - apenas criação */}
            {!isEditing && (
              <div className="space-y-2">
                <Label htmlFor="email">Email *</Label>
                <Input
                  id="email"
                  type="email"
                  {...register('email')}
                  placeholder="joao@example.com"
                  disabled={mutation.isPending}
                />
                {'email' in errors && errors.email && (
                  <p className="text-sm text-red-600">{errors.email.message}</p>
                )}
              </div>
            )}

            {/* Telefone */}
            <div className="space-y-2">
              <Label htmlFor="telefone">Telefone *</Label>
              <Input
                id="telefone"
                {...register('telefone')}
                onChange={handlePhoneChange}
                placeholder="(11) 98765-4321"
                disabled={mutation.isPending}
                maxLength={15}
              />
              {errors.telefone && (
                <p className="text-sm text-red-600">{errors.telefone.message}</p>
              )}
            </div>

            {/* Senha - apenas criação */}
            {!isEditing && (
              <div className="space-y-2">
                <Label htmlFor="senha">Senha *</Label>
                <Input
                  id="senha"
                  type="password"
                  {...register('senha')}
                  placeholder="Mínimo 8 caracteres"
                  disabled={mutation.isPending}
                />
                {'senha' in errors && errors.senha && (
                  <p className="text-sm text-red-600">{errors.senha.message}</p>
                )}
                <p className="text-sm text-gray-500">
                  O barbeiro receberá esta senha por email e poderá alterá-la depois.
                </p>
              </div>
            )}

            {/* Serviços */}
            <div className="space-y-2">
              <fieldset
                className="space-y-3 rounded-md border p-4"
                aria-describedby={servicesDescribedBy || undefined}
              >
                <legend className="text-sm font-medium">Serviços *</legend>
                {servicesData && servicesData.items.length > 0 ? (
                  servicesData.items.map((service) => (
                    <div key={service.id} className="flex items-center space-x-2">
                      <Checkbox
                        id={`service-${service.id}`}
                        checked={selectedServiceIds?.includes(service.id)}
                        onCheckedChange={(checked) =>
                          handleServiceToggle(service.id, checked as boolean)
                        }
                        disabled={mutation.isPending}
                      />
                      <label
                        htmlFor={`service-${service.id}`}
                        className="text-sm font-medium leading-none cursor-pointer peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                      >
                        {service.name} - R$ {(service.price / 100).toFixed(2)} ({service.durationMinutes} min)
                      </label>
                    </div>
                  ))
                ) : (
                  <p className="text-sm text-gray-500">
                    Nenhum serviço disponível. Cadastre serviços primeiro.
                  </p>
                )}
              </fieldset>
              {errors.serviceIds && (
                <p
                  id={servicesErrorId}
                  role="alert"
                  className="text-sm text-red-600"
                >
                  {errors.serviceIds.message}
                </p>
              )}
              <p id={servicesHelperId} className="text-sm text-gray-500">
                Selecione os serviços que este barbeiro pode realizar.
              </p>
            </div>

            {/* Ações */}
            <div className="flex gap-4">
              <Button
                type="submit"
                disabled={mutation.isPending}
                className="min-w-[120px]"
              >
                {mutation.isPending
                  ? 'Salvando...'
                  : isEditing
                  ? 'Atualizar'
                  : 'Cadastrar'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={handleCancel}
                disabled={mutation.isPending}
              >
                Cancelar
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  );
}
