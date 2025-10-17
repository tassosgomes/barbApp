# Índice de PRDs - barbApp Fase 1 (MVP)

## Visão Geral

Este diretório contém os Product Requirement Documents (PRDs) para a Fase 1 (MVP) do barbApp, um sistema SaaS multi-tenant para gestão de agendamentos em barbearias.

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunhos para Revisão

## Stack Tecnológica

- **Frontend**: React + Vite + TypeScript (Web Responsiva)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (Relacional)

## PRDs da Fase 1

### 1. [Gestão de Barbearias (Admin Central)](./prd-gestao-barbearias-admin-central/prd.md)
**Responsável**: Admin Central  
**Objetivo**: CRUD de barbearias, geração de código único, listagem e filtros

**Funcionalidades Principais**:
- Criar barbearia com geração automática de código único
- Editar informações cadastrais
- Visualizar e filtrar lista de barbearias
- Excluir barbearias com confirmação

**Dependências**: PRD 5 (Sistema Multi-tenant)

---

### 2. [Gestão de Barbeiros (Admin Barbearia)](./prd-gestao-barbeiros-admin-barbearia/prd.md)
**Responsável**: Admin da Barbearia  
**Objetivo**: Adicionar/remover barbeiros, visualizar agenda completa da equipe

**Funcionalidades Principais**:
- Adicionar barbeiros à equipe
- Remover barbeiros (com tratamento de agendamentos)
- Visualizar equipe de barbeiros
- Visualizar agenda consolidada de todos os barbeiros
- Filtrar agenda por barbeiro ou período

**Dependências**: PRD 1, PRD 5

---

### 3. [Sistema de Agendamentos (Barbeiro)](./prd-sistema-agendamentos-barbeiro/prd.md)
**Responsável**: Barbeiro  
**Objetivo**: Gerenciar agendas isoladas em múltiplas barbearias, confirmar/cancelar atendimentos

**Funcionalidades Principais**:
- Acesso multi-agenda isolado (uma agenda por barbearia)
- Visualização de agendamentos por dia/período
- Confirmar agendamentos pendentes
- Cancelar agendamentos
- Marcar agendamentos como concluídos
- Trocar de contexto entre barbearias

**Dependências**: PRD 2, PRD 5

---

### 4. [Cadastro e Agendamento (Cliente)](./prd-cadastro-agendamento-cliente/prd.md)
**Responsável**: Cliente  
**Objetivo**: Cadastro via código, agendamento de serviços, histórico isolado por barbearia

**Funcionalidades Principais**:
- Cadastro por código da barbearia
- Login simples (telefone + nome)
- Visualizar barbeiros e horários disponíveis
- Criar agendamentos (escolher barbeiro, serviço, data/hora)
- Visualizar e cancelar agendamentos futuros
- Visualizar histórico de serviços
- Acesso multi-barbearia isolado

**Dependências**: PRD 2, PRD 3, PRD 5

---

### 5. [Sistema Multi-tenant e Autenticação](./prd-sistema-multi-tenant/prd.md)
**Responsável**: Sistema/Infraestrutura  
**Objetivo**: Isolamento total de dados, autenticação multi-perfil, contexto por barbearia

**Funcionalidades Principais**:
- Identificação de contexto por código/URL
- Isolamento de dados multi-tenant (100% segregação)
- Autenticação multi-perfil (Admin Central, Admin Barbearia, Barbeiro, Cliente)
- Gerenciamento de sessão e contexto
- Troca de contexto para usuários multi-vinculados
- Autorização por perfil (RBAC)
- Cadastro multi-vinculado (mesmo telefone em múltiplas barbearias)

**Dependências**: Nenhuma (é a base de todos os outros PRDs)

---

### 15. [Onboarding Automático do Admin da Barbearia](./prd-onboarding-admin-barbearia/prd.md) 🆕
**Responsável**: Admin Central  
**Objetivo**: Automatizar criação de credenciais de acesso do Admin Barbearia no cadastro

**Funcionalidades Principais**:
- Criação automática de usuário Admin Barbearia ao cadastrar barbearia
- Geração de senha aleatória segura (12 caracteres)
- Envio de e-mail com credenciais de acesso
- Reenvio de credenciais pelo Admin Central (nova senha gerada)
- URLs parametrizadas (dev/prod)
- Suporte SMTP com e sem autenticação

