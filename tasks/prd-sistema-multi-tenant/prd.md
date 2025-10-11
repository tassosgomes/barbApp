# PRD - Sistema Multi-tenant e Autenticação

## Visão Geral

O Sistema Multi-tenant e Autenticação é a fundação arquitetural do barbApp que permite que múltiplas barbearias operem de forma completamente isolada dentro da mesma aplicação SaaS. Este sistema garante que dados, usuários e operações de cada barbearia sejam totalmente segregados, enquanto fornece autenticação simples e contexto apropriado para cada tipo de usuário (Admin Central, Admin da Barbearia, Barbeiro e Cliente). É a espinha dorsal que habilita o modelo de negócio multi-tenant do barbApp.

## Objetivos

- **Objetivo Principal**: Garantir isolamento total de dados entre barbearias em uma arquitetura multi-tenant, com autenticação apropriada para cada perfil de usuário
- **Métricas de Sucesso**:
  - 100% de isolamento de dados entre barbearias (zero vazamento)
  - Taxa de sucesso de login > 95%
  - Tempo de resposta de autenticação < 1 segundo
  - Zero incidentes de acesso não autorizado entre barbearias
- **Objetivos de Negócio**:
  - Viabilizar modelo SaaS escalável com múltiplas barbearias
  - Garantir privacidade e segurança dos dados de cada estabelecimento
  - Simplificar acesso para usuários finais
  - Permitir que profissionais trabalhem em múltiplas barbearias sem conflitos

## Histórias de Usuário

### Persona: Sistema / Infraestrutura
Este PRD é mais técnico/arquitetural, mas tem impacto direto em todos os usuários.

**Histórias de Isolamento:**

- Como Desenvolvedor do Sistema, eu quero **garantir que cada barbearia veja apenas seus próprios dados** para que não haja vazamento de informações entre clientes
- Como Desenvolvedor do Sistema, eu quero **identificar automaticamente o contexto da barbearia** em cada requisição para aplicar filtros corretos
- Como Desenvolvedor do Sistema, eu quero **permitir que um mesmo usuário (barbeiro/cliente) exista em múltiplas barbearias** de forma independente para flexibilizar modelo de trabalho

**Histórias de Autenticação:**

- Como Admin Central, eu quero **fazer login com credenciais de administrador** para acessar painel de gestão de todas as barbearias
- Como Admin da Barbearia, eu quero **fazer login no contexto da minha barbearia específica** para gerenciar apenas meu estabelecimento
- Como Barbeiro, eu quero **fazer login com telefone** e acessar agendas isoladas de cada barbearia onde trabalho
- Como Cliente, eu quero **fazer login simples com telefone e nome** na barbearia específica sem complexidade de senha no MVP

**Histórias de Contexto:**

- Como qualquer Usuário, eu quero que o sistema **mantenha o contexto da barbearia durante minha sessão** para que eu não precise informar repetidamente
- Como Barbeiro multi-vinculado, eu quero **trocar facilmente entre barbearias** onde trabalho para acessar diferentes agendas
- Como Cliente multi-cadastrado, eu quero **acessar cada barbearia independentemente** sem misturar meus dados

## Funcionalidades Principais

### 1. Identificação de Contexto por Código/URL

**O que faz**: Sistema identifica qual barbearia o usuário está acessando através do código na URL.

**Por que é importante**: É o mecanismo fundamental que estabelece o contexto multi-tenant e garante isolamento.

**Como funciona**:
- URL contém código da barbearia (ex: barbapp.com/XYZ123AB)
- Sistema valida código e identifica barbearia
- Contexto da barbearia é estabelecido para toda a sessão
- Todas as operações subsequentes são filtradas por essa barbearia

**Requisitos Funcionais:**

1.1. O sistema deve aceitar código da barbearia via URL path (ex: `/barbearia/{codigo}`) ou query string (ex: `?codigo={codigo}`)

1.2. O sistema deve validar código:
   - Verifica se código existe no banco de dados
   - Verifica se barbearia está ativa
   - Retorna erro 404 se código inválido ou barbearia inativa

1.3. O sistema deve armazenar contexto da barbearia na sessão do usuário após validação

1.4. O contexto deve incluir:
   - ID da barbearia
   - Nome da barbearia
   - Código único
   - Status

1.5. Todas as requisições subsequentes devem incluir contexto da barbearia automaticamente (via token JWT ou sessão)

