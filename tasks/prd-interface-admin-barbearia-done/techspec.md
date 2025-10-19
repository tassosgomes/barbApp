# Especificação Técnica: Interface Administrativa para Admin de Barbearia

## Resumo Executivo

Esta especificação detalha a implementação de uma interface web completa para administradores de barbearias (Admin Barbearia), complementando o sistema existente que atualmente suporta apenas Admin Central. A solução utiliza **URL-based tenant identification** através do padrão `/{codigo}/login`, eliminando campos de formulário adicionais e simplificando a UX.

A arquitetura seguirá os padrões estabelecidos em `barbapp-admin`, reutilizando componentes UI (Shadcn/ui), serviços, hooks e estrutura de rotas. Principais decisões técnicas: (1) Roteamento dinâmico com prefixo `:codigo` em todas as rotas, (2) Novo endpoint backend `GET /api/barbearias/codigo/:codigo` para validação prévia do código, (3) Context API para gerenciamento de estado da barbearia ativa, (4) Isolamento de dados garantido via JWT claims e middleware backend existente.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Frontend (barbapp-admin):**
- **Router Configuration**: Extensão do React Router para suportar rotas dinâmicas com prefixo `:codigo`
- **LoginAdminBarbearia Page**: Página de login standalone que valida código via API antes de exibir formulário
- **BarbeariaContext**: Context Provider que armazena e compartilha dados da barbearia ativa (id, nome, código)
- **useBarbeariaCode Hook**: Custom hook que extrai e valida código da URL usando `useParams()`
- **ProtectedBarbeariaRoute Component**: Wrapper de rotas que valida autenticação + tipo de usuário (AdminBarbearia)
- **Dashboard, Barbeiros, Serviços, Agenda Pages**: Páginas específicas adaptadas ao contexto de Admin Barbearia

**Backend (BarbApp.API):**
- **BarbershopsController**: Novo endpoint `GET /api/barbearias/codigo/:codigo` (público, sem autenticação)
- **AuthController**: Endpoint existente `POST /api/auth/admin-barbearia/login` (mantém estrutura atual)
- **JWT Middleware**: Validação de claims `tipo_usuario`, `barbearia_id`, `barbearia_codigo` (já existente)

**Fluxo de Dados:**
1. Usuário acessa `/{codigo}/login` → Hook extrai código → Request `GET /api/barbearias/codigo/:codigo`
2. Backend valida código → Retorna `{id, nome, codigo, isActive}` → Frontend exibe formulário com nome da barbearia
3. Usuário submete login → Request `POST /api/auth/admin-barbearia/login` → Backend gera JWT com claims
4. JWT armazenado em `localStorage` + contexto em `BarbeariaContext` → Redirecionamento para `/{codigo}/dashboard`
5. Navegação interna mantém código na URL → Requests usam JWT → Backend filtra dados por `barbearia_id` do token

### Componentes Principais e Responsabilidades

| Componente | Responsabilidade | Localização |
|------------|-----------------|-------------|
| `useBarbeariaCode` | Extração e validação de código da URL | `src/hooks/useBarbeariaCode.ts` |
| `BarbeariaContext` | Gerenciamento de estado global da barbearia | `src/contexts/BarbeariaContext.tsx` |
| `LoginAdminBarbearia` | Autenticação com validação prévia de código | `src/pages/LoginAdminBarbearia/LoginAdminBarbearia.tsx` |
| `ProtectedBarbeariaRoute` | Guarda de rotas com validação de tipo de usuário | `src/components/ProtectedBarbeariaRoute.tsx` |
| `barbeariaService` | Serviço de API para operações de barbearia | `src/services/barberia.service.ts` |
| `adminBarbeariaAuthService` | Serviço de autenticação específico | `src/services/adminBarbeariaAuth.service.ts` |

## Design de Implementação

### Interfaces Principais

```typescript
// src/hooks/useBarbeariaCode.ts
export interface UseBarbeariaCodeReturn {
  codigo: string | undefined;
  isValidating: boolean;
  barbeariaInfo: BarbeariaInfo | null;
  error: Error | null;
}

export function useBarbeariaCode(): UseBarbeariaCodeReturn;

// src/contexts/BarbeariaContext.tsx
export interface BarbeariaContextData {
  barbeariaId: string;
  nome: string;
  codigo: string;
  isActive: boolean;
}

export interface BarbeariaContextValue {
  barbearia: BarbeariaContextData | null;
  setBarbearia: (barbearia: BarbeariaContextData | null) => void;
  clearBarbearia: () => void;
}

export const BarbeariaContext = createContext<BarbeariaContextValue | undefined>(undefined);
export function useBarbearia(): BarbeariaContextValue;

// src/services/barberia.service.ts
export interface ValidateCodeResponse {
  id: string;
  nome: string;
  codigo: string;
  isActive: boolean;
}

export const barbeariaService = {
  validateCode: (codigo: string) => Promise<ValidateCodeResponse>;
  getMe: () => Promise<Barbershop>; // Dados completos da barbearia do usuário logado
};

// src/services/adminBarbeariaAuth.service.ts
export interface LoginAdminBarbeariaRequest {
  codigo: string;
  email: string;
  senha: string;
}

export interface LoginAdminBarbeariaResponse {
  token: string;
  tipoUsuario: 'AdminBarbearia';
  barbeariaId: string;
  nomeBarbearia: string;
  expiresAt: string;
}

export const adminBarbeariaAuthService = {
  login: (request: LoginAdminBarbeariaRequest) => Promise<LoginAdminBarbeariaResponse>;
  logout: () => void;
};
```

### Modelos de Dados

**Frontend Types:**

