---
status: completed
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
- [x] O componente renderiza a lista de serviços recebida via props.
- [x] O usuário pode marcar e desmarcar a visibilidade de um serviço individualmente.
- [x] O usuário pode usar os botões "Selecionar todos" e "Desmarcar todos" e o estado é refletido corretamente.
- [x] O usuário pode arrastar e soltar um serviço para reordená-lo na lista.
- [x] A função `onChange` é chamada sempre que a lista de serviços (visibilidade ou ordem) é modificada.
- [x] O componente é visualmente consistente com o design do painel admin.

## Status da Tarefa ✅ CONCLUÍDA

### ✅ 1.0 Implementação completada
- Componente ServiceManager criado com todas as funcionalidades requeridas
- Drag & drop implementado com @hello-pangea/dnd
- Controles de visibilidade individuais e em lote
- Estado local sincronizado com props
- Interface TypeScript completa

### ✅ 1.1 Definição da tarefa, PRD e tech spec validados
- Implementação alinhada com PRD seção 4
- Tech spec seção 1.4 completamente implementada
- Todos os requisitos funcionais atendidos

### ✅ 1.2 Análise de regras e conformidade verificadas
- Código segue `rules/code-standard.md` (camelCase, PascalCase, etc.)
- Componente funcional React com TypeScript
- Usa Tailwind CSS e shadcn/ui components
- Sem linting errors no componente

### ✅ 1.3 Revisão de código completada
- Arquitetura limpa e bem estruturada
- Type safety completa
- Performance otimizada
- Tratamento adequado de edge cases

### ✅ 1.4 Pronto para deploy
- 26 testes unitários passando (100% coverage dos requisitos)
- Componente testado e validado
- Documentação completa criada
- Relatório de revisão aprovado

---

**Data de Conclusão**: 2025-10-21  
**Status**: ✅ **CONCLUÍDA E APROVADA PARA DEPLOY**  
**Testes**: 26/26 passando  
**Qualidade**: Alta  
**Documentação**: [15_task_review.md](15_task_review.md)