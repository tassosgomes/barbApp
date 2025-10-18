# Comparação: Admin Central vs Admin Barbearia

Este documento compara as duas interfaces administrativas do sistema BarbApp, destacando diferenças de rotas, autenticação, permissões e funcionalidades.

## Índice

1. [Visão Geral](#visão-geral)
2. [Estrutura de Rotas](#estrutura-de-rotas)
3. [Autenticação e Segurança](#autenticação-e-segurança)
4. [Permissões e Acesso](#permissões-e-acesso)
5. [Funcionalidades](#funcionalidades)
6. [Multi-Tenancy](#multi-tenancy)
7. [Casos de Uso](#casos-de-uso)
8. [Tabela Comparativa](#tabela-comparativa)

---

## Visão Geral

### Admin Central

**Propósito:** Interface para gestão centralizada de **todas as barbearias** do sistema.

**Usuário-alvo:** Administradores da plataforma (super admins)

**Escopo:** Visão global de todo o sistema

### Admin Barbearia

**Propósito:** Interface para que cada barbearia gerencie seus **próprios recursos**.

**Usuário-alvo:** Administradores de cada barbearia individual

**Escopo:** Visão isolada dos dados de uma única barbearia (multi-tenant)

---

## Estrutura de Rotas

### Admin Central

Rotas **fixas** sem tenant no path:

```
/admin-central/login
/admin-central/dashboard
/admin-central/barbearias
/admin-central/barbearias/nova
/admin-central/barbearias/:id/editar
```

**Características:**
- Prefixo fixo: `/admin-central`
- Sem código de barbearia na URL
- Acesso global a todas as barbearias

### Admin Barbearia

Rotas **dinâmicas** com tenant no path:

```
/{codigo-barbearia}/login
/{codigo-barbearia}/dashboard
/{codigo-barbearia}/barbeiros
/{codigo-barbearia}/servicos
/{codigo-barbearia}/agenda
```

**Exemplos reais:**
```
/ABC123/login
/BARBER2024/dashboard
/TEST1234/barbeiros
```

**Características:**
- Prefixo dinâmico: `/{codigo-barbearia}`
- Código da barbearia extraído da URL
- Acesso isolado por tenant (multi-tenant)

---

## Autenticação e Segurança

### Admin Central

| Aspecto | Detalhes |
|---------|----------|
| **Endpoint de Login** | `POST /auth/admin-central/login` |
| **Credenciais** | Email + Senha |
| **Token JWT** | Armazenado em `localStorage` como `adminCentral_token` |
| **Claims** | `role: AdminCentral` |
| **Validação** | Token JWT com role `AdminCentral` |
| **Tenant** | Não aplicável (acesso global) |

**Exemplo de requisição:**
```http
POST /auth/admin-central/login
Content-Type: application/json

{
  "email": "admin@barbapp.com",
  "senha": "Senha@123"
}
```

### Admin Barbearia

| Aspecto | Detalhes |
|---------|----------|
| **Endpoint de Login** | `POST /auth/admin-barbearia/login` |
| **Credenciais** | Código da Barbearia + Email + Senha |
| **Token JWT** | Armazenado em `localStorage` como `adminBarbearia_token` |
| **Claims** | `role: AdminBarbearia`, `barbeariaId: {id}` |
| **Validação** | Token JWT com role `AdminBarbearia` + validação de tenant |
| **Tenant** | Obrigatório - extraído da URL e validado na API |

**Exemplo de requisição:**
```http
POST /auth/admin-barbearia/login
Content-Type: application/json

{
  "codigoBarbearia": "ABC123",
  "email": "admin@barbearia.com",
  "senha": "Senha@123"
}
```

**Fluxo de autenticação:**
1. Usuário acessa `/{codigo}/login`
2. Frontend valida código: `GET /barbershops/{codigo}/info`
3. Se válido, exibe formulário de login
4. Frontend envia: código + email + senha
5. Backend valida credenciais e retorna token com `barbeariaId`
6. Token é usado em todas as requisições subsequentes

---

## Permissões e Acesso

### Admin Central

**Permissões:**
- ✅ Criar novas barbearias
- ✅ Editar qualquer barbearia
- ✅ Desativar barbearias
- ✅ Visualizar todas as barbearias
- ✅ Criar administradores de barbearias
- ✅ Visualizar métricas globais
- ❌ Não gerencia barbeiros/serviços diretamente
- ❌ Não visualiza agendamentos individuais

**Restrições:**
- Não tem acesso às rotas de Admin Barbearia
- Não pode se autenticar como Admin Barbearia

### Admin Barbearia

**Permissões:**
- ✅ Criar/editar/desativar barbeiros da própria barbearia
- ✅ Criar/editar/desativar serviços da própria barbearia
- ✅ Visualizar agendamentos da própria barbearia
- ✅ Visualizar métricas da própria barbearia
- ❌ Não acessa dados de outras barbearias
- ❌ Não cria ou edita barbearias
- ❌ Não cria agendamentos (apenas visualiza)

**Restrições:**
- Acesso restrito apenas à barbearia autenticada (tenant)
- Não tem acesso às rotas de Admin Central
- Não pode acessar dados de outros tenants

**Isolamento Multi-Tenant:**
```typescript
// Exemplo de validação de tenant na API
if (authenticatedBarbeariaId !== requestedBarbeariaId) {
  return 403 Forbidden;
}
```

---

## Funcionalidades

### Admin Central

| Funcionalidade | Descrição |
|----------------|-----------|
| **Dashboard** | Métricas globais: total de barbearias, barbearias ativas/inativas |
| **Gestão de Barbearias** | CRUD completo de barbearias |
| **Cadastro de Admin Barbearia** | Criar conta de administrador para cada barbearia |
| **Onboarding** | Fluxo completo de cadastro de nova barbearia + admin |
| **Envio de Emails** | Email de boas-vindas com link e credenciais |
| **Visualização Global** | Lista todas as barbearias do sistema |

**Endpoints principais:**
```
GET    /barbershops
POST   /barbershops
PUT    /barbershops/{id}
DELETE /barbershops/{id}
POST   /auth/admin-central/login
POST   /admins/barbershop
```

### Admin Barbearia

| Funcionalidade | Descrição |
|----------------|-----------|
| **Dashboard** | Métricas da barbearia: total de barbeiros, serviços, agendamentos |
| **Gestão de Barbeiros** | CRUD completo de barbeiros |
| **Gestão de Serviços** | CRUD completo de serviços (preço, duração) |
| **Visualização de Agenda** | Lista agendamentos com filtros (barbeiro, data, status) |
| **Filtros Avançados** | Filtrar agendamentos por múltiplos critérios |
| **Detalhes de Agendamento** | Modal com informações completas |

**Endpoints principais:**
```
GET    /barbers?barbershopId={id}
POST   /barbers
PUT    /barbers/{id}
DELETE /barbers/{id}

GET    /services?barbershopId={id}
POST   /services
PUT    /services/{id}
DELETE /services/{id}

GET    /appointments?barbershopId={id}&filters=...
GET    /appointments/{id}

POST   /auth/admin-barbearia/login
GET    /barbershops/{code}/info
```

---

## Multi-Tenancy

### Admin Central: Sem Multi-Tenancy

- Acesso global a todos os dados
- Não há isolamento por tenant
- Role única: `AdminCentral`

### Admin Barbearia: Multi-Tenancy Completo

**Extração do Tenant:**
```typescript
// Frontend: Extração do código da URL
const { codigo } = useParams(); // "ABC123"

// Backend: Validação do tenant no token
const barbeariaId = jwt.claims.barbeariaId;
```

**Isolamento de Dados:**
```sql
-- Todas as queries incluem filtro por barbeariaId
SELECT * FROM Barbers WHERE BarbeariaId = @barbeariaId;
SELECT * FROM Services WHERE BarbeariaId = @barbeariaId;
SELECT * FROM Appointments WHERE BarbeariaId = @barbeariaId;
```

**Validação Cross-Tenant:**
```typescript
// Tentativa de acesso a dados de outro tenant
GET /barbers/123 (barber da barbearia X)
Token: barbeariaId = Y

// Resultado: 403 Forbidden (tenant mismatch)
```

**LocalStorage Isolado:**
```javascript
// Cada barbearia tem seu próprio token
localStorage.setItem('adminBarbearia_token', token);
localStorage.setItem('adminBarbearia_barbeariaId', barbeariaId);
localStorage.setItem('adminBarbearia_codigo', codigo);
```

---

## Casos de Uso

### Quando usar Admin Central?

**Persona:** Super administrador da plataforma

**Cenários:**
1. Cadastrar nova barbearia no sistema
2. Editar informações de qualquer barbearia (nome, endereço, telefone)
3. Desativar barbearia que não está mais operando
4. Criar conta de administrador para uma barbearia
5. Visualizar lista de todas as barbearias
6. Gerar relatórios globais

**Exemplo de fluxo:**
```
1. Admin Central acessa /admin-central/barbearias/nova
2. Preenche formulário: nome, endereço, telefone, código único
3. Sistema cria barbearia
4. Admin Central cria admin da barbearia: email + senha
5. Sistema envia email de boas-vindas com link: /{codigo}/login
6. Admin da barbearia acessa o link e faz login
```

### Quando usar Admin Barbearia?

**Persona:** Administrador de uma barbearia específica

**Cenários:**
1. Cadastrar novos barbeiros da equipe
2. Adicionar ou editar serviços oferecidos (cortes, barbas, etc)
3. Definir preços e durações dos serviços
4. Visualizar agendamentos do dia/semana/mês
5. Filtrar agendamentos por barbeiro ou status
6. Verificar detalhes de um agendamento específico

**Exemplo de fluxo:**
```
1. Admin Barbearia acessa /ABC123/login
2. Faz login com email e senha
3. Acessa /ABC123/barbeiros
4. Cadastra novo barbeiro: João Silva
5. Acessa /ABC123/servicos
6. Cadastra serviço: "Corte Masculino" - R$ 50,00 - 60 min
7. Acessa /ABC123/agenda
8. Visualiza agendamentos de João Silva para hoje
```

---

## Tabela Comparativa

| Aspecto | Admin Central | Admin Barbearia |
|---------|---------------|-----------------|
| **Prefixo de Rota** | `/admin-central` | `/{codigo-barbearia}` |
| **Tenant** | Não aplicável (global) | Obrigatório (multi-tenant) |
| **Autenticação** | Email + Senha | Código + Email + Senha |
| **Token LocalStorage** | `adminCentral_token` | `adminBarbearia_token` |
| **Role JWT** | `AdminCentral` | `AdminBarbearia` |
| **Escopo de Dados** | Todas as barbearias | Uma barbearia específica |
| **Gestão de Barbearias** | ✅ Completa (CRUD) | ❌ Não permitido |
| **Gestão de Barbeiros** | ❌ Não gerencia | ✅ Completa (CRUD) |
| **Gestão de Serviços** | ❌ Não gerencia | ✅ Completa (CRUD) |
| **Visualização de Agenda** | ❌ Não visualiza | ✅ Completa (filtros) |
| **Criação de Admins** | ✅ Cria admins de barbearias | ❌ Não cria |
| **Onboarding** | ✅ Fluxo completo | ❌ Apenas login |
| **Emails** | ✅ Envia boas-vindas | ❌ Não envia |
| **Dashboard** | Métricas globais | Métricas da barbearia |
| **Cross-Tenant Access** | ✅ Acessa todas | ❌ Bloqueado (403) |
| **URL de Acesso** | Fixa | Dinâmica por tenant |
| **Validação de Código** | Não necessária | `GET /barbershops/{code}/info` |
| **Isolamento de Dados** | Não aplicável | Query + Token validation |

---

## Exemplo Completo de Fluxo

### Cenário: Nova Barbearia

**1. Admin Central cria barbearia**
```http
POST /barbershops
Authorization: Bearer {adminCentral_token}

{
  "nome": "Barber Shop Elite",
  "endereco": "Rua das Flores, 123",
  "telefone": "(11) 98765-4321",
  "codigo": "ELITE2025"
}
```

**2. Admin Central cria admin da barbearia**
```http
POST /admins/barbershop
Authorization: Bearer {adminCentral_token}

{
  "barbeariaId": "abc-123-def",
  "nome": "João Silva",
  "email": "joao@elite.com",
  "senha": "Senha@123"
}
```

**3. Sistema envia email**
```
Para: joao@elite.com
Assunto: Bem-vindo ao BarbApp!

Olá João Silva!

Sua barbearia "Barber Shop Elite" foi cadastrada com sucesso!

Acesse: https://barbapp.com/ELITE2025
Email: joao@elite.com
Senha: Senha@123

Recomendamos trocar a senha no primeiro acesso.
```

**4. Admin Barbearia acessa e faz login**
```
URL: https://barbapp.com/ELITE2025/login

Sistema valida código: GET /barbershops/ELITE2025/info
✅ Código válido - exibe formulário

Credenciais:
Email: joao@elite.com
Senha: Senha@123

POST /auth/admin-barbearia/login
✅ Autenticado - Token gerado com barbeariaId
```

**5. Admin Barbearia cadastra barbeiro**
```http
POST /barbers
Authorization: Bearer {adminBarbearia_token}
X-Tenant-Id: ELITE2025

{
  "nome": "Carlos Barbeiro",
  "email": "carlos@elite.com",
  "telefone": "(11) 91234-5678",
  "especialidade": "Cortes modernos"
}

✅ Barbeiro criado com barbeariaId = "abc-123-def"
```

**6. Admin Barbearia cadastra serviço**
```http
POST /services
Authorization: Bearer {adminBarbearia_token}
X-Tenant-Id: ELITE2025

{
  "nome": "Corte + Barba",
  "descricao": "Corte completo com acabamento de barba",
  "preco": 75.00,
  "duracaoMinutos": 90
}

✅ Serviço criado com barbeariaId = "abc-123-def"
```

**7. Admin Barbearia visualiza agenda**
```http
GET /appointments?barbeariaId=abc-123-def&status=Agendado
Authorization: Bearer {adminBarbearia_token}
X-Tenant-Id: ELITE2025

✅ Retorna agendamentos apenas da barbearia ELITE2025
```

---

## Considerações de Segurança

### Admin Central
- ⚠️ Acesso privilegiado - requer credenciais super admin
- ⚠️ Não deve ser exposto publicamente
- ⚠️ Logs de auditoria obrigatórios para todas as ações

### Admin Barbearia
- 🔒 Isolamento multi-tenant obrigatório em todas as queries
- 🔒 Validação de tenant no token JWT + header da requisição
- 🔒 Código da barbearia sempre validado antes do login
- 🔒 Cross-tenant access resulta em 403 Forbidden
- 🔒 Tokens com expiração de 24 horas

---

## Conclusão

**Admin Central** e **Admin Barbearia** são interfaces complementares, mas com propósitos e escopos completamente diferentes:

- **Admin Central**: Gestão centralizada da plataforma (super admin)
- **Admin Barbearia**: Gestão isolada de cada barbearia (multi-tenant)

A separação clara de responsabilidades, rotas e autenticação garante:
1. Segurança: Isolamento de dados entre tenants
2. Escalabilidade: Cada barbearia opera independentemente
3. Manutenibilidade: Código organizado e separado por contexto
4. Usabilidade: Interfaces adaptadas para cada tipo de usuário

---

**Última atualização:** Janeiro 2025  
**Versão:** 1.0.0
