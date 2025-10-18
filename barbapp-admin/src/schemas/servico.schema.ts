/**
 * Schemas Zod para validação de serviços
 * Compatível com especificação da Task 11.0
 */

import { z } from 'zod';

/**
 * Schema para criação de serviço
 * Valida: nome (min 3 chars), descrição opcional, duração (1-480 min), preço (>= 0)
 * 
 * Regras de negócio:
 * - Nome: mínimo 3 caracteres, máximo 100
 * - Duração: entre 1 minuto e 480 minutos (8 horas)
 * - Preço: não pode ser negativo
 */
export const createServicoSchema = z.object({
  nome: z
    .string()
    .min(3, 'Nome deve ter no mínimo 3 caracteres')
    .max(100, 'Nome deve ter no máximo 100 caracteres')
    .trim(),
  
  descricao: z
    .string()
    .max(500, 'Descrição deve ter no máximo 500 caracteres')
    .optional()
    .or(z.literal('')),
  
  duracaoMinutos: z
    .number({
      required_error: 'Duração é obrigatória',
      invalid_type_error: 'Duração deve ser um número',
    })
    .int('Duração deve ser um número inteiro')
    .min(1, 'Duração deve ser maior que 0')
    .max(480, 'Duração máxima de 8 horas (480 minutos)'),
  
  preco: z
    .number({
      required_error: 'Preço é obrigatório',
      invalid_type_error: 'Preço deve ser um número',
    })
    .min(0, 'Preço não pode ser negativo')
    .max(99999.99, 'Preço muito alto'),
});

/**
 * Schema para atualização de serviço
 * Todos os campos são opcionais
 * Mantém as mesmas validações do schema de criação
 */
export const updateServicoSchema = z.object({
  nome: z
    .string()
    .min(3, 'Nome deve ter no mínimo 3 caracteres')
    .max(100, 'Nome deve ter no máximo 100 caracteres')
    .trim()
    .optional(),
  
  descricao: z
    .string()
    .max(500, 'Descrição deve ter no máximo 500 caracteres')
    .optional()
    .or(z.literal('')),
  
  duracaoMinutos: z
    .number({
      invalid_type_error: 'Duração deve ser um número',
    })
    .int('Duração deve ser um número inteiro')
    .min(1, 'Duração deve ser maior que 0')
    .max(480, 'Duração máxima de 8 horas (480 minutos)')
    .optional(),
  
  preco: z
    .number({
      invalid_type_error: 'Preço deve ser um número',
    })
    .min(0, 'Preço não pode ser negativo')
    .max(99999.99, 'Preço muito alto')
    .optional(),
});

/**
 * Tipos inferidos dos schemas para uso em formulários
 */
export type CreateServicoFormData = z.infer<typeof createServicoSchema>;
export type UpdateServicoFormData = z.infer<typeof updateServicoSchema>;

// Aliases compatíveis com nomenclatura em inglês (para compatibilidade com código existente)
export { createServicoSchema as createServiceSchemaV2 };
export { updateServicoSchema as updateServiceSchemaV2 };
export type { CreateServicoFormData as CreateServiceFormDataV2 };
export type { UpdateServicoFormData as UpdateServiceFormDataV2 };
