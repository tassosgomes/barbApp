---
status: pending
parallelizable: true
blocked_by: ["10.0", "5.0"]
---

<task_context>
<domain>frontend-admin/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>17.0, 18.0</unblocks>
</task_context>

# Tarefa 11.0: Hook useLandingPage e API Service

## Visão Geral

Implementar hook customizado para gerenciar estado e operações da landing page no admin, incluindo integração com API via TanStack Query.

<requirements>
- Hook `useLandingPage` com TanStack Query
- API service `landing-page.api.ts`
- Cache e invalidação automática
- Tratamento de erros
- Loading states
- Toasts de feedback
</requirements>

## Subtarefas

- [ ] 11.1 Criar `services/api/landing-page.api.ts`
- [ ] 11.2 Criar hook `useLandingPage.ts`
- [ ] 11.3 Implementar query para buscar config
- [ ] 11.4 Implementar mutation para atualizar
- [ ] 11.5 Adicionar tratamento de erros e toasts
- [ ] 11.6 Criar testes do hook

## Detalhes de Implementação

Ver techspec-frontend.md seção 1.3 para código completo.

## Critérios de Sucesso

- [ ] Hook funcionando e integrando com API
- [ ] Cache e invalidação automática
- [ ] Toasts de sucesso/erro
- [ ] Testes unitários passando
