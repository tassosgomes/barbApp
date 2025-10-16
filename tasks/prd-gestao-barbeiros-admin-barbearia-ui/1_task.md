status: completed
parallelizable: false
blocked_by: []
---

<task_context>
<domain>engine/frontend/bootstrap</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>http_server</dependencies>
<unblocks>"6.0","7.0","8.0","10.0","11.0","12.0","14.0"</unblocks>
</task_context>

# Tarefa 1.0: Bootstrap React Query e dependências

## Visão Geral
Adicionar TanStack React Query ao projeto, configurar o QueryClientProvider no `App`, e instalar dependências auxiliares (devtools). Opcional: date-fns/dayjs.

## Requisitos
- `@tanstack/react-query` instalado e configurado
- `QueryClientProvider` envolvendo a aplicação
- Devtools ativo apenas em DEV

## Subtarefas
- [x] 1.1 Instalar `@tanstack/react-query` e `@tanstack/react-query-devtools`
- [x] 1.2 Criar `src/lib/queryClient.ts` com configuração padrão
- [x] 1.3 Envolver `App` com `QueryClientProvider` em `main.tsx`
- [x] 1.4 Habilitar Devtools em ambiente de desenvolvimento

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 6.0, 7.0, 8.0, 10.0, 11.0, 12.0, 14.0
- Paralelizável: Não (base de dados do app)

## Detalhes de Implementação
Ver seção “Sequenciamento de Desenvolvimento” da Tech Spec e “React Query Provider (bootstrap)”.

## Critérios de Sucesso
- Build roda sem erros
- Queries podem ser usadas nas páginas
- Devtools aparecem em DEV

- [x] 1.0 Bootstrap React Query e dependências ✅ CONCLUÍDA
	- [x] 1.1 Implementação completada
	- [x] 1.2 Definição da tarefa, PRD e tech spec validados
	- [x] 1.3 Análise de regras e conformidade verificadas
	- [x] 1.4 Revisão de código completada
	- [x] 1.5 Pronto para deploy
