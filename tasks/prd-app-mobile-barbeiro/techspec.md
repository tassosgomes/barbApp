# Tech Spec — App Mobile do Barbeiro (MVP)

## Resumo Executivo

Implementaremos um app mobile (iOS/Android) para barbeiros, focado em visualizar e operar a própria agenda por barbearia (multi-tenant). A solução usará React Native com Expo (TS), TanStack Query para dados, Axios com interceptors para auth JWT, e Secure Storage para token. Fluxos principais: login (e-mail/senha), seleção de barbearia (quando multi‑vínculo), agenda do dia com polling (30s) e pull‑to‑refresh, detalhes + ações (confirmar, cancelar, concluir). Horários são UTC no backend; o app converte para timezone local.

Principais decisões: RN + Expo para agilidade de MVP; TanStack Query para caching/reatividade; padrão de services com Axios e interceptors; UI com React Native Paper + StyleSheet (sem Tailwind/NativeWind) para simplificar; Sentry.io integrado no MVP.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- App Shell (Expo + Router): navegação stack/tab mínima e inicialização de providers (QueryClientProvider, AuthProvider).
- Auth Module: telas de Login; serviço `authService` (login/logout), contexto `AuthContext` para token/usuário e troca de contexto.
- Barbershop Context: listagem de barbearias do barbeiro e seleção de contexto (`barbershopsService`, `BarbershopContext`).
- Schedule Module: telas Agenda do Dia e Detalhe; serviços `scheduleService` e `appointmentsService` (confirm/cancel/complete); hooks de dados com React Query. Refetch imediato ao voltar ao app (foreground) e ao focar a tela (useFocusEffect).
- UI/Design System: componentes RN (React Native Paper) + StyleSheet para estilos; toasts (react-native-flash-message). Estratégia de atualização: server-first (sem optimistic updates); após ações, refetch e sincronização com backend.
- Infra de Dados: `api.ts` (Axios instance + interceptors), `storage.ts` (SecureStore), `config.ts` (envs, baseURL).

Fluxo de dados resumido:
Login -> token salvo (SecureStore) -> (se multi‑vínculo) lista barbearias -> troca de contexto (novo token) -> Agenda (fetch por dia + polling) -> Ações (POST confirm/cancel/complete) -> invalidar/refetch cache.

## Design de Implementação

### Interfaces Principais

```ts
// src/services/authService.ts
export interface AuthResponse {
  token: string;
  user: { id: string; name: string; email: string; role: 'Barbeiro' };
}

export interface AuthService {
  login(email: string, password: string): Promise<AuthResponse>;
  listBarbershops(): Promise<Array<{ id: string; nome: string; codigo: string; isActive: boolean }>>;
  switchContext(novaBarbeariaId: string): Promise<AuthResponse>;
  logout(): Promise<void>;
}

// src/services/scheduleService.ts
export interface BarberAppointmentOutput {
  id: string;
  customerName: string;
  serviceTitle: string;
  startTime: string; // ISO UTC
  endTime: string;   // ISO UTC
  status: 0 | 1 | 2 | 3; // Pending, Confirmed, Completed, Cancelled
}

export interface BarberScheduleOutput {
  date: string; // ISO date (00:00Z)
  barberId: string;
  barberName: string;
  appointments: BarberAppointmentOutput[];
}

export interface ScheduleService {
  getMySchedule(date: Date): Promise<BarberScheduleOutput>;
}

// src/services/appointmentsService.ts
export interface AppointmentDetailsOutput extends BarberAppointmentOutput {
  customerPhone?: string;
  servicePrice?: number;
  serviceDurationMinutes?: number;
  createdAt?: string;
  confirmedAt?: string | null;
  cancelledAt?: string | null;
  completedAt?: string | null;
}

export interface AppointmentsService {
  getById(id: string): Promise<AppointmentDetailsOutput>;
  confirm(id: string): Promise<AppointmentDetailsOutput>;
  cancel(id: string): Promise<AppointmentDetailsOutput>;
  complete(id: string): Promise<AppointmentDetailsOutput>;
}
```

