# Implementação Cadastro e Agendamento (Cliente) - Resumo de Tarefas

## Visão Geral

Este módulo implementa o cadastro automático e agendamento de serviços para clientes através de URL específica de cada barbearia. O sistema é multi-tenant com isolamento total de dados, autenticação simplificada (telefone + nome), e validação crítica de conflitos de horários com lock otimista.

**Tempo Estimado Total**: 14-17 dias

**Arquitetura**: Clean Architecture + CQRS + Multi-tenancy (shared database)

**Stack Backend**: .NET 8, EF Core, PostgreSQL, JWT, Serilog, Prometheus

**Stack Frontend** (Fase 5): React, TypeScript, Vite, React Query, Tailwind CSS

## Tarefas do Backend

### Fase 1: Foundation (3-4 dias)

- [ ] **1.0** Domain: Entidades Cliente/Agendamento, Enum e Regras (com testes)
  - Entidades: Cliente, Agendamento, Value Object Telefone
  - Validações de negócio, transições de estado
  - Testes unitários com cobertura > 90%
  - **Bloqueado por**: Nenhuma (tarefa inicial)
  - **Desbloqueia**: 2.0, 3.0

- [ ] **2.0** Infra/DB: Migrations, Config EF Core, Global Filters e Repositórios
  - Migrations para tabelas clientes e agendamentos
  - Global Query Filters para isolamento multi-tenant
  - Repositórios com queries otimizadas e índices
  - Seeds de dados para testes (barbearias, barbeiros, serviços)
  - **Bloqueado por**: 1.0
  - **Desbloqueia**: 3.0, 5.0

### Fase 2: Autenticação (2 dias)

- [ ] **3.0** Application: DTOs, Validators e Use Cases de Autenticação (Cadastro/Login)
  - DTOs de Input/Output com validadores FluentValidation
  - CadastrarClienteUseCase (cadastro automático no primeiro agendamento)
  - LoginClienteUseCase (telefone + nome, sem senha)
  - Exceções customizadas e testes unitários
  - **Bloqueado por**: 1.0, 2.0
  - **Desbloqueia**: 4.0

- [ ] **4.0** API: Endpoints AuthCliente (cadastro/login) + JWT + Swagger
  - Controller AuthClienteController (POST cadastro, POST login)
  - Middleware JWT Authentication + TenantContextMiddleware
  - ExceptionHandlerMiddleware com Problem Details
  - Documentação Swagger completa
  - Testes de integração (201, 200, 401, 404, 422)
  - **Bloqueado por**: 3.0
  - **Desbloqueia**: 7.0, 8.0, 14.0 (Frontend)

### Fase 3: Consulta (2-3 dias)

- [ ] **5.0** Application: Listar Barbeiros e Serviços (consulta)
  - ListarBarbeirosUseCase e ListarServicosUseCase
  - Repositórios otimizados (sem N+1)
  - DTOs e mapeamento AutoMapper
  - Testes unitários
  - **Bloqueado por**: 2.0
  - **Desbloqueia**: 8.0
  - **Paralelizável**: ✅ Sim (pode ser feito em paralelo após 2.0)

- [ ] **6.0** Application: Algoritmo de Disponibilidade (+ Cache 5min)
  - ConsultarDisponibilidadeUseCase
  - Algoritmo de cálculo de slots disponíveis (08:00-20:00, intervalos 30min)
  - Lógica de sobreposição de horários
  - Cache IMemoryCache com TTL 5min
  - Testes unitários cobrindo todos os cenários de conflito
  - **Bloqueado por**: 2.0, 5.0
  - **Desbloqueia**: 7.0, 8.0

### Fase 4: Agendamento (3 dias)

- [ ] **7.0** Application: Criar/Cancelar/Listar/Editar Agendamentos (com lock otimista)
  - **CriarAgendamentoUseCase** (validação de conflito OBRIGATÓRIA + transaction)
  - CancelarAgendamentoUseCase (validações: não cancelar concluído/passado)
  - EditarAgendamentoUseCase (validação de conflito)
  - ListarAgendamentosClienteUseCase (filtros: próximos/histórico)
  - Lock otimista para prevenir race conditions
  - Invalidação de cache após criar/cancelar/editar
  - Testes de concorrência (2 clientes, mesmo horário)
  - **Bloqueado por**: 2.0, 5.0, 6.0
  - **Desbloqueia**: 8.0

- [ ] **8.0** API: Endpoints Barbeiros/Serviços/Agendamentos (REST + autorização)
  - BarbeirosController (GET listar, GET disponibilidade)
  - ServicosController (GET listar)
  - AgendamentosController (POST criar, GET listar, DELETE cancelar, PUT editar)
  - Autorização JWT obrigatória + validação de tenant
  - Rate limiting (100 req/min)
  - Documentação Swagger completa
  - Testes de integração para isolamento multi-tenant
  - **Bloqueado por**: 4.0, 5.0, 6.0, 7.0
  - **Desbloqueia**: 9.0, 14.0 (Frontend)

### Fase 5: Testes e Observabilidade (2 dias)

- [ ] **9.0** Testes de Integração: Autenticação, Isolamento Multi-tenant e Agendamentos
  - Setup: WebApplicationFactory + Testcontainers (PostgreSQL)
  - Testes de isolamento multi-tenant (barbearia A não vê dados de B)
  - Testes de concorrência (lock otimista)
  - Testes de autorização (401, 403)
  - Cobertura de integração > 70%
  - **Bloqueado por**: 4.0, 8.0
  - **Desbloqueia**: 14.0 (Frontend pode iniciar com mais confiança)

