# Interface de Login e Autenticação (Barbeiro)

## 📋 Visão Geral

Este PRD define a implementação da interface de login para barbeiros no sistema BarbApp. A interface permite que profissionais façam login usando telefone e código da barbearia, consumindo o endpoint de autenticação já implementado no backend.

## 🎯 Status do Projeto

**Status**: ✏️ Planejamento  
**Prioridade**: 🔴 Alta  
**Dependências**: ✅ Backend Multi-tenant (CONCLUÍDO)

## 📚 Documentação

- **PRD**: [prd.md](./prd.md) - Product Requirements Document completo
- **Tech Spec**: [techspec.md](./techspec.md) - Especificação técnica detalhada
- **Tasks**: [tasks.md](./tasks.md) - Resumo e sequenciamento de tarefas

## 🚀 Funcionalidades Principais

### ✨ Features Implementadas
Nenhuma ainda - PRD em fase de planejamento

### 🎯 Features Planejadas

1. **Tela de Login** - Formulário com código da barbearia e telefone
2. **Validação em Tempo Real** - Feedback imediato com Zod
3. **Máscara de Telefone** - Formatação automática durante digitação
4. **Persistência de Sessão** - Token JWT armazenado no localStorage
5. **Proteção de Rotas** - Rotas autenticadas protegidas automaticamente
6. **Loading States** - Indicadores visuais durante processamento
7. **Tratamento de Erros** - Mensagens claras e contextuais
8. **Ajuda para Primeiro Acesso** - Modal com instruções básicas

## 🏗️ Arquitetura

### Stack Tecnológica
- **Framework**: React 18 + Vite
- **Language**: TypeScript
- **Validação**: Zod + React Hook Form
- **Roteamento**: React Router v6
- **HTTP Client**: Axios
- **UI Components**: Shadcn UI
- **Styling**: Tailwind CSS
- **Testes**: Vitest + React Testing Library + Playwright

### Estrutura de Arquivos
```
src/
├── components/
│   └── auth/
│       ├── LoginForm.tsx
│       ├── ProtectedRoute.tsx
│       └── HelpModal.tsx
├── contexts/
│   └── AuthContext.tsx
├── hooks/
│   └── useAuth.ts (exported from context)
├── lib/
│   ├── api.ts (interceptors)
│   └── phone-utils.ts
├── pages/
│   └── auth/
│       └── LoginPage.tsx
├── schemas/
│   └── login.schema.ts
├── services/
│   └── auth.service.ts
└── types/
    └── auth.types.ts
```

## 📝 Tarefas

| # | Tarefa | Status | Estimativa | Complexidade |
|---|--------|--------|------------|--------------|
| 1.0 | Setup: Tipos, Schemas e Utils | ⬜ Pendente | 2h | Baixa |
| 2.0 | Services e Interceptors | ⬜ Pendente | 2h | Média |
| 3.0 | Context e Hooks | ⬜ Pendente | 3h | Média |
| 4.0 | Componentes UI | ⬜ Pendente | 4h | Média |
| 5.0 | Rotas e Proteção | ⬜ Pendente | 2h | Média |
| 6.0 | Testes Completos | ⬜ Pendente | 4h | Alta |
| 7.0 | Refinamento UX | ⬜ Pendente | 2h | Baixa |

**Total**: ~19 horas (2-3 dias úteis)

## 🔗 Integrações

### Backend (Já Implementado)
- ✅ `POST /api/auth/barbeiro/login` - Endpoint de autenticação
- ✅ JWT Token com role "Barbeiro"
- ✅ Validação de telefone formato +55XXXXXXXXXXX
- ✅ Sistema multi-tenant operacional

### Sistemas Relacionados
- 🔄 [Sistema de Agendamentos](../prd-sistema-agendamentos-barbeiro/) - Consumirá autenticação
- ✅ [Sistema Multi-tenant](../prd-sistema-multi-tenant-done/) - Backend base

## 🎨 Design e UX

### Princípios de Design
- **Mobile-First**: Interface otimizada para smartphone
- **Simplicidade**: Foco nos campos essenciais
- **Feedback Claro**: Estados visuais bem definidos
- **Acessibilidade**: Seguir boas práticas WCAG AA

### Fluxos Principais
1. **Login Bem-Sucedido**: Login → Validação → Armazenar token → Redirecionar
2. **Login com Erro**: Tentar login → Erro 401 → Mostrar mensagem → Permitir retry
3. **Sessão Persistente**: Abrir app → Validar token → Redirecionar direto
4. **Logout**: Clicar Sair → Confirmar → Limpar token → Redirecionar login

## 🧪 Estratégia de Testes

### Cobertura Planejada
- **Testes Unitários**: Funções utils, validações, componentes isolados
- **Testes de Integração**: Fluxos completos, interação com API
- **Testes E2E**: Experiência completa do usuário

### Cenários Críticos
- ✅ Login com credenciais válidas
- ✅ Login com credenciais inválidas (401)
- ✅ Validação de campos
- ✅ Máscara de telefone
- ✅ Persistência de sessão
- ✅ Token expirado (redirect automático)
- ✅ Navegação entre rotas protegidas
- ✅ Logout

## 📊 Métricas de Sucesso

### KPIs Técnicos
- Tempo de login < 10 segundos
- Taxa de erro de autenticação < 5%
- Cobertura de testes > 80%
- Performance: First Input Delay < 100ms

### KPIs de Negócio
- 100% dos barbeiros conseguem fazer primeiro acesso sem suporte
- Zero vazamentos de dados entre barbearias (multi-tenant)
- Taxa de adoção de 100% após onboarding

## ⚠️ Riscos e Considerações

### Riscos Técnicos
- **Token no localStorage**: Vulnerável a XSS (mitigação: sanitização, migração futura para httpOnly cookies)
- **Formato de telefone**: Backend deve aceitar +55XXXXXXXXXXX
- **Sessão única**: Token atual não gerencia múltiplos dispositivos inteligentemente

### Considerações Futuras
- Migrar para httpOnly cookies
- Implementar refresh token
- Adicionar autenticação 2FA
- Suporte a biometria (Face ID/Touch ID)
- Validação de telefone por SMS

## 📅 Cronograma

### Sprint 1 - Base (1 dia)
- ✅ PRD e TechSpec criados
- ⬜ Tarefas 1.0, 2.0, 3.0

### Sprint 2 - UI e Integração (1 dia)
- ⬜ Tarefas 4.0, 5.0
- ⬜ Testes básicos (6.0 parcial)

### Sprint 3 - Testes e Refinamento (0.5-1 dia)
- ⬜ Testes completos (6.0)
- ⬜ Refinamento final (7.0)

## 🤝 Contribuindo

Para implementar este PRD:

1. Leia o [PRD](./prd.md) completo
2. Revise a [Tech Spec](./techspec.md)
3. Siga a ordem de tarefas em [tasks.md](./tasks.md)
4. Consulte as tarefas individuais detalhadas (1_task.md - 7_task.md)
5. Siga as regras em `/rules/react.md` e `/rules/tests-react.md`

## 📞 Suporte

Para dúvidas sobre este PRD:
- Revisar documentação técnica completa
- Consultar PRD Multi-tenant (backend)
- Verificar sistema de agendamentos (uso da autenticação)

---

**Criado em**: 2025-10-19  
**Última atualização**: 2025-10-19  
**Versão**: 1.0
