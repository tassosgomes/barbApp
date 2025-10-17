# PRD: Interface Administrativa para Admin de Barbearia

## 1. Contexto e Motivação

Atualmente, o sistema BarbApp possui backend completo para suporte a múltiplos tipos de usuários (Admin Central, Admin Barbearia e Barbeiro), porém o frontend `barbapp-admin` está implementado apenas para **Admin Central**.

**Problema identificado:**
- Usuário Admin de Barbearia não consegue fazer login na aplicação
- Tentativa de login com credenciais válidas de Admin Barbearia (email: `tasso.gomes@outlook.com`) resulta em erro 401
- O endpoint `/api/auth/admin-barbearia/login` existe no backend mas não há interface frontend correspondente

**Impacto:**
- Administradores de barbearias não conseguem acessar o sistema
- Necessidade de criar credenciais de Admin Central para cada dono de barbearia (workaround inseguro)
- Violação do modelo multi-tenant do sistema

## 2. Objetivos

### Objetivo Principal
Criar interface web completa para que administradores de barbearias possam fazer login e gerenciar suas respectivas barbearias.

### Objetivos Específicos
1. Implementar página de login dedicada para Admin de Barbearia
2. Criar rotas e navegação específicas para contexto de Admin Barbearia
3. Adaptar funcionalidades existentes para escopo de uma única barbearia
4. Garantir isolamento de dados (tenant isolation) no frontend

## 3. Escopo

### 3.1. Funcionalidades Incluídas

#### Login e Autenticação
- [ ] Página de login para Admin Barbearia acessível via URL com código da barbearia
  - Formato da URL: `/{codigo}/login` (ex: `/6SJJRFPD/login`)
  - Sistema extrai código da URL automaticamente
- [ ] Formulário de login com campos simplificados:
  - Email
  - Senha
- [ ] Validação do código da barbearia antes de exibir formulário
  - Se código inválido: exibir mensagem de erro "Barbearia não encontrada"
  - Se código válido: exibir formulário com nome da barbearia no cabeçalho
- [ ] Integração com endpoint `/api/auth/admin-barbearia/login`
- [ ] Armazenamento seguro de token e contexto (barbearia_id, codigo)
- [ ] Redirecionamento pós-login para dashboard específico (`/{codigo}/dashboard`)

#### Dashboard e Navegação
- [ ] Dashboard com visão geral da barbearia
- [ ] Menu lateral adaptado ao contexto de Admin Barbearia:
  - Meus Dados / Perfil da Barbearia
  - Barbeiros
  - Serviços
  - Agendamentos
  - (Sem acesso a "Gestão de Barbearias")

#### Gestão de Barbeiros
- [ ] Listagem de barbeiros da barbearia
- [ ] Cadastro de novos barbeiros
- [ ] Edição de dados de barbeiros
- [ ] Ativação/Desativação de barbeiros

#### Gestão de Serviços
- [ ] Listagem de serviços oferecidos
- [ ] Cadastro de novos serviços
- [ ] Edição de serviços (nome, duração, preço)
- [ ] Ativação/Desativação de serviços

#### Visualização de Agendamentos
- [ ] Calendário/lista de agendamentos da equipe
- [ ] Filtros por barbeiro, data, status
- [ ] Detalhes de cada agendamento

### 3.2. Funcionalidades Excluídas (Fora do Escopo)
- ❌ Gestão de outras barbearias (privilégio exclusivo de Admin Central)
- ❌ Criação/Edição de dados da própria barbearia (apenas visualização)
- ❌ Reenvio de credenciais (funcionalidade de Admin Central)
- ❌ Sistema de pagamentos/financeiro
- ❌ Relatórios avançados

### 3.3. Diferenciação: Admin Central vs Admin Barbearia

| Funcionalidade | Admin Central | Admin Barbearia |
|----------------|---------------|-----------------|
| Listar todas as barbearias | ✅ Sim | ❌ Não |
| Criar nova barbearia | ✅ Sim | ❌ Não |
| Editar qualquer barbearia | ✅ Sim | ❌ Não |
| Desativar/Reativar barbearias | ✅ Sim | ❌ Não |
| Ver dados da própria barbearia | N/A | ✅ Sim (somente leitura) |
| Gerenciar barbeiros | ❌ Não | ✅ Sim (da sua barbearia) |
| Gerenciar serviços | ❌ Não | ✅ Sim (da sua barbearia) |
| Ver agendamentos | ❌ Não | ✅ Sim (da sua barbearia) |

