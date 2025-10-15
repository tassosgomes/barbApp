# Especificação Técnica — UI Gestão de Barbeiros e Serviços (Admin da Barbearia)

## Resumo Executivo

Esta Tech Spec descreve como implementar a UI de gestão de Barbeiros, Serviços e visualização de Agenda para o perfil Admin da Barbearia, garantindo isolamento multi-tenant via JWT e aderência aos contratos documentados em `docs/api-contracts-barbers-management.md`. A solução será construída com React + TypeScript (Vite), estilização com Tailwind e componentes Radix/shadcn, formulários com React Hook Form + Zod, e camada de dados com TanStack Query (React Query) + Axios com interceptors de autenticação. 

Decisões principais: 
- Adotar TanStack Query para cache, sincronização de dados, paginação e polling da agenda (30s), alinhando-se ao padrão do repositório (rules/react.md). 
- Padronizar “ativar/inativar” por toggle lógico (isActive) na camada de serviço do frontend. Enquanto o backend formaliza endpoints específicos, mapearemos DELETE como “inativação” (soft-delete) e manteremos uma única função de serviço para alternar status, permitindo troca transparente do contrato no futuro. 
- Estado de filtros/paginação sincronizado com URL (search params) para compartilhamento/refresh. 
- Observabilidade mínima: toasts para sucesso/erros e logging centralizado nos interceptors; hooks de boundary de erro; integração futura com Sentry (fora deste MVP, mas prevista). 

## Arquitetura do Sistema

### Visão Geral dos Componentes

- Camada de Dados
  - `axios` instance (`src/services/api.ts`) com interceptors (JWT, 401 redirect). 
  - Serviços dedicados: `barbers.service.ts`, `services.service.ts`, `schedule.service.ts` (novos), além de `barbershop.service.ts` existente. 
  - TanStack Query para queries e mutations, controle de estado remoto, invalidação de cache, paginação, polling. 
- Camada de Tipos e Esquemas
  - Tipos TS em `src/types/{barber.ts, service.ts, schedule.ts, filters.ts}`. 
  - Schemas Zod em `src/schemas/{barber.ts, service.ts}` para validação dos formulários. 
- Camada de Hooks
  - Hooks especializados: `useBarbers`, `useBarberMutations`, `useServices`, `useServiceMutations`, `useSchedule` (Polling), `useUrlState` (filtros via URL). 
- UI/Pages
  - Barbeiros: `BarbersListPage` (lista + filtros + paginação + modal criar/editar), `BarberForm` (react-hook-form + zod). 
  - Serviços: `ServicesListPage` (lista + filtros + paginação + modal criar/editar), `ServiceForm`. 
  - Agenda: `SchedulePage` (lista como padrão, toggle para calendário opcional), filtros por barbeiro/data/status, polling 30s. 
- Componentes Compartilhados
  - `DataTable` (paginação, ordenação mínima), `FiltersBar`, `StatusBadge`, `ConfirmDialog`, `DatePicker`, `Toast` (Radix Toast). 
- Fluxo de Dados (alto nível)
  - UI → Hooks (React Query) → Serviços Axios → API. 
  - URL query params ↔ filtros/paginação (sincronização bidirecional). 

## Design de Implementação

### Interfaces Principais

Exemplos de contratos internos do frontend (TypeScript). Funções de serviço encapsulam detalhes de endpoints, permitindo refactor sem impacto na UI.

```ts
// src/services/barbers.service.ts (contrato)
export interface BarberFilters {
  searchName?: string;
  isActive?: boolean; // true | false | undefined (todos)
  page?: number;
  pageSize?: number;
}

export interface BarberService {
  list(filters: BarberFilters): Promise<PaginatedResponse<Barber>>;
  getById(id: string): Promise<Barber>;
  create(req: CreateBarberRequest): Promise<Barber>;
  update(id: string, req: UpdateBarberRequest): Promise<Barber>;
  toggleActive(id: string, isActive: boolean): Promise<void>; // mapeia para DELETE ou /status
}
```

```ts
// src/services/services.service.ts (contrato)
export interface ServiceFilters {
  searchName?: string;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface BarbershopServiceService {
  list(filters: ServiceFilters): Promise<PaginatedResponse<BarbershopService>>;
  getById(id: string): Promise<BarbershopService>;
  create(req: CreateServiceRequest): Promise<BarbershopService>;
  update(id: string, req: UpdateServiceRequest): Promise<BarbershopService>;
  toggleActive(id: string, isActive: boolean): Promise<void>;
}
```

