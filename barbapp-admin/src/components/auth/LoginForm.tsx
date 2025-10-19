import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barberLoginSchema, type BarberLoginFormData } from '@/schemas/login.schema';
import { useAuth } from '@/contexts/AuthContext';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Loader2 } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';

/**
 * Formulário de login para barbeiros
 * 
 * Utiliza React Hook Form com validação Zod para autenticação via email+password.
 * Integra com o AuthContext para realizar o login e gerenciar o estado de autenticação.
 * 
 * Features:
 * - Validação em tempo real com mensagens de erro claras
 * - Estados de loading durante autenticação
 * - Feedback visual de erros da API
 * - Desabilitação de campos durante submit
 * 
 * @example
 * ```tsx
 * <LoginForm />
 * ```
 */
export function LoginForm() {
  const { login } = useAuth();
  const { toast } = useToast();
  
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BarberLoginFormData>({
    resolver: zodResolver(barberLoginSchema),
  });
  
  const onSubmit = async (data: BarberLoginFormData) => {
    try {
      await login(data);
      // Navegação é feita automaticamente pelo useAuth
    } catch (error: any) {
      // Tratamento de erros baseado no status HTTP
      if (error.response?.status === 401) {
        toast({
          variant: 'destructive',
          title: 'Erro de autenticação',
          description: 'E-mail ou senha inválidos. Verifique e tente novamente.',
        });
      } else if (error.response?.status === 400) {
        toast({
          variant: 'destructive',
          title: 'Dados inválidos',
          description: 'Verifique se os dados estão corretos e tente novamente.',
        });
      } else {
        toast({
          variant: 'destructive',
          title: 'Erro de conexão',
          description: 'Não foi possível conectar ao servidor. Tente novamente em instantes.',
        });
      }
    }
  };
  
  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4" data-testid="login-form">
      {/* Campo de E-mail */}
      <div className="space-y-2">
        <Label htmlFor="email">E-mail</Label>
        <Input
          id="email"
          type="email"
          autoComplete="email"
          {...register('email')}
          placeholder="seu.email@exemplo.com"
          disabled={isSubmitting}
          className={errors.email ? 'border-red-500' : ''}
          data-testid="email-input"
        />
        {errors.email && (
          <p className="text-sm text-red-600" role="alert" data-testid="email-error">
            {errors.email.message}
          </p>
        )}
      </div>
      
      {/* Campo de Senha */}
      <div className="space-y-2">
        <Label htmlFor="password">Senha</Label>
        <Input
          id="password"
          type="password"
          autoComplete="current-password"
          {...register('password')}
          placeholder="••••••"
          disabled={isSubmitting}
          className={errors.password ? 'border-red-500' : ''}
          data-testid="password-input"
        />
        {errors.password && (
          <p className="text-sm text-red-600" role="alert" data-testid="password-error">
            {errors.password.message}
          </p>
        )}
      </div>
      
      {/* Botão de Submit */}
      <Button
        type="submit"
        className="w-full"
        disabled={isSubmitting}
        data-testid="submit-button"
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
