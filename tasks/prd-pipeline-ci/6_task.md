---
status: pending
parallelizable: true
blocked_by: ["5.0"]
---

<task_context>
<domain>engine/infra/ci</domain>
<type>documentation</type>
<scope>configuration</scope>
<complexity>low</complexity>
<dependencies>temporal</dependencies>
<unblocks>"release-alignment"</unblocks>
</task_context>

# Tarefa 6.0: Documentar fluxo e alinhar governança da branch

## Visão Geral
Atualizar a documentação do repositório descrevendo o novo workflow, orientações de troubleshooting e próximos passos para ativar branch protection obrigando os checks `backend-tests` e `admin-tests`.

## Requisitos
- Atualização de README ou docs internos (`docs/` ou `README.md`) com instruções de uso da pipeline, comandos executados e como reexecutar jobs.
- Registro dos requisitos para branch protection (checks obrigatórios, quem pode aprovar, estratégia de rerun) e follow-up para ativação.
- Inclusão de guidance sobre interpretação dos artefatos e sumários.

## Subtarefas
- [ ] 6.1 Atualizar documentação mencionando workflow `ci-main` e etapas executadas.
- [ ] 6.2 Registrar orientações de branch protection (exigir checks, cancelar runs velhos) e comunicar stakeholders.
- [ ] 6.3 Revisar textos para garantir alinhamento com PRD (objetivo de zero regressões pós-merge) e Tech Spec.

## Sequenciamento
- Bloqueado por: 5.0
- Desbloqueia: release alignment/ativação de branch protection
- Paralelizável: Sim (trabalho de documentação)

## Detalhes de Implementação
- Tech Spec: “Sequenciamento de Desenvolvimento” passo 6, “Considerações Técnicas – Decisões Principais/Riscos”.
- PRD: “Feedback e governança” e “Questões em Aberto” (follow-ups de branch protection, SLO).

## Critérios de Sucesso
- Documentação atualizada mergeada com instruções claras sobre a pipeline.
- Stakeholders informados sobre passos para ativar branch protection e SLO monitorado.
- Q&A comum (reruns, troubleshooting) disponível para novos membros do time.
