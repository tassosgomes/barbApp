---
status: pending
parallelizable: true
blocked_by: ["22.0"]
---

<task_context>
<domain>frontend-public/components</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>none</dependencies>
<unblocks>24.0, 25.0, 26.0, 27.0, 28.0</unblocks>
</task_context>

# Tarefa 23.0: Componentes Compartilhados (ServiceCard, WhatsAppButton, etc)

## Visão Geral

Criar componentes reutilizáveis que serão usados em todos os 5 templates da landing page pública.

<requirements>
- ServiceCard com checkbox e informações
- WhatsAppButton (normal e floating)
- Header genérico
- Footer genérico
- Loading e Error states
</requirements>

## Subtarefas

- [ ] 23.1 Criar componente `ServiceCard.tsx`
- [ ] 23.2 Criar componente `WhatsAppButton.tsx`
- [ ] 23.3 Criar componente `Header.tsx` (opcional)
- [ ] 23.4 Criar componente `Footer.tsx` (opcional)
- [ ] 23.5 Criar componentes de Loading/Error
- [ ] 23.6 Estilizar com Tailwind
- [ ] 23.7 Criar testes de componentes

## Detalhes de Implementação

Ver techspec-frontend.md seção 2.4 para código completo.

## Critérios de Sucesso

- [ ] Componentes responsivos
- [ ] Acessibilidade (aria-labels)
- [ ] Estilização consistente
- [ ] Testes de renderização passando
