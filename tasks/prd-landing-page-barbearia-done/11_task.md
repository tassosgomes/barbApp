---
status: completed
parallelizable: true
blocked_by: ["10.0", "5.0"]
completed_date: 2025-10-21
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

- [x] 11.1 Criar `services/api/landing-page.api.ts`
- [x] 11.2 Criar hook `useLandingPage.ts`
- [x] 11.3 Implementar query para buscar config
- [x] 11.4 Implementar mutation para atualizar
- [x] 11.5 Adicionar tratamento de erros e toasts
- [x] 11.6 Criar testes do hook

## Detalhes de Implementação

Ver techspec-frontend.md seção 1.3 para código completo.

## Critérios de Sucesso

- [x] Hook funcionando e integrando com API
- [x] Cache e invalidação automática
- [x] Toasts de sucesso/erro
- [x] Testes unitários passando
