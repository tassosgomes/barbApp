---
status: pending
parallelizable: true
blocked_by: []
---

<task_context>
<domain>engine/web</domain>
<type>implementation|integration</type>
<scope>configuration|core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"5.0","7.0","9.0","10.0"</unblocks>
</task_context>

# Tarefa 4.0: Frontend - Inicialização @sentry/react no bootstrap

## Visão Geral
Inicializar o Sentry no `barbapp-admin` (React + Vite) no arquivo `src/main.tsx`, configurando `dsn`, `environment`, `release`, integração de tracing e `beforeSend` para remover dados sensíveis.

## Requisitos
- Adicionar dependência `@sentry/react`
- Inicializar Sentry no bootstrap com envs `VITE_SENTRY_*`
- Ativar `browserTracingIntegration` e `tracesSampleRate`
- Remover header `authorization` (e similares) no `beforeSend`

## Subtarefas
- [ ] 4.1 Instalar `@sentry/react`
- [ ] 4.2 Configurar `Sentry.init` em `barbapp-admin/src/main.tsx`
- [ ] 4.3 Criar `.env` de exemplo com placeholders de `VITE_SENTRY_*`
- [ ] 4.4 Testar erro simulado e verificar envio ao Sentry

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 5.0, 7.0, 9.0, 10.0
- Paralelizável: Sim (independente de backend)

## Detalhes de Implementação
- Ver Tech Spec (seção Frontend – Bootstrap)
- Exemplo inclui remoção de `authorization` no `beforeSend`

## Critérios de Sucesso
- Erros JS não tratados são capturados e enviados
- Eventos incluem `environment` e `release` corretos
- Nenhum dado sensível em eventos

