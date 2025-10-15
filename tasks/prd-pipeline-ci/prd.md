## Visão Geral

O repositório `barbApp` abriga dois componentes principais (`backend` e `barbapp-admin`) que hoje dependem de execuções manuais de build e testes antes de cada deploy. A ausência de uma pipeline contínua em `main` aumenta o risco de regressões silenciosas e reduz a confiança para promover mudanças a produção. A nova pipeline de CI no GitHub deve padronizar a validação automática de build e testes para os dois projetos, garantindo feedback rápido a desenvolvedores, líderes técnicos e stakeholders de produto.

## Objetivos

- Garantir que cada push ou commit direto na branch `main` execute todos os builds e suites de teste existentes para `backend` e `barbapp-admin` de forma determinística.
- Fornecer feedback automático via GitHub Checks em no máximo 10 minutos por execução normal, permitindo identificar falhas antes de qualquer processo de release.
- Aumentar a confiança do time ao promover releases, reduzindo a incidência de regressões em produção detectadas após merge em `main` a zero.
- Manter a taxa de pipelines quebradas abaixo de 5% ao mês, incentivando correções imediatas quando testes falham.

## Histórias de Usuário

- Como desenvolvedor backend, quero que o pipeline execute `dotnet test` automaticamente após meus pushes em `main`, para que eu detecte regressões no domínio C# sem precisar rodar suites manuais.
- Como desenvolvedor frontend, quero que o pipeline execute `npm run test` no projeto `barbapp-admin` e reporte falhas diretamente no GitHub, para que eu saiba se uma mudança de UI/UX quebrou testes antes de qualquer deploy.
- Como líder técnico ou responsável por release, quero visualizar rapidamente o estado das execuções de build/teste em `main`, para decidir se o projeto está saudável o suficiente para uma release planejada.
- Como engenheiro DevOps, quero que os jobs backend e frontend sejam isolados e independentes, para que eu possa diagnosticar falhas específicas sem ruído de executores compartilhados.

## Funcionalidades Principais

- **Pipeline de CI gatilhada na branch `main`**  
  - Define um workflow GitHub Actions ativado em cada push ou commit direto em `main`.  
  - Importância: estabelece controle contínuo de qualidade nos caminhos que chegam mais rápido a produção.  
  - Alto nível: arquivo YAML em `.github/workflows/` que descreve jobs paralelos, condições de gatilho e notificações.  
  - Requisitos funcionais:  
    - R1. O workflow deve ser acionado automaticamente em qualquer push na branch `main`, ignorando outras branches.  
    - R2. O workflow deve publicar o status consolidado via GitHub Checks na própria branch `main`.

