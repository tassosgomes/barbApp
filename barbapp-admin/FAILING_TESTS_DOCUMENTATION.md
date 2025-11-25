# Documentação de Testes Falhos - barbapp-admin

**Data de Criação:** $(date +%Y-%m-%d)  
**Total de Testes Falhos:** 10  
**Total de Testes:** 955  
**Taxa de Sucesso:** 98.95%

---

## Visão Geral

Este documento contém a análise preliminar dos 10 testes que ainda estão falhando após a sessão de correção de testes. Cada teste está documentado como uma tarefa separada para resolução futura.

---

## Tarefa 1: ProtectedRoute.test.tsx (Unit) - Redirect Path

**Arquivo:** `src/__tests__/unit/components/ProtectedRoute.test.tsx`  
**Nome do Teste:** `should redirect to login if not authenticated`  
**Status:** ❌ Falhando

### Erro

```
AssertionError: expected <mock> to be called at least once

- Expected call count: 1
- Actual call count: 0

  expect(mockNavigate).toHaveBeenCalledWith('/login', { replace: true })
```

### Saída HTML Relevante

O componente renderiza um `<Navigate>` com `data-to="/admin/login"`:

```html
<mock data-to="/admin/login" data-replace="true" />
```

### Análise Preliminar

1. O teste espera que o redirect seja para `/login`, mas o componente está redirecionando para `/admin/login`
2. O mock do `Navigate` pode não estar configurado corretamente para capturar a chamada
3. Há uma inconsistência entre a expectativa do teste e o comportamento real do componente

### Ações Sugeridas

- [ ] Verificar qual é o path correto de redirect no ProtectedRoute.tsx
- [ ] Se `/admin/login` é o correto, atualizar o teste para esperar este path
- [ ] Se `/login` é o correto, investigar por que o componente está usando `/admin/login`
- [ ] Ajustar o mock do Navigate se necessário

### Complexidade Estimada

🟡 Média - Pode envolver decisão de negócio sobre o path correto

---

## Tarefa 2: ProtectedRoute.test.tsx (Auth) - CSS Class Mismatch

**Arquivo:** `src/components/auth/__tests__/ProtectedRoute.test.tsx`  
**Nome do Teste:** `should display a loading state while checking authentication`  
**Status:** ❌ Falhando

### Erro

```
AssertionError: expected '<div class="flex items-center justify-…' to contain 'text-gray-600'

- Actual:   "text-gray-700"
- Expected: "text-gray-600"
```

### HTML Renderizado

```html
<div class="flex items-center justify-center h-screen">
  <div class="text-center">
    <div class="w-12 h-12 border-4 border-primary border-t-transparent rounded-full animate-spin mx-auto"></div>
    <p class="mt-4 text-gray-700">Verificando autenticação...</p>  <!-- Era esperado text-gray-600 -->
  </div>
</div>
```

### Análise Preliminar

1. O componente mudou a classe CSS de `text-gray-600` para `text-gray-700`
2. O teste está verificando classes CSS específicas, o que é frágil
3. A funcionalidade (mostrar loading) está correta, apenas o estilo mudou

### Ações Sugeridas

- [ ] **Opção 1:** Atualizar o teste para usar `text-gray-700`
- [ ] **Opção 2:** Melhorar o teste para não depender de classes CSS específicas
- [ ] Refatorar para usar `getByRole` ou `getByText` em vez de verificar classes

### Complexidade Estimada

🟢 Baixa - Simples atualização de expectativa ou refatoração para melhor abordagem

---

## Tarefa 3: SelectBarbershopPage.test.tsx - Button Role Query

**Arquivo:** `src/__tests__/unit/pages/SelectBarbershopPage.test.tsx`  
**Nome do Teste:** `should render barbershops list`  
**Status:** ❌ Falhando

### Erro

```
TestingLibraryElementError: Unable to find an accessible element with the role "button" 
and name "Barbearia Downtown - Centro"
```

### HTML Renderizado

```html
<div class="cursor-pointer hover:shadow-lg transition-shadow">
  <div class="rounded-lg border bg-card text-card-foreground shadow-sm hover:border-primary/50 transition-colors">
    <div class="flex flex-col space-y-1.5 p-6">
      <h3 class="font-semibold leading-none tracking-tight text-lg">Barbearia Downtown</h3>
      <div class="text-sm text-muted-foreground">
        <p class="text-muted-foreground mt-2">Centro</p>
      </div>
    </div>
  </div>
</div>
```

### Análise Preliminar

1. O componente renderiza cards clicáveis, mas sem role="button"
2. O card usa `cursor-pointer` para indicar clicabilidade, mas não é semanticamente um botão
3. O teste assume que há um `button` com o nome combinado "Barbearia Downtown - Centro"

### Ações Sugeridas

- [ ] **Opção 1:** Adicionar `role="button"` e `aria-label` adequado aos cards
- [ ] **Opção 2:** Refatorar o teste para usar uma query diferente (ex: `getByText` + container)
- [ ] Considerar acessibilidade - se os cards são clicáveis, devem ter semântica adequada
- [ ] Verificar como outros testes similares estão consultando cards clicáveis