**Stack Técnica**:
- Backend: MailKit + BCrypt + RandomNumberGenerator
- Frontend: Modal de confirmação + Toasts
- Dev: smtp4dev (Docker)
- Prod: SMTP configurável

**Estimativa**: ~23 pontos (3-4 dias)

**Documentos**:
- 📋 [README](./prd-onboarding-admin-barbearia/README.md) - Visão geral e navegação
- 📝 [PRD](./prd-onboarding-admin-barbearia/prd.md) - Requisitos de produto
- 🔧 [Tech Spec](./prd-onboarding-admin-barbearia/techspec.md) - Especificação técnica
- ✅ [Tasks](./prd-onboarding-admin-barbearia/tasks.md) - Breakdown de implementação
- ⚙️ [Config Guide](./prd-onboarding-admin-barbearia/CONFIG.md) - Setup e configuração

**Dependências**: PRD 1 (Gestão de Barbearias)

---

## Ordem de Implementação Sugerida

1. **PRD 5 - Sistema Multi-tenant** (base arquitetural)
2. **PRD 1 - Gestão de Barbearias** (criar barbearias no sistema)
3. **PRD 2 - Gestão de Barbeiros** (montar equipes)
4. **PRD 4 - Cliente (Cadastro)** + **PRD 3 - Barbeiro (Visualização)** (paralelamente)
5. **PRD 4 - Cliente (Agendamento)** (criar agendamentos)
6. **PRD 3 - Barbeiro (Confirmação/Cancelamento)** (gerenciar agendamentos)

## Atores do Sistema

### Admin Central
- Gerencia barbearias (CRUD)
- Não interage com barbeiros, clientes ou agendamentos
- Acesso cross-tenant (vê todas as barbearias)

### Admin da Barbearia
- Gerencia barbeiros da sua barbearia
- Visualiza agenda completa da equipe
- Não pode confirmar/cancelar agendamentos (apenas visualizar)

### Barbeiro
- Gerencia suas agendas em cada barbearia onde trabalha
- Confirma/cancela/conclui seus próprios agendamentos
- Pode trabalhar em múltiplas barbearias (agendas isoladas)

### Cliente
- Agenda serviços em barbearias
- Visualiza e cancela seus agendamentos
- Pode estar cadastrado em múltiplas barbearias (dados isolados)

## Princípios Fundamentais do MVP

### Isolamento Multi-tenant
- **100% de segregação** de dados entre barbearias
- Mesmo telefone pode existir em múltiplas barbearias de forma independente
- Zero vazamento de dados entre barbearias
- Contexto da barbearia sempre presente em todas as operações

### Simplicidade no MVP
- **Sem validação por SMS**: Login por telefone + nome apenas (Fase 2)
- **Sem notificações push**: Alertas ficam para Fase 2
- **Sem regras complexas**: Horários, conflitos e validações avançadas ficam para versões futuras
- **Sem pagamentos**: Apenas agendamentos no MVP
- **Sem relatórios**: Analytics ficam para Fase 3

### Mobile-First
- Interface otimizada primariamente para smartphones
- Responsiva para tablet e desktop
- Web (não native) para facilitar desenvolvimento

## Questões em Aberto Comuns

As seguintes questões aparecem em múltiplos PRDs e precisam ser decididas:

### Negócio
1. Política de cancelamento (tempo mínimo, penalidades)
2. Tratamento de agendamentos futuros ao remover barbeiro
3. Confirmação automática vs manual de agendamentos
4. Limite de agendamentos simultâneos por cliente

### Técnica
5. Autenticação inicial de barbeiro/admin barbearia (primeiro acesso)
6. Atualização em tempo real (WebSockets vs polling)
7. Soft delete vs hard delete de dados
8. Algoritmo JWT (HS256 vs RS256)

### UX
9. Visualização de agenda (calendário vs lista)
10. Onboarding de novos usuários
11. Notificação de novos agendamentos sem push
12. Compartilhamento do código da barbearia

## Próximos Passos

1. **Revisão dos PRDs**: Stakeholders revisam e respondem questões em aberto
2. **Priorização**: Definir ordem exata de implementação
3. **Tech Specs**: Criar especificações técnicas detalhadas baseadas nos PRDs aprovados
4. **Design**: Criar wireframes e protótipos de UI/UX
5. **Implementação**: Desenvolvimento iterativo começando pelo PRD 5

## Contato

Para questões sobre estes PRDs, entre em contato com a equipe de produto.

---

**Última Atualização**: 2025-10-10
