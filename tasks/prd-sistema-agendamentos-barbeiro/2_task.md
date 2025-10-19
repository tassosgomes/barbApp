---
status: completed
parallelizable: false
blocked_by: ["1.0"]
completed_date: 2025-10-19
---

<task_context>
<domain>engine/infra/persistence</domain>
<type>implementation|testing</type>
<scope>core_feature|configuration</scope>
<complexity>medium</complexity>
<dependencies>database|sql</dependencies>
<unblocks>"3.0","4.0","6.0","9.0","10.0"</unblocks>
</task_context>

# Tarefa 2.0: Infra/DB - Migration, EF Config, DbContext e AppointmentRepository

## Visão Geral
Criar schema de `appointments`, configurações EF Core, Global Query Filter por barbearia e implementação do `AppointmentRepository`.

## Requisitos
- Migration de `appointments` com FKs, índices e tipos (TIMESTAMP WITH TIME ZONE)
- EntityTypeConfiguration para `Appointment`
- DbSet e Global Query Filter no `BarbAppDbContext`
- `AppointmentRepository` com métodos GetById, GetByBarberAndDate, Insert, Update

## Subtarefas
- [x] 2.1 Criar migration para `appointments`
- [x] 2.2 Implementar `AppointmentConfiguration`
- [x] 2.3 Adicionar DbSet e filtro global no DbContext
- [x] 2.4 Implementar `AppointmentRepository`
- [x] 2.5 Validar índices com EXPLAIN/ANALYZE simples

## ✅ Checklist de Conclusão
- [x] 2.0 Infra/DB - Migration, EF Config, DbContext e AppointmentRepository ✅ CONCLUÍDA
  - [x] Implementação completada
  - [x] Definição da tarefa, PRD e tech spec validados
  - [x] Análise de regras e conformidade verificadas
  - [x] Revisão de código completada
  - [x] Todos os testes passando (9/9)
  - [x] Pronto para deploy

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 4.0, 6.0, 9.0, 10.0
- Paralelizável: Não

## Detalhes de Implementação
- Índices: barbearia_id, (barber_id, start_time), customer_id, status
- Observação: manter integridade referencial com ON DELETE CASCADE onde aplicável

## Critérios de Sucesso
- Migration aplica com sucesso e índices presentes
- Repository executa operações com isolamento multi-tenant