1.6. O sistema deve validar contexto em TODAS as requisições de API para garantir isolamento

1.7. Código deve ser case-insensitive para facilitar uso

1.8. Mudança de contexto (acessar outra barbearia) requer nova URL/código

### 2. Isolamento de Dados Multi-tenant

**O que faz**: Garante que queries ao banco de dados sempre filtrem por barbearia, impedindo vazamento de dados.

**Por que é importante**: É a garantia de privacidade e segurança no modelo SaaS multi-tenant.

**Como funciona**:
- Cada query inclui filtro por barbearia automaticamente
- Backend valida que usuário tem acesso àquela barbearia
- Dados de diferentes barbearias nunca são misturados
- Arquitetura garante isolamento em nível de código

**Requisitos Funcionais:**

2.1. TODAS as queries ao banco de dados devem incluir filtro por `barbeariaId` (exceto Admin Central que vê todas)

2.2. O sistema deve implementar filtro automático em nível de ORM/framework para evitar esquecimento

2.3. Modelos de dados devem incluir `BarbeariaId` como chave estrangeira obrigatória:
   - Barbeiros
   - Clientes
   - Agendamentos
   - Serviços
   - Configurações

2.4. O sistema deve validar em cada requisição de API:
   - Token/sessão contém barbearia
   - Usuário tem autorização para aquela barbearia
   - Recursos solicitados pertencem àquela barbearia

2.5. Tentativa de acesso a dados de outra barbearia deve retornar 403 Forbidden

2.6. Admin Central é exceção: tem acesso cross-tenant (todas as barbearias)

2.7. Logs de auditoria devem registrar sempre o contexto da barbearia

### 3. Autenticação Multi-perfil

**O que faz**: Implementa diferentes métodos de autenticação para cada tipo de usuário.

**Por que é importante**: Cada perfil tem necessidades diferentes de autenticação.

**Como funciona**:
- Admin Central: Email + Senha (autenticação tradicional)
- Admin Barbearia: Código da barbearia + Email + Senha
- Barbeiro: Código da barbearia + Telefone (sem senha no MVP)
- Cliente: Código da barbearia + Telefone + Nome (sem senha no MVP)

**Requisitos Funcionais:**

3.1. **Autenticação Admin Central**:
   - Endpoint: `POST /api/auth/admin-central/login`
   - Campos: email, senha
   - Retorna: Token JWT com role "AdminCentral"
   - Não requer contexto de barbearia

3.2. **Autenticação Admin da Barbearia**:
   - Endpoint: `POST /api/auth/admin-barbearia/login`
   - Campos: código da barbearia, email, senha
   - Valida código da barbearia
   - Valida que admin pertence àquela barbearia
   - Retorna: Token JWT com role "AdminBarbearia" + barbeariaId

3.3. **Autenticação Barbeiro**:
   - Endpoint: `POST /api/auth/barbeiro/login`
   - Campos: código da barbearia, telefone
   - Valida código da barbearia
   - Valida que telefone está cadastrado como barbeiro naquela barbearia
   - Retorna: Token JWT com role "Barbeiro" + barbeiroId + barbeariaId

3.4. **Autenticação Cliente**:
   - Endpoint: `POST /api/auth/cliente/login`
   - Campos: código da barbearia, telefone, nome
   - Valida código da barbearia
   - Valida que telefone + nome correspondem a cliente cadastrado naquela barbearia
   - Match de nome case-insensitive
   - Retorna: Token JWT com role "Cliente" + clienteId + barbeariaId

3.5. Todos os tokens JWT devem incluir:
   - ID do usuário
   - Role (AdminCentral, AdminBarbearia, Barbeiro, Cliente)
   - ID da barbearia (exceto AdminCentral)
   - Data de expiração (24h para MVP)

3.6. Sistema deve validar token em todas as requisições protegidas

3.7. Token expirado deve retornar 401 Unauthorized

### 4. Gerenciamento de Sessão e Contexto

**O que faz**: Mantém estado de autenticação e contexto da barbearia durante uso da aplicação.

**Por que é importante**: Usuário não deve precisar fazer login repetidamente ou informar barbearia em cada ação.

**Como funciona**:
- Token JWT armazenado no frontend (localStorage ou cookie)
- Contexto da barbearia incluído no token
- Frontend inclui token em todas as requisições
- Backend valida token e extrai contexto

