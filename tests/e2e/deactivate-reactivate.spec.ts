import { test, expect } from '@playwright/test';

test.describe('Deactivate/Reactivate Functionality', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto('/');

    // Mock the API responses for testing
    await page.route('**/api/barbearias*', async (route) => {
      const url = route.request().url();

      if (url.includes('/desativar')) {
        // Mock deactivate response
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({}),
        });
      } else if (url.includes('/reativar')) {
        // Mock reactivate response
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({}),
        });
      } else if (url.includes('?')) {
        // Mock list response with pagination
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: '1',
                name: 'Barbearia Teste',
                code: 'TEST123',
                document: '12.345.678/0001-90',
                phone: '(11) 99999-9999',
                ownerName: 'João Silva',
                email: 'teste@email.com',
                address: {
                  street: 'Rua Teste',
                  number: '123',
                  neighborhood: 'Centro',
                  city: 'São Paulo',
                  state: 'SP',
                  zipCode: '01000-000',
                },
                isActive: true,
                createdAt: '2024-01-01T00:00:00Z',
                updatedAt: '2024-01-01T00:00:00Z',
              },
              {
                id: '2',
                name: 'Barbearia Inativa',
                code: 'INAT456',
                document: '98.765.432/0001-10',
                phone: '(11) 88888-7777',
                ownerName: 'Maria Silva',
                email: 'inativa@email.com',
                address: {
                  street: 'Rua Inativa',
                  number: '456',
                  neighborhood: 'Bairro',
                  city: 'São Paulo',
                  state: 'SP',
                  zipCode: '02000-000',
                },
                isActive: false,
                createdAt: '2024-01-01T00:00:00Z',
                updatedAt: '2024-01-01T00:00:00Z',
              },
            ],
            pageNumber: 1,
            pageSize: 20,
            totalCount: 2,
            totalPages: 1,
            hasPreviousPage: false,
            hasNextPage: false,
          }),
        });
      } else {
        // Default list response
        await route.fulfill({
          status: 200,
          contentType: 'application/json',
          body: JSON.stringify({
            items: [
              {
                id: '1',
                name: 'Barbearia Teste',
                code: 'TEST123',
                document: '12.345.678/0001-90',
                phone: '(11) 99999-9999',
                ownerName: 'João Silva',
                email: 'teste@email.com',
                address: {
                  street: 'Rua Teste',
                  number: '123',
                  neighborhood: 'Centro',
                  city: 'São Paulo',
                  state: 'SP',
                  zipCode: '01000-000',
                },
                isActive: true,
                createdAt: '2024-01-01T00:00:00Z',
                updatedAt: '2024-01-01T00:00:00Z',
              },
            ],
            pageNumber: 1,
            pageSize: 20,
            totalCount: 1,
            totalPages: 1,
            hasPreviousPage: false,
            hasNextPage: false,
          }),
        });
      }
    });

    // Mock individual barbershop fetch
    await page.route('**/api/barbearias/1', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '1',
          name: 'Barbearia Teste',
          code: 'TEST123',
          document: '12.345.678/0001-90',
          phone: '(11) 99999-9999',
          ownerName: 'João Silva',
          email: 'teste@email.com',
          address: {
            street: 'Rua Teste',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        }),
      });
    });
  });

  test('should deactivate barbershop from list page', async ({ page }) => {
    // Navigate to barbershops list
    await page.goto('/barbearias');

    // Wait for the table to load
    await page.waitForSelector('table');

    // Find the deactivate button for the active barbershop
    const deactivateButton = page.locator('button').filter({ hasText: 'Desativar' }).first();
    await expect(deactivateButton).toBeVisible();

    // Click deactivate button
    await deactivateButton.click();

    // Check if modal opens
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();

    // Check modal content
    await expect(page.getByText('Confirmar Desativação')).toBeVisible();
    await expect(page.getByText(/Barbearia Teste/)).toBeVisible();
    await expect(page.getByText(/TEST123/)).toBeVisible();

    // Click confirm button
    const confirmButton = page.getByRole('button', { name: 'Confirmar Desativação' });
    await confirmButton.click();

    // Check if success toast appears
    await expect(page.getByText('Barbearia desativada com sucesso!')).toBeVisible();

    // Modal should close
    await expect(modal).not.toBeVisible();
  });

  test('should reactivate barbershop from list page', async ({ page }) => {
    // Navigate to barbershops list
    await page.goto('/barbearias');

    // Wait for the table to load
    await page.waitForSelector('table');

    // Find the reactivate button for the inactive barbershop
    const reactivateButton = page.locator('button').filter({ hasText: 'Reativar' });
    await expect(reactivateButton).toBeVisible();

    // Click reactivate button
    await reactivateButton.click();

    // Check if modal opens
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();

    // Check modal content
    await expect(page.getByText('Confirmar Reativação')).toBeVisible();
    await expect(page.getByText(/Barbearia Inativa/)).toBeVisible();
    await expect(page.getByText(/INAT456/)).toBeVisible();

    // Click confirm button
    const confirmButton = page.getByRole('button', { name: 'Confirmar Reativação' });
    await confirmButton.click();

    // Check if success toast appears
    await expect(page.getByText('Barbearia reativada com sucesso!')).toBeVisible();

    // Modal should close
    await expect(modal).not.toBeVisible();
  });

  test('should deactivate barbershop from details page', async ({ page }) => {
    // Navigate to barbershop details
    await page.goto('/barbearias/1');

    // Wait for the page to load
    await page.waitForSelector('h1');

    // Check if deactivate button is visible
    const deactivateButton = page.getByRole('button', { name: 'Desativar' });
    await expect(deactivateButton).toBeVisible();

    // Click deactivate button
    await deactivateButton.click();

    // Check if success toast appears (no modal on details page)
    await expect(page.getByText('Barbearia desativada com sucesso!')).toBeVisible();

    // Button should change to "Reativar"
    const reactivateButton = page.getByRole('button', { name: 'Reativar' });
    await expect(reactivateButton).toBeVisible();
  });

  test('should reactivate barbershop from details page', async ({ page }) => {
    // Mock inactive barbershop for details page
    await page.route('**/api/barbearias/1', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'application/json',
        body: JSON.stringify({
          id: '1',
          name: 'Barbearia Teste',
          code: 'TEST123',
          document: '12.345.678/0001-90',
          phone: '(11) 99999-9999',
          ownerName: 'João Silva',
          email: 'teste@email.com',
          address: {
            street: 'Rua Teste',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
          },
          isActive: false, // Inactive
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        }),
      });
    });

    // Navigate to barbershop details
    await page.goto('/barbearias/1');

    // Wait for the page to load
    await page.waitForSelector('h1');

    // Check if reactivate button is visible
    const reactivateButton = page.getByRole('button', { name: 'Reativar' });
    await expect(reactivateButton).toBeVisible();

    // Click reactivate button
    await reactivateButton.click();

    // Check if success toast appears
    await expect(page.getByText('Barbearia reativada com sucesso!')).toBeVisible();

    // Button should change to "Desativar"
    const deactivateButton = page.getByRole('button', { name: 'Desativar' });
    await expect(deactivateButton).toBeVisible();
  });

  test('should handle deactivate error', async ({ page }) => {
    // Mock error response for deactivate
    await page.route('**/api/barbearias/1/desativar', async (route) => {
      await route.fulfill({
        status: 400,
        contentType: 'application/json',
        body: JSON.stringify({ message: 'Erro ao desativar barbearia' }),
      });
    });

    // Navigate to barbershops list
    await page.goto('/barbearias');

    // Wait for the table to load
    await page.waitForSelector('table');

    // Click deactivate button
    const deactivateButton = page.locator('button').filter({ hasText: 'Desativar' }).first();
    await deactivateButton.click();

    // Click confirm in modal
    const confirmButton = page.getByRole('button', { name: 'Confirmar Desativação' });
    await confirmButton.click();

    // Check if error toast appears
    await expect(page.getByText('Erro ao desativar barbearia')).toBeVisible();

    // Modal should remain open
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();
  });

  test('should cancel deactivate modal', async ({ page }) => {
    // Navigate to barbershops list
    await page.goto('/barbearias');

    // Wait for the table to load
    await page.waitForSelector('table');

    // Click deactivate button
    const deactivateButton = page.locator('button').filter({ hasText: 'Desativar' }).first();
    await deactivateButton.click();

    // Check if modal opens
    const modal = page.locator('[role="dialog"]');
    await expect(modal).toBeVisible();

    // Click cancel button
    const cancelButton = page.getByRole('button', { name: 'Cancelar' });
    await cancelButton.click();

    // Modal should close
    await expect(modal).not.toBeVisible();

    // No API call should be made (we can check this by ensuring no error/success toasts)
    await expect(page.getByText('Barbearia desativada com sucesso!')).not.toBeVisible();
  });
});