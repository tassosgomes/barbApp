# PRD - Interface de Login e Autenticação (Barbeiro)

## Visão Geral

A Interface de Login do Barbeiro é a porta de entrada para profissionais acessarem o sistema de agendamentos. Esta interface deve ser simples, mobile-first e permitir autenticação usando e-mail e senha. O sistema já possui o backend de autenticação implementado (PRD Multi-tenant), precisando apenas da implementação da interface frontend.

## Objetivos

- **Objetivo Principal**: Permitir que barbeiros façam login de forma rápida e segura para acessar suas agendas
- **Métricas de Sucesso**:
  - Tempo de login < 10 segundos
  - Taxa de erro de autenticação < 5%
  - 100% dos barbeiros conseguem fazer primeiro acesso sem suporte
- **Objetivos de Negócio**:
  - Facilitar onboarding de novos barbeiros
  - Proporcionar experiência mobile-first
  - Garantir segurança no acesso

## Histórias de Usuário

### Persona: Barbeiro
Profissional que presta serviços de barbearia e precisa acessar sua agenda de trabalho.

**Histórias Principais:**

- Como Barbeiro, eu quero **fazer login com meu e-mail e senha** para que eu possa acessar minha agenda de forma rápida e segura
- Como Barbeiro, eu quero **ver validação em tempo real dos campos** para que eu saiba imediatamente se há erros no que digitei
- Como Barbeiro, eu quero **ver mensagens de erro claras** quando o login falha para que eu entenda o que preciso corrigir
- Como Barbeiro, eu quero **que o sistema lembre minha sessão** (dentro do prazo de expiração) para não precisar autenticar repetidamente

**Casos de Uso Secundários:**

- Como Barbeiro, eu quero **ver um indicador de carregamento** durante o login para saber que o sistema está processando
- Como Barbeiro, eu quero **ter campos otimizados para mobile** (teclado apropriado para e-mail; mostrar/ocultar senha) para facilitar a digitação
- Como Barbeiro, eu quero **fazer logout facilmente** quando termino meu trabalho

## Funcionalidades Principais

### 1. Tela de Login

**O que faz**: Permite que barbeiro insira credenciais e faça autenticação no sistema.

**Por que é importante**: É o ponto de entrada obrigatório para acessar o sistema de agendamentos.

**Como funciona**:
- Barbeiro acessa aplicação
- Vê tela de login com campos de e-mail e senha
- Preenche os campos
- Clica em "Entrar"
- Sistema valida e redireciona para agenda (ou seleção de barbearia se multi-vínculo)

**Requisitos Funcionais:**

1.1. A tela deve ter campo "E-mail" (teclado apropriado; validação de formato)

1.2. A tela deve ter campo "Senha" com opção mostrar/ocultar

1.3. Botão "Entrar" deve estar desabilitado até todos os campos serem preenchidos corretamente

1.4. Ao clicar em "Entrar", sistema deve exibir loading e desabilitar formulário

1.5. Sistema deve fazer requisição `POST /api/auth/barbeiro/login` com dados:
```json
{
  "email": "barbeiro@example.com",
  "password": "SenhaSegura123!"
}
```

1.6. Em caso de sucesso (200), armazenar token JWT com segurança

1.7. Após armazenar token, redirecionar para:
   - `/barber/schedule` se barbeiro trabalha em apenas 1 barbearia
   - `/barber/select-barbershop` se barbeiro trabalha em múltiplas

1.8. Em caso de erro (401), exibir mensagem: "E-mail ou senha inválidos. Verifique e tente novamente."

1.9. Em caso de erro (500), exibir mensagem: "Erro ao conectar. Tente novamente em instantes."

1.10. Validações no frontend:
   - E-mail: obrigatório, formato válido
   - Senha: obrigatória

1.11. Exibir mensagens de validação abaixo de cada campo

### 2. Persistência de Sessão

**O que faz**: Mantém barbeiro logado entre sessões e permite acesso direto sem novo login.

**Por que é importante**: Evita que barbeiro precise fazer login toda vez que acessa o sistema.

**Como funciona**:
- Após login bem-sucedido, token é armazenado
- Ao reabrir aplicação, sistema verifica se há token válido
- Se token válido, redireciona direto para agenda
- Se token inválido/expirado, mostra tela de login

**Requisitos Funcionais:**

2.1. Token JWT deve ser armazenado no `localStorage` (ou armazenamento seguro no mobile) com chave `barbapp-barber-token`

2.2. Ao carregar aplicação, verificar se existe token no localStorage

2.3. Se token existe, tentar validá-lo fazendo requisição autenticada (ex: endpoint de perfil `GET /api/auth/me` se disponível; ou via `/api/schedule/my-schedule`)

2.4. Se validação bem-sucedida, redirecionar para última tela acessada ou `/barber/schedule`

2.5. Se validação falha (401), limpar token e mostrar tela de login

