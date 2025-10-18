import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useBarbeariaCode } from '@/hooks/useBarbeariaCode';
import { useBarbearia } from '@/contexts/BarbeariaContext';
import { adminBarbeariaAuthService } from '@/services/adminBarbeariaAuth.service';
import { loginAdminBarbeariaSchema, type LoginAdminBarbeariaFormData } from '@/schemas/adminBarbearia.schema';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/hooks/use-toast';

/**
 * Login page for Admin Barbearia
 * Accessible at /:codigo/login
 * Validates barbershop code, displays barbershop name, and handles authentication
 */
export function LoginAdminBarbearia() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const { codigo, barbeariaInfo, isLoading: isValidatingCode, error: codeError } = useBarbeariaCode();
  const { setBarbearia } = useBarbearia();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginAdminBarbeariaFormData>({
    resolver: zodResolver(loginAdminBarbeariaSchema),
  });

  const loginMutation = useMutation({
    mutationFn: (data: LoginAdminBarbeariaFormData) =>
      adminBarbeariaAuthService.login({
        codigo: codigo!,
        email: data.email,
        senha: data.senha,
      }),
    onSuccess: (response) => {
      // Store barbershop context
      setBarbearia({
        barbeariaId: response.barbeariaId,
        nome: response.nome,
        codigo: response.codigo,
        isActive: true,
      });

      toast({
        title: 'Login realizado com sucesso!',
        description: `Bem-vindo à ${response.nome}!`,
      });

      // Redirect to dashboard
      navigate(`/${codigo}/dashboard`);
    },
    onError: (error: any) => {
      if (error.response?.status === 401) {
        toast({
          title: 'Credenciais inválidas',
          description: 'Email ou senha incorretos. Verifique e tente novamente.',
          variant: 'destructive',
        });
      } else if (error.response?.status === 403) {
        toast({
          title: 'Acesso negado',
          description: 'Você não tem permissão para acessar esta barbearia.',
          variant: 'destructive',
        });
      } else {
        toast({
          title: 'Erro ao fazer login',
          description: 'Ocorreu um erro inesperado. Tente novamente mais tarde.',
          variant: 'destructive',
        });
      }
    },
  });

  const onSubmit = (data: LoginAdminBarbeariaFormData) => {
    loginMutation.mutate(data);
  };

  // Show loading state while validating code
  if (isValidatingCode) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
        <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
          <div className="text-center">
            <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-gray-300 border-t-blue-600"></div>
            <p className="mt-4 text-sm text-gray-600">Validando código da barbearia...</p>
          </div>
        </div>
      </div>
    );
  }

  // Show error if code validation failed
  if (codeError) {
    const is404 = codeError.message.includes('404') || codeError.message.includes('Not Found');
    const is403 = codeError.message.includes('403') || codeError.message.includes('Forbidden');

    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
        <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-gray-900">🪒 BarbApp</h2>
            <div className="mt-6">
              <div className="text-red-600">
                {is404 && (
                  <>
                    <h3 className="text-lg font-medium">Barbearia não encontrada</h3>
                    <p className="mt-2 text-sm">
                      O código da barbearia informado não existe ou é inválido.
                    </p>
                  </>
                )}
                {is403 && (
                  <>
                    <h3 className="text-lg font-medium">Barbearia temporariamente indisponível</h3>
                    <p className="mt-2 text-sm">
                      Esta barbearia está temporariamente desativada.
                    </p>
                  </>
                )}
                {!is404 && !is403 && (
                  <>
                    <h3 className="text-lg font-medium">Erro ao validar código</h3>
                    <p className="mt-2 text-sm">
                      Ocorreu um erro ao validar o código da barbearia.
                    </p>
                  </>
                )}
              </div>
              <Button
                onClick={() => navigate('/')}
                className="mt-6"
                variant="outline"
              >
                Voltar ao início
              </Button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Show error if no barbershop info (shouldn't happen, but safety check)
  if (!barbeariaInfo) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
        <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
          <div className="text-center">
            <h2 className="text-3xl font-bold text-gray-900">🪒 BarbApp</h2>
            <div className="mt-6">
              <div className="text-red-600">
                <h3 className="text-lg font-medium">Código inválido</h3>
                <p className="mt-2 text-sm">
                  O código da barbearia é inválido.
                </p>
              </div>
              <Button
                onClick={() => navigate('/')}
                className="mt-6"
                variant="outline"
              >
                Voltar ao início
              </Button>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
        <div className="text-center">
          <h2 className="text-3xl font-bold text-gray-900">🪒 BarbApp</h2>
          <div className="mt-2">
            <h3 className="text-lg font-medium text-gray-900">{barbeariaInfo.nome}</h3>
            <p className="text-sm text-gray-600">
              Faça login para gerenciar sua barbearia
            </p>
          </div>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          {/* Email Field */}
          <div>
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              autoComplete="email"
              {...register('email')}
              className={errors.email ? 'border-red-500' : ''}
              aria-invalid={errors.email ? 'true' : 'false'}
              aria-describedby={errors.email ? 'email-error' : undefined}
              disabled={loginMutation.isPending}
            />
            {errors.email && (
              <p id="email-error" className="mt-1 text-sm text-red-500">
                {errors.email.message}
              </p>
            )}
          </div>

          {/* Password Field */}
          <div>
            <Label htmlFor="senha">Senha</Label>
            <Input
              id="senha"
              type="password"
              autoComplete="current-password"
              {...register('senha')}
              className={errors.senha ? 'border-red-500' : ''}
              aria-invalid={errors.senha ? 'true' : 'false'}
              aria-describedby={errors.senha ? 'senha-error' : undefined}
              disabled={loginMutation.isPending}
            />
            {errors.senha && (
              <p id="senha-error" className="mt-1 text-sm text-red-500">
                {errors.senha.message}
              </p>
            )}
          </div>

          {/* Submit Button */}
          <Button
            type="submit"
            className="w-full"
            disabled={loginMutation.isPending}
            aria-busy={loginMutation.isPending}
          >
            {loginMutation.isPending ? (
              <>
                <span className="mr-2 inline-block h-4 w-4 animate-spin rounded-full border-2 border-white border-t-transparent" />
                Entrando...
              </>
            ) : (
              'Entrar'
            )}
          </Button>
        </form>

        <div className="text-center">
          <p className="text-sm text-gray-600">
            Esqueceu a senha? Contate o suporte da sua barbearia
          </p>
        </div>
      </div>
    </div>
  );
}