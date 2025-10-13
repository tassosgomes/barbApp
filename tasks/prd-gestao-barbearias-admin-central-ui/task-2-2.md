# Task 2.2: Zod Validation Schemas

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 1 day
**Phase**: Phase 2 - Type Safety and API

## Description

Create comprehensive Zod validation schemas for all forms in the application, including barbershop creation/editing, login, and address validation with custom validators for Brazilian formats (CEP, CNPJ, phone).

These schemas provide runtime validation and type safety for form inputs.

## Acceptance Criteria

- [ ] Barbershop schema with nested address validation
- [ ] Login schema with email and password validation
- [ ] Custom validators for Brazilian formats (phone, CEP)
- [ ] Clear, user-friendly error messages in Portuguese
- [ ] Schema type inference matches TypeScript types
- [ ] All schemas exported via barrel export
- [ ] Unit tests for all validation rules
- [ ] Edge cases handled (empty strings, special characters, etc.)

## Dependencies

**Blocking Tasks**:
- Task 2.1 (TypeScript Types) must be completed

**Blocked Tasks**:
- Task 3.1 (Login Page) - uses loginSchema
- Task 7.1 (Create Page) - uses barbershopSchema
- Task 8.1 (Edit Page) - uses barbershopSchema
- All form implementations depend on these schemas

## Implementation Notes

### File to Create

Based on Tech Spec section 4.1:

**src/schemas/barbershop.schema.ts**

### Schema Definitions

```typescript
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
 * State code: 2 uppercase letters (e.g., SP, RJ)
 */
const stateRegex = /^[A-Z]{2}$/;

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
    .regex(stateRegex, 'Estado deve conter apenas letras maiúsculas')
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

// ===========================
// BARREL EXPORT
// ===========================

// Export schemas
export { addressSchema, barbershopSchema, loginSchema };

// Export inferred types
export type { BarbershopFormData, LoginFormData };
```

### Custom Validators (Future Enhancement)

For MVP, use regex validation. Post-MVP, add custom validators:

```typescript
// src/utils/validators.ts (optional, post-MVP)

/**
 * Validate CNPJ checksum (advanced validation)
 */
export function validateCNPJ(cnpj: string): boolean {
  // Implementation of CNPJ checksum algorithm
  // ...
  return true;
}

/**
 * Custom Zod validator for CNPJ
 */
export const cnpjValidator = z.string().refine(
  (val) => validateCNPJ(val),
  { message: 'CNPJ inválido' }
);
```

## Testing Requirements

- [ ] Unit tests for all validation rules
- [ ] Test valid inputs pass validation
- [ ] Test invalid inputs fail with correct error messages
- [ ] Test edge cases (empty, whitespace, special chars)
- [ ] Test schema type inference matches TypeScript types
- [ ] Test transform functions (toUpperCase for state)

**Test File** (`src/__tests__/unit/schemas/barbershop.schema.test.ts`):

```typescript
import { describe, it, expect } from 'vitest';
import { barbershopSchema, loginSchema } from '@/schemas/barbershop.schema';

describe('Barbershop Schema', () => {
  describe('valid inputs', () => {
    it('should validate complete barbershop data', () => {
      const validData = {
        name: 'Barbearia Teste',
        email: 'teste@barbapp.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Rua Teste',
          number: '123',
          complement: 'Apto 45',
          neighborhood: 'Centro',
          city: 'São Paulo',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(validData);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.name).toBe('Barbearia Teste');
      }
    });

    it('should transform state to uppercase', () => {
      const data = {
        name: 'Test',
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'sp', // lowercase
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(data);
      expect(result.success).toBe(true);
      if (result.success) {
        expect(result.data.address.state).toBe('SP'); // transformed
      }
    });
  });

  describe('invalid inputs', () => {
    it('should fail validation for short name', () => {
      const invalidData = {
        name: 'AB', // too short
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('mínimo 3 caracteres');
      }
    });

    it('should fail validation for invalid email', () => {
      const invalidData = {
        name: 'Test Name',
        email: 'invalid-email', // invalid format
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toBe('Email inválido');
      }
    });

    it('should fail validation for invalid phone format', () => {
      const invalidData = {
        name: 'Test Name',
        email: 'test@test.com',
        phone: '11999999999', // missing formatting
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000-000',
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        expect(result.error.issues[0].message).toContain('Formato esperado');
      }
    });

    it('should fail validation for invalid CEP format', () => {
      const invalidData = {
        name: 'Test Name',
        email: 'test@test.com',
        phone: '(11) 99999-9999',
        address: {
          street: 'Street',
          number: '123',
          neighborhood: 'Area',
          city: 'City',
          state: 'SP',
          zipCode: '01000000', // missing dash
        },
      };

      const result = barbershopSchema.safeParse(invalidData);
      expect(result.success).toBe(false);
      if (!result.success) {
        const zipCodeError = result.error.issues.find((issue) =>
          issue.path.includes('zipCode')
        );
        expect(zipCodeError?.message).toContain('CEP inválido');
      }
    });
  });
});

describe('Login Schema', () => {
  it('should validate correct login credentials', () => {
    const validData = {
      email: 'admin@barbapp.com',
      password: 'SecurePass123',
    };

    const result = loginSchema.safeParse(validData);
    expect(result.success).toBe(true);
  });

  it('should fail validation for short password', () => {
    const invalidData = {
      email: 'admin@barbapp.com',
      password: '12345', // too short
    };

    const result = loginSchema.safeParse(invalidData);
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.issues[0].message).toContain('mínimo 6 caracteres');
    }
  });
});
```

## Files to Create/Modify

**Create**:
- `src/schemas/barbershop.schema.ts`
- `src/schemas/index.ts` (barrel export)
- `src/__tests__/unit/schemas/barbershop.schema.test.ts`

**Barrel Export** (`src/schemas/index.ts`):
```typescript
export * from './barbershop.schema';
```

## Verification Checklist

Before marking as complete:

1. ✅ All schemas defined in `src/schemas/barbershop.schema.ts`
2. ✅ Barrel export created in `src/schemas/index.ts`
3. ✅ Type inference types exported (BarbershopFormData, LoginFormData)
4. ✅ Error messages in Portuguese and user-friendly
5. ✅ All validation rules tested (unit tests pass)
6. ✅ Edge cases tested (empty, whitespace, special characters)
7. ✅ Schema types match TypeScript types from Task 2.1
8. ✅ Transform functions work correctly (state uppercase)
9. ✅ Can import schemas using `@/schemas` alias
10. ✅ Git commit created for validation schemas

## Reference

- **Tech Spec Section**: 4.1 (Zod Validation)
- **PRD**: Section 2 (Cadastro), Section 9 (Login)
- **Zod Documentation**: https://zod.dev/

## Notes

- Error messages should guide users to correct input format
- Use `.trim()` to handle whitespace in text inputs
- Use `.toLowerCase()` for emails to normalize input
- Use `.transform()` for state to ensure uppercase
- Optional fields use `.optional()` modifier
- Test both `parse()` (throws) and `safeParse()` (returns result) methods

## Next Steps

After completion:
→ Proceed to **Task 2.3**: Axios Configuration and Barbershop Service
