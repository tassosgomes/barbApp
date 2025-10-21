# Relatório de Revisão - Tarefa 11.0: Hook useLandingPage e API Service

**Data**: 2025-10-21  
**Revisor**: GitHub Copilot  
**Status da Tarefa**: ❌ **NÃO CONCLUÍDA**

---

## 1. Resumo Executivo

### 1.1. Visão Geral
A Tarefa 11.0 visa implementar o hook customizado `useLandingPage` e o serviço de API `landing-page.api.ts` para gerenciar estado e operações da landing page no painel admin, incluindo integração com backend via TanStack Query.

### 1.2. Status Atual
**CRÍTICO**: A tarefa está **INCOMPLETA**. Análise detalhada revela que:

- ✅ **Parcialmente Implementado**: Types e interfaces criados (Tarefa 10.0)
- ❌ **NÃO IMPLEMENTADO**: Hook `useLandingPage`
- ❌ **NÃO IMPLEMENTADO**: Serviço de API `landing-page.api.ts`
- ❌ **NÃO IMPLEMENTADO**: Integração com TanStack Query
- ❌ **NÃO IMPLEMENTADO**: Tratamento de erros e toasts
- ❌ **NÃO IMPLEMENTADO**: Testes unitários

### 1.3. Veredicto Final
🔴 **REPROVADO** - A implementação está ausente. Apenas a base de types foi criada pela Tarefa 10.0. Os componentes principais (hook e API service) não foram implementados.

---

## 2. Validação da Definição da Tarefa

### 2.1. Alinhamento com PRD
**Status**: ✅ Requisitos bem definidos no PRD

| Requisito PRD | Relevância | Cobertura |
|---------------|-----------|-----------|
| Gestão de configuração da landing page | ✅ | ❌ Não implementado |
| Atualização de informações | ✅ | ❌ Não implementado |
| Upload de logo | ✅ | ❌ Não implementado |
| Integração com backend | ✅ | ❌ Não implementado |

### 2.2. Alinhamento com Tech Spec
**Status**: ✅ Especificação técnica completa em `techspec-frontend.md` seção 1.3

O Tech Spec fornece:
- ✅ Código completo do hook `useLandingPage`
- ✅ Código completo do API service
- ✅ Exemplos de integração com TanStack Query
- ✅ Tratamento de erros e toasts

**Problema**: Código de referência existe na spec, mas NÃO foi implementado no projeto.

### 2.3. Alinhamento com Arquivo de Tarefa
**Status**: ✅ Taref definida claramente em `11_task.md`

**Subtarefas Definidas**:
- [ ] 11.1 Criar `services/api/landing-page.api.ts` ❌
- [ ] 11.2 Criar hook `useLandingPage.ts` ❌
- [ ] 11.3 Implementar query para buscar config ❌
- [ ] 11.4 Implementar mutation para atualizar ❌
- [ ] 11.5 Adicionar tratamento de erros e toasts ❌
- [ ] 11.6 Criar testes do hook ❌

**Todas as subtarefas estão pendentes.**

### 2.4. Dependências
**Status**: ⚠️ Bloqueadores parcialmente resolvidos

| Dependência | Status | Impacto |
|-------------|--------|---------|
| Tarefa 10.0 (Types) | ✅ Concluída | Sem bloqueio |
| Tarefa 5.0 (API Endpoints Backend) | ⚠️ Status desconhecido | **BLOQUEIO POTENCIAL** |

**Ação Necessária**: Verificar se a Tarefa 5.0 (API Backend) está completa antes de prosseguir.

---

## 3. Análise de Regras e Revisão de Código

### 3.1. Regras Aplicáveis

#### 3.1.1. `rules/react.md`
**Regras Relevantes**:
- ✅ Utilizar componentes funcionais (N/A - é um hook)
- ✅ Utilizar TypeScript e extensão .ts
- ✅ React Query para comunicação com API
- ✅ Nomear hooks com "use"
- ❌ **VIOLAÇÃO**: Criar testes automatizados para todos os componentes/hooks

