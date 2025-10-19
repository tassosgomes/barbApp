import { z } from 'zod';

/**
 * Schema de validação para o formulário de login de barbeiros
 * 
 * Regras:
 * - barbershopCode: obrigatório, mínimo 6 caracteres, convertido para maiúsculas
 * - phone: obrigatório, formato brasileiro (XX) XXXXX-XXXX
 */
export const barberLoginSchema = z.object({
  barbershopCode: z
    .string()
    .min(1, 'Código da barbearia é obrigatório')
    .min(6, 'Código da barbearia muito curto. Mínimo 6 caracteres')
    .transform((val) => val.toUpperCase()),
  phone: z
    .string()
    .min(1, 'Telefone é obrigatório')
    .regex(
      /^\(\d{2}\) \d{5}-\d{4}$/,
      'Telefone inválido. Use o formato (XX) XXXXX-XXXX'
    ),
});

/**
 * Tipo inferido do schema de login de barbeiros
 */
export type BarberLoginFormData = z.infer<typeof barberLoginSchema>;
