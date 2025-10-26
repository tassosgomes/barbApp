# Tasks de Correção - Landing Page

**Baseado no**: BUGS_REPORT.md  
**Data**: 22 de Outubro de 2025

---

## 📋 Visão Geral das Fases

| Fase | Descrição | Duração Estimada | Bugs Corrigidos | Status |
|------|-----------|------------------|-----------------|--------|
| **Fase 1** | Correções Críticas Imediatas | 2-3 dias | Bug #4, #5 | � 50% (1/2) |
| **Fase 2** | Refatoração Infraestrutura | 1 dia | - | �🔴 Pendente |
| **Fase 3** | Landing Page Pública - MVP | 3-5 dias | Bug #1, #2, #3 | 🔴 Pendente |
| **Fase 4** | Templates Adicionais | 2-3 dias | - | 🔴 Pendente |
| **Fase 5** | Deploy e Finalização | 1-2 dias | - | 🔴 Pendente |

---

## 🔴 FASE 1: Correções Críticas Imediatas

**Objetivo**: Garantir que o editor de landing page no admin funcione completamente.

**Duração**: 2-3 dias  
**Prioridade**: 🔥 Crítica  
**Bugs Corrigidos**: Bug #4 (Upload), Bug #5 (Salvar)  
**Status**: 🟡 50% (1/2 tasks)

---

### Task 34.0: Investigar e Corrigir Bug #5 (Salvar Alterações - Erro 400)

**Status**: ✅ COMPLETO  
**Complexidade**: Média  
**Duração**: 4 horas  
**Arquivo**: [34_task_COMPLETED.md](./34_task_COMPLETED.md)

#### Subtarefas

- [ ] **34.1**: Reproduzir o erro localmente com Playwright/DevTools
- [ ] **34.2**: Inspecionar logs do backend (`dotnet run --project src/BarbApp.API/`)
- [ ] **34.3**: Capturar payload enviado pelo frontend (Network tab)
- [ ] **34.4**: Comparar payload com DTO esperado no backend
- [ ] **34.5**: Identificar campo/validação que está causando erro 400
- [ ] **34.6**: Corrigir validação no backend OU ajustar payload no frontend
- [ ] **34.7**: Testar edição de todos os campos:
  - Sobre a Barbearia
  - Horário de Funcionamento
  - WhatsApp
  - Instagram
  - Facebook
- [ ] **34.8**: Garantir que alterações são persistidas no banco
- [ ] **34.9**: Adicionar testes unitários para validação correta

#### Arquivos Envolvidos

**Frontend**:
- `barbapp-admin/src/pages/LandingPage/LandingPageEditor.tsx`
- `barbapp-admin/src/features/landing-page/api/landingPageApi.ts`

**Backend**:
- `backend/src/BarbApp.Application/UseCases/LandingPages/UpdateLandingPage/`
- `backend/src/BarbApp.API/Controllers/LandingPagesController.cs`

#### Critérios de Aceitação

- [ ] Editar "Sobre a Barbearia" e salvar sem erro 400
- [ ] Editar todos os campos de texto e salvar com sucesso
- [ ] API retorna 200 OK com dados atualizados
- [ ] Dados persistidos corretamente no banco
- [ ] Mensagem de sucesso exibida no frontend

---

### Task 35.0: Testar e Corrigir Bug #4 (Upload de Logo)

**Status**: ✅ COMPLETO  
**Complexidade**: Média  
**Duração**: 3 horas  
**Arquivo**: [35_task_IN_PROGRESS.md](./35_task_IN_PROGRESS.md)

#### Problema Identificado

Nome do campo FormData incorreto no frontend:
- ❌ `formData.append('logo', file)` 
- ✅ `formData.append('file', file)`

#### Correção

**Arquivo**: `/barbapp-admin/src/services/api/landing-page.api.ts` (linha 71)

```typescript
// ANTES (bug)
formData.append('logo', file);

// DEPOIS (correto)
formData.append('file', file);
```

#### Validação

- ✅ Upload via curl funcionou (200 OK)
- ✅ Backend processa imagens corretamente
- ✅ ImageSharp redimensiona para 300x300px
- ✅ Correção aplicada no frontend
- ✅ Upload funcional via interface