**Conformidade**: 0% (nenhum código implementado)

#### 3.1.2. `rules/code-standard.md`
**Regras Relevantes**:
- ✅ Utilizar camelCase para funções e variáveis
- ✅ Evitar métodos longos (máx. 50 linhas)
- ✅ Declarar constantes para magic numbers
- ✅ Inverter dependências (Dependency Inversion)

**Conformidade**: N/A (aguardando implementação)

#### 3.1.3. `rules/tests-react.md`
**Regras Relevantes**:
- ❌ **VIOLAÇÃO CRÍTICA**: Criar testes de unidade para todos os custom hooks
- ❌ **VIOLAÇÃO**: Utilizar React Testing Library (`renderHook`)
- ❌ **VIOLAÇÃO**: Estrutura AAA (Arrange, Act, Assert)
- ❌ **VIOLAÇÃO**: Mockar dependências externas (API calls)

**Conformidade**: 0% (testes ausentes)

### 3.2. Análise da Estrutura do Projeto

**Estrutura Existente**:
```
barbapp-admin/src/
├── features/
│   └── landing-page/
│       ├── types/           ✅ Criado
│       │   └── landing-page.types.ts
│       ├── constants/       ✅ Criado
│       │   ├── templates.ts
│       │   └── validation.ts
│       └── index.ts         ✅ Criado
├── hooks/                   ❌ Hook ausente
│   └── (nenhum landing page hook)
└── services/                ❌ API service ausente
    └── (nenhum landing-page.api.ts)
```

**Problemas Identificados**:
1. ❌ Diretório `features/landing-page/hooks/` não existe
2. ❌ Arquivo `hooks/useLandingPage.ts` não existe
3. ❌ Diretório `features/landing-page/services/` não existe
4. ❌ Arquivo `services/api/landing-page.api.ts` não existe
5. ❌ Nenhum teste em `__tests__/`

---

## 4. Problemas Críticos Identificados

### 4.1. Implementação Ausente

#### 🔴 **CRÍTICO 1: Hook `useLandingPage` não implementado**
**Severidade**: ALTA  
**Impacto**: Bloqueia todas as tarefas dependentes (17.0, 18.0)

**Evidência**:
- Busca por `useLandingPage` retorna apenas referências em types
- Arquivo `hooks/useLandingPage.ts` não existe
- Nenhuma implementação de TanStack Query encontrada

**Ação Requerida**:
```typescript
// Criar: barbapp-admin/src/features/landing-page/hooks/useLandingPage.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { landingPageApi } from '@/services/api/landing-page.api';
import { toast } from '@/components/ui/use-toast';
import type { UseLandingPageResult, UpdateLandingPageInput } from '../types/landing-page.types';

export const useLandingPage = (barbershopId: string): UseLandingPageResult => {
  const queryClient = useQueryClient();

  const { data: config, isLoading, error } = useQuery({
    queryKey: ['landingPage', barbershopId],
    queryFn: () => landingPageApi.getConfig(barbershopId),
    staleTime: 5 * 60 * 1000,
  });

  const updateMutation = useMutation({
    mutationFn: (data: UpdateLandingPageInput) =>
      landingPageApi.updateConfig(barbershopId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['landingPage', barbershopId] });
      toast({
        title: 'Sucesso!',
        description: 'Landing page atualizada com sucesso.',
        variant: 'success',
      });
    },
    onError: (error: any) => {
      toast({
        title: 'Erro',
        description: error.message || 'Erro ao atualizar landing page.',
        variant: 'destructive',
      });
    },
  });

  return {
    config,
    isLoading,
    error: error as Error,
    updateConfig: updateMutation.mutateAsync,
    isUpdating: updateMutation.isPending,
    refetch: async () => {
      await queryClient.invalidateQueries({ queryKey: ['landingPage', barbershopId] });
    },
  };
};
```

