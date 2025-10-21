# Implementação Landing Page Personalizável - Resumo de Tarefas

## Visão Geral

Este documento organiza as tarefas necessárias para implementar o sistema de Landing Pages personalizáveis para barbearias, incluindo o painel administrativo e a landing page pública.

## Fases de Implementação

### **Fase 1: Backend (Fundação)**
Implementação da API, banco de dados e lógica de negócio necessária para suportar o sistema de landing pages.

### **Fase 2: Admin Frontend (Gestão)**
Desenvolvimento da interface administrativa para gerenciamento das landing pages.

### **Fase 3: Landing Page Pública (Apresentação)**
Criação dos 5 templates e página pública acessível aos clientes.

### **Fase 4: Integração e Refinamento**
Integração completa entre sistemas e otimizações finais.

---

## Tarefas

### Fase 1: Backend - Fundação (Sequencial)

- [ ] 1.0 Estrutura de Banco de Dados e Migrations
- [ ] 2.0 Entities e DTOs do Domínio Landing Page
- [ ] 3.0 Repositórios e Unit of Work
- [ ] 4.0 Serviços de Domínio (Business Logic)
- [ ] 5.0 API Endpoints - Gestão Admin
- [ ] 6.0 API Endpoint - Landing Page Pública
- [ ] 7.0 Serviço de Upload e Processamento de Logos
- [ ] 8.0 Criação Automática no Cadastro da Barbearia
- [ ] 9.0 Testes Backend (Unit + Integration)

### Fase 2: Admin Frontend - Gestão (Parcialmente Paralelo)

**Lane 1: Setup e Infraestrutura (Bloqueante)**
- [ ] 10.0 Setup de Types, Interfaces e Constants

**Lane 2: Componentes Base (Paralelos após 10.0)**
- [ ] 11.0 Hook useLandingPage e API Service
- [ ] 12.0 Hook useLogoUpload
- [ ] 13.0 Componente TemplateGallery
- [ ] 14.0 Componente LogoUploader
- [ ] 15.0 Componente ServiceManager
- [ ] 16.0 Componente PreviewPanel

**Lane 3: Páginas e Integração (Após Lane 2)**
- [ ] 17.0 Componente LandingPageForm
- [ ] 18.0 Página LandingPageEditor
- [ ] 19.0 Integração com Rotas e Navegação
- [ ] 20.0 Testes Admin Frontend

### Fase 3: Landing Page Pública - Apresentação (Parcialmente Paralelo)

**Lane 1: Setup (Bloqueante)**
- [ ] 21.0 Setup Projeto barbapp-public

**Lane 2: Fundação (Após 21.0)**
- [ ] 22.0 Types, Hooks e API Integration
- [ ] 23.0 Componentes Compartilhados (ServiceCard, WhatsAppButton, etc)

**Lane 3: Templates (Paralelos após 22.0 e 23.0)**
- [ ] 24.0 Template 1 - Clássico
- [ ] 25.0 Template 2 - Moderno
- [ ] 26.0 Template 3 - Vintage
- [ ] 27.0 Template 4 - Urbano
- [ ] 28.0 Template 5 - Premium

**Lane 4: Integração (Após Templates)**
- [ ] 29.0 Página LandingPage e Router
- [ ] 30.0 Testes E2E Landing Page Pública

### Fase 4: Integração e Refinamento (Sequencial)

- [ ] 31.0 Integração Completa Backend ↔ Admin ↔ Público
- [ ] 32.0 Otimizações de Performance e SEO
- [ ] 33.0 Documentação e Deployment

---

## Análise de Paralelização

### Oportunidades de Execução Paralela

**Após conclusão do Backend (tarefas 1.0 - 9.0):**

1. **Lane Admin - Componentes Base** (Tarefas 11.0 - 16.0)
   - Podem ser desenvolvidas em paralelo após 10.0

2. **Lane Public - Templates** (Tarefas 24.0 - 28.0)
   - Os 5 templates podem ser desenvolvidos em paralelo após 22.0 e 23.0

**Desenvolvedores Recomendados:**
- **Dev 1**: Backend completo (Tarefas 1.0 - 9.0)
- **Dev 2**: Admin Frontend (Tarefas 10.0 - 20.0)
- **Dev 3**: Landing Page Pública (Tarefas 21.0 - 30.0)
- **Dev 2 + Dev 3**: Integração Final (Tarefas 31.0 - 33.0)

### Caminho Crítico

O caminho crítico (sequencial obrigatório) é:

1. **Backend** (1.0 → 2.0 → 3.0 → 4.0 → 5.0 + 6.0 + 7.0 + 8.0 → 9.0)
2. **Admin Setup** (10.0 → 11.0 → componentes paralelos → 17.0 → 18.0 → 19.0 → 20.0)
3. **Public Setup** (21.0 → 22.0 + 23.0 → templates paralelos → 29.0 → 30.0)
4. **Integração** (31.0 → 32.0 → 33.0)

**Estimativa de Tempo:**
- **Backend**: ~10-12 dias (sequencial)
- **Admin Frontend**: ~8-10 dias (com paralelização)
- **Landing Page Pública**: ~8-10 dias (com paralelização de templates)
- **Integração**: ~3-5 dias
- **Total**: ~29-37 dias (com 1 desenvolvedor) ou ~15-20 dias (com 3 desenvolvedores)

---

## Dependências Externas

- **Banco de Dados**: PostgreSQL com suporte a UUID
- **Storage**: Sistema de arquivos ou CDN para logos (S3, Cloudinary, etc)
- **Frontend Admin**: React + TypeScript + Vite (já existente)
- **Frontend Público**: Novo projeto React + TypeScript + Vite
- **Bibliotecas**:
  - `@tanstack/react-query` (cache e state management)
  - `react-hook-form` + `zod` (formulários)
  - `@hello-pangea/dnd` (drag & drop)
  - `lucide-react` (ícones)
  - `tailwindcss` (estilos)

---

## Notas de Implementação

1. **Backend deve ser completado primeiro** para ambos frontends terem API funcional
2. **Admin e Public frontends podem ser desenvolvidos em paralelo** após backend
3. **Templates podem ser desenvolvidos em paralelo** por diferentes desenvolvedores
4. **Testes devem ser escritos junto com a implementação**, não deixar para o final
5. **Preview no admin pode ser iterativo**: começar simples e melhorar depois
6. **Criação automática (8.0) pode ser implementada no final do backend** sem bloquear outras tarefas

---

**Data de Criação**: 2025-01-21  
**Versão**: 1.0  
**Feature Slug**: prd-landing-page-barbearia
