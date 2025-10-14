---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/infra/security</domain>
<type>implementation|documentation</type>
<scope>configuration|performance</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"7.0","9.0"</unblocks>
</task_context>

# Tarefa 3.0: Backend - Scrub de PII e política de amostragem

## Visão Geral
Configurar filtros para remover PII (ex.: headers de autorização, payloads sensíveis) e definir amostragem de traces/eventos por ambiente para controlar custo e overhead.

## Requisitos
- `SendDefaultPii = false` e scrubs adicionais em hook (ex.: `BeforeSend`)
- Definição de `TracesSampleRate` e event sampling por ambiente
- Documentar campos sensíveis a remover (ex.: `authorization`, tokens)

## Subtarefas
- [ ] 3.1 Adicionar hook/filters para scrub de headers/campos sensíveis
- [ ] 3.2 Parametrizar `TracesSampleRate` por env (dev/homolog/prod)
- [ ] 3.3 Documentar política de privacidade/retirada de dados em `backend/README.md`

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 7.0, 9.0
- Paralelizável: Sim (independe do frontend)

## Detalhes de Implementação
- Tech Spec sugere defaults seguros; expandir com `BeforeSend` se necessário
- Consultar PRD (Governança de Dados e Segurança)

## Critérios de Sucesso
- Nenhum evento contém tokens/headers sensíveis
- Rates de amostragem configuráveis por variável de ambiente
- Documentação clara para auditoria/segurança