## 4. Requisitos Funcionais

### RF01: Login de Admin Barbearia
**Descrição:** Sistema deve permitir login de administradores de barbearia via URL personalizada com email e senha.

**Critérios de Aceitação:**
- CA01: URL deve seguir padrão `/{codigo}/login` onde código tem 8 caracteres alfanuméricos
- CA02: Sistema deve validar código da barbearia ao carregar a página
  - Se código inválido/inexistente: exibir erro "Barbearia não encontrada"
  - Se código válido: exibir formulário com nome da barbearia
- CA03: Formulário deve ter apenas 2 campos: Email e Senha
- CA04: Validação de formato de email
- CA05: Validação de senha mínima de 6 caracteres
- CA06: Mensagem de erro clara em caso de credenciais inválidas
- CA07: Token JWT deve ser armazenado em localStorage
- CA08: Contexto da barbearia (ID, nome, código) deve ser armazenado
- CA09: Todas as rotas subsequentes devem manter o código na URL (`/{codigo}/dashboard`, `/{codigo}/barbeiros`, etc.)

### RF02: Dashboard de Admin Barbearia
**Descrição:** Após login, usuário deve visualizar dashboard com informações relevantes de sua barbearia.

**Critérios de Aceitação:**
- CA01: Dashboard deve exibir nome e código da barbearia
- CA02: Cards com métricas resumidas (total de barbeiros, serviços, agendamentos do dia)
- CA03: Menu lateral com opções: Perfil, Barbeiros, Serviços, Agendamentos
- CA04: Botão de logout visível

### RF03: Gestão de Barbeiros
**Descrição:** Admin Barbearia deve gerenciar equipe de barbeiros.

**Critérios de Aceitação:**
- CA01: Listar barbeiros com nome, email, telefone, status
- CA02: Cadastrar barbeiro com nome, email, telefone
- CA03: Editar dados de barbeiro existente
- CA04: Desativar/Reativar barbeiro
- CA05: Validações de email único e telefone válido

### RF04: Gestão de Serviços
**Descrição:** Admin Barbearia deve gerenciar serviços oferecidos.

**Critérios de Aceitação:**
- CA01: Listar serviços com nome, duração, preço, status
- CA02: Cadastrar serviço com nome, descrição, duração (minutos), preço
- CA03: Editar serviço existente
- CA04: Desativar/Reativar serviço
- CA05: Validação de preço >= 0 e duração > 0

### RF05: Visualização de Agendamentos
**Descrição:** Admin Barbearia deve visualizar agendamentos da equipe.

**Critérios de Aceitação:**
- CA01: Listar agendamentos com data, hora, cliente, barbeiro, serviço, status
- CA02: Filtrar por barbeiro
- CA03: Filtrar por data/período
- CA04: Exibir detalhes do agendamento ao clicar

## 5. Requisitos Não-Funcionais

### RNF01: Segurança
- Tokens JWT devem ter expiração configurável
- Rotas protegidas devem verificar tipo de usuário (AdminBarbearia)
- Dados de outras barbearias não devem ser acessíveis

### RNF02: Performance
- Tempo de carregamento de listagens < 2 segundos
- Feedback visual em operações assíncronas (loading states)

### RNF03: Usabilidade
- Interface responsiva (mobile-first)
- Mensagens de erro claras e acionáveis
- Consistência visual com padrão do Admin Central

### RNF04: Compatibilidade
- Suporte a navegadores modernos (Chrome, Firefox, Safari, Edge)
- Suporte a dispositivos móveis (iOS, Android)

## 6. Especificações Técnicas

### 6.0. Vantagens da Abordagem com Código na URL

**UX/Usabilidade:**
- ✅ **Simplicidade**: Usuário não precisa digitar/lembrar código complexo
- ✅ **Links personalizados**: Cada barbearia tem URL única e memorável
- ✅ **Facilita onboarding**: Admin Central pode enviar link direto no email de boas-vindas
- ✅ **Bookmarks**: Usuários podem salvar favorito do navegador com URL específica
- ✅ **Menos erros**: Elimina possibilidade de digitar código errado

**Técnico:**
- ✅ **Multi-tenancy claro**: Código na URL torna contexto explícito
- ✅ **Validação antecipada**: Backend valida código antes mesmo do login
- ✅ **SEO-friendly**: URLs estruturadas e descritivas
- ✅ **Deep linking**: Possível compartilhar links diretos para páginas específicas
- ✅ **Isolamento**: Cada barbearia tem "namespace" próprio na aplicação