### Complexidade Estimada

🟡 Média - Envolve decisão sobre abordagem de acessibilidade

---

## Tarefa 4: BarbeiroFormPage.test.tsx - Create Service Not Called

**Arquivo:** `src/pages/Barbeiros/__tests__/BarbeiroFormPage.test.tsx`  
**Nome do Teste:** `should call create service on submit`  
**Status:** ❌ Falhando

### Erro

```
AssertionError: expected "spy" to be called at least once

expect(mockBarbeiroService.create).toHaveBeenCalled()
```

### Análise Preliminar

1. O formulário parece não estar submetendo corretamente no teste
2. Pode haver validação que está bloqueando o submit
3. O mock do serviço pode não estar sendo chamado devido a:
   - Formulário não sendo submetido (validação falhou)
   - Evento de submit não sendo processado corretamente
   - React Query mutation não sendo disparada

### Ações Sugeridas

- [ ] Verificar se todos os campos obrigatórios estão preenchidos no teste
- [ ] Adicionar `await waitFor` após o click no botão de submit
- [ ] Verificar se há mensagens de erro de validação sendo exibidas
- [ ] Debug: adicionar console.log no handler de submit para verificar se é chamado
- [ ] Verificar se o mock do service está configurado corretamente com React Query

### Complexidade Estimada

🔴 Alta - Requer investigação aprofundada do fluxo de submit

---

## Tarefa 5: BarbeiroFormPage.test.tsx - Service Validation

**Arquivo:** `src/pages/Barbeiros/__tests__/BarbeiroFormPage.test.tsx`  
**Nome do Teste:** `should require at least one service selected`  
**Status:** ❌ Falhando

### Erro

```
TestingLibraryElementError: Unable to find an element with the text: 
/selecione pelo menos um serviço/i
```

### Análise Preliminar

1. O teste espera uma mensagem de validação quando nenhum serviço é selecionado
2. Possíveis causas:
   - A validação não está implementada no componente
   - A mensagem de erro tem texto diferente
   - A validação acontece de forma diferente (ex: no servidor)

### Ações Sugeridas

- [ ] Verificar se a validação de serviços está implementada no BarbeiroFormPage
- [ ] Verificar o texto exato da mensagem de validação
- [ ] Se a validação não existe, decidir se deve ser implementada ou o teste removido
- [ ] Verificar se há validação do lado do servidor apenas

### Complexidade Estimada

🟡 Média - Pode requerer implementação de validação

---

## Tarefa 6: BarbeiroFormPage.test.tsx - Pre-select Services

**Arquivo:** `src/pages/Barbeiros/__tests__/BarbeiroFormPage.test.tsx`  
**Nome do Teste:** `should pre-select services from existing barbeiro`  
**Status:** ❌ Falhando

### Erro

```
AssertionError: expected undefined to be true

checkbox.checked // undefined
```

### Análise Preliminar

1. O teste busca checkboxes de serviços e verifica se estão marcados
2. A propriedade `checked` retorna `undefined` em vez de `true`
3. Possíveis causas:
   - O elemento encontrado não é um input checkbox
   - É um componente customizado que não usa input nativo
   - Os dados do barbeiro com serviços pré-selecionados não estão carregando

### Ações Sugeridas

- [ ] Verificar como os checkboxes de serviços são implementados (nativo vs Radix)
- [ ] Se for Radix Checkbox, usar `data-state="checked"` em vez de `.checked`
- [ ] Verificar se o mock do barbeiro inclui serviços corretamente
- [ ] Usar `screen.debug()` para ver o estado real dos checkboxes

### Complexidade Estimada

🟡 Média - Requer entendimento da implementação do checkbox

---

## Tarefa 7: BarbeiroFormPage.test.tsx - Update Service Not Called

**Arquivo:** `src/pages/Barbeiros/__tests__/BarbeiroFormPage.test.tsx`  
**Nome do Teste:** `should call update service on submit`  
**Status:** ❌ Falhando

### Erro

```
AssertionError: expected "spy" to be called at least once

expect(mockBarbeiroService.update).toHaveBeenCalled()
```

### Análise Preliminar

1. Similar à Tarefa 4, mas para o fluxo de edição
2. O parâmetro `id` da URL pode não estar sendo capturado corretamente
3. O formulário em modo edição pode ter comportamento diferente

### Ações Sugeridas

- [ ] Verificar se o mock do useParams está retornando o ID corretamente
- [ ] Verificar se os dados do barbeiro existente estão carregando
- [ ] Debug do fluxo de submit em modo edição
- [ ] Verificar se mutation de update está configurada corretamente

### Complexidade Estimada

🔴 Alta - Similar à Tarefa 4, requer investigação profunda

---

## Tarefa 8: BarbeiroFormPage.test.tsx - Submitting State

