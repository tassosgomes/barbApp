/**
 * EXEMPLOS DE USO - Componentes de Login
 * 
 * Este arquivo demonstra como utilizar os componentes de autenticação
 * criados na Task 4.0 do PRD Interface de Login Barbeiro.
 */

// ===================================================================
// EXEMPLO 1: Configuração Básica das Rotas
// ===================================================================

import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from '@/contexts/AuthContext';
import { LoginPage } from '@/pages/auth/LoginPage';
import { Toaster } from '@/components/ui/toaster';

export function AppExample() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          {/* Outras rotas protegidas aqui */}
        </Routes>
        
        {/* Toaster para exibir mensagens de erro/sucesso */}
        <Toaster />
      </AuthProvider>
    </BrowserRouter>
  );
}

// ===================================================================
// EXEMPLO 2: Uso do LoginForm em Outra Página
// ===================================================================

import { LoginForm } from '@/components/auth/LoginForm';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

export function CustomLoginPage() {
  return (
    <div className="min-h-screen bg-slate-900 flex items-center justify-center p-4">
      <Card className="w-full max-w-lg">
        <CardHeader>
          <CardTitle>Área Restrita - Barbeiros</CardTitle>
        </CardHeader>
        <CardContent>
          <LoginForm />
        </CardContent>
      </Card>
    </div>
  );
}

// ===================================================================
// EXEMPLO 3: LoginPage com Logo Customizado
// ===================================================================

import { useState } from 'react';
import { LoginForm } from '@/components/auth/LoginForm';
import {
  Card,
  CardHeader,
  CardTitle,
  CardDescription,
  CardContent,
} from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

export function BrandedLoginPage() {
  const [showHelp, setShowHelp] = useState(false);

  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-gradient-to-br from-blue-50 to-gray-100">
      <div className="w-full max-w-md space-y-4">
        {/* Logo */}
        <div className="text-center mb-8">
          <img
            src="/logo.svg"
            alt="Logo BarbApp"
            className="h-16 mx-auto"
          />
          <h1 className="text-2xl font-bold mt-4 text-gray-800">BarbApp</h1>
        </div>

        {/* Card de Login */}
        <Card>
          <CardHeader>
            <CardTitle>Login Barbeiro</CardTitle>
            <CardDescription>
              Entre com suas credenciais para acessar o sistema
            </CardDescription>
          </CardHeader>

          <CardContent className="space-y-6">
            <LoginForm />

            <div className="text-center">
              <p className="text-sm text-gray-600">
                Primeiro acesso?{' '}
                <button
                  type="button"
                  className="text-blue-600 hover:underline focus:outline-none"
                  onClick={() => setShowHelp(true)}
                >
                  Precisa de ajuda?
                </button>
              </p>
            </div>
          </CardContent>
        </Card>

        {/* Footer */}
        <p className="text-center text-sm text-gray-500">
          © 2025 BarbApp. Todos os direitos reservados.
        </p>
      </div>

      {/* Modal de Ajuda */}
      <Dialog open={showHelp} onOpenChange={setShowHelp}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Como fazer login</DialogTitle>
            <DialogDescription className="space-y-3 pt-4">
              <p>
                <strong>E-mail:</strong> Use o e-mail cadastrado pelo
                administrador da sua barbearia.
              </p>
              <p>
                <strong>Senha:</strong> Use a senha fornecida. Você pode
                alterá-la após o primeiro acesso.
              </p>
              <p>
                <strong>Problemas?</strong> Entre em contato com o
                administrador.
              </p>
            </DialogDescription>
          </DialogHeader>
          <Button onClick={() => setShowHelp(false)} className="w-full">
            Entendi
          </Button>
        </DialogContent>
      </Dialog>
    </div>
  );
}

// ===================================================================
// EXEMPLO 4: Rota Protegida (Protected Route)
// ===================================================================

import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/contexts/AuthContext';
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

