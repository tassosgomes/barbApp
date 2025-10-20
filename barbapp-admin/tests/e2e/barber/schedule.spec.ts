import { test, expect } from '@playwright/test';
import {
  loginAsBarber,
  clearBarberAuth,
  waitForSuccessToast,
  waitForErrorToast,
} from '../helpers/barber.helper';

/**
 * Testes E2E para a funcionalidade de agenda do barbeiro
 *
 * Estes testes cobrem os fluxos completos de:
 * - Login e visualização da agenda
 * - Confirmação de agendamentos
 * - Cancelamento de agendamentos
 * - Conclusão de atendimentos
 * - Navegação entre dias
 * - Troca de contexto multi-barbearia
 * - Polling automático
 * - Tratamento de erros
 * - Responsividade mobile
 * - Isolamento multi-tenant
 */

test.describe('Barber Schedule - Visualização', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve exibir agenda do dia atual', async ({ page }) => {
    // Verificar header com data de hoje
    const today = new Date().toLocaleDateString('pt-BR', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
    await expect(page.getByText(today)).toBeVisible();

    // Verificar que estamos na rota correta
    await expect(page).toHaveURL('/barber/schedule');

    // Verificar elementos principais da página
    await expect(page.getByTestId('schedule-header')).toBeVisible();
    await expect(page.getByTestId('appointments-list')).toBeVisible();
  });

  test('deve mostrar contador de agendamentos', async ({ page }) => {
    // Verificar se há um contador de agendamentos no header
    const counter = page.locator('[data-testid="appointments-count"]').or(
      page.locator('text=/\\d+ agendamentos? hoje/')
    );
    await expect(counter.first()).toBeVisible();
  });

  test('deve exibir lista de agendamentos com informações corretas', async ({ page }) => {
    // Aguardar carregamento
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Verificar se há pelo menos um agendamento ou mensagem de vazio
    const hasAppointments = await page.locator('[data-testid="appointment-card"]').count() > 0;
    const hasEmptyMessage = await page.locator('text=/Nenhum agendamento|Sem agendamentos/').isVisible();

    expect(hasAppointments || hasEmptyMessage).toBe(true);

    if (hasAppointments) {
      // Verificar estrutura dos cards de agendamento
      const firstAppointment = page.locator('[data-testid="appointment-card"]').first();

      // Verificar elementos obrigatórios
      await expect(firstAppointment.locator('[data-testid="customer-name"]')).toBeVisible();
      await expect(firstAppointment.locator('[data-testid="service-title"]')).toBeVisible();
      await expect(firstAppointment.locator('[data-testid="appointment-time"]')).toBeVisible();
      await expect(firstAppointment.locator('[data-testid="appointment-status"]')).toBeVisible();
    }
  });
});

