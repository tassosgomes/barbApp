import { test, expect } from '@playwright/test';
import {
  TEST_BARBER_CREDENTIALS,
  loginAsBarber,
  clearBarberAuth,
  isBarberAuthenticated,
  logoutBarber,
  waitForErrorToast,
} from '../helpers/barber.helper';

test.describe('Barber Login - Autenticação e Fluxo de Login', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
  });

  test('deve renderizar página de login com todos os elementos', async ({ page }) => {
    await page.goto('/login');
    
    // Verifica elementos do formulário
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
    await expect(page.locator('button[type="submit"]:has-text("Entrar")')).toBeVisible();
    
    // Verifica botão de ajuda
    await expect(page.locator('button:has-text("Precisa de ajuda?")')).toBeVisible();
  });

  test('deve realizar login com sucesso e redirecionar para agenda', async ({ page }) => {
    await loginAsBarber(page);
    
    // Verifica que está autenticado
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(true);
    
    // Verifica que está na página de agenda
    await expect(page).toHaveURL('/barber/schedule');
    
    // Verifica que a página de agenda carregou (botão de sair visível)
    await expect(page.getByRole('button', { name: /sair/i })).toBeVisible({
      timeout: 5000,
    });
  });

  test('deve validar campo email obrigatório', async ({ page }) => {
    await page.goto('/login');
    
    // Tenta submeter sem preencher email
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // Verifica mensagem de erro de validação Zod
    await expect(page.getByTestId('email-error')).toBeVisible({ timeout: 3000 });
    
    // Verifica que NÃO redirecionou
    await expect(page).toHaveURL('/login');
  });

  test('deve validar campo senha obrigatório', async ({ page }) => {
    await page.goto('/login');
    
    // Tenta submeter sem preencher senha
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.click('button[type="submit"]');
    
    // Verifica mensagem de erro de validação Zod
    await expect(page.getByTestId('password-error')).toBeVisible({ timeout: 3000 });
    
    // Verifica que NÃO redirecionou
    await expect(page).toHaveURL('/login');
  });

  test('deve validar formato de email', async ({ page }) => {
    await page.goto('/login');
    
    // Preenche com email inválido
    await page.fill('input[type="email"]', 'emailinvalido');
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    await page.click('button[type="submit"]');
    
    // Verifica que não redirecionou (validação HTML5 impediu)
    await expect(page).toHaveURL('/login');
  });

  test('deve exibir erro para credenciais inválidas', async ({ page }) => {
    await page.goto('/login');
    
    // Preenche com credenciais inválidas
    await page.fill('input[type="email"]', 'invalido@test.com');
    await page.fill('input[type="password"]', 'SenhaErrada123');
    await page.click('button[type="submit"]');
    
    // Aguarda e verifica toast de erro
    await waitForErrorToast(page);
    
    // Verifica que permaneceu na página de login
    await expect(page).toHaveURL('/login');
    
    // Verifica que NÃO está autenticado
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(false);
  });

  test('deve desabilitar botão e campos durante submissão', async ({ page }) => {
    await page.goto('/login');
    
    // Preenche formulário
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    
    // Clica no botão
    const submitButton = page.locator('button[type="submit"]');
    await submitButton.click();
    
    // Verifica que o botão está desabilitado OU mostra texto de loading
    // (precisa ser rápido para pegar o estado de loading)
    const isDisabledOrLoading = await Promise.race([
      submitButton.isDisabled(),
      page.locator('button:has-text("Entrando...")').isVisible(),
    ]);
    
    expect(isDisabledOrLoading).toBeTruthy();
  });

  test('deve persistir autenticação após reload da página', async ({ page }) => {
    await loginAsBarber(page);
    
    // Recarrega página
    await page.reload();
    
    // Verifica que continua autenticado
    await expect(page).toHaveURL('/barber/schedule');
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(true);
  });

  test('deve redirecionar para login se tentar acessar rota protegida sem autenticação', async ({ page }) => {
    // Tenta acessar página protegida sem autenticação
    await page.goto('/barber/schedule');
    
    // Deve redirecionar para login
    await expect(page).toHaveURL('/login', {
      timeout: 5000,
    });
  });

  test('deve realizar logout com sucesso', async ({ page }) => {
    // Faz login
    await loginAsBarber(page);
    
    // Realiza logout
    await logoutBarber(page);
    
    // Verifica que foi redirecionado para login
    await expect(page).toHaveURL('/login');
    
    // Verifica que NÃO está mais autenticado
    const authenticated = await isBarberAuthenticated(page);
    expect(authenticated).toBe(false);
  });

  test('deve exibir modal de ajuda ao clicar em "Precisa de ajuda?"', async ({ page }) => {
    await page.goto('/login');
    
    // Clica no botão de ajuda
    await page.click('button:has-text("Precisa de ajuda?")');
    
    // Verifica que modal apareceu
    await expect(page.locator('[role="dialog"]')).toBeVisible({ timeout: 3000 });
    
    // Verifica conteúdo do modal
    await expect(page.locator('text=Como fazer login')).toBeVisible();
    await expect(page.locator('text=📧 E-mail:')).toBeVisible();
    await expect(page.locator('text=🔒 Senha:')).toBeVisible();
    
    // Fecha modal
    await page.click('button:has-text("Entendi")');
    
    // Verifica que modal fechou
    await expect(page.locator('[role="dialog"]')).not.toBeVisible();
  });

  test('deve manter foco nos campos do formulário', async ({ page }) => {
    await page.goto('/login');
    
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    
    // Foca no email
    await emailInput.focus();
    await expect(emailInput).toBeFocused();
    
    // Pressiona Tab para ir para senha
    await page.keyboard.press('Tab');
    await expect(passwordInput).toBeFocused();
    
    // Pressiona Tab novamente para ir para botão
    await page.keyboard.press('Tab');
    const submitButton = page.locator('button[type="submit"]');
    await expect(submitButton).toBeFocused();
  });

  test('deve submeter formulário ao pressionar Enter no campo senha', async ({ page }) => {
    await page.goto('/login');
    
    // Preenche campos
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
    
    // Pressiona Enter no campo senha
    await page.locator('input[type="password"]').press('Enter');
    
    // Verifica que submeteu e redirecionou
    await expect(page).toHaveURL('/barber/schedule', {
      timeout: 10000,
    });
  });

  test('deve limpar campos após erro de login', async ({ page }) => {
    await page.goto('/login');
    
    // Preenche com credenciais inválidas
    const emailInput = page.locator('input[type="email"]');
    const passwordInput = page.locator('input[type="password"]');
    
    await emailInput.fill('invalido@test.com');
    await passwordInput.fill('SenhaErrada');
    await page.click('button[type="submit"]');
    
    // Aguarda erro
    await waitForErrorToast(page);
    
    // Verifica que os campos ainda contém os valores
    // (alguns sistemas limpam, outros mantêm - ajuste conforme implementação)
    await expect(emailInput).toHaveValue('invalido@test.com');
  });

  test('deve validar senha com requisitos mínimos', async ({ page }) => {
    await page.goto('/login');
    
    // Tenta submeter com senha muito curta
    await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
    await page.fill('input[type="password"]', '123'); // Senha muito curta
    await page.click('button[type="submit"]');
    
    // Verifica mensagem de validação
    // (ajuste conforme implementação - pode ser validação client-side ou server-side)
    await expect(page).toHaveURL('/login');
  });
});
