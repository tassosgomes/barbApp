import { Page, expect } from '@playwright/test';

/**
 * Helpers para testes E2E do Admin Barbearia
 */

export const TEST_CREDENTIALS = {
  codigo: 'AMG7V8Y9',
  email: 'neide.patricio@hotmail.com',
  senha: 'S4nE23g@Qgu5',
  barbeariaNome: 'Barbearia da Neide',
};

/**
 * Realiza login no sistema Admin Barbearia
 */
export async function loginAsAdminBarbearia(page: Page) {
  await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
  
  // Aguarda validação do código
  await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
    timeout: 10000,
  });
  
  // Preenche formulário
  await page.fill('input[type="email"]', TEST_CREDENTIALS.email);
  await page.fill('input[type="password"]', TEST_CREDENTIALS.senha);
  
  // Clica em entrar
  await page.click('button:has-text("Entrar")');
  
  // Aguarda redirecionamento
  await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`, {
    timeout: 10000,
  });
}

/**
 * Verifica se está autenticado
 */
export async function isAuthenticated(page: Page): Promise<boolean> {
  const token = await page.evaluate(() => localStorage.getItem('adminBarbearia_token'));
  return token !== null;
}

/**
 * Realiza logout
 */
export async function logout(page: Page) {
  // Procura botão de logout (pode estar em dropdown)
  const logoutButton = page.locator('button:has-text("Sair")').first();
  if (await logoutButton.isVisible()) {
    await logoutButton.click();
  }
  
  // Aguarda redirecionamento para login
  await expect(page).toHaveURL(new RegExp(`/${TEST_CREDENTIALS.codigo}/login`), {
    timeout: 5000,
  });
}

/**
 * Limpa dados de autenticação
 */
export async function clearAuth(page: Page) {
  // Navigate to a page first to be able to access localStorage
  await page.goto('/');
  await page.evaluate(() => {
    localStorage.clear();
    sessionStorage.clear();
  });
}

/**
 * Aguarda toast de sucesso
 */
export async function waitForSuccessToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    await expect(page.locator('[role="status"]')).toBeVisible({ timeout: 5000 });
  }
}

/**
 * Aguarda toast de erro
 */
export async function waitForErrorToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    await expect(page.locator('[role="alert"]')).toBeVisible({ timeout: 5000 });
  }
}

/**
 * Navega para uma página específica do Admin Barbearia
 */
export async function navigateTo(page: Page, path: string) {
  await page.goto(`/${TEST_CREDENTIALS.codigo}/${path}`);
}

/**
 * Preenche formulário de barbeiro
 */
export async function fillBarbeiroForm(page: Page, data: {
  nome: string;
  email: string;
  telefone: string;
  servicos?: string[];
}) {
  await page.fill('input[name="nome"]', data.nome);
  await page.fill('input[name="email"]', data.email);
  await page.fill('input[name="telefone"]', data.telefone);
  
  if (data.servicos && data.servicos.length > 0) {
    for (const servico of data.servicos) {
      await page.check(`input[type="checkbox"][value="${servico}"]`);
    }
  }
}

/**
 * Preenche formulário de serviço
 */
export async function fillServicoForm(page: Page, data: {
  nome: string;
  descricao?: string;
  duracaoMinutos: number;
  preco: number;
}) {
  await page.fill('input[name="nome"]', data.nome);
  
  if (data.descricao) {
    await page.fill('textarea[name="descricao"]', data.descricao);
  }
  
  await page.fill('input[name="duracaoMinutos"]', data.duracaoMinutos.toString());
  await page.fill('input[name="preco"]', data.preco.toString());
}
