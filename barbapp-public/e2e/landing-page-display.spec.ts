import { test, expect } from '@playwright/test';

test.describe('Landing Page Display', () => {
  test('should display barbershop information correctly', async ({ page }) => {
    // Mock the API response
    const mockResponse = {
      barbershop: {
        id: '123',
        name: 'Barbearia do João',
        code: 'XYZ123AB',
        address: 'Rua das Flores, 123 - Centro, São Paulo - SP'
      },
      landingPage: {
        templateId: 1,
        logoUrl: 'https://example.com/logo.png',
        aboutText: 'Bem-vindo à nossa barbearia',
        openingHours: 'Segunda a Sexta: 09:00 - 19:00',
        instagramUrl: 'https://instagram.com/barbearia',
        facebookUrl: 'https://facebook.com/barbearia',
        whatsappNumber: '+5511999999999',
        services: [
          {
            id: '1',
            name: 'Corte Social',
            description: 'Corte completo',
            duration: 30,
            price: 35.00
          },
          {
            id: '2',
            name: 'Barba',
            description: 'Aparar barba',
            duration: 20,
            price: 25.00
          }
        ]
      }
    };

    await page.route('**/api/public/barbershops/XYZ123AB/landing-page', async route => {
      await route.fulfill({ json: mockResponse });
    });

    await page.goto('/barbearia/XYZ123AB');

    // Check if barbershop name is displayed in the title
    await expect(page.locator('h1')).toContainText('Barbearia do João');

    // Check if services section is visible
    await expect(page.locator('#servicos')).toBeVisible();

    // Check if WhatsApp button is visible
    await expect(page.locator('[aria-label="Contato via WhatsApp"]')).toBeVisible();
  });
});