```typescript
// src/types/adminBarbearia.ts

/**
 * Informação básica da barbearia retornada na validação de código
 */
export interface BarbeariaInfo {
  id: string;
  nome: string;
  codigo: string;
  isActive: boolean;
}

/**
 * Dados de autenticação para Admin Barbearia
 */
export interface AdminBarbeariaAuth {
  token: string;
  tipoUsuario: 'AdminBarbearia';
  barbeariaId: string;
  nomeBarbearia: string;
  codigo: string;
  expiresAt: string;
}

/**
 * Contexto de sessão do Admin Barbearia armazenado em localStorage
 */
export interface AdminBarbeariaSession {
  token: string;
  barbeariaId: string;
  nomeBarbearia: string;
  codigo: string;
}
```

**Backend DTOs (já existentes):**

```csharp
// BarbApp.Application/DTOs/LoginAdminBarbeariaInput.cs
public record LoginAdminBarbeariaInput
{
    public string Codigo { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}

// BarbApp.Application/DTOs/AuthResponse.cs (já existe)
public record AuthResponse
{
    public string Token { get; init; } = string.Empty;
    public string TipoUsuario { get; init; } = string.Empty;
    public Guid BarbeariaId { get; init; }
    public string NomeBarbearia { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
```

**Novo DTO Backend:**

```csharp
// BarbApp.Application/DTOs/ValidateBarbeariaCodeResponse.cs (NOVO)
public record ValidateBarbeariaCodeResponse
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Codigo { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}
```

### Endpoints de API

**Backend - Novos Endpoints:**

| Método | Endpoint | Auth | Descrição | Request | Response |
|--------|----------|------|-----------|---------|----------|
| `GET` | `/api/barbearias/codigo/:codigo` | ❌ Não | Valida código e retorna dados básicos | N/A | `ValidateBarbeariaCodeResponse` |
| `GET` | `/api/barbearias/me` | ✅ Sim | Retorna dados completos da barbearia do usuário logado | N/A | `Barbershop` |

**Backend - Endpoints Existentes (reutilizados):**

| Método | Endpoint | Auth | Descrição | Request | Response |
|--------|----------|------|-----------|---------|----------|
| `POST` | `/api/auth/admin-barbearia/login` | ❌ Não | Autenticação de Admin Barbearia | `LoginAdminBarbeariaInput` | `AuthResponse` |
| `GET` | `/api/barbeiros` | ✅ Sim | Lista barbeiros da barbearia (filtrado por JWT) | Query params | `PaginatedResponse<Barbeiro>` |
| `POST` | `/api/barbeiros` | ✅ Sim | Cria barbeiro na barbearia do token | `CreateBarbeiroInput` | `Barbeiro` |
| `PUT` | `/api/barbeiros/:id` | ✅ Sim | Atualiza barbeiro (valida ownership) | `UpdateBarbeiroInput` | `Barbeiro` |
| `PUT` | `/api/barbeiros/:id/desativar` | ✅ Sim | Desativa barbeiro | N/A | `void` |
| `PUT` | `/api/barbeiros/:id/reativar` | ✅ Sim | Reativa barbeiro | N/A | `void` |
| `GET` | `/api/servicos` | ✅ Sim | Lista serviços da barbearia | Query params | `PaginatedResponse<Servico>` |
| `POST` | `/api/servicos` | ✅ Sim | Cria serviço | `CreateServicoInput` | `Servico` |
| `PUT` | `/api/servicos/:id` | ✅ Sim | Atualiza serviço | `UpdateServicoInput` | `Servico` |
| `GET` | `/api/agendamentos` | ✅ Sim | Lista agendamentos da barbearia | Query params | `PaginatedResponse<Agendamento>` |

**Detalhes do Novo Endpoint:**

```http
GET /api/barbearias/codigo/{codigo}

Response 200 OK:
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nome": "Barbearia do Tasso Zé",
  "codigo": "6SJJRFPD",
  "isActive": true
}

Response 404 Not Found:
{
  "message": "Barbearia não encontrada"
}

Response 403 Forbidden (se isActive = false):
{
  "message": "Barbearia temporariamente indisponível"
}
```

### Schemas de Validação (Zod)

```typescript
// src/schemas/adminBarbearia.schema.ts

import { z } from 'zod';

/**
 * Schema para login de Admin Barbearia
 * Nota: Código vem da URL, não do formulário
 */
export const loginAdminBarbeariaSchema = z.object({
  email: z
    .string()
    .email('Email inválido')
    .toLowerCase()
    .trim(),

  senha: z
    .string()
    .min(6, 'Senha deve ter no mínimo 6 caracteres')
    .max(100, 'Senha deve ter no máximo 100 caracteres'),
});

export type LoginAdminBarbeariaFormData = z.infer<typeof loginAdminBarbeariaSchema>;

/**
 * Schema para validação de código da URL
 */
export const codigoSchema = z
  .string()
  .length(8, 'Código deve ter 8 caracteres')
  .regex(/^[A-Z0-9]{8}$/, 'Código deve conter apenas letras maiúsculas e números');
```

## Pontos de Integração

### Serviços Backend Existentes

**Nenhuma integração externa nova.** A implementação reutiliza serviços backend já existentes:

1. **AuthenticateAdminBarbeariaUseCase** (já implementado)
   - Localização: `BarbApp.Application/UseCases/AuthenticateAdminBarbeariaUseCase.cs`
   - Responsabilidade: Validação de credenciais e geração de JWT
   - Não requer modificações

2. **JWT Middleware** (já implementado)
   - Localização: `BarbApp.API/Middleware/JwtMiddleware.cs`
   - Responsabilidade: Validação de token e injeção de claims no contexto HTTP
   - Não requer modificações

3. **Tenant Isolation Middleware** (já implementado)
   - Localização: Integrado nos repositórios via `_currentUserService`
   - Responsabilidade: Filtrar queries por `barbearia_id` do token
   - Não requer modificações

