---
allowed-tools: Task, Read, Write, TodoWrite
description: Executar um relatório completo do estado do projeto incluindo dependências, arquitetura, componentes etc.
required-agents: orchestrator, dependency-auditor, architectural-analyzer, component-deep-analyzer
---
# Relatório Completo do Estado do Projeto

## Descrição

Produza um instantâneo completo e auditável do projeto coordenando agentes especialistas e consolidando suas saídas. Claude Code (VOCÊ) atua como coordenador. O agente orquestrador apenas prepara a estrutura e, posteriormente, sintetiza as saídas. O entregável final é:

1. Um arquivo README nomeado README-AAAA-MM-DD-HH:MM:SS.md com o timestamp ATUAL colocado dentro do diretório do agente orquestrador que contém uma breve descrição do projeto e um índice vinculando a cada relatório produzido pelos agentes

Ao criar o índice do README, liste apenas o título de cada relatório e vincule diretamente ao arquivo usando um caminho absoluto que comece na raiz do repositório com uma barra inicial única. Formato de exemplo do link:

```
[Project Architecture](/<link-para-o-relatorio>)
```

Valide cada link antes de salvar o README e assegure que o caminho realmente exista. Use o MANIFEST.md gerenciado pelo agente orquestrador como fonte de verdade para mapear todos os relatórios produzidos. Ele é INICIALIZADO na Fase 1, ATUALIZADO após cada agente completar nas Fases 2 e 3, e FINALIZADO na Fase 4. Certifique-se de informar esse comportamento ao agente **orchestrator**.

## Modelo de Saída

Use o seguinte modelo para o arquivo README. Substitua os espaços reservados e remova quaisquer seções vazias. Não insira linhas horizontais.

```
# <Project Name> Project State Full Report

<Breve descrição do projeto em um ou dois parágrafos>.
<Breve descrição explicando os objetivos deste documento, que é a consolidação dos principais aspectos do projeto como um Raio‑X.>

Generated on: YYYY-MM-DD HH:MM:SS

## Overview and Architecture
<Project Overview>
<Project Architecture>

## Components
<Component Name>

## Dependencies
<Dependencies Report>
```

## Restrições Críticas

1. NUNCA entregue o fluxo completo ao agente orquestrador
2. O agente orquestrador DEVE executar APENAS duas responsabilidades. a) Fase 1 cria a estrutura do projeto de acordo com a especificação do agente ou sinais explícitos do usuário como --project-folder, --output-folder e --ignore-folders. b) Fase 4 sintetiza as saídas geradas por outros agentes
3. Mantenha SEPARAÇÃO ESTRITA entre agentes a) Invoque cada agente especialista em uma chamada separada de Task. b) O orquestrador não deve gerar subagentes. c) Claude Code (VOCÊ) é o coordenador que ordena as fases e as execuções paralelas
4. SIGA EXATAMENTE a especificação escrita de cada agente armazenada em .claude/agents
5. NUNCA escreva saídas fora dos locais designados. NUNCA crie pastas que não sejam explicitamente definidas pelo orquestrador ou pelas especificações dos agentes
6. NUNCA forneça recomendações, planos de ação, alterações de código ou instruções de atualização no Resumo do Projeto. Resuma apenas o que os agentes relataram
7. NÃO estime tempos ou durações. EVITE linguagem vaga como provavelmente seguro ou deve ser suficiente
8. NÃO invente CVEs ou vulnerabilidades. Use evidência produzida pelo dependency-auditor.

### SEPARAÇÃO DE AGENTES OBRIGATÓRIA

* Cada agente DEVE ser invocado com uma chamada separada Task
* O orquestrador NÃO deve ser solicitado a gerar sub‑agentes
* VOCÊ (Claude Code) é o coordenador, NÃO o agente orquestrador
* Toda comunicação flui através de VOCÊ como coordenador. VOCÊ decide quais novos agentes iniciar. Depois que qualquer agente terminar, VOCÊ DEVE acionar o orquestrador para atualizar o MANIFEST.md. Na prática, isto significa chamar Task(orchestrator) uma vez por artefato concluído para que ele possa anexar a entrada (título, caminho absoluto enraizado em /, agente, timestamp). Exemplo: quando Task(architectural-analyzer) terminar, invoque imediatamente Task(orchestrator) para registrar o Relatório de Arquitetura em MANIFEST.md.