---

## � FASE 2: Refatoração de Infraestrutura

**Objetivo**: Migrar upload de arquivos do filesystem local para Cloudflare R2 Object Storage.

**Duração**: 1 dia (4-6 horas)  
**Prioridade**: 🔥 Alta  
**Status**: 📋 Backlog

**Justificativa**: 
- Arquivos salvos em containers são perdidos ao reiniciar
- Não escala em ambientes multi-container
- R2 oferece CDN integrado, backup automático e custo baixo

---

### Task 36.0: Refatorar Upload para Cloudflare R2

**Status**: 📋 BACKLOG  
**Complexidade**: Alta  
**Duração**: 4-6 horas  
**Arquivo**: [36_task_BACKLOG.md](./36_task_BACKLOG.md)

#### Escopo

**Backend:**
1. Instalar AWSSDK.S3 NuGet package
2. Criar `R2StorageService` (S3-compatible)
3. Criar `R2LogoUploadService` (substitui `LocalLogoUploadService`)
4. Configurar credenciais R2 em appsettings.json
5. Registrar serviços no DI

**Infraestrutura:**
1. Criar bucket `barbapp-assets` no Cloudflare R2
2. Configurar CORS
3. Gerar Access Keys
4. Configurar custom domain: `assets.barbapp.com`
5. Adicionar secrets no Docker Swarm

**Testes:**
1. Testes unitários do R2StorageService
2. Testes de integração do upload
3. Testes E2E com Playwright
4. Validação de CDN funcionando

#### Benefícios

- ✅ Persistência de dados (sobrevive restarts)
- ✅ Escalabilidade (funciona em clusters)
- ✅ CDN automático (performance)
- ✅ Custo baixo (~$0.05/mês)
- ✅ Backup e redundância
- ✅ Zero egress fees

#### Critérios de Aceitação

- [ ] Logos salvos no R2 ao invés de filesystem
- [ ] URLs retornadas apontam para CDN
- [ ] Upload funciona em dev e produção
- [ ] Migration guide criado (para logos existentes)
- [ ] Testes passando (unit + integration + E2E)
- [ ] Documentação atualizada

---

## 🔴 FASE 3: Landing Page Pública - MVP

**Objetivo**: Criar aplicação frontend pública para exibir landing pages das barbearias.

**Duração**: 3-5 dias  
**Prioridade**: Alta  
**Bugs Corrigidos**: Bug #1, #2, #3  
**Status**: 🔴 Pendente

**Requisitos**: Fase 1 e 2 completas

---

### Task 37.0: Setup Projeto barbapp-public

#### Subtarefas

- [ ] **35.1**: Preparar arquivo de teste (PNG 300x300px, <2MB)
- [ ] **35.2**: Tentar upload via interface do admin
- [ ] **35.3**: Capturar erro exato (se houver) no console/network
- [ ] **35.4**: Verificar endpoint de upload no backend:
  - `POST /api/admin/landing-pages/:id/logo`
- [ ] **35.5**: Verificar se multipart/form-data está configurado
- [ ] **35.6**: Verificar se storage (local/S3) está configurado
- [ ] **35.7**: Corrigir problemas identificados (backend e/ou frontend)
- [ ] **35.8**: Testar upload de diferentes formatos (PNG, JPG, SVG, WebP)
- [ ] **35.9**: Testar validações:
  - Arquivo maior que 2MB deve ser rejeitado
  - Formato inválido deve ser rejeitado
- [ ] **35.10**: Verificar se preview do logo aparece após upload

#### Arquivos Envolvidos

**Frontend**:
- `barbapp-admin/src/features/landing-page/components/LogoUpload.tsx`
- `barbapp-admin/src/features/landing-page/api/landingPageApi.ts`

**Backend**:
- `backend/src/BarbApp.Application/UseCases/LandingPages/UploadLogo/`
- `backend/src/BarbApp.API/Controllers/LandingPagesController.cs`
- `backend/src/BarbApp.Infrastructure/Storage/` (se existir)

#### Critérios de Aceitação