#### 🔴 **CRÍTICO 2: API Service não implementado**
**Severidade**: ALTA  
**Impacto**: Hook não pode funcionar sem o service

**Evidência**:
- Arquivo `services/api/landing-page.api.ts` não existe
- Nenhuma integração com axios/fetch encontrada
- Endpoints definidos no backend não têm client correspondente

**Ação Requerida**:
```typescript
// Criar: barbapp-admin/src/services/api/landing-page.api.ts
import { api } from './api';
import type { 
  LandingPageConfig, 
  UpdateLandingPageInput,
  LandingPageConfigOutput 
} from '@/features/landing-page/types/landing-page.types';

export const landingPageApi = {
  /**
   * Busca configuração da landing page
   */
  getConfig: async (barbershopId: string): Promise<LandingPageConfig> => {
    const { data } = await api.get<LandingPageConfigOutput>(
      `/admin/landing-pages/${barbershopId}`
    );
    return data.landingPage;
  },

  /**
   * Atualiza configuração da landing page
   */
  updateConfig: async (
    barbershopId: string,
    payload: UpdateLandingPageInput
  ): Promise<void> => {
    await api.put(`/admin/landing-pages/${barbershopId}`, payload);
  },

  /**
   * Upload de logo
   */
  uploadLogo: async (barbershopId: string, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('logo', file);

    const { data } = await api.post<{ logoUrl: string }>(
      `/admin/landing-pages/${barbershopId}/logo`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      }
    );

    return data.logoUrl;
  },
};
```

#### 🔴 **CRÍTICO 3: Testes completamente ausentes**
**Severidade**: ALTA  
**Impacto**: Violação de regras do projeto + impossível validar funcionamento

**Evidência**:
- Nenhum arquivo de teste em `__tests__/`
- Busca por `landing-page.test` retorna vazio
- Regra `tests-react.md` exige testes para todos os hooks

**Ação Requerida**:
```typescript
// Criar: barbapp-admin/src/features/landing-page/hooks/__tests__/useLandingPage.test.ts
import { renderHook, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useLandingPage } from '../useLandingPage';
import { landingPageApi } from '@/services/api/landing-page.api';

jest.mock('@/services/api/landing-page.api');

const createWrapper = () => {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return ({ children }: any) => (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
};

describe('useLandingPage', () => {
  const mockBarbershopId = '123';
  const mockConfig = {
    id: '1',
    barbershopId: mockBarbershopId,
    templateId: 1,
    whatsappNumber: '+5511999999999',
    isPublished: true,
    services: [],
    updatedAt: '2025-10-21',
    createdAt: '2025-10-21',
  };

  beforeEach(() => {
    jest.clearAllMocks();
  });

  it('should fetch landing page config', async () => {
    (landingPageApi.getConfig as jest.Mock).mockResolvedValue(mockConfig);

    const { result } = renderHook(() => useLandingPage(mockBarbershopId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.config).toEqual(mockConfig);
    expect(landingPageApi.getConfig).toHaveBeenCalledWith(mockBarbershopId);
  });

  it('should update landing page config', async () => {
    (landingPageApi.getConfig as jest.Mock).mockResolvedValue(mockConfig);
    (landingPageApi.updateConfig as jest.Mock).mockResolvedValue(undefined);

    const { result } = renderHook(() => useLandingPage(mockBarbershopId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    const updateData = { aboutText: 'Novo texto' };
    await result.current.updateConfig(updateData);

    expect(landingPageApi.updateConfig).toHaveBeenCalledWith(
      mockBarbershopId,
      updateData
    );
  });

  it('should handle errors', async () => {
    const mockError = new Error('API Error');
    (landingPageApi.getConfig as jest.Mock).mockRejectedValue(mockError);

    const { result } = renderHook(() => useLandingPage(mockBarbershopId), {
      wrapper: createWrapper(),
    });

    await waitFor(() => expect(result.current.isLoading).toBe(false));

    expect(result.current.error).toBeDefined();
  });
});
```

