import { test, expect } from '@playwright/test';

/**
 * E2E Tests for Barbers Management Flow
 * 
 * Flow: Login → List Barbers → Create → Edit → Deactivate → Reactivate
 */

const BARBEARIA_CODE = 'TEST1234';
const BASE_URL = `/${BARBEARIA_CODE}`;

test.describe('Barbers Management', () => {
  test.beforeEach(async ({ page }) => {
    // TODO: Implement login flow
    // For now, we'll mock the authentication state
    await page.goto(`${BASE_URL}/barbeiros`);
  });

  test('should display barbers list page', async ({ page }) => {
    // Navigate to barbers list
    await page.goto(`${BASE_URL}/barbeiros`);

    // Verify page title
    await expect(page.locator('h1')).toContainText('Barbeiros');

    // Verify "Novo Barbeiro" button exists
    await expect(page.getByRole('button', { name: /novo barbeiro/i })).toBeVisible();

    // Verify search input exists
    await expect(page.getByPlaceholder(/buscar por nome ou email/i)).toBeVisible();

    // Verify status filter exists
    await expect(page.getByRole('combobox')).toBeVisible();
  });

  test('should navigate to create barber form', async ({ page }) => {
    await page.goto(`${BASE_URL}/barbeiros`);

    // Click "Novo Barbeiro" button
    await page.getByRole('button', { name: /novo barbeiro/i }).click();

    // Verify redirected to create form
    await expect(page).toHaveURL(`${BASE_URL}/barbeiros/novo`);

    // Verify form title
    await expect(page.locator('h1')).toContainText('Novo Barbeiro');

    // Verify form fields exist
    await expect(page.locator('input#nome')).toBeVisible();
    await expect(page.locator('input#email')).toBeVisible();
    await expect(page.locator('input#telefone')).toBeVisible();
    await expect(page.locator('input#senha')).toBeVisible();
  });

  test('should validate required fields on create form', async ({ page }) => {
    await page.goto(`${BASE_URL}/barbeiros/novo`);

    // Try to submit empty form
    await page.getByRole('button', { name: /cadastrar/i }).click();

    // Verify validation errors appear
    await expect(page.locator('text=/nome deve ter no mínimo/i')).toBeVisible();
    await expect(page.locator('text=/email inválido/i')).toBeVisible();
  });

  test('should format phone number automatically', async ({ page }) => {
    await page.goto(`${BASE_URL}/barbeiros/novo`);

    const phoneInput = page.locator('input#telefone');

    // Type phone number
    await phoneInput.fill('11987654321');

    // Verify formatted output
    await expect(phoneInput).toHaveValue('(11) 98765-4321');
  });

  test('should cancel creation and return to list', async ({ page }) => {
    await page.goto(`${BASE_URL}/barbeiros/novo`);

    // Click cancel button
    await page.getByRole('button', { name: /cancelar/i }).click();

    // Verify redirected back to list
    await expect(page).toHaveURL(`${BASE_URL}/barbeiros`);
  });

  test.skip('should create new barber successfully', async ({ page }) => {
    // Skip until backend is available in test environment
    await page.goto(`${BASE_URL}/barbeiros/novo`);

    // Fill form
    await page.locator('input#nome').fill('João Silva');
    await page.locator('input#email').fill('joao.silva@test.com');
    await page.locator('input#telefone').fill('11987654321');
    await page.locator('input#senha').fill('senha12345');

    // Select at least one service (assuming first checkbox)
    const firstServiceCheckbox = page.locator('input[type="checkbox"]').first();
    await firstServiceCheckbox.check();

    // Submit form
    await page.getByRole('button', { name: /cadastrar/i }).click();

    // Verify success toast appears
    await expect(page.locator('text=/barbeiro criado com sucesso/i')).toBeVisible();

    // Verify redirected to list
    await expect(page).toHaveURL(`${BASE_URL}/barbeiros`);

    // Verify new barber appears in list
    await expect(page.locator('text=João Silva')).toBeVisible();
  });

  test.skip('should edit existing barber', async ({ page }) => {
    // Skip until backend is available
    await page.goto(`${BASE_URL}/barbeiros`);

    // Click edit button on first barber
    await page.getByRole('button', { name: /editar/i }).first().click();

    // Verify redirected to edit form
    await expect(page).toHaveURL(new RegExp(`${BASE_URL}/barbeiros/[a-f0-9-]+`));

    // Verify form title
    await expect(page.locator('h1')).toContainText('Editar Barbeiro');

    // Verify email and password fields are NOT visible (edit mode)
    await expect(page.locator('input#email')).not.toBeVisible();
    await expect(page.locator('input#senha')).not.toBeVisible();

    // Modify name
    const nameInput = page.locator('input#nome');
    await nameInput.fill('João Silva Updated');

    // Submit form
    await page.getByRole('button', { name: /atualizar/i }).click();

    // Verify success toast
    await expect(page.locator('text=/barbeiro atualizado com sucesso/i')).toBeVisible();

    // Verify redirected to list
    await expect(page).toHaveURL(`${BASE_URL}/barbeiros`);

    // Verify updated name appears
    await expect(page.locator('text=João Silva Updated')).toBeVisible();
  });

  test.skip('should deactivate barber with confirmation', async ({ page }) => {
    // Skip until backend is available
    await page.goto(`${BASE_URL}/barbeiros`);

    // Click deactivate button on active barber
    await page.getByRole('button', { name: /desativar/i }).first().click();

    // Verify confirmation modal appears
    await expect(page.locator('text=/desativar barbeiro/i')).toBeVisible();
    await expect(page.locator('text=/tem certeza/i')).toBeVisible();

    // Confirm deactivation
    await page.getByRole('button', { name: /desativar/i }).last().click();

    // Verify success toast
    await expect(page.locator('text=/barbeiro desativado com sucesso/i')).toBeVisible();

    // Verify barber status changed to "Inativo"
    await expect(page.locator('text=Inativo').first()).toBeVisible();
  });

  test.skip('should reactivate barber', async ({ page }) => {
    // Skip until backend is available
    await page.goto(`${BASE_URL}/barbeiros`);

    // Filter by inactive
    await page.getByRole('combobox').click();
    await page.getByRole('option', { name: /inativos/i }).click();

    // Click reactivate button
    await page.getByRole('button', { name: /reativar/i }).first().click();

    // Verify success toast
    await expect(page.locator('text=/barbeiro reativado com sucesso/i')).toBeVisible();

    // Verify barber status changed to "Ativo"
    await expect(page.locator('text=Ativo').first()).toBeVisible();
  });

  test.skip('should filter barbers by status', async ({ page }) => {
    // Skip until backend is available
    await page.goto(`${BASE_URL}/barbeiros`);

    // Open status filter
    await page.getByRole('combobox').click();

    // Select "Ativos"
    await page.getByRole('option', { name: /ativos/i }).click();

    // Verify only active barbers are shown
    await expect(page.locator('text=Ativo')).toHaveCount(await page.locator('text=Ativo').count());
    await expect(page.locator('text=Inativo')).toHaveCount(0);
  });

  test.skip('should search barbers by name', async ({ page }) => {
    // Skip until backend is available
    await page.goto(`${BASE_URL}/barbeiros`);

    // Type in search box
    await page.getByPlaceholder(/buscar por nome ou email/i).fill('João');

    // Wait for debounce (300ms)
    await page.waitForTimeout(350);

    // Verify only matching barbers are shown
    await expect(page.locator('text=João')).toBeVisible();
  });
});