```ts
// src/services/schedule.service.ts (contrato)
export interface ScheduleFilters {
  date?: string; // YYYY-MM-DD
  barberId?: string; // opcional: filtra por barbeiro
  status?: AppointmentStatus; // opcional
}

export interface ScheduleService {
  list(filters: ScheduleFilters): Promise<{ appointments: Appointment[] }>;
}
```

Hooks baseados em TanStack Query (exemplos):

```ts
// src/hooks/useBarbers.ts (exemplo)
export function useBarbers(filters: BarberFilters) {
  return useQuery({
    queryKey: ['barbers', filters],
    queryFn: () => barbersService.list(filters),
    staleTime: 30_000,
    keepPreviousData: true,
  });
}

export function useBarberMutations() {
  const qc = useQueryClient();
  const create = useMutation({
    mutationFn: (req: CreateBarberRequest) => barbersService.create(req),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['barbers'] }),
  });
  // update/toggleActive similares
  return { create };
}
```

### Modelos de Dados

- Entidades (frontend)
  - `Barber`: { id, name, email, phoneFormatted, services: ServiceSummary[], isActive, createdAt }
  - `BarbershopService`: { id, name, description, durationMinutes, price, isActive }
  - `Appointment`: { id, barberId, barberName, customerId, customerName, startTime, endTime, serviceName, status }
- Tipos de request
  - `CreateBarberRequest`: { name, email, password, phone, serviceIds[] }
  - `UpdateBarberRequest`: { name, phone, serviceIds[] }
  - `CreateServiceRequest`: { name, description, durationMinutes, price }
  - `UpdateServiceRequest`: { name, description, durationMinutes, price }
- Zod Schemas (validação client-side)
  - BarberForm: valida email, telefone BR, name ≤ 100, serviceIds não vazia.
  - ServiceForm: nome único (validação server-side com feedback 409), duração > 0, preço ≥ 0, formatação pt-BR.

### Endpoints de API

Usaremos os contratos em `docs/api-contracts-barbers-management.md`:

- Barbeiros
  - POST `/api/barbers` — criar
  - GET `/api/barbers` — listar com filtros `isActive`, `searchName`, `page`, `pageSize`
  - GET `/api/barbers/{id}` — obter
  - PUT `/api/barbers/{id}` — atualizar
  - DELETE `/api/barbers/{id}` — inativar (mapeado como toggle no serviço)
- Agenda
  - GET `/api/barbers/schedule?date=YYYY-MM-DD&barberId={?}` — listar agenda da equipe
- Serviços
  - POST `/api/barbershop-services` — criar
  - GET `/api/barbershop-services` — listar com filtros `isActive`, `searchName`, `page`, `pageSize`
  - GET `/api/barbershop-services/{id}` — obter
  - PUT `/api/barbershop-services/{id}` — atualizar
  - DELETE `/api/barbershop-services/{id}` — inativar (mapeado como toggle no serviço)

Notas de implementação:
- Todos os requests com header `Authorization: Bearer {token}` via interceptor.
- Paginação: respostas padronizadas para `PaginatedResponse<T>` (normalização local, como já feito em `barbershop.service.ts`).
- Datas em ISO (UTC). Exibir local (pt-BR) usando Intl.DateTimeFormat; considerar timezone do navegador.

## Pontos de Integração

- Backend BarbApp API (.NET): JWT role `AdminBarbearia` embutida no token determina tenant. Nenhum `barbeariaId` no frontend. 
- Viacep (já existe `viacep.service.ts`) não é requerido nesta UI, mas permanece disponível. 
- Observabilidade futura: Sentry (ver tasks/prd-observabilidade-sentry/). Nesta fase, logs de erro via interceptors + toasts.

Autenticação & Falhas:
- 401: interceptor limpa token e redireciona `/login` (já implementado).
- Timeouts e erros de rede: retries configuráveis no React Query (default 3 com backoff). Mostrar mensagens amigáveis.
- Idempotência de ações: botões “Salvar” desabilitados durante submit; cuidado com duplo clique.

## Análise de Impacto