**Segurança:**
- ✅ **Menor superfície de ataque**: Código não é enviado em formulário
- ✅ **Validação em camadas**: URL → Código → Autenticação → Autorização
- ✅ **Auditoria**: Logs incluem código da URL para rastreabilidade

**Exemplo de fluxo completo:**
```
Email de boas-vindas enviado pelo Admin Central:
┌────────────────────────────────────────────────┐
│ Olá, Tasso!                                    │
│                                                │
│ Sua barbearia foi cadastrada com sucesso!     │
│                                                │
│ Nome: Barbearia do Tasso Zé                    │
│ Código: 6SJJRFPD                               │
│                                                │
│ Acesse o painel administrativo:                │
│ 👉 http://app.barbapp.com/6SJJRFPD/login       │
│                                                │
│ Email: tasso.gomes@outlook.com                 │
│ Senha: 96z7ZBK#DXNn                            │
│                                                │
│ Recomendamos alterar a senha no primeiro      │
│ acesso.                                        │
└────────────────────────────────────────────────┘
```

### 6.1. Arquitetura

```
barbapp-admin-barbearia/  (nova aplicação ou contexto)
├── src/
│   ├── pages/
│   │   ├── Login/
│   │   │   └── LoginAdminBarbearia.tsx  (rota: /:codigo/login)
│   │   ├── Dashboard/
│   │   │   └── Dashboard.tsx            (rota: /:codigo/dashboard)
│   │   ├── Barbers/
│   │   │   ├── List.tsx                 (rota: /:codigo/barbeiros)
│   │   │   ├── Form.tsx                 (rota: /:codigo/barbeiros/novo | :id/editar)
│   │   ├── Services/
│   │   │   ├── List.tsx                 (rota: /:codigo/servicos)
│   │   │   ├── Form.tsx                 (rota: /:codigo/servicos/novo | :id/editar)
│   │   └── Schedule/
│   │       └── List.tsx                 (rota: /:codigo/agenda)
│   ├── services/
│   │   ├── auth.service.ts
│   │   ├── barbershop.service.ts        (validação de código)
│   │   ├── barber.service.ts
│   │   ├── service.service.ts
│   │   └── appointment.service.ts
│   ├── contexts/
│   │   └── BarbeariaContext.tsx         (armazena código e dados da barbearia)
│   ├── hooks/
│   │   └── useBarbeariaCode.ts          (extrai código da URL)
│   └── routes/
│       └── index.tsx                    (todas as rotas com prefixo /:codigo)
```

**Estrutura de Rotas:**
```
/:codigo/login              → LoginAdminBarbearia
/:codigo/dashboard          → Dashboard (protegida)
/:codigo/barbeiros          → ListaBarbeiros (protegida)
/:codigo/barbeiros/novo     → FormularioBarbeiro (protegida)
/:codigo/barbeiros/:id      → DetalhesBarbeiro (protegida)
/:codigo/servicos           → ListaServicos (protegida)
/:codigo/servicos/novo      → FormularioServico (protegida)
/:codigo/agenda             → Agendamentos (protegida)
```

### 6.2. Endpoints Backend Necessários

**Já Existentes:**
- `POST /api/auth/admin-barbearia/login`
- `GET /api/barbeiros` (listagem de barbeiros, filtrado por tenant)
- `POST /api/barbeiros` (criação de barbeiro)
- `PUT /api/barbeiros/:id` (edição de barbeiro)
- `DELETE /api/barbeiros/:id` (desativação de barbeiro)
- `GET /api/servicos` (listagem de serviços)
- `POST /api/servicos` (criação de serviço)
- `PUT /api/servicos/:id` (edição de serviço)
- `DELETE /api/servicos/:id` (desativação de serviço)
- `GET /api/agendamentos` (listagem de agendamentos)

**A Verificar/Implementar:**
- `GET /api/barbearias/me` (dados da barbearia do usuário logado)
- `GET /api/barbearias/codigo/:codigo` (validar código e obter nome da barbearia - **NOVO**)
  - Usado antes do login para validar URL e exibir nome da barbearia
  - Retorna apenas dados públicos: { id, nome, codigo, isActive }
  - Não requer autenticação

### 6.3. Tecnologias

**Frontend:**
- React 18
- TypeScript
- React Router v6
- TanStack Query (React Query)
- Axios
- Zod (validações)
- Shadcn/ui (componentes)
- Tailwind CSS