### 4.2. Problemas de Arquitetura

#### ⚠️ **MÉDIO 1: Organização de diretórios inconsistente**
**Severidade**: MÉDIA  
**Impacto**: Dificulta manutenção e escalabilidade

**Problema**:
- Types estão em `features/landing-page/types/`
- Hooks deveriam estar em `features/landing-page/hooks/` mas estão ausentes
- Services deveriam estar em `services/api/` mas estão ausentes

**Recomendação**:
```
barbapp-admin/src/features/landing-page/
├── hooks/
│   ├── useLandingPage.ts
│   ├── useTemplates.ts
│   ├── useLogoUpload.ts
│   └── __tests__/
│       ├── useLandingPage.test.ts
│       ├── useTemplates.test.ts
│       └── useLogoUpload.test.ts
├── services/
│   └── api/
│       └── landing-page.api.ts
├── types/
│   └── landing-page.types.ts
└── constants/
    ├── templates.ts
    └── validation.ts
```

#### ⚠️ **MÉDIO 2: Falta integração com instância API existente**
**Severidade**: MÉDIA  
**Impacto**: Pode causar duplicação de configuração HTTP

**Problema**:
- Projeto já tem instância configurada do axios em `services/api.ts`
- API service deve reutilizar essa instância

**Recomendação**:
```typescript
// Importar instância existente
import { api } from '../api';

// Usar api em vez de criar nova instância axios
export const landingPageApi = {
  getConfig: async (barbershopId: string) => {
    const { data } = await api.get(`/admin/landing-pages/${barbershopId}`);
    return data;
  },
  // ...
};
```

### 4.3. Problemas de Qualidade

#### ⚠️ **MÉDIO 3: Falta validação de entrada**
**Severidade**: MÉDIA  
**Impacto**: Erros podem ser enviados ao backend desnecessariamente

**Recomendação**:
```typescript
export const useLandingPage = (barbershopId: string) => {
  // Validar barbershopId
  if (!barbershopId || !barbershopId.trim()) {
    throw new Error('barbershopId é obrigatório');
  }

  // ... resto do hook
};
```

#### ⚠️ **BAIXO 1: Falta documentação JSDoc**
**Severidade**: BAIXA  
**Impacto**: Dificulta uso e manutenção

**Recomendação**:
```typescript
/**
 * Hook customizado para gerenciar estado e operações da landing page.
 * 
 * @param barbershopId - ID da barbearia
 * @returns Objeto com config, loading, error e funções de atualização
 * 
 * @example
 * ```typescript
 * const { config, updateConfig, isUpdating } = useLandingPage('123');
 * 
 * await updateConfig({
 *   aboutText: 'Nova descrição'
 * });
 * ```
 */
export const useLandingPage = (barbershopId: string): UseLandingPageResult => {
  // ...
};
```

---

## 5. Validação de Testes e Cobertura

### 5.1. Status Atual
❌ **NENHUM TESTE IMPLEMENTADO**

**Evidência**:
- Busca por `__tests__/**/*landing-page*` retorna vazio
- Nenhum arquivo `.test.ts` ou `.spec.ts` encontrado
- Cobertura de testes: **0%**

### 5.2. Testes Necessários

#### 5.2.1. Testes Unitários do Hook
**Arquivo**: `hooks/__tests__/useLandingPage.test.ts`

**Casos de Teste Obrigatórios**:
- ✅ Deve buscar configuração da landing page
- ✅ Deve atualizar configuração com sucesso
- ✅ Deve invalidar cache após atualização
- ✅ Deve exibir toast de sucesso após atualização
- ✅ Deve tratar erros de busca
- ✅ Deve tratar erros de atualização
- ✅ Deve exibir toast de erro em falhas
- ✅ Deve fazer refetch manualmente
- ✅ Deve respeitar staleTime de 5 minutos

