import { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import { BarbershopFormData } from '@/schemas/barbershop.schema';
import { FormField } from '@/components/form/FormField';
import { MaskedInput } from '@/components/form/MaskedInput';

interface BarbershopFormProps {
  register: UseFormRegister<BarbershopFormData>;
  errors: FieldErrors<BarbershopFormData>;
  setValue: UseFormSetValue<BarbershopFormData>;
}

export function BarbershopForm({ register, errors, setValue }: BarbershopFormProps) {
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
          <MaskedInput
            mask="cep"
            placeholder="99999-999"
            onChange={(value) => setValue('address.zipCode', value)}
          />
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