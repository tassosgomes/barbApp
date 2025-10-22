# ✅ Checklist de Correção - Landing Page

**Última atualização**: 22/10/2025  
**Progresso Geral**: 0/18 tasks (0%)

---

## 🔴 FASE 1: Correções Críticas Imediatas

**Status**: 🟡 Em Progresso | **Progresso**: 1/2 tasks (50%)

### Task 34: Corrigir Bug #5 (Salvar Alterações - Erro 400) ✅ COMPLETA
- [x] 34.1 - Reproduzir erro com Playwright/DevTools
- [x] 34.2 - Inspecionar logs do backend
- [x] 34.3 - Capturar payload enviado (Network tab)
- [x] 34.4 - Comparar payload com DTO backend
- [x] 34.5 - Identificar campo/validação causando erro
- [x] 34.6 - Corrigir validação (backend ou frontend)
- [x] 34.7 - Testar edição de todos os campos
- [x] 34.8 - Garantir persistência no banco
- [ ] 34.9 - Adicionar testes unitários (pendente)

**Critério de Aceitação**:
- [x] ✅ Editar "Sobre a Barbearia" e salvar sem erro
- [x] ✅ API retorna 204 No Content
- [x] ✅ Dados persistidos no banco

---

### Task 35: Corrigir Bug #4 (Upload de Logo)
- [ ] 35.1 - Preparar arquivo de teste (PNG 300x300px)
- [ ] 35.2 - Tentar upload via admin
- [ ] 35.3 - Capturar erro (console/network)
- [ ] 35.4 - Verificar endpoint backend
- [ ] 35.5 - Verificar multipart/form-data
- [ ] 35.6 - Verificar storage (local/S3)
- [ ] 35.7 - Corrigir problemas
- [ ] 35.8 - Testar formatos (PNG, JPG, SVG, WebP)
- [ ] 35.9 - Testar validações (tamanho, formato)
- [ ] 35.10 - Verificar preview do logo

**Critério de Aceitação**:
- [ ] ✅ Upload de PNG funciona
- [ ] ✅ Logo aparece no preview
- [ ] ✅ URL do logo salva no banco

---

**📊 Resultado da Fase 1**:
- [ ] ✅ Admin 100% funcional
- [ ] ✅ Editar e salvar dados sem erros
- [ ] ✅ Upload de logo funcionando

---

## 🟡 FASE 2: Landing Page Pública - MVP

**Status**: ⬜ Não Iniciada | **Progresso**: 0/8 tasks (0%)

### Task 36: Setup Projeto barbapp-public
- [ ] 36.1 - Criar projeto Vite + React + TypeScript
- [ ] 36.2 - Instalar dependências
- [ ] 36.3 - Configurar Tailwind CSS
- [ ] 36.4 - Criar estrutura de pastas
- [ ] 36.5 - Configurar vite.config.ts
- [ ] 36.6 - Criar .env.example
- [ ] 36.7 - Criar README.md
- [ ] 36.8 - Testar npm run dev (porta 3001)
- [ ] 36.9 - Testar npm run build

**Critério de Aceitação**:
- [ ] ✅ Projeto rodando em localhost:3001
- [ ] ✅ Tailwind CSS funcionando
- [ ] ✅ Build production funciona

---

### Task 37: Implementar Types e Hook useLandingPageData
- [ ] 37.1 - Criar types/landing-page.types.ts
- [ ] 37.2 - Criar api/client.ts
- [ ] 37.3 - Criar api/landingPageApi.ts
- [ ] 37.4 - Configurar TanStack Query provider
- [ ] 37.5 - Criar hooks/useLandingPageData.ts
- [ ] 37.6 - Adicionar tratamento loading/error

**Critério de Aceitação**:
- [ ] ✅ Hook retorna { data, isLoading, error }
- [ ] ✅ Cache de 5 minutos configurado

---

### Task 38: Implementar Componentes Base
- [ ] 38.1 - Criar ServiceCard.tsx
- [ ] 38.2 - Criar WhatsAppButton.tsx
- [ ] 38.3 - Criar LoadingSpinner.tsx
- [ ] 38.4 - Criar ErrorMessage.tsx
- [ ] 38.5 - Criar Header.tsx
- [ ] 38.6 - Criar Footer.tsx

**Critério de Aceitação**:
- [ ] ✅ Componentes renderizam corretamente
- [ ] ✅ Estilização responsiva

---