### Novos Serviços Backend Necessários

**1. ValidateBarbeariaCodeUseCase** (NOVO)
- **Responsabilidade**: Validar código e retornar dados públicos da barbearia
- **Localização**: `BarbApp.Application/UseCases/ValidateBarbeariaCodeUseCase.cs`
- **Dependências**: `IBarbershopRepository`
- **Tratamento de Erros**:
  - Código não encontrado → `NotFoundException` (404)
  - Barbearia inativa → `ForbiddenException` (403)

**2. GetMyBarbershopUseCase** (NOVO)
- **Responsabilidade**: Retornar dados completos da barbearia do usuário logado
- **Localização**: `BarbApp.Application/UseCases/GetMyBarbershopUseCase.cs`
- **Dependências**: `IBarbershopRepository`, `ICurrentUserService`
- **Tratamento de Erros**:
  - Token inválido/expirado → `UnauthorizedException` (401)
  - Usuário não associado a barbearia → `ForbiddenException` (403)

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|--------------------|-----------------|----------------------------|----------------|
| **BarbershopsController** | Novo Endpoint (Aditivo) | Adiciona `GET /api/barbearias/codigo/:codigo`. Sem impacto em endpoints existentes. **Risco: Baixo** | Implementar endpoint, adicionar testes unitários |
| **Router (React Router)** | Mudança de Configuração | Adiciona rotas com prefixo dinâmico `/:codigo/*`. Rotas existentes de Admin Central não afetadas. **Risco: Baixo** | Atualizar `src/routes/index.tsx`, adicionar rotas isoladas |
| **API Service (axios)** | Sem Mudança | Reutiliza instância existente. Token continua via interceptor. **Risco: Nenhum** | Nenhuma |
| **useAuth Hook** | Extensão (Compatível) | Possível criação de `useAdminBarbeariaAuth` separado, mas `useAuth` existente permanece intacto. **Risco: Baixo** | Criar novo hook sem modificar existente |
| **ProtectedRoute Component** | Sem Mudança | Admin Central continua usando componente atual. Novo `ProtectedBarbeariaRoute` criado separadamente. **Risco: Nenhum** | Criar novo componente isolado |
| **localStorage** | Novo Item | Adiciona chaves `admin_barbearia_token`, `admin_barbearia_session`. Não conflita com `auth_token` do Admin Central. **Risco: Nenhum** | Documentar namespacing de chaves |
| **JWT Claims** | Sem Mudança | Claims `barbearia_id`, `barbearia_codigo`, `tipo_usuario` já existem para AdminBarbearia. **Risco: Nenhum** | Nenhuma |
| **Email Service** | Mudança de Template | Atualizar template de boas-vindas para incluir link `/{codigo}/login`. **Risco: Baixo** | Modificar `CreateBarbershopUseCase` para incluir código na URL do email |

**Impactos Positivos:**
- ✅ Zero breaking changes em funcionalidades existentes de Admin Central
- ✅ Reutilização massiva de componentes UI, serviços e tipos
- ✅ Isolamento completo entre Admin Central e Admin Barbearia (rotas, estado, localStorage)

**Dependências Diretas:**
- `BarbershopsController` → `ValidateBarbeariaCodeUseCase` (novo)
- `LoginAdminBarbearia` → `adminBarbeariaAuthService` → `POST /api/auth/admin-barbearia/login` (existente)
- `ProtectedBarbeariaRoute` → `useAdminBarbeariaAuth` → `localStorage.admin_barbearia_token` (novo)

**Recursos Compartilhados:**
- Tabelas de banco de dados (leitura apenas, sem migrations)
- Componentes UI do Shadcn/ui (Card, Button, Input, etc.)
- API axios instance e interceptors

## Abordagem de Testes

### Testes Unitários

**Frontend:**

```typescript
// src/hooks/__tests__/useBarbeariaCode.test.ts
describe('useBarbeariaCode', () => {
  it('should extract codigo from URL params');
  it('should validate codigo format (8 chars alphanumeric)');
  it('should fetch barbearia info on mount');
  it('should handle invalid codigo (404)');
  it('should handle inactive barbearia (403)');
});

// src/contexts/__tests__/BarbeariaContext.test.tsx
describe('BarbeariaContext', () => {
  it('should provide barbearia data to children');
  it('should persist barbearia in localStorage');
  it('should clear barbearia on logout');
});

// src/pages/LoginAdminBarbearia/__tests__/LoginAdminBarbearia.test.tsx
describe('LoginAdminBarbearia', () => {
  it('should display loading state while validating codigo');
  it('should display error for invalid codigo');
  it('should display form with barbearia name on valid codigo');
  it('should submit login with codigo from URL + form data');
  it('should store token and redirect on success');
  it('should display error on invalid credentials');
});

// src/components/__tests__/ProtectedBarbeariaRoute.test.tsx
describe('ProtectedBarbeariaRoute', () => {
  it('should redirect to /:codigo/login when not authenticated');
  it('should render children when authenticated as AdminBarbearia');
  it('should reject authentication from other user types');
});
```

**Backend:**

```csharp
// BarbApp.Application.Tests/UseCases/ValidateBarbeariaCodeUseCaseTests.cs
[Fact]
public async Task ExecuteAsync_ValidCode_ReturnsValidateBarbeariaCodeResponse()

[Fact]
public async Task ExecuteAsync_InvalidCode_ThrowsNotFoundException()

[Fact]
public async Task ExecuteAsync_InactiveBarbers_ThrowsForbiddenException()

// BarbApp.API.Tests/Controllers/BarbershopsControllerTests.cs
[Fact]
public async Task GetByCode_ValidCode_Returns200WithData()

[Fact]
public async Task GetByCode_InvalidCode_Returns404()
```

