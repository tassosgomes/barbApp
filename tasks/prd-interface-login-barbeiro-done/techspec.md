# Especificação Técnica - Interface de Login e Autenticação (Barbeiro)

## Resumo Executivo

Este documento detalha a implementação da interface de login para barbeiros no frontend React. A solução consiste em uma tela de autenticação mobile-first que consome o endpoint de autenticação já implementado no backend (PRD Multi-tenant). O barbeiro fará login usando código da barbearia e telefone, receberá um token JWT que será armazenado no localStorage, e será redirecionado para sua agenda. A implementação utiliza React Hook Form para gestão de formulário, Zod para validação, React Router para navegação, e Shadcn UI para componentes visuais. A autenticação persiste por 24 horas através do token JWT, com logout manual disponível.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Pages**
- `LoginPage`: Página principal de autenticação com formulário
- Protected routes com verificação de autenticação

**Components**
- `LoginForm`: Formulário de login isolado e testável
- `ProtectedRoute`: HOC/Component para proteger rotas autenticadas
- `LogoutButton`: Botão de logout reutilizável

**Hooks**
- `useAuth`: Hook customizado para gestão de autenticação
- `useAuthValidation`: Hook para validar sessão ao carregar app

**Services**
- `auth.service.ts`: Serviço de autenticação (login, logout, validação)

**Context**
- `AuthContext`: Context API para estado global de autenticação

**Types**
- `auth.types.ts`: Tipos para autenticação (LoginInput, AuthResponse, User)

**Schemas**
- `login.schema.ts`: Schema Zod para validação do formulário

### Fluxo de Dados

```
LoginPage
    ↓
LoginForm (React Hook Form + Zod)
    ↓
onSubmit → useAuth.login()
    ↓
authService.login() → POST /api/auth/barbeiro/login
    ↓ (success)
JWT Token
    ↓
localStorage.setItem('barbapp-barber-token', token)
    ↓
AuthContext.setUser(user)
    ↓
navigate('/barber/schedule')
```

**Fluxo de Validação de Sessão (App Load):**
```
App Mount
    ↓
useAuthValidation()
    ↓
token = localStorage.getItem('barbapp-barber-token')
    ↓ (existe)
authService.validateToken() → GET /api/barber/profile
    ↓ (success)
AuthContext.setUser(user)
    ↓
navigate to requested route
```

## Design de Implementação

### Interfaces Principais

```typescript
// auth.types.ts
export interface LoginInput {
  barbershopCode: string;
  phone: string;
}

export interface AuthResponse {
  token: string;
  user: User;
}

export interface User {
  id: string;
  name: string;
  phone: string;
  role: 'Barbeiro';
  barbershopId?: string; // Pode vir se trabalha em apenas 1 barbearia
}

export interface AuthContextType {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (data: LoginInput) => Promise<void>;
  logout: () => void;
  validateSession: () => Promise<boolean>;
}
```

### Modelos de Dados

**Schema de Validação (Zod)**
```typescript
// login.schema.ts
import { z } from 'zod';

export const loginSchema = z.object({
  barbershopCode: z
    .string()
    .min(1, 'Código da barbearia é obrigatório')
    .min(6, 'Código da barbearia muito curto. Mínimo 6 caracteres')
    .toUpperCase(),
  phone: z
    .string()
    .min(1, 'Telefone é obrigatório')
    .regex(
      /^\(\d{2}\) \d{5}-\d{4}$/,
      'Telefone inválido. Use o formato (XX) XXXXX-XXXX'
    )
});

export type LoginFormData = z.infer<typeof loginSchema>;
```

**Transformação de Telefone**
```typescript
// Função para transformar (11) 99999-9999 em +5511999999999
export function formatPhoneToAPI(phone: string): string {
  const digitsOnly = phone.replace(/\D/g, '');
  return `+55${digitsOnly}`;
}

// Função para aplicar máscara durante digitação
export function applyPhoneMask(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 11);
  
  if (digits.length <= 2) {
    return digits;
  }
  if (digits.length <= 7) {
    return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  }
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}
```

### Componentes React

**LoginPage**
```typescript
// src/pages/auth/LoginPage.tsx
import { LoginForm } from '@/components/auth/LoginForm';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';

export function LoginPage() {
  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Login Barbeiro</CardTitle>
          <CardDescription>
            Entre com seu telefone e código da barbearia
          </CardDescription>
        </CardHeader>
        <CardContent>
          <LoginForm />
          <p className="text-sm text-gray-600 mt-4 text-center">
            Primeiro acesso?{' '}
            <button
              type="button"
              className="text-blue-600 hover:underline"
              onClick={() => {/* Abrir modal de ajuda */}}
            >
              Precisa de ajuda?
            </button>
          </p>
        </CardContent>
      </Card>
    </div>
  );
}
```

