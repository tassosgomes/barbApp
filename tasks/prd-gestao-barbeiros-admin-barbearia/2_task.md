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
<unblocks>"3.0","4.0","6.0","9.0"</unblocks>
</task_context>

# Tarefa 2.0: Infra/DB - Migrations, Configurações EF Core, DbContext e Repositórios

## Visão Geral
Criar o esquema de banco e infraestrutura de persistência: migrations para `barbers` e `barbershop_services`, EntityTypeConfigurations, extensão do `DbContext` com Global Query Filters multi-tenant e repositórios concretos.

## Requisitos
- Migrations para tabelas conforme Tech Spec (constraints, índices)
- Configurações EF Core para `Barber` e `BarbershopService`
- Global Query Filters baseados no `ITenantContext`
- Repositórios `BarberRepository` e `BarbershopServiceRepository`
- Scripts/steps para aplicar migrations localmente

## Subtarefas
- [ ] 2.1 Criar migrations de `barbers` e `barbershop_services`
- [ ] 2.2 Implementar `BarberConfiguration` e `BarbershopServiceConfiguration`
- [ ] 2.3 Estender `BarbAppDbContext` (DbSets e filtros globais)
- [ ] 2.4 Implementar repositórios concretos
- [ ] 2.5 Validar índices e performance básica (explain/analyze simples)

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 4.0, 6.0, 9.0
- Paralelizável: Não (depende do domínio e antecede application/API)

## Detalhes de Implementação
- Seguir mapeamentos fornecidos na Tech Spec.
- Garantir constraint UNIQUE (barbearia_id, phone) e índices listados.
- Aplicar filtros globais conforme snippet da Tech Spec.

## Critérios de Sucesso
- Migrations aplicam com sucesso e tabelas/índices existem
- Repositórios executam CRUD básico sem violar isolamento multi-tenant
- Consultas de listagem respeitam paginação e filtros