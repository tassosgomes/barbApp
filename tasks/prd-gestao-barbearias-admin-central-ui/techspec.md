# Especificação Técnica: Sistema de Gestão de Barbearias - Admin Central (Frontend)

**Documento gerado por:** Claude Code - SuperClaude Framework
**Data:** 2025-10-13
**Versão:** 1.0
**Tipo:** Frontend MVP
**Stack:** React Vite + TypeScript + TailwindCSS + shadcn/ui

---

## 1. Visão Geral Técnica

### 1.1 Resumo
Interface administrativa SPA (Single Page Application) para gestão completa de barbearias, incluindo operações de CRUD, autenticação e controle de status.

### 1.2 Objetivos Técnicos
- Interface responsiva e acessível (mobile-first)
- Integração completa com API REST existente
- Validação robusta client-side com feedback em tempo real
- Cobertura de testes >70%
- Performance otimizada (bundle inicial <500KB)

### 1.3 Escopo da Solução
**Frontend APENAS** - Backend já implementado em /src com .NET Core

---

## 2. Arquitetura da Solução

### 2.1 Estrutura de Projeto
```
barbapp-admin/
├── public/
├── src/
│   ├── assets/                    # Imagens, ícones
│   ├── components/
│   │   ├── ui/                    # shadcn/ui components
│   │   ├── layout/                # Header, Sidebar, Footer
│   │   └── barbershop/            # Componentes específicos
│   │       ├── BarbershopTable.tsx
│   │       ├── BarbershopForm.tsx
│   │       ├── BarbershopCard.tsx
│   │       └── DeactivateModal.tsx
│   ├── pages/
│   │   ├── Login/
│   │   │   └── Login.tsx
│   │   └── Barbershops/
│   │       ├── List.tsx
│   │       ├── Create.tsx
│   │       ├── Edit.tsx
│   │       └── Details.tsx
│   ├── services/
│   │   ├── api.ts                 # Axios config
│   │   └── barbershop.service.ts
│   ├── hooks/
│   │   ├── useAuth.ts
│   │   ├── useBarbershops.ts
│   │   └── usePagination.ts
│   ├── types/
│   │   ├── barbershop.ts
│   │   ├── pagination.ts
│   │   └── auth.ts
│   ├── schemas/
│   │   └── barbershop.schema.ts   # Zod validation
│   ├── utils/
│   │   ├── errorHandler.ts
│   │   ├── formatters.ts
│   │   └── validators.ts
│   ├── routes/
│   │   └── index.tsx
│   ├── App.tsx
│   └── main.tsx
├── package.json
├── tsconfig.json
├── vite.config.ts
├── tailwind.config.js
└── .env.example
```

### 2.2 Stack Tecnológica

| Categoria | Tecnologia | Versão | Propósito |
|-----------|-----------|--------|-----------|
| Framework | React | 18+ | UI library |
| Build Tool | Vite | 5+ | Dev server & bundler |
| Linguagem | TypeScript | 5+ | Type safety |
| Estilização | TailwindCSS | 3.4+ | Utility-first CSS |
| Componentes | shadcn/ui | latest | Component library |
| Roteamento | React Router | v6 | Client-side routing |
| HTTP Client | Axios | 1.6+ | API requests |
| Formulários | React Hook Form | 7+ | Form management |
| Validação | Zod | 3+ | Schema validation |
| Testes Unitários | Vitest | 1+ | Unit testing |
| Testes E2E | Playwright | 1+ | End-to-end testing |

### 2.3 Fluxo de Dados
```
UI Component
    ↓ (user action)
Custom Hook (useBarbershops)
    ↓ (call service)
Service Layer (barbershop.service.ts)
    ↓ (HTTP request via Axios)
API Backend (/api/barbearias)
    ↓ (HTTP response)
Service Layer
    ↓ (update state)
Custom Hook
    ↓ (re-render)
UI Component (updated)
```

---

## 3. Integração com API Backend

### 3.1 Configuração Base do Axios

```typescript
// src/services/api.ts
import axios from 'axios';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5000/api',
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor: Adicionar token JWT
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('auth_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Interceptor: Tratar erros globais
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('auth_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

### 3.2 Service de Barbearias

```typescript
// src/services/barbershop.service.ts
import api from './api';
import type {
  Barbershop,
  CreateBarbershopRequest,
  UpdateBarbershopRequest,
  PaginatedResponse,
  BarbershopFilters
} from '@/types/barbershop';

