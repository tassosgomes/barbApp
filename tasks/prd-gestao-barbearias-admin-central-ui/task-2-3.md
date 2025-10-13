# Task 2.3: Axios Configuration and Barbershop Service

**Status**: 🔵 Not Started
**Priority**: Crítica
**Estimated Effort**: 1 day
**Phase**: Phase 2 - Type Safety and API

## Description

Configure Axios HTTP client with base URL, interceptors for authentication and error handling, and implement the complete barbershop service with all CRUD methods aligned with the backend API contract.

This establishes the communication layer between frontend and backend.

## Acceptance Criteria

- [ ] Axios instance configured with base URL from environment variables
- [ ] Request interceptor adds JWT token to all requests
- [ ] Response interceptor handles 401 errors (redirect to login)
- [ ] Barbershop service with all CRUD methods implemented
- [ ] Service methods typed with request/response interfaces
- [ ] Error handling utility for user-friendly messages
- [ ] Integration tests with MSW mocks pass
- [ ] Service methods tested against running backend (manual verification)

## Dependencies

**Blocking Tasks**:
- Task 2.1 (TypeScript Types) must be completed
- Task 2.2 (Zod Schemas) should be completed (for validation reference)

**Blocked Tasks**:
- Task 3.1 (Login Page) - uses auth endpoint
- Task 6.1 (List Page) - uses getAll method
- Task 7.1 (Create Page) - uses create method
- Task 8.1 (Edit Page) - uses update method
- Task 9.1 (Details Page) - uses getById method
- Task 10.1 (Deactivate/Reactivate) - uses deactivate/reactivate methods

## Implementation Notes

### Files to Create

Based on Tech Spec sections 3.1 and 3.2:

1. **src/services/api.ts** - Axios configuration
2. **src/services/barbershop.service.ts** - Barbershop CRUD service
3. **src/utils/errorHandler.ts** - Error handling utility
4. **src/services/index.ts** - Barrel export

### Axios Configuration (src/services/api.ts)

```typescript
import axios from 'axios';

/**
 * Axios instance configured with base URL and interceptors
 */
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

/**
 * Request interceptor: Add JWT token to all requests
 */
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('auth_token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

/**
 * Response interceptor: Handle global errors
 * - 401: Clear token and redirect to login
 * - Other errors: Pass through for component-level handling
 */
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Unauthorized - clear token and redirect
      localStorage.removeItem('auth_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

### Barbershop Service (src/services/barbershop.service.ts)

Complete implementation from Tech Spec section 3.2:

```typescript
import api from './api';
import type {
  Barbershop,
  CreateBarbershopRequest,
  UpdateBarbershopRequest,
  PaginatedResponse,
  BarbershopFilters,
} from '@/types';

export const barbershopService = {
  /**
   * List barbershops with pagination and filters
   * @param filters - Query parameters for filtering and pagination
   * @returns Paginated list of barbershops
   */
  getAll: async (filters: BarbershopFilters): Promise<PaginatedResponse<Barbershop>> => {
    const { data } = await api.get<PaginatedResponse<Barbershop>>('/barbearias', {
      params: filters,
    });
    return data;
  },

  /**
   * Get barbershop by ID
   * @param id - Barbershop unique identifier
   * @returns Barbershop details
   */
  getById: async (id: string): Promise<Barbershop> => {
    const { data } = await api.get<Barbershop>(`/barbearias/${id}`);
    return data;
  },

  /**
   * Create new barbershop
   * @param request - Barbershop creation data
   * @returns Created barbershop with generated ID
   */
  create: async (request: CreateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.post<Barbershop>('/barbearias', request);
    return data;
  },

  /**
   * Update existing barbershop
   * @param id - Barbershop unique identifier
   * @param request - Updated barbershop data
   * @returns Updated barbershop
   */
  update: async (id: string, request: UpdateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.put<Barbershop>(`/barbearias/${id}`, request);
    return data;
  },

  /**
   * Deactivate barbershop (soft delete)
   * @param id - Barbershop unique identifier
   */
  deactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/desativar`);
  },

  /**
   * Reactivate barbershop
   * @param id - Barbershop unique identifier
   */
  reactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/reativar`);
  },
};
```

### Error Handler Utility (src/utils/errorHandler.ts)

From Tech Spec section 7.1:

```typescript
import axios from 'axios';

/**
 * Extract user-friendly error message from API error
 * @param error - Error object from API call
 * @returns User-friendly error message in Portuguese
 */
export function handleApiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.response) {
      // Backend responded with error
      const message = error.response.data?.message || error.response.data?.title;
      return message || 'Erro ao processar requisição';
    } else if (error.request) {
      // Request made but no response
      return 'Servidor não respondeu. Verifique sua conexão.';
    }
  }
  return 'Erro inesperado. Tente novamente.';
}
```

### Barrel Export (src/services/index.ts)

```typescript
export { default as api } from './api';
export { barbershopService } from './barbershop.service';
```

### Barrel Export for Utils (src/utils/index.ts)

```typescript
export { handleApiError } from './errorHandler';
```

## Testing Requirements

### Integration Tests with MSW

**Test File** (`src/__tests__/integration/services/barbershop.service.test.ts`):

```typescript
import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { barbershopService } from '@/services/barbershop.service';

