---
status: completed
parallelizable: false
blocked_by: ["1.0","2.0"]
---

<task_context>
<domain>engine/frontend/state</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies></dependencies>
<unblocks>"4.0","5.0"</unblocks>
</task_context>

# Tarefa 3.0: Context e Hooks - AuthContext e useAuth

## Visão Geral
Implementar Context API para gestão global do estado de autenticação e hook customizado `useAuth` para facilitar acesso às funcionalidades de autenticação em qualquer componente.

## Requisitos
- `AuthContext` com estado de usuário, loading e funções de autenticação
- Hook `useAuth` para consumir o context
- Validação automática de sessão ao carregar aplicação
- Persistência e recuperação de token do localStorage
- Navegação automática após login/logout

## Subtarefas
- [x] 3.1 Criar `src/contexts/AuthContext.tsx`:
  - Estado: user, isLoading
  - Funções: login, logout, validateSession
  - useEffect para validar sessão ao montar
- [x] 3.2 Exportar hook `useAuth()` no mesmo arquivo
- [x] 3.3 Integrar com `authService`
- [x] 3.4 Implementar lógica de navegação (useNavigate)
- [x] 3.5 Testes do hook com renderHook

## Sequenciamento
- Bloqueado por: 1.0 (Tipos), 2.0 (Services)
- Desbloqueia: 4.0, 5.0
- Paralelizável: Não (dependência crítica)

## Detalhes de Implementação

**AuthContext:**
```typescript
// src/contexts/AuthContext.tsx
import { createContext, useState, useEffect, useContext, ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { authService } from '@/services/auth.service';
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
    
    // Redirecionar para agenda
    navigate('/barber/schedule');
  };
  
  const logout = () => {
    authService.logout();
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

**Uso do Hook:**
```typescript
// Em qualquer componente
import { useAuth } from '@/contexts/AuthContext';

function MyComponent() {
  const { user, isAuthenticated, login, logout } = useAuth();
  
  // Usar as funções e estado
}
```

**Provider na Aplicação:**
```typescript
// src/App.tsx
import { BrowserRouter } from 'react-router-dom';
import { AuthProvider } from '@/contexts/AuthContext';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        {/* Rotas aqui */}
      </AuthProvider>
    </BrowserRouter>
  );
}
```

## Critérios de Sucesso
- Context provê estado e funções corretamente
- Hook useAuth funciona em qualquer componente dentro do Provider
- Validação de sessão ocorre ao carregar app
- Token é persistido e recuperado corretamente
- Navegação automática funciona após login/logout
- Testes cobrem os cenários principais