## Fluxo de Execução

**Fase 1: Task(orchestrator)**

1. Leia os flags do usuário e normalize caminhos. Respeite --project-folder e --output-folder quando fornecidos. Se não fornecidos, use os locais padrão definidos pela especificação do orquestrador
2. Crie apenas os diretórios exigidos pela especificação do orquestrador. Não invente níveis extras como output ou reports a menos que a especificação do orquestrador os exija
3. Aplique a lista de ignorados antes de qualquer leitura de arquivos. Nunca analise ou escaneie arquivos dentro de qualquer pasta listada em --ignore-folders
4. Inicialize **MANIFEST.md** no diretório do orquestrador com uma estrutura de índice vazia (título, caminho absoluto, agente, timestamp).

**Fase 2: Task(dependency-auditor) e Task(architectural-analyzer) em paralelo**

1. Task(dependency-auditor) produz um relatório completo de dependências seguindo sua especificação de agente.
   Importante: Para validação de dependências nesta Task, use servidores MCP como Context7 e Firecrawl para verificar versões, status de manutenção e vulnerabilidades conhecidas.
2. Task(architectural-analyzer) produz um Relatório de Arquitetura completo seguindo sua especificação de agente.
   Importante: APENAS o orquestrador faz append em **MANIFEST.md** quando cada Task completar.

**Fase 3: Task(component-deep-analyzer) em paralelo, uma por componente**

1. Analise o **Relatório de Arquitetura** da Fase 2, tratando-o como um artefato produzido pelo architectural-analyzer.

2. Para cada componente listado no **Relatório de Arquitetura** (por exemplo, na seção "Critical Components Analysis" ou qualquer seção que enumere componentes), inicie uma chamada separada Task(component-deep-analyzer) para esse componente, em paralelo.

3. Cada Task(component-deep-analyzer) DEVE analisar completamente apenas seu componente atribuído e produzir um relatório individual.

4. Requisito de cobertura e exemplo: se o Relatório de Arquitetura listar 10 componentes, VOCÊ DEVE iniciar 10 execuções paralelas de Task(component-deep-analyzer) e produzir 10 relatórios de componente correspondentes. Nenhum componente pode ser omitido.

5. Após todas as execuções de Task(component-deep-analyzer) completarem, VOCÊ DEVE verificar que cada componente tenha um relatório correspondente. Reabra o Relatório de Arquitetura e revise suas seções de componentes linha por linha. Se algum componente não tiver relatório, inicie execuções adicionais de Task(component-deep-analyzer) para os componentes ausentes até que a cobertura seja 100%.

IMPORTANTE: Certifique-se de NÃO duplicar nenhum relatório; portanto, pense com MUITA atenção se o relatório já existe antes de criar um novo com nome, timestamp, etc. VOCÊ DEVE ser extremamente preciso com essa verificação.

**Fase 4: Task(orchestrator)**

1. Agregue referências para todos os relatórios gerados
2. FINALIZE **MANIFEST.md** dentro do diretório do agente orquestrador: valide todas as entradas, assegure que títulos e caminhos absolutos existam, remova duplicatas, confirme nomes de agentes e timestamps.

**Fase 5: Task(VOCÊ)**

1. Leia MANIFEST.md do diretório do orquestrador. Construa o índice usando títulos de relatórios e links absolutos que comecem com uma única barra
2. Valide cada link antes de escrever. Use o algoritmo de validação de links definido abaixo
3. Salve README-AAAA-MM-DD-HH:MM:SS.md com a data/hora ATUAL dentro do diretório do orquestrador

**LEMBRETE:** O agente orquestrador é apenas outro especialista que:

* Configura a estrutura do projeto (Fase 1)
* Sintetiza as saídas (Fase 4)
* NÃO coordena outros agentes - essa é função do seu coordenador (VOCÊ, Claude Code)

## Exemplos de Uso

Use $ARGUMENTS como a pasta do projeto e, se fornecida, o caminho onde salvar os arquivos de saída.

NUNCA use quaisquer outros caminhos para salvar relatórios, arquivos ou manifests a menos que eles sejam explicitamente fornecidos pelo usuário. Não crie subpastas como `reports` ou `output`.

NUNCA crie quaisquer arquivos ou pastas que não sejam especificados pela especificação do agente ou do orquestrador.