- **Job de backend (C#)**  
  - Roda build/restaurar dependências .NET e executa `dotnet test` no diretório `backend`.  
  - Importância: valida lógica de negócio e APIs antes de licenciar releases.  
  - Alto nível: job utiliza runner Ubuntu, instala SDK .NET compatível, restaura packages, builda e roda testes com cobertura existente.  
  - Requisitos funcionais:  
    - R3. O job deve usar o SDK .NET suportado pelo projeto (versão a confirmar no `global.json` ou `.csproj`).  
    - R4. O comando `dotnet test` deve falhar o job caso qualquer teste retorne status diferente de sucesso.  
    - R5. Logs de teste devem ser persistidos no output do job para depuração direta no GitHub Actions.

- **Job de frontend (`barbapp-admin`)**  
  - Executa instalação de dependências e `npm run test` no projeto React/Node.  
  - Importância: garante estabilidade das interfaces administrativas.  
  - Alto nível: job em runner Ubuntu configurado com Node.js LTS, instala dependências com `npm ci` (ou `npm install` se preferível) e executa suite de testes.  
  - Requisitos funcionais:  
    - R6. O job deve usar a versão de Node.js alinhada ao projeto (definida em `.nvmrc` ou `package.json`).  
    - R7. `npm ci` deve ser utilizado sempre que possível para builds reprodutíveis; se indisponível, a pipeline deve sinalizar manualmente a alternativa adotada.  
    - R8. `npm run test` deve ser executado em modo não interativo e falhar a pipeline em caso de qualquer teste reprovado.

- **Serviço PostgreSQL via devcontainer**  
  - Disponibiliza instancia de banco compatível com o ambiente de desenvolvimento, necessária para testes que dependem de dados persistidos.  
  - Alto nível: workflow reusa a configuração do devcontainer ou starta serviço PostgreSQL via `services`.  
  - Requisitos funcionais:  
    - R9. O workflow deve iniciar um serviço PostgreSQL com as mesmas credenciais/versão definidas no devcontainer para prover consistência de ambiente.  
    - R10. Os jobs devem consumir variáveis de ambiente e strings de conexão alinhadas ao devcontainer para evitar divergências de configuração local vs pipeline.

- **Feedback e governança**  
  - Consolida artefatos e status em GitHub.  
  - Requisitos funcionais:  
    - R11. Cada job deve expor claramente o status (success/failure/cancelled) com logs acessíveis no GitHub Actions.  
    - R12. O workflow deve bloquear merges ou deploys automatizados subsequentes que dependam de `main` caso qualquer job falhe (via branch protection ou integração com release pipeline futura).  
    - R13. O workflow deve permitir reexecução manual de jobs falhos diretamente no GitHub Actions para mitigar falhas transitórias.

## Experiência do Usuário

- **Personas**:  
  - Desenvolvedores backend e frontend que precisam de feedback confiável sem sair do GitHub.  
  - Liderança técnica e QA que consomem o status agregado da branch `main` para decidir sobre releases.
- **Fluxos principais**:  
  - Push direto em `main` aciona pipeline automaticamente; o autor monitora os checks no cartão do commit.  
  - Em caso de falha, o autor recebe indicação visual (vermelha) em GitHub, acessa logs do job específico e corrige o código antes de novo push.  
  - Em caso de sucesso, a branch mantém selo verde que alimenta processos de release contínua.  
  - Jobs devem rodar em paralelo para reduzir tempo total; logs separados dão clareza em qual stack falhou.
- **Considerações de UX**:  
  - Nomear jobs de maneira amigável (`Backend Tests`, `Admin Tests`) para leitura rápida.  
  - Destacar comandos executados e caminho do projeto nos logs iniciais para facilitar debugging.  
  - Evitar prompts interativos, garantindo execução não supervisionada.
- **Acessibilidade**:  
  - Usar mensagens de status claras e cores padrão do GitHub.  
  - Logs devem mencionar o status textual e não depender exclusivamente de cores/ícones.

## Restrições Técnicas de Alto Nível

- GitHub Actions será a plataforma de CI padrão; não se prevê uso de serviços externos pagos adicionais.  
- O pipeline deve reutilizar a configuração do devcontainer existente para provisionar PostgreSQL ou reproduzi-la fielmente via `services`.  
- As execuções devem completar em até 10 minutos em condições normais; acima disso deve ser investigada a necessidade de otimização (cache de dependências, paralelismo etc.).  
- Tests podem depender de variáveis de ambiente sensíveis; segredos devem ser armazenados em GitHub Secrets ou reutilizar os padrões do devcontainer sem expô-los em logs.  
- Runners hospedados pelo GitHub serão utilizados, assumindo compatibilidade com .NET e Node.js.  
- A pipeline não deve alterar o estado do banco de dados persistente externo; qualquer dado deve residir em instância efêmera de testes.

## Não-Objetivos (Fora de Escopo)

- Automatizar deploys para qualquer ambiente (staging, produção) está fora do escopo imediato.  
- Não serão adicionadas etapas de lint, análise estática, checagens de segurança ou testes end-to-end nesta fase.  
- Não haverá suporte a gatilhos para branches diferentes de `main` (ex.: pull requests, tags) até avaliação posterior.  
- Não serão provisionados recursos de infraestrutura permanentes (clusters, VMs dedicadas); tudo deverá ser efêmero.

## Questões em Aberto

- Qual o tempo máximo aceitável para cada job antes de ser considerado um problema operacional (SLO exato)?  
- Devemos publicar artefatos (ex.: resultados de cobertura, relatórios HTML) ou basta rely nos logs padrão?  
- Há testes conhecidos como flakey em `backend` ou `barbapp-admin` que exigem estabilização prévia?  
- É necessário configurar cache persistente de dependências (NuGet/npm) para acelerar builds, ou o tempo atual é aceitável?  
- Como integrarmos a pipeline com proteções de branch (branch protection rules) ou políticas de aprovação já existentes?
