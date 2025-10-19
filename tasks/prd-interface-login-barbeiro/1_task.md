---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>engine/frontend/types</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies></dependencies>
<unblocks>"2.0","3.0","4.0"</unblocks>
</task_context>

# Tarefa 1.0: Setup - Tipos TypeScript, Schemas Zod e Utilitários

## Visão Geral
Criar a base de tipos TypeScript para autenticação, schemas Zod para validação do formulário de login, e funções utilitárias para formatação e máscara de telefone.

## Requisitos
- Tipos TypeScript para Login, AuthResponse, User
- Schema Zod para validação de formulário
- Funções de formatação de telefone (máscara e conversão para API)
- Exports centralizados

## Subtarefas
- [ ] 1.1 Criar `src/types/auth.types.ts`:
  - Interface `LoginInput`
  - Interface `AuthResponse`
  - Interface `User`
  - Interface `AuthContextType`
- [ ] 1.2 Criar `src/schemas/login.schema.ts`:
  - Schema Zod com validações
  - Mensagens de erro personalizadas
  - Export do tipo inferido
- [ ] 1.3 Criar `src/lib/phone-utils.ts`:
  - `applyPhoneMask(value: string): string` - aplica máscara durante digitação
  - `formatPhoneToAPI(phone: string): string` - converte para formato API (+55...)
  - Testes unitários das funções
- [ ] 1.4 Exportar via `src/types/index.ts`

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0
- Paralelizável: Não (base para outras tarefas)

## Detalhes de Implementação

**Tipos principais:**
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
  barbershopId?: string;
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

**Schema Zod:**
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

**Phone Utils:**
```typescript
// phone-utils.ts
export function applyPhoneMask(value: string): string {
  const digits = value.replace(/\D/g, '').slice(0, 11);
  
  if (digits.length <= 2) return digits;
  if (digits.length <= 7) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
  return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}

export function formatPhoneToAPI(phone: string): string {
  const digitsOnly = phone.replace(/\D/g, '');
  return `+55${digitsOnly}`;
}
```

## Critérios de Sucesso
- Tipos compilam sem erros
- Schema Zod valida corretamente
- Funções de telefone testadas e funcionando
- Exports acessíveis para outros componentes
