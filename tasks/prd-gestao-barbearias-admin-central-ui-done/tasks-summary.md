# Implementação Admin Central UI - Resumo de Tarefas

**Feature**: Admin Central - Interface de Gestão de Barbearias
**Status**: 🔵 Planejamento
**Timeline**: 13-15 dias
**Effort Total**: ~80-95 horas

## Visão Geral

Desenvolvimento completo da interface administrativa SPA (Single Page Application) para gestão de barbearias, incluindo operações de CRUD, autenticação, validação robusta e cobertura de testes >70%.

**Stack Tecnológica**:
- React 18+ com Vite 5+
- TypeScript 5+
- TailwindCSS 3.4+ + shadcn/ui
- React Router v6 + React Hook Form 7+ + Zod 3+
- Vitest + Playwright para testes

## Estrutura de Fases

### 📋 Fase 1: Foundation (CRÍTICA, 2 dias)
Base técnica e estrutura do projeto

### 📋 Fase 2: Type Safety and API (CRÍTICA, 2 dias)
Tipos TypeScript, validações Zod e serviços da API

### 📋 Fase 3: Authentication (CRÍTICA, 2 dias)
Sistema completo de autenticação com JWT

### 📋 Fase 4: Routing and Layout (CRÍTICA, 1 dia)
Configuração de rotas e layout base

### 📋 Fase 5: Reusable Components (ALTA, 2 dias)
Componentes shadcn/ui e custom components

### 📋 Fase 6-9: CRUD Operations (ALTA, 3 dias cada - PARALELO)
Implementação completa das operações de gestão

### 📋 Fase 10: Additional Features (MÉDIA, 1 dia)
Funcionalidades de desativar/reativar

### 📋 Fase 11: Testing (ALTA, 3 dias)
Testes unitários, integração e E2E

### 📋 Fase 12: Refinement (MÉDIA, 1 dia - PARALELO)
Acessibilidade, performance e documentação

## Lista Completa de Tarefas

### Phase 1: Foundation (Days 1-2)
- [ ] **Task 1.1**: Project Setup and Initial Configuration (Crítica, 1 dia)
- [ ] **Task 1.2**: Folder Structure and Path Aliases (Crítica, 0.5 dia)

### Phase 2: Type Safety and API (Days 2-4)
- [ ] **Task 2.1**: TypeScript Types and Interfaces (Crítica, 0.5 dia)
- [ ] **Task 2.2**: Zod Validation Schemas (Crítica, 1 dia)
- [ ] **Task 2.3**: Axios Configuration and Barbershop Service (Crítica, 1 dia)

### Phase 3: Authentication (Days 4-6)
- [ ] **Task 3.1**: Login Page Implementation (Crítica, 1 dia)
- [ ] **Task 3.2**: Auth Hooks and Protected Routes (Crítica, 0.5 dia)
- [x] **Task 3.3**: API Interceptors and Session Management (Crítica, 0.5 dia)

### Phase 4: Routing and Layout (Day 6)
- [ ] **Task 4.1**: React Router Configuration (Crítica, 0.5 dia)
- [ ] **Task 4.2**: Header Component and Base Layout (Crítica, 0.5 dia)

### Phase 5: Reusable Components (Days 7-8)
- [ ] **Task 5.1**: shadcn/ui Components Installation (Alta, 0.5 dia)
- [ ] **Task 5.2**: Custom Form Components (Alta, 0.5 dia)
- [ ] **Task 5.3**: Table and Data Display Components (Alta, 0.5 dia)
- [ ] **Task 5.4**: Modal and Feedback Components (Alta, 0.5 dia)

### Phase 6: CRUD - List Page (Days 9-11, PARALELO)
- [ ] **Task 6.1**: List Page with Table and useBarbershops Hook (Alta, 3 dias)

### Phase 7: CRUD - Create Page (Days 9-11, PARALELO)
- [ ] **Task 7.1**: Create Page with Form and Validation (Alta, 3 dias)

### Phase 8: CRUD - Edit Page (Days 9-11, PARALELO)
- [ ] **Task 8.1**: Edit Page with Dirty State Detection (Alta, 3 dias)

### Phase 9: CRUD - Details Page (Days 9-11, PARALELO)
- [ ] **Task 9.1**: Details Page Read-Only View (Alta, 3 dias)

### Phase 10: Additional Features (Day 12)
- [ ] **Task 10.1**: Deactivate and Reactivate Functionality (Média, 1 dia)

### Phase 11: Testing (Days 12-14)
- [ ] **Task 11.1**: Test Configuration and Setup (Alta, 0.5 dia)
- [ ] **Task 11.2**: Unit Tests for Components and Hooks (Alta, 1 dia)
- [ ] **Task 11.3**: Integration Tests for Services (Alta, 0.5 dia)
- [ ] **Task 11.4**: E2E Tests with Playwright (Alta, 1 dia)

### Phase 12: Refinement (Day 15, PARALELO)
- [ ] **Task 12.1**: Accessibility Improvements (Média, 0.5 dia)
- [ ] **Task 12.2**: Performance Optimization (Média, 0.25 dia)
- [ ] **Task 12.3**: Documentation (Média, 0.25 dia)