### Modelos de Dados

- Auth
  - Request: `{ email: string; password: string }`
  - Response: `{ token: string; user: { id, name, email, role: 'Barbeiro' } }`
- Barbershops (multi‑vínculo)
  - Response: `[{ id: string; nome: string; codigo: string; isActive: boolean }]`
- Schedule (dia)
  - `BarberScheduleOutput` e `BarberAppointmentOutput` conforme contratos existentes (UTC strings)
- Appointment Details
  - `AppointmentDetailsOutput` com timestamps opcionais (UTC)

Conversões de tempo: usar `date-fns` + `date-fns-tz` para formatar/exibir horários no fuso local (ex.: America/Sao_Paulo).

### Endpoints de API

- Auth
  - POST `/api/auth/barbeiro/login` — body `{ email, password }`
  - GET `/api/barbeiro/barbearias` — lista de vínculos do barbeiro autenticado (seleção de barbearia)
  - POST `/api/auth/barbeiro/trocar-contexto` — body `{ novaBarbeariaId: Guid }` -> novo token contextualizado
- Schedule
  - GET `/api/schedule/my-schedule?date=YYYY-MM-DD` — agenda do barbeiro (dia)
- Appointments
  - GET `/api/appointments/{id}` — detalhes
  - POST `/api/appointments/{id}/confirm` — confirmar (409 se inválido)
  - POST `/api/appointments/{id}/cancel` — cancelar (409 se inválido)
  - POST `/api/appointments/{id}/complete` — concluir (400 antes do horário; 409 transição inválida)

Observações:
- Backend retorna UTC; app exibe no fuso local.
- `GET /api/barbeiro/barbearias` — REQUISITO DE BACKEND PARA O MVP (novo endpoint):
  - Role: `Barbeiro` (Bearer JWT)
  - Response 200: `[{ id: Guid, nome: string, codigo: string, isActive: bool }]`
  - Regras: retornar apenas vínculos ativos; ordenado por `nome`
  - Erros: 401/403/500

## Pontos de Integração

- APIs internas do backend BarbApp (sem serviços de terceiros no MVP).
- Autenticação Bearer JWT via Axios Interceptor.
- Armazenamento de token em SecureStore (Expo) com fallback para AsyncStorage em dev.
- Observabilidade (MVP): Sentry.io integrado (Expo + sentry-expo) para capturar erros não tratados, breadcrumbs básicos (navegação/fetch), e associar `release`/`environment`.

Tratamento de erros (padrões):
- 401: limpar sessão e redirecionar para Login.
- 409: exibir toast de aviso, invalidar/atualizar caches (refetch imediato).
- 400 (complete antes do horário): exibir mensagem específica do backend.

## Análise de Impacto

| Componente Afetado                 | Tipo de Impacto             | Descrição & Nível de Risco                                                  | Ação Requerida                  |
| ---------------------------------- | --------------------------- | ---------------------------------------------------------------------------- | ------------------------------- |
| Backend `/api/auth/barbeiro/login` | Uso conforme contrato       | Troca para e‑mail/senha. Baixo risco.                                        | Confirmar contrato final.       |
| Listagem de barbearias             | Nova API (Obrigatória)      | Implementar `GET /api/barbeiro/barbearias` no backend para seleção de contexto | Implementar + documentar rota.  |
| Troca de contexto                  | Uso conforme contrato       | Gera novo token. Baixo risco.                                                | Sem ação além de documentação.  |
| Schedule/Appointments              | Uso conforme contrato       | Polling (30s) aumenta requisições. Baixo risco.                              | Monitorar carga no backend.     |
| Observabilidade Mobile             | Pós‑MVP                     | Sentry RN/Crashlytics.                                                       | Planejar integração futura.     |

Impacto de performance: polling a cada 30s na agenda visível; usar visibilidade de tela (focus) para pausar/resumir. Uso de React Query para dedupe e cache.