| Componente Afetado                 | Tipo de Impacto               | Descrição & Nível de Risco                                 | Ação Requerida                 |
| ---------------------------------- | ----------------------------- | ----------------------------------------------------------- | ------------------------------ |
| `src/services/api.ts`              | Sem mudança (reuso)           | Interceptors já atendem JWT/401. Baixo risco.              | Apenas reusar                  |
| Novo: `barbers.service.ts`         | Adição de serviço             | Encapsular CRUD e toggle status. Baixo risco.              | Implementar                    |
| Novo: `services.service.ts`        | Adição de serviço             | Encapsular CRUD e toggle status. Baixo risco.              | Implementar                    |
| Novo: `schedule.service.ts`        | Adição de serviço             | Endpoint agenda com filtros e polling. Baixo risco.        | Implementar                    |
| Hooks (novos)                      | Adição de hooks               | Queries/mutations com TanStack Query. Baixo risco.         | Implementar                    |
| Páginas `Barbers/Services/Schedule`| Novas páginas                 | Conforme PRD; roteamento protegido. Médio (UX).            | Implementar                    |
| Dependências (React Query)         | Nova dependência              | Aderir a `rules/react.md`. Baixo risco.                    | Adicionar pacote               |
| Testes (unit/integration/e2e)      | Ampliação da suíte            | Cobrir formulários, listas, filtros e agenda. Médio.       | Escrever testes                |

Performance:
- Listas paginadas e filtradas (metas: lista <1s). Usar `keepPreviousData` para transição fluida entre páginas.
- Agenda: `refetchInterval: 30_000` e `staleTime` ajustado para minimizar chamadas desnecessárias.

## Abordagem de Testes

### Testes Unitários

- Componentes de formulário (`BarberForm`, `ServiceForm`):
  - Validação com zod (e.g., email inválido, duração negativa, preço negativo) — exibir erros inline.
  - Submissão bem-sucedida dispara mutation e toasts.
- Componentes de lista (`BarbersList`, `ServicesList`):
  - Renderização com dados mockados; paginação; filtros; status badge.
- Hooks (`useBarbers`, `useServices`, `useSchedule`):
  - Testar query keys, polling e invalidações (mock de serviços + MSW).

### Testes de Integração

- Páginas completas com MSW:
  - Barbeiros: criar/editar/toggle e refletir atualização na lista.
  - Serviços: criar/editar/toggle e refletir atualização na lista.
  - Agenda: filtros por barbeiro/data/status, navegação dia anterior/próximo, polling.
- Erros comuns: 409 (conflito e-mail/serviço), 422 (validação), 401 (redireciona para login).

E2E (Playwright):
- Fluxos principais do PRD: criar barbeiro, criar serviço, visualizar agenda com filtros.
- Checks de acessibilidade básicos (focus, teclado) nos modais.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. Tipos e Schemas
   - Adicionar tipos TS (`barber.ts`, `service.ts`, `schedule.ts`, `filters.ts`). 
   - Schemas Zod para formulários. 
2. Serviços de API
   - `barbers.service.ts`, `services.service.ts`, `schedule.service.ts` com normalização de paginação.
3. Hooks com React Query
   - `useBarbers`, `useBarberMutations`, `useServices`, `useServiceMutations`, `useSchedule` (polling).
4. Páginas e Componentes
   - Listas, formulários (modal/página), filtros, badges, dialogs, date picker. 
5. Roteamento e Proteção
   - Rotas protegidas por `useAuth` (já existente). 
6. Testes Unitários/Integração
   - Cobrir componentes e hooks; MSW para API. 
7. E2E
   - Fluxos críticos com Playwright. 

### Dependências Técnicas

- Adicionar: `@tanstack/react-query`, `@tanstack/react-query-devtools`.
- Utilidades: `date-fns` (ou `dayjs`) para datas; usar `Intl` para moedas/datas pt-BR.
- Backend disponível com endpoints conforme contratos.

## Monitoramento e Observabilidade

- Logs:
  - Interceptors já registram request/response/error. 
  - Adicionar logs de eventos críticos: falha ao salvar, conflitos 409, validações 422. 
- Métricas (futuro): integrar com Sentry ou solução adotada no repositório (fora deste MVP). 
- UX feedback: Radix Toast para sucesso/erro; skeletons/loading states em listas e formulários. 
- Error Boundary: componente de alto nível para capturar erros de renderização.

## Considerações Técnicas

### Decisões Principais

