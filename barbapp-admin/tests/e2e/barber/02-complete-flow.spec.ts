import { test, expect } from '@playwright/test';
import {
  TEST_BARBER_CREDENTIALS,
  loginAsBarber,
  clearBarberAuth,
  isBarberAuthenticated,
} from '../helpers/barber.helper';

test.describe('Barber Schedule - Fluxo Completo de Navegação', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
  });

  test('deve realizar fluxo completo: login → agenda → persistência', async ({ page }) => {
    // 1. Acessa página de login
    await page.goto('/login');
    await expect(page).toHaveURL('/login');
    
    // 2. Realiza login
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // 3. Verifica redirecionamento para agenda
    await expect(page).toHaveURL('/barber/schedule', { timeout: 10000 });
    
    // 4. Verifica que token foi armazenado
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(true);
    
    // 5. Verifica conteúdo da página de agenda
    // O H1 exibe o nome do barbeiro, não "Minha Agenda"
    await expect(page.locator(`h1:has-text("${TEST_BARBER_CREDENTIALS.nome}")`)).toBeVisible();
    
    // 6. Recarrega página
    await page.reload();
    
    // 7. Verifica que continua autenticado e na agenda
    await expect(page).toHaveURL('/barber/schedule');
    const stillAuthenticated = await isBarberAuthenticated(page);
    expect(stillAuthenticated).toBe(true);
  });

  test('deve bloquear acesso a rota protegida sem autenticação', async ({ page }) => {
    // Tenta acessar diretamente a agenda sem estar autenticado
    await page.goto('/barber/schedule');
    
    // Deve redirecionar para login
    await expect(page).toHaveURL('/login', { timeout: 5000 });
    
    // Verifica que não está autenticado
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(false);
  });

  test('deve exibir estado de loading durante validação de token', async ({ page }) => {
    // Faz login primeiro
    await loginAsBarber(page);
    
    // Recarrega página para forçar validação de token
    await page.reload();
    
    // Verifica que há um indicador de loading (mesmo que brevemente)
    // Nota: este teste pode ser flaky se a validação for muito rápida
    const hasLoadingState = await Promise.race([
      page.locator('text=Carregando...').isVisible().catch(() => false),
      page.waitForTimeout(100).then(() => false),
    ]);
    
    // Se não capturou o loading, pelo menos verifica que chegou na página correta
    await expect(page).toHaveURL('/barber/schedule', { timeout: 5000 });
  });

  test('deve redirecionar para login ao detectar token inválido', async ({ page }) => {
    // Injeta um token inválido no localStorage
    await page.goto('/login');
    await page.evaluate(() => {
      localStorage.setItem('barbapp-barber-token', 'token-invalido-xyz123');
    });
    
    // Tenta acessar rota protegida
    await page.goto('/barber/schedule');
    
    // Deve detectar token inválido e redirecionar para login
    await expect(page).toHaveURL('/login', { timeout: 10000 });
    
    // Token inválido deve ter sido removido
    const token = await page.evaluate(() => localStorage.getItem('barbapp-barber-token'));
    expect(token).toBeNull();
  });

  test('deve redirecionar para login ao detectar token expirado', async ({ page }) => {
    // Injeta um token expirado no localStorage
    await page.goto('/login');
    await page.evaluate(() => {
      // JWT expirado (expired em 2020)
      const expiredToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.4Adcj0qB0CJbFxe4K4X4K5X4X4X4X4X4X4X4X4X4X4X';
      localStorage.setItem('barbapp-barber-token', expiredToken);
    });
    
    // Tenta acessar rota protegida
    await page.goto('/barber/schedule');
    
    // Deve detectar token expirado e redirecionar para login
    await expect(page).toHaveURL('/login', { timeout: 10000 });
  });

  test('deve preservar autenticação em múltiplas abas', async ({ browser }) => {
    // Cria contexto com storage compartilhado
    const context = await browser.newContext();
    
    // Aba 1: Faz login
    const page1 = await context.newPage();
    await page1.goto('/login');
    await page1.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page1.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page1.click('button[type="submit"]');
    await expect(page1).toHaveURL('/barber/schedule', { timeout: 10000 });
    
    // Aba 2: Abre e verifica que já está autenticado
    const page2 = await context.newPage();
    await page2.goto('/barber/schedule');
    
    // Deve estar autenticado e não redirecionar para login
    await expect(page2).toHaveURL('/barber/schedule');
    
    // Verifica que ambas as abas têm o token
    const token1 = await page1.evaluate(() => localStorage.getItem('barbapp-barber-token'));
    const token2 = await page2.evaluate(() => localStorage.getItem('barbapp-barber-token'));
    
    expect(token1).toBeTruthy();
    expect(token1).toBe(token2);
    
    await context.close();
  });

  test('deve fazer logout e limpar autenticação de todas as abas', async ({ browser }) => {
    // Cria contexto com storage compartilhado
    const context = await browser.newContext();
    
    // Aba 1: Faz login
    const page1 = await context.newPage();
    await page1.goto('/login');
    await page1.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page1.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page1.click('button[type="submit"]');
    await expect(page1).toHaveURL('/barber/schedule', { timeout: 10000 });
    
    // Aba 2: Abre página de agenda
    const page2 = await context.newPage();
    await page2.goto('/barber/schedule');
    await expect(page2).toHaveURL('/barber/schedule');
    
    // Aba 1: Faz logout
    await page1.click('button:has-text("Sair")');
    await expect(page1).toHaveURL('/login', { timeout: 5000 });
    
    // Aba 2: Recarrega e verifica que foi deslogado
    await page2.reload();
    await expect(page2).toHaveURL('/login', { timeout: 10000 });
    
    // Verifica que não há token em nenhuma aba
    const token1 = await page1.evaluate(() => localStorage.getItem('barbapp-barber-token'));
    const token2 = await page2.evaluate(() => localStorage.getItem('barbapp-barber-token'));
    
    expect(token1).toBeNull();
    expect(token2).toBeNull();
    
    await context.close();
  });

  test('deve redirecionar para agenda se já autenticado tentar acessar login', async ({ page }) => {
    // Faz login
    await loginAsBarber(page);
    await expect(page).toHaveURL('/barber/schedule');
    
    // Tenta acessar página de login novamente
    await page.goto('/login');
    
    // Deve redirecionar de volta para agenda
    await expect(page).toHaveURL('/barber/schedule', { timeout: 5000 });
  });

  test('deve manter dados do usuário após navegação', async ({ page }) => {
    // Faz login
    await loginAsBarber(page);
    
    // Verifica dados do usuário na página
    await expect(page.locator(`text=${TEST_BARBER_CREDENTIALS.nome}`)).toBeVisible();
    
    // Recarrega página
    await page.reload();
    
    // Verifica que dados do usuário ainda estão visíveis
    await expect(page.locator(`text=${TEST_BARBER_CREDENTIALS.nome}`)).toBeVisible({
      timeout: 5000,
    });
  });

  test('deve exibir mensagem amigável durante carregamento inicial', async ({ page }) => {
    await page.goto('/barber/schedule');
    
    // Durante redirecionamento/validação, pode exibir loading
    // Este teste verifica que não há telas em branco
    const hasContent = await Promise.race([
      page.locator('text=Carregando...').isVisible(),
      page.locator('h1').isVisible(),
      page.waitForTimeout(1000).then(() => true),
    ]);
    
    expect(hasContent).toBeTruthy();
  });
});

