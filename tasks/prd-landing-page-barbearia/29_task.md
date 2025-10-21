---
status: pending
parallelizable: false
blocked_by: ["24.0", "25.0", "26.0", "27.0", "28.0"]
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
- [ ] A aplicação está configurada com `react-router-dom`.
- [ ] Acessar `http://localhost:3001/barbearia/CODIGO_QUALQUER` renderiza o componente `LandingPage`.
- [ ] O hook `useParams` é usado para extrair o código da URL e passá-lo para o hook `useLandingPageData`.
- [ ] Um componente de `loading` (ex: um spinner) é exibido enquanto os dados da API estão sendo buscados.
- [ ] Uma mensagem de `erro` clara é exibida se a API retornar um erro 404 ou falhar.
- [ ] Após o sucesso da busca, o componente de template correto é renderizado dinamicamente com base no `templateId` recebido.
- [ ] Um template padrão (ex: `Template1Classic`) é usado como fallback caso o `templateId` seja inválido.