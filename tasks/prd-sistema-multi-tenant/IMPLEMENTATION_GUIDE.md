# Guia de Implementação - Sistema Multi-tenant e Autenticação

## Visão Geral

Este documento orienta a implementação do sistema multi-tenant e autenticação do barbApp, com foco em **execução eficiente através de paralelização** onde possível.

## Estrutura de Arquivos

```
tasks/prd-sistema-multi-tenant/
├── prd.md                    # Product Requirements Document
├── techspec.md               # Especificação Técnica
├── tasks.md                  # Resumo de todas as tarefas
├── IMPLEMENTATION_GUIDE.md   # Este arquivo
└── [1-14]_task.md           # Tarefas individuais detalhadas
```

## Estratégia de Execução

### Fase 1: Fundação Sequencial (5h)
**Tarefas que DEVEM ser executadas em sequência:**

```
1.0 (Setup e Dependências) - 2h
  ↓
2.0 (Domain Layer Base) - 3h
```

**Por quê sequencial?**
- Tarefa 1.0 instala pacotes necessários para tarefa 2.0
- Tarefa 2.0 define interfaces e contratos usados por todas as outras

### Fase 2: Desenvolvimento Paralelo (12h → 7h com 2 devs)

Após completar tarefa 2.0, **DUAS TRILHAS INDEPENDENTES** podem ser executadas em paralelo:

#### **Trilha A - Domain & Application** (Developer 1)
```
3.0 (Entidades de Usuários) - 4h
  ↓
6.0 (DTOs e Validadores) - 2h
  ↓
7.0 (Use Cases de Autenticação) - 8h
```
**Total: 14h sequencial**

#### **Trilha B - Infrastructure** (Developer 2)
```
4.0 (DbContext e Query Filters) - 5h
  ↓
5.0 (Repositórios) - 3h
  ↓
8.0 (JWT e Segurança) - 4h
```
**Total: 12h sequencial**

**Ganho de Paralelização:** De 26h para 14h (economia de 12h)

### Fase 3: Integração e API (10h)

Após convergência das trilhas A e B:

```
9.0 (Middlewares) - 3h
  ↓ (requer 7.0 + 8.0)
10.0 (Controller de Autenticação) - 4h
  ↓ (requer 9.0)
11.0 (Configurar API e Pipeline) - 3h
```

**Paralelização possível:**
- 12.0 (Documentar Swagger) - 2h pode ser feito em paralelo com 11.0

### Fase 4: Testes e Validação (10h → 7h com 2 devs)

```
13.0 (Testes de Integração) - 6h
  ↓ (pode executar em paralelo inicial com 11.0 finalizando)
14.0 (Validação E2E) - 4h
```

## Caminho Crítico Total

### Execução Sequencial (1 desenvolvedor)
```
1.0 → 2.0 → 3.0 → 6.0 → 7.0 → 9.0 → 10.0 → 11.0 → 13.0 → 14.0
2h + 3h + 4h + 2h + 8h + 3h + 4h + 3h + 6h + 4h = 39h
```

**Tempo Total: ~39 horas (5 dias úteis)**

### Execução com Paralelização (2 desenvolvedores)
```
Fase 1: 5h (sequencial)
Fase 2: 14h (Dev1: Trilha A) || 12h (Dev2: Trilha B) = 14h
Fase 3: 10h (convergência)
Fase 4: 10h
```

**Tempo Total: ~39 horas de trabalho, mas ~27 horas de tempo real**

## Dependências e Bloqueios

### Matriz de Dependências

| Tarefa | Bloqueada Por | Desbloqueia | Paralelizável |
|--------|---------------|-------------|---------------|
| 1.0    | -             | 2.0         | ❌ Não        |
| 2.0    | 1.0           | 3.0, 4.0    | ❌ Não        |
| 3.0    | 2.0           | 6.0         | ✅ Sim (com 4.0) |
| 4.0    | 2.0           | 5.0         | ✅ Sim (com 3.0) |
| 5.0    | 4.0           | -           | ✅ Sim (com 6.0) |
| 6.0    | 3.0           | 7.0         | ✅ Sim (com 5.0) |
| 7.0    | 6.0           | 10.0        | ✅ Sim (com 8.0) |
| 8.0    | 5.0           | 9.0         | ✅ Sim (com 7.0) |
| 9.0    | 7.0, 8.0      | 10.0        | ❌ Não        |
| 10.0   | 9.0           | 11.0        | ❌ Não        |
| 11.0   | 10.0          | 13.0        | ✅ Sim (com 12.0) |
| 12.0   | 10.0          | -           | ✅ Sim (com 11.0) |
| 13.0   | 11.0          | 14.0        | ❌ Não        |
| 14.0   | 13.0          | -           | ❌ Não        |

