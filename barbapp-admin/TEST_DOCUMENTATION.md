# Test Documentation - BarbApp Admin

## 📊 Coverage Status

**Overall Coverage: ~90%** ✅ (Target: >70%)

| Category | Coverage | Status |
|----------|----------|--------|
| Components | 93%+ | ✅ Excellent |
| Hooks | 97%+ | ✅ Excellent |
| Utils | 90%+ | ✅ Excellent |
| Pages | 92%+ | ✅ Excellent |
| Services | 82%+ | ✅ Good |
| **Overall** | **~90%** | ✅ **Exceeds Target** |

## 🧪 Test Statistics

- **Test Files**: 36
- **Total Tests**: 246 passing + 1 skipped
- **Test Types**: Unit, Integration, E2E

## 📁 Test Structure

```
src/
├── __tests__/
│   ├── setup.ts                        # Test environment setup
│   ├── types.test.ts                   # Type definitions tests
│   ├── unit/
│   │   ├── components/                 # Component tests
│   │   │   ├── BarbershopTable.test.tsx
│   │   │   ├── BarbershopForm.test.tsx
│   │   │   ├── BarbershopEditForm.test.tsx
│   │   │   ├── DeactivateModal.test.tsx
│   │   │   ├── EmptyState.test.tsx
│   │   │   ├── Pagination.test.tsx
│   │   │   ├── StatusBadge.test.tsx
│   │   │   ├── MaskedInput.test.tsx
│   │   │   ├── FormField.test.tsx
│   │   │   ├── SelectField.test.tsx
│   │   │   ├── ReadOnlyField.test.tsx
│   │   │   ├── ConfirmDialog.test.tsx
│   │   │   ├── LoadingSpinner.test.tsx
│   │   │   ├── ErrorBoundary.test.tsx
│   │   │   ├── ProtectedRoute.test.tsx
│   │   │   └── BarbershopTableSkeleton.test.tsx
│   │   ├── hooks/                      # Hook tests
│   │   │   ├── useAuth.test.ts
│   │   │   ├── useBarbershops.test.ts
│   │   │   ├── useBarbershop.test.ts
│   │   │   ├── useDebounce.test.ts
│   │   │   └── useViaCep.test.ts
│   │   ├── pages/                      # Page tests
│   │   │   ├── Login.test.tsx
│   │   │   ├── BarbershopList.test.tsx
│   │   │   ├── BarbershopCreate.test.tsx
│   │   │   ├── BarbershopEdit.test.tsx
│   │   │   └── BarbershopDetails.test.tsx
│   │   ├── schemas/                    # Schema validation tests
│   │   │   └── barbershop.schema.test.ts
│   │   └── utils/                      # Utility tests
│   │       ├── errorHandler.test.ts
│   │       ├── formatters.test.ts
│   │       └── toast.test.ts
│   └── integration/
│       └── services/                   # Service integration tests
│           ├── api.interceptors.test.ts
│           └── barbershop.service.test.ts
├── components/
│   └── layout/
│       └── Header.test.tsx             # Layout component test
├── hooks/
│   └── __tests__/
│       └── useViaCep.test.ts           # Additional hook test
└── services/
    └── __tests__/
        └── viacep.service.test.ts      # Service test
```

## 🛠️ Test Tools & Configuration

### Tools Used

- **Vitest** (v1.6.1): Test runner and framework
- **React Testing Library** (v14.2.2): Component testing
- **@testing-library/user-event** (v14.5.2): User interaction simulation
- **MSW** (v2.2.13): API mocking
- **Playwright** (v1.42.1): E2E testing
- **jsdom** (v24.1.3): DOM implementation

### Configuration

Coverage thresholds are set to 70% for:
- Lines
- Functions
- Branches
- Statements

Excluded from coverage:
- Config files
- Type definitions
- Re-export index files
- Router configuration (tested via E2E)
- shadcn/ui utility files
- Test files themselves

## 📝 Test Patterns & Examples

### 1. Component Testing

#### Basic Component Test

```typescript
import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { StatusBadge } from '@/components/barbershop/StatusBadge';

describe('StatusBadge', () => {
  it('should render active status correctly', () => {
    render(<StatusBadge isActive={true} />);
    
    const badge = screen.getByText('Ativo');
    expect(badge).toBeInTheDocument();
    expect(badge).toHaveClass('bg-green-100');
  });
});
```

#### Component with User Interaction

