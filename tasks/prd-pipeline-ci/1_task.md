---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>engine/infra/ci</domain>
<type>implementation</type>
<scope>configuration</scope>
<complexity>medium</complexity>
<dependencies>temporal</dependencies>
<unblocks>"2.0","3.0","4.0","5.0","6.0"</unblocks>
</task_context>

# Tarefa 1.0: Configurar workflow base da pipeline `main`

## Visão Geral
Configurar o arquivo inicial do workflow GitHub Actions (`.github/workflows/ci-main.yml`) com os gatilhos da branch `main`, execução manual opcional e variáveis globais necessárias para a pipeline, garantindo estrutura clara para os jobs posteriores.

## Requisitos
- Workflow deve ser acionado automaticamente em pushes para `main` e permitir disparo manual (`workflow_dispatch`).
- Variáveis globais (`CI`, `DOTNET_CLI_TELEMETRY_OPTOUT`, `DOTNET_SKIP_FIRST_TIME_EXPERIENCE`, `DOTNET_CLI_HOME`) configuradas conforme especificação.
- Nome do workflow e descrição alinhados ao objetivo do PRD.

## Subtarefas
- [ ] 1.1 Criar o arquivo `.github/workflows/ci-main.yml` com cabeçalho, nome e gatilhos corretos.
- [ ] 1.2 Definir variáveis de ambiente globais e defaults compartilhados pelos jobs.
- [ ] 1.3 Validar sintaxe do workflow localmente (ex.: `act -l`) ou via visualização do GitHub.

## Sequenciamento
- Bloqueado por: —
- Desbloqueia: 2.0, 3.0, 4.0, 5.0, 6.0
- Paralelizável: Não (etapa fundacional)

## Detalhes de Implementação
- Seção “Visão Geral dos Componentes” da Tech Spec (`Workflow ci-main.yml`).
- Seção “Sequenciamento de Desenvolvimento” passo 1.

## Critérios de Sucesso
- Workflow aparece listado no GitHub Actions com os gatilhos esperados.
- Execuções manuais (`workflow_dispatch`) podem ser iniciadas sem erro.
- Variáveis globais visíveis nos logs dos jobs subsequentes.
