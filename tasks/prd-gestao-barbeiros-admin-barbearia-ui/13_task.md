---
status: completed
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/frontend/routes</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"10.0","11.0","12.0"</unblocks>
</task_context>

# Tarefa 13.0: Roteamento e Guarda de Autenticação

## Visão Geral
Configurar rotas para Barbeiros, Serviços e Agenda, protegendo-as via `useAuth` e redirecionando para `/login` quando necessário.

## Requisitos
- Rotas privadas + rota de login existente
- Reading de session flag `session_expired` e feedback ao usuário (opcional)

## Subtarefas
- [x] 13.1 Definir rotas no `routes/`
- [x] 13.2 Implementar `PrivateRoute`/guard com `useAuth`
- [x] 13.3 Redirecionamentos e mensagens de sessão expirada

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 10.0, 11.0, 12.0
- Paralelizável: Sim

## Detalhes de Implementação
Ver interceptors em `api.ts` e hook `useAuth` existente.

## Critérios de Sucesso
- Rotas protegidas e navegação estável

## Status de Conclusão ✅ CONCLUÍDA
- [x] 13.1 Rotas definidas
  - [x] Rota `/barbeiros` já existia (tarefa anterior)
  - [x] Rota `/servicos` já existia (tarefa anterior)
  - [x] Rota `/agenda` adicionada (durante Task 12.0)
- [x] 13.2 Proteção de rotas via ProtectedRoute já implementada
- [x] 13.3 Interceptors de autenticação já funcionando
- [x] 13.4 Pronto para uso
