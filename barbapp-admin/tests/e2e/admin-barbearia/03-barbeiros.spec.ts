import { test, expect } from '@playwright/test';
import {
  loginAsAdminBarbearia,
  clearAuth,
  fillBarbeiroForm,
  waitForSuccessToast,
  waitForErrorToast,
  navigateTo,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Gestão de Barbeiros', () => {
  test.beforeEach(async ({ page }) => {
    await clearAuth(page);
    await loginAsAdminBarbearia(page);
  });

  test('deve listar barbeiros existentes', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    // Deve exibir título
    await expect(page.locator('h1:has-text("Barbeiros")')).toBeVisible();
    
    // Deve exibir tabela ou mensagem de vazio
    const hasTable = await page.locator('table').isVisible().catch(() => false);
    const hasEmptyMessage = await page
      .locator('text=/nenhum barbeiro/i')
      .isVisible()
      .catch(() => false);
    
    expect(hasTable || hasEmptyMessage).toBeTruthy();
  });

  test('deve abrir modal de criação de barbeiro', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    // Clica no botão de novo barbeiro
    await page.click('button:has-text("Novo Barbeiro")');
    
    // Modal deve abrir
    await expect(page.locator('text=/cadastrar barbeiro/i')).toBeVisible();
    await expect(page.locator('input[name="nome"]')).toBeVisible();
    await expect(page.locator('input[name="email"]')).toBeVisible();
    await expect(page.locator('input[name="telefone"]')).toBeVisible();
  });

  test('deve criar novo barbeiro com sucesso', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    await page.click('button:has-text("Novo Barbeiro")');
    
    const barbeiroData = {
      nome: `Barbeiro Teste ${Date.now()}`,
      email: `barbeiro${Date.now()}@test.com`,
      telefone: '11999887766',
      especialidade: 'Cortes modernos',
    };
    
    await fillBarbeiroForm(page, barbeiroData);
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir toast de sucesso
    await waitForSuccessToast(page);
    
    // Modal deve fechar
    await expect(page.locator('text=/cadastrar barbeiro/i')).not.toBeVisible();
    
    // Barbeiro deve aparecer na lista
    await expect(page.locator(`text=${barbeiroData.nome}`)).toBeVisible();
  });

  test('deve validar campos obrigatórios ao criar barbeiro', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    await page.click('button:has-text("Novo Barbeiro")');
    
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

  test('deve editar barbeiro existente', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    // Cria barbeiro primeiro
    await page.click('button:has-text("Novo Barbeiro")');
    const originalName = `Barbeiro Edit ${Date.now()}`;
    await fillBarbeiroForm(page, {
      nome: originalName,
      email: `edit${Date.now()}@test.com`,
      telefone: '11999887766',
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Localiza e edita o barbeiro
    const barbeiroRow = page.locator(`tr:has-text("${originalName}")`);
    await barbeiroRow.locator('button[aria-label="Editar"]').click();
    
    // Modal de edição deve abrir
    await expect(page.locator('text=/editar barbeiro/i')).toBeVisible();
    
    // Modifica nome
    const newName = `${originalName} Editado`;
    await page.fill('input[name="nome"]', newName);
    await page.click('button:has-text("Salvar")');
    
    await waitForSuccessToast(page);
    
    // Nome atualizado deve aparecer
    await expect(page.locator(`text=${newName}`)).toBeVisible();
  });

  test('deve desativar barbeiro', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    // Cria barbeiro
    await page.click('button:has-text("Novo Barbeiro")');
    const barbeiroName = `Barbeiro Deactivate ${Date.now()}`;
    await fillBarbeiroForm(page, {
      nome: barbeiroName,
      email: `deactivate${Date.now()}@test.com`,
      telefone: '11999887766',
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Localiza barbeiro e desativa
    const barbeiroRow = page.locator(`tr:has-text("${barbeiroName}")`);
    await barbeiroRow.locator('button[aria-label="Desativar"]').click();
    
    // Confirma desativação no modal
    await page.click('button:has-text("Confirmar")');
    
    await waitForSuccessToast(page);
    
    // Barbeiro deve ter status inativo
    await expect(barbeiroRow.locator('text=/inativo/i')).toBeVisible();
  });

  test('deve filtrar barbeiros por nome', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    // Cria dois barbeiros
    const barbeiro1 = `Barbeiro Filter Alpha ${Date.now()}`;
    const barbeiro2 = `Barbeiro Filter Beta ${Date.now()}`;
    
    for (const nome of [barbeiro1, barbeiro2]) {
      await page.click('button:has-text("Novo Barbeiro")');
      await fillBarbeiroForm(page, {
        nome,
        email: `${nome.toLowerCase().replace(/\s/g, '')}@test.com`,
        telefone: '11999887766',
      });
      await page.click('button:has-text("Salvar")');
      await waitForSuccessToast(page);
    }
    
    // Filtra por "Alpha"
    const searchInput = page.locator('input[placeholder*="Buscar"]');
    if (await searchInput.isVisible()) {
      await searchInput.fill('Alpha');
      
      // Deve mostrar apenas barbeiro1
      await expect(page.locator(`text=${barbeiro1}`)).toBeVisible();
      await expect(page.locator(`text=${barbeiro2}`)).not.toBeVisible();
    }
  });

  test('deve exibir erro ao criar barbeiro com email duplicado', async ({ page }) => {
    await navigateTo(page, 'barbeiros');
    
    const duplicateEmail = `duplicate${Date.now()}@test.com`;
    
    // Cria primeiro barbeiro
    await page.click('button:has-text("Novo Barbeiro")');
    await fillBarbeiroForm(page, {
      nome: 'Barbeiro Um',
      email: duplicateEmail,
      telefone: '11999887766',
    });
    await page.click('button:has-text("Salvar")');
    await waitForSuccessToast(page);
    
    // Tenta criar segundo com mesmo email
    await page.click('button:has-text("Novo Barbeiro")');
    await fillBarbeiroForm(page, {
      nome: 'Barbeiro Dois',
      email: duplicateEmail,
      telefone: '11999887766',
    });
    await page.click('button:has-text("Salvar")');
    
    // Deve exibir erro
    await waitForErrorToast(page);
  });
});