**Requisitos Funcionais:**

4.1. Token JWT deve ser armazenado no frontend de forma segura

4.2. Frontend deve incluir token em header de todas as requisições: `Authorization: Bearer {token}`

4.3. Backend deve validar token e extrair contexto automaticamente

4.4. Contexto extraído deve estar disponível em todos os controllers/services

4.5. Se token inválido ou expirado, retornar 401 e redirecionar para login

4.6. Logout deve invalidar token (implementar blacklist ou token de curta duração)

4.7. Sessão deve persistir até logout explícito ou expiração (24h)

### 5. Troca de Contexto (Barbeiros Multi-vinculados)

**O que faz**: Permite que barbeiros que trabalham em múltiplas barbearias troquem de contexto facilmente.

**Por que é importante**: Barbeiro precisa acessar diferentes agendas sem fazer múltiplos logins.

**Como funciona**:
- Barbeiro faz login inicial
- Sistema identifica todas as barbearias onde está vinculado
- Barbeiro seleciona barbearia
- Sistema gera novo token com contexto daquela barbearia
- Barbeiro pode trocar de contexto via seletor

**Requisitos Funcionais:**

5.1. Após login inicial, sistema deve consultar todas as barbearias onde barbeiro está vinculado

5.2. Se barbeiro está em múltiplas barbearias, exibir tela de seleção

5.3. Se barbeiro está em apenas uma barbearia, ir direto para agenda

5.4. Endpoint para listar barbearias do barbeiro: `GET /api/barbeiro/minhas-barbearias`

5.5. Endpoint para trocar contexto: `POST /api/barbeiro/trocar-contexto` (body: barbeariaId)

5.6. Ao trocar contexto, sistema deve:
   - Validar que barbeiro tem acesso àquela barbearia
   - Gerar novo token JWT com novo contexto
   - Retornar novo token para frontend

5.7. Frontend deve atualizar token armazenado

5.8. Interface deve sempre mostrar qual barbearia está ativa (header/navbar)

5.9. Seletor de contexto deve estar sempre acessível para troca rápida

### 6. Autorização por Perfil

**O que faz**: Controla o que cada tipo de usuário pode fazer no sistema.

**Por que é importante**: Garantir que usuários só acessem recursos permitidos para seu perfil.

**Como funciona**:
- Sistema valida role do token em cada requisição
- Endpoints têm restrições por role
- Ações são autorizadas ou negadas conforme perfil

**Requisitos Funcionais:**

6.1. **Admin Central** pode:
   - CRUD de barbearias
   - Ver todas as barbearias
   - Não acessa dados de barbeiros, clientes ou agendamentos

6.2. **Admin da Barbearia** pode (apenas da sua barbearia):
   - CRUD de barbeiros
   - Visualizar agenda completa
   - Visualizar agendamentos
   - Não pode confirmar/cancelar agendamentos (apenas visualizar)

6.3. **Barbeiro** pode (apenas da barbearia atual no contexto):
   - Visualizar sua própria agenda
   - Confirmar/cancelar/concluir seus agendamentos
   - Não pode adicionar/remover outros barbeiros
   - Não pode criar agendamentos

6.4. **Cliente** pode (apenas da barbearia atual no contexto):
   - Visualizar barbeiros e horários disponíveis
   - Criar agendamentos
   - Visualizar e cancelar seus próprios agendamentos
   - Visualizar seu próprio histórico
   - Não pode ver agendamentos de outros clientes

6.5. Tentativa de acesso não autorizado deve retornar 403 Forbidden

6.6. Sistema deve validar autorização em nível de API (backend) não apenas frontend

### 7. Cadastro Multi-vinculado (Barbeiro e Cliente)

**O que faz**: Permite que um mesmo telefone (barbeiro ou cliente) esteja cadastrado em múltiplas barbearias de forma independente.

**Por que é importante**: Barbeiros podem trabalhar em vários lugares, clientes podem frequentar várias barbearias.

**Como funciona**:
- Chave única é telefone + barbeariaId (não apenas telefone)
- Mesmo telefone pode ter registros diferentes em cada barbearia
- Dados são completamente isolados

**Requisitos Funcionais:**

7.1. Tabela de Barbeiros deve ter constraint UNIQUE em (telefone, barbeariaId)

