import { test, expect } from '@playwright/test';
import {
  TEST_CREDENTIALS,
  loginAsAdminBarbearia,
  clearAuth,
  isAuthenticated,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Login e Autenticação', () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
  });

  test('deve validar código da barbearia ao carregar página de login', async ({ page }) => {
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    // Deve exibir nome da barbearia após validação
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    // Deve exibir formulário de login
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
    await expect(page.locator('button:has-text("Entrar")')).toBeVisible();
  });

  test('deve realizar login com sucesso', async ({ page }) => {
    await loginAsAdminBarbearia(page);
    
    // Deve estar autenticado
    const authenticated = await isAuthenticated(page);
    expect(authenticated).toBe(true);
    
    // Deve estar na página de dashboard
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`);
  });

  test('deve exibir menu de navegação após login', async ({ page }) => {
    await loginAsAdminBarbearia(page);
    
    // Deve exibir links de navegação
    await expect(page.locator('a:has-text("Dashboard")')).toBeVisible();
    await expect(page.locator('a:has-text("Barbeiros")')).toBeVisible();
    await expect(page.locator('a:has-text("Serviços")')).toBeVisible();
    await expect(page.locator('a:has-text("Agenda")')).toBeVisible();
  });

  test('deve persistir autenticação após reload', async ({ page }) => {
    await loginAsAdminBarbearia(page);
    
    // Recarrega página
    await page.reload();
    
    // Deve continuar autenticado
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`);
    const authenticated = await isAuthenticated(page);
    expect(authenticated).toBe(true);
  });

  test('deve redirecionar para login se não autenticado', async ({ page }) => {
    // Tenta acessar página protegida sem autenticação
    await page.goto(`/${TEST_CREDENTIALS.codigo}/barbeiros`);
    
    // Deve redirecionar para login
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/login`, {
      timeout: 5000,
    });
  });

  test('deve validar campos obrigatórios no formulário', async ({ page }) => {
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    // Aguarda validação do código
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    // Tenta submeter sem preencher
    await page.click('button:has-text("Entrar")');
    
    // Deve exibir mensagens de validação HTML5
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    
    await expect(emailInput).toHaveAttribute('required', '');
    await expect(passwordInput).toHaveAttribute('required', '');
  });

  test('deve desabilitar botão durante submit', async ({ page }) => {
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    await page.fill('input[type="email"]', TEST_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_CREDENTIALS.senha);
    
    const submitButton = page.locator('button:has-text("Entrar")');
    
    // Clica e verifica se desabilita
    await submitButton.click();
    
    // Durante processamento, botão deve estar desabilitado ou com texto "Entrando..."
    const isDisabledOrLoading = await Promise.race([
      submitButton.isDisabled(),
      page.locator('button:has-text("Entrando...")').isVisible(),
    ]);
    
    expect(isDisabledOrLoading).toBeTruthy();
  });
});