SIGA EXATAMENTE o padrão de saída definido abaixo.

```bash
# Execute o fluxo no projeto com base na pasta raiz
/run-project-state-full-report

# Execute o fluxo na pasta do projeto fornecida pelo usuário
/run-project-state-full-report --project-folder=project-folder

# Execute o fluxo e salve os relatórios na pasta fornecida pelo usuário. Todos os relatórios, arquivos e manifests devem ser salvos na pasta de saída usando o seguinte padrão: <output-folder>/<agent-name>/<file-name-provided-by-agent>.md. Por exemplo: output-folder/dependency-auditor/dependencies-report-YYYY-MM-DD-HH:MM:SS.md
/run-project-state-full-report --project-folder=project-folder --output-folder=output-folder

# Execute o fluxo e ignore as pastas fornecidas pelo usuário. Não leia nem audite os arquivos nas pastas fornecidas pelo usuário.
/run-project-state-full-report --ignore-folders=adk_repo,venv,.env,node_modules,.git
```

## Instruções Negativas

1. NUNCA modifique ou sugira edições ao código fonte.
   * Exemplos: NÃO abra pull requests, renomeie arquivos, refatore funções, altere padrões de configuração ou scripts de build.
   * Permitido: Resumir descobertas apenas e referenciá-las no relatório do agente onde o problema está documentado.

2. NÃO execute atualizações ou prescreva migrações.
   * Exemplos proibidos: "npm update", "go get -u", "helm upgrade", "aplicar migrações de banco de dados".
   * Este comando é descritivo, não prescritivo. Mantenha as saídas informacionais.

3. NÃO invente CVEs ou assuma vulnerabilidades sem evidência explícita do dependency-auditor ou da validação MCP.
   * Proibido: "provavelmente vulnerável", "possível CVE-2023-XXXXX" ou alegações não verificadas.
   * Permitido: Citar nomes de pacotes e versões e citar as descobertas produzidas pelo dependency-auditor.

4. NÃO use linguagem vaga.
   * Evite frases como "provavelmente seguro", "deve estar bem", "parece OK".
   * Use linguagem neutra e factual extraída dos relatórios dos agentes.

5. NÃO use emojis ou caracteres estilizados.

6. NÃO forneça estimativas de tempo.
   * Proibido: "em 2 horas", "até amanhã", "dentro de X dias" ou qualquer afirmação temporal.
   * Se o tempo for solicitado, declare que estimativas estão fora do escopo deste comando.

7. NUNCA crie pastas de agentes na raiz do repositório.
   * Proibido: "/agents", "/architectural-analyzer" na raiz do repositório.
   * Permitido: Usar apenas os caminhos especificados por cada agente ou pelo orquestrador, por exemplo `docs/agents/orchestrator`.

8. NUNCA crie arquivos ou pastas que não sejam especificados pelo agente ou orquestrador.
   * Proibido: diretórios ad-hoc como "reports", "output", "tmp" a menos que explicitamente permitido.
   * Todas as saídas devem seguir o padrão `<output-folder>/<agent-name>/<file-name-provided-by-agent>.md` ou ser armazenadas no diretório do orquestrador conforme especificado.

8. NUNCA duplique um relatório. Se for necessário fazer alterações, edite o relatório que já existe em vez de criar outro novo
    * Proibido: duplicar um relatório "component-analysis" durante a revisão se todos os relatórios já existirem.

## Observações

Pense com MUITA atenção em cada passo do fluxo de trabalho e determine as instruções mais claras para cada agente para que possam completar suas tarefas. Como coordenador mestre (VOCÊ), VOCÊ DEVE fornecer todo o contexto necessário para cada agente. Para fazer isso, leia cada especificação de agente, entenda passo a passo o que cada agente deve fazer, e passe as entradas, restrições e caminhos específicos que eles precisam para ter sucesso.

O orquestrador sozinho mantém o **MANIFEST.md**. Instrua o orquestrador a anexar uma nova entrada ao **MANIFEST.md** imediatamente após qualquer agente terminar, registrando o título do relatório, o caminho absoluto enraizado em `/`, o nome do agente e o timestamp.

Exemplo: Assim que o **Relatório de Arquitetura** estiver pronto, o orquestrador DEVE anexar sua entrada ao **MANIFEST.md** e marcar essa tarefa como concluída.