- [ ] **10.0** Observabilidade: Logging Estruturado, Métricas Prometheus e Healthchecks
  - Serilog com logs estruturados + Correlation ID
  - Métricas Prometheus: agendamentos, conflitos, cache hit rate, latência
  - Healthchecks: PostgreSQL, IMemoryCache, endpoints críticos
  - Dashboard Grafana (arquivo JSON)
  - Alertas Prometheus (alta taxa de conflitos, database down)
  - **Bloqueado por**: 4.0, 8.0
  - **Paralelizável**: ✅ Sim (pode ser feito em paralelo com 9.0)

## Análise de Paralelização

### Caminho Crítico (Sequencial)

```
1.0 → 2.0 → 3.0 → 4.0 → 6.0 → 7.0 → 8.0 → 9.0
```

**Justificativa**: Estes componentes têm dependências diretas e formam a espinha dorsal do sistema.

### Lane Paralela 1: Consultas (após 2.0)

```
2.0 → 5.0 (Barbeiros/Serviços) → 8.0
```

**Justificativa**: Consultas são independentes de autenticação, apenas dependem de repositórios.

### Lane Paralela 2: Observabilidade (após 8.0)

```
8.0 → 10.0 (Logging/Métricas)
```

**Justificativa**: Observabilidade pode ser implementada em paralelo com testes de integração.

### Oportunidades de Paralelização

**Após 2.0 completo**:
- ✅ Iniciar 5.0 (Consultas) em paralelo com 3.0 (Autenticação)
- 🔹 Ganho: ~1 dia

**Após 8.0 completo**:
- ✅ Iniciar 10.0 (Observabilidade) em paralelo com 9.0 (Testes de Integração)
- 🔹 Ganho: ~1 dia

**Total de Ganho com Paralelização**: 2 dias (de 16-17 dias para 14-15 dias)

## Dependências Externas

### Pré-requisitos Obrigatórios

1. **Sistema Multi-tenant Base** (PRD-5):
   - JWT Service com contexto de barbearia
   - Middleware TenantContext
   - Global Query Filters configurados
   - **Impacto se não estiver pronto**: Bloqueador para 5.0 em diante

2. **Tabelas Base**:
   - `barbearias` (PRD-1): Código único, nome, ativa
   - `barbeiros` (PRD-2): Nome, foto, especialidades, ativo
   - `servicos`: Nome, descrição, duração, preço, ativo
   - **Impacto se não estiverem prontas**: Bloqueador para 2.0 (migrations)

3. **Seeds de Dados para Testes**:
   - Pelo menos 1 barbearia ativa com código TEST123
   - Pelo menos 2 barbeiros ativos
   - Pelo menos 2 serviços (Corte, Barba)
   - **Impacto se não existirem**: Testes falharão

### Configurações de Ambiente

- PostgreSQL 15+ configurado
- .NET 8 SDK
- Variáveis de ambiente: JWT:SecretKey, ConnectionStrings:DefaultConnection

## Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Sistema multi-tenant não está pronto | Média | Alto | Implementar multi-tenant base primeiro (coordenar com time) |
| Race conditions em agendamentos | Alta | Crítico | Testes de concorrência obrigatórios na tarefa 7.0 e 9.0 |
| Performance de disponibilidade ruim | Média | Médio | Cache com TTL 5min (tarefa 6.0) + índices otimizados |
| Testes de integração lentos | Alta | Baixo | Testcontainers otimizado + execução paralela |
| Complexidade do algoritmo de conflito | Média | Alto | Testes unitários extensivos + revisão de código |

## Métricas de Sucesso

### Backend

- ✅ Cobertura de testes unitários > 80%
- ✅ Cobertura de testes de integração > 70%
- ✅ Performance: criar agendamento em < 500ms (p95)
- ✅ Performance: consultar disponibilidade em < 300ms (p95)
- ✅ Zero conflitos de horário em produção (validação funciona)
- ✅ 100% de isolamento multi-tenant (testes comprovam)
- ✅ Cache hit rate > 80% em disponibilidade

### Funcional (após Frontend)

- ✅ Tempo médio de agendamento completo < 3 minutos
- ✅ Taxa de conversão (acesso → agendamento) > 60%
- ✅ Taxa de cancelamento < 15%

## Notas Importantes

### Validação de Conflito é CRÍTICA

A lógica de validação de conflito de horários (tarefa 7.0) é o componente mais crítico do sistema. DEVE:
- Usar transaction com lock otimista
- Validar ANTES de inserir no banco
- Cobrir todos os cenários de sobreposição (testes obrigatórios)

### Isolamento Multi-tenant é OBRIGATÓRIO

Todos os endpoints devem validar que:
- Token contém barbeariaId válido
- Recurso solicitado pertence à mesma barbearia do token
- Testes de integração DEVEM comprovar isolamento (tarefa 9.0)

### Simplicidade MVP

- Autenticação simplificada (telefone + nome, sem senha ou SMS)
- Sem notificações (futuro)
- Sem políticas de cancelamento com prazo (futuro)
- Horário fixo 08:00-20:00 (não configurável no MVP)