- [ ] Upload de PNG válido funciona
- [ ] Upload de JPG válido funciona
- [ ] Upload de SVG válido funciona
- [ ] Upload de WebP válido funciona
- [ ] Logo aparece no preview após upload
- [ ] Arquivo é salvo no storage correto
- [ ] URL do logo é retornada e salva no banco
- [ ] Validações de tamanho e formato funcionam

---

## 🟡 FASE 2: Landing Page Pública - MVP

**Objetivo**: Criar landing page pública funcional com 1 template.

**Duração**: 3-5 dias  
**Prioridade**: 🔥 Crítica  
**Bugs Corrigidos**: Bug #1, #2, #3 (rotas públicas)

---

### Task 36.0: Setup Projeto barbapp-public (Task 21)

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 2 horas

#### Subtarefas

- [ ] **36.1**: Criar projeto Vite + React + TypeScript
  ```bash
  cd barbApp
  npm create vite@latest barbapp-public -- --template react-ts
  cd barbapp-public
  npm install
  ```
- [ ] **36.2**: Instalar dependências principais
  ```bash
  npm install react-router-dom @tanstack/react-query axios lucide-react
  npm install -D tailwindcss postcss autoprefixer
  npx tailwindcss init -p
  ```
- [ ] **36.3**: Configurar Tailwind CSS (cores dos templates)
- [ ] **36.4**: Criar estrutura de pastas:
  ```
  src/
    templates/         # Template1, Template2, etc
    components/        # ServiceCard, WhatsAppButton
    hooks/             # useLandingPageData
    types/             # landing-page.types.ts
    pages/             # LandingPage.tsx
    api/               # axios client
  ```
- [ ] **36.5**: Configurar `vite.config.ts` (porta 3001)
- [ ] **36.6**: Criar `.env.example`:
  ```
  VITE_API_URL=http://localhost:5070/api
  ```
- [ ] **36.7**: Criar `README.md` do projeto
- [ ] **36.8**: Testar `npm run dev` (deve rodar em localhost:3001)
- [ ] **36.9**: Testar `npm run build` (deve buildar sem erros)

#### Critérios de Aceitação

- [ ] Projeto criado e rodando em localhost:3001
- [ ] Tailwind CSS funcionando
- [ ] Estrutura de pastas criada
- [ ] Build production funciona
- [ ] README documentado

---

### Task 37.0: Implementar Types e Hook useLandingPageData (Task 22 - Parcial)

**Status**: 🔴 Pendente  
**Complexidade**: Média  
**Duração**: 3 horas

#### Subtarefas

- [ ] **37.1**: Criar `types/landing-page.types.ts`
  ```typescript
  export interface LandingPageData {
    id: string;
    barbeariaId: string;
    templateId: number;
    logoUrl?: string;
    sobre?: string;
    horarioFuncionamento?: string;
    whatsapp?: string;
    instagram?: string;
    facebook?: string;
    services: ServiceItem[];
  }
  
  export interface ServiceItem {
    id: string;
    nome: string;
    descricao?: string;
    preco: number;
    duracaoMinutos: number;
    ordem: number;
  }
  ```
- [ ] **37.2**: Criar `api/client.ts` (Axios configurado)
- [ ] **37.3**: Criar `api/landingPageApi.ts` com método:
  ```typescript
  export const fetchPublicLandingPage = (codigo: string): Promise<LandingPageData>
  ```
- [ ] **37.4**: Configurar TanStack Query provider em `main.tsx`
- [ ] **37.5**: Criar `hooks/useLandingPageData.ts`:
  ```typescript
  export function useLandingPageData(codigo: string) {
    return useQuery({
      queryKey: ['landing-page', codigo],
      queryFn: () => fetchPublicLandingPage(codigo),
      staleTime: 5 * 60 * 1000, // 5 minutos
    });
  }
  ```
- [ ] **37.6**: Adicionar tratamento de loading e error states

#### Critérios de Aceitação

- [ ] Types definidos corretamente
- [ ] Hook retorna `{ data, isLoading, error }`
- [ ] Cache de 5 minutos configurado
- [ ] Tratamento de erro implementado

---

### Task 38.0: Implementar Componentes Base (Task 23 - Parcial)

**Status**: 🔴 Pendente  
**Complexidade**: Média  
**Duração**: 4 horas

