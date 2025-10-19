# Task 2.1: TypeScript Types and Interfaces

**Status**: ✅ Completed
**Priority**: Crítica
**Estimated Effort**: 0.5 day
**Phase**: Phase 2 - Type Safety and API

## Description

Define all TypeScript types and interfaces for the application domain, including Barbershop entities, API request/response types, authentication types, and pagination utilities.

These types provide compile-time safety and IDE autocomplete for the entire application.

## Acceptance Criteria

- [x] Barbershop interface with all fields defined
- [x] Address interface with complete address structure
- [x] Auth types (LoginRequest, LoginResponse, User)
- [x] API response wrapper types (ApiResponse, PaginatedResponse)
- [x] Filter and query parameter types
- [x] All types exported via barrel export
- [x] No TypeScript errors in type definitions
- [x] JSDoc comments for complex types

## Dependencies

**Blocking Tasks**:
- Task 1.2 (Folder Structure) must be completed

**Blocked Tasks**:
- Task 2.2 (Zod Schemas) - uses these types for validation
- Task 2.3 (API Services) - uses these types for requests/responses
- All page implementations depend on these types

## Implementation Notes

### Files to Create

Based on Tech Spec section 3.3:

1. **src/types/barbershop.ts**
2. **src/types/auth.ts**
3. **src/types/pagination.ts**
4. **src/types/index.ts** (barrel export)

### Type Definitions

#### Barbershop Types (src/types/barbershop.ts)

```typescript
/**
 * Barbershop entity representing a registered barbershop in the system
 */
export interface Barbershop {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: Address;
  isActive: boolean;
  createdAt: string; // ISO 8601 date string
  updatedAt: string; // ISO 8601 date string
}

/**
 * Address information for barbershops
 */
export interface Address {
  street: string;
  number: string;
  complement?: string; // Optional field
  neighborhood: string;
  city: string;
  state: string; // 2-letter state code (e.g., "SP")
  zipCode: string; // Format: "99999-999"
}

/**
 * Request payload for creating a new barbershop
 */
export interface CreateBarbershopRequest {
  name: string;
  email: string;
  phone: string;
  address: Address;
}

/**
 * Request payload for updating an existing barbershop
 * Same as CreateBarbershopRequest (all fields editable except id)
 */
export interface UpdateBarbershopRequest extends CreateBarbershopRequest {}

/**
 * Query parameters for filtering barbershops
 */
export interface BarbershopFilters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string; // Search by name, email, or city
  isActive?: boolean; // Filter by status
}
```

#### Auth Types (src/types/auth.ts)

```typescript
/**
 * Login credentials for admin authentication
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Authentication response containing token and user info
 */
export interface LoginResponse {
  token: string;
  user: User;
}

/**
 * Authenticated user information
 */
export interface User {
  id: string;
  email: string;
  name: string;
}
```

#### Pagination Types (src/types/pagination.ts)

```typescript
/**
 * Generic paginated response wrapper from API
 * @template T - The type of items in the paginated list
 */
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

/**
 * Generic API response wrapper (for non-paginated responses)
 * @template T - The type of data in the response
 */
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message?: string;
  errors?: string[];
}
```

#### Barrel Export (src/types/index.ts)

```typescript
// Barbershop types
export type {
  Barbershop,
  Address,
  CreateBarbershopRequest,
  UpdateBarbershopRequest,
  BarbershopFilters,
} from './barbershop';

// Auth types
export type {
  LoginRequest,
  LoginResponse,
  User,
} from './auth';

// Pagination types
export type {
  PaginatedResponse,
  ApiResponse,
} from './pagination';
```

## Testing Requirements

- [ ] TypeScript compilation succeeds with all type definitions
- [ ] Import types from `@/types` works in a test file
- [ ] No circular dependency errors
- [ ] VSCode autocomplete shows type properties correctly
- [ ] Types align with backend API contract (verify with team)

**Test File** (`src/__tests__/types.test.ts`):
```typescript
import { describe, it, expect } from 'vitest';
import type { Barbershop, CreateBarbershopRequest, PaginatedResponse } from '@/types';

describe('Type Definitions', () => {
  it('should compile barbershop types correctly', () => {
    const barbershop: Barbershop = {
      id: '1',
      name: 'Test Barber',
      email: 'test@barber.com',
      phone: '(11) 99999-9999',
      address: {
        street: 'Main St',
        number: '123',
        neighborhood: 'Downtown',
        city: 'São Paulo',
        state: 'SP',
        zipCode: '01000-000',
      },
      isActive: true,
      createdAt: '2024-01-01T00:00:00Z',
      updatedAt: '2024-01-01T00:00:00Z',
    };

    expect(barbershop).toBeDefined();
    expect(barbershop.id).toBe('1');
  });

  it('should compile paginated response types correctly', () => {
    const response: PaginatedResponse<Barbershop> = {
      items: [],
      pageNumber: 1,
      pageSize: 20,
      totalPages: 1,
      totalCount: 0,
      hasPreviousPage: false,
      hasNextPage: false,
    };

    expect(response).toBeDefined();
  });
});
```

## Files to Create/Modify

**Create**:
- `src/types/barbershop.ts`
- `src/types/auth.ts`
- `src/types/pagination.ts`
- `src/types/index.ts`
- `src/__tests__/types.test.ts` (unit test)

## Verification Checklist

Before marking as complete:

1. ✅ All type files created in `src/types/`
2. ✅ Barrel export (`index.ts`) exports all types
3. ✅ TypeScript compilation passes (`npm run build`)
4. ✅ Can import types using `@/types` alias
5. ✅ VSCode shows autocomplete for type properties
6. ✅ JSDoc comments added for complex types
7. ✅ Types align with backend API contract (confirmed with team)
8. ✅ Unit test for type definitions passes
9. ✅ No circular dependencies detected
10. ✅ Git commit created for type definitions

## Reference

- **Tech Spec Section**: 3.3 (TypeScript Types)
- **PRD**: All sections (foundation for type safety)
- **Backend Contract**: `/src/BarbApp.API/Controllers/BarbershopsController.cs`

## Notes

- These types must match the backend API contract exactly
- Use `string` for dates (ISO 8601 format) - convert to Date objects in components if needed
- Keep types simple - complex logic belongs in services/utils
- Optional fields use `?` syntax (e.g., `complement?: string`)
- Use interfaces for extensibility, type aliases for unions/intersections

## API Contract Verification

Before marking complete, verify these types match backend responses:

```bash
# Test against running backend
curl http://localhost:5000/api/barbearias | jq

# Compare response structure with Barbershop interface
# Verify field names, types, and optional fields match
```

## Next Steps

After completion:
→ Proceed to **Task 2.2**: Zod Validation Schemas
