---
status: pending
parallelizable: false
blocked_by: ["29.0"]
---

# Tarefa 30.0: Testes E2E da Landing Page Pública

## Visão Geral
Criar testes end-to-end (E2E) para a aplicação `barbapp-public` usando Playwright. O objetivo é simular a jornada completa de um cliente, desde o acesso à página até a interação com seus principais recursos, garantindo que os fluxos críticos estejam funcionando como esperado em um ambiente que se aproxima do de produção.

## Requisitos
- **Ferramenta**: Utilizar Playwright para escrever e executar os testes.
- **Fluxos Críticos**: Os testes devem cobrir os seguintes cenários:
  1. Visualização da página e suas informações.
  2. Seleção de serviços e navegação para a página de agendamento.
  3. Interação com o botão do WhatsApp.
- **Mock de API**: Os testes E2E devem rodar contra um servidor de API mockado ou um ambiente de staging para garantir consistência e evitar dependência de um backend em desenvolvimento.

## Detalhes de Implementação (techspec-frontend.md Seção 6)

### Configuração do Playwright
- Instalar Playwright no projeto `barbapp-public`.
- Configurar o `playwright.config.ts` para definir a URL base de teste (ex: `baseURL: 'http://localhost:3001'`) e outras opções, como navegadores a serem testados.

### Cenários de Teste

1.  **Carregamento e Visualização da Página** (`e2e/landing-page-display.spec.ts`)
    - **Dado** que a API retorna sucesso para o código `XYZ123AB`.
    - **Quando** o usuário navega para `/barbearia/XYZ123AB`.
    - **Então** a página deve exibir o nome da barbearia (ex: "Barbearia do João") no título.
    - **E** a seção de serviços (`#servicos`) deve estar visível.
    - **E** o botão do WhatsApp deve estar visível.

2.  **Seleção de Serviços e Agendamento** (`e2e/booking-flow.spec.ts`)
    - **Dado** que o usuário está na página `/barbearia/XYZ123AB`.
    - **Quando** ele clica no card do serviço com `data-service-id="1"`.
    - **E** clica no card do serviço com `data-service-id="2"`.
    - **Então** o botão flutuante de agendamento deve aparecer com o texto "Agendar 2 serviços".
    - **Quando** ele clica neste botão.
    - **Então** a URL da página deve mudar para `/barbearia/XYZ123AB/agendar?servicos=1,2` (ou o formato de URL correspondente).

3.  **Interação com o WhatsApp** (`e2e/whatsapp-interaction.spec.ts`)
    - **Dado** que o usuário está na página `/barbearia/XYZ123AB`.
    - **Quando** ele clica no botão flutuante do WhatsApp (`[aria-label="Contato via WhatsApp"]`).
    - **Então** uma nova aba ou janela do navegador deve ser aberta.
    - **E** a URL da nova página deve conter `https://wa.me/...`.

## Exemplo de Código de Teste (Playwright)
```typescript
// e2e/booking-flow.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Landing Page Booking Flow', () => {
  test('should select services and navigate to booking page', async ({ page }) => {
    // Mock da resposta da API antes de navegar
    await page.route('**/api/public/barbershops/XYZ123AB/landing-page', async route => {
      const json = { /* ... mock da resposta da API ... */ };
      await route.fulfill({ json });
    });

    await page.goto('/barbearia/XYZ123AB');

    // Clica nos serviços
    await page.locator('[data-testid="service-card-1"]').click();
    await page.locator('[data-testid="service-card-2"]').click();

    // Verifica o botão flutuante
    const scheduleButton = page.locator('button:has-text("Agendar 2 serviços")');
    await expect(scheduleButton).toBeVisible();

    // Clica para agendar
    await scheduleButton.click();

    // Verifica a navegação
    await expect(page).toHaveURL(/.*\/agendar\?servicos=1,2/);
  });
});
```

## Critérios de Aceitação
- [ ] Playwright está instalado e configurado no projeto `barbapp-public`.
- [ ] O teste de visualização da página passa com sucesso.
- [ ] O teste de seleção de serviços e navegação para o agendamento passa com sucesso.
- [ ] O teste de interação com o botão do WhatsApp passa com sucesso.
- [ ] Os testes rodam de forma confiável em um ambiente de CI (se aplicável).