export const barbershopService = {
  /**
   * Listar barbearias com paginação e filtros
   * Endpoint: GET /api/barbearias
   */
  getAll: async (filters: BarbershopFilters): Promise<PaginatedResponse<Barbershop>> => {
    const { data } = await api.get<PaginatedResponse<Barbershop>>('/barbearias', {
      params: filters
    });
    return data;
  },

  /**
   * Obter barbearia por ID
   * Endpoint: GET /api/barbearias/{id}
   */
  getById: async (id: string): Promise<Barbershop> => {
    const { data } = await api.get<Barbershop>(`/barbearias/${id}`);
    return data;
  },

  /**
   * Criar nova barbearia
   * Endpoint: POST /api/barbearias
   */
  create: async (request: CreateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.post<Barbershop>('/barbearias', request);
    return data;
  },

  /**
   * Atualizar barbearia existente
   * Endpoint: PUT /api/barbearias/{id}
   */
  update: async (id: string, request: UpdateBarbershopRequest): Promise<Barbershop> => {
    const { data } = await api.put<Barbershop>(`/barbearias/${id}`, request);
    return data;
  },

  /**
   * Desativar barbearia (soft delete)
   * Endpoint: PUT /api/barbearias/{id}/desativar
   */
  deactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/desativar`);
  },

  /**
   * Reativar barbearia
   * Endpoint: PUT /api/barbearias/{id}/reativar
   */
  reactivate: async (id: string): Promise<void> => {
    await api.put(`/barbearias/${id}/reativar`);
  },
};
```

### 3.3 Tipos TypeScript

```typescript
// src/types/barbershop.ts
export interface Barbershop {
  id: string;
  name: string;
  email: string;
  phone: string;
  address: Address;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface Address {
  street: string;
  number: string;
  complement?: string;
  neighborhood: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface CreateBarbershopRequest {
  name: string;
  email: string;
  phone: string;
  address: Address;
}

export interface UpdateBarbershopRequest extends CreateBarbershopRequest {}

export interface BarbershopFilters {
  pageNumber?: number;
  pageSize?: number;
  searchTerm?: string;
  isActive?: boolean;
}

// src/types/pagination.ts
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// src/types/auth.ts
export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: {
    id: string;
    email: string;
    name: string;
  };
}
```

---

## 4. Validação com Zod

### 4.1 Schema de Barbearia

```typescript
// src/schemas/barbershop.schema.ts
import { z } from 'zod';

// Regex patterns
const phoneRegex = /^\(\d{2}\) \d{5}-\d{4}$/;
const zipCodeRegex = /^\d{5}-\d{3}$/;

// Address schema
const addressSchema = z.object({
  street: z.string().min(3, 'Logradouro deve ter no mínimo 3 caracteres').max(200),
  number: z.string().min(1, 'Número é obrigatório').max(10),
  complement: z.string().max(100).optional(),
  neighborhood: z.string().min(3, 'Bairro deve ter no mínimo 3 caracteres').max(100),
  city: z.string().min(3, 'Cidade deve ter no mínimo 3 caracteres').max(100),
  state: z.string().length(2, 'Estado deve ter 2 caracteres (ex: SP)').toUpperCase(),
  zipCode: z.string().regex(zipCodeRegex, 'CEP inválido (formato: 99999-999)'),
});

// Barbershop schema
export const barbershopSchema = z.object({
  name: z.string()
    .min(3, 'Nome deve ter no mínimo 3 caracteres')
    .max(100, 'Nome deve ter no máximo 100 caracteres'),
  email: z.string()
    .email('Email inválido')
    .max(100),
  phone: z.string()
    .regex(phoneRegex, 'Telefone inválido (formato: (99) 99999-9999)'),
  address: addressSchema,
});

export type BarbershopFormData = z.infer<typeof barbershopSchema>;

// Login schema
export const loginSchema = z.object({
  email: z.string().email('Email inválido'),
  password: z.string().min(6, 'Senha deve ter no mínimo 6 caracteres'),
});

export type LoginFormData = z.infer<typeof loginSchema>;
```

---

## 5. Páginas e Funcionalidades

### 5.1 Login (`src/pages/Login/Login.tsx`)

**Referência de Design:** `tela-login.html`

**Funcionalidades:**
- Form com email e senha
- Validação client-side com Zod
- Autenticação via `/api/auth/admin-central`
- Armazenar token JWT em localStorage (MVP)
- Redirect para `/barbearias` após sucesso
- Mensagem de erro clara em caso de falha

**Implementação:**

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { loginSchema, LoginFormData } from '@/schemas/barbershop.schema';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { useToast } from '@/components/ui/use-toast';
import api from '@/services/api';

export function Login() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isLoading, setIsLoading] = useState(false);

  const { register, handleSubmit, formState: { errors } } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true);
    try {
      const response = await api.post('/auth/admin-central', data);
      localStorage.setItem('auth_token', response.data.token);
      toast({ title: 'Login realizado com sucesso!' });
      navigate('/barbearias');
    } catch (error) {
      toast({
        title: 'Erro ao fazer login',
        description: 'Verifique suas credenciais e tente novamente.',
        variant: 'destructive',
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50">
      <div className="w-full max-w-md space-y-8 rounded-lg bg-white p-8 shadow-md">
        <div className="text-center">
          <h2 className="text-3xl font-bold">BarbApp Admin</h2>
          <p className="mt-2 text-sm text-gray-600">Faça login para continuar</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div>
            <Label htmlFor="email">Email</Label>
            <Input
              id="email"
              type="email"
              {...register('email')}
              className={errors.email ? 'border-red-500' : ''}
            />
            {errors.email && (
              <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>
            )}
          </div>

          <div>
            <Label htmlFor="password">Senha</Label>
            <Input
              id="password"
              type="password"
              {...register('password')}
              className={errors.password ? 'border-red-500' : ''}
            />
            {errors.password && (
              <p className="mt-1 text-sm text-red-500">{errors.password.message}</p>
            )}
          </div>

          <Button type="submit" className="w-full" disabled={isLoading}>
            {isLoading ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
      </div>
    </div>
  );
}
```

### 5.2 Listagem de Barbearias (`src/pages/Barbershops/List.tsx`)

**Referência de Design:** `gerenciamento-barbearia.html`

**Funcionalidades:**
- Tabela responsiva com paginação
- Filtros: busca textual + status (ativo/inativo/todos)
- Debounce de 300ms na busca textual
- Ações: Ver detalhes, Editar, Desativar/Reativar
- Estados: Loading (skeleton), Success, Error, Empty
- Confirmação antes de desativar

**Custom Hook:**

```typescript
// src/hooks/useBarbershops.ts
import { useState, useEffect } from 'react';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop, BarbershopFilters, PaginatedResponse } from '@/types/barbershop';

export function useBarbershops(filters: BarbershopFilters) {
  const [data, setData] = useState<PaginatedResponse<Barbershop> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function fetchData() {
      try {
        setLoading(true);
        setError(null);
        const response = await barbershopService.getAll(filters);
        if (!cancelled) {
          setData(response);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err as Error);
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    fetchData();

    return () => {
      cancelled = true;
    };
  }, [filters.pageNumber, filters.pageSize, filters.searchTerm, filters.isActive]);

  return { data, loading, error };
}
```

**Implementação da Página:**

```typescript
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useBarbershops } from '@/hooks/useBarbershops';
import { useDebounce } from '@/hooks/useDebounce';
import { BarbershopTable } from '@/components/barbershop/BarbershopTable';
import { BarbershopTableSkeleton } from '@/components/barbershop/BarbershopTableSkeleton';
import { EmptyState } from '@/components/barbershop/EmptyState';
import { DeactivateModal } from '@/components/barbershop/DeactivateModal';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Select } from '@/components/ui/select';
import { Pagination } from '@/components/ui/pagination';
import { useToast } from '@/components/ui/use-toast';
import { barbershopService } from '@/services/barbershop.service';

export function BarbershopList() {
  const navigate = useNavigate();
  const { toast } = useToast();

  // Estado dos filtros
  const [searchTerm, setSearchTerm] = useState('');
  const [statusFilter, setStatusFilter] = useState<boolean | undefined>(undefined);
  const [currentPage, setCurrentPage] = useState(1);
  const pageSize = 20;

  // Debounce da busca
  const debouncedSearch = useDebounce(searchTerm, 300);

  // Buscar dados
  const { data, loading, error } = useBarbershops({
    searchTerm: debouncedSearch,
    isActive: statusFilter,
    pageNumber: currentPage,
    pageSize,
  });

  // Modal de confirmação
  const [deactivateModalOpen, setDeactivateModalOpen] = useState(false);
  const [selectedBarbershop, setSelectedBarbershop] = useState<string | null>(null);

  const handleDeactivate = (id: string) => {
    setSelectedBarbershop(id);
    setDeactivateModalOpen(true);
  };

  const confirmDeactivate = async () => {
    if (!selectedBarbershop) return;

    try {
      await barbershopService.deactivate(selectedBarbershop);
      toast({ title: 'Barbearia desativada com sucesso!' });
      setDeactivateModalOpen(false);
      // Refetch data (trigger useEffect)
      setCurrentPage(currentPage); // Force re-render
    } catch (error) {
      toast({
        title: 'Erro ao desativar barbearia',
        description: 'Tente novamente mais tarde.',
        variant: 'destructive',
      });
    }
  };

  const handleReactivate = async (id: string) => {
    try {
      await barbershopService.reactivate(id);
      toast({ title: 'Barbearia reativada com sucesso!' });
    } catch (error) {
      toast({
        title: 'Erro ao reativar barbearia',
        variant: 'destructive',
      });
    }
  };

  if (loading) return <BarbershopTableSkeleton />;
  if (error) return <div>Erro ao carregar dados: {error.message}</div>;
  if (!data || data.items.length === 0) return <EmptyState />;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-bold">Gestão de Barbearias</h1>
        <Button onClick={() => navigate('/barbearias/nova')}>
          + Nova Barbearia
        </Button>
      </div>

      {/* Filtros */}
      <div className="flex gap-4">
        <Input
          placeholder="Buscar por nome, email ou cidade..."
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
          className="max-w-md"
        />
        <Select
          value={statusFilter?.toString() ?? 'all'}
          onValueChange={(value) => {
            setStatusFilter(value === 'all' ? undefined : value === 'true');
          }}
        >
          <option value="all">Todos</option>
          <option value="true">Ativos</option>
          <option value="false">Inativos</option>
        </Select>
      </div>

      {/* Tabela */}
      <BarbershopTable
        barbershops={data.items}
        onView={(id) => navigate(`/barbearias/${id}`)}
        onEdit={(id) => navigate(`/barbearias/${id}/editar`)}
        onDeactivate={handleDeactivate}
        onReactivate={handleReactivate}
      />

      {/* Paginação */}
      <Pagination
        currentPage={data.pageNumber}
        totalPages={data.totalPages}
        onPageChange={setCurrentPage}
        hasPreviousPage={data.hasPreviousPage}
        hasNextPage={data.hasNextPage}
      />

      {/* Modal de Confirmação */}
      <DeactivateModal
        open={deactivateModalOpen}
        onClose={() => setDeactivateModalOpen(false)}
        onConfirm={confirmDeactivate}
      />
    </div>
  );
}
```

### 5.3 Cadastro de Barbearia (`src/pages/Barbershops/Create.tsx`)

**Referência:** PRD `cadastro-barbearia.md`

**Funcionalidades:**
- Formulário completo com validação Zod
- Máscaras: telefone `(99) 99999-9999`, CEP `99999-999`
- Integração com ViaCEP (opcional para MVP)
- Feedback visual de erros em tempo real
- Loading durante submissão
- Redirect para listagem após sucesso

**Implementação:**

```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barbershopSchema, BarbershopFormData } from '@/schemas/barbershop.schema';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import { barbershopService } from '@/services/barbershop.service';
import { handleApiError } from '@/utils/errorHandler';