**Mocks Requeridos:**
- `axios`: mockar requests `GET /api/barbearias/codigo/:codigo` e `POST /api/auth/admin-barbearia/login`
- `useParams`: mockar retorno de `{ codigo: '6SJJRFPD' }`
- `localStorage`: mockar métodos `getItem`, `setItem`, `removeItem`
- `IBarbershopRepository`: mockar métodos `GetByCodeAsync`, `GetByIdAsync`

**Cobertura Esperada:**
- Frontend: >= 80% (linhas, branches)
- Backend: >= 90% (use cases críticos de autenticação)

### Testes de Integração

**Frontend (Playwright):**

```typescript
// tests/e2e/admin-barbearia-login.spec.ts
test('should complete full login flow for Admin Barbearia', async ({ page }) => {
  // 1. Navigate to /:codigo/login
  await page.goto('/6SJJRFPD/login');
  
  // 2. Verify barbearia name displayed
  await expect(page.locator('h1')).toContainText('Barbearia do Tasso Zé');
  
  // 3. Fill login form
  await page.fill('input[name="email"]', 'tasso.gomes@outlook.com');
  await page.fill('input[name="senha"]', '96z7ZBK#DXNn');
  
  // 4. Submit
  await page.click('button[type="submit"]');
  
  // 5. Verify redirect to dashboard
  await expect(page).toHaveURL('/6SJJRFPD/dashboard');
  
  // 6. Verify token stored
  const token = await page.evaluate(() => localStorage.getItem('admin_barbearia_token'));
  expect(token).toBeTruthy();
});

test('should reject invalid codigo', async ({ page }) => {
  await page.goto('/INVALID!/login');
  await expect(page.locator('text=Barbearia não encontrada')).toBeVisible();
});
```

**Backend (Integration Tests):**

```csharp
// BarbApp.API.IntegrationTests/Controllers/BarbershopsControllerTests.cs
[Fact]
public async Task GetByCode_WithRealDatabase_ReturnsCorrectData()
{
    // Arrange: Seed database with test barbershop
    var barbershop = await SeedTestBarbershop("TEST1234");
    
    // Act
    var response = await _client.GetAsync("/api/barbearias/codigo/TEST1234");
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var result = await response.Content.ReadAsAsync<ValidateBarbeariaCodeResponse>();
    result.Codigo.Should().Be("TEST1234");
}
```

**Requisitos de Dados de Teste:**
- Barbershop seed: código `6SJJRFPD`, nome `Barbearia do Tasso Zé`, ativo
- AdminBarbearia user: email `tasso.gomes@outlook.com`, senha hash válido
- Vincular AdminBarbearia ao Barbershop via `BarbeariaId`

**Localização dos Testes:**
- Frontend E2E: `barbapp-admin/tests/e2e/admin-barbearia/`
- Backend Integration: `backend/tests/BarbApp.API.IntegrationTests/`

## Sequenciamento de Desenvolvimento

### Ordem de Construção

**Fase 1: Infraestrutura Backend (Sprint 1 - Semana 1)**
*Por que primeiro: Bloqueia todo desenvolvimento frontend*

1. **ValidateBarbeariaCodeUseCase** e endpoint `GET /api/barbearias/codigo/:codigo`
   - Use Case com validação de código, lógica de inativação
   - Controller action em `BarbershopsController`
   - Testes unitários do Use Case
   - Testes de integração do endpoint

2. **GetMyBarbershopUseCase** e endpoint `GET /api/barbearias/me`
   - Use Case com leitura de `ICurrentUserService.BarbeariaId`
   - Controller action em `BarbershopsController` com `[Authorize]`
   - Testes unitários e integração

3. **Atualizar template de email** em `CreateBarbershopUseCase`
   - Modificar corpo do email para incluir `http://app.barbapp.com/{codigo}/login`

**Dependências:** Nenhuma (novos endpoints isolados)

---

**Fase 2: Autenticação Frontend (Sprint 2 - Semana 2)**
*Por que segundo: Depende de endpoints backend; bloqueia todas as páginas protegidas*

1. **Hook `useBarbeariaCode`**
   - Extração de `:codigo` via `useParams()`
   - Validação de formato via Zod
   - Request para `GET /api/barbearias/codigo/:codigo`
   - Estados: `isValidating`, `barbeariaInfo`, `error`
   - Testes unitários

2. **Context `BarbeariaContext`**
   - Provider com estado `barbearia: BarbeariaContextData | null`
   - Persistência em `localStorage.admin_barbearia_session`
   - Hook `useBarbearia()` para consumir contexto
   - Testes unitários

3. **Service `adminBarbeariaAuthService`**
   - Método `login(codigo, email, senha)` → `POST /api/auth/admin-barbearia/login`
   - Método `logout()` → limpar `localStorage` e context
   - Tratamento de erros (401, 403, 500)

4. **Página `LoginAdminBarbearia`**
   - Integração com `useBarbeariaCode` (validação ao montar)
   - Formulário com Zod (`loginAdminBarbeariaSchema`)
   - Exibição de nome da barbearia no header
   - Submit via `adminBarbeariaAuthService.login`
   - Redirect para `/{codigo}/dashboard` após sucesso
   - Testes unitários

5. **Component `ProtectedBarbeariaRoute`**
   - Verificação de `localStorage.admin_barbearia_token`
   - Verificação de `tipo_usuario === 'AdminBarbearia'` (decodificar JWT)
   - Redirect para `/{codigo}/login` se não autenticado
   - Testes unitários

**Dependências:** Fase 1 completa (endpoints backend)

---

**Fase 3: Dashboard e Navegação (Sprint 3 - Semana 3)**
*Por que terceiro: Depende de autenticação; serve de base para outras páginas*

