---
status: pending
parallelizable: true
blocked_by: ["2.0","3.0"]
---

<task_context>
<domain>engine/infra/performance</domain>
<type>implementation|testing</type>
<scope>performance|configuration</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 9.0: Performance - Índices e Tempos de Resposta

## Visão Geral
Garantir SLAs: agenda <3s, ações <1s, troca de contexto <2s; validar índices e otimizações.

## Requisitos
- Índices no schema de `appointments` válidos
- Análises de consultas com EXPLAIN/ANALYZE
- Métricas de duração para endpoints críticos

## Subtarefas
- [ ] 9.1 Verificar/criar índices e medir
- [ ] 9.2 Ajustar queries e paginação
- [ ] 9.3 Medir tempos em dev e registrar resultados

## Sequenciamento
- Bloqueado por: 2.0, 3.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Considerar caching leve (TTL 10s) para agenda, se necessário.

## Critérios de Sucesso
- Tempos aferidos atendem requisitos do PRD