export function BarbershopCreate() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { register, handleSubmit, formState: { errors }, setValue } = useForm<BarbershopFormData>({
    resolver: zodResolver(barbershopSchema),
  });

  const onSubmit = async (data: BarbershopFormData) => {
    setIsSubmitting(true);
    try {
      await barbershopService.create(data);
      toast({ title: 'Barbearia cadastrada com sucesso!' });
      navigate('/barbearias');
    } catch (error) {
      toast({
        title: 'Erro ao cadastrar barbearia',
        description: handleApiError(error),
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Nova Barbearia</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl space-y-6">
        <BarbershopForm
          register={register}
          errors={errors}
          setValue={setValue}
        />

        <div className="flex gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => navigate(-1)}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Salvando...' : 'Salvar'}
          </Button>
        </div>
      </form>
    </div>
  );
}
```

### 5.4 Edição de Barbearia (`src/pages/Barbershops/Edit.tsx`)

**Referência:** PRD `edicao-barbearia.md`

**Funcionalidades:**
- Carregar dados existentes via `GET /api/barbearias/{id}`
- Form pré-preenchido
- Detectar alterações (dirty state)
- Confirmação ao sair sem salvar (useBeforeUnload)
- Mesmas validações do cadastro

**Implementação:**

```typescript
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { barbershopSchema, BarbershopFormData } from '@/schemas/barbershop.schema';
import { BarbershopForm } from '@/components/barbershop/BarbershopForm';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import { barbershopService } from '@/services/barbershop.service';
import { handleApiError } from '@/utils/errorHandler';

export function BarbershopEdit() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [loading, setLoading] = useState(true);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const { register, handleSubmit, formState: { errors, isDirty }, setValue, reset } = useForm<BarbershopFormData>({
    resolver: zodResolver(barbershopSchema),
  });

  // Carregar dados existentes
  useEffect(() => {
    async function loadBarbershop() {
      if (!id) return;
      try {
        const data = await barbershopService.getById(id);
        reset({
          name: data.name,
          email: data.email,
          phone: data.phone,
          address: data.address,
        });
      } catch (error) {
        toast({
          title: 'Erro ao carregar barbearia',
          description: handleApiError(error),
          variant: 'destructive',
        });
        navigate('/barbearias');
      } finally {
        setLoading(false);
      }
    }
    loadBarbershop();
  }, [id, reset, toast, navigate]);

  // Confirmação ao sair sem salvar
  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  const onSubmit = async (data: BarbershopFormData) => {
    if (!id) return;
    setIsSubmitting(true);
    try {
      await barbershopService.update(id, data);
      toast({ title: 'Barbearia atualizada com sucesso!' });
      navigate('/barbearias');
    } catch (error) {
      toast({
        title: 'Erro ao atualizar barbearia',
        description: handleApiError(error),
        variant: 'destructive',
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (loading) return <div>Carregando...</div>;

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Editar Barbearia</h1>

      <form onSubmit={handleSubmit(onSubmit)} className="max-w-2xl space-y-6">
        <BarbershopForm
          register={register}
          errors={errors}
          setValue={setValue}
        />

        <div className="flex gap-4">
          <Button
            type="button"
            variant="outline"
            onClick={() => {
              if (isDirty) {
                if (window.confirm('Você tem alterações não salvas. Deseja realmente sair?')) {
                  navigate(-1);
                }
              } else {
                navigate(-1);
              }
            }}
            disabled={isSubmitting}
          >
            Cancelar
          </Button>
          <Button type="submit" disabled={isSubmitting || !isDirty}>
            {isSubmitting ? 'Salvando...' : 'Salvar Alterações'}
          </Button>
        </div>
      </form>
    </div>
  );
}
```

### 5.5 Detalhes da Barbearia (`src/pages/Barbershops/Details.tsx`)

**Referência:** PRD `detalhe-barbearia.md`

**Funcionalidades:**
- Exibição read-only de todos os dados
- Badge de status (ativo/inativo)
- Informações de auditoria (criado em, atualizado em)
- Botões: Editar, Desativar/Reativar, Voltar
- Layout responsivo

**Implementação:**

```typescript
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { barbershopService } from '@/services/barbershop.service';
import type { Barbershop } from '@/types/barbershop';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/components/ui/use-toast';
import { formatDate } from '@/utils/formatters';

export function BarbershopDetails() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { toast } = useToast();
  const [barbershop, setBarbershop] = useState<Barbershop | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadBarbershop() {
      if (!id) return;
      try {
        const data = await barbershopService.getById(id);
        setBarbershop(data);
      } catch (error) {
        toast({
          title: 'Erro ao carregar barbearia',
          variant: 'destructive',
        });
        navigate('/barbearias');
      } finally {
        setLoading(false);
      }
    }
    loadBarbershop();
  }, [id, toast, navigate]);

  const handleToggleStatus = async () => {
    if (!barbershop) return;
    try {
      if (barbershop.isActive) {
        await barbershopService.deactivate(barbershop.id);
        toast({ title: 'Barbearia desativada com sucesso!' });
      } else {
        await barbershopService.reactivate(barbershop.id);
        toast({ title: 'Barbearia reativada com sucesso!' });
      }
      // Recarregar dados
      const updated = await barbershopService.getById(barbershop.id);
      setBarbershop(updated);
    } catch (error) {
      toast({
        title: 'Erro ao alterar status',
        variant: 'destructive',
      });
    }
  };

  if (loading) return <div>Carregando...</div>;
  if (!barbershop) return null;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <h1 className="text-3xl font-bold">{barbershop.name}</h1>
          <Badge variant={barbershop.isActive ? 'success' : 'destructive'}>
            {barbershop.isActive ? 'Ativo' : 'Inativo'}
          </Badge>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate(-1)}>
            Voltar
          </Button>
          <Button onClick={() => navigate(`/barbearias/${id}/editar`)}>
            Editar
          </Button>
          <Button
            variant={barbershop.isActive ? 'destructive' : 'default'}
            onClick={handleToggleStatus}
          >
            {barbershop.isActive ? 'Desativar' : 'Reativar'}
          </Button>
        </div>
      </div>

      <div className="grid gap-6 rounded-lg border p-6">
        <div>
          <h2 className="mb-4 text-xl font-semibold">Informações Gerais</h2>
          <dl className="grid grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">Email</dt>
              <dd>{barbershop.email}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Telefone</dt>
              <dd>{barbershop.phone}</dd>
            </div>
          </dl>
        </div>

        <div>
          <h2 className="mb-4 text-xl font-semibold">Endereço</h2>
          <dl className="grid grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">Logradouro</dt>
              <dd>{barbershop.address.street}, {barbershop.address.number}</dd>
            </div>
            {barbershop.address.complement && (
              <div>
                <dt className="font-medium text-gray-500">Complemento</dt>
                <dd>{barbershop.address.complement}</dd>
              </div>
            )}
            <div>
              <dt className="font-medium text-gray-500">Bairro</dt>
              <dd>{barbershop.address.neighborhood}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Cidade/UF</dt>
              <dd>{barbershop.address.city} - {barbershop.address.state}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">CEP</dt>
              <dd>{barbershop.address.zipCode}</dd>
            </div>
          </dl>
        </div>

        <div>
          <h2 className="mb-4 text-xl font-semibold">Auditoria</h2>
          <dl className="grid grid-cols-2 gap-4">
            <div>
              <dt className="font-medium text-gray-500">Criado em</dt>
              <dd>{formatDate(barbershop.createdAt)}</dd>
            </div>
            <div>
              <dt className="font-medium text-gray-500">Atualizado em</dt>
              <dd>{formatDate(barbershop.updatedAt)}</dd>
            </div>
          </dl>
        </div>
      </div>
    </div>
  );
}
```

---

## 6. Componentes Reutilizáveis

### 6.1 Componentes shadcn/ui Necessários

Instalar via CLI:

```bash
npx shadcn-ui@latest init
npx shadcn-ui@latest add button input label table dialog toast skeleton badge select form
```

### 6.2 BarbershopTable Component

```typescript
// src/components/barbershop/BarbershopTable.tsx
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import type { Barbershop } from '@/types/barbershop';