// Uso nas rotas:
export function AppWithProtectedRoutes() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          {/* Rota pública */}
          <Route path="/login" element={<LoginPage />} />

          {/* Rotas protegidas */}
          <Route element={<ProtectedRoute />}>
            <Route path="/barber/schedule" element={<div>Agenda</div>} />
            <Route path="/barber/profile" element={<div>Perfil</div>} />
          </Route>

          {/* Redirect padrão */}
          <Route path="/" element={<Navigate to="/login" replace />} />
        </Routes>

        <Toaster />
      </AuthProvider>
    </BrowserRouter>
  );
}

// ===================================================================
// EXEMPLO 5: Tratamento de Erros Customizado
// ===================================================================

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barberLoginSchema, type BarberLoginFormData } from '@/schemas/login.schema';
import { useAuth } from '@/contexts/AuthContext';
import { useToast } from '@/hooks/use-toast';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Loader2, AlertCircle } from 'lucide-react';

export function LoginFormWithCustomErrors() {
  const { login } = useAuth();
  const { toast } = useToast();
  const [serverError, setServerError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BarberLoginFormData>({
    resolver: zodResolver(barberLoginSchema),
  });

  const onSubmit = async (data: BarberLoginFormData) => {
    setServerError(null);

    try {
      await login(data);
    } catch (error: any) {
      // Exibir erro inline ao invés de toast
      if (error.response?.status === 401) {
        setServerError('E-mail ou senha incorretos. Verifique suas credenciais.');
      } else if (error.response?.status === 400) {
        setServerError('Dados inválidos. Verifique os campos e tente novamente.');
      } else {
        setServerError('Erro de conexão. Tente novamente em instantes.');
      }
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      {/* Alerta de erro do servidor */}
      {serverError && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{serverError}</AlertDescription>
        </Alert>
      )}

      {/* Campo de E-mail */}
      <div className="space-y-2">
        <Label htmlFor="email">E-mail</Label>
        <Input
          id="email"
          type="email"
          {...register('email')}
          placeholder="seu.email@exemplo.com"
          disabled={isSubmitting}
          className={errors.email ? 'border-red-500' : ''}
        />
        {errors.email && (
          <p className="text-sm text-red-600">{errors.email.message}</p>
        )}
      </div>

      {/* Campo de Senha */}
      <div className="space-y-2">
        <Label htmlFor="password">Senha</Label>
        <Input
          id="password"
          type="password"
          {...register('password')}
          placeholder="••••••"
          disabled={isSubmitting}
          className={errors.password ? 'border-red-500' : ''}
        />
        {errors.password && (
          <p className="text-sm text-red-600">{errors.password.message}</p>
        )}
      </div>

      {/* Botão de Submit */}
      <Button type="submit" className="w-full" disabled={isSubmitting}>
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

// ===================================================================
// EXEMPLO 6: Integração com React Query (Opcional)
// ===================================================================

import { useMutation } from '@tanstack/react-query';
import { authService } from '@/services/auth.service';

export function LoginFormWithReactQuery() {
  const navigate = useNavigate();
  const { toast } = useToast();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<BarberLoginFormData>({
    resolver: zodResolver(barberLoginSchema),
  });

  const loginMutation = useMutation({
    mutationFn: (data: BarberLoginFormData) => authService.login(data),
    onSuccess: (response) => {
      localStorage.setItem('barbapp-barber-token', response.token);
      toast({
        title: 'Login realizado!',
        description: 'Redirecionando para sua agenda...',
      });
      navigate('/barber/schedule');
    },
    onError: (error: any) => {
      if (error.response?.status === 401) {
        toast({
          variant: 'destructive',
          title: 'Erro de autenticação',
          description: 'E-mail ou senha inválidos.',
        });
      }
    },
  });

  return (
    <form onSubmit={handleSubmit((data) => loginMutation.mutate(data))} className="space-y-4">
      {/* Campos do formulário... */}
      <Button type="submit" disabled={loginMutation.isPending} className="w-full">
        {loginMutation.isPending ? 'Entrando...' : 'Entrar'}
      </Button>
    </form>
  );
}
