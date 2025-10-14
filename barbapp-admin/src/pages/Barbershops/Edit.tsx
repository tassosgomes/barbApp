import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barbershopEditSchema, BarbershopEditFormData } from '@/schemas/barbershop.schema';
import { BarbershopEditForm } from '@/components/barbershop/BarbershopEditForm';
import { Button } from '@/components/ui/button';
import { useToast } from '@/hooks/use-toast';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types';

export function BarbershopEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [barbershop, setBarbershop] = useState<Barbershop | null>(null);
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { register, handleSubmit, formState: { errors, isDirty }, setValue, reset, watch } = useForm<BarbershopEditFormData>({
    resolver: zodResolver(barbershopEditSchema),
  });

  // Load existing data
  useEffect(() => {
    async function loadBarbershop() {
      if (!id) return;
      try {
        const data = await barbershopService.getById(id);
        setBarbershop(data);
        reset({
          name: data.name,
          ownerName: data.ownerName,
          email: data.email,
          phone: data.phone,
          address: data.address,
        });
      } catch (_error) { // eslint-disable-line @typescript-eslint/no-unused-vars
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
  }, [id, reset, toast, navigate]);

  // Confirm navigation if unsaved changes
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  const onSubmit = async (data: BarbershopEditFormData) => {
    if (!id || !barbershop) return;
    setIsSubmitting(true);
    try {
      await barbershopService.update(id, {
        id,
        name: data.name,
        phone: data.phone,
        ownerName: data.ownerName,
        email: data.email,
        zipCode: data.address.zipCode,
        street: data.address.street,
        number: data.address.number,
        complement: data.address.complement,
        neighborhood: data.address.neighborhood,
        city: data.address.city,
        state: data.address.state,
      });
      toast({
        title: 'Barbearia atualizada com sucesso!',
        description: 'As alterações foram salvas.',
      });
      navigate('/barbearias');
    } catch (_error) { // eslint-disable-line @typescript-eslint/no-unused-vars
      toast({
        title: 'Erro ao atualizar barbearia',
        description: 'Verifique os dados e tente novamente.',
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancel = () => {
    if (isDirty) {
      if (window.confirm('Você tem alterações não salvas. Deseja realmente sair?')) {
        navigate('/barbearias');
      }
    } else {
      navigate('/barbearias');
    }
  };

  if (loading) {
    return (
      <div className="flex items-center justify-center min-h-96">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-gray-900 mx-auto"></div>
          <p className="mt-2 text-gray-600">Carregando...</p>
        </div>
      </div>
    );
  }

  if (!barbershop) {
    return null;
  }

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Editar Barbearia</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl space-y-6">
        <BarbershopEditForm
          register={register}
          errors={errors}
          setValue={setValue}
          watch={watch}
          readOnlyData={{
            document: barbershop.document,
            code: barbershop.code,
          }}
        />

        <div className="flex gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
          <Button
            type="submit"
            disabled={isSubmitting || !isDirty}
          >
            {isSubmitting ? 'Salvando...' : 'Salvar Alterações'}
          </Button>
        </div>
      </form>
    </div>
  );
}