**Backend:**
- .NET 8 (já implementado)
- PostgreSQL (já implementado)

## 7. Fluxos de Usuário

### Diagrama de Sequência - Login com Código na URL

```
Admin      Frontend                    Backend API                Database
  |            |                            |                         |
  |--[1]------>|                            |                         |
  | Acessa     |                            |                         |
  | /6SJJRFPD/login                        |                         |
  |            |                            |                         |
  |            |--[2] GET /api/barbearias/codigo/6SJJRFPD           |
  |            |                            |                         |
  |            |                            |--[3] SELECT * FROM barbershops
  |            |                            |     WHERE code = '6SJJRFPD'
  |            |                            |<--------[4]------------|
  |            |                            | {id, nome, codigo, isActive}
  |            |<-------[5] 200 OK----------|                         |
  |            | {nome: "Barbearia do Tasso"}                        |
  |            |                            |                         |
  |<--[6]------|                            |                         |
  | Exibe tela |                            |                         |
  | "Login - Barbearia do Tasso"           |                         |
  |            |                            |                         |
  |--[7]------>|                            |                         |
  | Preenche   |                            |                         |
  | email/senha|                            |                         |
  |            |                            |                         |
  |--[8]------>|                            |                         |
  | Clica      |                            |                         |
  | "Entrar"   |                            |                         |
  |            |                            |                         |
  |            |--[9] POST /api/auth/admin-barbearia/login          |
  |            |    { codigo: "6SJJRFPD",   |                         |
  |            |      email: "tasso@...",   |                         |
  |            |      senha: "..." }        |                         |
  |            |                            |                         |
  |            |                            |--[10] Valida credenciais
  |            |                            |<--------[11]-----------|
  |            |                            | Admin encontrado       |
  |            |<------[12] 200 OK----------|                         |
  |            | { token, barbeariaId, ... }|                         |
  |            |                            |                         |
  |<--[13]-----|                            |                         |
  | Armazena   |                            |                         |
  | token +    |                            |                         |
  | contexto   |                            |                         |
  |            |                            |                         |
  |<--[14]-----|                            |                         |
  | Redireciona para /6SJJRFPD/dashboard   |                         |
  |            |                            |                         |
```

### Fluxo 1: Login de Admin Barbearia

```
1. Admin Barbearia recebe link/URL personalizada: http://app.barbapp.com/6SJJRFPD/login
2. Usuário acessa a URL
3. Sistema extrai código "6SJJRFPD" da URL
4. Sistema faz requisição GET /api/barbearias/codigo/6SJJRFPD (sem autenticação)
   4a. Se código inválido (404): exibe "Barbearia não encontrada"
   4b. Se barbearia inativa: exibe "Barbearia temporariamente indisponível"
   4c. Se válido (200): continua para passo 5
5. Sistema exibe página de login com:
   - Cabeçalho: "Login - [Nome da Barbearia]" (ex: "Login - Barbearia do Tasso")
   - Campo Email
   - Campo Senha
   - Botão "Entrar"
6. Usuário preenche email e senha
7. Usuário clica em "Entrar"
8. Sistema valida formato dos campos
9. Sistema envia POST para /api/auth/admin-barbearia/login com:
   {
     "codigo": "6SJJRFPD",  // extraído da URL
     "email": "tasso.gomes@outlook.com",
     "senha": "96z7ZBK#DXNn"
   }
10. Backend valida credenciais
    10a. Se inválido: retorna 401 com mensagem de erro
    10b. Se válido: retorna 200 com token JWT e dados da barbearia
11. Sistema armazena token e contexto (barbearia_id, nome, código)
12. Sistema redireciona para /6SJJRFPD/dashboard
13. Todas as navegações subsequentes mantêm o código na URL:
    - /6SJJRFPD/barbeiros
    - /6SJJRFPD/servicos
    - /6SJJRFPD/agenda
```

### Fluxo 2: Cadastro de Barbeiro

```
1. Admin Barbearia acessa página "Barbeiros"
2. Sistema lista barbeiros da barbearia
3. Admin clica em "Novo Barbeiro"
4. Sistema exibe formulário com campos:
   - Nome
   - Email
   - Telefone
5. Admin preenche dados
6. Admin clica em "Salvar"
7. Sistema valida campos
8. Sistema envia POST para /api/barbeiros com token
9. Backend cria barbeiro associado à barbearia do token
   9a. Se email duplicado: retorna 409 com erro
   9b. Se sucesso: retorna 201 com barbeiro criado
10. Sistema exibe mensagem de sucesso
11. Sistema retorna para listagem
```

