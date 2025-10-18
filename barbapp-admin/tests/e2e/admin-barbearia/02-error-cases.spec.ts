import { test, expect } from '@playwright/test';
import {
  TEST_CREDENTIALS,
  clearAuth,
  waitForErrorToast,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Casos de Erro', () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
  });

  test('deve exibir erro para código de barbearia inválido', async ({ page }) => {
    await page.goto('/CODIGOINVALIDO/login');
    
    // Deve exibir mensagem de erro
    await expect(page.locator('text=/código.*inválido/i')).toBeVisible({
      timeout: 10000,
    });
    
    // Não deve exibir formulário de login
    await expect(page.locator('input[type="email"]')).not.toBeVisible();
  });

  test('deve exibir erro para credenciais inválidas', async ({ page }) => {
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    // Preenche com credenciais erradas
    await page.fill('input[type="email"]', 'wrong@test.com');
    await page.fill('input[type="password"]', 'WrongPassword123!');
    await page.click('button:has-text("Entrar")');
    
    // Deve exibir toast de erro
    await waitForErrorToast(page);
  });

  test('deve exibir erro para email inválido', async ({ page }) => {
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    const emailInput = page.locator('input[type="email"]');
    
    // Preenche email inválido
    await emailInput.fill('emailinvalido');
    await page.fill('input[type="password"]', TEST_CREDENTIALS.senha);
    await page.click('button:has-text("Entrar")');
    
    // Validação HTML5 deve impedir submit ou deve exibir erro
    const validationMessage = await emailInput.evaluate(
      (el: HTMLInputElement) => el.validationMessage
    );
    
    expect(validationMessage).toBeTruthy();
  });

  test('deve bloquear acesso a rotas protegidas sem autenticação', async ({ page }) => {
    const protectedRoutes = [
      `/${TEST_CREDENTIALS.codigo}/dashboard`,
      `/${TEST_CREDENTIALS.codigo}/barbeiros`,
      `/${TEST_CREDENTIALS.codigo}/servicos`,
      `/${TEST_CREDENTIALS.codigo}/agenda`,
    ];
    
    for (const route of protectedRoutes) {
      await page.goto(route);
      
      // Deve redirecionar para login
      await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/login`, {
        timeout: 5000,
      });
    }
  });

  test('deve exibir erro ao tentar acessar rota de outro tenant', async ({ page }) => {
    // Faz login com tenant válido
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    await page.fill('input[type="email"]', TEST_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_CREDENTIALS.senha);
    await page.click('button:has-text("Entrar")');
    
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`);
    
    // Tenta acessar rota de outro tenant
    await page.goto('/OUTROTENANT/barbeiros');
    
    // Deve exibir erro ou redirecionar
    const hasError = await Promise.race([
      page.locator('text=/não autorizado/i').isVisible(),
      page.locator('text=/acesso negado/i').isVisible(),
      page.waitForURL(/\/login/, { timeout: 5000 }).then(() => true),
    ]).catch(() => false);
    
    expect(hasError).toBeTruthy();
  });
});