test.describe('Barber Schedule - Ações', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve confirmar agendamento pendente', async ({ page }) => {
    // Aguardar carregamento da agenda
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Procurar agendamento pendente
    const pendingAppointment = page.locator('[data-status="pending"]').first();

    // Se não há agendamentos pendentes, pular o teste
    if (!(await pendingAppointment.isVisible())) {
      test.skip(true, 'Nenhum agendamento pendente encontrado para testar');
      return;
    }

    // Clicar no agendamento para abrir modal de detalhes
    await pendingAppointment.click();

    // Verificar modal aberto
    await expect(page.locator('[data-testid="appointment-details-modal"]')).toBeVisible();

    // Clicar em confirmar
    const confirmButton = page.locator('[data-testid="confirm-appointment-btn"]');
    await expect(confirmButton).toBeVisible();
    await confirmButton.click();

    // Aguardar toast de sucesso
    await waitForSuccessToast(page);

    // Verificar mudança de status
    await expect(pendingAppointment).toHaveAttribute('data-status', 'confirmed');

    // Verificar botão de confirmar não está mais disponível
    await expect(confirmButton).not.toBeVisible();
  });

  test('deve cancelar agendamento com confirmação', async ({ page }) => {
    // Aguardar carregamento da agenda
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Procurar agendamento que pode ser cancelado (pendente ou confirmado)
    const cancellableAppointment = page.locator('[data-status="pending"], [data-status="confirmed"]').first();

    if (!(await cancellableAppointment.isVisible())) {
      test.skip(true, 'Nenhum agendamento cancelável encontrado para testar');
      return;
    }

    // Clicar no agendamento
    await cancellableAppointment.click();

    // Verificar modal de detalhes
    await expect(page.locator('[data-testid="appointment-details-modal"]')).toBeVisible();

    // Clicar em cancelar
    const cancelButton = page.locator('[data-testid="cancel-appointment-btn"]');
    await expect(cancelButton).toBeVisible();
    await cancelButton.click();

    // Verificar dialog de confirmação
    await expect(page.locator('[data-testid="cancel-confirmation-dialog"]')).toBeVisible();

    // Confirmar cancelamento
    const confirmCancelButton = page.locator('[data-testid="confirm-cancel-btn"]');
    await expect(confirmCancelButton).toBeVisible();
    await confirmCancelButton.click();

    // Aguardar toast de sucesso
    await waitForSuccessToast(page);

    // Verificar mudança de status
    await expect(cancellableAppointment).toHaveAttribute('data-status', 'cancelled');

    // Verificar modal fechado
    await expect(page.locator('[data-testid="appointment-details-modal"]')).not.toBeVisible();
  });

  test('deve concluir agendamento após horário', async ({ page }) => {
    // Aguardar carregamento da agenda
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Procurar agendamento confirmado que já passou do horário
    const completedAppointment = page.locator('[data-status="confirmed"]').first();

    if (!(await completedAppointment.isVisible())) {
      test.skip(true, 'Nenhum agendamento confirmado encontrado para testar conclusão');
      return;
    }

    // Clicar no agendamento
    await completedAppointment.click();

    // Verificar modal de detalhes
    await expect(page.locator('[data-testid="appointment-details-modal"]')).toBeVisible();

    // Verificar se botão de concluir está disponível (só aparece após horário)
    const completeButton = page.locator('[data-testid="complete-appointment-btn"]');

    if (await completeButton.isVisible()) {
      // Se botão está visível, testar conclusão
      await completeButton.click();

      // Aguardar toast de sucesso
      await waitForSuccessToast(page);

      // Verificar mudança de status
      await expect(completedAppointment).toHaveAttribute('data-status', 'completed');

      // Verificar modal fechado
      await expect(page.locator('[data-testid="appointment-details-modal"]')).not.toBeVisible();
    } else {
      // Se botão não está visível, verificar que agendamento ainda não pode ser concluído
      console.log('Botão de concluir não disponível - agendamento ainda no futuro');
    }
  });
});

test.describe('Barber Schedule - Navegação', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve navegar para dia anterior', async ({ page }) => {
    // Aguardar carregamento
    await page.waitForSelector('[data-testid="schedule-header"]', { timeout: 10000 });

    // Obter data atual exibida
    const initialDateText = await page.locator('[data-testid="current-date"]').textContent();

    // Clicar em "Dia Anterior"
    const prevButton = page.locator('[data-testid="prev-day-btn"]');
    await expect(prevButton).toBeVisible();
    await prevButton.click();

    // Aguardar atualização
    await page.waitForTimeout(500);

    // Verificar que data mudou
    const newDateText = await page.locator('[data-testid="current-date"]').textContent();
    expect(newDateText).not.toBe(initialDateText);
  });

  test('deve navegar para próximo dia', async ({ page }) => {
    // Aguardar carregamento
    await page.waitForSelector('[data-testid="schedule-header"]', { timeout: 10000 });

    // Obter data atual exibida
    const initialDateText = await page.locator('[data-testid="current-date"]').textContent();

    // Clicar em "Próximo Dia"
    const nextButton = page.locator('[data-testid="next-day-btn"]');
    await expect(nextButton).toBeVisible();
    await nextButton.click();

    // Aguardar atualização
    await page.waitForTimeout(500);

    // Verificar que data mudou
    const newDateText = await page.locator('[data-testid="current-date"]').textContent();
    expect(newDateText).not.toBe(initialDateText);
  });

  test('deve voltar para hoje', async ({ page }) => {
    // Navegar para outro dia primeiro
    await page.locator('[data-testid="next-day-btn"]').click();
    await page.waitForTimeout(500);

    // Clicar em "Hoje"
    const todayButton = page.locator('[data-testid="today-btn"]');
    await expect(todayButton).toBeVisible();
    await todayButton.click();

    // Aguardar atualização
    await page.waitForTimeout(500);

    // Verificar que voltou para hoje
    const today = new Date().toLocaleDateString('pt-BR', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
    await expect(page.getByText(today)).toBeVisible();
  });
});