## Pontos de Convergência Críticos

### 🔴 Convergência 1: Após Tarefa 2.0
**Libera:** Trilhas paralelas A (Domain/Application) e B (Infrastructure)

### 🔴 Convergência 2: Após Tarefas 7.0 + 8.0
**Libera:** Tarefa 9.0 (Middlewares) - **requer ambas completadas**

### 🔴 Convergência 3: Após Tarefa 11.0
**Libera:** Testes de integração

## Checklist de Validação por Fase

### ✅ Checkpoint 1: Fundação (Após 2.0)
- [ ] Todos os pacotes NuGet instalados
- [ ] Build executando sem erros
- [ ] Interfaces de domínio definidas
- [ ] Testes de BarbeariaCode passando (10+ testes)
- [ ] Cobertura de código > 90% no domínio

### ✅ Checkpoint 2: Desenvolvimento Paralelo (Após 7.0 e 8.0)
- [ ] Entidades de usuários implementadas e testadas
- [ ] DbContext com Global Query Filters funcionando
- [ ] Repositórios implementados
- [ ] DTOs e validadores criados
- [ ] Use Cases de autenticação implementados
- [ ] JWT e PasswordHasher funcionando
- [ ] Testes unitários passando (30+ testes)
- [ ] Migration executada com sucesso

### ✅ Checkpoint 3: API Funcional (Após 11.0)
- [ ] Middlewares configurados
- [ ] Controller de autenticação implementado
- [ ] Pipeline de autenticação funcionando
- [ ] Swagger documentado
- [ ] API respondendo em localhost
- [ ] Login manual testado via Swagger

### ✅ Checkpoint 4: Produção (Após 14.0)
- [ ] Testes de integração passando (20+ testes)
- [ ] Isolamento multi-tenant validado
- [ ] Testes E2E executados com sucesso
- [ ] Todos os requisitos do PRD atendidos
- [ ] Performance < 1s para autenticação
- [ ] Documentação completa

## Comandos Úteis

### Durante Desenvolvimento
```bash
# Build incremental
dotnet build

# Executar testes unitários
dotnet test --filter "FullyQualifiedName~Domain.Tests"

# Criar migration
dotnet ef migrations add AddUserEntities -p BarbApp.Infrastructure -s BarbApp.API

# Aplicar migration
dotnet ef database update -p BarbApp.Infrastructure -s BarbApp.API

# Executar API localmente
dotnet run --project BarbApp.API
```

### Validação de Qualidade
```bash
# Testes com cobertura
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage

# Executar apenas testes de integração
dotnet test --filter "Category=Integration"

# Verificar warnings
dotnet build --warnaserror
```

## Métricas de Sucesso

### Objetivos de Performance
- ✅ Autenticação < 1 segundo (requisito PRD)
- ✅ Validação de JWT < 100ms
- ✅ Global Query Filters < 50ms overhead

### Objetivos de Qualidade
- ✅ Cobertura de testes > 80%
- ✅ 0 warnings no build
- ✅ 100% de isolamento multi-tenant (zero vazamento)
- ✅ Taxa de sucesso de login > 95%

### Objetivos de Segurança
- ✅ Secret key com 32+ caracteres
- ✅ Passwords com BCrypt work factor 12
- ✅ Cookies com HttpOnly + Secure flags
- ✅ CORS configurado corretamente

## Próximos Passos

1. **Imediato:** Começar com tarefa 1.0 (Setup e Dependências)
2. **Após 2.0:** Dividir equipe em 2 trilhas paralelas
3. **Durante:** Validar checkpoints a cada convergência
4. **Final:** Executar validação E2E completa

## Referências Rápidas

- **PRD:** [tasks/prd-sistema-multi-tenant/prd.md](./prd.md)
- **TechSpec:** [tasks/prd-sistema-multi-tenant/techspec.md](./techspec.md)
- **Tasks:** [tasks/prd-sistema-multi-tenant/tasks.md](./tasks.md)
- **Tarefas Individuais:** [1_task.md](./1_task.md) até [14_task.md](./14_task.md)

---

**Última Atualização:** 2025-10-11
**Status:** Pronto para Implementação
**Estimativa Total:** 39h (trabalho) / 27h (tempo real com 2 devs)
