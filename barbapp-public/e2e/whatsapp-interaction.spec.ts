import { test, expect } from '@playwright/test';

test.describe('WhatsApp Interaction', () => {
  test('should open WhatsApp when clicking the button', async ({ page, context }) => {
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
          }
        ]
      }
    };

    await page.route('**/api/public/barbershops/XYZ123AB/landing-page', async route => {
      await route.fulfill({ json: mockResponse });
    });

    await page.goto('/barbearia/XYZ123AB');

    // Click the WhatsApp button
    const [newPage] = await Promise.all([
      context.waitForEvent('page'),
      page.locator('[aria-label="Contato via WhatsApp"]').click()
    ]);

    // Check if new page opened with WhatsApp URL
    expect(newPage.url()).toContain('https://wa.me/');
    expect(newPage.url()).toContain('5511999999999');
  });
});