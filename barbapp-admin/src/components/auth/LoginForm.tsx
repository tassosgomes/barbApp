import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barberLoginSchema, type BarberLoginFormData } from '@/schemas/login.schema';
import { useAuth } from '@/contexts/AuthContext';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { Loader2 } from 'lucide-react';
import { useToast } from '@/hooks/use-toast';
import { getAuthErrorToast } from '@/lib/auth-errors';
import { useState } from 'react';

/**
 * Formulário de login para barbeiros
 * 
 * Utiliza React Hook Form com validação Zod para autenticação via email+password.
 * Integra com o AuthContext para realizar o login e gerenciar o estado de autenticação.
 * 
 * Features:
 * - Validação em tempo real com mensagens de erro claras
 * - Estados de loading durante autenticação
 * - Feedback visual de erros da API com mensagens contextuais
 * - Desabilitação de campos durante submit
 * - Animações de erro (shake) para feedback visual
 * - Acessibilidade completa (ARIA labels, keyboard navigation)
 * 
 * @example
 * ```tsx
 * <LoginForm />
 * ```
 */
export function LoginForm() {
  const { login } = useAuth();
  const { toast } = useToast();
  const [hasError, setHasError] = useState(false);
  
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<BarberLoginFormData>({
    resolver: zodResolver(barberLoginSchema),
  });
  
  const onSubmit = async (data: BarberLoginFormData) => {
    try {
      setHasError(false);
      await login(data);
      // Navegação é feita automaticamente pelo useAuth
    } catch (error: any) {
      // Trigger shake animation
      setHasError(true);
      setTimeout(() => setHasError(false), 300);
      
      // Mostra mensagem de erro contextual
      const { title, description } = getAuthErrorToast(error);
      toast({
        variant: 'destructive',
        title,
        description,
      });
    }
  };
  
  return (
    <form 
      onSubmit={handleSubmit(onSubmit)} 
      className={`space-y-4 ${hasError ? 'shake' : ''}`}
      data-testid="login-form"
      noValidate
    >
      {/* Campo de E-mail */}
      <div className="space-y-2">
        <Label htmlFor="email">
          E-mail
          <span className="sr-only">(obrigatório)</span>
        </Label>
        <Input
          id="email"
          type="email"
          autoComplete="email"
          {...register('email')}
          placeholder="seu.email@exemplo.com"
          disabled={isSubmitting}
          className={`transition-smooth ${errors.email ? 'border-red-500 focus:ring-red-500' : ''}`}
          style={{ fontSize: '16px' }} // Previne zoom no iOS
          aria-invalid={!!errors.email}
          aria-describedby={errors.email ? 'email-error' : undefined}
          data-testid="email-input"
        />
        {errors.email && (
          <p 
            id="email-error"
            className="text-sm text-red-600" 
            role="alert" 
            aria-live="polite"
            data-testid="email-error"
          >
            {errors.email.message}
          </p>
        )}
      </div>
      
      {/* Campo de Senha */}
      <div className="space-y-2">
        <Label htmlFor="password">
          Senha
          <span className="sr-only">(obrigatório)</span>
        </Label>
        <Input
          id="password"
          type="password"
          autoComplete="current-password"
          {...register('password')}
          placeholder="••••••"
          disabled={isSubmitting}
          className={`transition-smooth ${errors.password ? 'border-red-500 focus:ring-red-500' : ''}`}
          style={{ fontSize: '16px' }} // Previne zoom no iOS
          aria-invalid={!!errors.password}
          aria-describedby={errors.password ? 'password-error' : undefined}
          data-testid="password-input"
        />
        {errors.password && (
          <p 
            id="password-error"
            className="text-sm text-red-600" 
            role="alert"
            aria-live="polite"
            data-testid="password-error"
          >
            {errors.password.message}
          </p>
        )}
      </div>
      
      {/* Botão de Submit */}
      <Button
        type="submit"
        className="w-full min-h-[44px] transition-smooth" // Touch target adequado
        disabled={isSubmitting}
        aria-busy={isSubmitting}
        data-testid="submit-button"
      >
        {isSubmitting ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
            <span>Entrando...</span>
          </>
        ) : (
          'Entrar'
        )}
      </Button>
    </form>
  );
}
