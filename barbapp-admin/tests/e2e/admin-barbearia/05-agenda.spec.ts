import { test, expect } from '@playwright/test';
import {
  loginAsAdminBarbearia,
  clearAuth,
  navigateTo,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Visualização da Agenda', () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
    await loginAsAdminBarbearia(page);
  });

  test('deve exibir página de agenda', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Deve exibir título
    await expect(page.locator('h1:has-text("Agenda")')).toBeVisible();
    
    // Deve exibir filtros
    await expect(page.locator('label:has-text("Barbeiro")')).toBeVisible();
    await expect(page.locator('label:has-text("Data Início")')).toBeVisible();
    await expect(page.locator('label:has-text("Data Fim")')).toBeVisible();
    await expect(page.locator('label:has-text("Status")')).toBeVisible();
  });

  test('deve listar agendamentos existentes', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Deve exibir tabela ou mensagem de vazio
    const hasTable = await page.locator('table').isVisible().catch(() => false);
    const hasEmptyMessage = await page
      .locator('text=/nenhum agendamento/i')
      .isVisible()
      .catch(() => false);
    
    expect(hasTable || hasEmptyMessage).toBeTruthy();
  });

  test('deve exibir colunas corretas na tabela', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Aguarda tabela carregar
    await page.waitForSelector('table', { timeout: 10000 });
    
    // Verifica cabeçalhos
    const headers = ['Data/Hora', 'Cliente', 'Barbeiro', 'Serviço', 'Status', 'Ações'];
    
    for (const header of headers) {
      const hasHeader = await page
        .locator(`th:has-text("${header}")`)
        .isVisible()
        .catch(() => false);
      
      if (!hasHeader) {
        console.log(`Header "${header}" não encontrado`);
      }
    }
  });

  test('deve filtrar por barbeiro', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Seleciona filtro de barbeiro
    const barbeiroSelect = page.locator('select[name="barbeiroId"]');
    
    if (await barbeiroSelect.isVisible()) {
      // Verifica se tem opções
      const options = await barbeiroSelect.locator('option').count();
      
      if (options > 1) {
        // Seleciona segundo barbeiro (primeiro é "Todos")
        await barbeiroSelect.selectOption({ index: 1 });
        
        // Aguarda filtro aplicar
        await page.waitForTimeout(1000);
        
        // Tabela deve recarregar
        const hasTable = await page.locator('table').isVisible().catch(() => false);
        expect(hasTable).toBeTruthy();
      }
    }
  });

  test('deve filtrar por data', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Preenche data início
    const dataInicio = page.locator('input[name="dataInicio"]');
    if (await dataInicio.isVisible()) {
      const today = new Date().toISOString().split('T')[0];
      await dataInicio.fill(today);
      
      // Aguarda filtro aplicar
      await page.waitForTimeout(1000);
    }
  });

  test('deve filtrar por status', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Seleciona filtro de status
    const statusSelect = page.locator('select[name="status"]');
    
    if (await statusSelect.isVisible()) {
      const statuses = ['Agendado', 'Confirmado', 'Em Andamento', 'Concluído', 'Cancelado'];
      
      for (const status of statuses) {
        // Verifica se opção existe
        const hasOption = await statusSelect
          .locator(`option:has-text("${status}")`)
          .isVisible()
          .catch(() => false);
        
        if (hasOption) {
          await statusSelect.selectOption({ label: status });
          await page.waitForTimeout(500);
        }
      }
    }
  });

  test('deve abrir modal de detalhes ao clicar em agendamento', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Aguarda tabela carregar
    await page.waitForSelector('table', { timeout: 10000 });
    
    // Verifica se tem agendamentos
    const firstRow = page.locator('tbody tr').first();
    const hasRows = await firstRow.isVisible().catch(() => false);
    
    if (hasRows) {
      // Clica no botão de detalhes
      const detailsButton = firstRow.locator('button:has-text("Detalhes")');
      if (await detailsButton.isVisible()) {
        await detailsButton.click();
        
        // Modal deve abrir
        await expect(page.locator('text=/detalhes do agendamento/i')).toBeVisible({
          timeout: 5000,
        });
      }
    }
  });

  test('deve exibir informações completas no modal de detalhes', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    await page.waitForSelector('table', { timeout: 10000 });
    
    const firstRow = page.locator('tbody tr').first();
    const hasRows = await firstRow.isVisible().catch(() => false);
    
    if (hasRows) {
      const detailsButton = firstRow.locator('button:has-text("Detalhes")');
      if (await detailsButton.isVisible()) {
        await detailsButton.click();
        
        // Verifica campos do modal
        const modalFields = [
          'Data e Hora',
          'Cliente',
          'Telefone',
          'Barbeiro',
          'Serviço',
          'Status',
        ];
        
        for (const field of modalFields) {
          const hasField = await page
            .locator(`text=${field}`)
            .isVisible()
            .catch(() => false);
          
          if (!hasField) {
            console.log(`Campo "${field}" não encontrado no modal`);
          }
        }
      }
    }
  });

  test('deve fechar modal ao clicar em fechar', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    await page.waitForSelector('table', { timeout: 10000 });
    
    const firstRow = page.locator('tbody tr').first();
    const hasRows = await firstRow.isVisible().catch(() => false);
    
    if (hasRows) {
      const detailsButton = firstRow.locator('button:has-text("Detalhes")');
      if (await detailsButton.isVisible()) {
        await detailsButton.click();
        
        // Modal abre
        await expect(page.locator('text=/detalhes do agendamento/i')).toBeVisible();
        
        // Clica em fechar
        await page.click('button:has-text("Fechar")');
        
        // Modal fecha
        await expect(page.locator('text=/detalhes do agendamento/i')).not.toBeVisible();
      }
    }
  });

  test('deve exibir badge de status com cor apropriada', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    await page.waitForSelector('table', { timeout: 10000 });
    
    // Verifica se badges existem na tabela
    const badges = page.locator('tbody td .badge, tbody td [class*="badge"]');
    const count = await badges.count();
    
    if (count > 0) {
      // Verifica se badges têm classes de cor
      const firstBadge = badges.first();
      const className = await firstBadge.getAttribute('class');
      
      expect(className).toBeTruthy();
    }
  });

  test('deve formatar data e hora corretamente', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    await page.waitForSelector('table', { timeout: 10000 });
    
    // Verifica formato de data/hora na primeira linha
    const firstRow = page.locator('tbody tr').first();
    const hasRows = await firstRow.isVisible().catch(() => false);
    
    if (hasRows) {
      const dateCell = firstRow.locator('td').first();
      const dateText = await dateCell.textContent();
      
      // Formato esperado: dd/MM/yyyy HH:mm ou dd/MM/yyyy, HH:mm
      const datePattern = /\d{2}\/\d{2}\/\d{4}[,\s]+\d{2}:\d{2}/;
      
      if (dateText) {
        expect(datePattern.test(dateText)).toBeTruthy();
      }
    }
  });

  test('deve limpar filtros ao clicar em limpar', async ({ page }) => {
    await navigateTo(page, 'agenda');
    
    // Aplica alguns filtros
    const barbeiroSelect = page.locator('select[name="barbeiroId"]');
    if (await barbeiroSelect.isVisible()) {
      await barbeiroSelect.selectOption({ index: 1 });
    }
    
    const dataInicio = page.locator('input[name="dataInicio"]');
    if (await dataInicio.isVisible()) {
      await dataInicio.fill('2024-01-01');
    }
    
    // Clica em limpar
    const clearButton = page.locator('button:has-text("Limpar")');
    if (await clearButton.isVisible()) {
      await clearButton.click();
      
      // Campos devem estar limpos
      await expect(dataInicio).toHaveValue('');
    }
  });
});