- TanStack Query: atende cache, paginação e polling de agenda, reduzindo código manual e alinhando com `rules/react.md` ("Sempre utilize React Query"). 
- Toggle de status (isActive): abstração no serviço que mapeia para DELETE temporariamente, garantindo compatibilidade imediata com o backend e fácil migração para PATCH/PUT no futuro. 
- URL como fonte de verdade para filtros/página: melhora compartilhamento e refresh, atende PRD 1.7 e 2.7. 
- Lista como visão padrão da agenda; calendário opcional (toggle). Justificativa: implementação incremental, metas de desempenho e simplicidade de navegação no MVP.

### Riscos Conhecidos

- Divergência API vs UI para inativação: mitigado por abstração `toggleActive`. 
- Timezone e exibição de horários: padronizar conversão UTC → local via Intl; validar com backend. 
- Performance em agendas muito densas: considerar virtualização de lista se necessário. 
- Ausência atual de React Query no projeto: requer adicionar dependência e bootstrap do `QueryClientProvider` no `App`. 

### Requisitos Especiais

- Performance metas PRD: 
  - Listas < 1s: usar `keepPreviousData`, desduplicação de requests e paginação. 
  - Agenda < 3s: cache + polling 30s, queries seletivas por barbeiro/data/status. 
- Segurança/LGPD:
  - Evitar exibir PII além do necessário; mascarar telefone em listagens; logs não devem conter dados sensíveis. 
  - Garantir que o token nunca é logado. 

### Conformidade com Padrões

- `rules/react.md`
  - Componentes funcionais com TSX, Tailwind, Radix/shadcn. 
  - “Sempre utilize React Query”: adicionado. 
  - Hooks nomeados com `use*`: seguir convenção. 
- `rules/tests-react.md`
  - Testes com Vitest + RTL + user-event; MSW para integração. 
  - AAA/GWT e organização próxima aos componentes. 
- Outros
  - Axios centralizado, interceptors para JWT/401. 
  - Evitar componentes > 300 linhas; dividir por responsabilidade. 

## Endpoints de API (Detalhamento Mínimo)

- Barbeiros
  - POST `/api/barbers` — cria (201). 
  - GET `/api/barbers` — lista (200), filtros: `isActive`, `searchName`, `page`, `pageSize`. 
  - GET `/api/barbers/{id}` — obtém (200). 
  - PUT `/api/barbers/{id}` — atualiza (200). 
  - DELETE `/api/barbers/{id}` — inativa (204). 
- Serviços
  - POST `/api/barbershop-services` — cria (201). 
  - GET `/api/barbershop-services` — lista (200), filtros. 
  - GET `/api/barbershop-services/{id}` — obtém (200). 
  - PUT `/api/barbershop-services/{id}` — atualiza (200). 
  - DELETE `/api/barbershop-services/{id}` — inativa (204). 
- Agenda
  - GET `/api/barbers/schedule?date=YYYY-MM-DD&barberId={?}` — agenda (200). 

Códigos de erro: 400/401/403/404/409/422 conforme doc; exibir mensagens claras (Servidor → UI).

## Contratos de Componentes (resumo)

- BarbersListPage
  - Entradas: query params (search, status, page, pageSize). 
  - Saídas: tabela com `name`, `email`, `phone`, `services`, `status`, `createdAt` e ações (editar, ativar/inativar). 
- BarberForm (modal ou rota dedicada)
  - Entradas: `barber?` (edição). 
  - Validações: zod (nome, email, telefone, serviços). 
  - Ações: create/update; toasts; refetch lista.
- ServicesListPage / ServiceForm: análogo a barbeiros.
- SchedulePage
  - Entradas: filtros (barberId|all, date, status) via URL; visão padrão lista. 
  - Polling: 30s (`refetchInterval`); destaque de horário atual; cores por status. 

## Especificação de UI e Interações

- Filtros persistentes na URL; botão “Limpar” volta ao default. 
- Loading states visíveis (skeleton/shimmer) em listas e formulário (botão “Salvar” com spinner). 
- Erros 409/422 exibidos próximos ao campo e toast com resumo. 
- Confirmação para inativar (dialog) com informações de impacto. 
- Acessibilidade: foco visível, labels associados, navegação por teclado básica.

## Itens de Implementação (resumo de arquivos)

