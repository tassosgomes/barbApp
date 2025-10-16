import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { serviceSchema } from '@/schemas/service';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { CreateServiceRequest, UpdateServiceRequest, BarbershopService } from '@/types';

interface ServiceFormProps {
  service?: BarbershopService;
  onSubmit: (data: CreateServiceRequest | UpdateServiceRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function ServiceForm({ service, onSubmit, onCancel, isLoading = false }: ServiceFormProps) {
  const isEditing = !!service;

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<CreateServiceRequest>({
    resolver: zodResolver(serviceSchema),
    defaultValues: {
      name: service?.name || '',
      description: service?.description || '',
      durationMinutes: service?.durationMinutes || 0,
      price: service?.price || 0,
    },
  });

  const priceValue = watch('price');

  useEffect(() => {
    if (isEditing && service) {
      setValue('name', service.name);
      setValue('description', service.description);
      setValue('durationMinutes', service.durationMinutes);
      setValue('price', service.price);
    }
  }, [service, setValue, isEditing]);

  const handleFormSubmit = async (data: CreateServiceRequest) => {
    await onSubmit(data as CreateServiceRequest | UpdateServiceRequest);
  };

  const formatCurrency = (value: number): string => {
    return new Intl.NumberFormat('pt-BR', {
      style: 'currency',
      currency: 'BRL',
    }).format(value);
  };

  const handlePriceChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value.replace(/\D/g, '');
    const numericValue = parseFloat(value) / 100;
    setValue('price', numericValue);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 gap-4">
        <div className="space-y-2">
          <Label htmlFor="name">Nome *</Label>
          <Input
            id="name"
            {...register('name')}
            placeholder="Nome do serviço"
            disabled={isLoading}
          />
          {errors.name && (
            <p className="text-sm text-red-600">{String(errors.name.message)}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="description">Descrição *</Label>
          <Input
            id="description"
            {...register('description')}
            placeholder="Descrição do serviço"
            disabled={isLoading}
          />
          {errors.description && (
            <p className="text-sm text-red-600">{String(errors.description.message)}</p>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div className="space-y-2">
            <Label htmlFor="durationMinutes">Duração (minutos) *</Label>
            <Input
              id="durationMinutes"
              type="number"
              {...register('durationMinutes', { valueAsNumber: true })}
              placeholder="30"
              disabled={isLoading}
              min="1"
            />
            {errors.durationMinutes && (
              <p className="text-sm text-red-600">{String(errors.durationMinutes.message)}</p>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="price">Preço *</Label>
            <Input
              id="price"
              type="text"
              value={formatCurrency(priceValue || 0)}
              onChange={handlePriceChange}
              placeholder="R$ 0,00"
              disabled={isLoading}
            />
            {errors.price && (
              <p className="text-sm text-red-600">{String(errors.price.message)}</p>
            )}
          </div>
        </div>
      </div>

      <div className="flex justify-end gap-3">
        <Button
          type="button"
          variant="outline"
          onClick={onCancel}
          disabled={isLoading}
        >
          Cancelar
        </Button>
        <Button type="submit" disabled={isLoading}>
          {isLoading ? 'Salvando...' : isEditing ? 'Atualizar' : 'Criar'}
        </Button>
      </div>
    </form>
  );
}
