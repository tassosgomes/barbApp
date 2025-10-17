# Task 15 - Onboarding Automático do Admin da Barbearia

## 📋 Visão Geral

Automatizar a criação de credenciais de acesso para o Admin da Barbearia no momento do cadastro da barbearia pelo Admin Central, enviando as credenciais por e-mail e permitindo acesso imediato ao sistema.

## 📂 Estrutura de Documentos

```
prd-onboarding-admin-barbearia/
├── README.md           # Este arquivo (visão geral e navegação)
├── prd.md             # Product Requirements Document (o quê e por quê)
├── techspec.md        # Especificação Técnica (como implementar)
├── tasks.md           # Breakdown de tarefas (15 tasks detalhadas)
├── CONFIG.md          # Guia rápido de configuração (setup dev/prod)
├── SNIPPETS.md        # Exemplos de código prontos (copy & paste)
├── DIAGRAMS.md        # Diagramas de fluxo visuais (ASCII art)
└── FAQ.md             # Perguntas frequentes e troubleshooting
```

## 🎯 Objetivos

### Para o Usuário
- ✅ Admin Central cadastra barbearia sem precisar criar manualmente o usuário
- ✅ Admin Barbearia recebe credenciais de acesso imediatamente por e-mail
- ✅ Admin Barbearia consegue fazer login em < 2 minutos após cadastro
- ✅ Admin Central pode reenviar credenciais em caso de perda

### Para o Negócio
- ✅ Automatizar completamente o onboarding de Admin Barbearia
- ✅ Reduzir suporte relacionado a credenciais de acesso
- ✅ Garantir que toda barbearia cadastrada tenha acesso funcional ao sistema

## 🚀 Funcionalidades

### 1. Criação Automática de Admin Barbearia
Ao cadastrar barbearia, o sistema:
- Cria automaticamente usuário com role `AdminBarbearia`
- Gera senha aleatória segura (12 caracteres)
- Vincula usuário à barbearia
- Envia e-mail com credenciais

### 2. E-mail com Credenciais
Template HTML com:
- Nome da barbearia
- E-mail de acesso
- Senha gerada
- Link direto para login (parametrizado dev/prod)

### 3. Reenvio de Credenciais
- Botão na listagem de barbearias (Admin Central)
- Gera nova senha
- Envia novo e-mail

## 🔧 Stack Técnica

### Backend (.NET)
- ✅ Gerador de senha seguro (`RandomNumberGenerator`)
- ✅ Serviço SMTP com MailKit
- ✅ Retry automático (3 tentativas, backoff exponencial)
- ✅ Transação atômica (rollback se e-mail falhar)
- ✅ URLs parametrizadas via `AppSettings`

### Frontend (React + TypeScript)
- ✅ Modal de confirmação para reenvio
- ✅ Toasts de feedback
- ✅ Mensagem de sucesso atualizada no cadastro

### Infraestrutura
- ✅ **Dev**: smtp4dev (Docker, porta 2525)
- ✅ **Prod**: SMTP configurável (com/sem autenticação)

## 📖 Como Usar Este Repositório

### 1. Entender o Requisito
👉 Leia: **[prd.md](./prd.md)**
- Histórias de usuário
- Funcionalidades detalhadas
- Restrições e não-objetivos