#### Subtarefas

- [ ] **38.1**: Criar `components/ServiceCard.tsx`
  - Exibir nome, descrição, preço, duração
  - Estilização com Tailwind
- [ ] **38.2**: Criar `components/WhatsAppButton.tsx`
  - Versão normal (inline)
  - Versão floating (fixed bottom-right)
  - Link para `https://wa.me/${whatsapp}`
- [ ] **38.3**: Criar `components/LoadingSpinner.tsx`
- [ ] **38.4**: Criar `components/ErrorMessage.tsx`
- [ ] **38.5**: Criar `components/Header.tsx` (simples, com logo)
- [ ] **38.6**: Criar `components/Footer.tsx` (copyright + link admin)

#### Critérios de Aceitação

- [ ] Componentes renderizam corretamente
- [ ] Estilização responsiva (mobile + desktop)
- [ ] Acessibilidade (aria-labels)

---

### Task 39.0: Implementar Template 3 - Vintage (Task 26 - MVP)

**Status**: 🔴 Pendente  
**Complexidade**: Alta  
**Duração**: 6 horas

#### Subtarefas

- [ ] **39.1**: Criar `templates/Template3.tsx` (Vintage)
- [ ] **39.2**: Implementar seções:
  - Hero com logo e fundo escuro
  - Sobre a Barbearia (texto com ícones vintage)
  - Serviços (grid de cards)
  - Contato (horário, WhatsApp, redes sociais)
  - Footer
