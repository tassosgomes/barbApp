---
status: pending
parallelizable: false
blocked_by: ["1.0","2.0","4.0","5.0"]
---

<task_context>
<domain>engine/testing</domain>
<type>testing|documentation</type>
<scope>quality_gate</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server|temporal</dependencies>
<unblocks>"10.0"</unblocks>
</task_context>

# Tarefa 9.0: Testes e Validação - Backend + Frontend

## Visão Geral
Criar e executar plano de testes para validar captura de erros, contexto, sourcemaps e impacto de performance. Cobrir unitários, integração manual e verificação no Sentry.

## Requisitos
- Backend: teste unitário para `GlobalExceptionHandlerMiddleware` invocar captura
- Backend: cenários manuais com tags de tenant/usuário e sem PII
- Frontend: erro simulado com stack trace desofuscado (sourcemaps)
- Medir overhead básico pós-ativação (sem regressão perceptível)

## Subtarefas
- [ ] 9.1 Adicionar testes unitários no backend (captura e tags)
- [ ] 9.2 Roteiro manual: backend (erros, 4xx/5xx, autenticação)
- [ ] 9.3 Roteiro manual: frontend (erros e navegação)
- [ ] 9.4 Verificar eventos no Sentry (contexto, release, env)
- [ ] 9.5 Checar overhead (métricas básicas) e ajustar sampling

## Sequenciamento
- Bloqueado por: 1.0, 2.0, 4.0, 5.0
- Desbloqueia: 10.0
- Paralelizável: Não (depende de integrações estarem ativas)

## Detalhes de Implementação
- Tech Spec: Abordagem de Testes (unitários/manuais)
- PRD: Metas de cobertura/estabilidade e privacidade

## Critérios de Sucesso
- Eventos com contexto e sourcemaps OK no Sentry
- Testes unitários do backend passando
- Overhead dentro da meta (~1–3%) e sampling ajustado