### Task 39: Implementar Template 3 - Vintage (MVP)
- [ ] 39.1 - Criar templates/Template3.tsx
- [ ] 39.2 - Implementar seções (Hero, Sobre, Serviços, Contato)
- [ ] 39.3 - Estilizar com paleta vintage
- [ ] 39.4 - Adicionar ícones lucide-react
- [ ] 39.5 - Tornar responsivo
- [ ] 39.6 - Adicionar animações
- [ ] 39.7 - Integrar com useLandingPageData

**Critério de Aceitação**:
- [ ] ✅ Template renderiza com dados reais da API
- [ ] ✅ Responsivo (mobile + desktop)
- [ ] ✅ Estilização vintage aplicada

---

### Task 40: Criar Página LandingPage.tsx
- [ ] 40.1 - Criar pages/LandingPage.tsx
- [ ] 40.2 - Extrair codigo da URL (useParams)
- [ ] 40.3 - Usar useLandingPageData(codigo)
- [ ] 40.4 - Renderizar template baseado em templateId
- [ ] 40.5 - Adicionar loading spinner
- [ ] 40.6 - Adicionar erro 404

**Critério de Aceitação**:
- [ ] ✅ Página carrega dados da API
- [ ] ✅ Template correto renderizado
- [ ] ✅ Erro 404 se código inválido

---

### Task 41: Implementar Endpoint Público no Backend
- [ ] 41.1 - Criar PublicLandingPagesController.cs
- [ ] 41.2 - Implementar GET /api/public/landing-pages/{codigo}
- [ ] 41.3 - Criar GetLandingPageByCodeQuery
- [ ] 41.4 - Criar PublicLandingPageDto
- [ ] 41.5 - Adicionar cache (5 minutos)
- [ ] 41.6 - Tratar erro 404
- [ ] 41.7 - Adicionar testes unitários

**Critério de Aceitação**:
- [ ] ✅ Endpoint retorna 200 OK com dados
- [ ] ✅ Endpoint é público (sem auth)
- [ ] ✅ Cache de 5 minutos funcionando

---

### Task 42: Configurar Rota Pública no Frontend
- [ ] 42.1 - Modificar routes/index.tsx
- [ ] 42.2 - Adicionar rota /barbearia/:codigo
- [ ] 42.3 - Criar PublicLandingPageRedirect.tsx
- [ ] 42.4 - Decidir estratégia (redirect vs iframe vs separado)

**Critério de Aceitação**:
- [ ] ✅ /barbearia/CEB4XAR7 carrega landing page
- [ ] ✅ Não redireciona para login

---

### Task 43: Testar Integração Completa Fase 2
- [ ] 43.1 - Editar landing page no admin
- [ ] 43.2 - Alterar "Sobre" e salvar
- [ ] 43.3 - Clicar "Abrir Landing Page"
- [ ] 43.4 - Verificar que abre em nova aba
- [ ] 43.5 - Verificar alterações aparecem
- [ ] 43.6 - Testar responsividade
- [ ] 43.7 - Testar botão WhatsApp
- [ ] 43.8 - Verificar redes sociais
- [ ] 43.9 - Testar cache (2 acessos, 1 request)

**Critério de Aceitação**:
- [ ] ✅ Bug #1 corrigido (botão "Abrir" funciona)
- [ ] ✅ Bug #2 corrigido (URL pública funciona)
- [ ] ✅ Bug #3 corrigido (botão "Abrir Landing Page")
- [ ] ✅ Dados admin refletem na landing pública
- [ ] ✅ Landing page é pública (sem login)

---

**📊 Resultado da Fase 2**:
- [ ] ✅ Landing page pública funcionando
- [ ] ✅ 1 template (Vintage) renderizado
- [ ] ✅ Bugs #1, #2, #3 corrigidos
- [ ] ✅ Cliente pode acessar /barbearia/CODIGO
- [ ] 🚀 **PRONTO PARA DEPLOY MVP**

---

## 🟢 FASE 3: Templates Adicionais

**Status**: ⬜ Não Iniciada | **Progresso**: 0/5 tasks (0%)

### Task 44: Template 1 - Moderno
- [ ] 44.1 - Criar templates/Template1.tsx
- [ ] 44.2 - Implementar design moderno
- [ ] 44.3 - Tornar responsivo
- [ ] 44.4 - Integrar com useLandingPageData
- [ ] 44.5 - Testar renderização

**Status**: ⬜ Não Iniciada

---

### Task 45: Template 2 - Clássico
- [ ] 45.1 - Criar templates/Template2.tsx
- [ ] 45.2 - Implementar design clássico
- [ ] 45.3 - Tornar responsivo
- [ ] 45.4 - Integrar com useLandingPageData
- [ ] 45.5 - Testar renderização