## Análise de Paralelização

### Critical Path (Sequential)
1. Phases 1-5: Foundation, Types, Auth, Routing, Components (7 dias)
2. Phase 11: Testing execution (3 dias)
3. Total Critical Path: ~10 dias

### Parallel Lanes
**Lane 1 (Days 9-11)**: List Page Implementation
**Lane 2 (Days 9-11)**: Create Page Implementation
**Lane 3 (Days 9-11)**: Edit Page Implementation
**Lane 4 (Days 9-11)**: Details Page Implementation

**Lane 5 (Day 15)**: Accessibility + Performance + Documentation (can run in parallel)

### Optimization Opportunities
- **CRUD Pages (Tasks 6.1, 7.1, 8.1, 9.1)**: Can be developed in parallel by different developers after Phase 5 completes
- **Testing (Phase 11)**: Can start unit tests as soon as components are ready, don't need to wait for all CRUD pages
- **Refinement (Phase 12)**: All three tasks can run simultaneously

### Timeline with Parallelization
- **Sequential Work**: 10 dias (critical path)
- **Parallel Work**: 3 dias (CRUD pages in parallel)
- **Final Polish**: 1 dia (refinement in parallel)
- **Total Timeline**: 13-15 dias (vs 23 dias sequential)

## Dependencies Map

```
Task 1.1 → Task 1.2 → Tasks 2.1, 2.2, 2.3 → Tasks 3.1, 3.2, 3.3 → Tasks 4.1, 4.2 → Tasks 5.1, 5.2, 5.3, 5.4

After Phase 5 completes:
├─ Task 6.1 (List) ────┐
├─ Task 7.1 (Create) ──┤─→ Task 10.1 → Phase 11 (Testing) → Phase 12 (Refinement)
├─ Task 8.1 (Edit) ────┤
└─ Task 9.1 (Details) ─┘
```

## Quality Gates

### Phase Exit Criteria

**Phase 1**: Project builds successfully, folder structure created, path aliases working
**Phase 2**: All types defined, Zod schemas validate correctly, API service methods tested
**Phase 3**: Login flow works end-to-end, protected routes redirect correctly, token management functional
**Phase 4**: All routes accessible, navigation works, layout renders correctly
**Phase 5**: All reusable components installed and working, Storybook stories created (optional)
**Phases 6-9**: Each CRUD page fully functional with validation, error handling, and loading states
**Phase 10**: Deactivate/reactivate flow works with confirmations
**Phase 11**: >70% test coverage achieved, all E2E scenarios passing
**Phase 12**: WCAG AA compliance, bundle <500KB, README complete

### Success Metrics
- ✅ All functional requirements from PRD implemented
- ✅ Test coverage >70%
- ✅ Zero critical accessibility violations
- ✅ Initial bundle size <500KB
- ✅ All E2E scenarios passing
- ✅ No console errors in production build

## Risk Management

### High Risk Areas
1. **Authentication Integration**: API contract alignment with backend
2. **Form Validation**: Complex Zod schemas with nested address validation
3. **State Management**: Keeping list filters/pagination state during navigation
4. **Test Coverage**: Achieving >70% with meaningful tests, not just coverage numbers

### Mitigation Strategies
1. **Early API Contract Validation**: Task 2.3 includes API contract testing with backend team
2. **Incremental Validation**: Task 2.2 builds schemas incrementally with unit tests
3. **State Persistence Strategy**: Task 4.1 includes URL state management design
4. **Test-First Approach**: Write tests alongside features, not after completion

## Team Recommendations

### Optimal Team Structure (for parallel execution)
- **Developer 1**: Foundation + Auth + Routing (Tasks 1-4)
- **Developer 2**: Reusable Components (Task 5) → Create Page (Task 7.1)
- **Developer 3**: List Page (Task 6.1) → Testing setup (Task 11.1)
- **Developer 4**: Edit Page (Task 8.1) → Details Page (Task 9.1)

### Solo Developer Timeline
- Follow sequential path with focus sessions
- Phases 1-5: 7 days (critical foundation)
- Phases 6-9: 6 days (one CRUD page per 1.5 days)
- Phases 10-12: 2 days (features + testing + polish)
- Total: 15 days

## Next Steps

1. ✅ Review and approve task structure
2. ✅ Assign tasks to developers (if team) or plan solo execution order
3. 🔄 **Execute Task 1.1**: Project Setup and Initial Configuration
4. Follow task order respecting dependencies
5. Update task status as work progresses
6. Conduct phase exit reviews at each milestone

## References

- **PRD**: `/home/tsgomes/github-tassosgomes/barbApp/tasks/prd-gestao-barbearias-admin-central-ui/prd.md`
- **Tech Spec**: `/home/tsgomes/github-tassosgomes/barbApp/tasks/prd-gestao-barbearias-admin-central-ui/techspec.md`
- **Backend API**: `/src/BarbApp.API/Controllers/BarbershopsController.cs`

---

**Documento gerado**: 2025-10-13
**Versão**: 1.0
**Status**: Aprovado para Execução
