# E2E Tests - Barbearia Schedule

Este documento descreve os testes end-to-end implementados para validar os fluxos completos do sistema de agendamentos do barbeiro.

## Visão Geral

Os testes E2E cobrem todas as funcionalidades principais da agenda do barbeiro, incluindo:
- Login e visualização da agenda
- Ações de agendamento (confirmar, cancelar, concluir)
- Navegação entre dias
- Tratamento de erros
- Responsividade mobile
- Isolamento multi-tenant (planejado)

## Estrutura dos Testes

### Arquivos de Teste

- `barber/schedule.spec.ts` - Testes principais da funcionalidade de agenda

### Helpers

- `helpers/barber.helper.ts` - Funções utilitárias para autenticação e interações comuns

## Cenários de Teste Implementados

### 1. Visualização (`Barber Schedule - Visualização`)

- **Exibir agenda do dia atual**: Verifica carregamento da agenda e elementos principais
- **Mostrar contador de agendamentos**: Valida exibição do número de agendamentos
- **Exibir lista de agendamentos**: Verifica estrutura e dados dos cards de agendamento

### 2. Ações (`Barber Schedule - Ações`)

- **Confirmar agendamento pendente**: Testa fluxo completo de confirmação
- **Cancelar agendamento**: Testa cancelamento com modal de confirmação
- **Concluir agendamento**: Testa marcação de conclusão após horário

### 3. Navegação (`Barber Schedule - Navegação`)

- **Navegar para dia anterior**: Testa botão "Dia Anterior"
- **Navegar para próximo dia**: Testa botão "Próximo Dia"
- **Voltar para hoje**: Testa botão "Hoje"

### 4. Polling (`Barber Schedule - Polling`)

- **Atualização automática**: Verifica que a agenda é atualizada a cada 10 segundos

### 5. Tratamento de Erros (`Barber Schedule - Tratamento de Erros`)

- **Erro 404**: Simula tentativa de ação em agendamento inexistente
- **Erro 409**: Simula conflito de status (concorrência)

### 6. Mobile (`Barber Schedule - Mobile Responsividade`)

- **Viewport mobile**: Testa funcionamento em dispositivos móveis
- **Pull-to-refresh**: Testa gesto de atualização

## Configuração do Playwright

### Browsers Suportados

- **Chromium** (Desktop e Mobile)
- Firefox e WebKit desabilitados temporariamente devido a problemas de instalação

### Viewports

- **Desktop**: 1280x720 (padrão do Playwright)
- **Mobile**: 375x667 (iPhone SE)

### Timeouts

- **Navegação**: 10 segundos para carregamento inicial
- **Ações**: 5 segundos para elementos aparecerem
- **Teste total**: 30 segundos por teste

## Pré-requisitos para Execução

### 1. Backend Rodando

O backend .NET deve estar executando na porta padrão:

```bash
cd ../backend
dotnet run --project src/BarbApp.API/
```

### 2. Frontend Rodando

O frontend deve estar disponível (gerenciado automaticamente pelo Playwright):

```bash
npm run dev  # Executa na porta 3002
```

### 3. Dados de Teste

Os testes assumem a existência de:
- Usuário barbeiro: `barbeiro@test.com` / `Test@123`
- Agendamentos de teste no banco de dados

## Como Executar os Testes

### Todos os Testes

```bash
npm run test:e2e
```

### Apenas Testes de Agenda

```bash
npx playwright test barber/schedule.spec.ts
```

### Testes em Modo Visual (Headed)

```bash
npx playwright test --headed barber/schedule.spec.ts
```

### Apenas um Projeto/Browser

```bash
npx playwright test --project=chromium barber/schedule.spec.ts
```

### Com Relatório HTML

```bash
npx playwright test --reporter=html barber/schedule.spec.ts
```

## Debugging

### Ver Relatório de Testes

```bash
npx playwright show-report
```

### Executar em Modo Debug

```bash
npx playwright test --debug barber/schedule.spec.ts
```

### Capturas de Tela em Falhas

As capturas são salvas automaticamente em `test-results/` quando um teste falha.

## Problemas Conhecidos

### 1. LocalStorage Access

**Sintoma**: `SecurityError: Failed to read the 'localStorage' property`

**Solução**: Os helpers foram atualizados para tratar erros de localStorage com try/catch.

### 2. Browsers Adicionais

**Sintoma**: Firefox e WebKit não iniciam

**Solução**: Temporariamente desabilitados na configuração. Para reabilitar:

```typescript
// playwright.config.ts
projects: [
  { name: 'chromium', use: { ...devices['Desktop Chrome'] } },
  { name: 'firefox', use: { ...devices['Desktop Firefox'] } },
  { name: 'webkit', use: { ...devices['Desktop Safari'] } },
  // ...
]
```

### 3. Dependência de Dados

**Sintoma**: Testes pulam quando não há dados adequados

**Solução**: Implementado `test.skip()` condicional para cenários sem dados.

## Cobertura de Cenários

| Cenário | Status | Observações |
|---------|--------|-------------|
| Login e visualização | ✅ Implementado | |
| Confirmação de agendamento | ✅ Implementado | |
| Cancelamento de agendamento | ✅ Implementado | |
| Conclusão de agendamento | ✅ Implementado | |
| Navegação entre dias | ✅ Implementado | |
| Troca de contexto multi-barbearia | ❌ Pendente | Requer dados de teste multi-barbearia |
| Polling automático | ✅ Implementado | |
| Tratamento de erros | ✅ Implementado | |
| Responsividade mobile | ✅ Implementado | |
| Isolamento multi-tenant | ❌ Pendente | Requer setup complexo de dados |

## Métricas de Qualidade

- **Tempo de execução**: ~2-3 minutos para todos os testes
- **Estabilidade**: Testes devem passar consistentemente
- **Cobertura**: 80%+ dos fluxos principais cobertos
- **Manutenibilidade**: Código bem estruturado e documentado

## Próximos Passos

1. **Configurar dados de teste** para cenários multi-barbearia
2. **Implementar testes de isolamento** entre barbearias
3. **Adicionar testes de performance** para carregamento da agenda
4. **Expandir cobertura de browsers** (Firefox, WebKit, Safari)
5. **Integrar com CI/CD** para execução automática

## Estrutura de Diretórios

```
tests/e2e/
├── barber/
│   ├── schedule.spec.ts      # Testes da agenda
│   └── 01-auth.spec.ts       # Testes de autenticação (existente)
├── helpers/
│   ├── barber.helper.ts      # Helpers para barbeiro
│   └── admin-barbearia.helper.ts  # Helpers para admin
└── README.md                 # Esta documentação
```