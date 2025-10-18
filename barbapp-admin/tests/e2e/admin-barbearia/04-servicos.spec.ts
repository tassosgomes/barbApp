import { test, expect } from '@playwright/test';
import {
  loginAsAdminBarbearia,
  clearAuth,
  fillServicoForm,
  waitForSuccessToast,
  waitForErrorToast,
  navigateTo,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Gestão de Serviços', () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
    await loginAsAdminBarbearia(page);
  });

  test('deve listar serviços existentes', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    // Deve exibir título
    await expect(page.locator('h1:has-text("Serviços")')).toBeVisible();
    
    // Deve exibir tabela ou mensagem de vazio
    const hasTable = await page.locator('table').isVisible().catch(() => false);
    const hasEmptyMessage = await page
      .locator('text=/nenhum serviço/i')
      .isVisible()
      .catch(() => false);
    
    expect(hasTable || hasEmptyMessage).toBeTruthy();
  });

  test('deve abrir modal de criação de serviço', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    // Clica no botão de novo serviço
    await page.click('button:has-text("Novo Serviço")');
    
    // Modal deve abrir
    await expect(page.locator('text=/cadastrar serviço/i')).toBeVisible();
    await expect(page.locator('input[name="nome"]')).toBeVisible();
    await expect(page.locator('input[name="preco"]')).toBeVisible();
    await expect(page.locator('input[name="duracao"]')).toBeVisible();
  });

  test('deve criar novo serviço com sucesso', async ({ page }) => {
    await navigateTo(page, 'servicos');
    await page.click('button:has-text("Novo Serviço")');
    
    const servicoData = {
      nome: `Serviço Teste ${Date.now()}`,
      descricao: 'Descrição do serviço',
      preco: 50.00,
      duracaoMinutos: 60,
    };
    
    await fillServicoForm(page, servicoData);
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir toast de sucesso
    await waitForSuccessToast(page);
    
    // Modal deve fechar
    await expect(page.locator('text=/cadastrar serviço/i')).not.toBeVisible();
    
    // Serviço deve aparecer na lista
    await expect(page.locator(`text=${servicoData.nome}`)).toBeVisible();
  });

  test('deve validar campos obrigatórios ao criar serviço', async ({ page }) => {
    await navigateTo(page, 'servicos');
    await page.click('button:has-text("Novo Serviço")');
    
    // Tenta salvar sem preencher
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir mensagens de validação
    const hasValidationError = await Promise.race([
      page.locator('text=/campo obrigatório/i').isVisible(),
      page.locator('text=/preencha este campo/i').isVisible(),
      page.locator('.text-destructive').isVisible(),
    ]);
    
    expect(hasValidationError).toBeTruthy();
  });

  test('deve validar formato de preço', async ({ page }) => {
    await navigateTo(page, 'servicos');
    await page.click('button:has-text("Novo Serviço")');
    
    await fillServicoForm(page, {
      nome: 'Serviço Teste',
      preco: -10.00, // Preço negativo
      duracaoMinutos: 30,
    });
    
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir erro de validação
    const hasError = await Promise.race([
      page.locator('text=/preço.*inválido/i').isVisible(),
      page.locator('text=/deve ser maior que zero/i').isVisible(),
      waitForErrorToast(page),
    ]);
    
    expect(hasError).toBeTruthy();
  });

  test('deve validar duração mínima', async ({ page }) => {
    await navigateTo(page, 'servicos');
    await page.click('button:has-text("Novo Serviço")');
    
    await fillServicoForm(page, {
      nome: 'Serviço Teste',
      preco: 50.00,
      duracaoMinutos: 0, // Duração zero
    });
    
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir erro de validação
    const hasError = await Promise.race([
      page.locator('text=/duração.*inválida/i').isVisible(),
      page.locator('text=/deve ser maior que zero/i').isVisible(),
      waitForErrorToast(page),
    ]);
    
    expect(hasError).toBeTruthy();
  });

  test('deve editar serviço existente', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    // Cria serviço primeiro
    await page.click('button:has-text("Novo Serviço")');
    const originalName = `Serviço Edit ${Date.now()}`;
    await fillServicoForm(page, {
      nome: originalName,
      preco: 50.00,
      duracaoMinutos: 60,
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Localiza e edita o serviço
    const servicoRow = page.locator(`tr:has-text("${originalName}")`);
    await servicoRow.locator('button[aria-label="Editar"]').click();
    
    // Modal de edição deve abrir
    await expect(page.locator('text=/editar serviço/i')).toBeVisible();
    
    // Modifica nome e preço
    const newName = `${originalName} Editado`;
    await page.fill('input[name="nome"]', newName);
    await page.fill('input[name="preco"]', '75.00');
    await page.click('button:has-text("Salvar")');
    
    await waitForSuccessToast(page);
    
    // Nome e preço atualizados devem aparecer
    await expect(page.locator(`text=${newName}`)).toBeVisible();
    await expect(page.locator('text=R$ 75,00')).toBeVisible();
  });

  test('deve desativar serviço', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    // Cria serviço
    await page.click('button:has-text("Novo Serviço")');
    const servicoName = `Serviço Deactivate ${Date.now()}`;
    await fillServicoForm(page, {
      nome: servicoName,
      preco: 50.00,
      duracaoMinutos: 60,
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Localiza serviço e desativa
    const servicoRow = page.locator(`tr:has-text("${servicoName}")`);
    await servicoRow.locator('button[aria-label="Desativar"]').click();
    
    // Confirma desativação no modal
    await page.click('button:has-text("Confirmar")');
    
    await waitForSuccessToast(page);
    
    // Serviço deve ter status inativo
    await expect(servicoRow.locator('text=/inativo/i')).toBeVisible();
  });

  test('deve filtrar serviços por nome', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    // Cria dois serviços
    const servico1 = `Serviço Filter Alpha ${Date.now()}`;
    const servico2 = `Serviço Filter Beta ${Date.now()}`;
    
    for (const nome of [servico1, servico2]) {
      await page.click('button:has-text("Novo Serviço")');
      await fillServicoForm(page, {
        nome,
        preco: 50.00,
        duracaoMinutos: 60,
      });
      await page.click('button:has-text("Salvar")');
      await waitForSuccessToast(page);
    }
    
    // Filtra por "Alpha"
    const searchInput = page.locator('input[placeholder*="Buscar"]');
    if (await searchInput.isVisible()) {
      await searchInput.fill('Alpha');
      
      // Deve mostrar apenas servico1
      await expect(page.locator(`text=${servico1}`)).toBeVisible();
      await expect(page.locator(`text=${servico2}`)).not.toBeVisible();
    }
  });

  test('deve exibir preço formatado em reais', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    await page.click('button:has-text("Novo Serviço")');
    await fillServicoForm(page, {
      nome: `Serviço Price ${Date.now()}`,
      preco: 123.45,
      duracaoMinutos: 60,
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Deve exibir preço formatado com vírgula
    await expect(page.locator('text=R$ 123,45')).toBeVisible();
  });

  test('deve exibir duração formatada', async ({ page }) => {
    await navigateTo(page, 'servicos');
    
    await page.click('button:has-text("Novo Serviço")');
    await fillServicoForm(page, {
      nome: `Serviço Duration ${Date.now()}`,
      preco: 50.00,
      duracaoMinutos: 90,
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Deve exibir duração formatada
    await expect(page.locator('text=/90.*min/i')).toBeVisible();
  });
});
