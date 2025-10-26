---
status: pending
parallelizable: true
blocked_by: ["14.0","15.0"]
---

<task_context>
<domain>engine/frontend/testing</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies></dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 17.0: Testes End-to-End - Fluxos Completos de Agendamento

## Visão Geral
Implementar testes E2E com Playwright para validar fluxos completos do sistema de agendamentos do barbeiro, incluindo visualização, ações e troca de contexto multi-barbearia.

## Requisitos
- Testes E2E com Playwright
- Cobertura de fluxos principais do PRD
- Testes de integração com backend (mock ou dev)
- Validação de UI responsiva (mobile/desktop)
- Testes de isolamento multi-tenant

## Subtarefas
- [ ] 17.1 Setup Playwright e fixtures para autenticação de barbeiro
- [ ] 17.2 Teste E2E: Login e visualização de agenda
  - Login como barbeiro
  - Verificar carregamento da agenda do dia
  - Verificar exibição de agendamentos
- [ ] 17.3 Teste E2E: Confirmar agendamento
  - Clicar em agendamento pendente
  - Confirmar agendamento
  - Verificar mudança de status visual
  - Verificar toast de sucesso
- [ ] 17.4 Teste E2E: Cancelar agendamento
  - Abrir modal de confirmação
  - Confirmar cancelamento
  - Verificar status atualizado
- [ ] 17.5 Teste E2E: Concluir agendamento
  - Verificar que botão só aparece após horário
  - Concluir agendamento
  - Verificar status final
- [ ] 17.6 Teste E2E: Navegação entre dias
  - Clicar em "Próximo dia"
  - Verificar atualização de data
  - Verificar carregamento de novos dados
  - Voltar para "Hoje"
- [ ] 17.7 Teste E2E: Troca de contexto (multi-barbearia)
  - Barbeiro com múltiplas barbearias
  - Selecionar outra barbearia
  - Verificar atualização da agenda
  - Verificar isolamento de dados
- [ ] 17.8 Teste E2E: Polling e atualização automática
  - Aguardar 10 segundos
  - Verificar que dados foram atualizados
  - Simular novo agendamento no backend
  - Verificar aparecimento na agenda
- [ ] 17.9 Teste E2E: Tratamento de erros
  - Simular erro 404
  - Simular erro 409 (conflito)
  - Verificar mensagens de erro apropriadas
- [ ] 17.10 Teste E2E: Responsividade mobile
  - Executar testes em viewport mobile
  - Verificar pull-to-refresh
  - Verificar botões touch-friendly
- [ ] 17.11 Teste de isolamento multi-tenant
  - Barbeiro A não vê agendamentos de Barbeiro B
  - Dados de Barbearia X não aparecem ao selecionar Barbearia Y

## Sequenciamento
- Bloqueado por: 14.0 (Componentes), 15.0 (Página)
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação

