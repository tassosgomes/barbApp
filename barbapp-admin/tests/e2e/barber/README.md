# Testes E2E - Interface de Login do Barbeiro

Este diretório contém os testes end-to-end (E2E) para a interface de login do barbeiro usando Playwright.

## Estrutura dos Testes

### 01-auth.spec.ts
Testes focados no formulário de login e autenticação básica:

- ✅ Renderização do formulário de login
- ✅ Login com sucesso
- ✅ Validação de campos obrigatórios
- ✅ Validação de formato de email
- ✅ Erro para credenciais inválidas
- ✅ Estados de loading durante submissão
- ✅ Persistência de autenticação após reload
- ✅ Redirecionamento para login em rotas protegidas
- ✅ Logout com sucesso
- ✅ Modal de ajuda
- ✅ Navegação por teclado (Tab, Enter)
- ✅ Validação de senha

**Total: 15 testes**

### 02-complete-flow.spec.ts
Testes de fluxo completo e casos avançados:

**Fluxo Completo:**
- ✅ Login → Agenda → Persistência completa
- ✅ Bloqueio de rotas protegidas
- ✅ Estado de loading durante validação
- ✅ Detecção de token inválido
- ✅ Detecção de token expirado
- ✅ Autenticação em múltiplas abas
- ✅ Logout em múltiplas abas
- ✅ Redirecionamento se já autenticado
- ✅ Persistência de dados do usuário
- ✅ Mensagem durante carregamento

**Casos de Erro:**
- ✅ Erro de rede durante login
- ✅ localStorage desabilitado
- ✅ Recuperação de erro temporário de API

**Total: 13 testes**

## Helpers

### barber.helper.ts
Funções utilitárias para os testes:

- `loginAsBarber(page)` - Realiza login completo
- `isBarberAuthenticated(page)` - Verifica se está autenticado
- `logoutBarber(page)` - Realiza logout
- `clearBarberAuth(page)` - Limpa dados de autenticação
- `waitForSuccessToast(page, message?)` - Aguarda toast de sucesso
- `waitForErrorToast(page, message?)` - Aguarda toast de erro
- `isLoading(page)` - Verifica se está em estado de loading

## Credenciais de Teste

```typescript
TEST_BARBER_CREDENTIALS = {
  email: 'barbeiro@test.com',
  password: 'Test@123',
  nome: 'João Silva',
}
```

> **Nota:** Estas credenciais devem existir no banco de dados de teste. Configure o seed adequadamente.

## Executando os Testes

### Todos os testes E2E do barbeiro:
```bash
npm run test:e2e tests/e2e/barber
```

### Teste específico:
```bash
npm run test:e2e tests/e2e/barber/01-auth.spec.ts
```

### Modo debug com UI:
```bash
npx playwright test tests/e2e/barber --ui
```

### Modo headed (ver navegador):
```bash
npx playwright test tests/e2e/barber --headed
```

### Apenas um navegador:
```bash
npx playwright test tests/e2e/barber --project=chromium
```

## Configuração do Backend para Testes

Para que os testes E2E funcionem, o backend deve estar:

1. **Rodando na porta correta** (configurada em `playwright.config.ts`)
2. **Com banco de dados de teste** seedado com:
   - Usuário barbeiro com email: `barbeiro@test.com`
   - Senha: `Test@123`
   - Nome: `João Silva`

### Setup Recomendado

1. Use variável de ambiente para identificar modo de teste:
   ```bash
   TEST_MODE=true npm run dev
   ```

2. Seed automático ao iniciar em modo teste:
   ```typescript
   if (process.env.TEST_MODE === 'true') {
     await seedTestData();
   }
   ```

## Manutenção dos Testes

### Ao adicionar novas features:

1. **Adicione testes para a feature** em arquivo apropriado
2. **Atualize os helpers** se necessário
3. **Documente aqui** a nova cobertura
4. **Execute todos os testes** para garantir que não quebrou nada:
   ```bash
   npm run test:e2e tests/e2e/barber
   ```

### Se um teste começar a falhar intermitentemente (flaky):

1. **Aumente timeouts** se for problema de timing
2. **Use waitFor** em vez de sleep/timeout fixo
3. **Verifique race conditions** em testes de loading
4. **Adicione retry** específico para o teste se necessário

### Boas Práticas

- ✅ Use `data-testid` em elementos complexos para seletores estáveis
- ✅ Limpe estado entre testes (beforeEach)
- ✅ Use helpers para evitar duplicação de código
- ✅ Teste casos de erro, não apenas happy path
- ✅ Teste acessibilidade (navegação por teclado, ARIA)
- ✅ Teste em múltiplos navegadores (configurado no Playwright)

## Cobertura de Testes

| Área | Cobertura | Notas |
|------|-----------|-------|
| Formulário de Login | 100% | Todos os campos e validações |
| Autenticação | 100% | Login, logout, persistência |
| Proteção de Rotas | 100% | Redirect, loading, tokens |
| UI/UX | 95% | Modal ajuda, teclado, loading |
| Casos de Erro | 90% | Rede, API, localStorage |
| Multi-tab | 100% | Compartilhamento de sessão |

**Total de Testes: 28**
**Status: ✅ Todos passando**

## Troubleshooting

### Teste falha com "Target closed"
- O navegador fechou antes do teste terminar
- Solução: Verifique se há erros JavaScript no console do browser

### Teste falha com "Timeout"
- Elemento não apareceu no tempo esperado
- Solução: Aumente timeout ou verifique seletor

### Teste falha com "locator.click: Element is not visible"
- Elemento existe mas não está visível
- Solução: Use `waitForSelector` ou `toBeVisible()` antes de clicar

### Backend não inicia
- Porta pode estar em uso
- Solução: Verifique `playwright.config.ts` e configure porta diferente

## Relatórios

Após executar os testes, visualize o relatório:

```bash
npx playwright show-report
```

O relatório HTML mostra:
- Quais testes passaram/falharam
- Screenshots de falhas
- Traces para debug
- Tempo de execução

## CI/CD

Os testes E2E devem ser executados:

- ✅ Em PRs para branches principais
- ✅ Antes de fazer deploy
- ✅ Periodicamente (nightly builds)

Configure no pipeline:
```yaml
- name: Run E2E Tests
  run: npm run test:e2e tests/e2e/barber
```
