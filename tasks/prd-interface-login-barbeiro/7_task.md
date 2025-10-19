---
status: pending
parallelizable: true
blocked_by: ["4.0","5.0","6.0"]
---

<task_context>
<domain>engine/frontend/ux</domain>
<type>implementation</type>
<scope>polish</scope>
<complexity>low</complexity>
<dependencies></dependencies>
<unblocks></unblocks>
</task_context>

# Tarefa 7.0: Refinamento - UX, Feedback Visual e Acessibilidade

## Visão Geral
Refinar a experiência do usuário com animações suaves, transições, feedback visual aprimorado, mensagens de erro contextuais e melhorias de acessibilidade básica.

## Requisitos
- Transições suaves entre estados (loading, erro, sucesso)
- Animações de entrada/saída
- Mensagens de erro contextuais e acionáveis
- Indicadores de progresso claros
- Acessibilidade básica (ARIA labels, keyboard navigation)
- Otimizações mobile (touch targets, viewport)

## Subtarefas
- [ ] 7.1 Adicionar transições CSS/animações:
  - Fade in da página de login
  - Transição de loading no botão
  - Shake animation em erro de validação
- [ ] 7.2 Melhorar mensagens de erro:
  - Texto específico por tipo de erro
  - Ícones visuais (warning, error)
  - Sugestões de correção quando possível
- [ ] 7.3 Adicionar feedback visual adicional:
  - Toast de sucesso após login (opcional)
  - Progress indicator durante validação de sessão
  - Skeleton loader (opcional)
- [ ] 7.4 Melhorias de acessibilidade:
  - Adicionar aria-labels onde necessário
  - Garantir navegação por teclado
  - Contraste de cores adequado
  - Anúncios de erro com aria-live
- [ ] 7.5 Otimizações mobile:
  - Verificar touch targets (mínimo 44x44px)
  - Prevenir zoom automático (font-size >= 16px)
  - Testar em diferentes tamanhos de tela
- [ ] 7.6 Adicionar estados vazios/placeholder:
  - Texto explicativo se necessário
  - Ícones ilustrativos

## Sequenciamento
- Bloqueado por: 4.0 (Componentes), 5.0 (Rotas), 6.0 (Testes)
- Desbloqueia: —
- Paralelizável: Sim

## Detalhes de Implementação

**Animações CSS:**
```css
/* src/styles/animations.css */
@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  25% { transform: translateX(-10px); }
  75% { transform: translateX(10px); }
}

.fade-in {
  animation: fadeIn 0.3s ease-out;
}

.shake {
  animation: shake 0.3s ease-out;
}
```

**Mensagens de Erro Contextuais:**
```typescript
// src/lib/auth-errors.ts
export function getAuthErrorMessage(error: any): string {
  const status = error.response?.status;
  
  switch (status) {
    case 400:
      return 'Dados inválidos. Verifique o código e telefone.';
    case 401:
      return 'Código ou telefone incorretos. Verifique e tente novamente.';
    case 404:
      return 'Barbearia não encontrada. Confirme o código com seu gerente.';
    case 500:
      return 'Erro no servidor. Tente novamente em instantes.';
    case 503:
      return 'Serviço temporariamente indisponível. Tente em alguns minutos.';
    default:
      if (!navigator.onLine) {
        return 'Sem conexão com a internet. Verifique sua conexão.';
      }
      return 'Erro ao conectar. Tente novamente.';
  }
}
```

**Melhorias de Acessibilidade:**
```typescript
// LoginForm com melhorias a11y
<form onSubmit={handleSubmit(onSubmit)} className="space-y-4" noValidate>
  <div className="space-y-2">
    <Label htmlFor="barbershopCode">
      Código da Barbearia
      <span className="sr-only">(obrigatório)</span>
    </Label>
    <Input
      id="barbershopCode"
      {...register('barbershopCode')}
      placeholder="Ex: BARB001"
      disabled={isSubmitting}
      className={errors.barbershopCode ? 'border-red-500' : ''}
      aria-invalid={!!errors.barbershopCode}
      aria-describedby={errors.barbershopCode ? 'code-error' : undefined}
    />
    {errors.barbershopCode && (
      <p 
        id="code-error" 
        className="text-sm text-red-600"
        role="alert"
        aria-live="polite"
      >
        {errors.barbershopCode.message}
      </p>
    )}
  </div>
  
  {/* Similar para telefone */}
  
  <Button
    type="submit"
    className="w-full min-h-[44px]" // Touch target adequado
    disabled={isSubmitting}
    aria-busy={isSubmitting}
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
```

**Loading State Melhorado:**
```typescript
// ProtectedRoute com skeleton
export function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();
  
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="text-center space-y-4">
          <div className="relative">
            <Loader2 className="h-12 w-12 animate-spin text-blue-600 mx-auto" />
            <div className="absolute inset-0 h-12 w-12 rounded-full border-4 border-blue-200 mx-auto" />
          </div>
          <p className="text-gray-600 font-medium">Verificando autenticação</p>
          <p className="text-sm text-gray-500">Aguarde um momento...</p>
        </div>
      </div>
    );
  }
  
  // ... resto
}
```

**Otimizações Mobile:**
```css
/* Garantir que inputs não causem zoom no iOS */
input[type="text"],
input[type="tel"] {
  font-size: 16px; /* Mínimo para evitar zoom no iOS */
}

/* Touch targets adequados */
button,
a {
  min-height: 44px;
  min-width: 44px;
}

/* Ajustes de viewport */
@media (max-width: 640px) {
  .login-card {
    margin: 1rem;
    max-width: 100%;
  }
}
```

## Critérios de Sucesso
- Animações são suaves e não afetam performance
- Mensagens de erro são claras e acionáveis
- Navegação por teclado funciona completamente
- Touch targets são adequados (≥44x44px)
- Contraste de cores passa WCAG AA
- Não há zoom automático no iOS
- Screen readers anunciam erros corretamente
- Experiência é fluida em mobile e desktop
- Testes de acessibilidade básicos passam