const server = setupServer(
  http.get('http://localhost:5000/api/barbearias', () => {
    return HttpResponse.json({
      items: [
        {
          id: '1',
          name: 'Barbearia Mock',
          email: 'mock@email.com',
          phone: '(11) 99999-9999',
          address: {
            street: 'Rua Mock',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('barbershopService', () => {
  it('should fetch barbershops with pagination', async () => {
    const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

    expect(result.items).toHaveLength(1);
    expect(result.items[0].name).toBe('Barbearia Mock');
    expect(result.pageNumber).toBe(1);
    expect(result.totalCount).toBe(1);
  });

  it('should fetch barbershop by id', async () => {
    server.use(
      http.get('http://localhost:5000/api/barbearias/:id', () => {
        return HttpResponse.json({
          id: '1',
          name: 'Barbearia Específica',
          email: 'especifica@email.com',
          phone: '(11) 98888-7777',
          address: {
            street: 'Rua Específica',
            number: '456',
            neighborhood: 'Vila',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '02000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        });
      })
    );

    const result = await barbershopService.getById('1');

    expect(result.name).toBe('Barbearia Específica');
    expect(result.email).toBe('especifica@email.com');
  });

  it('should create new barbershop', async () => {
    server.use(
      http.post('http://localhost:5000/api/barbearias', async ({ request }) => {
        const body = await request.json();
        return HttpResponse.json({
          id: 'new-id',
          ...(body as any),
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        });
      })
    );

    const result = await barbershopService.create({
      name: 'Nova Barbearia',
      email: 'nova@email.com',
      phone: '(11) 99999-9999',
      address: {
        street: 'Rua Nova',
        number: '100',
        neighborhood: 'Novo Bairro',
        city: 'São Paulo',
        state: 'SP',
        zipCode: '01000-000',
      },
    });

    expect(result.id).toBe('new-id');
    expect(result.name).toBe('Nova Barbearia');
  });

  it('should handle error responses', async () => {
    server.use(
      http.get('http://localhost:5000/api/barbearias/:id', () => {
        return HttpResponse.json(
          { message: 'Barbearia não encontrada' },
          { status: 404 }
        );
      })
    );

    await expect(barbershopService.getById('invalid-id')).rejects.toThrow();
  });
});
```

### Manual Backend Verification

Before marking complete, verify against running backend:

```bash
# 1. Start backend server
cd /src
dotnet run --project BarbApp.API

# 2. Test endpoints with curl
curl -X GET http://localhost:5000/api/barbearias
curl -X GET http://localhost:5000/api/barbearias/{id}
curl -X POST http://localhost:5000/api/barbearias -H "Content-Type: application/json" -d '{...}'

# 3. Verify response structures match TypeScript types
# 4. Document any discrepancies for backend team
```

## Files to Create/Modify

**Create**:
- `src/services/api.ts`
- `src/services/barbershop.service.ts`
- `src/services/index.ts`
- `src/utils/errorHandler.ts`
- `src/utils/index.ts`
- `src/__tests__/integration/services/barbershop.service.test.ts`

## Verification Checklist

Before marking as complete:

1. ✅ Axios instance configured with base URL from .env
2. ✅ Request interceptor adds token to headers
3. ✅ Response interceptor handles 401 errors
4. ✅ All barbershop service methods implemented
5. ✅ Service methods properly typed
6. ✅ Error handler returns user-friendly messages
7. ✅ Integration tests pass with MSW mocks
8. ✅ Manual verification against backend API successful
9. ✅ API contract discrepancies documented (if any)
10. ✅ Git commit created for API services

## Reference

- **Tech Spec Section**: 3.1 (Axios Config), 3.2 (Service), 7.1 (Error Handler)
- **PRD**: All CRUD sections (foundation for data operations)
- **Backend API**: `/src/BarbApp.API/Controllers/BarbershopsController.cs`

## Notes

- Base URL must be configurable via environment variable
- Token storage in localStorage is MVP approach (consider httpOnly cookies post-MVP)
- 401 interceptor provides global session management
- Service methods are async and return promises
- Error handling at service level is minimal - let components handle display
- Use `params` option for query parameters, not manual URL construction

## API Contract Checklist

Verify these endpoints match backend:

- [ ] `GET /api/barbearias` - Returns PaginatedResponse<Barbershop>
- [ ] `GET /api/barbearias/{id}` - Returns Barbershop
- [ ] `POST /api/barbearias` - Accepts CreateBarbershopRequest, returns Barbershop
- [ ] `PUT /api/barbearias/{id}` - Accepts UpdateBarbershopRequest, returns Barbershop
- [ ] `PUT /api/barbearias/{id}/desativar` - Returns void
- [ ] `PUT /api/barbearias/{id}/reativar` - Returns void

## Next Steps

After completion:
→ Proceed to **Task 3.1**: Login Page Implementation