interface Props {
  barbershops: Barbershop[];
  onView: (id: string) => void;
  onEdit: (id: string) => void;
  onDeactivate: (id: string) => void;
  onReactivate: (id: string) => void;
}

export function BarbershopTable({ barbershops, onView, onEdit, onDeactivate, onReactivate }: Props) {
  return (
    <div className="rounded-lg border">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Nome</TableHead>
            <TableHead>Email</TableHead>
            <TableHead>Telefone</TableHead>
            <TableHead>Cidade</TableHead>
            <TableHead>Status</TableHead>
            <TableHead className="text-right">Ações</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {barbershops.map((barbershop) => (
            <TableRow key={barbershop.id}>
              <TableCell className="font-medium">{barbershop.name}</TableCell>
              <TableCell>{barbershop.email}</TableCell>
              <TableCell>{barbershop.phone}</TableCell>
              <TableCell>{barbershop.address.city}</TableCell>
              <TableCell>
                <Badge variant={barbershop.isActive ? 'success' : 'destructive'}>
                  {barbershop.isActive ? 'Ativo' : 'Inativo'}
                </Badge>
              </TableCell>
              <TableCell className="text-right">
                <div className="flex justify-end gap-2">
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onView(barbershop.id)}
                  >
                    Ver
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => onEdit(barbershop.id)}
                  >
                    Editar
                  </Button>
                  {barbershop.isActive ? (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onDeactivate(barbershop.id)}
                      className="text-red-600"
                    >
                      Desativar
                    </Button>
                  ) : (
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => onReactivate(barbershop.id)}
                      className="text-green-600"
                    >
                      Reativar
                    </Button>
                  )}
                </div>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}
```

### 6.3 BarbershopForm Component

```typescript
// src/components/barbershop/BarbershopForm.tsx
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import type { UseFormRegister, FieldErrors, UseFormSetValue } from 'react-hook-form';
import type { BarbershopFormData } from '@/schemas/barbershop.schema';
import { applyPhoneMask, applyZipCodeMask } from '@/utils/formatters';

interface Props {
  register: UseFormRegister<BarbershopFormData>;
  errors: FieldErrors<BarbershopFormData>;
  setValue: UseFormSetValue<BarbershopFormData>;
}