**Status**: ⬜ Não Iniciada

---

### Task 46: Template 4 - Minimalista
- [ ] 46.1 - Criar templates/Template4.tsx
- [ ] 46.2 - Implementar design minimalista
- [ ] 46.3 - Tornar responsivo
- [ ] 46.4 - Integrar com useLandingPageData
- [ ] 46.5 - Testar renderização

**Status**: ⬜ Não Iniciada

---

### Task 47: Template 5 - Luxo
- [ ] 47.1 - Criar templates/Template5.tsx
- [ ] 47.2 - Implementar design luxo
- [ ] 47.3 - Tornar responsivo
- [ ] 47.4 - Integrar com useLandingPageData
- [ ] 47.5 - Testar renderização

**Status**: ⬜ Não Iniciada

---

### Task 48: Testar Seleção de Templates
- [ ] 48.1 - Trocar template no admin
- [ ] 48.2 - Verificar preview atualiza
- [ ] 48.3 - Acessar landing page pública
- [ ] 48.4 - Testar todos os 5 templates
- [ ] 48.5 - Verificar dados aparecem em todos

**Critério de Aceitação**:
- [ ] ✅ Trocar template no admin reflete na landing pública
- [ ] ✅ Todos os 5 templates renderizam
- [ ] ✅ Preview corresponde ao template público

---

**📊 Resultado da Fase 3**:
- [ ] ✅ 5 templates disponíveis
- [ ] ✅ Admin pode escolher qualquer template
- [ ] ✅ Sistema completo

---

## 🔵 FASE 4: Deploy e Finalização

**Status**: ⬜ Não Iniciada | **Progresso**: 0/3 tasks (0%)

### Task 49: Dockerfile e Docker Compose
- [ ] 49.1 - Criar barbapp-public/Dockerfile
- [ ] 49.2 - Adicionar ao docker-compose.yml
- [ ] 49.3 - Configurar Nginx (rotas)
- [ ] 49.4 - Testar build e deploy

**Status**: ⬜ Não Iniciada

---

### Task 50: CI/CD Pipeline
- [ ] 50.1 - Configurar pipeline para barbapp-public
- [ ] 50.2 - Testar build automatizado
- [ ] 50.3 - Testar deploy automatizado

**Status**: ⬜ Não Iniciada

---

### Task 51: Documentação Final
- [ ] 51.1 - Atualizar README principal
- [ ] 51.2 - Documentar barbapp-public
- [ ] 51.3 - Criar guia de deploy

**Status**: ⬜ Não Iniciada

---

**📊 Resultado da Fase 4**:
- [ ] ✅ Deploy automatizado
- [ ] ✅ Documentação completa
- [ ] 🎉 **PROJETO FINALIZADO**

---

## 📈 Progresso por Fase

```
FASE 1: ████████████░░░░░░ 50% (1/2 tasks) ✅ Task 34 completa
FASE 2: ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%  (0/8 tasks)
FASE 3: ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%  (0/5 tasks)
FASE 4: ⬜⬜⬜⬜⬜⬜⬜⬜⬜⬜ 0%  (0/3 tasks)
──────────────────────────────────
TOTAL:  ██░░░░░░░░░░░░░░░░ 6%  (1/18 tasks)
```

---

## 🎯 Marcos (Milestones)

- [ ] **Milestone 1**: Fase 1 Completa - Admin 100% Funcional
- [ ] **Milestone 2**: Fase 2 Completa - Landing Page MVP Funcionando 🚀
- [ ] **Milestone 3**: Fase 3 Completa - 5 Templates Disponíveis
- [ ] **Milestone 4**: Fase 4 Completa - Deploy Automatizado 🎉

---

## 📅 Cronograma Sugerido

| Fase | Início | Fim | Duração |
|------|--------|-----|---------|
| Fase 1 | Dia 1 | Dia 1 | 1 dia |
| Fase 2 | Dia 2 | Dia 5 | 4 dias |
| Fase 3 | Dia 6 | Dia 8 | 3 dias |
| Fase 4 | Dia 9 | Dia 9 | 1 dia |
| **TOTAL** | - | - | **9 dias** |

---

## 🔄 Como Usar Este Checklist

1. **Marque cada item** com `[x]` ao completar
2. **Atualize o progresso** no topo de cada fase
3. **Registre a data** ao completar uma fase
4. **Documente problemas** encontrados em comentários
5. **Celebre os marcos!** 🎉

---

**Última atualização**: 22/10/2025  
**Próxima task**: Task 34.0 - Corrigir Bug #5