2.6. Token deve expirar em 24 horas (conforme backend)

2.7. Requisições com token expirado devem redirecionar automaticamente para login

### 3. Logout

**O que faz**: Permite que barbeiro encerre sessão e saia do sistema.

**Por que é importante**: Segurança quando barbeiro compartilha dispositivo ou termina turno.

**Como funciona**:
- Barbeiro clica em botão/link "Sair" (no header/menu)
- Sistema remove token do localStorage
- Redireciona para tela de login

**Requisitos Funcionais:**

3.1. Deve haver botão "Sair" sempre visível (header ou menu lateral)

3.2. Ao clicar em "Sair", exibir confirmação: "Deseja realmente sair?"

3.3. Se confirmar, remover token do localStorage

3.4. Redirecionar para `/login`

3.5. Limpar qualquer contexto/estado da aplicação

### 4. Validação e Feedback Visual

**O que faz**: Fornece feedback imediato sobre validade dos dados e estado do processo.

**Por que é importante**: Melhora experiência do usuário e reduz erros de digitação.

**Como funciona**:
- Campos são validados em tempo real (on blur ou on change)
- Erros são mostrados abaixo de cada campo
- Loading state é exibido durante autenticação
- Mensagens de erro são claras e acionáveis

**Requisitos Funcionais:**

4.1. Validação deve ocorrer ao sair do campo (onBlur)

4.2. Mensagens de erro devem aparecer abaixo do campo com cor vermelha

4.3. Campos com erro devem ter borda vermelha

4.4. Campos válidos devem ter borda verde (opcional, pode ser apenas neutra)

4.5. Durante loading, exibir spinner no botão "Entrar" e texto "Entrando..."

4.6. Durante loading, desabilitar campos e botão

4.7. Erros de API devem ser exibidos em toast/alert no topo da tela

4.8. Mensagens de erro devem ser específicas:
   - "E-mail inválido. Use um formato válido (ex.: nome@dominio.com)"
   - "Este campo é obrigatório"

### 5. Primeiro Acesso (Onboarding Mínimo)

**O que faz**: Fornece orientações básicas para barbeiros fazendo primeiro login.

**Por que é importante**: Reduz necessidade de suporte e facilita onboarding.

**Como funciona**:
- Tela de login tem texto explicativo
- Link "Precisa de ajuda?" leva para instruções básicas

**Requisitos Funcionais:**

5.1. Abaixo do formulário, exibir texto: "Primeiro acesso? Peça ao administrador da sua barbearia para criar/habilitar seu usuário e definir sua senha."

5.2. Link "Precisa de ajuda?" abre modal ou página com:
   - "Seu usuário é seu e-mail cadastrado"
   - "Sua senha é definida pelo administrador ou via recuperação (pós-MVP)"
   - "Em caso de dúvidas, contate o administrador"

5.3. Modal/página deve ter botão "Entendi" para fechar

## Experiência do Usuário

### Persona: Barbeiro
- **Necessidades**: Login rápido, interface simples, sem complicações
- **Contexto de Uso**: Início do turno, durante o dia (verificar agenda), fim do turno (logout)
- **Nível Técnico**: Básico a intermediário
- **Dispositivos**: Principalmente smartphone

### Fluxo Principal: Login Bem-Sucedido

1. Barbeiro abre aplicação
2. Sistema verifica localStorage - não há token ou está expirado
3. Exibe tela de login
4. Barbeiro digita e-mail e senha
5. Campos são validados e botão "Entrar" fica habilitado
6. Barbeiro clica em "Entrar"
7. Botão mostra loading e fica desabilitado
8. Sistema envia requisição ao backend (`POST /api/auth/barbeiro/login`)
9. Backend valida e retorna token JWT
10. Frontend armazena token no localStorage
11. Sistema redireciona para `/barber/schedule` (ou seleção de barbearia)
12. Barbeiro vê sua agenda do dia

### Fluxo Alternativo: Erro de Autenticação

1. Barbeiro digita e-mail ou senha incorretos
2. Clica em "Entrar"
3. Sistema tenta autenticar
4. Backend retorna 401 Unauthorized
5. Frontend exibe toast vermelho: "E-mail ou senha inválidos. Verifique e tente novamente."
6. Campos permanecem preenchidos (não limpar)
7. Barbeiro corrige dados e tenta novamente

### Fluxo: Retorno à Aplicação (Sessão Válida)

1. Barbeiro abre aplicação (já havia feito login antes)
2. Sistema verifica localStorage - token existe
3. Sistema valida token com backend
4. Token válido - redireciona direto para `/barber/schedule`
5. Barbeiro vê agenda sem precisar fazer login novamente

### Fluxo: Logout

1. Barbeiro está na tela de agenda
2. Clica em "Sair" no header
3. Sistema exibe confirmação: "Deseja realmente sair?"
4. Barbeiro confirma
5. Sistema remove token do localStorage
6. Redireciona para `/login`

