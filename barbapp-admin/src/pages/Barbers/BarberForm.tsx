import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useEffect } from 'react';
import { createBarberSchema, updateBarberSchema } from '@/schemas/barber';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useServices } from '@/hooks';
import type { CreateBarberRequest, UpdateBarberRequest, Barber } from '@/types';

interface BarberFormProps {
  barber?: Barber;
  onSubmit: (data: CreateBarberRequest | UpdateBarberRequest) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

export function BarberForm({ barber, onSubmit, onCancel, isLoading = false }: BarberFormProps) {
  const isEditing = !!barber;
  const schema = isEditing ? updateBarberSchema : createBarberSchema;

  const { register, handleSubmit, formState: { errors }, setValue, watch } = useForm<CreateBarberRequest>({
    resolver: zodResolver(schema),
  });

  // Fetch services for the select
  const { data: servicesData } = useServices({ page: 1, pageSize: 100 }); // Get all services
  const services = servicesData?.items || [];

  const selectedServiceIds = watch('serviceIds') || [];

  useEffect(() => {
    if (isEditing && barber) {
      setValue('name', barber.name);
      setValue('phone', barber.phoneFormatted);
      setValue('serviceIds', barber.services.map(s => s.id));
    }
  }, [barber, setValue, isEditing]);

  const handleFormSubmit = async (data: CreateBarberRequest) => {
    await onSubmit(data as CreateBarberRequest | UpdateBarberRequest);
  };

  return (
    <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="name">Nome *</Label>
          <Input
            id="name"
            {...register('name')}
            placeholder="Nome do barbeiro"
            disabled={isLoading}
          />
          {errors.name && (
            <p className="text-sm text-red-600">{String(errors.name.message)}</p>
          )}
        </div>

        {!isEditing && (
          <div className="space-y-2">
            <Label htmlFor="email">Email *</Label>
            <Input
              id="email"
              type="email"
              {...register('email')}
              placeholder="email@exemplo.com"
              disabled={isLoading}
            />
            {errors.email && (
              <p className="text-sm text-red-600">{String(errors.email.message)}</p>
            )}
          </div>
        )}

        <div className="space-y-2">
          <Label htmlFor="phone">Telefone *</Label>
          <Input
            id="phone"
            {...register('phone')}
            placeholder="(11) 99999-9999"
            disabled={isLoading}
          />
          {errors.phone && (
            <p className="text-sm text-red-600">{String(errors.phone.message)}</p>
          )}
        </div>

        {!isEditing && (
          <div className="space-y-2">
            <Label htmlFor="password">Senha *</Label>
            <Input
              id="password"
              type="password"
              {...register('password')}
              placeholder="Senha inicial"
              disabled={isLoading}
            />
            {errors.password && (
              <p className="text-sm text-red-600">{String(errors.password.message)}</p>
            )}
          </div>
        )}
      </div>

      <div className="space-y-2">
        <Label>Serviços *</Label>
        <div className="grid grid-cols-2 md:grid-cols-3 gap-2">
          {services.map(service => (
            <label key={service.id} className="flex items-center space-x-2">
              <input
                type="checkbox"
                checked={selectedServiceIds.includes(service.id)}
                onChange={(e) => {
                  const current = selectedServiceIds;
                  if (e.target.checked) {
                    setValue('serviceIds', [...current, service.id]);
                  } else {
                    setValue('serviceIds', current.filter((id: string) => id !== service.id));
                  }
                }}
                disabled={isLoading}
                className="rounded"
              />
              <span className="text-sm">{service.name}</span>
            </label>
          ))}
        </div>
        {errors.serviceIds && (
          <p className="text-sm text-red-600">{String(errors.serviceIds.message)}</p>
        )}
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