import { test, expect } from '@playwright/test';

test.describe('Basic E2E Test', () => {
  test('should load the application', async ({ page }) => {
    await page.goto('/');

    // Wait for the page to load
    await expect(page).toHaveTitle('barbapp-admin');
  });
});