export function BarbershopForm({ register, errors, setValue }: Props) {
  return (
    <div className="space-y-6">
      {/* Informações Gerais */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold">Informações Gerais</h2>

        <div>
          <Label htmlFor="name">Nome *</Label>
          <Input
            id="name"
            {...register('name')}
            className={errors.name ? 'border-red-500' : ''}
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-500">{errors.name.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="email">Email *</Label>
          <Input
            id="email"
            type="email"
            {...register('email')}
            className={errors.email ? 'border-red-500' : ''}
          />
          {errors.email && (
            <p className="mt-1 text-sm text-red-500">{errors.email.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="phone">Telefone *</Label>
          <Input
            id="phone"
            {...register('phone')}
            onChange={(e) => {
              const masked = applyPhoneMask(e.target.value);
              setValue('phone', masked);
            }}
            placeholder="(99) 99999-9999"
            className={errors.phone ? 'border-red-500' : ''}
          />
          {errors.phone && (
            <p className="mt-1 text-sm text-red-500">{errors.phone.message}</p>
          )}
        </div>
      </div>

      {/* Endereço */}
      <div className="space-y-4">
        <h2 className="text-lg font-semibold">Endereço</h2>

        <div>
          <Label htmlFor="zipCode">CEP *</Label>
          <Input
            id="zipCode"
            {...register('address.zipCode')}
            onChange={(e) => {
              const masked = applyZipCodeMask(e.target.value);
              setValue('address.zipCode', masked);
            }}
            placeholder="99999-999"
            className={errors.address?.zipCode ? 'border-red-500' : ''}
          />
          {errors.address?.zipCode && (
            <p className="mt-1 text-sm text-red-500">{errors.address.zipCode.message}</p>
          )}
        </div>

        <div className="grid grid-cols-3 gap-4">
          <div className="col-span-2">
            <Label htmlFor="street">Logradouro *</Label>
            <Input
              id="street"
              {...register('address.street')}
              className={errors.address?.street ? 'border-red-500' : ''}
            />
            {errors.address?.street && (
              <p className="mt-1 text-sm text-red-500">{errors.address.street.message}</p>
            )}
          </div>

          <div>
            <Label htmlFor="number">Número *</Label>
            <Input
              id="number"
              {...register('address.number')}
              className={errors.address?.number ? 'border-red-500' : ''}
            />
            {errors.address?.number && (
              <p className="mt-1 text-sm text-red-500">{errors.address.number.message}</p>
            )}
          </div>
        </div>

        <div>
          <Label htmlFor="complement">Complemento</Label>
          <Input
            id="complement"
            {...register('address.complement')}
          />
        </div>

        <div>
          <Label htmlFor="neighborhood">Bairro *</Label>
          <Input
            id="neighborhood"
            {...register('address.neighborhood')}
            className={errors.address?.neighborhood ? 'border-red-500' : ''}
          />
          {errors.address?.neighborhood && (
            <p className="mt-1 text-sm text-red-500">{errors.address.neighborhood.message}</p>
          )}
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label htmlFor="city">Cidade *</Label>
            <Input
              id="city"
              {...register('address.city')}
              className={errors.address?.city ? 'border-red-500' : ''}
            />
            {errors.address?.city && (
              <p className="mt-1 text-sm text-red-500">{errors.address.city.message}</p>
            )}
          </div>

          <div>
            <Label htmlFor="state">Estado *</Label>
            <Input
              id="state"
              {...register('address.state')}
              maxLength={2}
              placeholder="SP"
              className={errors.address?.state ? 'border-red-500' : ''}
            />
            {errors.address?.state && (
              <p className="mt-1 text-sm text-red-500">{errors.address.state.message}</p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
```

### 6.4 DeactivateModal Component

```typescript
// src/components/barbershop/DeactivateModal.tsx
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';

interface Props {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
}

export function DeactivateModal({ open, onClose, onConfirm }: Props) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Confirmar Desativação</DialogTitle>
          <DialogDescription>
            Tem certeza que deseja desativar esta barbearia?
            Esta ação não poderá ser desfeita automaticamente, mas a barbearia pode ser reativada posteriormente.
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancelar
          </Button>
          <Button variant="destructive" onClick={onConfirm}>
            Confirmar Desativação
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
```

### 6.5 Pagination Component

```typescript
// src/components/ui/pagination.tsx
import { Button } from '@/components/ui/button';

interface Props {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  hasPreviousPage,
  hasNextPage,
}: Props) {
  return (
    <div className="flex items-center justify-between">
      <div className="text-sm text-gray-500">
        Página {currentPage} de {totalPages}
      </div>
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={!hasPreviousPage}
          onClick={() => onPageChange(currentPage - 1)}
        >
          ← Anterior
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={!hasNextPage}
          onClick={() => onPageChange(currentPage + 1)}
        >
          Próxima →
        </Button>
      </div>
    </div>
  );
}
```

### 6.6 EmptyState Component

```typescript
// src/components/barbershop/EmptyState.tsx
import { Button } from '@/components/ui/button';
import { useNavigate } from 'react-router-dom';

export function EmptyState() {
  const navigate = useNavigate();

  return (
    <div className="flex flex-col items-center justify-center py-12 text-center">
      <svg
        className="mb-4 h-16 w-16 text-gray-400"
        fill="none"
        viewBox="0 0 24 24"
        stroke="currentColor"
      >
        <path
          strokeLinecap="round"
          strokeLinejoin="round"
          strokeWidth={2}
          d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4"
        />
      </svg>
      <h3 className="mb-2 text-lg font-semibold text-gray-900">
        Nenhuma barbearia encontrada
      </h3>
      <p className="mb-6 text-sm text-gray-500">
        Comece cadastrando a primeira barbearia do sistema.
      </p>
      <Button onClick={() => navigate('/barbearias/nova')}>
        + Nova Barbearia
      </Button>
    </div>
  );
}
```

### 6.7 BarbershopTableSkeleton Component

```typescript
// src/components/barbershop/BarbershopTableSkeleton.tsx
import { Skeleton } from '@/components/ui/skeleton';

export function BarbershopTableSkeleton() {
  return (
    <div className="space-y-4">
      {[...Array(5)].map((_, i) => (
        <Skeleton key={i} className="h-16 w-full" />
      ))}
    </div>
  );
}
```

---

## 7. Utilitários

### 7.1 Error Handler

```typescript
// src/utils/errorHandler.ts
import axios from 'axios';

export function handleApiError(error: unknown): string {
  if (axios.isAxiosError(error)) {
    if (error.response) {
      // Backend respondeu com erro
      const message = error.response.data?.message || error.response.data?.title;
      return message || 'Erro ao processar requisição';
    } else if (error.request) {
      // Requisição feita mas sem resposta
      return 'Servidor não respondeu. Verifique sua conexão.';
    }
  }
  return 'Erro inesperado. Tente novamente.';
}
```

### 7.2 Formatters

```typescript
// src/utils/formatters.ts

/**
 * Aplicar máscara de telefone (99) 99999-9999
 */
export function applyPhoneMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');
  const match = cleaned.match(/^(\d{2})(\d{5})(\d{4})$/);
  if (match) {
    return `(${match[1]}) ${match[2]}-${match[3]}`;
  }
  return value;
}

/**
 * Aplicar máscara de CEP 99999-999
 */
export function applyZipCodeMask(value: string): string {
  const cleaned = value.replace(/\D/g, '');
  const match = cleaned.match(/^(\d{5})(\d{3})$/);
  if (match) {
    return `${match[1]}-${match[2]}`;
  }
  return value;
}

/**
 * Formatar data ISO para pt-BR
 */
export function formatDate(isoDate: string): string {
  return new Date(isoDate).toLocaleDateString('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
```

### 7.3 Custom Hooks

```typescript
// src/hooks/useDebounce.ts
import { useEffect, useState } from 'react';

export function useDebounce<T>(value: T, delay: number): T {
  const [debouncedValue, setDebouncedValue] = useState<T>(value);

  useEffect(() => {
    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);

    return () => {
      clearTimeout(handler);
    };
  }, [value, delay]);

  return debouncedValue;
}

// src/hooks/useAuth.ts
import { useState, useEffect } from 'react';

export function useAuth() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);

  useEffect(() => {
    const token = localStorage.getItem('auth_token');
    setIsAuthenticated(!!token);
  }, []);

  const logout = () => {
    localStorage.removeItem('auth_token');
    setIsAuthenticated(false);
    window.location.href = '/login';
  };

  return { isAuthenticated, logout };
}
```

---

## 8. Roteamento

### 8.1 Configuração React Router

```typescript
// src/routes/index.tsx
import { createBrowserRouter, Navigate } from 'react-router-dom';
import { Login } from '@/pages/Login/Login';
import { BarbershopList } from '@/pages/Barbershops/List';
import { BarbershopCreate } from '@/pages/Barbershops/Create';
import { BarbershopEdit } from '@/pages/Barbershops/Edit';
import { BarbershopDetails } from '@/pages/Barbershops/Details';
import { ProtectedRoute } from '@/components/ProtectedRoute';

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <Login />,
  },
  {
    path: '/',
    element: <ProtectedRoute />,
    children: [
      {
        index: true,
        element: <Navigate to="/barbearias" replace />,
      },
      {
        path: 'barbearias',
        children: [
          { index: true, element: <BarbershopList /> },
          { path: 'nova', element: <BarbershopCreate /> },
          { path: ':id', element: <BarbershopDetails /> },
          { path: ':id/editar', element: <BarbershopEdit /> },
        ],
      },
    ],
  },
  {
    path: '*',
    element: <Navigate to="/barbearias" replace />,
  },
]);
```

### 8.2 ProtectedRoute Component

```typescript
// src/components/ProtectedRoute.tsx
import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/useAuth';
import { Header } from '@/components/layout/Header';

export function ProtectedRoute() {
  const { isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <Header />
      <main className="container mx-auto px-4 py-8">
        <Outlet />
      </main>
    </div>
  );
}
```

### 8.3 Header Component

```typescript
// src/components/layout/Header.tsx
import { Button } from '@/components/ui/button';
import { useAuth } from '@/hooks/useAuth';

export function Header() {
  const { logout } = useAuth();

  return (
    <header className="border-b bg-white">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <h1 className="text-xl font-bold">BarbApp Admin</h1>
        <Button variant="outline" onClick={logout}>
          Sair
        </Button>
      </div>
    </header>
  );
}
```

---

## 9. Testes

### 9.1 Estrutura de Testes

```
src/
├── __tests__/
│   ├── unit/
│   │   ├── components/
│   │   │   ├── BarbershopTable.test.tsx
│   │   │   ├── BarbershopForm.test.tsx
│   │   │   └── Pagination.test.tsx
│   │   ├── hooks/
│   │   │   ├── useBarbershops.test.ts
│   │   │   └── useDebounce.test.ts
│   │   └── utils/
│   │       ├── formatters.test.ts
│   │       └── errorHandler.test.ts
│   ├── integration/
│   │   └── services/
│   │       └── barbershop.service.test.ts
│   └── e2e/
│       └── barbershop-crud.spec.ts
```

### 9.2 Configuração Vitest

```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/__tests__/setup.ts'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/__tests__/',
        '**/*.config.ts',
        '**/types/**',
        '**/main.tsx',
      ],
      thresholds: {
        lines: 70,
        functions: 70,
        branches: 70,
        statements: 70,
      },
    },
  },
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
});
```

```typescript
// src/__tests__/setup.ts
import '@testing-library/jest-dom';
import { cleanup } from '@testing-library/react';
import { afterEach } from 'vitest';