#### 5.2.2. Testes de Integração do API Service
**Arquivo**: `services/api/__tests__/landing-page.api.test.ts`

**Casos de Teste Obrigatórios**:
- ✅ Deve chamar endpoint correto para getConfig
- ✅ Deve chamar endpoint correto para updateConfig
- ✅ Deve chamar endpoint correto para uploadLogo
- ✅ Deve enviar FormData corretamente no upload
- ✅ Deve propagar erros HTTP

### 5.3. Configuração de Mocks

**Mock necessário** para `landingPageApi`:
```typescript
// __mocks__/landing-page.api.ts
export const landingPageApi = {
  getConfig: jest.fn(),
  updateConfig: jest.fn(),
  uploadLogo: jest.fn(),
};
```

### 5.4. Conformidade com Regras de Teste

**Checklist `tests-react.md`**:
- ❌ Utilizar Jest como framework
- ❌ Utilizar React Testing Library
- ❌ Utilizar `renderHook` para hooks
- ❌ Estrutura AAA (Arrange, Act, Assert)
- ❌ Mockar dependências externas
- ❌ Usar `beforeEach` para limpeza
- ❌ Testes independentes e repetíveis

**Conformidade**: 0/7 (0%)

---

## 6. Checklist de Conclusão da Tarefa

### 6.1. Subtarefas
| ID | Subtarefa | Status | Prioridade |
|----|-----------|--------|------------|
| 11.1 | Criar `services/api/landing-page.api.ts` | ❌ Pendente | P0 |
| 11.2 | Criar hook `useLandingPage.ts` | ❌ Pendente | P0 |
| 11.3 | Implementar query para buscar config | ❌ Pendente | P0 |
| 11.4 | Implementar mutation para atualizar | ❌ Pendente | P0 |
| 11.5 | Adicionar tratamento de erros e toasts | ❌ Pendente | P0 |
| 11.6 | Criar testes do hook | ❌ Pendente | P0 |

**Progresso**: 0/6 (0%)

### 6.2. Critérios de Sucesso
| Critério | Status | Observações |
|----------|--------|-------------|
| Hook funcionando e integrando com API | ❌ | Hook não existe |
| Cache e invalidação automática | ❌ | TanStack Query não configurado |
| Toasts de sucesso/erro | ❌ | Tratamento de erros ausente |
| Testes unitários passando | ❌ | Testes não existem |

**Progresso**: 0/4 (0%)

### 6.3. Requisitos Obrigatórios (PRD)
| Requisito | Status | Bloqueador? |
|-----------|--------|-------------|
| Integração com API backend | ❌ | Sim |
| Gerenciamento de estado com React Query | ❌ | Sim |
| Tratamento de erros | ❌ | Não |
| Feedback visual (toasts) | ❌ | Não |
| Cache de 5 minutos | ❌ | Não |

---

## 7. Plano de Ação Corretiva

### 7.1. Passos Imediatos (Prioridade P0)

#### **Passo 1: Verificar Tarefa 5.0 (Backend)**
**Responsável**: Dev Backend  
**Prazo**: Antes de iniciar frontend  
**Ação**:
```bash
# Verificar se endpoints existem
curl -X GET http://localhost:5000/api/admin/landing-pages/{id}
curl -X PUT http://localhost:5000/api/admin/landing-pages/{id}
```

**Validação**:
- ✅ Endpoint GET retorna 200
- ✅ Endpoint PUT retorna 204
- ✅ Autenticação funcionando
- ✅ Swagger documentado

#### **Passo 2: Criar API Service**
**Arquivo**: `barbapp-admin/src/services/api/landing-page.api.ts`  
**Prazo**: 1 hora  
**Dependências**: Tarefa 5.0 (Backend) completa