1. **Estrutura de rotas** em `src/routes/index.tsx`
   - Adicionar rotas com prefixo `/:codigo`
   - Aplicar `ProtectedBarbeariaRoute` como wrapper
   - Rotas: `/login`, `/dashboard`, `/barbeiros`, `/servicos`, `/agenda`

2. **Layout `AdminBarbeariaLayout`**
   - Header com nome da barbearia e código (consumir `BarbeariaContext`)
   - Menu lateral com navegação
   - Botão de logout
   - Reutilizar componentes `Header`, `Sidebar` com props customizáveis

3. **Página `Dashboard`**
   - Cards com métricas (total barbeiros, serviços, agendamentos do dia)
   - Request para endpoints agregados (ou calcular no frontend)
   - Reutilizar componentes `Card`, `Skeleton` para loading states

**Dependências:** Fase 2 completa (autenticação)

---

**Fase 4: Gestão de Barbeiros (Sprint 4 - Semana 4)**
*Por que quarto: Funcionalidade principal, reutiliza endpoints backend já existentes*

1. **Página `BarbeirosListPage`** (adaptada de Admin Central)
   - Request `GET /api/barbeiros` (backend filtra por `barbearia_id` do token)
   - DataTable com colunas: nome, email, telefone, status
   - Ações: editar, desativar/reativar

2. **Página `BarbeiroFormPage`** (create/edit)
   - Formulário com campos: nome, email, telefone
   - Validação Zod
   - Submit para `POST /api/barbeiros` ou `PUT /api/barbeiros/:id`
   - Feedback de sucesso/erro via toast

3. **Modal `DesativarBarbeiroModal`**
   - Confirmação de desativação
   - Request `PUT /api/barbeiros/:id/desativar`

**Dependências:** Fase 3 completa (layout e navegação)

---

**Fase 5: Gestão de Serviços (Sprint 5 - Semana 5)**
*Por que quinto: Similar a barbeiros, sem dependências complexas*

1. **Página `ServicosListPage`**
   - Request `GET /api/servicos`
   - DataTable: nome, duração, preço, status

2. **Página `ServicoFormPage`**
   - Formulário: nome, descrição, duração (minutos), preço
   - Validação: duração > 0, preço >= 0
   - Submit para `POST /api/servicos` ou `PUT /api/servicos/:id`

**Dependências:** Fase 4 completa (opcional, podem ser paralelas)

---

**Fase 6: Visualização de Agendamentos (Sprint 6 - Semana 6)**
*Por que sexto: Depende de barbeiros e serviços existirem para exibir dados completos*

1. **Página `AgendamentosListPage`**
   - Request `GET /api/agendamentos`
   - Filtros: barbeiro, data (DatePicker), status
   - DataTable: data, hora, cliente, barbeiro, serviço, status

2. **Modal `AgendamentoDetailsModal`**
   - Exibir todos os detalhes do agendamento
   - Sem edição (apenas visualização para Admin Barbearia)

**Dependências:** Fase 5 completa (barbeiros e serviços devem estar disponíveis)

---

**Fase 7: Testes E2E e Refinamentos (Sprint 7 - Semana 7)**
*Por que último: Testa todo o fluxo integrado*

1. **Testes E2E Playwright**
   - Fluxo completo: login → dashboard → criar barbeiro → criar serviço → visualizar agenda
   - Casos de erro: código inválido, credenciais inválidas, sessão expirada

2. **Ajustes de UX/UI**
   - Feedback de usuários internos
   - Melhorias de responsividade

3. **Documentação**
   - Atualizar README com instruções de setup para Admin Barbearia
   - Documentar variáveis de ambiente (se necessário)

**Dependências:** Fases 1-6 completas

### Dependências Técnicas

**Bloqueantes:**

1. ✅ **Backend endpoints disponíveis** (Fase 1)
   - `GET /api/barbearias/codigo/:codigo`
   - `GET /api/barbearias/me`
   - Todos os endpoints de barbeiros, serviços, agendamentos (já existem)

2. ✅ **Shadcn/ui components instalados** (já disponível)
   - Card, Button, Input, DataTable, DatePicker, Modal, Toast, Skeleton

3. ✅ **React Router v6 configurado** (já disponível)

**Não-bloqueantes:**

1. ⚠️ **Seed de dados de teste** (facilita desenvolvimento, mas não bloqueia)
   - Barbershop com código `6SJJRFPD`
   - AdminBarbearia user vinculado

2. ⚠️ **Ambiente de staging** (para testes antes de produção)

## Monitoramento e Observabilidade

**Frontend:**

```typescript
// Logging com console estruturado (desenvolvimento)
console.log('[AdminBarbearia] Validating codigo:', codigo);
console.error('[AdminBarbearia] Login failed:', error);

// Métricas (se integrado com analytics)
analytics.track('admin_barbearia_login', { codigo, email });
analytics.track('admin_barbearia_create_barbeiro', { barbeariaId });
```

**Backend:**

```csharp
// Logs estruturados com ILogger (já implementado)
_logger.LogInformation(
    "Validating barbershop code: {Codigo}",
    codigo);

_logger.LogWarning(
    "Admin Barbearia login attempt for inactive barbershop: {Codigo}",
    codigo);

_logger.LogError(
    "Failed to validate barbershop code: {Codigo}, Error: {Error}",
    codigo,
    ex.Message);
```

**Métricas a Monitorar:**

| Métrica | Tipo | Descrição | Alerta |
|---------|------|-----------|--------|
| `admin_barbearia_login_attempts_total` | Counter | Total de tentativas de login | - |
| `admin_barbearia_login_failures_total` | Counter | Logins falhados | > 10/min |
| `barbershop_code_validation_duration_seconds` | Histogram | Tempo de validação de código | p95 > 500ms |
| `admin_barbearia_active_sessions` | Gauge | Sessões ativas | - |

