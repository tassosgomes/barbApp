/**
 * Schemas Zod para validação de barbeiros
 * Compatível com especificação da Task 9.0
 */

import { z } from 'zod';

/**
 * Schema para criação de barbeiro
 * Valida: nome (min 3 chars), email, telefone formato brasileiro, senha (min 8 chars)
 * 
 * Nota: O telefone aceita formato com máscara (11) 98765-4321 ou sem máscara 11987654321
 * Será transformado para enviar apenas números para a API
 */
export const createBarbeiroSchema = z.object({
  nome: z.string().min(3, 'Nome deve ter no mínimo 3 caracteres').max(100, 'Nome deve ter no máximo 100 caracteres'),
  email: z.string().email('Email inválido').toLowerCase(),
  telefone: z
    .string()
    .regex(/^(\(\d{2}\)\s?)?\d{4,5}-?\d{4}$/, 'Telefone inválido. Use o formato (11) 98765-4321 ou 11987654321')
    .transform((val) => val.replace(/\D/g, '')), // Remove máscara antes de enviar para API
  senha: z.string().min(8, 'Senha deve ter no mínimo 8 caracteres'),
  serviceIds: z.array(z.string().uuid('ID de serviço inválido')).min(1, 'Selecione pelo menos um serviço'),
});

/**
 * Schema para atualização de barbeiro
 * Campos opcionais: nome, telefone, serviceIds
 * Nota: Email e senha não podem ser atualizados via este endpoint
 */
export const updateBarbeiroSchema = z.object({
  nome: z.string().min(3, 'Nome deve ter no mínimo 3 caracteres').max(100, 'Nome deve ter no máximo 100 caracteres').optional(),
  telefone: z
    .string()
    .regex(/^(\(\d{2}\)\s?)?\d{4,5}-?\d{4}$/, 'Telefone inválido. Use o formato (11) 98765-4321 ou 11987654321')
    .transform((val) => val.replace(/\D/g, '')) // Remove máscara antes de enviar para API
    .optional(),
  serviceIds: z.array(z.string().uuid('ID de serviço inválido')).min(1, 'Selecione pelo menos um serviço').optional(),
});

/**
 * Tipos inferidos dos schemas para uso em formulários
 */
export type CreateBarbeiroFormData = z.infer<typeof createBarbeiroSchema>;
export type UpdateBarbeiroFormData = z.infer<typeof updateBarbeiroSchema>;

// Aliases compatíveis com nomenclatura em inglês (para compatibilidade com código existente)
export { createBarbeiroSchema as createBarberSchemaV2 };
export { updateBarbeiroSchema as updateBarberSchemaV2 };
