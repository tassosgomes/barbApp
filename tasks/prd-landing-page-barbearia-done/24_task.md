---
status: completed
parallelizable: true
blocked_by: ["22.0", "23.0"]
---

# Tarefa 24.0: Template 1 - Clássico

## Visão Geral
Implementar o componente `Template1Classic.tsx` para a landing page pública. Este template deve seguir uma identidade visual elegante e tradicional, com uma paleta de cores focada em preto, dourado e branco.

## Requisitos de Design (prd.md Seção 2.2)
- **Tema**: Elegante e tradicional.
- **Cores**: Preto (`#1A1A1A`), Dourado (`#D4AF37`), Branco (`#FFFFFF`).
- **Fontes**: `Playfair Display` (serif) para títulos, `Inter` (sans-serif) para corpo de texto.
- **Layout**: Header com logo centralizado, seção hero com imagem de fundo, grid de serviços, rodapé com informações.
- **Ícones**: Usar ícones tradicionais (tesoura, navalha) se possível, ou os ícones padrão do `lucide-react`.

## Detalhes de Implementação (techspec-frontend.md Seção 2.5)
- **Framework**: React com TypeScript.
- **Estilização**: Tailwind CSS. As cores e fontes customizadas devem ser configuradas no `tailwind.config.js`.
- **Props**: O componente receberá `data: PublicLandingPage` contendo todas as informações da barbearia e da landing page.
- **Componentes Reutilizáveis**: Utilizar os componentes compartilhados criados na Tarefa 23.0, como `ServiceCard` e `WhatsAppButton`.
- **Hooks**: Utilizar o hook `useServiceSelection` para gerenciar a seleção de serviços e o `useNavigate` do `react-router-dom` para o agendamento.

## Estrutura do Componente (`Template1Classic.tsx`)
- **Busca de Dados**: Os dados virão por props, fornecidos pela página `LandingPage.tsx`.
- **Gerenciamento de Estado**: `const { selectedIds, totalPrice, hasSelection, toggleService } = useServiceSelection(landingPage.services);`
- **Handler de Agendamento**: `handleSchedule()` que constrói a URL de agendamento com base nos serviços selecionados e navega para a página de agendamento.
- **Renderização das Seções**:
  - **Header**: Logo, nome da barbearia, navegação interna (`#servicos`, `#sobre`, `#contato`) e botão "Agendar Agora".
  - **Hero**: Imagem de fundo (pode ser uma imagem estática do template), título, subtítulo e botões de ação.
  - **Serviços**: Título da seção e um grid renderizando os componentes `ServiceCard`.
  - **Sobre**: Seção condicional que só aparece se `landingPage.aboutText` existir.
  - **Contato/Informações**: Exibe endereço, horário de funcionamento e redes sociais.
  - **Footer**: Copyright e link para a "Área Admin".
  - **Botões Flutuantes**: `WhatsAppButton` (fixo) e o botão de agendamento que aparece condicionalmente (`hasSelection`).

## Critérios de Aceitação
- [x] O componente renderiza todas as informações da barbearia (logo, nome, textos) corretamente.
- [x] A paleta de cores (preto, dourado, branco) e as fontes (serif/sans-serif) são aplicadas corretamente.
- [x] O layout é responsivo e se adapta bem em dispositivos mobile, tablet e desktop.
- [x] A seleção de serviços funciona, e o botão flutuante de agendamento aparece com o número de serviços e o preço total.
- [x] Clicar em "Agendar Agora" (com ou sem serviços selecionados) redireciona para a URL de agendamento correta.
- [x] O botão do WhatsApp abre o link `wa.me` com o número e a mensagem corretos.
- [x] As seções "Sobre" e "Redes Sociais" são renderizadas condicionalmente, apenas se os dados existirem.

## ✅ CONCLUÍDA

### 1.0 Implementação completada
- ✅ Componente `Template1Classic.tsx` criado com todas as seções especificadas
- ✅ Integração com hooks `useServiceSelection` e `useNavigate`
- ✅ Uso de componentes compartilhados `ServiceCard` e `WhatsAppButton`
- ✅ Configuração do Tailwind CSS com cores e fontes customizadas
- ✅ Layout responsivo implementado

### 1.1 Definição da tarefa, PRD e tech spec validados
- ✅ Requisitos do PRD atendidos (cores, layout, funcionalidades)
- ✅ Especificações técnicas seguidas (React + TS, Tailwind, hooks)
- ✅ Critérios de aceitação validados

### 1.2 Análise de regras e conformidade verificadas
- ✅ Padrões de código seguidos (camelCase, PascalCase, early returns)
- ✅ Regras React aplicadas (componentes funcionais, hooks, TypeScript)
- ✅ Testes automatizados criados seguindo diretrizes
- ✅ Sem linting errors

### 1.3 Revisão de código completada
- ✅ Código limpo e bem estruturado
- ✅ Sem duplicação de código
- ✅ Tratamento adequado de erros
- ✅ Performance otimizada

### 1.4 Pronto para deploy
- ✅ Build bem-sucedido
- ✅ Todos os testes passando (11/11)
- ✅ Sem erros de compilação
- ✅ Funcionalidades validadas