**Implementação**:
```typescript
import { api } from './api';
import type { 
  LandingPageConfig, 
  UpdateLandingPageInput,
  LandingPageConfigOutput 
} from '@/features/landing-page/types/landing-page.types';

export const landingPageApi = {
  getConfig: async (barbershopId: string): Promise<LandingPageConfig> => {
    const { data } = await api.get<LandingPageConfigOutput>(
      `/admin/landing-pages/${barbershopId}`
    );
    return data.landingPage;
  },

  updateConfig: async (
    barbershopId: string,
    payload: UpdateLandingPageInput
  ): Promise<void> => {
    await api.put(`/admin/landing-pages/${barbershopId}`, payload);
  },

  uploadLogo: async (barbershopId: string, file: File): Promise<string> => {
    const formData = new FormData();
    formData.append('logo', file);

    const { data } = await api.post<{ logoUrl: string }>(
      `/admin/landing-pages/${barbershopId}/logo`,
      formData,
      {
        headers: { 'Content-Type': 'multipart/form-data' },
      }
    );

    return data.logoUrl;
  },
};
```

**Validação**:
```bash
# Testar importação
npm run build
# Verificar erros de TypeScript
```

#### **Passo 3: Criar Hook useLandingPage**
**Arquivo**: `barbapp-admin/src/features/landing-page/hooks/useLandingPage.ts`  
**Prazo**: 2 horas  
**Dependências**: Passo 2

**Implementação**: Ver código completo na seção 4.1 (CRÍTICO 1)

**Validação**:
- ✅ Hook exporta todas as funções definidas em `UseLandingPageResult`
- ✅ TypeScript compila sem erros
- ✅ Query com `staleTime` de 5 minutos
- ✅ Mutation invalida cache corretamente
- ✅ Toasts de sucesso/erro funcionando

#### **Passo 4: Criar Testes Unitários**
**Arquivo**: `barbapp-admin/src/features/landing-page/hooks/__tests__/useLandingPage.test.ts`  
**Prazo**: 2 horas  
**Dependências**: Passo 3

**Implementação**: Ver código completo na seção 4.1 (CRÍTICO 3)

**Comandos**:
```bash
# Rodar testes
npm test -- useLandingPage.test.ts

# Verificar cobertura
npm test -- --coverage useLandingPage.test.ts
```

**Validação**:
- ✅ Todos os testes passando
- ✅ Cobertura > 80%
- ✅ Mocks funcionando corretamente

#### **Passo 5: Integração e Validação**
**Prazo**: 1 hora  
**Dependências**: Passos 1-4

**Checklist Final**:
- [ ] API service integrado com axios existente
- [ ] Hook usando API service corretamente
- [ ] TanStack Query configurado no contexto da aplicação
- [ ] Toast component importado e funcionando
- [ ] Testes passando
- [ ] TypeScript sem erros
- [ ] Code review aprovado

### 7.2. Passos Adicionais (Prioridade P1)

#### **Documentação**
- [ ] Adicionar JSDoc em todas as funções
- [ ] Criar exemplos de uso no README
- [ ] Documentar tipos exportados

#### **Qualidade**
- [ ] Adicionar validação de entrada
- [ ] Implementar retry logic para falhas de rede
- [ ] Adicionar logging de erros

---

## 8. Recomendações Técnicas

### 8.1. Arquitetura

#### Recomendação 1: Centralizar hooks de feature
```
features/landing-page/
├── hooks/
│   ├── index.ts          # Exporta todos os hooks
│   ├── useLandingPage.ts
│   ├── useTemplates.ts
│   └── useLogoUpload.ts
```

**Benefícios**:
- ✅ Facilita importação
- ✅ Melhora organização
- ✅ Segue padrão do projeto

#### Recomendação 2: Criar hook customizado de toast
```typescript
// hooks/useToast.ts
export const useLandingPageToast = () => {
  const { toast } = useToast();

  return {
    success: (message: string) =>
      toast({ title: 'Sucesso!', description: message, variant: 'success' }),
    error: (error: Error) =>
      toast({ title: 'Erro', description: error.message, variant: 'destructive' }),
  };
};
```

