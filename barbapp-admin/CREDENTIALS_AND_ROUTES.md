# Credenciais e Rotas de Login - BarbApp

## ✅ Credenciais Corretas

### 1. Admin Central
- **Email**: `admin@babapp.com`
- **Senha**: `123456`
- **Rota de Login**: `/admin/login`
- **API Endpoint**: `/api/auth/admin-central/login`
- **Token localStorage**: `barbapp-admin-central-token`

### 2. Barbeiro
- **Email**: `dino@sauro.com`
- **Senha**: `Neide@9090`
- **Rota de Login**: `/login`
- **API Endpoint**: `/api/auth/barbeiro/login`
- **Token localStorage**: `barbapp-barber-token`

### 3. Admin Barbearia
- **Código da Barbearia**: `AMG7V8Y9`
- **Email**: `neide.patricio@hotmail.com`
- **Senha**: `S4nE23g@Qgu5`
- **Rota de Login**: `/:codigo/login` (ex: `/AMG7V8Y9/login`)
- **API Endpoint**: `/api/auth/admin-barbearia/login`
- **Token localStorage**: `barbapp-adminBarbearia-token`

## 📝 Rotas de Login por Tipo de Usuário

### Admin Central
```typescript
// Rota
/admin/login

// Componente
<Login /> from '@/pages/Login/Login'

// Exemplo de navegação
await page.goto('/admin/login');
```

### Barbeiro
```typescript
// Rota
/login

// Componente
<LoginPage /> from '@/pages/auth/LoginPage'

// Exemplo de navegação
await page.goto('/login');
```

### Admin Barbearia
```typescript
// Rota
/:codigo/login

// Componente
<LoginAdminBarbearia /> from '@/pages/LoginAdminBarbearia'

// Exemplo de navegação
await page.goto('/AMG7V8Y9/login');
```

## 🔧 Correções Aplicadas

### 1. Código da Barbearia
- ❌ Antes: `TEST1234`
- ✅ Agora: `AMG7V8Y9`
- **Arquivos atualizados**:
  - `tests/e2e/helpers/admin-barbearia.helper.ts`
  - `tests/e2e/barbeiros.spec.ts`

### 2. Rotas de Login do Admin Central
- ❌ Antes: `/login`
- ✅ Agora: `/admin/login`
- **Arquivos atualizados**:
  - `tests/e2e/barbershop-crud.spec.ts` (5 ocorrências)

### 3. Login no barbeiros.spec.ts
- ❌ Antes: TODO sem implementação
- ✅ Agora: Usa `loginAsAdminBarbearia()` helper
- **Arquivos atualizados**:
  - `tests/e2e/barbeiros.spec.ts`

## 📊 Status dos Testes

### Barbeiro (15/15) ✅ 100%
- Todas as credenciais corretas
- Todas as rotas corretas
- Todos os testes passando

### Admin Central (Pendente)
- ✅ Credenciais corretas aplicadas
- ✅ Rotas de login corrigidas
- ⏳ Aguardando execução dos testes

### Admin Barbearia (Pendente)
- ✅ Código da barbearia corrigido
- ✅ Credenciais corretas
- ✅ Login implementado nos testes
- ⏳ Aguardando execução dos testes

## 🎯 Próximos Passos

1. Executar suite completa de testes E2E
2. Verificar se Admin Central passa nos testes de autenticação
3. Verificar se Admin Barbearia passa nos testes de gestão de barbeiros
4. Ajustar seletores e validações conforme necessário (seguindo padrões do Barbeiro)
