---
status: completed
parallelizable: false
blocked_by: ["24.0", "25.0", "26.0", "27.0", "28.0"]
completed_date: 2025-10-23
---

# Tarefa 29.0: Página LandingPage e Roteador

## Visão Geral
Criar a página principal `LandingPage.tsx` do projeto `barbapp-public`. Esta página será responsável por buscar os dados da landing page com base no código da barbearia na URL, e então renderizar dinamicamente o template correto. Também envolve a configuração do roteador principal da aplicação.

## Requisitos Funcionais (prd.md Seção 5.1)
- **Roteamento Dinâmico**: A aplicação deve responder à rota `/barbearia/:code`.
- **Busca de Dados**: Ao acessar a rota, a página deve usar o `:code` da URL para buscar os dados da landing page correspondente via API.
- **Renderização de Template**: Com base no `templateId` retornado pela API, a página deve renderizar o componente de template correto (ex: `Template1Classic`, `Template2Modern`, etc.).
- **Estados de UI**: A página deve lidar com os estados de `loading` (enquanto os dados estão sendo buscados) e `error` (se a landing page não for encontrada ou a API falhar).

## Detalhes de Implementação (techspec-frontend.md Seção 2.6)
- **Framework**: React com TypeScript.
- **Roteamento**: Utilizar `react-router-dom` para configurar as rotas. O componente `BrowserRouter` deve envolver a aplicação no `main.tsx` ou `App.tsx`.
- **Hooks**: 
  - `useParams` para extrair o `:code` da URL.
  - `useLandingPageData` (criado na Tarefa 22.0) para buscar os dados.
- **Seleção Dinâmica de Componente**: Criar um mapa (objeto) que associa o `templateId` (número) ao componente React correspondente.

## Estrutura da Implementação

**`App.tsx` (ou `main.tsx`)**
```typescript
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { LandingPage } from './pages/LandingPage';

const queryClient = new QueryClient();

function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <Routes>
          <Route path="/barbearia/:code" element={<LandingPage />} />
          {/* Outras rotas públicas, se houver */}
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
  );
}
```

**`pages/LandingPage.tsx`**
```typescript
import { useParams } from 'react-router-dom';
import { useLandingPageData } from '@/hooks/useLandingPageData';
import { Template1Classic } from '@/templates/Template1Classic';
// ... importar outros templates

// Mapa de templates
const TEMPLATE_COMPONENTS: Record<number, React.FC<any>> = {
  1: Template1Classic,
  2: Template2Modern,
  // ... etc.
};

export const LandingPage: React.FC = () => {
  const { code } = useParams<{ code: string }>();
  const { data, isLoading, error } = useLandingPageData(code!);

  if (isLoading) {
    // Renderizar componente de loading
  }

  if (error || !data) {
    // Renderizar componente de erro
  }

  const TemplateComponent = TEMPLATE_COMPONENTS[data.landingPage.templateId] || Template1Classic;

  return <TemplateComponent data={data} />;
};
```

## Critérios de Aceitação
- [x] A aplicação está configurada com `react-router-dom`.
- [x] Acessar `http://localhost:3001/barbearia/CODIGO_QUALQUER` renderiza o componente `LandingPage`.
- [x] O hook `useParams` é usado para extrair o código da URL e passá-lo para o hook `useLandingPageData`.
- [x] Um componente de `loading` (ex: um spinner) é exibido enquanto os dados da API estão sendo buscados.
- [x] Uma mensagem de `erro` clara é exibida se a API retornar um erro 404 ou falhar.
- [x] Após o sucesso da busca, o componente de template correto é renderizado dinamicamente com base no `templateId` recebido.
- [x] Um template padrão (ex: `Template1Classic`) é usado como fallback caso o `templateId` seja inválido.

## ✅ CONCLUÍDA

### 1.0 Implementação completada
- ✅ Componente `LandingPage.tsx` criado com lógica de roteamento e renderização dinâmica
- ✅ Configuração do roteador `react-router-dom` no `App.tsx`
- ✅ Integração com hook `useLandingPageData` para busca de dados
- ✅ Mapa de templates para seleção dinâmica baseada no `templateId`
- ✅ Estados de loading e error implementados com componentes reutilizáveis
- ✅ Fallback para `Template1Classic` em caso de `templateId` inválido

### 1.1 Definição da tarefa, PRD e tech spec validados
- ✅ Requisitos do PRD atendidos (roteamento dinâmico, busca de dados, renderização de templates)
- ✅ Especificações técnicas seguidas (React + TS, react-router-dom, hooks)
- ✅ Critérios de aceitação validados

### 1.2 Análise de regras e conformidade verificadas
- ✅ Padrões de código seguidos (camelCase, PascalCase, early returns)
- ✅ Regras React aplicadas (componentes funcionais, hooks, TypeScript)
- ✅ Testes automatizados criados seguindo diretrizes (8 testes unitários)
- ✅ Sem linting errors

### 1.3 Revisão de código completada
- ✅ Código limpo e bem estruturado
- ✅ Sem duplicação de código
- ✅ Tratamento adequado de erros
- ✅ Performance otimizada

### 1.4 Pronto para deploy
- ✅ Build bem-sucedido
- ✅ Todos os testes passando (108/108)
- ✅ Sem erros de compilação
- ✅ Funcionalidades validadas