- Tipos: `src/types/barber.ts`, `src/types/service.ts`, `src/types/schedule.ts`, `src/types/filters.ts`. 
- Schemas: `src/schemas/barber.ts`, `src/schemas/service.ts`. 
- Serviços: `src/services/barbers.service.ts`, `src/services/services.service.ts`, `src/services/schedule.service.ts`. 
- Hooks: `src/hooks/useBarbers.ts`, `src/hooks/useBarberMutations.ts`, `src/hooks/useServices.ts`, `src/hooks/useServiceMutations.ts`, `src/hooks/useSchedule.ts`, `src/hooks/useUrlState.ts`. 
- Páginas: `src/pages/Barbers/`, `src/pages/Services/`, `src/pages/Schedule/`. 
- Componentes compartilhados: `src/components/DataTable.tsx`, `src/components/FiltersBar.tsx`, `src/components/StatusBadge.tsx`, `src/components/ConfirmDialog.tsx`, `src/components/DatePicker.tsx`, `src/components/ToastProvider.tsx`. 
- Bootstrap React Query: envolver `App` com `QueryClientProvider` e opcionalmente Devtools em dev. 

## Notas de Implementação e Erros Comuns

- Normalizar paginação similar a `barbershop.service.ts` para manter consistência (page, pageSize, totalCount, totalPages). 
- Evitar estados duplicados: preferir React Query para fonte de verdade remota; estado local apenas para UI (modais, inputs controlados). 
- Debounce de busca (já existe `useDebounce`) para reduzir tráfego. 
- Não logar tokens/PII; mascarar telefone quando aplicável. 

---

## Esclarecimentos Técnicos (Perguntas)

1. Ativar/Inativar vs Delete: podemos padronizar endpoints de status (`PUT/PATCH /{id}/status {isActive}`) no backend? Enquanto isso, confirmamos se `DELETE` hoje é soft-delete (não remoção física) para refletir UI. 
2. Agenda — timezone: confirmar suposição de horários em UTC e exibição no fuso local do navegador. 
3. Agenda — visão padrão: manter “lista” como default (ok para MVP)? Calendário fica como toggle opcional. 
4. Preço: confirmar formatação `pt-BR` (R$) e regra de arredondamento (half-up?). 
5. Conflitos: e-mail duplicado de barbeiro (por barbearia) retorna 409 com mensagem padronizada? Nome de serviço duplicado idem? 
6. Catálogo de serviços: existe limite de duração/preço? Precisamos de faixas validadas client-side? 
7. LGPD: há campos de PII adicionais a ocultar nas listas (ex.: telefone completo) ou política de truncamento/máscara? 

---

## Checklist de Qualidade

- [x] PRD revisado e requisitos técnicos mapeados
- [x] Análise do repositório e das regras concluída
- [x] Esclarecimentos levantados
- [x] Tech Spec gerada usando o template e adaptada ao frontend
- [x] Caminho de saída definido em `tasks/prd-gestao-barbeiros-admin-barbearia-ui/techspec.md`

---

## Apêndice: Trechos Exemplificativos

Schemas Zod (excertos):

```ts
export const createBarberSchema = z.object({
  name: z.string().min(1).max(100),
  email: z.string().email(),
  password: z.string().min(8),
  phone: z.string().regex(/^\(\d{2}\) \d{4,5}-\d{4}$/),
  serviceIds: z.array(z.string().uuid()).min(1),
});

export const updateBarberSchema = createBarberSchema.omit({ email: true, password: true });

export const serviceSchema = z.object({
  name: z.string().min(1),
  description: z.string().min(1),
  durationMinutes: z.number().int().positive(),
  price: z.number().min(0),
});
```

React Query Provider (bootstrap):

```tsx
const queryClient = new QueryClient({
  defaultOptions: {
    queries: { refetchOnWindowFocus: false, retry: 2, staleTime: 15_000 },
  },
});

root.render(
  <QueryClientProvider client={queryClient}>
    <App />
    {import.meta.env.DEV && <ReactQueryDevtools initialIsOpen={false} />}
  </QueryClientProvider>
);
```

Polling Agenda:

```ts
export function useSchedule(filters: ScheduleFilters) {
  return useQuery({
    queryKey: ['schedule', filters],
    queryFn: () => scheduleService.list(filters),
    refetchInterval: 30_000,
    staleTime: 25_000,
  });
}
```

---

## Mapeamento a Padrões (@rules)

- React: funcionais, TSX, Tailwind, Radix/shadcn, hooks nomeados, evitar componentes grandes. 
- Tests-React: Jest/Vitest + RTL + user-event; MSW para integração; AAA/GWT; cobertura com `vitest --coverage`. 
- HTTP: uso consistente de Axios centralizado e tratamento de erros nos interceptors. 
- Logging: logs de erro significativos; sem dados sensíveis; toasts para feedback. 
