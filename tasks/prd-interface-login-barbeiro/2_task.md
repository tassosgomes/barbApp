---
status: pending
parallelizable: true
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/frontend/services</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis|http_server</dependencies>
<unblocks>"3.0","4.0"</unblocks>
</task_context>

# Tarefa 2.0: Services - Auth Service e Interceptors Axios

## Visão Geral
Implementar o serviço de autenticação que consome o endpoint do backend e configurar interceptors do Axios para adicionar token JWT automaticamente e tratar respostas 401 (token expirado).

## Requisitos
- Service `auth.service.ts` com métodos login, validateToken, logout
- Interceptor de request para adicionar token JWT no header
- Interceptor de response para tratar 401 e redirecionar para login
- Tratamento de erros específicos (400, 401, 500)

## Subtarefas
- [ ] 2.1 Criar `src/services/auth.service.ts`:
  - `login(data: LoginInput): Promise<AuthResponse>`
  - `validateToken(): Promise<User>`
  - `logout(): void`
- [ ] 2.2 Atualizar `src/lib/api.ts`:
  - Interceptor de request (adicionar token se existir)
  - Interceptor de response (tratar 401)
- [ ] 2.3 Testar com chamadas ao backend real/mock
- [ ] 2.4 Documentar formato esperado de request/response

## Sequenciamento
- Bloqueado por: 1.0 (Tipos)
- Desbloqueia: 3.0, 4.0
- Paralelizável: Sim (pode começar com mocks)

## Detalhes de Implementação

**Auth Service:**
```typescript
// src/services/auth.service.ts
import { api } from '@/lib/api';
import { formatPhoneToAPI } from '@/lib/phone-utils';
import type { LoginInput, AuthResponse, User } from '@/types/auth.types';

export const authService = {
  login: async (data: LoginInput): Promise<AuthResponse> => {
    const response = await api.post<AuthResponse>('/auth/barbeiro/login', {
      barbershopCode: data.barbershopCode.toUpperCase(),
      phone: formatPhoneToAPI(data.phone)
    });
    
    return response.data;
  },
  
  validateToken: async (): Promise<User> => {
    // Endpoint para validar token e buscar dados do usuário
    const response = await api.get<User>('/barber/profile');
    return response.data;
  },
  
  logout: () => {
    localStorage.removeItem('barbapp-barber-token');
  }
};
```

**Axios Interceptors:**
```typescript
// src/lib/api.ts (atualização)
import axios from 'axios';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api'
});

// Request interceptor - adicionar token
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('barbapp-barber-token');
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    return config;
  },
  (error) => Promise.reject(error)
);

// Response interceptor - tratar 401
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Token expirado ou inválido
      localStorage.removeItem('barbapp-barber-token');
      
      // Redirecionar para login apenas se não estiver já na página de login
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login';
      }
    }
    
    return Promise.reject(error);
  }
);
```

**Formato de Request/Response:**
```typescript
// Request
POST /api/auth/barbeiro/login
{
  "barbershopCode": "BARB001",
  "phone": "+5511999999999"
}

// Response Success (200)
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": "uuid",
    "name": "João Silva",
    "phone": "+5511999999999",
    "role": "Barbeiro"
  }
}

// Response Error (401)
{
  "message": "Invalid credentials"
}
```

## Critérios de Sucesso
- Service executa chamadas HTTP corretamente
- Interceptor adiciona token em todas as requisições autenticadas
- Interceptor trata 401 e redireciona para login
- Erros são propagados corretamente para serem tratados no UI
- Testes com mock do axios passam