## Abordagem de Testes

### Testes Unitários

- Services (auth/schedule/appointments): simular Axios com MSW/axios-mock-adapter.
- Hooks (useLogin, useSchedule, useAppointmentActions): casos de sucesso, 401, 409, 400.
- Utilitários (timezone, formatadores): conversões de UTC -> local.

### Testes de Integração

- Fluxo Login -> (Seleção) -> Agenda: RN Testing Library, navegando pelas telas e verificando chamadas/estados.
- Ações no agendamento: confirmar/cancelar/concluir com verificação de toasts e invalidação de cache.
- Testes de contrato básicos podem referenciar exemplos em `docs/api-contracts-schedule-barber.md`.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. Infra de dados: `api.ts` (Axios), `config.ts` (baseURL), `storage.ts` (SecureStore), interceptors (401 handler).
2. Auth: tela de Login, `authService`, `AuthContext` (+ testes unitários).
3. Barbershops: implementar `GET /api/barbeiro/barbearias` (backend) e integrar `listBarbershops()` + tela de Seleção + `switchContext()`.
4. Schedule: tela Agenda do Dia, `scheduleService`, hooks de dados, polling/pull‑to‑refresh, refetch em foco/foreground.
5. Appointment Details: modal/bottom sheet e ações (confirm/cancel/complete), handling de 409/400 (refetch e toasts).
6. Observabilidade: integrar Sentry.io (sentry-expo) com DSN/env e captura de erros não tratados.
7. Refinos de UX e estados vazios/erros; testes de integração de fluxos principais.

### Dependências Técnicas

- Backend com rotas ativas e CORS (se web dev com Expo Web). 
- Confirmação do endpoint de listagem de barbearias. 
- Provisionar variáveis `API_BASE_URL` nos ambientes. 

## Monitoramento e Observabilidade

- Sentry.io (MVP):
  - SDK: `sentry-expo` (Expo) ou `@sentry/react-native` se bare workflow.
  - DSN via env: `EXPO_PUBLIC_SENTRY_DSN` (ou `SENTRY_DSN`).
  - Tags: `environment` (development/staging/production), `release` (app-name@semver+commit), `platform` (ios/android).
  - Ativar captura de erros não tratados e unhandled rejections; breadcrumbs de navegação/fetch.
  - Scrubbing de PII ativado; não enviar telefones/emails em extras.
  - Source maps: habilitar upload (EAS/CI) para stacks legíveis.
- Logs de rede: Axios interceptor reportando status/rota em `warn` (4xx) e `error` (5xx/sem rede).
- Métricas futuras: taxa de erro por ação (confirm/cancel/complete), latência média das chamadas (pós‑MVP se necessário).

## Considerações Técnicas

### Decisões Principais

- React Native (Expo) + TypeScript: velocidade de entrega, comunidade, DX consistente com stack React do repositório.
- TanStack Query: cache, invalidação e polling; padrão do repositório (rules/react.md).
- Axios + interceptors: padronização de auth/erro; facilita logs e headers.
- UI lib: React Native Paper + StyleSheet (sem NativeWind) para reduzir dependências e simplificar MVP.
- Observabilidade: Sentry.io no MVP via `sentry-expo`.

Trade-offs:
- RN vs Web responsivo (Capacitor): RN oferece UX nativa superior; custo de UI nativa maior que web.
- Paper+StyleSheet vs NativeWind: menos utilitários, porém menos dependências e menor curva de adoção; preferido pelo critério de simplicidade.

Alternativas rejeitadas:
- Flutter/Kotlin/Swift nativos: maior custo/time-to-market, menos alinhamento com stack existente.
- Expo EAS + push no MVP: adiado para pós‑MVP.

### Riscos Conhecidos

- Endpoint de listagem de barbearias pode não estar formalmente documentado: risco baixo, requer confirmação.
- Tratamento de timezone incorreto pode causar confusão de horários: mitigar com utilitário único e testes.
- Polling em segundo plano pode aumentar consumo: mitigar pausando em background e em telas não ativas.

