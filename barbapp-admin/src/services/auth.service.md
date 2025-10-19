# Auth Service - Documentação

## Visão Geral
Serviço responsável pela autenticação de barbeiros no sistema. Gerencia login, validação de token e logout.

## Métodos

### `login(data: LoginInput): Promise<AuthResponse>`

Realiza autenticação de barbeiro usando código da barbearia e telefone.

**Request:**
```http
POST /api/auth/barbeiro/login
Content-Type: application/json

{
  "barbershopCode": "BARB001",
  "phone": "+5511999999999"
}
```

**Response Success (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "uuid-do-barbeiro",
    "name": "João Silva",
    "phone": "+5511999999999",
    "role": "Barbeiro",
    "barbershopId": "uuid-da-barbearia"
  }
}
```

**Response Error (401 - Unauthorized):**
```json
{
  "message": "Invalid credentials"
}
```

**Response Error (400 - Bad Request):**
```json
{
  "message": "Validation failed",
  "errors": {
    "barbershopCode": "Barbershop code is required",
    "phone": "Invalid phone format"
  }
}
```

**Response Error (500 - Internal Server Error):**
```json
{
  "message": "Internal server error"
}
```

### `validateToken(): Promise<User>`

Valida o token JWT armazenado e retorna dados atualizados do usuário.

**Request:**
```http
GET /api/barber/profile
Authorization: Bearer {token}
```

**Response Success (200):**
```json
{
  "id": "uuid-do-barbeiro",
  "name": "João Silva",
  "phone": "+5511999999999",
  "role": "Barbeiro",
  "barbershopId": "uuid-da-barbearia"
}
```

**Response Error (401 - Unauthorized):**
```json
{
  "message": "Invalid or expired token"
}
```

### `logout(): void`

Remove o token JWT do localStorage, efetivamente deslogando o usuário.

**Comportamento:**
- Remove chave `barbapp-barber-token` do localStorage
- Não faz requisição ao servidor
- Retorna void

## Armazenamento de Token

**Chave do localStorage:** `barbapp-barber-token`

**Formato:** String JWT (Bearer token)

**Expiração:** 24 horas (gerenciado pelo backend)

## Interceptors Axios

### Request Interceptor

Adiciona automaticamente o token JWT ao header de todas as requisições quando:
- A rota atual começa com `/barber`
- Existe token armazenado em `barbapp-barber-token`

**Header adicionado:**
```
Authorization: Bearer {token}
```

### Response Interceptor

Trata erros 401 (Unauthorized) automaticamente:
1. Remove token do localStorage
2. Redireciona para `/login` (apenas se não estiver já na página de login)

## Exemplo de Uso

```typescript
import { authService } from '@/services/auth.service';

// Login
try {
  const response = await authService.login({
    barbershopCode: 'BARB001',
    phone: '(11) 99999-9999'
  });
  
  // Token é retornado, deve ser armazenado
  localStorage.setItem('barbapp-barber-token', response.token);
  
  // Usar dados do usuário
  console.log(response.user.name);
  
  // Navegar para agenda
  navigate('/barber/schedule');
} catch (error) {
  if (error.response?.status === 401) {
    console.error('Credenciais inválidas');
  }
}

// Validar token ao carregar app
try {
  const user = await authService.validateToken();
  console.log('Usuário autenticado:', user.name);
} catch (error) {
  // Token inválido, redirecionar para login
  authService.logout();
  navigate('/login');
}

// Logout
authService.logout();
navigate('/login');
```

## Notas Importantes

1. **Formato de Telefone:** 
   - UI aceita: `(11) 99999-9999`
   - API espera: `+5511999999999`
   - Conversão feita automaticamente pelo `formatPhoneToAPI()`

2. **Código da Barbearia:**
   - Sempre convertido para UPPERCASE antes de enviar à API

3. **Múltiplos Tokens:**
   - Sistema suporta 3 tipos de autenticação:
     - `auth_token`: Admin Central
     - `admin_barbearia_token`: Admin Barbearia
     - `barbapp-barber-token`: Barbeiro
   - Interceptor detecta contexto automaticamente pela rota

4. **Redirecionamento 401:**
   - Evita loop infinito verificando se já está na página de login
   - Limpa apenas o token relevante ao contexto atual

## Tratamento de Erros

| Status | Significado | Ação Recomendada |
|--------|-------------|------------------|
| 200 | Sucesso | Armazenar token e redirecionar |
| 400 | Dados inválidos | Mostrar erros de validação no formulário |
| 401 | Credenciais inválidas | Mostrar mensagem: "Código ou telefone inválidos" |
| 500 | Erro do servidor | Mostrar mensagem: "Erro ao conectar. Tente novamente" |
| Timeout | Sem resposta | Mostrar mensagem: "Conexão lenta. Tente novamente" |
