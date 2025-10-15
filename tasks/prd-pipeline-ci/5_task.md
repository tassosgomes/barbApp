---
status: pending
parallelizable: false
blocked_by: ["3.0","4.0"]
---

<task_context>
<domain>engine/infra/ci</domain>
<type>implementation</type>
<scope>performance</scope>
<complexity>medium</complexity>
<dependencies>temporal</dependencies>
<unblocks>"6.0"</unblocks>
</task_context>

# Tarefa 5.0: Acrescentar otimizações e observabilidade do workflow

## Visão Geral
Refinar o workflow adicionando publicação de artefatos de teste, resumos consolidados (`GITHUB_STEP_SUMMARY`) e controles opcionais de concorrência/retention para estabilizar e tornar diagnósticos mais rápidos.

## Requisitos
- Passos `actions/upload-artifact@v4` para arquivos TRX do backend e relatórios Vitest no frontend, com `retention-days` configurado (ex.: 7).
- Passo pós-testes que gera resumo dos resultados (quantidade de testes, links para artefatos) via `GITHUB_STEP_SUMMARY`.
- Configuração `concurrency` no nível do workflow (`group: main-ci`) com `cancel-in-progress: true`.
- Garantir que caches existentes continuem funcionais após ajustes.

## Subtarefas
- [ ] 5.1 Adicionar upload de artefatos do backend (`TestResults.trx`) com retenção definida.
- [ ] 5.2 Adicionar upload de artefatos do frontend (relatórios Vitest JSON/HTML) com retenção definida.
- [ ] 5.3 Configurar passo de resumo usando `GITHUB_STEP_SUMMARY` consolidando tempos/resultados.
- [ ] 5.4 Inserir bloco `concurrency` cancelando execuções anteriores da branch `main` e validar comportamento em run de teste.

## Sequenciamento
- Bloqueado por: 3.0, 4.0
- Desbloqueia: 6.0
- Paralelizável: Não (depende dos jobs concluídos)

## Detalhes de Implementação
- Tech Spec: “Arquitetura do Sistema – Relatórios/Observabilidade”, “Monitoramento e Observabilidade”, “Sequenciamento de Desenvolvimento” passo 5.
- PRD: “Feedback e governança” requisitos R11-R13.

## Critérios de Sucesso
- Execução do workflow disponibiliza artefatos baixáveis para backend e frontend.
- Sumário do run apresenta status dos testes e links para artefatos.
- Execuções consecutivas cancelam run anterior da mesma branch ao iniciar nova.
