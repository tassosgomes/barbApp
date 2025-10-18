import { z } from 'zod';

/**
 * Schema for validating barbershop code from URL
 * Code must be exactly 8 uppercase alphanumeric characters
 */
export const codigoSchema = z
  .string()
  .length(8, 'Código deve ter 8 caracteres')
  .regex(/^[A-Z0-9]{8}$/, 'Código deve conter apenas letras maiúsculas e números');

/**
 * Schema for barbershop info returned from validation endpoint
 */
export const barbeariaInfoSchema = z.object({
  id: z.string().uuid(),
  nome: z.string(),
  codigo: codigoSchema,
  isActive: z.boolean(),
});

/**
 * Schema for Admin Barbearia login form
 * Note: Code comes from URL, not from form
 */
export const loginAdminBarbeariaSchema = z.object({
  email: z
    .string()
    .email('Email inválido')
    .toLowerCase()
    .trim(),

  senha: z
    .string()
    .min(6, 'Senha deve ter no mínimo 6 caracteres')
    .max(100, 'Senha deve ter no máximo 100 caracteres'),
});

export type LoginAdminBarbeariaFormData = z.infer<typeof loginAdminBarbeariaSchema>;