7.2. Tabela de Clientes deve ter constraint UNIQUE em (telefone, barbeariaId)

7.3. Sistema deve permitir cadastro do mesmo telefone em múltiplas barbearias

7.4. Ao fazer login, sistema deve validar telefone no contexto da barbearia específica

7.5. Dados de barbeiro/cliente são isolados por barbearia (podem ter nomes diferentes, etc.)

7.6. Não há vínculo ou sincronização entre registros do mesmo telefone em barbearias diferentes

## Experiência do Usuário

### Fluxos de Autenticação

**Fluxo 1: Cliente - Primeiro Acesso**
1. Cliente recebe link: barbapp.com/XYZ123AB
2. Sistema valida código XYZ123AB
3. Sistema exibe tela de boas-vindas da "Barbearia XYZ"
4. Cliente clica "Quero me cadastrar"
5. Cliente preenche telefone e nome
6. Sistema cria cliente no contexto da barbearia XYZ
7. Sistema gera token JWT com contexto
8. Cliente é logado automaticamente

**Fluxo 2: Cliente - Login Recorrente**
1. Cliente acessa barbapp.com/XYZ123AB
2. Sistema identifica que telefone já está cadastrado
3. Cliente informa telefone e nome
4. Sistema valida no contexto da barbearia XYZ
5. Sistema gera token JWT
6. Cliente é logado

**Fluxo 3: Barbeiro - Login Multi-barbearia**
1. Barbeiro acessa barbapp.com/barbeiro (ou URL geral)
2. Sistema solicita telefone
3. Barbeiro informa telefone
4. Sistema consulta todas as barbearias onde telefone está cadastrado
5. Sistema exibe: "Você trabalha em 3 barbearias"
6. Barbeiro seleciona "Barbearia ABC"
7. Sistema gera token com contexto da Barbearia ABC
8. Barbeiro acessa agenda

**Fluxo 4: Barbeiro - Trocar de Contexto**
1. Barbeiro está na agenda da Barbearia ABC
2. Clica no seletor de contexto (dropdown no header)
3. Seleciona "Barbearia XYZ"
4. Sistema valida acesso e gera novo token
5. Página recarrega com dados da Barbearia XYZ
6. Header atualiza: "Barbearia XYZ"

**Fluxo 5: Admin da Barbearia - Login**
1. Admin acessa barbapp.com/admin-barbearia/XYZ123AB
2. Sistema solicita email e senha
3. Admin preenche credenciais
4. Sistema valida no contexto da barbearia
5. Sistema gera token com role AdminBarbearia
6. Admin acessa painel de gestão

**Fluxo 6: Admin Central - Login**
1. Admin Central acessa barbapp.com/admin-central
2. Sistema solicita email e senha
3. Admin preenche credenciais globais
4. Sistema valida (sem contexto de barbearia)
5. Sistema gera token com role AdminCentral
6. Admin acessa painel de gestão de todas as barbearias

### Requisitos de UX

- Feedback claro sobre qual barbearia está sendo acessada (sempre visível)
- Login simples sem complexidade desnecessária
- Seletor de contexto intuitivo para barbeiros multi-vinculados
- Mensagens de erro claras quando código inválido ou credenciais erradas
- Logout acessível em todas as páginas
- Persistência de sessão para evitar logins repetitivos

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (relacional)
- **Autenticação**: JWT (JSON Web Tokens)

### Arquitetura Multi-tenant

**Modelo**: Shared Database, Shared Schema com Tenant Discriminator
- Uma única instância de banco de dados
- Mesmas tabelas para todas as barbearias
- Coluna `BarbeariaId` em todas as tabelas relevantes
- Filtros automáticos por `BarbeariaId` em todas as queries

**Alternativas Descartadas**:
- Database por tenant: Complexidade operacional alta
- Schema por tenant: Migração e manutenção complexas

### Segurança

- **JWT**: Algoritmo HS256 ou RS256 (definir na Tech Spec)
- **Secret Key**: Armazenada em variável de ambiente (nunca em código)
- **Token Expiration**: 24 horas para MVP
- **HTTPS**: Obrigatório em produção
- **CORS**: Configurado para aceitar apenas origens confiáveis
- **SQL Injection**: Proteção via ORM parametrizado
- **XSS**: Sanitização de inputs
- **CSRF**: Proteção via tokens (se usar cookies)

