# Reestruturação de Tarefas - Landing Page Barbearia

## Data: 21/10/2025

## Motivação

O projeto **BarbApp** utiliza **Entity Framework Core com abordagem Code-First**, onde:

1. ✅ Entidades são definidas primeiro em C#
2. ✅ `IEntityTypeConfiguration<T>` configura o mapeamento ORM
3. ✅ Migrations são **geradas automaticamente** pelo EF Core
4. ✅ O SQL é produzido pelo framework, não escrito manualmente

## Mudanças Realizadas

### 📋 Tarefa 1.0 - DESCONTINUADA

**Status Anterior**: `pending`  
**Status Atual**: `deprecated`

**Motivo**: A tarefa propunha criar migrations SQL manualmente, o que não é o padrão do projeto.

**Arquivo**: `1_task.md` foi marcado como deprecated com explicação clara.

---

### ✅ Tarefa 2.0 - EXPANDIDA E ATUALIZADA

**Título Anterior**: "Entities e DTOs do Domínio Landing Page"  
**Título Atual**: "Entities, DTOs, EntityTypeConfiguration e Migration"

**Mudanças**:

1. **Bloqueio removido**: `blocked_by: []` (era `blocked_by: ["1.0"]`)
2. **Requisitos expandidos**:
   - ✅ Adicionado: `EntityTypeConfiguration para ambas as entidades`
   - ✅ Adicionado: `Migration gerada e aplicada no banco`

3. **Novas subtarefas**:
   - `2.3` - Criar `LandingPageConfigConfiguration` (IEntityTypeConfiguration)
   - `2.4` - Criar `LandingPageServiceConfiguration` (IEntityTypeConfiguration)
   - `2.9` - Gerar migration: `dotnet ef migrations add AddLandingPageEntities`
   - `2.10` - Aplicar migration: `dotnet ef database update`
   - `2.11` - Validar estrutura no banco (tabelas, FKs, índices, constraints)

4. **Detalhes de implementação**:
   - Seção completa de `EntityTypeConfiguration` adicionada
   - Comandos de migration documentados
   - Script de validação SQL incluído

---

### 📝 Tarefa 9.0 - DEPENDÊNCIA AJUSTADA

**Mudança**: Removida dependência da Tarefa 1.0

**Antes**: `blocked_by: ["1.0", "2.0", "3.0", "4.0", "5.0", "6.0", "7.0", "8.0"]`  
**Depois**: `blocked_by: ["2.0", "3.0", "4.0", "5.0", "6.0", "7.0", "8.0"]`

---

## Estrutura Final das Tarefas

```
Tarefa 1.0 [DEPRECATED] - Migrations SQL manuais ❌
  └─> Conteúdo incorporado na Tarefa 2.0

Tarefa 2.0 [ATUALIZADA] - Entities + DTOs + Config + Migration ✅
  ├─> Entities (Domain Layer)
  ├─> EntityTypeConfiguration (Infrastructure Layer)
  ├─> DTOs com validações
  ├─> AutoMapper profiles
  └─> Migration EF Core

Tarefa 3.0 - Repositórios
  └─> Depende de: 2.0

Tarefa 4.0+ - Demais features
  └─> Fluxo mantido
```

---

## Padrão Adotado no Projeto

Este alinhamento segue o padrão estabelecido em outras PRDs já concluídas:

✅ **PRD Gestão de Barbeiros** ([`prd-gestao-barbeiros-admin-barbearia-done`](../prd-gestao-barbeiros-admin-barbearia-done/))
✅ **PRD Sistema de Agendamentos** ([`prd-sistema-agendamentos-barbeiro`](../prd-sistema-agendamentos-barbeiro/))
✅ **PRD Sistema Multi-Tenant** ([`prd-sistema-multi-tenant-done`](../prd-sistema-multi-tenant-done/))

Todos seguem: **Entities → EntityTypeConfiguration → Migration gerada pelo EF Core**

---

## Benefícios

1. ✅ **Consistência**: Alinhado com todas as outras features do projeto
2. ✅ **Type-Safety**: Configuração em C# com intellisense e compile-time checking
3. ✅ **Manutenibilidade**: Mudanças no modelo refletem automaticamente nas migrations
4. ✅ **Versionamento**: Migrations geradas são versionadas junto com o código
5. ✅ **Rollback**: EF Core gerencia `Up()` e `Down()` automaticamente

---

## Comandos de Referência

```bash
# Gerar migration
dotnet ef migrations add AddLandingPageEntities \
  --project src/BarbApp.Infrastructure \
  --startup-project src/BarbApp.API

# Aplicar migration
dotnet ef database update \
  --project src/BarbApp.Infrastructure \
  --startup-project src/BarbApp.API

# Remover última migration (se necessário)
dotnet ef migrations remove \
  --project src/BarbApp.Infrastructure \
  --startup-project src/BarbApp.API
```

---

**Revisão aprovada por**: @tsgomes  
**Data**: 21/10/2025
