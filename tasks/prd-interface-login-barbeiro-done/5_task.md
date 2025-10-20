---
status: completed
parallelizable: false
blocked_by: ["3.0","4.0"]
---

<task_context>
<domain>engine/frontend/routing</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies></dependencies>
<unblocks>"6.0","7.0"</unblocks>
</task_context>

# Tarefa 5.0: Rotas - ProtectedRoute e Configuração React Router

## Visão Geral
Configurar rotas da aplicação com React Router, implementar componente ProtectedRoute para proteger rotas autenticadas, e integrar AuthProvider na hierarquia da aplicação.

## Requisitos
- Configuração de rotas com React Router v6
- Componente `ProtectedRoute` que verifica autenticação
- Rota `/login` pública
- Rotas `/barber/*` protegidas
- Loading state durante validação de sessão
- Redirect automático baseado em autenticação

## Subtarefas
- [x] 5.1 Criar `src/components/auth/ProtectedRoute.tsx`:
  - Verificar isAuthenticated
  - Mostrar loading durante validação
  - Redirecionar para /login se não autenticado
  - Usar Outlet para renderizar rotas filhas
- [x] 5.2 Configurar rotas em `src/App.tsx`:
  - Envolver com BrowserRouter
  - Adicionar AuthProvider
  - Definir rota pública /login
  - Definir rotas protegidas /barber/*
- [x] 5.3 Adicionar rota raiz (/) com redirect para /login
- [x] 5.4 Testar navegação e proteção de rotas

## Sequenciamento
- Bloqueado por: 3.0 (AuthContext), 4.0 (LoginPage)
- Desbloqueia: 6.0, 7.0
- Paralelizável: Não (integração crítica)

## Detalhes de Implementação

**ProtectedRoute:**
```typescript
// src/components/auth/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
import { Loader2 } from 'lucide-react';

export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <Loader2 className="h-8 w-8 animate-spin text-gray-600 mx-auto mb-4" />
          <p className="text-gray-600">Verificando autenticação...</p>
        </div>
      </div>
    );
  }
  
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }
  
  return <Outlet />;
}
```

**Configuração de Rotas:**
```typescript
// src/App.tsx
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from '@/contexts/AuthContext';
import { ProtectedRoute } from '@/components/auth/ProtectedRoute';
import { LoginPage } from '@/pages/auth/LoginPage';
import { BarberSchedulePage } from '@/pages/barber/SchedulePage';
import { Toaster } from '@/components/ui/sonner';

function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Rota pública */}
          <Route path="/login" element={<LoginPage />} />
          
          {/* Rotas protegidas */}
          <Route element={<ProtectedRoute />}>
            <Route path="/barber/schedule" element={<BarberSchedulePage />} />
            {/* Outras rotas protegidas aqui */}
          </Route>
          
          {/* Redirect raiz para login */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          
          {/* 404 - opcional */}
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
        
        <Toaster />
      </AuthProvider>
    </BrowserRouter>
  );
}

export default App;
```

**Fluxos de Navegação:**

1. **Usuário não autenticado acessa /**:
   - Redirect para `/login`

2. **Usuário não autenticado tenta acessar /barber/schedule**:
   - ProtectedRoute detecta não autenticado
   - Redirect para `/login`

3. **Usuário autenticado acessa /login**:
   - Mostra tela de login (pode adicionar redirect para /barber/schedule se já logado)

4. **Usuário autenticado acessa /barber/schedule**:
   - ProtectedRoute valida sessão
   - Renderiza página normalmente

5. **Token expira durante uso**:
   - Interceptor Axios detecta 401
   - Remove token e redireciona para /login

## Critérios de Sucesso
- Rotas públicas acessíveis sem autenticação
- Rotas protegidas redirecionam para login se não autenticado
- Loading state aparece durante validação
- Navegação funciona após login
- Logout redireciona para login
- Token expirado aciona redirect
- Não há loops infinitos de redirect
- Testes de navegação passam