**Benefícios**:
- ✅ Reutilizável
- ✅ Consistente
- ✅ Testável isoladamente

### 8.2. Performance

#### Recomendação 3: Implementar optimistic updates
```typescript
const updateMutation = useMutation({
  mutationFn: (data: UpdateLandingPageInput) =>
    landingPageApi.updateConfig(barbershopId, data),
  onMutate: async (newData) => {
    // Cancelar queries em andamento
    await queryClient.cancelQueries({ queryKey: ['landingPage', barbershopId] });

    // Snapshot do estado anterior
    const previousData = queryClient.getQueryData(['landingPage', barbershopId]);

    // Atualização otimista
    queryClient.setQueryData(['landingPage', barbershopId], (old: any) => ({
      ...old,
      ...newData,
    }));

    return { previousData };
  },
  onError: (err, newData, context) => {
    // Rollback em caso de erro
    queryClient.setQueryData(['landingPage', barbershopId], context?.previousData);
  },
});
```

**Benefícios**:
- ✅ UX mais responsiva
- ✅ Reduz perceived latency
- ✅ Rollback automático em erros

#### Recomendação 4: Prefetch de dados
```typescript
export const usePrefetchLandingPage = (barbershopId: string) => {
  const queryClient = useQueryClient();

  return () => {
    queryClient.prefetchQuery({
      queryKey: ['landingPage', barbershopId],
      queryFn: () => landingPageApi.getConfig(barbershopId),
      staleTime: 5 * 60 * 1000,
    });
  };
};
```

**Uso**:
```typescript
// Em menu ou navegação
const prefetch = usePrefetchLandingPage(barbershopId);
<button onMouseEnter={prefetch}>Landing Page</button>
```

### 8.3. Segurança

#### Recomendação 5: Validar barbershopId
```typescript
export const useLandingPage = (barbershopId: string) => {
  if (!barbershopId || typeof barbershopId !== 'string') {
    throw new Error('barbershopId inválido');
  }

  // ... resto do hook
};
```

#### Recomendação 6: Implementar rate limiting no client
```typescript
import { throttle } from 'lodash-es';

export const useLandingPage = (barbershopId: string) => {
  const updateConfig = useMemo(
    () =>
      throttle((data) => updateMutation.mutate(data), 1000, {
        leading: true,
        trailing: false,
      }),
    [updateMutation]
  );

  return { updateConfig, ... };
};
```

---

## 9. Impacto em Tarefas Dependentes

### 9.1. Tarefas Bloqueadas

#### Tarefa 17.0: Componentes de Edição
**Status**: 🔴 Bloqueada  
**Impacto**: ALTO  
**Motivo**: Componentes precisam do hook `useLandingPage`

**Dependências**:
- ❌ `useLandingPage` para gerenciar estado
- ❌ API service para salvar alterações
- ❌ Types definidos (✅ já existe)

**Prazo Estimado**: +4 horas de atraso

#### Tarefa 18.0: Página Principal
**Status**: 🔴 Bloqueada  
**Impacto**: ALTO  
**Motivo**: Página precisa integrar todos os componentes

**Dependências**:
- ❌ Hook `useLandingPage`
- ❌ Componentes de edição (Tarefa 17.0)
- ❌ Templates (outras tarefas)

**Prazo Estimado**: +8 horas de atraso

### 9.2. Cadeia de Dependências

```
Tarefa 11.0 (useLandingPage)
    ↓
Tarefa 17.0 (Componentes de Edição)
    ↓
Tarefa 18.0 (Página Principal)
    ↓
Tarefa 19.0 (Testes E2E)
```

**Risco**: Atraso em cascata de **12+ horas** se não corrigido imediatamente.

---

## 10. Estimativa de Esforço

### 10.1. Tempo de Implementação

