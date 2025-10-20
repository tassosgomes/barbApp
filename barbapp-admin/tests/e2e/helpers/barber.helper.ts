import { Page, expect } from '@playwright/test';

/**
 * Helpers para testes E2E do Interface de Login do Barbeiro
 */

export const TEST_BARBER_CREDENTIALS = {
  email: 'dino@sauro.com',
  password: 'Neide@9090',
  nome: 'Dino Sauro',
};

/**
 * Realiza login no sistema do Barbeiro
 */
export async function loginAsBarber(page: Page) {
  // Mock da API de login
  await page.route('**/api/auth/barbeiro/login', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        token: 'mock-jwt-token-for-barber',
        tipoUsuario: 'Barbeiro',
        barbeariaId: '123e4567-e89b-12d3-a456-426614174000',
        nomeBarbearia: 'Barbearia Teste',
        codigoBarbearia: 'TEST001',
        expiresAt: new Date(Date.now() + 3600000).toISOString(), // 1 hour from now
      }),
    });
  });

  // Mock da API de validação de token
  await page.route('**/api/barber/profile', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        id: '123e4567-e89b-12d3-a456-426614174001',
        nome: TEST_BARBER_CREDENTIALS.nome,
        email: TEST_BARBER_CREDENTIALS.email,
        tipoUsuario: 'Barbeiro',
        barbeariaId: '123e4567-e89b-12d3-a456-426614174000',
      }),
    });
  });

  // Mock da API de barbearias do barbeiro
  await page.route('**/api/barbeiro/barbearias', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify([
        {
          id: '123e4567-e89b-12d3-a456-426614174000',
          nome: 'Barbearia Teste',
          codigo: 'TEST001',
          isActive: true,
        },
      ]),
    });
  });

  // Mock da API de agenda do barbeiro
  await page.route('**/api/schedule/my-schedule**', async (route) => {
    const url = new URL(route.request().url());
    const date = url.searchParams.get('date') || new Date().toISOString().split('T')[0];
    
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({
        date: date,
        barberId: '123e4567-e89b-12d3-a456-426614174001',
        barberName: 'Dino Sauro',
        appointments: [
          {
            id: '123e4567-e89b-12d3-a456-426614174002',
            customerName: 'João Silva',
            serviceTitle: 'Corte de Cabelo',
            startTime: `${date}T10:00:00Z`,
            endTime: `${date}T11:00:00Z`,
            status: 1, // Confirmed
          },
          {
            id: '123e4567-e89b-12d3-a456-426614174003',
            customerName: 'Maria Santos',
            serviceTitle: 'Barba',
            startTime: `${date}T14:00:00Z`,
            endTime: `${date}T14:30:00Z`,
            status: 0, // Pending
          },
          {
            id: '123e4567-e89b-12d3-a456-426614174004',
            customerName: 'Pedro Oliveira',
            serviceTitle: 'Corte + Barba',
            startTime: `${date}T16:00:00Z`,
            endTime: `${date}T17:00:00Z`,
            status: 1, // Confirmed
          },
        ],
      }),
    });
  });

  // Mock da API de confirmação de agendamento
  await page.route('**/api/appointments/*/confirm', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock da API de cancelamento de agendamento
  await page.route('**/api/appointments/*/cancel', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock da API de conclusão de agendamento
  await page.route('**/api/appointments/*/complete', async (route) => {
    await route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify({ success: true }),
    });
  });

  // Mock da API de detalhes do agendamento
  await page.route('**/api/appointments/*', async (route) => {
    if (route.request().method() === 'GET') {
      const url = route.request().url();
      const appointmentId = url.split('/').pop();
      const date = new Date().toISOString().split('T')[0];
      
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: appointmentId,
          customerName: 'João Silva',
          serviceTitle: 'Corte de Cabelo',
          startTime: `${date}T10:00:00Z`,
          endTime: `${date}T11:00:00Z`,
          status: 1, // Confirmed
          customerPhone: '+5511999999999',
          servicePrice: 35.00,
          serviceDurationMinutes: 60,
          createdAt: new Date().toISOString(),
          confirmedAt: new Date().toISOString(),
        }),
      });
    }
  });

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
        return localStorage.getItem('barbapp-barber-token');
      } catch (e) {
        return null;
      }
    });
    return token !== null && token !== '';
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
      localStorage.removeItem('barbapp-barber-token');
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
      localStorage.removeItem('barbapp-barber-token');
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
/**
 * Aguarda toast de sucesso
 */
export async function waitForSuccessToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    // Toast do Radix UI
    await expect(page.locator('[data-state="open"]').first()).toBeVisible({ timeout: 5000 });
  }
}

/**
 * Aguarda toast de erro (variante destructive)
 */
export async function waitForErrorToast(page: Page, message?: string) {
  if (message) {
    await expect(page.locator(`text=${message}`)).toBeVisible({ timeout: 5000 });
  } else {
    // Toast de erro do Radix UI com variante destructive
    await expect(page.locator('.destructive').first()).toBeVisible({ 
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
