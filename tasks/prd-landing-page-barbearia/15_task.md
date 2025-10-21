---
status: pending
parallelizable: true
blocked_by: ["10.0"]
---

# Tarefa 15.0: Componente ServiceManager

## Visão Geral
Criar o componente `ServiceManager.tsx` para o painel de administração. Este componente permitirá que o administrador da barbearia gerencie quais serviços são exibidos na landing page, controle a ordem de exibição e a visibilidade de cada um.

## Requisitos Funcionais (prd.md Seção 4)
- **Listagem de Serviços**: Deve listar todos os serviços ativos da barbearia.
- **Controle de Visibilidade**: Cada serviço deve ter um checkbox para controlar sua exibição (`isVisible`).
- **Reordenação**: O admin deve poder reordenar os serviços usando drag & drop.
- **Ações em Massa**: Botões para "Selecionar todos" e "Desmarcar todos".
- **Validação**: Garantir que pelo menos um serviço esteja sempre selecionado.

## Detalhes de Implementação (techspec-frontend.md Seção 1.4)
- **Framework**: React com TypeScript.
- **Estado Local**: O componente deve gerenciar um estado local (`localServices`) para permitir edições antes de salvar.
- **Drag & Drop**: Utilizar a biblioteca `@hello-pangea/dnd`.
- **Componentes UI**: Usar componentes do `shadcn/ui` como `Checkbox`, `Button`, e `Card`.
- **Ícones**: Utilizar `lucide-react` para ícones como `GripVertical`, `Eye`, `EyeOff`.
- **Props**:
  - `services: LandingPageService[]`: Lista inicial de serviços.
  - `onChange: (services: LandingPageService[]) => void`: Callback para notificar o componente pai sobre mudanças.

## Estrutura do Componente (`ServiceManager.tsx`)
- **Estado**: `const [localServices, setLocalServices] = useState(services);`
- **Handler de Drag & Drop**: `handleDragEnd` para atualizar a ordem (`displayOrder`).
- **Handler de Visibilidade**: `toggleVisibility` para ligar/desligar um serviço.
- **Ações em Massa**: Funções `selectAll` e `deselectAll`.
- **Renderização**:
  - `DragDropContext` envolvendo um `Droppable`.
  - Um `Draggable` para cada item da lista.
  - Cada item deve exibir:
    - Handle de arrastar (`GripVertical`).
    - `Checkbox` para visibilidade.
    - Nome, duração e preço do serviço.
    - Ícone de status (`Eye` ou `EyeOff`).

## Critérios de Aceitação
- [ ] O componente renderiza a lista de serviços recebida via props.
- [ ] O usuário pode marcar e desmarcar a visibilidade de um serviço individualmente.
- [ ] O usuário pode usar os botões "Selecionar todos" e "Desmarcar todos" e o estado é refletido corretamente.
- [ ] O usuário pode arrastar e soltar um serviço para reordená-lo na lista.
- [ ] A função `onChange` é chamada sempre que a lista de serviços (visibilidade ou ordem) é modificada.
- [ ] O componente é visualmente consistente com o design do painel admin.