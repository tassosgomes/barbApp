---
status: completed
parallelizable: true
blocked_by: ["22.0", "23.0"]
completed_date: 2025-10-23
---

# Tarefa 25.0: Template 2 - Moderno

## Visão Geral
Implementar o componente `Template2Modern.tsx` para a landing page pública. Este template deve ter um design limpo, minimalista e contemporâneo, usando uma paleta de cores sóbria com um toque de cor vibrante.

## Requisitos de Design (prd.md Seção 2.2)
- **Tema**: Limpo e minimalista.
- **Cores**: Cinza escuro (`#2C3E50`), Azul elétrico (`#3498DB`), Branco/Cinza claro (`#ECF0F1`).
- **Fontes**: Fontes sans-serif modernas como `Montserrat` ou `Poppins`.
- **Layout**: Header fixo, seção hero com um call-to-action (CTA) em destaque, cards de serviços com sombras sutis para dar profundidade.
- **Animações**: Animações discretas como scroll suave e efeitos de `hover` nos cards.

## Detalhes de Implementação
- **Framework**: React com TypeScript.
- **Estilização**: Tailwind CSS. As cores e fontes devem ser adicionadas ou customizadas no `tailwind.config.js`.
- **Props**: O componente receberá `data: PublicLandingPage`.
- **Componentes Reutilizáveis**: Utilizar `ServiceCard` e `WhatsAppButton` da Tarefa 23.0.
- **Hooks**: `useServiceSelection` para a lógica de seleção de serviços e `useNavigate` para o roteamento.

## Estrutura do Componente (`Template2Modern.tsx`)
- **Lógica Principal**: A lógica de estado e handlers (`handleSchedule`, `toggleService`) será muito similar à do `Template1Classic`.
- **Diferenças na Renderização**:
  - **Header**: Deve ser fixo no topo da página (`sticky` ou `fixed`) para estar sempre visível durante o scroll.
  - **Hero**: Foco em um grande título e um botão de CTA proeminente com a cor de destaque (azul elétrico).
  - **Cards de Serviço**: Aplicar `shadow-md` ou `shadow-lg` e uma transição no `hover` (ex: `hover:shadow-xl hover:-translate-y-1`).
  - **Seção "Sobre"**: Pode ter um layout um pouco mais elaborado, talvez com uma imagem ao lado do texto.
  - **Fontes**: Garantir que as fontes `Montserrat` ou `Poppins` (precisam ser importadas, ex: do Google Fonts) sejam aplicadas corretamente.

## Critérios de Aceitação
- [x] O componente renderiza todas as informações da barbearia corretamente.
- [x] A paleta de cores (cinza escuro, azul, branco) e as fontes sans-serif modernas são aplicadas.
- [x] O header é fixo e permanece visível ao rolar a página.
- [x] Os cards de serviço têm efeito de sombra e animação de hover.
- [x] O layout é totalmente responsivo.
- [x] A funcionalidade de seleção de serviços e agendamento está operando corretamente.
- [x] O botão do WhatsApp está funcional.
- [x] O design geral transmite uma sensação de modernidade e minimalismo.

## Validação Realizada
- ✅ **Implementação Completa**: Componente `Template2Modern.tsx` criado com todas as funcionalidades requeridas
- ✅ **Design Moderno**: Paleta de cores moderna aplicada (cinza escuro #2C3E50, azul #3498DB, branco #ECF0F1)
- ✅ **Fontes Modernas**: Google Fonts (Montserrat e Poppins) integradas via `index.html`
- ✅ **Header Fixo**: Header com `position: fixed` e `z-50` para permanência durante scroll
- ✅ **Hero Section**: Design clean com CTA proeminente e gradiente de fundo
- ✅ **Cards de Serviço**: Efeitos de sombra (`shadow-lg`) e hover (`hover:shadow-xl hover:-translate-y-2`)
- ✅ **Responsividade**: Layout totalmente responsivo com breakpoints mobile/tablet/desktop
- ✅ **Funcionalidades**: Seleção de serviços, navegação e WhatsApp funcionais
- ✅ **Testes**: 11 testes passando cobrindo todas as funcionalidades
- ✅ **Qualidade de Código**: Segue padrões React, TypeScript e regras de codificação
- ✅ **Build**: Compilação e linting sem erros
- ✅ **Integração**: Componente integrado ao sistema de templates e roteamento