### 2. Entender a Implementação
👉 Leia: **[techspec.md](./techspec.md)**
- Arquitetura completa
- Design de implementação (código C# e TypeScript)
- Endpoints de API
- Análise de impacto
- Abordagem de testes

### 3. Implementar
👉 Leia: **[tasks.md](./tasks.md)**
- 15 tarefas detalhadas (backend + frontend + testes)
- Estimativas (~23 pontos)
- Ordem de implementação sugerida
- Dependências entre tarefas
- Critérios de aceitação

### 4. Configurar Ambiente
👉 Leia: **[CONFIG.md](./CONFIG.md)**
- Setup do smtp4dev (Docker)
- Configuração do backend (`appsettings.json`)
- Dependências NuGet
- Checklist de implementação
- Troubleshooting

### 5. Copiar Código Pronto
👉 Leia: **[SNIPPETS.md](./SNIPPETS.md)**
- Todos os arquivos completos prontos para copiar
- Configurações de DI
- Templates de e-mail
- Testes com cURL
- Checklist de implementação

### 6. Visualizar Fluxos
👉 Leia: **[DIAGRAMS.md](./DIAGRAMS.md)**
- Fluxo completo de cadastro de barbearia
- Fluxo de envio de e-mail (com retry)
- Fluxo de reenvio de credenciais
- Diagrama de geração de senha segura
- Estados do Admin Barbearia
- Modelo de dados e relacionamentos

### 7. Dúvidas e Problemas
👉 Leia: **[FAQ.md](./FAQ.md)**
- Perguntas frequentes sobre segurança, e-mail, transações
- Troubleshooting de problemas comuns
- Configuração de produção
- Roadmap futuro (fora do MVP)
- Referências e boas práticas

## 🏁 Quick Start

### 1. Configurar smtp4dev (Dev)

```bash
docker run -d -p 3000:80 -p 2525:25 rnwood/smtp4dev
```

Acesse: http://localhost:3000

### 2. Configurar Backend

Adicione ao `appsettings.Development.json`:

```json
{
  "AppSettings": {
    "FrontendUrl": "https://dev-barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 2525,
    "UseSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "dev@barbapp.tasso.dev.br",
    "FromName": "BarbApp Dev"
  }
}
```

### 3. Adicionar Dependências

```bash
cd backend/src/BarbApp.Infrastructure
dotnet add package MailKit
dotnet add package MimeKit
```

### 4. Começar Implementação

Siga a ordem em **[tasks.md](./tasks.md)**:
1. Infraestrutura (gerador de senha + serviço de e-mail)
2. Repository (validação de e-mail único)
3. Use Cases (CreateBarbershop + ResendCredentials)
4. API (endpoint de reenvio)
5. Frontend (modal + botão + mensagem)
6. Testes

## 📊 Estimativa

- **Backend**: ~13 pontos (7 tasks)
- **Frontend**: ~5 pontos (4 tasks)
- **Testes**: ~5 pontos (4 tasks)
- **Total**: ~23 pontos (3-4 dias)

## ✅ Critérios de Aceitação

### Backend
- [ ] Admin Barbearia é criado automaticamente ao cadastrar barbearia
- [ ] E-mail com credenciais é enviado com sucesso
- [ ] Transação é revertida se e-mail falhar
- [ ] Endpoint de reenvio funciona corretamente
- [ ] Nova senha é gerada e enviada ao reenviar
- [ ] Logs registram eventos críticos
- [ ] Testes unitários e integração passam (cobertura > 80%)

### Frontend
- [ ] Mensagem de sucesso informa sobre envio de credenciais
- [ ] Botão "Reenviar Credenciais" aparece na listagem
- [ ] Modal de confirmação exibe informações corretas
- [ ] Toast de sucesso/erro é exibido após reenvio
- [ ] Testes de integração passam

### E2E
- [ ] Fluxo de cadastro cria Admin Barbearia e envia e-mail
- [ ] Fluxo de reenvio gera nova senha e envia e-mail
- [ ] E-mails são recebidos corretamente (smtp4dev)

## 🎨 Fluxo Visual

```
┌─────────────────────┐
│   Admin Central     │
│   (Frontend)        │
└──────────┬──────────┘
           │
           │ POST /api/barbershops
           │ { name, email, ... }
           ▼
┌─────────────────────────────────────┐
│   Backend (.NET)                    │
│                                     │
│  1. Validar dados                   │
│  2. Iniciar transação               │
│  3. Criar Barbearia                 │
│  4. Gerar senha aleatória           │
│  5. Criar Admin Barbearia           │
│  6. Enviar e-mail (retry 3x)        │
│  7. Commit (ou rollback se falhar)  │
└──────────┬──────────────────────────┘
           │
           │ SMTP
           ▼
┌─────────────────────┐
│   smtp4dev          │
│   (Dev)             │
│                     │
│   http://localhost:3000
└─────────────────────┘
           │
           │ E-mail com credenciais
           ▼
┌─────────────────────┐
│  Admin Barbearia    │
│  (Recebe e-mail)    │
│                     │
│  email: xxx@xxx.com │
│  senha: ABC123...   │
└─────────────────────┘
```

## 📝 Decisões Técnicas Importantes

1. ✅ **Validação de e-mail único**: Na aplicação (não no banco)
2. ✅ **SMTP**: Suporte com e sem autenticação
3. ✅ **URLs**: Parametrizadas via `AppSettings.FrontendUrl`
4. ✅ **Transação**: Rollback se e-mail falhar
5. ✅ **Retry**: 3 tentativas com backoff (1s, 2s, 4s)

## 🔗 Links Úteis

- **smtp4dev**: https://github.com/rnwood/smtp4dev
- **MailKit**: https://github.com/jstedfast/MailKit
- **BCrypt.Net**: https://github.com/BcryptNet/bcrypt.net

## 🐛 Troubleshooting

Ver seção completa em **[CONFIG.md](./CONFIG.md#-troubleshooting)**

---

**Data de Criação**: 2025-10-16  
**Versão**: 1.0 (MVP)  
**Status**: ✅ Pronto para Implementação  
**Responsável**: Time de Desenvolvimento