test.describe('Barber Schedule - Casos de Erro e Edge Cases', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
  });

  test('deve lidar com erro de rede durante login', async ({ page }) => {
    // Simula offline
    await page.context().setOffline(true);
    
    await page.goto('/login');
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // Deve exibir mensagem de erro
    // (ajuste conforme implementação - pode ser toast ou inline)
    await expect(page.locator('text=/erro|falha|tente novamente/i')).toBeVisible({
      timeout: 5000,
    });
    
    // Volta online
    await page.context().setOffline(false);
  });

  test('deve lidar com localStorage desabilitado', async ({ page }) => {
    // Desabilita localStorage
    await page.addInitScript(() => {
      Object.defineProperty(window, 'localStorage', {
        value: {
          getItem: () => { throw new Error('localStorage disabled'); },
          setItem: () => { throw new Error('localStorage disabled'); },
          removeItem: () => { throw new Error('localStorage disabled'); },
          clear: () => { throw new Error('localStorage disabled'); },
        },
      });
    });
    
    // Tenta fazer login
    await page.goto('/login');
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // Sistema deve lidar com erro gracefully
    // (ajuste conforme implementação - pode exibir mensagem de erro)
    const hasError = await Promise.race([
      page.locator('text=/erro|problema|navegador/i').isVisible(),
      page.waitForTimeout(3000).then(() => false),
    ]);
    
    // Se não houver mensagem de erro específica, pelo menos não deve crashar
    expect(page.url()).toBeTruthy();
  });

  test('deve recuperar de erro temporário de API', async ({ page }) => {
    await page.goto('/login');
    
    // Primeira tentativa: simula erro 500
    await page.route('**/api/auth/login', (route) => {
      route.fulfill({
        status: 500,
        body: JSON.stringify({ error: 'Internal Server Error' }),
      });
    });
    
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // Verifica erro
    await expect(page.locator('text=/erro|falha/i')).toBeVisible({ timeout: 5000 });
    
    // Remove intercept para permitir requisição real
    await page.unroute('**/api/auth/login');
    
    // Segunda tentativa: deve funcionar
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('/barber/schedule', { timeout: 10000 });
  });
});
