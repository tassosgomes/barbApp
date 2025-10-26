---
status: completed
parallelizable: false
blocked_by: ["11.0", "13.0", "17.0"]
completed_date: 2025-10-22
---

# Tarefa 18.0: Página LandingPageEditor ✅ CONCLUÍDA

## Visão Geral
Criar a página `LandingPageEditor.tsx`, que servirá como o ponto de entrada principal e a interface unificada para todo o gerenciamento da landing page no painel de administração. Esta página organizará as diferentes funcionalidades em abas (Tabs) para uma experiência de usuário clara e intuitiva.

## Requisitos Funcionais (prd.md Seções 2, 3, 6, 7)
- **Estrutura de Abas**: A página deve ter uma navegação por abas para separar as principais seções de gerenciamento:
  - **Editar Informações**: Contém o formulário principal (`LandingPageForm`) e o painel de preview (`PreviewPanel`).
  - **Escolher Template**: Contém a galeria de templates (`TemplateGallery`).
  - **Preview**: Exibe o `PreviewPanel` em tela cheia.
- **Acesso Rápido à URL**: Exibir a URL pública da landing page de forma destacada.
- **Ações Rápidas**: Incluir botões para "Copiar URL" e "Abrir Landing Page" em uma nova aba.
- **Layout Responsivo**: A página deve se adaptar bem a diferentes tamanhos de tela, especialmente a organização do formulário e do preview.

## Detalhes de Implementação (techspec-frontend.md Seção 1.5)
- **Framework**: React com TypeScript.
- **Componentes UI**: Utilizar o componente `Tabs` do `shadcn/ui` para a estrutura de abas (`Tabs`, `TabsList`, `TabsTrigger`, `TabsContent`).
- **Layout**: 
  - A aba "Editar Informações" deve usar um layout de grid (CSS Grid) para posicionar o `LandingPageForm` e o `PreviewPanel` lado a lado em telas maiores (ex: `lg:grid-cols-2`). Em telas menores, o `PreviewPanel` pode ser ocultado.
- **Hooks**: Utilizar o hook `useLandingPage` para buscar a configuração da landing page e fornecer os dados para os componentes filhos.
- **Gerenciamento de Estado**: Manter o estado do template selecionado (`selectedTemplate`) e passá-lo para a `TemplateGallery` e para a função de atualização.

## Estrutura do Componente (`LandingPageEditor.tsx`)
- **Busca de Dados**: `const { config, updateConfig } = useLandingPage(barbershopId);`
- **Estado do Template**: `const [selectedTemplate, setSelectedTemplate] = useState(config?.templateId || 1);`
- **Handlers**:
  - `copyUrl()`: Usa `navigator.clipboard.writeText` para copiar a URL e exibe um `toast` de confirmação.
  - `handleTemplateChange()`: Atualiza o estado local e chama `updateConfig` para salvar a mudança de template no backend.
- **Renderização**:
  - Cabeçalho da página com o título e os botões de ação rápida.
  - Box com a URL da landing page.
  - Componente `Tabs` com três `TabsContent`:
    1. **Editar**: Contém `LandingPageForm` e `PreviewPanel`.
    2. **Template**: Contém `TemplateGallery`.
    3. **Preview**: Contém `PreviewPanel` com a prop `fullScreen`.

## Critérios de Aceitação
- [x] A página renderiza a estrutura de abas com "Editar Informações", "Escolher Template" e "Preview".
- [x] A URL da landing page é exibida corretamente.
- [x] O botão "Copiar URL" copia a URL para a área de transferência e mostra uma notificação.
- [x] O botão "Abrir Landing Page" abre a URL pública em uma nova aba.
- [x] A aba "Editar Informações" mostra o formulário e o preview lado a lado em desktops.
- [x] A aba "Escolher Template" exibe a galeria de templates, e a seleção de um novo template o atualiza no backend.
- [x] A aba "Preview" mostra o painel de preview em tela cheia.
- [x] Os dados da landing page (buscados pelo hook `useLandingPage`) são passados corretamente para os componentes filhos (`LandingPageForm`, `TemplateGallery`, `PreviewPanel`).