afterEach(() => {
  cleanup();
});
```

### 9.3 Exemplo de Teste Unitário

```typescript
// src/__tests__/unit/components/BarbershopTable.test.tsx
import { render, screen } from '@testing-library/react';
import { userEvent } from '@testing-library/user-event';
import { describe, it, expect, vi } from 'vitest';
import { BarbershopTable } from '@/components/barbershop/BarbershopTable';
import type { Barbershop } from '@/types/barbershop';

const mockBarbershops: Barbershop[] = [
  {
    id: '1',
    name: 'Barbearia Teste',
    email: 'teste@email.com',
    phone: '(11) 99999-9999',
    address: {
      street: 'Rua Teste',
      number: '123',
      neighborhood: 'Centro',
      city: 'São Paulo',
      state: 'SP',
      zipCode: '01000-000',
    },
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z',
    updatedAt: '2024-01-01T00:00:00Z',
  },
];

describe('BarbershopTable', () => {
  it('should render barbershop data correctly', () => {
    render(
      <BarbershopTable
        barbershops={mockBarbershops}
        onView={vi.fn()}
        onEdit={vi.fn()}
        onDeactivate={vi.fn()}
        onReactivate={vi.fn()}
      />
    );

    expect(screen.getByText('Barbearia Teste')).toBeInTheDocument();
    expect(screen.getByText('teste@email.com')).toBeInTheDocument();
    expect(screen.getByText('(11) 99999-9999')).toBeInTheDocument();
    expect(screen.getByText('São Paulo')).toBeInTheDocument();
    expect(screen.getByText('Ativo')).toBeInTheDocument();
  });

  it('should call onEdit when edit button is clicked', async () => {
    const user = userEvent.setup();
    const onEdit = vi.fn();

    render(
      <BarbershopTable
        barbershops={mockBarbershops}
        onView={vi.fn()}
        onEdit={onEdit}
        onDeactivate={vi.fn()}
        onReactivate={vi.fn()}
      />
    );

    await user.click(screen.getByRole('button', { name: /editar/i }));
    expect(onEdit).toHaveBeenCalledWith('1');
  });

  it('should show deactivate button for active barbershop', () => {
    render(
      <BarbershopTable
        barbershops={mockBarbershops}
        onView={vi.fn()}
        onEdit={vi.fn()}
        onDeactivate={vi.fn()}
        onReactivate={vi.fn()}
      />
    );

    expect(screen.getByRole('button', { name: /desativar/i })).toBeInTheDocument();
    expect(screen.queryByRole('button', { name: /reativar/i })).not.toBeInTheDocument();
  });
});
```

### 9.4 Exemplo de Teste de Integração

```typescript
// src/__tests__/integration/services/barbershop.service.test.ts
import { describe, it, expect, beforeAll, afterAll, afterEach } from 'vitest';
import { setupServer } from 'msw/node';
import { http, HttpResponse } from 'msw';
import { barbershopService } from '@/services/barbershop.service';

