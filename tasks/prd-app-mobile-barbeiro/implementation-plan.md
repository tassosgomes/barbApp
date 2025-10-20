# Plano de Implementação — App Mobile do Barbeiro (MVP)

## Tarefas e Estimativas

1) Infra de Dados (Axios + Interceptors + SecureStore) — 1d
- Criar `api.ts` (Axios instance, baseURL via env)
- Request interceptor (Authorization Bearer)
- Response interceptor (401 → limpar sessão/navegar login)
- `storage.ts` (SecureStore/AsyncStorage fallback)

2) Autenticação (Login e-mail/senha) — 1.5d
- Tela de Login (validação + loading + erros)
- `authService.login({ email, password })`
- `AuthContext` (token/user) e provider
- Testes unitários (sucesso/401/400)

3) Backend: `GET /api/barbeiro/barbearias` — 1d
- Implementar endpoint no backend (Application + Repository + Controller)
- Swagger + testes de integração básicos
- Integração no app: `authService.listBarbershops()`

4) Seleção de Barbearia (multi‑vínculo) — 1d
- Tela de seleção (lista, estados vazio/erro)
- `authService.switchContext(novaBarbeariaId)`
- Atualizar token/contexto e navegar para agenda

5) Agenda do Dia — 2d
- Tela da agenda (lista/timeline simples)
- `scheduleService.getMySchedule(date)` + polling 30s + pull‑to‑refresh
- Conversão UTC → local (date-fns-tz)
- Estados vazio/erro/loading
- Testes (hook + tela)

6) Detalhe e Ações (Confirmar/Cancelar/Concluir) — 2d
- Tela/modal de detalhes
- `appointmentsService.getById/confirm/cancel/complete`
- Tratamento de 409/400 com toasts e refetch
- Testes unit/integration principais

7) Observabilidade (Sentry.io) — 0.5d
- Configurar `sentry-expo` com DSN/env
- Captura de erros não tratados, breadcrumbs de navegação/fetch
- Verificar scrubbing de PII

8) Polimento e Testes Finais — 1d
- Revisão de UX (loading/empty/error)
- Ajustes de acessibilidade básica
- Ensaios manuais de fluxos críticos

Total estimado: ~10 dias úteis

## Dependências e Bloqueios
- Backend com `GET /api/barbeiro/barbearias` implementado e publicado (bloqueia item 4 integração)
- DSN do Sentry por ambiente (bloqueia item 7)
- BaseURL de API (dev/staging/prod)

## Marcos e Entregas
- M1 (Dia 3): Login funcional + infra
- M2 (Dia 6): Seleção de barbearia + agenda do dia (sem ações)
- M3 (Dia 9): Detalhes e ações funcionando (server-first)
- M4 (Dia 10): Observabilidade + testes finais

## Observações
- Refetch ao focar tela da agenda e ao voltar do background
- Server-first (sem optimistic UI) nas ações com refetch imediato
- Timezone: exibir sempre horário local do dispositivo