### Fluxo 3: Visualização de Agendamentos

```
1. Admin Barbearia acessa página "Agendamentos"
2. Sistema carrega agendamentos da barbearia (GET /api/agendamentos)
3. Sistema exibe lista ordenada por data/hora
4. Admin pode aplicar filtros:
   - Por barbeiro
   - Por data/período
5. Sistema atualiza listagem conforme filtros
6. Admin clica em agendamento para ver detalhes
7. Sistema exibe modal/página com detalhes completos
```

## 8. Mockups e Wireframes

### Tela de Login Admin Barbearia

```
┌─────────────────────────────────────────┐
│                                         │
│          🪒 BarbApp                     │
│       Barbearia do Tasso Zé             │
│     (extraído de /6SJJRFPD/login)       │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │ Email                             │  │
│  │ [_________________________]       │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │ Senha                             │  │
│  │ [_________________________] 👁️    │  │
│  └───────────────────────────────────┘  │
│                                         │
│  ┌───────────────────────────────────┐  │
│  │        [  ENTRAR  ]               │  │
│  └───────────────────────────────────┘  │
│                                         │
│  Esqueceu a senha? Contate o suporte   │
│                                         │
└─────────────────────────────────────────┘

Nota: O código "6SJJRFPD" vem da URL, não é digitado.
URL completa: http://localhost:3000/6SJJRFPD/login
```

### Dashboard Admin Barbearia

```
┌──────────────────────────────────────────────────────────────┐
│ 🪒 Barbearia do Tasso Zé (COD: 6SJJRFPD)           👤 Tasso▼│
├─────────────┬────────────────────────────────────────────────┤
│ 📊 Dashboard│  Bem-vindo, Tasso Silva Gomes                  │
│ 👨‍💼 Barbeiros│                                                │
│ ✂️ Serviços  │  ┌──────────┐ ┌──────────┐ ┌──────────┐      │
│ 📅 Agenda   │  │ Barbeiros│ │ Serviços │ │Agend.Hoje│      │
│             │  │    5     │ │    8     │ │    12    │      │
│ 🚪 Sair     │  └──────────┘ └──────────┘ └──────────┘      │
│             │                                                │
│             │  Próximos Agendamentos:                        │
│             │  ┌──────────────────────────────────────────┐ │
│             │  │ 10:00 - Corte (João, Cliente A)         │ │
│             │  │ 10:30 - Barba (Pedro, Cliente B)        │ │
│             │  │ 11:00 - Corte (João, Cliente C)         │ │
│             │  └──────────────────────────────────────────┘ │
└─────────────┴────────────────────────────────────────────────┘

URL: http://localhost:3000/6SJJRFPD/dashboard

Navegação:
- Clicar em "Barbeiros" → /6SJJRFPD/barbeiros
- Clicar em "Serviços"  → /6SJJRFPD/servicos
- Clicar em "Agenda"    → /6SJJRFPD/agenda
```

## 9. Estratégia de Implementação

### Fase 1: Autenticação e Estrutura (Sprint 1)
- [ ] Criar estrutura base da aplicação com rotas dinâmicas (`:codigo`)
- [ ] Implementar hook `useBarbeariaCode()` para extrair código da URL
- [ ] Criar serviço `barbershop.service.ts` com método `validateCode(codigo)`
- [ ] Implementar página de login Admin Barbearia
  - [ ] Validação de código ao carregar (GET /api/barbearias/codigo/:codigo)
  - [ ] Exibição do nome da barbearia no cabeçalho
  - [ ] Formulário com email e senha
- [ ] Integrar com endpoint de autenticação POST /api/auth/admin-barbearia/login
- [ ] Implementar armazenamento de contexto (token, barbearia_id, codigo)
- [ ] Criar ProtectedRoute que valida código + token
- [ ] Implementar logout
- [ ] Criar página de erro 404 para códigos inválidos

### Fase 2: Dashboard e Navegação (Sprint 2)
- [ ] Criar Dashboard com métricas básicas
- [ ] Implementar layout com menu lateral
- [ ] Criar estrutura de rotas para cada seção
- [ ] Implementar header com informações da barbearia

### Fase 3: Gestão de Barbeiros (Sprint 3)
- [ ] Criar página de listagem de barbeiros
- [ ] Implementar formulário de cadastro/edição
- [ ] Integrar com endpoints de barbeiros
- [ ] Implementar ativação/desativação
- [ ] Adicionar validações e tratamento de erros

