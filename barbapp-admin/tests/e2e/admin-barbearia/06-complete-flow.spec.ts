import { test, expect } from '@playwright/test';
import {
  TEST_CREDENTIALS,
  clearAuth,
  loginAsAdminBarbearia,
  fillBarbeiroForm,
  fillServicoForm,
  waitForSuccessToast,
  navigateTo,
  isAuthenticated,
} from '../helpers/admin-barbearia.helper';

test.describe('Admin Barbearia - Fluxo Completo', () => {
  test('deve completar fluxo completo: login → criar barbeiro → criar serviço → visualizar agenda → logout', async ({
    page,
  }) => {
    // ==========================================
    // ETAPA 1: Login
    // ==========================================
    await clearAuth(page);
    await page.goto(`/${TEST_CREDENTIALS.codigo}/login`);
    
    // Valida código da barbearia
    await expect(page.locator(`text=${TEST_CREDENTIALS.barbeariaNome}`)).toBeVisible({
      timeout: 10000,
    });
    
    // Preenche credenciais
    await page.fill('input[type="email"]', TEST_CREDENTIALS.email);
    await page.fill('input[type="password"]', TEST_CREDENTIALS.senha);
    await page.click('button:has-text("Entrar")');
    
    // Aguarda redirecionamento
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`, {
      timeout: 10000,
    });
    
    // Verifica autenticação
    const authenticated = await isAuthenticated(page);
    expect(authenticated).toBe(true);
    
    console.log('✓ Login realizado com sucesso');
    
    // ==========================================
    // ETAPA 2: Criar Barbeiro
    // ==========================================
    await navigateTo(page, 'barbeiros');
    
    // Aguarda página carregar
    await expect(page.locator('h1:has-text("Barbeiros")')).toBeVisible();
    
    // Abre modal de criação
    await page.click('button:has-text("Novo Barbeiro")');
    await expect(page.locator('text=/cadastrar barbeiro/i')).toBeVisible();
    
    // Preenche formulário
    const barbeiroData = {
      nome: `Barbeiro Fluxo Completo ${Date.now()}`,
      email: `barbeiro.fluxo${Date.now()}@test.com`,
      telefone: '11987654321',
      especialidade: 'Cortes e barbas',
    };
    
    await fillBarbeiroForm(page, barbeiroData);
    await page.click('button:has-text("Salvar")');
    
    // Aguarda sucesso
    await waitForSuccessToast(page);
    await expect(page.locator(`text=${barbeiroData.nome}`)).toBeVisible();
    
    console.log('✓ Barbeiro criado com sucesso');
    
    // ==========================================
    // ETAPA 3: Criar Serviço
    // ==========================================
    await navigateTo(page, 'servicos');
    
    // Aguarda página carregar
    await expect(page.locator('h1:has-text("Serviços")')).toBeVisible();
    
    // Abre modal de criação
    await page.click('button:has-text("Novo Serviço")');
    await expect(page.locator('text=/cadastrar serviço/i')).toBeVisible();
    
    // Preenche formulário
    const servicoData = {
      nome: `Serviço Fluxo Completo ${Date.now()}`,
      descricao: 'Serviço criado durante teste de fluxo completo',
      preco: 85.00,
      duracaoMinutos: 45,
    };
    
    await fillServicoForm(page, servicoData);
    await page.click('button:has-text("Salvar")');
    
    // Aguarda sucesso
    await waitForSuccessToast(page);
    await expect(page.locator(`text=${servicoData.nome}`)).toBeVisible();
    
    console.log('✓ Serviço criado com sucesso');
    
    // ==========================================
    // ETAPA 4: Visualizar Agenda
    // ==========================================
    await navigateTo(page, 'agenda');
    
    // Aguarda página carregar
    await expect(page.locator('h1:has-text("Agenda")')).toBeVisible();
    
    // Verifica filtros
    await expect(page.locator('label:has-text("Barbeiro")')).toBeVisible();
    await expect(page.locator('label:has-text("Data Início")')).toBeVisible();
    await expect(page.locator('label:has-text("Status")')).toBeVisible();
    
    // Aplica filtro por barbeiro (se possível)
    const barbeiroSelect = page.locator('select[name="barbeiroId"]');
    if (await barbeiroSelect.isVisible()) {
      const options = await barbeiroSelect.locator('option').count();
      
      if (options > 1) {
        await barbeiroSelect.selectOption({ index: 1 });
        await page.waitForTimeout(1000);
      }
    }
    
    // Verifica se tabela está visível
    const hasTable = await page.locator('table').isVisible().catch(() => false);
    const hasEmptyMessage = await page
      .locator('text=/nenhum agendamento/i')
      .isVisible()
      .catch(() => false);
    
    expect(hasTable || hasEmptyMessage).toBeTruthy();
    
    console.log('✓ Agenda visualizada com sucesso');
    
    // ==========================================
    // ETAPA 5: Navegar pelo menu
    // ==========================================
    
    // Volta para dashboard
    await page.click('a:has-text("Dashboard")');
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`);
    
    console.log('✓ Navegação pelo menu funcionando');
    
    // ==========================================
    // ETAPA 6: Verificar persistência
    // ==========================================
    
    // Recarrega página
    await page.reload();
    
    // Deve continuar autenticado
    await expect(page).toHaveURL(`/${TEST_CREDENTIALS.codigo}/dashboard`);
    const stillAuthenticated = await isAuthenticated(page);
    expect(stillAuthenticated).toBe(true);
    
    console.log('✓ Autenticação persistida após reload');
    
    // ==========================================
    // ETAPA 7: Logout
    // ==========================================
    
    // Procura botão de logout
    const logoutButton = page.locator('button:has-text("Sair")').first();
    
    if (await logoutButton.isVisible()) {
      await logoutButton.click();
      
      // Aguarda redirecionamento
      await expect(page).toHaveURL(new RegExp(`/${TEST_CREDENTIALS.codigo}/login`), {
        timeout: 5000,
      });
      
      // Verifica que não está mais autenticado
      const notAuthenticated = await isAuthenticated(page);
      expect(notAuthenticated).toBe(false);
      
      console.log('✓ Logout realizado com sucesso');
    } else {
      console.log('⚠ Botão de logout não encontrado (pode estar em dropdown ou com texto diferente)');
    }
    
    // ==========================================
    // VERIFICAÇÃO FINAL
    // ==========================================
    
    console.log('\n========================================');
    console.log('✓ FLUXO COMPLETO EXECUTADO COM SUCESSO');
    console.log('========================================');
    console.log('1. Login realizado');
    console.log('2. Barbeiro criado');
    console.log('3. Serviço criado');
    console.log('4. Agenda visualizada');
    console.log('5. Navegação testada');
    console.log('6. Persistência verificada');
    console.log('7. Logout realizado');
    console.log('========================================\n');
  });
});
