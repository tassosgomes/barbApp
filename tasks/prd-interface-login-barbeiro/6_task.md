---
status: pending
parallelizable: true
blocked_by: ["4.0","5.0"]
---

<task_context>
<domain>engine/frontend/testing</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies></dependencies>
<unblocks>"7.0"</unblocks>
</task_context>

# Tarefa 6.0: Testes - Unitários, Integração e E2E

## Visão Geral
Implementar suite completa de testes para o fluxo de autenticação: testes unitários de funções e componentes, testes de integração do fluxo completo, e testes E2E com Playwright.

## Requisitos
- Testes unitários de utilitários (phone-utils)
- Testes unitários de componentes (LoginForm)
- Testes de integração do fluxo de login
- Testes E2E com Playwright
- Cobertura de casos de sucesso e erro
- Mocks de API e navegação

## Subtarefas
- [ ] 6.1 Testes unitários de `phone-utils.ts`:
  - Testar applyPhoneMask com diferentes inputs
  - Testar formatPhoneToAPI
- [ ] 6.2 Testes unitários de `LoginForm`:
  - Renderização correta
  - Validação de campos
  - Aplicação de máscara
  - Estados de loading
- [ ] 6.3 Testes de integração de `useAuth` hook:
  - Login bem-sucedido
  - Validação de sessão
  - Logout
- [ ] 6.4 Testes de integração do fluxo completo:
  - Login → Armazenar token → Navegar
  - Sessão persistente ao recarregar
  - Token expirado → Redirect login
- [ ] 6.5 Testes E2E com Playwright:
  - Fluxo completo de login
  - Erros de validação
  - Erros de API
  - Sessão persistente

## Sequenciamento
- Bloqueado por: 4.0 (Componentes), 5.0 (Rotas)
- Desbloqueia: 7.0
- Paralelizável: Sim (pode começar incrementalmente)

## Detalhes de Implementação

**Testes de phone-utils:**
```typescript
// src/lib/__tests__/phone-utils.test.ts
import { describe, it, expect } from 'vitest';
import { applyPhoneMask, formatPhoneToAPI } from '../phone-utils';

describe('phone-utils', () => {
  describe('applyPhoneMask', () => {
    it('deve aplicar máscara para 11 dígitos', () => {
      expect(applyPhoneMask('11999999999')).toBe('(11) 99999-9999');
    });
    
    it('deve aplicar máscara parcial durante digitação', () => {
      expect(applyPhoneMask('11')).toBe('11');
      expect(applyPhoneMask('119')).toBe('(11) 9');
      expect(applyPhoneMask('1199999')).toBe('(11) 99999');
    });
    
    it('deve remover caracteres não numéricos', () => {
      expect(applyPhoneMask('(11) 99999-9999')).toBe('(11) 99999-9999');
    });
    
    it('deve limitar a 11 dígitos', () => {
      expect(applyPhoneMask('119999999999')).toBe('(11) 99999-9999');
    });
  });
  
  describe('formatPhoneToAPI', () => {
    it('deve converter para formato internacional', () => {
      expect(formatPhoneToAPI('(11) 99999-9999')).toBe('+5511999999999');
    });
    
    it('deve remover todos os caracteres especiais', () => {
      expect(formatPhoneToAPI('11-99999-9999')).toBe('+5511999999999');
    });
  });
});
```