```typescript
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { ConfirmDialog } from '@/components/ui/confirm-dialog';

describe('ConfirmDialog', () => {
  it('should call onConfirm when confirm button is clicked', async () => {
    const user = userEvent.setup();
    const onConfirm = vi.fn();
    
    render(
      <ConfirmDialog
        open={true}
        onClose={() => {}}
        onConfirm={onConfirm}
        title="Confirm Action"
      />
    );
    
    await user.click(screen.getByRole('button', { name: /confirmar/i }));
    expect(onConfirm).toHaveBeenCalledOnce();
  });
});
```

### 2. Hook Testing

#### Custom Hook Test

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import { useDebounce } from '@/hooks/useDebounce';

describe('useDebounce', () => {
  it('should debounce value changes', async () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );
    
    expect(result.current).toBe('initial');
    
    // Update value
    rerender({ value: 'updated', delay: 500 });
    
    // Should still have old value immediately
    expect(result.current).toBe('initial');
    
    // Should update after delay
    await waitFor(() => expect(result.current).toBe('updated'), {
      timeout: 600
    });
  });
});
```

#### Hook with API Calls

```typescript
import { renderHook, waitFor } from '@testing-library/react';
import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { useBarbershops } from '@/hooks/useBarbershops';

const server = setupServer(
  http.get('http://localhost:5000/api/barbearias', () => {
    return HttpResponse.json({
      items: [{ id: '1', name: 'Test Shop' }],
      pageNumber: 1,
      totalPages: 1,
    });
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('useBarbershops', () => {
  it('should fetch barbershops', async () => {
    const { result } = renderHook(() => useBarbershops({
      pageNumber: 1,
      pageSize: 20
    }));
    
    expect(result.current.loading).toBe(true);
    
    await waitFor(() => expect(result.current.loading).toBe(false));
    
    expect(result.current.data?.items).toHaveLength(1);
    expect(result.current.error).toBeNull();
  });
});
```

### 3. Form Testing

#### Form with Validation

```typescript
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';

describe('BarbershopForm', () => {
  it('should display validation errors for invalid input', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(<BarbershopForm onSubmit={onSubmit} />);
    
    // Submit without filling required fields
    await user.click(screen.getByRole('button', { name: /salvar/i }));
    
    // Check for validation errors
    expect(screen.getByText(/nome é obrigatório/i)).toBeInTheDocument();
    expect(screen.getByText(/email é obrigatório/i)).toBeInTheDocument();
    
    // Form should not be submitted
    expect(onSubmit).not.toHaveBeenCalled();
  });
  
  it('should submit form with valid data', async () => {
    const user = userEvent.setup();
    const onSubmit = vi.fn();
    
    render(<BarbershopForm onSubmit={onSubmit} />);
    
    // Fill form
    await user.type(screen.getByLabelText(/nome/i), 'Barbearia Teste');
    await user.type(screen.getByLabelText(/email/i), 'teste@example.com');
    await user.type(screen.getByLabelText(/telefone/i), '11999999999');
    
    // Submit
    await user.click(screen.getByRole('button', { name: /salvar/i }));
    
    // Should submit with formatted data
    expect(onSubmit).toHaveBeenCalledWith({
      name: 'Barbearia Teste',
      email: 'teste@example.com',
      phone: '(11) 99999-9999',
    });
  });
});
```

### 4. Integration Testing

#### Service Integration Test

```typescript
import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { barbershopService } from '@/services/barbershop.service';

const server = setupServer();

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('barbershopService', () => {
  it('should create a new barbershop', async () => {
    server.use(
      http.post('http://localhost:5000/api/barbearias', async ({ request }) => {
        const body = await request.json();
        return HttpResponse.json({
          id: '123',
          ...body,
          createdAt: new Date().toISOString(),
        }, { status: 201 });
      })
    );
    
    const newBarbershop = {
      name: 'Nova Barbearia',
      email: 'nova@example.com',
      phone: '(11) 99999-9999',
      address: {
        street: 'Rua Teste',
        number: '123',
        city: 'São Paulo',
        state: 'SP',
        zipCode: '01000-000',
      },
    };
    
    const result = await barbershopService.create(newBarbershop);
    
    expect(result.id).toBe('123');
    expect(result.name).toBe('Nova Barbearia');
  });
});
```

### 5. Utility Function Testing

#### Formatter Tests

```typescript
import { describe, it, expect } from 'vitest';
import { applyPhoneMask, applyZipCodeMask } from '@/utils/formatters';

describe('formatters', () => {
  describe('applyPhoneMask', () => {
    it('should format complete phone number', () => {
      expect(applyPhoneMask('11999999999')).toBe('(11) 99999-9999');
    });
    
    it('should handle incomplete input', () => {
      expect(applyPhoneMask('119999')).toBe('(11) 9999');
    });
    
    it('should remove non-numeric characters', () => {
      expect(applyPhoneMask('(11) 99999-9999')).toBe('(11) 99999-9999');
    });
  });
});
```

#### Error Handler Tests

```typescript
import { describe, it, expect } from 'vitest';
import { handleApiError } from '@/utils/errorHandler';
import axios from 'axios';

describe('errorHandler', () => {
  it('should extract message from Axios error response', () => {
    const error = {
      isAxiosError: true,
      response: {
        data: { message: 'Error message' }
      }
    };
    
    const message = handleApiError(error);
    expect(message).toBe('Error message');
  });
  
  it('should handle network errors', () => {
    const error = {
      isAxiosError: true,
      request: {},
    };
    
    const message = handleApiError(error);
    expect(message).toContain('Servidor não respondeu');
  });
});
```

## 🏃 Running Tests

### Run All Tests

```bash
npm test
```

### Run Tests with Coverage

```bash
npm run test:coverage
```

### Run Tests in UI Mode

```bash
npm run test:ui
```

### Run Specific Test File

```bash
npm test src/__tests__/unit/components/BarbershopTable.test.tsx
```

### Run Tests in Watch Mode

```bash
npm test -- --watch
```

### Run E2E Tests

```bash
npm run test:e2e
```

## 📈 Coverage Reports

After running `npm run test:coverage`, coverage reports are generated in:
- **Text**: Console output
- **HTML**: `coverage/index.html` (open in browser for detailed view)
- **JSON**: `coverage/coverage-final.json`

## ✅ Testing Checklist

### Component Tests
- [x] Render without crashing
- [x] Display correct data from props
- [x] Handle user interactions (clicks, typing, etc.)
- [x] Show/hide elements based on state
- [x] Display loading states
- [x] Display error states
- [x] Call callbacks with correct arguments
- [x] Validate form inputs
- [x] Apply correct CSS classes

### Hook Tests
- [x] Return correct initial values
- [x] Update state correctly
- [x] Handle async operations
- [x] Clean up side effects
- [x] Handle errors gracefully
- [x] Debounce/throttle correctly (if applicable)

### Utility Tests
- [x] Transform data correctly
- [x] Handle edge cases
- [x] Return correct types
- [x] Handle null/undefined inputs
- [x] Format strings correctly

### Integration Tests
- [x] API calls with correct parameters
- [x] Handle successful responses
- [x] Handle error responses
- [x] Retry logic (if applicable)
- [x] Authentication/authorization

## 🎯 Best Practices

1. **Arrange-Act-Assert (AAA) Pattern**: Structure tests clearly
2. **Test User Behavior**: Test what users see and do, not implementation
3. **Mock External Dependencies**: Use MSW for API calls
4. **Avoid Testing Implementation Details**: Focus on behavior
5. **Use Descriptive Test Names**: Make failures easy to understand
6. **Keep Tests Independent**: No shared state between tests
7. **Test Edge Cases**: Empty states, errors, loading states
8. **Use TypeScript**: Catch type errors in tests
9. **Clean Up**: Always cleanup after tests (MSW, timers, etc.)
10. **Coverage is Not Everything**: Aim for meaningful tests, not just high numbers

## 🐛 Common Issues & Solutions

### Issue: Tests timeout

**Solution**: Increase timeout or check for missing `await` on async operations

```typescript
it('should load data', async () => {
  // Add explicit timeout
  await waitFor(() => expect(result).toBeDefined(), { timeout: 5000 });
}, 10000); // Test timeout
```

### Issue: MSW handlers not working

**Solution**: Ensure server is properly setup and handlers match URLs exactly

```typescript
beforeAll(() => server.listen({ onUnhandledRequest: 'error' }));
afterEach(() => server.resetHandlers());
afterAll(() => server.close());
```

### Issue: Form validation not triggering

**Solution**: Use `userEvent` instead of `fireEvent` for realistic interactions

```typescript
const user = userEvent.setup();
await user.type(input, 'value');
await user.click(button);
```

### Issue: React Router errors in tests

**Solution**: Wrap components with MemoryRouter

```typescript
import { MemoryRouter } from 'react-router-dom';

render(
  <MemoryRouter>
    <Component />
  </MemoryRouter>
);
```

## 📚 Additional Resources

- [Vitest Documentation](https://vitest.dev/)
- [React Testing Library](https://testing-library.com/react)
- [MSW Documentation](https://mswjs.io/)
- [Playwright Documentation](https://playwright.dev/)
- [Testing Best Practices](https://kentcdodds.com/blog/common-mistakes-with-react-testing-library)

---

**Last Updated**: 2025-10-14
**Coverage Target**: >70% ✅
**Current Coverage**: ~90% ✅