**Dashboards (se Grafana disponível):**
- Dashboard "Admin Barbearia - Autenticação": login attempts, failures, duração média
- Dashboard "Admin Barbearia - Uso": páginas mais acessadas, ações por hora

**Integração com Sentry (se disponível):**
- Capturar erros de validação de código (404, 403)
- Capturar erros de autenticação (401)
- Capturar erros de network (timeout, 500)

## Considerações Técnicas

### Decisões Principais

**1. URL-based Tenant Identification (Código na URL)**

**Decisão:** Usar padrão `/{codigo}/login` em vez de campo de formulário.

**Justificativa:**
- **UX Superior:** Usuário recebe link direto no email, não precisa lembrar/digitar código
- **Deep Linking:** URLs estruturadas permitem bookmarks e compartilhamento
- **Validação Antecipada:** Backend valida código antes mesmo do login, melhor feedback de erros
- **Multi-tenancy Explícito:** Código na URL torna contexto visível e auditável

**Trade-offs:**
- ➕ Menos campos no formulário (UX simplificada)
- ➕ URLs mais descritivas e navegáveis
- ➖ URLs ligeiramente mais longas
- ➖ Código visível na barra de endereço (mas código não é sensível)

**Alternativas Rejeitadas:**
- ❌ Campo de código no formulário: mais cliques, mais erros de digitação, UX inferior
- ❌ Subdomain-based (`6sjjrfpd.barbapp.com`): requer configuração DNS, complexidade desnecessária para MVP

---

**2. Context API para Estado Global da Barbearia**

**Decisão:** Usar React Context API (`BarbeariaContext`) em vez de estado local ou Redux.

**Justificativa:**
- **Simplicidade:** Context API nativo do React, sem dependências adicionais
- **Escopo Limitado:** Estado da barbearia ativa é pequeno e previsível
- **Compartilhamento Fácil:** Múltiplos componentes (Header, Sidebar, Pages) precisam do mesmo dado
- **Persistência:** Sincronização com `localStorage` para sobreviver reloads

**Trade-offs:**
- ➕ Zero dependências externas
- ➕ API simples e familiar para desenvolvedores React
- ➖ Re-renders se não otimizado (mitigado com `useMemo` no Provider)

**Alternativas Rejeitadas:**
- ❌ Redux: overkill para estado simples de leitura majoritária
- ❌ Zustand/Jotai: adiciona dependência sem benefício significativo
- ❌ Props drilling: impraticável com múltiplos níveis de componentes

---

**3. Reutilização Massiva de Componentes Admin Central**

**Decisão:** Reutilizar componentes UI, serviços, tipos do Admin Central sem forking.

**Justificativa:**
- **Consistência Visual:** Mesma aparência entre Admin Central e Admin Barbearia
- **Manutenibilidade:** Correções de bugs beneficiam ambos
- **Velocidade de Desenvolvimento:** Componentes já testados e validados
- **Design System Unificado:** Shadcn/ui serve como base comum

**Trade-offs:**
- ➕ Desenvolvimento 3x mais rápido (estimativa)
- ➕ Menor superfície de bugs (código já estável)
- ➖ Dependência entre módulos (mas controlada via props)

**Estratégia de Reutilização:**
- ✅ Componentes UI: 100% reutilizados (`Button`, `Input`, `DataTable`, etc.)
- ✅ Serviços de API: Reutilizados com extensões (`barbershopService`, `barberService`, etc.)
- ✅ Tipos: Reutilizados onde aplicável (`Barbershop`, `Barber`, `Service`)
- ⚠️ Páginas: Adaptadas (lógica similar, UI customizada para contexto)

---

**4. Separação de Hooks de Autenticação**

**Decisão:** Criar `useAdminBarbeariaAuth` separado de `useAuth` (Admin Central).

**Justificativa:**
- **Isolamento de Responsabilidades:** Cada tipo de usuário tem fluxo próprio
- **Diferentes Chaves localStorage:** `auth_token` vs `admin_barbearia_token`
- **Evitar Conflitos:** Usuário pode estar logado como Admin Central em outra aba

**Alternativas Rejeitadas:**
- ❌ Unificar em `useAuth` com parâmetro `tipoUsuario`: aumenta complexidade, viola Single Responsibility Principle

---

**5. Endpoint Público para Validação de Código**

**Decisão:** `GET /api/barbearias/codigo/:codigo` **não requer autenticação**.

**Justificativa:**
- **Necessidade Técnica:** Validação deve ocorrer **antes** do login
- **Dados Não-sensíveis:** Retorna apenas `{id, nome, codigo, isActive}` (sem email, telefone, endereço)
- **Segurança Aceitável:** Código não é secreto; qualquer um pode tentar adivinhar, mas sem autenticação não acessa dados reais
- **Rate Limiting:** Backend pode implementar throttling se necessário (fora do escopo desta spec)

**Mitigação de Riscos:**
- ✅ Endpoint retorna apenas dados públicos
- ✅ Código tem 8 caracteres alfanuméricos (62^8 = ~218 trilhões de combinações)
- ✅ Backend pode implementar rate limiting (ex: 10 req/min por IP)

