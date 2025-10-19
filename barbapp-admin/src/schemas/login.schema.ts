import { z } from 'zod';

/**
 * Schema de validação para o formulário de login de barbeiros
 * 
 * Regras:
 * - email: obrigatório, formato de e-mail válido
 * - password: obrigatório, mínimo 6 caracteres
 */
export const barberLoginSchema = z.object({
  email: z
    .string()
    .min(1, 'E-mail é obrigatório')
    .email('E-mail inválido'),
  password: z
    .string()
    .min(1, 'Senha é obrigatória')
    .min(6, 'Senha deve ter no mínimo 6 caracteres'),
});

/**
 * Tipo inferido do schema de login de barbeiros
 */
export type BarberLoginFormData = z.infer<typeof barberLoginSchema>;
