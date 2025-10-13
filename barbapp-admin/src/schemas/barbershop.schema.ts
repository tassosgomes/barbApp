import { z } from 'zod';

// ===========================
// REGEX PATTERNS
// ===========================

/**
 * Brazilian phone format: (99) 99999-9999
 */
const phoneRegex = /^\(\d{2}\) \d{5}-\d{4}$/;

/**
 * Brazilian ZIP code format: 99999-999
 */
const zipCodeRegex = /^\d{5}-\d{3}$/;

/**
 * Brazilian document format: CPF (999.999.999-99) or CNPJ (99.999.999/9999-99)
 */
const documentRegex = /^(\d{3}\.\d{3}\.\d{3}-\d{2}|\d{2}\.\d{3}\.\d{3}\/\d{4}-\d{2})$/;

// ===========================
// ADDRESS SCHEMA
// ===========================

export const addressSchema = z.object({
  street: z
    .string()
    .min(3, 'Logradouro deve ter no mínimo 3 caracteres')
    .max(200, 'Logradouro deve ter no máximo 200 caracteres')
    .trim(),

  number: z
    .string()
    .min(1, 'Número é obrigatório')
    .max(10, 'Número deve ter no máximo 10 caracteres')
    .trim(),

  complement: z
    .string()
    .max(100, 'Complemento deve ter no máximo 100 caracteres')
    .trim()
    .optional(),

  neighborhood: z
    .string()
    .min(3, 'Bairro deve ter no mínimo 3 caracteres')
    .max(100, 'Bairro deve ter no máximo 100 caracteres')
    .trim(),

  city: z
    .string()
    .min(3, 'Cidade deve ter no mínimo 3 caracteres')
    .max(100, 'Cidade deve ter no máximo 100 caracteres')
    .trim(),

  state: z
    .string()
    .length(2, 'Estado deve ter 2 caracteres (ex: SP, RJ)')
    .regex(/^[A-Za-z]{2}$/, 'Estado deve conter apenas letras')
    .transform((val) => val.toUpperCase()),

  zipCode: z
    .string()
    .regex(zipCodeRegex, 'CEP inválido. Formato esperado: 99999-999')
    .trim(),
});

// ===========================
// BARBERSHOP SCHEMA
// ===========================

export const barbershopSchema = z.object({
  name: z
    .string()
    .min(3, 'Nome deve ter no mínimo 3 caracteres')
    .max(100, 'Nome deve ter no máximo 100 caracteres')
    .trim(),

  document: z
    .string()
    .regex(documentRegex, 'Documento inválido. Formato esperado: CPF (999.999.999-99) ou CNPJ (99.999.999/9999-99)')
    .trim(),

  ownerName: z
    .string()
    .min(3, 'Nome do proprietário deve ter no mínimo 3 caracteres')
    .max(100, 'Nome do proprietário deve ter no máximo 100 caracteres')
    .trim(),

  email: z
    .string()
    .email('Email inválido')
    .max(100, 'Email deve ter no máximo 100 caracteres')
    .toLowerCase()
    .trim(),

  phone: z
    .string()
    .regex(phoneRegex, 'Telefone inválido. Formato esperado: (99) 99999-9999')
    .trim(),

  address: addressSchema,
});

/**
 * Type inference from schema - matches CreateBarbershopRequest
 */
export type BarbershopFormData = z.infer<typeof barbershopSchema>;

// ===========================
// LOGIN SCHEMA
// ===========================

export const loginSchema = z.object({
  email: z
    .string()
    .email('Email inválido')
    .toLowerCase()
    .trim(),

  password: z
    .string()
    .min(6, 'Senha deve ter no mínimo 6 caracteres')
    .max(100, 'Senha deve ter no máximo 100 caracteres'),
});

/**
 * Type inference from schema - matches LoginRequest
 */
export type LoginFormData = z.infer<typeof loginSchema>;