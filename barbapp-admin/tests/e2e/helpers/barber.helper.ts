import { Page, expect } from '@playwright/test';

/**
 * Helpers para testes E2E do Interface de Login do Barbeiro
 */

export const TEST_BARBER_CREDENTIALS = {
  email: 'barbeiro@test.com',
  password: 'Test@123',
  nome: 'João Silva',
};

/**
 * Realiza login no sistema do Barbeiro
 */
export async function loginAsBarber(page: Page) {
  await page.goto('/login');
  
  // Preenche formulário
  await page.fill('input[type="email"]', TEST_BARBER_CREDENTIALS.email);
  await page.fill('input[type="password"]', TEST_BARBER_CREDENTIALS.password);
  
  // Clica em entrar
  await page.click('button[type="submit"]:has-text("Entrar")');
  
  // Aguarda redirecionamento
  await expect(page).toHaveURL('/barber/schedule', {
    timeout: 10000,
  });
}

/**
 * Verifica se está autenticado como barbeiro
 */
export async function isBarberAuthenticated(page: Page): Promise<boolean> {
  try {
    const token = await page.evaluate(() => {
      try {
        return localStorage.getItem('barber_token');
      } catch (e) {
        return null;
      }
    });
    return token !== null;
  } catch (e) {
    return false;
  }
}

/**
 * Realiza logout do barbeiro
 */
export async function logoutBarber(page: Page) {
  // Procura botão de logout
  const logoutButton = page.locator('button:has-text("Sair")').first();
  if (await logoutButton.isVisible()) {
    await logoutButton.click();
  }
  
  // Aguardar redirecionamento para login
  await expect(page).toHaveURL('/login', {
    timeout: 5000,
  });
  
  // Limpar localStorage após logout
  await page.evaluate(() => {
    try {
      localStorage.removeItem('barber_token');
      localStorage.removeItem('barber_user');
    } catch (e) {
      // Ignorar erros
    }
  });
}

/**
 * Limpa dados de autenticação do barbeiro
 */
export async function clearBarberAuth(page: Page) {
  // Aguardar página carregar completamente
  await page.waitForLoadState('domcontentloaded');
  
  await page.evaluate(() => {
    try {
      localStorage.removeItem('barber_token');
      localStorage.removeItem('barber_user');
    } catch (e) {
      // Ignorar erros de localStorage (pode não estar disponível)
      console.log('localStorage not available:', e);
    }
  });
}

/**
 * Aguarda toast de sucesso
 */
export async function waitForSuccessToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    // Toast do shadcn-ui tem role="status"
    await expect(page.locator('[role="status"]').first()).toBeVisible({ timeout: 5000 });
  }
}

/**
 * Aguarda toast de erro
 */
export async function waitForErrorToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    // Toast de erro tem variante destrutiva
    await expect(page.locator('[data-sonner-toast][data-type="error"]').first()).toBeVisible({ 
      timeout: 5000 
    });
  }
}

/**
 * Verifica se está na página de loading
 */
export async function isLoading(page: Page): Promise<boolean> {
  const loadingIndicator = page.locator('text=Carregando...').first();
  return await loadingIndicator.isVisible();
}