### Performance

- Validação de token deve ser < 100ms
- Queries com filtro de barbearia devem ter índices apropriados
- Cache de informações da barbearia para evitar queries repetidas

### Conformidade

- **LGPD**: Dados pessoais (telefone, nome) devem ser protegidos
- **Auditoria**: Logs de acesso devem registrar contexto da barbearia
- **Isolamento**: Garantia de zero vazamento de dados entre tenants

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Validação por SMS**: Código de verificação por telefone fica para Fase 2
- **Autenticação Social**: Login com Google/Facebook fica para versão futura
- **Senha para Cliente/Barbeiro**: MVP usa apenas telefone/nome
- **Autenticação Multi-fator (MFA)**: Segurança adicional fica para Fase 3
- **Single Sign-On (SSO)**: Integração com sistemas externos fica fora do MVP
- **Permissões Granulares**: MVP tem perfis fixos sem customização
- **Auditoria Avançada**: Logs detalhados de todas as ações ficam para Fase 3
- **Rate Limiting Avançado**: Proteção básica contra brute force fica para Fase 2
- **Gestão de Sessões Concorrentes**: Permitir apenas uma sessão ativa fica para versão futura
- **Database por Tenant**: Não é o modelo escolhido
- **IP Whitelisting**: Restrição por IP fica para versão enterprise futura
- **Delegação de Acesso**: Permitir que Admin dê acesso temporário fica fora do MVP

### Considerações Futuras (Pós-MVP)

- Validação por SMS (Fase 2)
- MFA para Admin Central e Admin Barbearia (Fase 3)
- Auditoria detalhada de acessos (Fase 3)
- Autenticação social
- Permissões customizadas por barbearia
- Rate limiting robusto
- Gestão avançada de sessões

## Questões em Aberto

### Questões de Arquitetura

1. **Algoritmo JWT**: Usar HS256 (symmetric) ou RS256 (asymmetric)? HS256 é mais simples para MVP.

2. **Storage de Token**: LocalStorage ou Cookie HttpOnly? Cookie é mais seguro mas complexifica mobile.

3. **Refresh Token**: Implementar refresh token para renovação ou apenas expiração fixa de 24h?

4. **Token Blacklist**: Como invalidar tokens após logout? Blacklist em Redis ou apenas expiração natural?

5. **Isolamento de Serviços**: Backend API único ou microsserviços por domínio? (Sugestão: monolito modular para MVP)

### Questões de Segurança

6. **Primeiro Acesso Barbeiro**: Como barbeiro cria senha/acessa pela primeira vez após ser adicionado? Admin da Barbearia envia link?

7. **Primeiro Acesso Admin Barbearia**: Como Admin da Barbearia cria conta após barbearia ser criada? Admin Central envia credenciais?

8. **Validação de Telefone**: No MVP sem SMS, como garantir que telefone é válido? Apenas formato?

9. **Brute Force Protection**: Quantas tentativas de login antes de bloquear? 5 tentativas em 15 minutos?

10. **Rate Limiting**: Limitar requisições por IP/usuário? Quantas por minuto?

### Questões de Dados

11. **Migração entre Barbearias**: Deve ser possível "transferir" um barbeiro/cliente entre barbearias ou sempre são registros independentes?

12. **Exclusão de Dados**: Quando barbearia é excluída, soft delete ou hard delete? LGPD requer exclusão definitiva?

13. **Telefone Internacional**: Suportar telefones internacionais ou apenas Brasil (+55)?

14. **Normalização de Telefone**: Como armazenar? Com ou sem formatação? (Sugestão: apenas números)

### Questões de UX

15. **Seletor de Contexto para Cliente**: Cliente multi-cadastrado deve ter seletor ou sempre acessa via código específico?

16. **Logout**: Logout deve ser global (todas as barbearias) ou apenas da barbearia atual?

17. **Sessão Expirada**: Como notificar usuário quando sessão expira? Redirect para login com mensagem?

18. **Primeiro Acesso**: Precisa de onboarding/tutorial para Admin Barbearia e Barbeiro?

### Questões de Performance

19. **Cache de Barbearia**: Informações da barbearia (nome, código) devem ser cacheadas no token ou consultadas em cada requisição?

20. **Índices**: Quais índices criar no banco? (Sugestão: telefone + barbeariaId, código da barbearia)

---

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunho para Revisão