### Fase 4: Gestão de Serviços (Sprint 4)
- [ ] Criar página de listagem de serviços
- [ ] Implementar formulário de cadastro/edição
- [ ] Integrar com endpoints de serviços
- [ ] Implementar ativação/desativação
- [ ] Adicionar validações de preço e duração

### Fase 5: Visualização de Agendamentos (Sprint 5)
- [ ] Criar página de listagem de agendamentos
- [ ] Implementar filtros (barbeiro, data)
- [ ] Criar modal/página de detalhes
- [ ] Implementar paginação

### Fase 6: Testes e Refinamentos (Sprint 6)
- [ ] Testes unitários de componentes
- [ ] Testes de integração
- [ ] Testes E2E de fluxos principais
- [ ] Ajustes de UX/UI
- [ ] Documentação

## 10. Critérios de Sucesso

### Métricas Quantitativas
- [ ] 100% dos endpoints de Admin Barbearia integrados
- [ ] Cobertura de testes >= 80%
- [ ] Tempo de carregamento < 2s
- [ ] 0 erros críticos em produção

### Métricas Qualitativas
- [ ] Admin Barbearia consegue fazer login sem assistência
- [ ] Admin Barbearia consegue cadastrar barbeiro em < 2 minutos
- [ ] Admin Barbearia consegue cadastrar serviço em < 1 minuto
- [ ] Interface responsiva em dispositivos móveis

## 11. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|---------------|---------|-----------|
| Endpoints backend incompletos | Média | Alto | Validar todos os endpoints necessários antes de iniciar frontend |
| Isolamento de dados (tenant) não funcionar | Baixa | Crítico | Testes de segurança rigorosos, validação de token em cada requisição |
| Inconsistência UX entre Admin Central e Admin Barbearia | Média | Médio | Reutilizar componentes do Admin Central, seguir design system |
| Confusão de usuários entre URLs | Alta | Médio | URLs distintas, branding claro em cada aplicação |

## 12. Dependências

### Dependências Backend (Críticas - Devem ser implementadas primeiro)
- [ ] **Endpoint de validação de código** - `GET /api/barbearias/codigo/:codigo`
  - **Prioridade:** Alta (bloqueia desenvolvimento frontend)
  - **Descrição:** Endpoint público (sem autenticação) que valida código da barbearia
  - **Request:** GET `/api/barbearias/codigo/6SJJRFPD`
  - **Response 200:** `{ "id": "uuid", "nome": "Barbearia do Tasso Zé", "codigo": "6SJJRFPD", "isActive": true }`
  - **Response 404:** `{ "message": "Barbearia não encontrada" }`
  - **Response 403:** `{ "message": "Barbearia inativa" }` (se `isActive = false`)
  - **Observação:** Este endpoint não deve retornar dados sensíveis (email, telefone, endereço completo)

### Dependências Gerais
- [ ] Backend endpoints funcionais para Admin Barbearia (já existentes - validar funcionamento)
- [ ] Banco de dados com seed de Admin Barbearia para testes
- [ ] Design system / biblioteca de componentes UI (Shadcn/ui)
- [ ] Ambiente de staging para testes

### Tarefas Backend Adicionais
- [ ] Atualizar email de boas-vindas do `CreateBarbershopUseCase` para incluir link personalizado
  - Formato: `http://app.barbapp.com/{codigo}/login`
- [ ] Validar que endpoint `/api/auth/admin-barbearia/login` aceita código na URL ou no body

## 13. Documentação Técnica Necessária

- [ ] Guia de setup do ambiente de desenvolvimento
- [ ] Documentação de API (Swagger)
- [ ] Guia de estilo de código
- [ ] Documentação de deploy

## 14. Cronograma Estimado

| Fase | Duração | Início | Fim |
|------|---------|--------|-----|
| Fase 1 | 1 semana | S1 | S1 |
| Fase 2 | 1 semana | S2 | S2 |
| Fase 3 | 1 semana | S3 | S3 |
| Fase 4 | 1 semana | S4 | S4 |
| Fase 5 | 1 semana | S5 | S5 |
| Fase 6 | 1 semana | S6 | S6 |
| **Total** | **6 semanas** | | |

## 15. Aprovações

- [ ] Product Owner
- [ ] Tech Lead
- [ ] UX/UI Designer
- [ ] QA Lead

---

**Criado em:** 2025-10-17  
**Versão:** 1.0  
**Status:** Proposta
