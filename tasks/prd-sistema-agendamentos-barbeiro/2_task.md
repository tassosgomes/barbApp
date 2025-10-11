---
status: pending
parallelizable: false
blocked_by: ["1.0"]
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
- [ ] 2.1 Criar migration para `appointments`
- [ ] 2.2 Implementar `AppointmentConfiguration`
- [ ] 2.3 Adicionar DbSet e filtro global no DbContext
- [ ] 2.4 Implementar `AppointmentRepository`
- [ ] 2.5 Validar índices com EXPLAIN/ANALYZE simples

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