### Requisitos Especiais

- Performance (UX): agenda do dia < 3s em rede saudável; ações < 1s com feedback otimista opcional (sem confirmar status final antes do refetch).
- Segurança: token em SecureStore; evitar PII em logs; Authorization header em todas as chamadas; limpar token em 401.

### Conformidade com Padrões

- Regras React (rules/react.md):
  - TypeScript: OK; componentes RN em TSX.
  - React Query: OK (TanStack Query para RN).
  - Tailwind: substituído por StyleSheet no RN para simplicidade (desvio justificado: ambiente nativo, evitar dependências extras). 
  - Shadcn UI: não aplicável em RN; uso de React Native Paper.
- Regras HTTP (rules/http.md): headers adequados, timeouts razoáveis, retries limitados (ex.: retry 1x para GET schedule).
- Regras de Logging (rules/logging.md): logs desacoplados, níveis: warn (409), error (500/sem rede).
- Regras de Testes (rules/tests-react.md): RN Testing Library para componentes/hooks; mocks de rede controlados.

---

## Apêndice — Esboços de Código

```ts
// src/infra/api.ts
import axios from 'axios';
import { getToken, clearToken } from './storage';

export const api = axios.create({ baseURL: process.env.EXPO_PUBLIC_API_BASE_URL });

api.interceptors.request.use(async (config) => {
  const token = await getToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

api.interceptors.response.use(
  (res) => res,
  async (err) => {
    if (err?.response?.status === 401) {
      await clearToken();
      // emitir evento global para redirecionar ao Login
    }
    return Promise.reject(err);
  }
);
```

```ts
// src/services/authService.ts
import { api } from '../infra/api';

export const authService = {
  async login(email: string, password: string) {
    const { data } = await api.post('/api/auth/barbeiro/login', { email, password });
    return data as AuthResponse;
  },
  async listBarbershops() {
    const { data } = await api.get('/api/barbeiro/barbearias');
    return data as Array<{ id: string; nome: string; codigo: string; isActive: boolean }>;
  },
  async switchContext(novaBarbeariaId: string) {
    const { data } = await api.post('/api/auth/barbeiro/trocar-contexto', { novaBarbeariaId });
    return data as AuthResponse;
  },
};
```

```ts
// src/services/scheduleService.ts
import { api } from '../infra/api';

export const scheduleService = {
  async getMySchedule(date: Date) {
    const d = date.toISOString().split('T')[0];
    const { data } = await api.get(`/api/schedule/my-schedule?date=${d}`);
    return data as BarberScheduleOutput;
  },
};
```

```ts
// src/screens/ScheduleScreen.tsx (trecho)
import { useFocusEffect } from '@react-navigation/native';
import { AppState } from 'react-native';
import { useQueryClient } from '@tanstack/react-query';

export function useScheduleAutoRefresh(queryKey: unknown[]) {
  const qc = useQueryClient();
  useFocusEffect(
    React.useCallback(() => {
      qc.invalidateQueries({ queryKey });
      const sub = AppState.addEventListener('change', (state) => {
        if (state === 'active') {
          qc.invalidateQueries({ queryKey });
        }
      });
      return () => sub.remove();
    }, [qc])
  );
}
```

```ts
// src/services/appointmentsService.ts
import { api } from '../infra/api';

export const appointmentsService = {
  getById: async (id: string) => (await api.get(`/api/appointments/${id}`)).data as AppointmentDetailsOutput,
  confirm: async (id: string) => (await api.post(`/api/appointments/${id}/confirm`)).data as AppointmentDetailsOutput,
  cancel: async (id: string) => (await api.post(`/api/appointments/${id}/cancel`)).data as AppointmentDetailsOutput,
  complete: async (id: string) => (await api.post(`/api/appointments/${id}/complete`)).data as AppointmentDetailsOutput,
};
```