- [ ] **39.3**: Estilizar com paleta vintage:
  - Dourado (#D4AF37)
  - Preto/cinza escuro
  - Tipografia: Playfair Display (serif)
- [ ] **39.4**: Adicionar ícones com lucide-react
- [ ] **39.5**: Tornar responsivo (breakpoints: mobile, tablet, desktop)
- [ ] **39.6**: Adicionar animações sutis (fade-in on scroll)
- [ ] **39.7**: Integrar com `useLandingPageData`:
  ```typescript
  const { data, isLoading, error } = useLandingPageData(codigo);
  ```

#### Critérios de Aceitação

- [ ] Template renderiza com dados reais da API
- [ ] Todas as seções funcionando
- [ ] Responsivo em mobile e desktop
- [ ] Estilização vintage aplicada
- [ ] Loading e error states tratados

---

### Task 40.0: Criar Página Pública LandingPage.tsx

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 2 horas

#### Subtarefas

- [ ] **40.1**: Criar `pages/LandingPage.tsx`
- [ ] **40.2**: Extrair `codigo` da URL usando `useParams()`
- [ ] **40.3**: Usar `useLandingPageData(codigo)` para buscar dados
- [ ] **40.4**: Renderizar template baseado em `data.templateId`:
  ```typescript
  const templates = {
    3: Template3,
    // Fase 3: adicionar Template1, Template2, etc
  };
  const TemplateComponent = templates[data.templateId] || Template3;
  return <TemplateComponent data={data} />;
  ```
- [ ] **40.5**: Adicionar loading spinner enquanto carrega
- [ ] **40.6**: Adicionar erro 404 se barbearia não encontrada

#### Critérios de Aceitação

- [ ] Página carrega dados da API pública
- [ ] Template correto é renderizado
- [ ] Loading state funciona
- [ ] Erro 404 exibido se código inválido

---

### Task 41.0: Implementar Endpoint Público no Backend (Task 30)

**Status**: 🔴 Pendente  
**Complexidade**: Média  
**Duração**: 3 horas

#### Subtarefas

- [ ] **41.1**: Criar `PublicLandingPagesController.cs`:
  ```csharp
  [Route("api/public/landing-pages")]
  [AllowAnonymous]
  public class PublicLandingPagesController : ControllerBase
  ```
- [ ] **41.2**: Implementar endpoint:
  ```csharp
  [HttpGet("{codigo}")]
  public async Task<IActionResult> GetByCode(string codigo)
  ```
- [ ] **41.3**: Criar query `GetLandingPageByCodeQuery`:
  - Buscar barbearia por código
  - Buscar landing page por barbeariaId
  - Incluir serviços (com `ordem` e `ativo = true`)
- [ ] **41.4**: Criar DTO `PublicLandingPageDto`:
  - Sem expor dados sensíveis (ex: email admin)
  - Apenas dados públicos (logo, sobre, horário, etc)
- [ ] **41.5**: Adicionar cache no endpoint (5 minutos):
  ```csharp
  [ResponseCache(Duration = 300)]
  ```
- [ ] **41.6**: Tratar erro 404 se código não existir
- [ ] **41.7**: Adicionar testes unitários do endpoint

#### Arquivos Envolvidos

**Backend**:
- `backend/src/BarbApp.API/Controllers/PublicLandingPagesController.cs` (novo)
- `backend/src/BarbApp.Application/UseCases/LandingPages/GetByCode/` (novo)

#### Critérios de Aceitação

- [ ] Endpoint retorna 200 OK com dados corretos
- [ ] Endpoint é público (sem autenticação)
- [ ] Cache de 5 minutos funcionando
- [ ] Retorna 404 se código inválido
- [ ] Testes unitários passando

---

### Task 42.0: Configurar Rota Pública no Frontend Admin (Task 29)

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 1 hora

#### Subtarefas

- [ ] **42.1**: Modificar `barbapp-admin/src/routes/index.tsx`
- [ ] **42.2**: Adicionar rota pública `/barbearia/:codigo`:
  ```typescript
  {
    path: '/barbearia/:codigo',
    element: <PublicLandingPageRedirect />,
  }
  ```
- [ ] **42.3**: Criar componente `PublicLandingPageRedirect.tsx`:
  - Redireciona para `http://localhost:3001/barbearia/:codigo`
  - Ou renderiza `<iframe>` apontando para barbapp-public
- [ ] **42.4**: ⚠️ **ALTERNATIVA RECOMENDADA**: Mover rota para `barbapp-public`
  - Landing page pública deve rodar em domínio/porta separado
  - Nginx reverso pode rotear `/barbearia/*` para porta 3001

#### Critérios de Aceitação

- [ ] Acessar `/barbearia/CEB4XAR7` carrega landing page pública
- [ ] Não redireciona para login
- [ ] Landing page é acessível sem autenticação

---

### Task 43.0: Testar Integração Completa da Fase 2

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 2 horas

#### Subtarefas

- [ ] **43.1**: Editar landing page no admin (`/CEB4XAR7/landing-page`)
- [ ] **43.2**: Alterar "Sobre a Barbearia" e salvar
- [ ] **43.3**: Clicar em "Abrir Landing Page"
- [ ] **43.4**: Verificar que landing page pública abre em nova aba
- [ ] **43.5**: Verificar que alterações aparecem na landing page pública
- [ ] **43.6**: Testar responsividade (mobile, tablet, desktop)
- [ ] **43.7**: Testar botão WhatsApp (deve abrir link wa.me)
- [ ] **43.8**: Verificar redes sociais (Instagram, Facebook)
- [ ] **43.9**: Testar cache (acessar 2x, verificar se faz 1 request)

#### Critérios de Aceitação

- [ ] ✅ Bug #1 corrigido (botão "Abrir" funciona)
- [ ] ✅ Bug #2 corrigido (URL pública funciona)
- [ ] ✅ Bug #3 corrigido (botão "Abrir Landing Page" funciona)
- [ ] Dados do admin refletem na landing pública
- [ ] Landing page é pública (sem login)

---

## 🟢 FASE 3: Templates Adicionais

**Objetivo**: Implementar os 4 templates restantes (1, 2, 4, 5).

**Duração**: 2-3 dias  
**Prioridade**: Média

---

### Task 44.0: Implementar Template 1 - Moderno (Task 24)

**Status**: 🔴 Pendente  
**Complexidade**: Alta  
**Duração**: 4 horas

#### Subtarefas

- [ ] **44.1**: Criar `templates/Template1.tsx`
- [ ] **44.2**: Implementar design moderno:
  - Hero com gradiente
  - Cards com sombras e bordas arredondadas
  - Tipografia: Inter (sans-serif)
  - Paleta: azul/roxo/verde
- [ ] **44.3**: Tornar responsivo
- [ ] **44.4**: Integrar com `useLandingPageData`
- [ ] **44.5**: Testar renderização

---

### Task 45.0: Implementar Template 2 - Clássico (Task 25)

**Status**: 🔴 Pendente  
**Complexidade**: Alta  
**Duração**: 4 horas

*(Mesma estrutura da Task 44, mas design clássico)*

---

### Task 46.0: Implementar Template 4 - Minimalista (Task 27)

**Status**: 🔴 Pendente  
**Complexidade**: Alta  
**Duração**: 4 horas

*(Mesma estrutura da Task 44, mas design minimalista)*

---

### Task 47.0: Implementar Template 5 - Luxo (Task 28)

**Status**: 🔴 Pendente  
**Complexidade**: Alta  
**Duração**: 4 horas

*(Mesma estrutura da Task 44, mas design luxo)*

---

### Task 48.0: Testar Seleção de Templates

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 2 horas

#### Subtarefas

- [ ] **48.1**: Trocar template no admin (aba "Escolher Template")
- [ ] **48.2**: Verificar que preview atualiza no admin
- [ ] **48.3**: Acessar landing page pública e verificar novo template
- [ ] **48.4**: Testar todos os 5 templates
- [ ] **48.5**: Verificar que dados (logo, sobre, etc) aparecem em todos

#### Critérios de Aceitação

- [ ] Trocar template no admin reflete na landing page pública
- [ ] Todos os 5 templates renderizam corretamente
- [ ] Preview corresponde ao template público

---

## 🔵 FASE 4: Deploy e Finalização

**Objetivo**: Preparar sistema para produção.

**Duração**: 1-2 dias  
**Prioridade**: Baixa

---

### Task 49.0: Dockerfile e Docker Compose (Task 31)

**Status**: 🔴 Pendente  
**Complexidade**: Média  
**Duração**: 3 horas

#### Subtarefas

- [ ] **49.1**: Criar `barbapp-public/Dockerfile`
- [ ] **49.2**: Adicionar `barbapp-public` ao `docker-compose.yml` (porta 3001)
- [ ] **49.3**: Configurar Nginx para rotear:
  - `/barbearia/*` → barbapp-public (porta 3001)
  - `/admin/*` → barbapp-admin (porta 3000)
  - `/api/*` → backend (porta 5070)
- [ ] **49.4**: Testar build e deploy com Docker

---

### Task 50.0: CI/CD Pipeline (Task 32)

**Status**: 🔴 Pendente  
**Complexidade**: Média  
**Duração**: 2 horas

---

### Task 51.0: Documentação Final (Task 33)

**Status**: 🔴 Pendente  
**Complexidade**: Baixa  
**Duração**: 2 horas

---

## 📊 Resumo de Esforço

| Fase | Tasks | Duração Total | Status |
|------|-------|---------------|--------|
| **Fase 1** | 34-35 | 7 horas (~1 dia) | 🔴 Pendente |
| **Fase 2** | 36-43 | 23 horas (~3 dias) | 🔴 Pendente |
| **Fase 3** | 44-48 | 18 horas (~2 dias) | 🔴 Pendente |
| **Fase 4** | 49-51 | 7 horas (~1 dia) | 🔴 Pendente |
| **TOTAL** | 18 tasks | **55 horas (~7 dias)** | 🔴 Pendente |

---

## 🎯 Recomendação de Execução

### **Abordagem Ágil: Entregar Valor Incremental**

1. **Sprint 1 (2 dias)**: Fase 1 completa
   - ✅ Admin totalmente funcional
   - ✅ Edição e upload funcionando
   
2. **Sprint 2 (5 dias)**: Fase 2 completa (MVP)
   - ✅ Landing page pública funcionando
   - ✅ 1 template (Vintage)
   - ✅ Bugs #1, #2, #3 corrigidos
   - 🚀 **DEPLOY MVP**
   
3. **Sprint 3 (3 dias)**: Fase 3 completa
   - ✅ 5 templates disponíveis
   - ✅ Sistema completo
   
4. **Sprint 4 (1 dia)**: Fase 4 completa
   - ✅ Deploy automatizado
   - ✅ Documentação finalizada

---

**Próxima Ação**: Começar Task 34.0 (Investigar Bug #5)