### Requisitos de UI/UX

- **Mobile-First**: Interface otimizada para smartphone (principal dispositivo)
- **Design Limpo**: Foco nos campos essenciais, sem distrações
- **Acessibilidade de Campos**:
  - Labels claros e visíveis
  - Placeholders descritivos
  - Mensagens de erro abaixo dos campos
- **Feedback Visual Claro**:
  - Estados de loading bem definidos
  - Erros em vermelho (#dc2626)
  - Sucesso em verde (#16a34a)
- **Tipografia Legível**: Tamanho mínimo 16px para evitar zoom automático no iOS
- **Botão Proeminente**: CTA "Entrar" grande e destacado
- **Cores Consistentes**: Seguir paleta do design system (Shadcn UI)
- **Teclado Mobile**: Otimizado para cada tipo de campo

### Considerações de Acessibilidade

Para o MVP, seguir boas práticas básicas:
- Labels associados aos campos (for/id)
- Mensagens de erro anunciadas por screen readers (aria-live)
- Contraste adequado (mínimo 4.5:1)
- Campos com área de toque adequada (mínimo 44x44px)
- Navegação por teclado funcional (Tab order lógica)

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript
- **Routing**: React Router v6
- **Validação**: Zod + React Hook Form
- **HTTP Client**: Axios (configurado em `api.ts`)
- **UI Components**: Shadcn UI
- **Styling**: Tailwind CSS

### Autenticação
- Login com e-mail e senha
- Token JWT retornado pelo backend
- Token armazenado em localStorage (MVP) ou armazenamento seguro no mobile
- Token incluído em todas as requisições via interceptor Axios

### Integração com Backend
- Endpoint: `POST /api/auth/barbeiro/login`
- Input:
```typescript
{
  email: string;
  password: string;
}
```
- Output (sucesso):
```typescript
{
  token: string;
  user: {
    id: string;
    name: string;
    email: string;
    role: "Barbeiro";
  }
}
```
- Erros:
  - 400: Validação falhou
  - 401: Credenciais inválidas
  - 500: Erro interno

### Performance
- Carregamento inicial da tela < 1 segundo
- Resposta de login < 3 segundos (depende do backend)
- Validação de campos em tempo real sem delay perceptível

### Segurança
- Token JWT com expiração de 24h
- HTTPS obrigatório em produção
- Sanitização de inputs (prevenir XSS)
- Rate limiting no backend (prevenir brute force)
- Não expor detalhes de erro que possam ajudar atacantes

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Recuperação de Senha**: Pós-MVP
- **Cadastro de Novo Barbeiro**: Barbeiro é cadastrado pelo Admin da Barbearia
- **Autenticação por SMS**: Fase 2
- **Login com Biometria**: Face ID, Touch ID ficam para versão futura
- **Múltiplos Métodos de Login**: Apenas e-mail + senha (sem social login)
- **Remember Me**: Sessão persiste por 24h, sem opção de "lembrar"
- **Histórico de Logins**: Não há registro de acessos no MVP
- **Autenticação 2FA**: Fica para Fase 2
- **Modo Offline**: Aplicação requer conexão para login

### Considerações Futuras (Pós-MVP)

- Recuperação de senha (e-mail) e/ou SMS
- Biometria (Face ID/Touch ID)
- Refresh token automático
- Histórico de acessos e dispositivos
- Notificação de login suspeito
- Timeout automático por inatividade

## Questões em Aberto

### Questões de Negócio

1. **Recuperação de Acesso**: Se barbeiro perde acesso (troca telefone, esquece código), qual o processo? Contatar admin ou há self-service?

2. **Múltiplos Dispositivos**: Barbeiro pode estar logado em múltiplos dispositivos simultaneamente?

3. **Timeout de Sessão**: Há inatividade que force logout antes das 24h?

### Questões Técnicas

4. **Armazenamento de Token**: localStorage é adequado ou deve usar sessionStorage/httpOnly cookies para maior segurança?

5. **Refresh Token**: Backend implementa refresh token ou apenas access token de 24h?

6. **Endpoint de Perfil**: Teremos `GET /api/auth/me` para validar sessão e recuperar perfil/claims?

7. **Redirect após Login**: Como detectar multi-vínculo? Usar `GET /api/barbeiro/barbearias` e, se >1, direcionar à seleção?

### Questões de UX

8. **Tela de Splash**: Mostrar logo/splash screen enquanto valida token ao abrir app?

9. **Manter conectado**: Exibir opção ou considerar implícito pela expiração do token?

10. **Deep Links**: Suportar links diretos (ex: abrir agendamento específico) que requerem autenticação?

---

**Data de Criação**: 2025-10-19  
**Versão**: 1.1  
**Status**: Rascunho para Revisão  
**Dependências**: PRD Sistema Multi-tenant (Backend - DONE)
