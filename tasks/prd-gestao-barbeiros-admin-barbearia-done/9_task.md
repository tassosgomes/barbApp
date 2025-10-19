---
status: completed
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

# Tarefa 9.0: Performance e Índices - Paginação, Índices e Tempos de Resposta

## Visão Geral
Garantir que listagens e agenda cumpram SLAs do PRD: <1s lista de barbeiros, <3s agenda, filtros <500ms.

## Requisitos
- Índices listados criados e validados
- Paginação `limit/offset` com limites e defaults
- Métricas de tempo coletadas nos endpoints críticos

## Subtarefas
- [x] 9.1 Verificar/criar índices e medir ganhos
- [x] 9.2 Validar paginação e limites
- [x] 9.3 Medir tempos em ambiente dev (amostras) e ajustar queries

## Sequenciamento
- Bloqueado por: 2.0, 3.0
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação
- Usar EXPLAIN/ANALYZE para confirmar uso de índices.
- Avaliar caching leve para agenda (TTL 10s) se necessário.

## Critérios de Sucesso
- Tempos aferidos atendem requisitos do PRD