| Atividade | Estimativa | Prioridade |
|-----------|------------|------------|
| Criar API Service | 1-2h | P0 |
| Criar Hook useLandingPage | 2-3h | P0 |
| Criar Testes Unitários | 2-3h | P0 |
| Documentação JSDoc | 1h | P1 |
| Code Review + Ajustes | 1-2h | P0 |
| **TOTAL** | **7-11h** | - |

### 10.2. Risco de Atrasos

**Fatores de Risco**:
1. 🔴 **ALTO**: Tarefa 5.0 (Backend) não concluída
2. ⚠️ **MÉDIO**: Falta de familiaridade com TanStack Query
3. ⚠️ **MÉDIO**: Problemas de integração com API existente
4. ✅ **BAIXO**: Types já estão definidos

**Mitigação**:
- Priorizar validação do backend (Tarefa 5.0)
- Pair programming para TanStack Query
- Seguir exemplos de outros hooks no projeto

---

## 11. Conclusão

### 11.1. Resumo de Problemas

#### 🔴 Críticos (Bloqueadores)
1. Hook `useLandingPage` **não implementado**
2. API Service `landing-page.api.ts` **não implementado**
3. Testes unitários **completamente ausentes**
4. Dependência da Tarefa 5.0 (Backend) **não verificada**

#### ⚠️ Importantes (Não-bloqueadores imediatos)
5. Organização de diretórios inconsistente
6. Falta integração com instância API existente
7. Falta validação de entrada
8. Falta documentação JSDoc

### 11.2. Veredicto Final

**Status**: 🔴 **REPROVADA**

**Razões**:
1. ❌ Nenhuma subtarefa foi concluída (0/6)
2. ❌ Nenhum critério de sucesso foi atingido (0/4)
3. ❌ Implementação está 0% completa
4. ❌ Testes estão 0% completos
5. ❌ Bloqueia tarefas críticas (17.0, 18.0)

**Impacto no Projeto**:
- 🔴 Atraso estimado: **12+ horas** em cascata
- 🔴 Risco de não entregar funcionalidade de Landing Page
- 🔴 Bloqueia desenvolvimento do painel admin

### 11.3. Próximos Passos

1. ✅ **IMEDIATO**: Verificar status da Tarefa 5.0 (Backend API)
2. ✅ **URGENTE**: Implementar API Service (1-2h)
3. ✅ **URGENTE**: Implementar Hook useLandingPage (2-3h)
4. ✅ **URGENTE**: Criar testes unitários (2-3h)
5. ⚠️ **IMPORTANTE**: Code review e ajustes (1-2h)
6. ⚠️ **IMPORTANTE**: Documentação (1h)

**Prazo Recomendado**: 1 dia útil (8 horas) para completar a tarefa.

---

## 12. Revisão Final do Revisor

**Revisor**: GitHub Copilot  
**Data**: 2025-10-21  
**Recomendação**: ❌ **REJEITAR** até implementação completa

**Comentários Finais**:

A Tarefa 11.0 está completamente pendente. Apesar de haver uma base sólida de types (Tarefa 10.0), os componentes críticos (hook e API service) não foram implementados. Isso representa um **bloqueio severo** para o progresso do projeto.

A especificação técnica está bem definida no `techspec-frontend.md`, o que facilita a implementação. O código de referência está disponível e apenas precisa ser adaptado ao contexto do projeto.

**Ação Requerida**: 
- Priorizar esta tarefa como **P0 (Crítica)**
- Alocar desenvolvedor frontend experiente com TanStack Query
- Verificar status do backend antes de iniciar
- Implementar com testes desde o início (TDD)
- Fazer code review antes de marcar como concluída

**Estimativa de Recuperação**: 8 horas (1 dia útil) se iniciado imediatamente.

---

**Assinatura Digital**: GitHub Copilot  
**Versão do Relatório**: 1.0  
**Próxima Revisão**: Após implementação completa
