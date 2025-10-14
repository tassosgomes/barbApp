import { useEffect } from 'react';
import { UseFormRegister, FieldErrors, UseFormSetValue, UseFormWatch } from 'react-hook-form';
import { BarbershopFormData } from '@/schemas/barbershop.schema';
import { FormField } from '@/components/form/FormField';
import { MaskedInput } from '@/components/form/MaskedInput';
import { useViaCep } from '@/hooks/useViaCep';
import { useToast } from '@/hooks/use-toast';
import { Loader2 } from 'lucide-react';

interface BarbershopFormProps {
  register: UseFormRegister<BarbershopFormData>;
  errors: FieldErrors<BarbershopFormData>;
  setValue: UseFormSetValue<BarbershopFormData>;
  watch: UseFormWatch<BarbershopFormData>;
}

export function BarbershopForm({ register, errors, setValue, watch }: BarbershopFormProps) {
  const { searchCep, loading, error, data, clearError } = useViaCep();
  const { toast } = useToast();
  const zipCode = watch('address.zipCode');

  // Auto-search address when CEP has 8 digits
  useEffect(() => {
    const cepDigits = zipCode?.replace(/\D/g, '');
    
    if (cepDigits && cepDigits.length === 8) {
      searchCep(zipCode);
    }
  }, [zipCode, searchCep]);

  // Fill form when address data is received
  useEffect(() => {
    if (data) {
      setValue('address.street', data.street);
      setValue('address.neighborhood', data.neighborhood);
      setValue('address.city', data.city);
      setValue('address.state', data.state);

      toast({
        title: 'Endereço encontrado',
        description: 'Os campos foram preenchidos automaticamente.',
      });
    }
  }, [data, setValue, toast]);

  // Show error toast
  useEffect(() => {
    if (error) {
      toast({
        title: 'Erro ao buscar CEP',
        description: error,
        variant: 'destructive',
      });
      clearError();
    }
  }, [error, toast, clearError]);
  return (
    <div className="space-y-6">
      {/* Informações Gerais */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold">Informações Gerais</h2>

        <FormField
          label="Nome da Barbearia"
          name="name"
          placeholder="Nome da Barbearia"
          error={errors.name}
          register={register}
          required
        />

        <FormField
          label="Documento (CPF/CNPJ)"
          name="document"
          error={errors.document}
          register={register}
          required
        >
          <MaskedInput
            mask="document"
            placeholder="999.999.999-99 ou 99.999.999/9999-99"
            onChange={(value) => setValue('document', value)}
          />
        </FormField>

        <FormField
          label="Proprietário"
          name="ownerName"
          placeholder="Nome do proprietário"
          error={errors.ownerName}
          register={register}
          required
        />

        <FormField
          label="Email"
          name="email"
          type="email"
          placeholder="email@exemplo.com"
          error={errors.email}
          register={register}
          required
        />

        <FormField
          label="Telefone"
          name="phone"
          error={errors.phone}
          register={register}
          required
        >
          <MaskedInput
            mask="phone"
            placeholder="(99) 99999-9999"
            onChange={(value) => setValue('phone', value)}
          />
        </FormField>
      </div>

      {/* Endereço */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold">Endereço</h2>

        <FormField
          label="CEP"
          name="address.zipCode"
          error={errors.address?.zipCode}
          register={register}
          required
        >
          <div className="relative">
            <MaskedInput
              mask="cep"
              placeholder="99999-999"
              onChange={(value) => setValue('address.zipCode', value)}
            />
            {loading && (
              <div className="absolute right-3 top-1/2 -translate-y-1/2">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              </div>
            )}
          </div>
          <p className="text-xs text-muted-foreground mt-1">
            Digite o CEP para preencher o endereço automaticamente
          </p>
        </FormField>

        <div className="grid grid-cols-3 gap-4">
          <div className="col-span-2">
            <FormField
              label="Logradouro"
              name="address.street"
              placeholder="Rua, Avenida, etc."
              error={errors.address?.street}
              register={register}
              required
            />
          </div>

          <FormField
            label="Número"
            name="address.number"
            placeholder="123"
            error={errors.address?.number}
            register={register}
            required
          />
        </div>

        <FormField
          label="Complemento"
          name="address.complement"
          placeholder="Apto, Sala, etc. (opcional)"
          error={errors.address?.complement}
          register={register}
        />

        <FormField
          label="Bairro"
          name="address.neighborhood"
          placeholder="Nome do bairro"
          error={errors.address?.neighborhood}
          register={register}
          required
        />

        <div className="grid grid-cols-2 gap-4">
          <FormField
            label="Cidade"
            name="address.city"
            placeholder="Nome da cidade"
            error={errors.address?.city}
            register={register}
            required
          />

          <FormField
            label="Estado"
            name="address.state"
            placeholder="SP"
            error={errors.address?.state}
            register={register}
            required
          />
        </div>
      </div>
    </div>
  );
}