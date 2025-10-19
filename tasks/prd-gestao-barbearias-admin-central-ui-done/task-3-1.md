# Task 3.1: Login Page Implementation

**Status**: ✅ CONCLUÍDA
**Priority**: Crítica
**Estimated Effort**: 1 day
**Phase**: Phase 3 - Authentication

## Description

Implement complete login page with form validation, authentication flow, error handling, and redirect to barbershops list on success. This is the entry point for all Admin Central users.

## Acceptance Criteria

- [x] Login form with email and password fields
- [x] Form validation with Zod schema and react-hook-form
- [x] Show/hide password toggle
- [x] Loading state during authentication
- [x] Clear error messages for invalid credentials
- [x] Success: Store token and redirect to `/barbearias`
- [x] Failure: Display error without clearing form
- [x] Responsive design matching design reference
- [x] Accessibility: keyboard navigation, ARIA labels

## Dependencies

**Blocking Tasks**:
- Task 2.2 (Zod Schemas) - uses loginSchema
- Task 2.3 (API Services) - uses auth endpoint

**Blocked Tasks**:
- Task 3.2 (Auth Hooks) - uses login page for testing
- Task 4.1 (Routing) - includes login route

## Implementation Notes

### Component Structure

Based on Tech Spec section 5.1:

**src/pages/Login/Login.tsx**

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, LoginFormData } from '@/schemas/barbershop.schema';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import api from '@/services/api';

export function Login() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      const response = await api.post('/auth/admin-central', data);
      localStorage.setItem('auth_token', response.data.token);

      toast({
        title: 'Login realizado com sucesso!',
        description: `Bem-vindo, ${response.data.user.name}`,
      });

      navigate('/barbearias');
    } catch (error) {
      toast({
        title: 'Erro ao fazer login',
        description: 'Verifique suas credenciais e tente novamente.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
        <div className="text-center">
          <h2 className="text-3xl font-bold text-gray-900">BarbApp Admin</h2>
          <p className="mt-2 text-sm text-gray-600">
            Faça login para gerenciar barbearias
          </p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Email Field */}
          <div>
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              autoComplete="email"
              {...register('email')}
              className={errors.email ? 'border-red-500' : ''}
              aria-invalid={errors.email ? 'true' : 'false'}
              aria-describedby={errors.email ? 'email-error' : undefined}
            />
            {errors.email && (
              <p id="email-error" className="mt-1 text-sm text-red-500">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Password Field */}
          <div>
            <Label htmlFor="password">Senha</Label>
            <div className="relative">
              <Input
                id="password"
                type={showPassword ? 'text' : 'password'}
                autoComplete="current-password"
                {...register('password')}
                className={errors.password ? 'border-red-500' : ''}
                aria-invalid={errors.password ? 'true' : 'false'}
                aria-describedby={errors.password ? 'password-error' : undefined}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-500 hover:text-gray-700"
                aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
              >
                {showPassword ? '🙈' : '👁️'}
              </button>
            </div>
            {errors.password && (
              <p id="password-error" className="mt-1 text-sm text-red-500">
                {errors.password.message}
              </p>
            )}
          </div>

          {/* Submit Button */}
          <Button
            type="submit"
            className="w-full"
            disabled={isLoading}
            aria-busy={isLoading}
          >
            {isLoading ? (
              <>
                <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Entrando...
              </>
            ) : (
              'Entrar'
            )}
          </Button>
        </form>
      </div>
    </div>
  );
}
```

### Barrel Export

**src/pages/Login/index.ts**:
```typescript
export { Login } from './Login';
```

## Testing Requirements

### Unit Tests

**src/__tests__/unit/pages/Login.test.tsx**:

```typescript
import { render, screen, waitFor } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { BrowserRouter } from 'react-router-dom';
import { Login } from '@/pages/Login';
import api from '@/services/api';

vi.mock('@/services/api');

const mockNavigate = vi.fn();
vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual('react-router-dom');
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  };
});