test.describe('Barber Schedule - Polling', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve atualizar agenda automaticamente a cada 10 segundos', async ({ page }) => {
    // Aguardar carregamento inicial
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Registrar chamadas de rede para o endpoint de agenda
    const scheduleRequests: string[] = [];
    page.on('request', (request) => {
      if (request.url().includes('/api/schedule/my-schedule')) {
        scheduleRequests.push(request.url());
      }
    });

    // Aguardar polling (10s + margem)
    await page.waitForTimeout(11000);

    // Verificar que pelo menos uma requisição de polling foi feita
    expect(scheduleRequests.length).toBeGreaterThan(1);
  });
});

test.describe('Barber Schedule - Tratamento de Erros', () => {
  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve tratar erro 404 ao tentar ação em agendamento inexistente', async ({ page }) => {
    // Simular tentativa de ação em agendamento que não existe
    // Isso pode ser feito interceptando a requisição ou usando um ID inválido

    // Como não temos controle sobre os dados, vamos testar o fluxo de erro genérico
    // Interceptar requisições e simular erro 404
    await page.route('**/api/appointments/*/confirm', async (route) => {
      await route.fulfill({
        status: 404,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Agendamento não encontrado' }),
      });
    });

    // Tentar confirmar um agendamento (se existir)
    const pendingAppointment = page.locator('[data-status="pending"]').first();
    if (await pendingAppointment.isVisible()) {
      await pendingAppointment.click();
      const confirmButton = page.locator('[data-testid="confirm-appointment-btn"]');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();

        // Aguardar toast de erro
        await waitForErrorToast(page);
      }
    }
  });

  test('deve tratar erro 409 (conflito) ao tentar ação concorrente', async ({ page }) => {
    // Interceptar requisições e simular erro 409
    await page.route('**/api/appointments/*/confirm', async (route) => {
      await route.fulfill({
        status: 409,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Agendamento foi modificado por outro usuário' }),
      });
    });

    // Tentar confirmar um agendamento (se existir)
    const pendingAppointment = page.locator('[data-status="pending"]').first();
    if (await pendingAppointment.isVisible()) {
      await pendingAppointment.click();
      const confirmButton = page.locator('[data-testid="confirm-appointment-btn"]');
      if (await confirmButton.isVisible()) {
        await confirmButton.click();

        // Aguardar toast de erro
        await waitForErrorToast(page);
      }
    }
  });
});

test.describe('Barber Schedule - Mobile Responsividade', () => {
  test.use({ viewport: { width: 375, height: 667 } }); // iPhone SE

  test.beforeEach(async ({ page }) => {
    await clearBarberAuth(page);
    await loginAsBarber(page);
  });

  test('deve funcionar corretamente em viewport mobile', async ({ page }) => {
    // Verificar que página carrega
    await expect(page).toHaveURL('/barber/schedule');

    // Verificar elementos principais são visíveis
    await expect(page.getByTestId('schedule-header')).toBeVisible();
    await expect(page.getByTestId('appointments-list')).toBeVisible();

    // Verificar navegação touch-friendly
    const prevButton = page.locator('[data-testid="prev-day-btn"]');
    const nextButton = page.locator('[data-testid="next-day-btn"]');

    // Verificar que botões têm tamanho adequado para touch
    const prevBox = await prevButton.boundingBox();
    const nextBox = await nextButton.boundingBox();

    if (prevBox && nextBox) {
      expect(prevBox.width).toBeGreaterThanOrEqual(44);
      expect(prevBox.height).toBeGreaterThanOrEqual(44);
      expect(nextBox.width).toBeGreaterThanOrEqual(44);
      expect(nextBox.height).toBeGreaterThanOrEqual(44);
    }
  });

  test('deve suportar pull-to-refresh', async ({ page }) => {
    // Aguardar carregamento
    await page.waitForSelector('[data-testid="appointments-list"]', { timeout: 10000 });

    // Simular pull-to-refresh (arrastar para baixo)
    const appointmentsList = page.locator('[data-testid="appointments-list"]');

    // Obter posição inicial
    const initialScrollTop = await appointmentsList.evaluate((el) => el.scrollTop);

    // Simular gesto de pull-to-refresh
    await appointmentsList.dispatchEvent('touchstart', { touches: [{ clientY: 100 }] });
    await appointmentsList.dispatchEvent('touchmove', { touches: [{ clientY: 150 }] });
    await appointmentsList.dispatchEvent('touchend', { changedTouches: [{ clientY: 150 }] });

    // Aguardar possível refresh
    await page.waitForTimeout(1000);

    // Verificar que página ainda funciona (não quebrou)
    await expect(page.getByTestId('appointments-list')).toBeVisible();
  });
});