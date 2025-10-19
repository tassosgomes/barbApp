---
status: pending
parallelizable: true
blocked_by: ["1.0","3.0"]
---

<task_context>
<domain>engine/frontend/components</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies></dependencies>
<unblocks>"5.0","6.0"</unblocks>
</task_context>

# Tarefa 4.0: Componentes UI - LoginForm e LoginPage

## Visão Geral
Criar os componentes visuais da tela de login: formulário com validação em tempo real, máscaras de input, feedback de loading e erros, e página completa com instruções de ajuda.

## Requisitos
- Componente `LoginForm` com React Hook Form + Zod
- Componente `LoginPage` com layout e card
- Máscara de telefone aplicada durante digitação
- Estados de loading e erro visuais
- Modal/texto de ajuda para primeiro acesso
- Design mobile-first com Shadcn UI

## Subtarefas
- [ ] 4.1 Criar `src/components/auth/LoginForm.tsx`:
  - Integração com React Hook Form
  - Validação com schema Zod
  - Aplicação de máscara no campo telefone
  - Estados de loading e erro
  - Integração com useAuth().login()
- [ ] 4.2 Criar `src/pages/auth/LoginPage.tsx`:
  - Layout centralizado
  - Card com título e descrição
  - Inclusão do LoginForm
  - Texto de ajuda e link
- [ ] 4.3 Criar `src/components/auth/HelpModal.tsx` (opcional):
  - Modal com instruções para primeiro acesso
  - Informações sobre código da barbearia
- [ ] 4.4 Estilizar com Shadcn UI e Tailwind
- [ ] 4.5 Testar responsividade mobile

## Sequenciamento
- Bloqueado por: 1.0 (Tipos, Schemas), 3.0 (useAuth)
- Desbloqueia: 5.0, 6.0
- Paralelizável: Sim (pode começar com useAuth mockado)

## Detalhes de Implementação

**LoginForm:**
```typescript
// src/components/auth/LoginForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, type LoginFormData } from '@/schemas/login.schema';
import { useAuth } from '@/contexts/AuthContext';
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
      // Navegação feita pelo useAuth
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

**LoginPage:**
```typescript
// src/pages/auth/LoginPage.tsx
import { LoginForm } from '@/components/auth/LoginForm';
import { Card, CardHeader, CardTitle, CardDescription, CardContent } from '@/components/ui/card';
import { useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from '@/components/ui/dialog';

export function LoginPage() {
  const [showHelp, setShowHelp] = useState(false);
  
  return (
    <div className="min-h-screen flex items-center justify-center p-4 bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle className="text-2xl">Login Barbeiro</CardTitle>
          <CardDescription>
            Entre com seu telefone e código da barbearia
          </CardDescription>
        </CardHeader>
        <CardContent>
          <LoginForm />
          
          <p className="text-sm text-gray-600 mt-6 text-center">
            Primeiro acesso?{' '}
            <button
              type="button"
              className="text-blue-600 hover:underline focus:outline-none"
              onClick={() => setShowHelp(true)}
            >
              Precisa de ajuda?
            </button>
          </p>
        </CardContent>
      </Card>
      
      <Dialog open={showHelp} onOpenChange={setShowHelp}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Como fazer login</DialogTitle>
            <DialogDescription className="space-y-2 pt-4">
              <p>• O código da barbearia é fornecido pelo administrador da sua barbearia</p>
              <p>• Use seu número de telefone cadastrado para entrar</p>
              <p>• Em caso de dúvidas, contate o administrador</p>
            </DialogDescription>
          </DialogHeader>
        </DialogContent>
      </Dialog>
    </div>
  );
}
```

**Componentes Shadcn UI necessários:**
- Card, CardHeader, CardTitle, CardDescription, CardContent
- Input
- Label
- Button
- Dialog (para ajuda)
- Toast/Sonner (para mensagens de erro)

## Critérios de Sucesso
- Formulário renderiza corretamente
- Validação Zod funciona em tempo real
- Máscara de telefone aplica durante digitação
- Estados de loading mostram spinner e desabilitam campos
- Erros são exibidos abaixo dos campos
- Toast aparece para erros de API
- Modal de ajuda abre e fecha corretamente
- Design é responsivo e mobile-first
- Segue regras de `rules/react.md`