describe('Login Page', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('should render login form', () => {
    render(
      <BrowserRouter>
        <Login />
      </BrowserRouter>
    );

    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/senha/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument();
  });

  it('should show validation errors for empty fields', async () => {
    const user = userEvent.setup();

    render(
      <BrowserRouter>
        <Login />
      </BrowserRouter>
    );

    const submitButton = screen.getByRole('button', { name: /entrar/i });
    await user.click(submitButton);

    await waitFor(() => {
      expect(screen.getByText(/email inválido/i)).toBeInTheDocument();
      expect(screen.getByText(/senha deve ter no mínimo 6 caracteres/i)).toBeInTheDocument();
    });
  });

  it('should call API and navigate on successful login', async () => {
    const user = userEvent.setup();
    vi.mocked(api.post).mockResolvedValueOnce({
      data: {
        token: 'fake-token',
        user: { id: '1', name: 'Admin', email: 'admin@test.com' },
      },
    });

    render(
      <BrowserRouter>
        <Login />
      </BrowserRouter>
    );

    await user.type(screen.getByLabelText(/email/i), 'admin@test.com');
    await user.type(screen.getByLabelText(/senha/i), 'password123');
    await user.click(screen.getByRole('button', { name: /entrar/i }));

    await waitFor(() => {
      expect(api.post).toHaveBeenCalledWith('/auth/admin-central', {
        email: 'admin@test.com',
        password: 'password123',
      });
      expect(mockNavigate).toHaveBeenCalledWith('/barbearias');
    });
  });

  it('should show error message on failed login', async () => {
    const user = userEvent.setup();
    vi.mocked(api.post).mockRejectedValueOnce(new Error('Invalid credentials'));

    render(
      <BrowserRouter>
        <Login />
      </BrowserRouter>
    );

    await user.type(screen.getByLabelText(/email/i), 'wrong@test.com');
    await user.type(screen.getByLabelText(/senha/i), 'wrongpass');
    await user.click(screen.getByRole('button', { name: /entrar/i }));

    await waitFor(() => {
      expect(screen.getByText(/erro ao fazer login/i)).toBeInTheDocument();
    });
  });

  it('should toggle password visibility', async () => {
    const user = userEvent.setup();

    render(
      <BrowserRouter>
        <Login />
      </BrowserRouter>
    );

    const passwordInput = screen.getByLabelText(/senha/i);
    const toggleButton = screen.getByLabelText(/mostrar senha/i);

    expect(passwordInput).toHaveAttribute('type', 'password');

    await user.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'text');

    await user.click(toggleButton);
    expect(passwordInput).toHaveAttribute('type', 'password');
  });
});
```

## Files to Create/Modify

**Create**:
- `src/pages/Login/Login.tsx`
- `src/pages/Login/index.ts`
- `src/__tests__/unit/pages/Login.test.tsx`

## Verification Checklist

1. ✅ Login form renders with email and password fields
2. ✅ Form validation works (empty fields, invalid email, short password)
3. ✅ Show/hide password toggle works
4. ✅ Loading state shows during authentication
5. ✅ Success: Token stored in localStorage and redirects to `/barbearias`
6. ✅ Failure: Error toast displayed, form not cleared
7. ✅ Responsive design works on mobile and desktop
8. ✅ Keyboard navigation works (Tab, Enter to submit)
9. ✅ ARIA labels present for accessibility
10. ✅ Unit tests pass

## Completion Summary

- [x] 3.1.0 [Login Page Implementation] ✅ CONCLUÍDA
  - [x] 3.1.1 Componente Login implementado com validação Zod
  - [x] 3.1.2 Integração com API de autenticação
  - [x] 3.1.3 Estados de loading e error implementados
  - [x] 3.1.4 Toggle de visibilidade da senha
  - [x] 3.1.5 Testes unitários completos
  - [x] 3.1.6 Acessibilidade e design responsivo
  - [x] 3.1.7 Conformidade com regras do projeto

## Reference

- **Tech Spec Section**: 5.1 (Login Page Implementation)
- **PRD**: Section 9 (Login Functionality)
- **Design Reference**: `tela-login.html`

## Next Steps

After completion:
→ Proceed to **Task 3.2**: Auth Hooks and Protected Routes
