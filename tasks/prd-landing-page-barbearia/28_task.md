---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---

# Tarefa 28.0: Template 5 - Premium

## Visão Geral
Implementar o componente `Template5Premium.tsx`. Este template deve transmitir uma sensação de luxo, sofisticação e exclusividade, ideal para barbearias de alto padrão. O design deve ser limpo, elegante e com foco em detalhes refinados.

## Requisitos de Design (prd.md Seção 2.2)
- **Tema**: Luxuoso e sofisticado.
- **Cores**: Preto (`#1C1C1C`), Dourado metálico (`#C9A961`), Cinza escuro (`#2E2E2E`).
- **Fontes**: Uma serif clássica e elegante para títulos (ex: `Playfair Display`) e uma sans-serif limpa e fina para o corpo (ex: `Lato`, `Roboto`).
- **Layout**: Header transparente que se torna sólido no scroll, seção hero com efeito de `parallax`, serviços apresentados em uma lista detalhada (em vez de cards), e uma seção para depoimentos (se houver dados).
- **Animações**: Transições suaves (`fade-in`), animações sutis no scroll e o efeito parallax na seção hero.

## Detalhes de Implementação
- **Framework**: React com TypeScript.
- **Estilização**: Tailwind CSS. As cores e fontes customizadas devem ser configuradas no `tailwind.config.js`.
- **Efeito Parallax**: Pode ser implementado com CSS puro (usando `background-attachment: fixed`) ou com uma biblioteca leve de JavaScript se for necessário um controle mais complexo.
- **Props**: O componente receberá `data: PublicLandingPage`.
- **Componentes Reutilizáveis**: O `ServiceCard` pode não ser adequado aqui. Talvez seja melhor criar um componente `ServiceListItem` específico para este template, que apresente as informações de forma mais descritiva.
- **Hooks**: `useServiceSelection` e `useNavigate`.

## Estrutura do Componente (`Template5Premium.tsx`)
- **Lógica Principal**: A lógica de estado e handlers permanece a mesma.
- **Diferenças na Renderização**:
  - **Header**: Inicialmente transparente sobre a seção hero, mudando para uma cor sólida (ex: preto) quando o usuário rola a página. Isso pode ser feito com um hook que monitora a posição do scroll (`window.scrollY`).
  - **Seção Hero**: Implementar o efeito parallax na imagem de fundo.
  - **Lista de Serviços**: Em vez de um grid, usar uma lista vertical. Cada item da lista pode ter o nome e a descrição de um lado, e a duração e o preço do outro, com um botão ou checkbox de seleção.
  - **Animações**: Usar classes de transição do Tailwind para fazer os elementos aparecerem suavemente (`fade-in`) conforme o usuário rola a página (pode ser feito com a `Intersection Observer API`).

## Critérios de Aceitação
- [ ] O componente renderiza todas as informações da barbearia corretamente.
- [ ] A paleta de cores (preto, dourado, cinza escuro) e as fontes elegantes são aplicadas corretamente.
- [ ] O header transparente se torna sólido ao rolar a página.
- [ ] O efeito parallax na seção hero está funcionando.
- [ ] Os serviços são exibidos em um formato de lista detalhada.
- [ ] Animações sutis de scroll e fade-in estão presentes.
- [ ] O layout é responsivo e mantém a aparência premium em todos os dispositivos.
- [ ] A funcionalidade de seleção de serviços e agendamento está operando corretamente.