**Testes de LoginForm:**
```typescript
// src/components/auth/__tests__/LoginForm.test.tsx
import { describe, it, expect, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { LoginForm } from '../LoginForm';
import { useAuth } from '@/contexts/AuthContext';

vi.mock('@/contexts/AuthContext');

describe('LoginForm', () => {
  const mockLogin = vi.fn();
  
  beforeEach(() => {
    vi.mocked(useAuth).mockReturnValue({
      login: mockLogin,
      user: null,
      isAuthenticated: false,
      isLoading: false,
      logout: vi.fn(),
      validateSession: vi.fn()
    });
  });
  
  it('deve renderizar campos e botão', () => {
    render(<LoginForm />);
    
    expect(screen.getByLabelText(/código da barbearia/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/telefone/i)).toBeInTheDocument();
    expect(screen.getByRole('button', { name: /entrar/i })).toBeInTheDocument();
  });
  
  it('deve aplicar máscara no campo telefone', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);
    
    const phoneInput = screen.getByLabelText(/telefone/i);
    await user.type(phoneInput, '11999999999');
    
    expect(phoneInput).toHaveValue('(11) 99999-9999');
  });
  
  it('deve mostrar erros de validação', async () => {
    const user = userEvent.setup();
    render(<LoginForm />);
    
    const submitButton = screen.getByRole('button', { name: /entrar/i });
    await user.click(submitButton);
    
    expect(await screen.findByText(/código da barbearia é obrigatório/i)).toBeInTheDocument();
    expect(await screen.findByText(/telefone é obrigatório/i)).toBeInTheDocument();
  });
  
  it('deve chamar login com dados corretos', async () => {
    const user = userEvent.setup();
    mockLogin.mockResolvedValue(undefined);
    
    render(<LoginForm />);
    
    await user.type(screen.getByLabelText(/código/i), 'BARB001');
    await user.type(screen.getByLabelText(/telefone/i), '11999999999');
    await user.click(screen.getByRole('button', { name: /entrar/i }));
    
    await waitFor(() => {
      expect(mockLogin).toHaveBeenCalledWith({
        barbershopCode: 'BARB001',
        phone: '(11) 99999-9999'
      });
    });
  });
  
  it('deve desabilitar campos durante submissão', async () => {
    const user = userEvent.setup();
    mockLogin.mockImplementation(() => new Promise(() => {})); // Never resolves
    
    render(<LoginForm />);
    
    await user.type(screen.getByLabelText(/código/i), 'BARB001');
    await user.type(screen.getByLabelText(/telefone/i), '11999999999');
    await user.click(screen.getByRole('button', { name: /entrar/i }));
    
    expect(screen.getByLabelText(/código/i)).toBeDisabled();
    expect(screen.getByLabelText(/telefone/i)).toBeDisabled();
    expect(screen.getByRole('button')).toBeDisabled();
  });
});
```

**Testes E2E:**
```typescript
// tests/e2e/auth.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Autenticação Barbeiro', () => {
  test('deve fazer login com sucesso', async ({ page }) => {
    await page.goto('/login');
    
    // Preencher formulário
    await page.fill('[id="barbershopCode"]', 'BARB001');
    await page.fill('[id="phone"]', '11999999999');
    
    // Verificar máscara aplicada
    await expect(page.locator('[id="phone"]')).toHaveValue('(11) 99999-9999');
    
    // Submeter
    await page.click('button[type="submit"]');
    
    // Aguardar navegação
    await page.waitForURL('/barber/schedule');
    
    // Verificar que está na página de agenda
    expect(page.url()).toContain('/barber/schedule');
  });
  
  test('deve mostrar erro para credenciais inválidas', async ({ page }) => {
    await page.goto('/login');
    
    await page.fill('[id="barbershopCode"]', 'INVALID');
    await page.fill('[id="phone"]', '11999999999');
    await page.click('button[type="submit"]');
    
    // Verificar toast de erro
    await expect(page.getByText(/código ou telefone inválidos/i)).toBeVisible();
  });
  
  test('deve persistir sessão após reload', async ({ page }) => {
    // Fazer login
    await page.goto('/login');
    await page.fill('[id="barbershopCode"]', 'BARB001');
    await page.fill('[id="phone"]', '11999999999');
    await page.click('button[type="submit"]');
    await page.waitForURL('/barber/schedule');
    
    // Recarregar página
    await page.reload();
    
    // Verificar que continua autenticado
    expect(page.url()).toContain('/barber/schedule');
  });
  
  test('deve fazer logout', async ({ page }) => {
    // Login
    await page.goto('/login');
    await page.fill('[id="barbershopCode"]', 'BARB001');
    await page.fill('[id="phone"]', '11999999999');
    await page.click('button[type="submit"]');
    await page.waitForURL('/barber/schedule');
    
    // Logout
    await page.click('text=Sair');
    await page.click('text=Confirmar'); // Se houver confirmação
    
    // Verificar redirect para login
    await page.waitForURL('/login');
    expect(page.url()).toContain('/login');
  });
});
```

## Critérios de Sucesso
- Todos os testes unitários passam
- Testes de integração cobrem fluxos principais
- Testes E2E validam experiência completa
- Cobertura de código > 80%
- Casos de erro são testados
- Mocks são limpos entre testes
- Testes são estáveis (não flaky)
- Segue `rules/tests-react.md`
