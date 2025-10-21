---
status: pending
parallelizable: true
blocked_by: ["10.0"]
---

# Tarefa 16.0: Componente PreviewPanel

## Visão Geral
Desenvolver o componente `PreviewPanel.tsx` que exibe uma pré-visualização em tempo real da landing page pública. Este painel será usado dentro do painel de administração para que o usuário veja suas alterações antes de salvar.

## Requisitos Funcionais (prd.md Seção 6)
- **Visualização Realista**: O preview deve renderizar a landing page exatamente como ela aparecerá para o cliente final.
- **Atualização em Tempo Real**: O conteúdo do preview deve ser atualizado em tempo real (com debounce) conforme o admin edita os campos no formulário.
- **Controles de Visualização**: Deve incluir botões para alternar entre as visualizações de `Desktop` e `Mobile`.
- **Contextos de Uso**:
  - **Edição de Informações**: Exibido em uma visualização de tela dividida (split view) ao lado do formulário.
  - **Galeria de Templates**: Usado dentro de um modal para mostrar como um template ficaria com os dados atuais da barbearia.

## Detalhes de Implementação (techspec-frontend.md Seção 1.5)
- **Framework**: React com TypeScript.
- **Props**: O componente receberá a configuração da landing page (`config: LandingPageConfig`) e um booleano opcional (`fullScreen`) para controlar o layout.
- **Renderização Dinâmica de Template**: O `PreviewPanel` deve ser capaz de carregar e renderizar dinamicamente o componente de template correto (ex: `Template1Classic`, `Template2Modern`) com base no `templateId` presente na `config`.
- **Simulação de Dispositivo**: A alternância entre mobile e desktop pode ser feita aplicando classes CSS que definem a largura do contêiner do preview (por exemplo, `w-[375px]` para mobile e `w-full` para desktop).
- **Isolamento**: O preview deve ser renderizado dentro de um `iframe` ou de uma forma que seu CSS não vaze para o painel de administração.
- **Interatividade**: O preview é apenas visual; todos os links e botões dentro dele devem ser desabilitados (`pointer-events-none`).

## Estrutura do Componente (`PreviewPanel.tsx`)
- **Estado**: `const [view, setView] = useState<'mobile' | 'desktop'>('desktop');`
- **Seleção de Template**: Um objeto ou switch-case para mapear `templateId` para o componente de template correspondente.
- **Renderização**:
  - Controles para alternar a visualização (ícones de `Smartphone` e `Monitor`).
  - Um contêiner principal que ajusta sua classe CSS com base no estado `view`.
  - Renderização do componente de template selecionado, passando os dados da `config` como props.

## Critérios de Aceitação
- [ ] O componente renderiza o template correto com base no `templateId` da prop `config`.
- [ ] As informações exibidas no preview (logo, textos, serviços) correspondem aos dados da prop `config`.
- [ ] Os botões de `Mobile` e `Desktop` alteram a largura da visualização para simular os respectivos dispositivos.
- [ ] O preview não é interativo (links e botões desabilitados).
- [ ] O componente pode ser renderizado em modo `fullScreen` (para a aba de Preview) e em modo de painel lateral.
- [ ] O preview reflete as alterações feitas no formulário em tempo real (após a implementação do formulário principal).