### Riscos Conhecidos

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| **Endpoints backend incompletos/bugados** | Média | Alto | Validar todos os endpoints na Fase 1 com testes de integração rigorosos; rodar suite de testes E2E do backend |
| **Isolamento de dados (tenant) falhando** | Baixa | Crítico | Testes de segurança: Admin Barbearia A não deve ver dados de Barbearia B; adicionar testes de integração específicos para multi-tenancy |
| **Conflito de localStorage entre Admin Central e Admin Barbearia** | Baixa | Médio | Usar prefixos diferentes: `auth_token` vs `admin_barbearia_token`; limpar storage ao trocar de contexto |
| **Performance em listagens grandes** | Média | Médio | Implementar paginação corretamente (backend já suporta); adicionar virtual scrolling se listas > 100 itens |
| **Usuário acessa URL com código de outra barbearia** | Alta | Baixo | Backend valida `barbearia_id` do token contra dados solicitados; retorna 403 se mismatch |
| **Código da URL alterado manualmente durante navegação** | Média | Médio | Hook `useBarbeariaCode` revalida código ao mudar; logout automático se código não bate com token |

**Áreas Precisando Pesquisa/Validação:**
- ⚠️ Comportamento do React Router com rotas dinâmicas aninhadas (`:codigo/*`)
- ⚠️ Estratégia de cache para dados de barbearia (evitar revalidação a cada navegação)
- ⚠️ Melhor abordagem para "esqueci minha senha" (Admin Barbearia depende de Admin Central?)

### Requisitos Especiais

**Performance:**

- **Tempo de carregamento inicial (Login page):** < 1 segundo (validação de código + renderização)
- **Tempo de resposta do login:** < 2 segundos (autenticação + redirect)
- **Tempo de carregamento de listagens:** < 2 segundos (com até 100 itens)

**Estratégias de Otimização:**
- React.lazy() para code-splitting de páginas
- TanStack Query com caching agressivo (staleTime: 5min)
- Skeleton loaders para melhor perceived performance

**Segurança:**

- **Tokens JWT:** Expiração de 8 horas (configurável via backend)
- **Refresh de sessão:** Logout automático ao expirar (sem refresh token por enquanto)
- **HTTPS:** Obrigatório em produção
- **CORS:** Backend deve permitir apenas domínios autorizados
- **XSS Protection:** Zod valida todos os inputs; React escapa outputs automaticamente

**Acessibilidade:**

- **WCAG 2.1 Level AA:** Garantir contraste de cores, labels em inputs, navegação por teclado
- **Screen Readers:** Usar atributos ARIA onde necessário
- **Mobile-first:** Design responsivo com breakpoints em 640px, 768px, 1024px

### Conformidade com Padrões

**Frontend (@rules/react.md):**

- ✅ **Componentes Funcionais:** Todos os componentes usam function components + hooks
- ✅ **TypeScript Strict Mode:** `strict: true` no `tsconfig.json`
- ✅ **Naming Conventions:** PascalCase para componentes, camelCase para funções/variáveis
- ✅ **Props Interfaces:** Todas as props tipadas com interfaces `ComponentNameProps`
- ✅ **Custom Hooks:** Prefixo `use`, extração de lógica reutilizável
- ✅ **Error Boundaries:** Componentes de página wrapped em `ErrorBoundary`

**Frontend (@rules/tests-react.md):**

- ✅ **Testing Library:** Usar `@testing-library/react` para testes de componentes
- ✅ **User-centric Tests:** Testar comportamento, não implementação
- ✅ **Coverage Mínima:** 80% linhas, 75% branches
- ✅ **Playwright E2E:** Testes críticos de fluxo end-to-end

**Backend (@rules/tests.md):**

- ✅ **Unit Tests:** xUnit para Use Cases, controllers
- ✅ **Integration Tests:** WebApplicationFactory para testes de API
- ✅ **Mocking:** Moq para dependências, evitar mocks de entidades de domínio
- ✅ **Coverage Mínima:** 90% para Use Cases críticos

**Backend (@rules/unit-of-work.md):**

- ✅ **Transaction Boundaries:** Use Cases chamam `IUnitOfWork.Commit()`
- ✅ **Repositories:** Não chamam `SaveChangesAsync()` diretamente
- ✅ **Atomicidade:** Operações compostas em uma única transação

**Backend (@rules/logging.md):**

- ✅ **Structured Logging:** `ILogger<T>` com placeholders `{PropertyName}`
- ✅ **Níveis Apropriados:** Information (fluxo normal), Warning (degradação), Error (falhas)
- ✅ **Sem Dados Sensíveis:** Nunca logar senhas, tokens completos

**Validação de Conformidade:**

| Padrão | Aplicável | Status | Notas |
|--------|-----------|--------|-------|
| **react.md** | ✅ Frontend | ✅ Conforme | Todos os componentes seguem padrões estabelecidos |
| **tests-react.md** | ✅ Frontend | ✅ Conforme | Estratégia de testes definida; cobertura >= 80% |
| **tests.md** | ✅ Backend | ✅ Conforme | Use Cases testados com xUnit; integration tests planejados |
| **unit-of-work.md** | ✅ Backend | ✅ Conforme | Novos Use Cases seguem padrão UoW; nenhuma mudança em repositórios |
| **logging.md** | ✅ Backend | ✅ Conforme | Logs estruturados em Use Cases e Controllers |
| **http.md** | ✅ API | ✅ Conforme | Status codes corretos (200, 401, 403, 404); DTOs padronizados |
| **git-commit.md** | ✅ Geral | ✅ Conforme | Commits seguirão Conventional Commits |
| **code-standard.md** | ✅ Geral | ✅ Conforme | C# (.NET 8) e TypeScript (ES2022) |

---

## Apêndice: Diagramas

### Diagrama de Sequência - Login Completo

