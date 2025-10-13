import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barbershopSchema, BarbershopFormData } from '@/schemas/barbershop.schema';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';
import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { barbershopService } from '@/services/barbershop.service';
import { handleApiError } from '@/utils/errorHandler';
import type { Barbershop } from '@/types';

export function BarbershopCreate() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [createdBarbershop, setCreatedBarbershop] = useState<Barbershop | null>(null);

  const { register, handleSubmit, formState: { errors }, setValue } = useForm<BarbershopFormData>({
    resolver: zodResolver(barbershopSchema),
  });

  const onSubmit = async (data: BarbershopFormData) => {
    setIsSubmitting(true);
    try {
      // Flatten address for API
      const requestData = {
        name: data.name,
        document: data.document,
        ownerName: data.ownerName,
        email: data.email,
        phone: data.phone,
        zipCode: data.address.zipCode,
        street: data.address.street,
        number: data.address.number,
        complement: data.address.complement,
        neighborhood: data.address.neighborhood,
        city: data.address.city,
        state: data.address.state,
      };

      const result = await barbershopService.create(requestData);
      setCreatedBarbershop(result);
      toast({
        title: 'Barbearia cadastrada com sucesso!',
        description: `Código gerado: ${result.code || 'N/A'}`,
      });
    } catch (error) {
      toast({
        title: 'Erro ao cadastrar barbearia',
        description: handleApiError(error),
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (createdBarbershop) {
    return (
      <div className="space-y-6">
        <div className="text-center">
          <h1 className="text-3xl font-bold text-green-600">Barbearia Criada com Sucesso!</h1>
          <p className="mt-4 text-lg">Código único gerado:</p>
          <div className="mt-4 p-4 bg-green-50 border border-green-200 rounded-lg">
            <p className="text-2xl font-mono font-bold text-green-800">
              {createdBarbershop.code || 'Código não disponível'}
            </p>
          </div>
          <div className="mt-6 flex gap-4 justify-center">
            <Button onClick={() => navigate('/barbearias')}>
              Voltar à Lista
            </Button>
            <Button variant="outline" onClick={() => {
              setCreatedBarbershop(null);
              // Reset form if needed
            }}>
              Criar Outra
            </Button>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Nova Barbearia</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl space-y-6">
        <BarbershopForm
          register={register}
          errors={errors}
          setValue={setValue}
        />

        <div className="flex gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Salvando...' : 'Salvar'}
          </Button>
        </div>
      </form>
    </div>
  );
}