const server = setupServer(
  http.get('http://localhost:5000/api/barbearias', () => {
    return HttpResponse.json({
      items: [
        {
          id: '1',
          name: 'Barbearia Mock',
          email: 'mock@email.com',
          phone: '(11) 99999-9999',
          address: {
            street: 'Rua Mock',
            number: '123',
            neighborhood: 'Centro',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '01000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        },
      ],
      pageNumber: 1,
      pageSize: 20,
      totalCount: 1,
      totalPages: 1,
      hasPreviousPage: false,
      hasNextPage: false,
    });
  })
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

describe('barbershopService', () => {
  it('should fetch barbershops with pagination', async () => {
    const result = await barbershopService.getAll({ pageNumber: 1, pageSize: 20 });

    expect(result.items).toHaveLength(1);
    expect(result.items[0].name).toBe('Barbearia Mock');
    expect(result.pageNumber).toBe(1);
    expect(result.totalCount).toBe(1);
  });

  it('should fetch barbershop by id', async () => {
    server.use(
      http.get('http://localhost:5000/api/barbearias/:id', () => {
        return HttpResponse.json({
          id: '1',
          name: 'Barbearia Específica',
          email: 'especifica@email.com',
          phone: '(11) 98888-7777',
          address: {
            street: 'Rua Específica',
            number: '456',
            neighborhood: 'Vila',
            city: 'São Paulo',
            state: 'SP',
            zipCode: '02000-000',
          },
          isActive: true,
          createdAt: '2024-01-01T00:00:00Z',
          updatedAt: '2024-01-01T00:00:00Z',
        });
      })
    );

    const result = await barbershopService.getById('1');

    expect(result.name).toBe('Barbearia Específica');
    expect(result.email).toBe('especifica@email.com');
  });
});
```

### 9.5 Exemplo de Teste E2E (Playwright)

```typescript
// src/__tests__/e2e/barbershop-crud.spec.ts
import { test, expect } from '@playwright/test';

test.describe('Barbershop CRUD', () => {
  test.beforeEach(async ({ page }) => {
    // Login
    await page.goto('http://localhost:3000/login');
    await page.fill('input[id="email"]', 'admin@barbapp.com');
    await page.fill('input[id="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await expect(page).toHaveURL('http://localhost:3000/barbearias');
  });

  test('should create a new barbershop', async ({ page }) => {
    // Navigate to create page
    await page.click('text=Nova Barbearia');
    await expect(page).toHaveURL(/\/barbearias\/nova/);

    // Fill form
    await page.fill('input[id="name"]', 'Nova Barbearia E2E');
    await page.fill('input[id="email"]', 'novae2e@barbapp.com');
    await page.fill('input[id="phone"]', '(11) 98765-4321');
    await page.fill('input[id="zipCode"]', '01310-100');
    await page.fill('input[id="street"]', 'Avenida Paulista');
    await page.fill('input[id="number"]', '1000');
    await page.fill('input[id="neighborhood"]', 'Bela Vista');
    await page.fill('input[id="city"]', 'São Paulo');
    await page.fill('input[id="state"]', 'SP');

    // Submit
    await page.click('button[type="submit"]');

    // Assert success
    await expect(page).toHaveURL('http://localhost:3000/barbearias');
    await expect(page.locator('text=Nova Barbearia E2E')).toBeVisible();
  });

  test('should edit an existing barbershop', async ({ page }) => {
    // Assuming barbershop exists, click edit
    await page.click('text=Editar >> nth=0');
    await expect(page).toHaveURL(/\/barbearias\/[^/]+\/editar/);

    // Edit name
    await page.fill('input[id="name"]', 'Barbearia Editada');

    // Submit
    await page.click('button[type="submit"]');

    // Assert success
    await expect(page).toHaveURL('http://localhost:3000/barbearias');
    await expect(page.locator('text=Barbearia Editada')).toBeVisible();
  });

  test('should deactivate a barbershop', async ({ page }) => {
    // Click deactivate
    await page.click('text=Desativar >> nth=0');

    // Confirm in modal
    await page.click('text=Confirmar Desativação');

    // Assert status changed
    await expect(page.locator('text=Inativo')).toBeVisible();
  });

  test('should filter barbershops by search term', async ({ page }) => {
    // Type in search
    await page.fill('input[placeholder*="Buscar"]', 'Teste');

    // Wait for debounce and results
    await page.waitForTimeout(500);

    // Assert filtered results
    const rows = page.locator('table tbody tr');
    await expect(rows).toHaveCount(1); // Assuming only one matches "Teste"
  });
});
```

### 9.6 Scripts de Teste no package.json

```json
{
  "scripts": {
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest run --coverage",
    "test:e2e": "playwright test",
    "test:e2e:ui": "playwright test --ui",
    "test:e2e:headed": "playwright test --headed"
  }
}
```

---

## 10. Configuração do Projeto

### 10.1 package.json

```json
{
  "name": "barbapp-admin",
  "version": "0.1.0",
  "type": "module",
  "scripts": {
    "dev": "vite",
    "build": "tsc && vite build",
    "preview": "vite preview",
    "test": "vitest",
    "test:ui": "vitest --ui",
    "test:coverage": "vitest run --coverage",
    "test:e2e": "playwright test",
    "lint": "eslint . --ext ts,tsx --report-unused-disable-directives --max-warnings 0",
    "format": "prettier --write \"src/**/*.{ts,tsx,json,css,md}\""
  },
  "dependencies": {
    "react": "^18.3.1",
    "react-dom": "^18.3.1",
    "react-router-dom": "^6.22.3",
    "react-hook-form": "^7.51.2",
    "@hookform/resolvers": "^3.3.4",
    "zod": "^3.23.0",
    "axios": "^1.6.8",
    "@radix-ui/react-dialog": "^1.0.5",
    "@radix-ui/react-label": "^2.0.2",
    "@radix-ui/react-select": "^2.0.0",
    "@radix-ui/react-toast": "^1.1.5",
    "class-variance-authority": "^0.7.0",
    "clsx": "^2.1.0",
    "lucide-react": "^0.363.0",
    "tailwind-merge": "^2.2.2"
  },
  "devDependencies": {
    "@types/react": "^18.2.66",
    "@types/react-dom": "^18.2.22",
    "@typescript-eslint/eslint-plugin": "^7.2.0",
    "@typescript-eslint/parser": "^7.2.0",
    "@vitejs/plugin-react": "^4.2.1",
    "vite": "^5.2.0",
    "typescript": "^5.4.3",
    "tailwindcss": "^3.4.1",
    "postcss": "^8.4.38",
    "autoprefixer": "^10.4.19",
    "vitest": "^1.4.0",
    "@vitest/ui": "^1.4.0",
    "@testing-library/react": "^14.2.2",
    "@testing-library/jest-dom": "^6.4.2",
    "@testing-library/user-event": "^14.5.2",
    "@playwright/test": "^1.42.1",
    "msw": "^2.2.13",
    "eslint": "^8.57.0",
    "prettier": "^3.2.5",
    "tailwindcss-animate": "^1.0.7"
  }
}
```

### 10.2 vite.config.ts

```typescript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import path from 'path';

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
});
```

### 10.3 tsconfig.json

```json
{
  "compilerOptions": {
    "target": "ES2020",
    "useDefineForClassFields": true,
    "lib": ["ES2020", "DOM", "DOM.Iterable"],
    "module": "ESNext",
    "skipLibCheck": true,
    "moduleResolution": "bundler",
    "allowImportingTsExtensions": true,
    "resolveJsonModule": true,
    "isolatedModules": true,
    "noEmit": true,
    "jsx": "react-jsx",
    "strict": true,
    "noUnusedLocals": true,
    "noUnusedParameters": true,
    "noFallthroughCasesInSwitch": true,
    "baseUrl": ".",
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["src"],
  "references": [{ "path": "./tsconfig.node.json" }]
}
```

### 10.4 tailwind.config.js

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: [
    './pages/**/*.{ts,tsx}',
    './components/**/*.{ts,tsx}',
    './app/**/*.{ts,tsx}',
    './src/**/*.{ts,tsx}',
  ],
  theme: {
    container: {
      center: true,
      padding: '2rem',
      screens: {
        '2xl': '1400px',
      },
    },
    extend: {
      colors: {
        border: 'hsl(var(--border))',
        input: 'hsl(var(--input))',
        ring: 'hsl(var(--ring))',
        background: 'hsl(var(--background))',
        foreground: 'hsl(var(--foreground))',
        primary: {
          DEFAULT: 'hsl(var(--primary))',
          foreground: 'hsl(var(--primary-foreground))',
        },
        secondary: {
          DEFAULT: 'hsl(var(--secondary))',
          foreground: 'hsl(var(--secondary-foreground))',
        },
        destructive: {
          DEFAULT: 'hsl(var(--destructive))',
          foreground: 'hsl(var(--destructive-foreground))',
        },
        success: {
          DEFAULT: 'hsl(142 76% 36%)',
          foreground: 'hsl(142 76% 96%)',
        },
        muted: {
          DEFAULT: 'hsl(var(--muted))',
          foreground: 'hsl(var(--muted-foreground))',
        },
        accent: {
          DEFAULT: 'hsl(var(--accent))',
          foreground: 'hsl(var(--accent-foreground))',
        },
      },
      borderRadius: {
        lg: 'var(--radius)',
        md: 'calc(var(--radius) - 2px)',
        sm: 'calc(var(--radius) - 4px)',
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
};
```

### 10.5 .env.example

```
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=BarbApp Admin
```

### 10.6 .eslintrc.cjs

```javascript
module.exports = {
  root: true,
  env: { browser: true, es2020: true },
  extends: [
    'eslint:recommended',
    'plugin:@typescript-eslint/recommended',
    'plugin:react-hooks/recommended',
  ],
  ignorePatterns: ['dist', '.eslintrc.cjs'],
  parser: '@typescript-eslint/parser',
  plugins: ['react-refresh'],
  rules: {
    'react-refresh/only-export-components': [
      'warn',
      { allowConstantExport: true },
    ],
  },
};
```

---

## 11. Guia de Implementação

### 11.1 Setup Inicial

```bash
# 1. Criar projeto Vite
npm create vite@latest barbapp-admin -- --template react-ts

# 2. Entrar no diretório
cd barbapp-admin

# 3. Instalar dependências base
npm install

# 4. Instalar TailwindCSS
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p

# 5. Configurar shadcn/ui
npx shadcn-ui@latest init

# 6. Instalar dependências adicionais
npm install react-router-dom react-hook-form @hookform/resolvers zod axios

# 7. Instalar componentes shadcn/ui
npx shadcn-ui@latest add button input label table dialog toast skeleton badge select form

# 8. Instalar dependências de teste
npm install -D vitest @vitest/ui @testing-library/react @testing-library/jest-dom @testing-library/user-event @playwright/test msw

# 9. Inicializar Playwright
npx playwright install

# 10. Criar arquivo de ambiente
cp .env.example .env
```

### 11.2 Cronograma de Implementação

| Fase | Descrição | Dias | Prioridade |
|------|-----------|------|-----------|
| **Fase 1: Fundação** | Setup Vite + TailwindCSS + shadcn/ui + Estrutura de pastas | 1-2 | 🔴 Crítica |
| **Fase 2: Autenticação** | Login + useAuth + ProtectedRoute + Interceptors | 1-2 | 🔴 Crítica |
| **Fase 3: CRUD Básico** | Listagem + Cadastro + Validação + Integração API | 2-3 | 🔴 Crítica |
| **Fase 4: CRUD Avançado** | Edição + Detalhes + Desativar/Reativar + Modal | 2-3 | 🟡 Alta |
| **Fase 5: UI/UX** | Loading states + Empty states + Error handling + Toasts | 1-2 | 🟡 Alta |
| **Fase 6: Testes** | Unitários + Integração + E2E (>70% coverage) | 2-3 | 🟡 Alta |
| **Fase 7: Refinamento** | Acessibilidade + Performance + Docs + README | 1-2 | 🟢 Média |

**Total Estimado:** 10-17 dias

### 11.3 Checklist de Implementação

#### ✅ Fundação
- [ ] Criar projeto Vite + TypeScript
- [ ] Configurar TailwindCSS
- [ ] Configurar shadcn/ui
- [ ] Criar estrutura de pastas
- [ ] Configurar path aliases (@/)
- [ ] Criar arquivos de configuração (.env, tsconfig, vite.config)

#### ✅ Serviços e Tipos
- [ ] Configurar Axios (api.ts)
- [ ] Criar barbershop.service.ts com todos os métodos
- [ ] Definir tipos TypeScript (Barbershop, Address, Pagination)
- [ ] Criar schemas Zod (barbershopSchema, loginSchema)

#### ✅ Autenticação
- [ ] Criar página de Login
- [ ] Implementar hook useAuth
- [ ] Criar ProtectedRoute component
- [ ] Configurar interceptors do Axios
- [ ] Testar fluxo de autenticação

#### ✅ Roteamento
- [ ] Configurar React Router
- [ ] Criar rotas protegidas
- [ ] Criar Header component com logout
- [ ] Testar navegação entre páginas

#### ✅ CRUD - Listagem
- [ ] Criar página BarbershopList
- [ ] Implementar hook useBarbershops
- [ ] Criar BarbershopTable component
- [ ] Implementar filtros (busca + status)
- [ ] Implementar paginação
- [ ] Criar BarbershopTableSkeleton
- [ ] Criar EmptyState

#### ✅ CRUD - Cadastro
- [ ] Criar página BarbershopCreate
- [ ] Criar BarbershopForm component
- [ ] Implementar validação com Zod
- [ ] Implementar máscaras (telefone, CEP)
- [ ] Implementar submissão do formulário
- [ ] Testar validações e feedback de erro

#### ✅ CRUD - Edição
- [ ] Criar página BarbershopEdit
- [ ] Implementar carregamento de dados existentes
- [ ] Implementar detecção de alterações (dirty state)
- [ ] Implementar confirmação ao sair sem salvar
- [ ] Testar atualização

#### ✅ CRUD - Detalhes
- [ ] Criar página BarbershopDetails
- [ ] Implementar exibição read-only
- [ ] Adicionar badge de status
- [ ] Adicionar informações de auditoria
- [ ] Implementar botões de ação

#### ✅ Desativar/Reativar
- [ ] Criar DeactivateModal component
- [ ] Implementar lógica de desativação
- [ ] Implementar lógica de reativação
- [ ] Adicionar confirmação antes de desativar
- [ ] Testar fluxo completo

#### ✅ UI/UX
- [ ] Implementar loading states em todas as ações
- [ ] Implementar toasts para feedback
- [ ] Implementar tratamento de erros
- [ ] Verificar responsividade mobile
- [ ] Testar em diferentes tamanhos de tela

#### ✅ Testes
- [ ] Configurar Vitest + React Testing Library
- [ ] Escrever testes unitários de componentes (>70% coverage)
- [ ] Escrever testes de hooks
- [ ] Escrever testes de integração de services
- [ ] Configurar MSW para mock de API
- [ ] Escrever testes E2E com Playwright
- [ ] Verificar cobertura de testes

#### ✅ Acessibilidade
- [ ] Adicionar labels em todos os inputs
- [ ] Adicionar ARIA labels em botões de ação
- [ ] Testar navegação por teclado
- [ ] Verificar contraste de cores (WCAG AA)
- [ ] Testar com leitor de tela

#### ✅ Performance
- [ ] Implementar lazy loading de rotas
- [ ] Implementar debounce em busca
- [ ] Verificar bundle size (<500KB)
- [ ] Otimizar re-renders desnecessários

#### ✅ Documentação
- [ ] Criar README.md
- [ ] Documentar estrutura do projeto
- [ ] Documentar scripts disponíveis
- [ ] Adicionar JSDoc nos componentes principais
- [ ] Criar guia de contribuição

---

## 12. Próximos Passos (Pós-MVP)

### 12.1 Melhorias de UX
- [ ] Undo/Redo de ações críticas
- [ ] Drag & drop para upload de logo da barbearia
- [ ] Filtros avançados (múltiplos critérios combinados)
- [ ] Exportação CSV/Excel da listagem
- [ ] Dark mode
- [ ] Infinite scroll como alternativa à paginação

### 12.2 Features Adicionais
- [ ] Dashboard com métricas (total de barbearias, crescimento, etc.)
- [ ] Gestão de serviços oferecidos por barbearia
- [ ] Gestão de barbeiros vinculados
- [ ] Configuração de horários de funcionamento
- [ ] Integração com sistema de agendamento
- [ ] Notificações em tempo real (WebSocket)

### 12.3 Melhorias Técnicas
- [ ] Migrar estado para React Query (cache e sincronização)
- [ ] Adicionar Zustand para estado de UI global
- [ ] Implementar Storybook para documentação de componentes
- [ ] Adicionar Sentry para monitoramento de erros
- [ ] Implementar CI/CD com GitHub Actions
- [ ] Containerizar com Docker
- [ ] Configurar CDN para assets estáticos

---

## 13. Referências

### 13.1 PRDs Originais
- [Listagem de Barbearias](./listagem-barbearia.md)
- [Cadastro de Barbearia](./cadastro-barbearia.md)
- [Edição de Barbearia](./edicao-barbearia.md)
- [Detalhes de Barbearia](./detalhe-barbearia.md)
- [Confirmação de Desativação](./confirmar-desativacao-barbearia.md)

### 13.2 Referências de Design
- [gerenciamento-barbearia.html](./gerenciamento-barbearia.html)
- [tela-login.html](./tela-login.html)

### 13.3 API Backend
- Documentação da API: `/src/BarbApp.API`
- Controllers: `/src/BarbApp.API/Controllers/BarbershopsController.cs`
- Application Layer: `/src/BarbApp.Application/Services/BarbershopService.cs`

### 13.4 Documentação Externa
- [React](https://react.dev/)
- [Vite](https://vitejs.dev/)
- [TailwindCSS](https://tailwindcss.com/)
- [shadcn/ui](https://ui.shadcn.com/)
- [React Hook Form](https://react-hook-form.com/)
- [Zod](https://zod.dev/)
- [React Router](https://reactrouter.com/)
- [Vitest](https://vitest.dev/)
- [Playwright](https://playwright.dev/)

---

## 14. Contato e Suporte

Para dúvidas sobre esta especificação técnica ou implementação:

- **Responsável Técnico:** Equipe BarbApp
- **Documento Gerado:** 2025-10-13
- **Versão:** 1.0
- **Status:** Aprovado para Desenvolvimento

---

**FIM DA ESPECIFICAÇÃO TÉCNICA**
