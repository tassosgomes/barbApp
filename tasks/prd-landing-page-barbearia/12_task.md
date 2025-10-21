---
status: pending
parallelizable: true
blocked_by: ["10.0", "5.0"]
---

<task_context>
<domain>frontend-admin/hooks</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>external_apis</dependencies>
<unblocks>14.0</unblocks>
</task_context>

# Tarefa 12.0: Hook useLogoUpload

## Visão Geral

Implementar hook para gerenciar upload de logo com preview, validação e feedback visual.

<requirements>
- Upload de arquivo com FormData
- Preview local antes do upload
- Validação de tipo e tamanho
- Progress indicator
- Tratamento de erros
</requirements>

## Subtarefas

- [ ] 12.1 Criar hook `useLogoUpload.ts`
- [ ] 12.2 Implementar validação de arquivo
- [ ] 12.3 Adicionar preview local
- [ ] 12.4 Integrar com endpoint de upload
- [ ] 12.5 Adicionar feedback visual (loading, sucesso, erro)
- [ ] 12.6 Criar testes

## Detalhes de Implementação

Ver techspec-frontend.md seção 1.3 para código completo do hook.

## Critérios de Sucesso

- [ ] Upload funcionando para JPG, PNG, SVG
- [ ] Preview local imediato
- [ ] Validações client-side efetivas
- [ ] Feedback claro para o usuário
