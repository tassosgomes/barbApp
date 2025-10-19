---
status: completed
parallelizable: true
blocked_by: ["3.0","4.0"]
completed_date: 2025-10-19
---

<task_context>
<domain>engine/application</domain>
<type>integration|documentation</type>
<scope>performance|configuration</scope>
<complexity>low</complexity>
<dependencies>http_server|temporal</dependencies>
<unblocks>"8.0","9.0"</unblocks>
</task_context>

# Tarefa 5.0: Agenda e Polling - Contrato e Diretrizes (10s)

## Visão Geral
Definir contrato de agenda (payload) e diretrizes de polling (10s) para o frontend, incluindo cancelamento de polling ao sair da tela e tratamento de erros.

## Requisitos
- Contrato JSON de saída para `GET /api/schedule/my-schedule`
- Recomendação de polling: 10s; backoff exponencial opcional em erro
- Orientações para exibir loading/empty states e indicadores de atualização

## Subtarefas
- [x] 5.1 Consolidar contrato com exemplos
- [x] 5.2 Documentar diretrizes de polling e edge cases

## Sequenciamento
- Bloqueado por: 3.0, 4.0
- Desbloqueia: 8.0, 9.0
- Paralelizável: Sim

## Detalhes de Implementação
- Sugerir abort controllers no frontend para cancelar polling ao navegar.

## Critérios de Sucesso
- Frontend confirma clareza do contrato e do fluxo de polling