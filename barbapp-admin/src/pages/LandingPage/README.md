# LandingPageEditor Page

## Visão Geral

Página principal para gerenciamento da landing page no painel de administração da barbearia. Organiza todas as funcionalidades de customização em uma interface intuitiva com navegação por abas (Tabs).

## Funcionalidades

### 1. **Aba "Editar Informações"**
- Formulário completo de edição da landing page
- Preview lateral em tempo real (apenas desktop)
- Campos: logo, sobre, horário, WhatsApp, redes sociais, serviços

### 2. **Aba "Escolher Template"**
- Galeria visual dos 5 templates disponíveis
- Preview de cada template com cores e tema
- Seleção e aplicação imediata

### 3. **Aba "Preview"**
- Visualização da landing page em tela cheia
- Alternância entre visualizações mobile/desktop
- Exatamente como ficará publicamente

### 4. **Ações Rápidas**
- **Copiar URL**: Copia a URL pública da landing page para clipboard
- **Abrir Landing Page**: Abre a landing page em nova aba

## Estrutura

```
LandingPageEditor
├── Header
│   ├── Título e Descrição
│   └── Botões de Ação (Copiar URL, Abrir)
├── URL Box (com código da barbearia)
└── Tabs
    ├── Tab: Editar Informações
    │   ├── LandingPageForm (formulário)
    │   └── PreviewPanel (preview lateral - desktop only)
    ├── Tab: Escolher Template
    │   └── TemplateGallery
    └── Tab: Preview
        └── PreviewPanel (fullScreen)
```

## Props

Nenhuma - O componente usa contexto (`useBarbearia`) para obter dados da barbearia.

## Hooks Utilizados

- `useBarbearia()`: Obtém dados da barbearia atual (ID, código)
- `useLandingPage(barbershopId)`: Gerencia estado e operações da landing page
- `useToast()`: Exibe notificações de sucesso/erro

## Estados Locais

- `selectedTemplate`: ID do template atualmente selecionado

## Computed Values

- `landingPageUrl`: URL pública completa da landing page

## Handlers

### `handleCopyUrl()`
Copia a URL da landing page para a área de transferência usando `navigator.clipboard.writeText()`. Exibe toast de confirmação ou erro.

### `handleOpenLandingPage()`
Abre a landing page pública em nova aba usando `window.open()` com flags de segurança (`noopener,noreferrer`).

### `handleTemplateChange(templateId)`
Atualiza o estado local e envia para o backend através de `updateConfig({ templateId })`.

## Loading States

**Quando `isLoading === true`:**
- Exibe spinner centralizado
- Mensagem: "Carregando configuração da landing page..."

## Error States

**Quando `error` existe ou `config === null`:**
- Exibe Alert (variante `destructive`)
- Mensagem de erro genérica
- Detalhes do erro (se disponível)

## Layout Responsivo

### Mobile (< 768px)
- Header com botões empilhados
- URL box com scroll horizontal
- Tabs em grid 3 colunas
- Preview lateral ocultado no Tab "Editar"

### Tablet (768px - 1024px)
- Layout similar ao mobile
- Botões side-by-side

### Desktop (> 1024px)
- Header horizontal com botões
- Grid 2 colunas no Tab "Editar": Form + Preview
- Preview lateral sticky (top: 1.5rem)

## Integração com Componentes

### LandingPageForm
- Recebe `barbershopId` como prop
- Gerencia próprio estado e submissão
- Integra `LogoUploader` e `ServiceManager`

### TemplateGallery
- Recebe `selectedTemplateId` e `onSelectTemplate`
- Callback chama `handleTemplateChange`

### PreviewPanel
- Recebe `config` atual
- Prop `fullScreen` no Tab Preview
- Prop `device` para alternar visualizações

## Roteamento

**Rota:** `/:codigo/landing-page`

Exemplo: `/TESTE123/landing-page`

Definida em: `routes/adminBarbearia.routes.tsx`

## Estados da API

### Query (GET)
- `isLoading`: Carregando configuração inicial
- `error`: Erro ao buscar configuração

### Mutation (UPDATE)
- `isUpdating`: Salvando alterações
- Toasts automáticos (sucesso/erro) via `useLandingPage`

## Fluxo de Uso

1. Admin acessa `/:codigo/landing-page`
2. Sistema carrega configuração existente via `useLandingPage`
3. Admin pode:
   - Editar informações no formulário (Tab 1)
   - Escolher novo template (Tab 2)
   - Visualizar preview (Tab 3)
   - Copiar URL para compartilhar
   - Abrir landing page para ver resultado
4. Alterações são salvas automaticamente pelos componentes internos

## Critérios de Aceitação

- [x] Página renderiza estrutura de abas corretamente
- [x] URL da landing page é exibida com código da barbearia
- [x] Botão "Copiar URL" copia para clipboard e mostra notificação
- [x] Botão "Abrir Landing Page" abre em nova aba
- [x] Tab "Editar Informações" mostra formulário + preview lateral
- [x] Tab "Escolher Template" mostra galeria de templates
- [x] Tab "Preview" mostra preview em tela cheia
- [x] Seleção de template atualiza no backend
- [x] Layout responsivo funciona em mobile/tablet/desktop
- [x] Loading state exibe spinner
- [x] Error state exibe mensagem de erro

## Testes

Ver: `__tests__/LandingPageEditor.test.tsx`

**Cobertura:**
- Renderização de elementos
- Loading e error states
- Funcionalidade de copiar URL
- Abertura da landing page
- Navegação entre tabs
- Integração com hooks

## Dependências

### Componentes shadcn/ui
- `Tabs`, `TabsContent`, `TabsList`, `TabsTrigger`
- `Button`
- `Card`, `CardContent`
- `Alert`, `AlertDescription`

### Features
- `LandingPageForm`
- `TemplateGallery`
- `PreviewPanel`
- `useLandingPage` hook

### Ícones (lucide-react)
- `Copy`: Botão copiar
- `ExternalLink`: Botão abrir
- `Loader2`: Loading spinner
- `AlertCircle`: Ícone de erro

## Melhorias Futuras

- [ ] Adicionar undo/redo de alterações
- [ ] Histórico de versões da landing page
- [ ] Agendamento de publicação
- [ ] Analytics integrados (visitas, conversões)
- [ ] A/B Testing de templates
- [ ] Export/Import de configurações
- [ ] Preview em tempo real (debounced)

## Notas Técnicas

### Sincronização de Template
O estado `selectedTemplate` é sincronizado com `config.templateId` através de `useEffect`. Isso garante que o template selecionado sempre reflita o valor atual no backend.

### URL Construction
A URL é construída dinamicamente usando `window.location.origin` + código da barbearia. Isso garante que funciona em todos os ambientes (dev, staging, prod).

### Clipboard API
Usa `navigator.clipboard.writeText()` com try/catch para lidar com possíveis erros (ex: permissões negadas).

### Window.open Security
Flags `noopener,noreferrer` previnem vulnerabilities relacionadas a `window.opener`.

## Versionamento

- **v1.0** (2025-10-22): Versão inicial implementada
  - Todas funcionalidades do PRD
  - 3 tabs de navegação
  - Integração completa com componentes
  - Testes unitários
