import { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { servicoService } from '@/services/servico.service';
import {
  createServicoSchema,
  updateServicoSchema,
  type CreateServicoFormData,
  type UpdateServicoFormData,
} from '@/schemas/servico.schema';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { useToast } from '@/hooks/use-toast';
import { Skeleton } from '@/components/ui/skeleton';
import { ArrowLeft } from 'lucide-react';

/**
 * ServicoFormPage - Página de criação/edição de serviço
 * 
 * Features:
 * - Formulário com validação Zod
 * - Suporte para criação e edição
 * - Carrega dados existentes no modo edição
 * - Validação de preço e duração
 * - Toast feedback
 * - Loading states
 */
export function ServicoFormPage() {
  const navigate = useNavigate();
  const { id } = useParams();
  const { barbearia } = useBarbearia();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const isEditing = !!id;

  // Buscar serviço existente (modo edição)
  const { data: servico, isLoading: isLoadingServico } = useQuery({
    queryKey: ['servico', id],
    queryFn: () => servicoService.getById(id!),
    enabled: isEditing,
  });

  // Setup do formulário
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateServicoFormData | UpdateServicoFormData>({
    resolver: zodResolver(isEditing ? updateServicoSchema : createServicoSchema),
  });

  // Carregar dados do serviço no formulário (modo edição)
  useEffect(() => {
    if (servico && isEditing) {
      reset({
        nome: servico.name,
        descricao: servico.description || '',
        duracaoMinutos: servico.durationMinutes,
        preco: servico.price,
      });
    }
  }, [servico, isEditing, reset]);

  // Mutation para criar/atualizar serviço
  const mutation = useMutation({
    mutationFn: (data: CreateServicoFormData | UpdateServicoFormData) => {
      // Transform para formato da API (backend usa inglês)
      const apiData = {
        name: data.nome!,
        description: data.descricao || '',
        durationMinutes: data.duracaoMinutos!,
        price: data.preco!,
      };

      if (isEditing) {
        return servicoService.update(id!, apiData);
      }
      return servicoService.create(apiData);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['servicos'] });
      toast({
        title: `Serviço ${isEditing ? 'atualizado' : 'criado'} com sucesso`,
        description: `O serviço foi ${isEditing ? 'atualizado' : 'cadastrado'} com sucesso.`,
      });
      navigate(`/${barbearia?.codigo}/servicos`);
    },
    onError: (error: any) => {
      const errorMessage =
        error?.response?.data?.message || 'Ocorreu um erro ao salvar o serviço.';
      toast({
        title: `Erro ao ${isEditing ? 'atualizar' : 'criar'} serviço`,
        description: errorMessage,
        variant: 'destructive',
      });
    },
  });

  const onSubmit = (data: CreateServicoFormData | UpdateServicoFormData) => {
    mutation.mutate(data);
  };

  const handleBack = () => {
    navigate(`/${barbearia?.codigo}/servicos`);
  };

  // Loading state
  if (isLoadingServico && isEditing) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-64" />
        <Card>
          <CardHeader>
            <Skeleton className="h-6 w-48" />
          </CardHeader>
          <CardContent className="space-y-4">
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-10 w-full" />
            <Skeleton className="h-10 w-full" />
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header com botão voltar */}
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" onClick={handleBack}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div>
          <h1 className="text-3xl font-bold">
            {isEditing ? 'Editar Serviço' : 'Novo Serviço'}
          </h1>
          <p className="text-gray-600 mt-1">
            {isEditing
              ? 'Atualize as informações do serviço'
              : 'Cadastre um novo serviço oferecido pela barbearia'}
          </p>
        </div>
      </div>

      {/* Formulário */}
      <Card>
        <CardHeader>
          <CardTitle>Informações do Serviço</CardTitle>
          <CardDescription>
            Preencha os dados do serviço. Todos os campos marcados com * são obrigatórios.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
            {/* Nome */}
            <div className="space-y-2">
              <Label htmlFor="nome">
                Nome do Serviço <span className="text-red-500">*</span>
              </Label>
              <Input
                id="nome"
                placeholder="Ex: Corte de Cabelo"
                {...register('nome')}
              />
              {errors.nome && (
                <p className="text-sm text-red-600">{errors.nome.message}</p>
              )}
            </div>

            {/* Descrição */}
            <div className="space-y-2">
              <Label htmlFor="descricao">Descrição</Label>
              <Textarea
                id="descricao"
                placeholder="Descreva o serviço..."
                rows={3}
                {...register('descricao')}
              />
              {errors.descricao && (
                <p className="text-sm text-red-600">{errors.descricao.message}</p>
              )}
              <p className="text-sm text-gray-500">Opcional</p>
            </div>

            {/* Duração e Preço lado a lado */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              {/* Duração */}
              <div className="space-y-2">
                <Label htmlFor="duracaoMinutos">
                  Duração (minutos) <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="duracaoMinutos"
                  type="number"
                  min="1"
                  max="480"
                  placeholder="30"
                  {...register('duracaoMinutos', { valueAsNumber: true })}
                />
                {errors.duracaoMinutos && (
                  <p className="text-sm text-red-600">{errors.duracaoMinutos.message}</p>
                )}
                <p className="text-sm text-gray-500">Duração em minutos (máx. 8 horas)</p>
              </div>

              {/* Preço */}
              <div className="space-y-2">
                <Label htmlFor="preco">
                  Preço (R$) <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="preco"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="35.00"
                  {...register('preco', { valueAsNumber: true })}
                />
                {errors.preco && (
                  <p className="text-sm text-red-600">{errors.preco.message}</p>
                )}
                <p className="text-sm text-gray-500">Valor em reais</p>
              </div>
            </div>

            {/* Botões de ação */}
            <div className="flex gap-3 pt-4">
              <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending
                  ? isEditing
                    ? 'Salvando...'
                    : 'Criando...'
                  : isEditing
                  ? 'Salvar Alterações'
                  : 'Criar Serviço'}
              </Button>
              <Button
                type="button"
                variant="outline"
                onClick={handleBack}
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