```
┌─────────┐    ┌──────────────┐    ┌─────────────┐    ┌─────────────┐    ┌──────────┐
│ Usuário │    │   Browser    │    │  Frontend   │    │  Backend    │    │ Database │
└────┬────┘    └──────┬───────┘    └──────┬──────┘    └──────┬──────┘    └────┬─────┘
     │                 │                   │                  │                 │
     │ Acessa link     │                   │                  │                 │
     │ /6SJJRFPD/login │                   │                  │                 │
     │─────────────────>                   │                  │                 │
     │                 │                   │                  │                 │
     │                 │ GET /6SJJRFPD/login                  │                 │
     │                 │───────────────────>                  │                 │
     │                 │                   │                  │                 │
     │                 │                   │ useBarbeariaCode │                 │
     │                 │                   │ extracts codigo  │                 │
     │                 │                   │──────────┐       │                 │
     │                 │                   │          │       │                 │
     │                 │                   │<─────────┘       │                 │
     │                 │                   │                  │                 │
     │                 │                   │ GET /api/barbearias/codigo/6SJJRFPD
     │                 │                   │──────────────────>                 │
     │                 │                   │                  │                 │
     │                 │                   │                  │ SELECT * FROM  │
     │                 │                   │                  │ barbershops    │
     │                 │                   │                  │ WHERE code =   │
     │                 │                   │                  │ '6SJJRFPD'     │
     │                 │                   │                  │─────────────────>
     │                 │                   │                  │                 │
     │                 │                   │                  │ {id, nome...}  │
     │                 │                   │                  │<─────────────────
     │                 │                   │                  │                 │
     │                 │                   │ 200 OK           │                 │
     │                 │                   │ {id, nome,       │                 │
     │                 │                   │  codigo,         │                 │
     │                 │                   │  isActive}       │                 │
     │                 │                   │<──────────────────                 │
     │                 │                   │                  │                 │
     │                 │ Render login form │                  │                 │
     │                 │ "Login - Barbearia│                  │                 │
     │                 │  do Tasso Zé"     │                  │                 │
     │                 │<───────────────────                  │                 │
     │                 │                   │                  │                 │
     │ Vê formulário   │                   │                  │                 │
     │<─────────────────                   │                  │                 │
     │                 │                   │                  │                 │
     │ Preenche        │                   │                  │                 │
     │ email/senha     │                   │                  │                 │
     │─────────────────>                   │                  │                 │
     │                 │                   │                  │                 │
     │ Clica "Entrar"  │                   │                  │                 │
     │─────────────────>                   │                  │                 │
     │                 │                   │                  │                 │
     │                 │ onSubmit()        │                  │                 │
     │                 │───────────────────>                  │                 │
     │                 │                   │                  │                 │
     │                 │                   │ POST /api/auth/  │                 │
     │                 │                   │ admin-barbearia/ │                 │
     │                 │                   │ login            │                 │
     │                 │                   │ {codigo, email,  │                 │
     │                 │                   │  senha}          │                 │
     │                 │                   │──────────────────>                 │
     │                 │                   │                  │                 │
     │                 │                   │                  │ Validate       │
     │                 │                   │                  │ credentials    │
     │                 │                   │                  │─────────┐      │
     │                 │                   │                  │         │      │
     │                 │                   │                  │<────────┘      │
     │                 │                   │                  │                 │
     │                 │                   │                  │ Generate JWT   │
     │                 │                   │                  │─────────┐      │
     │                 │                   │                  │         │      │
     │                 │                   │                  │<────────┘      │
     │                 │                   │                  │                 │
     │                 │                   │ 200 OK           │                 │
     │                 │                   │ {token,          │                 │
     │                 │                   │  tipoUsuario,    │                 │
     │                 │                   │  barbeariaId,    │                 │
     │                 │                   │  nomeBarbearia,  │                 │
     │                 │                   │  expiresAt}      │                 │
     │                 │                   │<──────────────────                 │
     │                 │                   │                  │                 │
     │                 │ Store token +     │                  │                 │
     │                 │ session in        │                  │                 │
     │                 │ localStorage      │                  │                 │
     │                 │<───────────────────                  │                 │
     │                 │                   │                  │                 │
     │                 │ Redirect to       │                  │                 │
     │                 │ /6SJJRFPD/dashboard                  │                 │
     │                 │<───────────────────                  │                 │
     │                 │                   │                  │                 │
     │ Dashboard       │                   │                  │                 │
     │ carregado       │                   │                  │                 │
     │<─────────────────                   │                  │                 │
     │                 │                   │                  │                 │
```

### Diagrama de Componentes - Hierarquia de Páginas

```
App.tsx
├── BrowserRouter
│   ├── Routes
│   │   ├── Route path="/login" → Login (Admin Central)
│   │   ├── Route path="/" → ProtectedRoute (Admin Central)
│   │   │   ├── Route path="/barbearias/*" → BarbershopPages
│   │   │   ├── Route path="/barbeiros" → BarbersListPage (Admin Central)
│   │   │   └── ...
│   │   │
│   │   └── Route path="/:codigo/*" → BarbeariaContext.Provider
│   │       ├── Route path="login" → LoginAdminBarbearia
│   │       └── Route path="/" → ProtectedBarbeariaRoute
│   │           ├── AdminBarbeariaLayout
│   │           │   ├── Header (nome + código da barbearia)
│   │           │   ├── Sidebar (navegação)
│   │           │   └── Outlet (conteúdo dinâmico)
│   │           │
│   │           ├── Route path="dashboard" → Dashboard
│   │           ├── Route path="barbeiros" → BarbeirosListPage
│   │           ├── Route path="barbeiros/novo" → BarbeiroFormPage
│   │           ├── Route path="servicos" → ServicosListPage
│   │           └── Route path="agenda" → AgendamentosListPage
```

---

**Documento criado em:** 2025-01-17  
**Versão:** 1.0  
**Autor:** GitHub Copilot  
**Baseado em:** PRD v1.0 - Interface Administrativa para Admin de Barbearia  
**Status:** ✅ Pronto para Aprovação e Implementação