**LoginForm**
```typescript
// src/components/auth/LoginForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, type LoginFormData } from '@/schemas/login.schema';
import { useAuth } from '@/hooks/useAuth';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { applyPhoneMask } from '@/lib/phone-utils';
import { Loader2 } from 'lucide-react';
import { toast } from 'sonner';

export function LoginForm() {
  const { login } = useAuth();
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
    setValue,
    watch
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema)
  });
  
  const phoneValue = watch('phone');
  
  // Aplicar máscara ao telefone
  const handlePhoneChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const masked = applyPhoneMask(e.target.value);
    setValue('phone', masked);
  };
  
  const onSubmit = async (data: LoginFormData) => {
    try {
      await login(data);
      // Navegação é feita no hook useAuth
    } catch (error: any) {
      if (error.response?.status === 401) {
        toast.error('Código ou telefone inválidos. Verifique e tente novamente.');
      } else {
        toast.error('Erro ao conectar. Tente novamente em instantes.');
      }
    }
  };
  
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="barbershopCode">Código da Barbearia</Label>
        <Input
          id="barbershopCode"
          {...register('barbershopCode')}
          placeholder="Ex: BARB001"
          disabled={isSubmitting}
          className={errors.barbershopCode ? 'border-red-500' : ''}
        />
        {errors.barbershopCode && (
          <p className="text-sm text-red-600">{errors.barbershopCode.message}</p>
        )}
      </div>
      
      <div className="space-y-2">
        <Label htmlFor="phone">Telefone</Label>
        <Input
          id="phone"
          type="tel"
          inputMode="numeric"
          {...register('phone')}
          onChange={handlePhoneChange}
          placeholder="(11) 99999-9999"
          disabled={isSubmitting}
          className={errors.phone ? 'border-red-500' : ''}
        />
        {errors.phone && (
          <p className="text-sm text-red-600">{errors.phone.message}</p>
        )}
      </div>
      
      <Button
        type="submit"
        className="w-full"
        disabled={isSubmitting}
      >
        {isSubmitting ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Entrando...
          </>
        ) : (
          'Entrar'
        )}
      </Button>
    </form>
  );
}
```

### Hooks e Context

**AuthContext**
```typescript
// src/contexts/AuthContext.tsx
import { createContext, useState, useEffect, ReactNode } from 'react';
import { authService } from '@/services/auth.service';
import { useNavigate } from 'react-router-dom';
import type { AuthContextType, User, LoginInput } from '@/types/auth.types';

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();
  
  // Validar sessão ao carregar app
  useEffect(() => {
    validateSession();
  }, []);
  
  const validateSession = async (): Promise<boolean> => {
    const token = localStorage.getItem('barbapp-barber-token');
    
    if (!token) {
      setIsLoading(false);
      return false;
    }
    
    try {
      const userData = await authService.validateToken();
      setUser(userData);
      setIsLoading(false);
      return true;
    } catch (error) {
      localStorage.removeItem('barbapp-barber-token');
      setUser(null);
      setIsLoading(false);
      return false;
    }
  };
  
  const login = async (data: LoginInput) => {
    const response = await authService.login(data);
    
    localStorage.setItem('barbapp-barber-token', response.token);
    setUser(response.user);
    
    // Redirecionar baseado em número de barbearias
    // Por ora, sempre vai para /barber/schedule
    // (lógica de múltiplas barbearias será implementada no PRD de Agendamentos)
    navigate('/barber/schedule');
  };
  
  const logout = () => {
    localStorage.removeItem('barbapp-barber-token');
    setUser(null);
    navigate('/login');
  };
  
  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        validateSession
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }
  return context;
}
```

**ProtectedRoute**
```typescript
// src/components/auth/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Loader2 } from 'lucide-react';

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Loader2 className="h-8 w-8 animate-spin text-gray-600" />
      </div>
    );
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  return <Outlet />;
}
```

### Services

**auth.service.ts**
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
    const response = await api.get<User>('/barber/profile');
    return response.data;
  },
  
  logout: () => {
    localStorage.removeItem('barbapp-barber-token');
  }
};
```

### Configuração do Axios Interceptor

```typescript
// src/lib/api.ts (atualização)
import axios from 'axios';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api'
});

// Interceptor para adicionar token
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

// Interceptor para tratar 401 (token expirado)
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

### Rotas

```typescript
// src/App.tsx (rotas)
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from '@/contexts/AuthContext';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { LoginPage } from '@/pages/auth/LoginPage';
import { BarberSchedulePage } from '@/pages/barber/SchedulePage';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          
          <Route element={<ProtectedRoute />}>
            <Route path="/barber/schedule" element={<BarberSchedulePage />} />
            {/* Outras rotas protegidas */}
          </Route>
          
          <Route path="/" element={<Navigate to="/login" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
```

## Pontos de Integração

