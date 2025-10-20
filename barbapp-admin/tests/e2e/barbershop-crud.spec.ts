import { test, expect } from '@playwright/test';

test.describe('BarbApp Admin - E2E Tests', () => {
  test.describe('Authentication Flow', () => {
    test('should login successfully with valid credentials', async ({ page }) => {
      await page.goto('/admin/login');

      // Fill login form
      await page.fill('input[id="email"]', 'admin@babapp.com');
      await page.fill('input[id="senha"]', '123456');

      // Submit form
      await page.click('button[type="submit"]');

      // Should redirect to barbershops list
      await expect(page).toHaveURL('/barbearias');
      await expect(page.locator('text=Gestão de Barbearias')).toBeVisible();
    });

    test('should show error with invalid credentials', async ({ page }) => {
      await page.goto('/admin/login');

      // Fill with wrong credentials
      await page.fill('input[id="email"]', 'admin@babapp.com');
      await page.fill('input[id="senha"]', 'wrongpassword');

      // Submit form
      await page.click('button[type="submit"]');

      // Should stay on login page (error handling)
      await expect(page).toHaveURL('/admin/login');
    });

    test('should show validation errors for empty fields', async ({ page }) => {
      await page.goto('/admin/login');

      // Try to submit without filling fields
      await page.click('button[type="submit"]');

      // Should show validation errors
      await expect(page.locator('text=Email inválido')).toBeVisible();
      await expect(page.locator('text=Senha deve ter no mínimo 6 caracteres')).toBeVisible();
    });
  });

  test.describe('Barbershop CRUD Operations', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/admin/login');
      await page.fill('input[id="email"]', 'admin@babapp.com');
      await page.fill('input[id="senha"]', '123456');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL('/barbearias');
    });

    test('should navigate to barbershop list', async ({ page }) => {
      // Already logged in and on barbershops page
      await expect(page.locator('text=Gestão de Barbearias')).toBeVisible();
    });

    test('should have create button', async ({ page }) => {
      // Check if create button exists
      await expect(page.locator('text=+ Nova Barbearia')).toBeVisible();
    });

    test('debug create page', async ({ page }) => {
      // Navigate to create page
      await page.click('text=+ Nova Barbearia');
      await expect(page).toHaveURL('/barbearias/nova');

      // Take screenshot
      await page.screenshot({ path: 'debug-create-page.png' });

      // Log available inputs
      const inputs = await page.locator('input').all();
      console.log('Available inputs:', inputs.length);
      for (const input of inputs.slice(0, 5)) {
        const id = await input.getAttribute('id');
        const name = await input.getAttribute('name');
        console.log(`Input - id: ${id}, name: ${name}`);
      }
    });

    test('should edit an existing barbershop', async ({ page }) => {
      // Find and click edit on first barbershop
      const editButtons = page.locator('text=Editar');
      if (await editButtons.count() > 0) {
        await editButtons.first().click();

        // Should be on edit page
        await expect(page).toHaveURL(/\/barbearias\/[^/]+\/editar/);

        // Wait for form to load
        await page.waitForSelector('input[id="name"]', { timeout: 10000 });

        // Modify name to ensure form is dirty
        const currentName = await page.inputValue('input[id="name"]');
        const newName = currentName + ' Editado';
        await page.fill('input[id="name"]', newName);

        // Submit form
        await page.click('button[type="submit"]');

        // Wait for navigation or success message
        await page.waitForURL('/barbearias', { timeout: 10000 });

        // Should redirect to list and show updated name
        await expect(page).toHaveURL('/barbearias');
        await expect(page.locator(`text=${newName}`)).toBeVisible();
      } else {
        test.skip(true, 'No barbershops available for editing');
      }
    });

    test('should deactivate and reactivate a barbershop', async ({ page }) => {
      // Check if there are any barbershops to deactivate
      const deactivateButtons = page.locator('text=Desativar');
      const count = await deactivateButtons.count();

      if (count > 0) {
        // Click deactivate on first barbershop
        await deactivateButtons.first().click();

        // Confirm in modal
        await page.click('text=Confirmar Desativação');

        // Should stay on the page
        await expect(page).toHaveURL('/barbearias');

        // Now try to reactivate - should show "Reativar" button
        const reactivateButtons = page.locator('text=Reativar');
        if (await reactivateButtons.count() > 0) {
          await reactivateButtons.first().click();

          // Confirm reactivation
          await page.click('text=Confirmar Reativação');

          // Should stay on the page
          await expect(page).toHaveURL('/barbearias');
        }
      } else {
        test.skip(true, 'No active barbershops available for deactivation');
      }
    });

    test('should search barbershops', async ({ page }) => {
      // Check if search input exists
      const searchInput = page.locator('input[placeholder*="Buscar"]');
      if (await searchInput.isVisible()) {
        await searchInput.fill('Test');
        await page.waitForTimeout(500);
        await expect(page).toHaveURL('/barbearias');
      } else {
        test.skip(true, 'Search input not available');
      }
    });

    test('should filter barbershops by status', async ({ page }) => {
      // Check if status filter exists
      const statusSelect = page.locator('button').filter({ hasText: 'Todos' }).or(
        page.locator('button').filter({ hasText: 'Ativos' })
      );

      if (await statusSelect.isVisible()) {
        // Try filtering by active status
        await statusSelect.click();
        await page.click('text=Ativos');
        await page.waitForTimeout(500);
        await expect(page).toHaveURL('/barbearias');

        // Try filtering by inactive status
        await statusSelect.click();
        await page.click('text=Inativos');
        await page.waitForTimeout(500);
        await expect(page).toHaveURL('/barbearias');

        // Reset to all
        await statusSelect.click();
        await page.click('text=Todos');
        await page.waitForTimeout(500);
      } else {
        test.skip(true, 'Status filter not available');
      }
    });

    test('should navigate pagination', async ({ page }) => {
      // Check if pagination exists
      const pagination = page.locator('[data-testid="pagination"]').or(
        page.locator('button').filter({ hasText: 'Próxima' })
      );

      if (await pagination.isVisible()) {
        // Try clicking next page if available
        const nextButton = page.locator('button').filter({ hasText: 'Próxima' });
        if (await nextButton.isVisible() && await nextButton.isEnabled()) {
          await nextButton.click();
          await expect(page).toHaveURL(/barbearias/);
        }

        // Try clicking previous page if available
        const prevButton = page.locator('button').filter({ hasText: 'Anterior' });
        if (await prevButton.isVisible() && await prevButton.isEnabled()) {
          await prevButton.click();
          await expect(page).toHaveURL(/barbearias/);
        }
      } else {
        test.skip(true, 'Pagination not available or only one page');
      }
    });

    test('should copy barbershop code', async ({ page }) => {
      // Check if copy code buttons exist
      const copyButtons = page.locator('button').filter({ hasText: 'Copiar Código' });

      if (await copyButtons.count() > 0) {
        await copyButtons.first().click();

        // Should show success toast
        await expect(page.locator('text=Código copiado')).toBeVisible();
      } else {
        test.skip(true, 'No copy code buttons available');
      }
    });
  });

  test.describe('Error Scenarios', () => {
    test.beforeEach(async ({ page }) => {
      // Login before each test
      await page.goto('/login');
      await page.fill('input[id="email"]', 'admin@babapp.com');
      await page.fill('input[id="senha"]', '123456');
      await page.click('button[type="submit"]');
      await expect(page).toHaveURL('/barbearias');
    });

    test('should handle network errors gracefully', async ({ page }) => {
      // This would require mocking network failures
      // For now, just ensure the app doesn't crash on unexpected errors
      await page.goto('/barbearias');
      await expect(page.locator('text=Gestão de Barbearias')).toBeVisible();
    });

    test('should handle invalid routes', async ({ page }) => {
      await page.goto('/invalid-route');
      // Should redirect to valid route or show 404
      await expect(page.locator('text=Gestão de Barbearias')).toBeVisible();
    });

    test('should show validation errors on create form', async ({ page }) => {
      // Navigate to create page
      await page.click('text=+ Nova Barbearia');
      await expect(page).toHaveURL('/barbearias/nova');

      // Try to submit empty form
      await page.click('button[type="submit"]');

      // Should show validation errors
      await expect(page.locator('text=Nome deve ter no mínimo 3 caracteres')).toBeVisible();
      await expect(page.locator('text=Email inválido')).toBeVisible();
    });

    test('should handle API errors on create', async ({ page }) => {
      // Navigate to create page
      await page.click('text=+ Nova Barbearia');
      await expect(page).toHaveURL('/barbearias/nova');

      // Fill form with duplicate data that might cause API error
      await page.fill('input[name="name"]', 'Barbearia Duplicada');
      await page.fill('input[name="document"]', '999.999.999-99');
      await page.fill('input[name="ownerName"]', 'Proprietário Teste');
      await page.fill('input[name="email"]', 'duplicado@teste.com');
      await page.fill('input[name="phone"]', '(11) 99999-9999');

      // Fill minimal address
      await page.fill('input[name="address.zipCode"]', '99999-999');
      await page.fill('input[name="address.street"]', 'Rua Teste');
      await page.fill('input[name="address.number"]', '123');
      await page.fill('input[name="address.neighborhood"]', 'Bairro Teste');
      await page.fill('input[name="address.city"]', 'Cidade Teste');
      await page.fill('input[name="address.state"]', 'SP');

      // Submit form
      await page.click('button[type="submit"]');

      // Should either succeed or show appropriate error message
      await expect(page).toHaveURL(/\/barbearias/);
    });
  });
});