**Estrutura de Testes:**
```typescript
// tests/e2e/barber-schedule.spec.ts
import { test, expect } from '@playwright/test';
import { loginAsBarber, mockScheduleData } from './helpers';

test.describe('Barber Schedule - Visualização', () => {
  test.beforeEach(async ({ page }) => {
    await loginAsBarber(page, { phone: '+5511999999999' });
  });
  
  test('deve exibir agenda do dia atual', async ({ page }) => {
    await page.goto('/barber/schedule');
    
    // Verificar header com data de hoje
    const today = new Date().toLocaleDateString('pt-BR', { 
      weekday: 'long', 
      day: 'numeric', 
      month: 'long' 
    });
    await expect(page.getByText(today)).toBeVisible();
    
    // Verificar lista de agendamentos
    await expect(page.getByTestId('appointments-list')).toBeVisible();
  });
});

test.describe('Barber Schedule - Ações', () => {
  test('deve confirmar agendamento pendente', async ({ page }) => {
    await page.goto('/barber/schedule');
    
    // Encontrar agendamento pendente
    const pendingCard = page.locator('[data-status="pending"]').first();
    await pendingCard.click();
    
    // Clicar em confirmar no modal
    await page.getByRole('button', { name: /confirmar/i }).click();
    
    // Verificar toast de sucesso
    await expect(page.getByText(/agendamento confirmado/i)).toBeVisible();
    
    // Verificar mudança visual do status
    await expect(pendingCard).toHaveAttribute('data-status', 'confirmed');
  });
  
  test('deve cancelar agendamento com confirmação', async ({ page }) => {
    await page.goto('/barber/schedule');
    
    const appointment = page.locator('[data-status="confirmed"]').first();
    await appointment.click();
    
    // Clicar em cancelar
    await page.getByRole('button', { name: /cancelar/i }).click();
    
    // Confirmar no dialog
    await page.getByRole('button', { name: /confirmar cancelamento/i }).click();
    
    // Verificar sucesso
    await expect(page.getByText(/agendamento cancelado/i)).toBeVisible();
  });
});

test.describe('Barber Schedule - Multi-tenant', () => {
  test('deve trocar de barbearia e atualizar agenda', async ({ page }) => {
    await loginAsBarber(page, { 
      phone: '+5511999999999',
      barbershops: ['barbearia-1', 'barbearia-2']
    });
    
    await page.goto('/barber/schedule');
    
    // Verificar barbearia atual
    await expect(page.getByTestId('barbershop-selector')).toContainText('Barbearia A');
    
    // Trocar para outra barbearia
    await page.getByTestId('barbershop-selector').click();
    await page.getByRole('menuitem', { name: /barbearia b/i }).click();
    
    // Verificar atualização do contexto
    await expect(page.getByTestId('barbershop-selector')).toContainText('Barbearia B');
    
    // Verificar que agenda foi recarregada
    await expect(page.getByTestId('appointments-list')).toBeVisible();
  });
  
  test('deve isolar dados entre barbearias', async ({ page }) => {
    // TODO: Implementar teste de isolamento
    // Verificar que agendamentos de uma barbearia não aparecem em outra
  });
});

test.describe('Barber Schedule - Polling', () => {
  test('deve atualizar agenda automaticamente', async ({ page }) => {
    await page.goto('/barber/schedule');
    
    const initialCount = await page.getByTestId('appointments-count').textContent();
    
    // Aguardar polling (10s + margem)
    await page.waitForTimeout(11000);
    
    // Verificar se houve atualização (mesmo que seja a mesma quantidade)
    // O importante é que a requisição foi feita
    const requests = page.context().routes();
    // Validar que requisição GET /schedule foi feita
  });
});
```

**Helpers e Fixtures:**
```typescript
// tests/e2e/helpers/auth.ts
export async function loginAsBarber(page: Page, options: {
  phone: string;
  barbershops?: string[];
}) {
  // Simular login e configurar token
  await page.goto('/login');
  await page.fill('[name="phone"]', options.phone);
  await page.click('button[type="submit"]');
  
  // Se múltiplas barbearias, selecionar a primeira
  if (options.barbershops && options.barbershops.length > 1) {
    await page.waitForURL('/barber/select-barbershop');
    await page.click(`[data-barbershop-id="${options.barbershops[0]}"]`);
  }
  
  await page.waitForURL('/barber/schedule');
}
```

**Configuração Playwright:**
- Testes em Chrome, Firefox, Safari
- Viewport mobile (375x667) e desktop (1280x720)
- Screenshots em falhas
- Vídeos para testes críticos

## Critérios de Sucesso
- Todos os testes E2E passam
- Cobertura dos fluxos principais do PRD
- Testes executam em <5 minutos
- Falhas geram screenshots/vídeos para debug
- Testes são estáveis (não flaky)
- Validação de isolamento multi-tenant funciona
- Testes mobile validam responsividade
- Documentação clara de como executar testes
- Segue `rules/tests-react.md`