- **Backend de Autenticação**: Depende do endpoint `POST /api/auth/barbeiro/login` já implementado no PRD Multi-tenant
- **Perfil do Barbeiro**: Endpoint `GET /api/barber/profile` deve existir para validação de token
- **Sistema de Agendamentos**: Após login, redireciona para `/barber/schedule` (PRD de Agendamentos)

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|---|---|---|---|
| Rotas da Aplicação | Extensão | Adicionar rotas `/login` e proteger rotas existentes. **Baixo risco**. | Configurar React Router. |
| Estado Global | Nova Funcionalidade | Implementar AuthContext para estado de autenticação. **Baixo risco**. | Criar context e provider. |
| Axios Config | Modificação | Adicionar interceptors para token e 401. **Médio risco** - pode afetar outras requisições. | Testar todas as requisições. |
| localStorage | Uso Novo | Armazenar token JWT. **Baixo risco** - comum em SPAs. | Implementar gestão de token. |

## Abordagem de Testes

### Testes Unitários

- **LoginForm**: Testar validação de campos, máscaras, estados de loading
- **useAuth hook**: Testar login, logout, validação de sessão com mocks
- **authService**: Testar chamadas à API com mock do axios
- **Phone utils**: Testar funções de formatação e máscara

### Testes de Integração

- **Fluxo de Login**: Renderizar LoginPage, preencher formulário, submeter e verificar redirecionamento
- **Persistência de Sessão**: Verificar que token é salvo e recuperado corretamente
- **ProtectedRoute**: Verificar que rota protegida redireciona se não autenticado
- **Token Expirado**: Simular 401 e verificar que usuário é deslogado

### Testes E2E (Playwright)

- **Login bem-sucedido**: Do login até visualização da agenda
- **Login com erro**: Credenciais inválidas mostram mensagem de erro
- **Sessão persistente**: Recarregar página mantém login
- **Logout**: Sair e verificar redirecionamento para login

## Sequenciamento de Desenvolvimento

1. **Setup e Tipos (2h)**:
   - Criar tipos TypeScript (`auth.types.ts`)
   - Criar schema Zod (`login.schema.ts`)
   - Funções utilitárias de telefone

2. **Services e API (2h)**:
   - Implementar `auth.service.ts`
   - Configurar interceptors do Axios
   - Testar com Postman/mock

3. **Context e Hooks (3h)**:
   - Implementar `AuthContext`
   - Implementar `useAuth` hook
   - Testar validação de sessão

4. **Componentes UI (4h)**:
   - Implementar `LoginForm` com validações
   - Implementar `LoginPage`
   - Implementar `ProtectedRoute`
   - Estilizar com Shadcn UI

5. **Rotas e Integração (2h)**:
   - Configurar rotas no React Router
   - Integrar AuthProvider na aplicação
   - Testar fluxo completo

6. **Testes (4h)**:
   - Testes unitários de componentes
   - Testes de integração do fluxo
   - Testes E2E básicos

7. **Refinamento e UX (2h)**:
   - Feedback visual e animações
   - Tratamento de erros refinado
   - Accessibility básica

**Total Estimado**: ~19 horas

## Monitoramento e Observabilidade

- **Logging no Frontend**:
  - Log de tentativas de login (sem dados sensíveis)
  - Log de erros de autenticação
  - Log de sessão expirada

- **Métricas Úteis** (se integrado com analytics):
  - Taxa de sucesso de login
  - Tempo médio de login
  - Taxa de abandono na tela de login
  - Erros mais comuns (401, 500, timeout)

- **Erros a Monitorar**:
  - Falhas de conexão com backend
  - Tokens expirados inesperadamente
  - Erros de validação frequentes

## Considerações Técnicas

### Decisões Principais

- **Armazenamento do Token**: Usar `localStorage` para MVP. É mais simples e funcional. Para produção, considerar `httpOnly cookies` para maior segurança.
- **Validação de Sessão**: Feita ao carregar app e ao acessar rotas protegidas. Evita múltiplas requisições desnecessárias.
- **Tratamento de 401**: Interceptor global redireciona para login automaticamente. Evita lógica duplicada em cada requisição.
- **Formato de Telefone**: Frontend aceita formato brasileiro com máscara, mas envia para API no formato internacional (+55...).

### Riscos Conhecidos

- **XSS e Token no localStorage**: Token no localStorage é vulnerável a XSS. Mitigação: sanitização de inputs, CSP headers, atualização futura para httpOnly cookies.
- **Sessão Única**: Token atual não suporta múltiplos dispositivos de forma inteligente. Se fizer login em novo dispositivo, token antigo ainda funciona até expirar.
- **Validação de Telefone**: Backend deve aceitar formato +55XXXXXXXXXXX. Qualquer divergência causará erro 400/401.

### Conformidade com Padrões

Esta especificação segue todos os padrões definidos em `rules/react.md` e `rules/tests-react.md`, garantindo consistência com o restante do projeto frontend.
