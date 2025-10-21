---
status: pending
parallelizable: false
blocked_by: ["21.0", "6.0"]
---

<task_context>
<domain>frontend-public/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>24.0, 25.0, 26.0, 27.0, 28.0</unblocks>
</task_context>

# Tarefa 22.0: Types, Hooks e API Integration (Public)

## Visão Geral

Criar types, hooks customizados e integração com API para o frontend público. Inclui `useLandingPageData` e `useServiceSelection`.

<requirements>
- Types compartilhados (landing-page.types.ts)
- Hook `useLandingPageData` (fetch data)
- Hook `useServiceSelection` (seleção de serviços)
- Integração com API pública
- Cache com TanStack Query
</requirements>

## Subtarefas

- [ ] 22.1 Criar `types/landing-page.types.ts`
- [ ] 22.2 Criar hook `useLandingPageData.ts`
- [ ] 22.3 Criar hook `useServiceSelection.ts`
- [ ] 22.4 Configurar TanStack Query provider
- [ ] 22.5 Adicionar tratamento de loading/error
- [ ] 22.6 Criar testes dos hooks

## Detalhes de Implementação

Ver techspec-frontend.md seções 2.2 e 2.3 para código completo.

## Critérios de Sucesso

- [ ] Hooks funcionando corretamente
- [ ] Seleção de serviços com cálculo de totais
- [ ] Cache funcionando (5 minutos)
- [ ] Testes unitários passando