**Arquivo:** `src/pages/Barbeiros/__tests__/BarbeiroFormPage.test.tsx`  
**Nome do Teste:** `should disable form fields while submitting`  
**Status:** ❌ Falhando

### Erro

```
TestingLibraryElementError: Unable to find an element with the text: /salvando/i
```

### Análise Preliminar

1. O teste espera que o botão mostre "Salvando..." durante o submit
2. O estado `isSubmitting` pode não estar sendo ativado
3. O texto do botão durante submit pode ser diferente

### Ações Sugeridas

- [ ] Verificar o texto real do botão durante o estado de submitting
- [ ] Verificar se o mutation `isPending` está sendo usado corretamente
- [ ] Pode ser necessário mockar o serviço para demorar (simular loading)
- [ ] Usar `vi.useFakeTimers()` se necessário para capturar estado transitório

### Complexidade Estimada

🟡 Média - Depende de como o loading state é implementado

---

## Tarefa 9: Dashboard.test.tsx - Upcoming Appointments

**Arquivo:** `src/pages/Dashboard/__tests__/Dashboard.test.tsx`  
**Nome do Teste:** `should render upcoming appointments when available`  
**Status:** ❌ Falhando

### Erro

```
TestingLibraryElementError: Unable to find an element with the text: João Silva
```

### HTML Renderizado (Parcial)

O Dashboard renderiza os cards de métricas, mas a seção de agendamentos não aparece ou não tem os dados esperados.

### Análise Preliminar

1. O mock de agendamentos pode não estar sendo retornado corretamente
2. A seção de "próximos agendamentos" pode não estar sendo renderizada
3. O componente pode ter condições para mostrar agendamentos que não estão sendo satisfeitas

### Ações Sugeridas

- [ ] Verificar se o mock de `appointments` está configurado no MSW ou no service mock
- [ ] Verificar se há uma seção específica para próximos agendamentos no Dashboard
- [ ] Debug: verificar o que está sendo renderizado com `screen.debug()`
- [ ] Verificar se há query de appointments sendo feita

### Complexidade Estimada

🟡 Média - Requer análise da implementação do Dashboard

---

## Tarefa 10: SchedulePage.test.tsx - Group by Barber

**Arquivo:** `src/pages/Schedule/__tests__/SchedulePage.test.tsx`  
**Nome do Teste:** `should group appointments by barber`  
**Status:** ❌ Falhando

### Erro

```
TestingLibraryElementError: Unable to find an element with the text: /João Silva/
```

### HTML Renderizado (Parcial)

A página de agenda renderiza os controles de filtro (data, barbeiro, status), mas a lista de agendamentos agrupados não aparece com os dados mockados.

### Análise Preliminar

1. O agrupamento por barbeiro pode não estar funcionando corretamente
2. O mock de agendamentos pode não incluir o campo do barbeiro
3. A query de agendamentos pode estar filtrando por uma data específica que não tem dados

### Ações Sugeridas

- [ ] Verificar o mock de appointments usado no teste
- [ ] Verificar se a data do filtro corresponde aos dados mockados
- [ ] Verificar se o agrupamento por barbeiro está implementado
- [ ] Debug: verificar se há dados de agendamentos na resposta mockada

### Complexidade Estimada

🟡 Média - Requer análise da implementação da SchedulePage

---

## Resumo de Priorização

| Prioridade | Tarefa | Complexidade | Arquivo |
|------------|--------|--------------|---------|
| 1 | Tarefa 2 | 🟢 Baixa | ProtectedRoute (CSS) |
| 2 | Tarefa 1 | 🟡 Média | ProtectedRoute (Path) |
| 3 | Tarefa 3 | 🟡 Média | SelectBarbershopPage |
| 4 | Tarefa 6 | 🟡 Média | BarbeiroFormPage (Pre-select) |
| 5 | Tarefa 5 | 🟡 Média | BarbeiroFormPage (Validation) |
| 6 | Tarefa 8 | 🟡 Média | BarbeiroFormPage (Submitting) |
| 7 | Tarefa 9 | 🟡 Média | Dashboard |
| 8 | Tarefa 10 | 🟡 Média | SchedulePage |
| 9 | Tarefa 4 | 🔴 Alta | BarbeiroFormPage (Create) |
| 10 | Tarefa 7 | 🔴 Alta | BarbeiroFormPage (Update) |

---

## Dependências Comuns

Várias tarefas do BarbeiroFormPage (4, 5, 6, 7, 8) podem compartilhar a mesma causa raiz. Recomenda-se investigar o mecanismo de submit do formulário primeiro.

---

## Comandos Úteis para Debug

```bash
# Rodar um teste específico com output verbose
npx vitest run --reporter=verbose 'path/to/test.tsx'

# Rodar com debug habilitado
DEBUG=1 npx vitest run 'path/to/test.tsx'

# Rodar teste específico pelo nome
npx vitest run -t "nome do teste"

# Rodar todos os testes falhos
npx vitest run --reporter=verbose | grep -A 20 "FAIL"
```
