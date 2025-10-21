---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---

# Tarefa 26.0: Template 3 - Vintage

## Visão Geral
Implementar o componente `Template3Vintage.tsx`. Este template deve evocar uma atmosfera retrô, inspirada nas barbearias clássicas dos anos 50 e 60, utilizando cores, fontes e elementos gráficos que remetam a essa época.

## Requisitos de Design (prd.md Seção 2.2)
- **Tema**: Retrô, anos 50/60.
- **Cores**: Marrom (`#5D4037`), Creme (`#F5E6D3`), Vermelho escuro (`#B71C1C`).
- **Fontes**: Uma fonte "display" com estilo vintage para títulos (ex: `Lobster`, `Bebas Neue`) e uma sans-serif legível para o corpo de texto.
- **Layout**: Header com um banner que pode conter uma ilustração, tipografia grande na seção hero e uma lista de serviços que pode usar ilustrações ou ícones vintage.
- **Elementos Gráficos**: Uso de texturas sutis (como papel antigo no fundo) e bordas ornamentais para decorar seções.

## Detalhes de Implementação
- **Framework**: React com TypeScript.
- **Estilização**: Tailwind CSS. As cores e fontes customizadas devem ser configuradas no `tailwind.config.js`. Pode ser necessário usar imagens de fundo para texturas.
- **Props**: O componente receberá `data: PublicLandingPage`.
- **Componentes Reutilizáveis**: Adaptar `ServiceCard` e `WhatsAppButton` para o estilo vintage, ou criar variações se necessário.
- **Hooks**: `useServiceSelection` e `useNavigate`.

## Estrutura do Componente (`Template3Vintage.tsx`)
- **Lógica Principal**: A lógica de estado e handlers será a mesma dos outros templates.
- **Diferenças na Renderização**:
  - **Fontes**: Importar e aplicar as fontes vintage escolhidas. A fonte de título deve ser usada com destaque.
  - **Layout**: Em vez de um grid de cards, os serviços podem ser apresentados em um layout de lista com duas colunas, onde cada item tem uma linha pontilhada conectando o nome ao preço, um estilo comum em menus antigos.
  - **Elementos Visuais**: Adicionar bordas ornamentais usando pseudo-elementos CSS (`::before`, `::after`) ou imagens SVG. Aplicar uma imagem de textura de fundo ao corpo da página ou a seções específicas.
  - **Ícones**: Se possível, usar um conjunto de ícones com estilo de ilustração ou "line art" que combine com o tema.

## Critérios de Aceitação
- [ ] O componente renderiza todas as informações da barbearia corretamente.
- [ ] A paleta de cores (marrom, creme, vermelho) e as fontes vintage são aplicadas de forma consistente.
- [ ] Elementos gráficos como texturas e bordas ornamentais estão presentes e contribuem para a estética retrô.
- [ ] O layout é responsivo e funciona bem em todos os dispositivos.
- [ ] A funcionalidade de seleção de serviços e agendamento está operando corretamente.
- [ ] O design geral consegue transmitir a sensação de uma barbearia vintage clássica.