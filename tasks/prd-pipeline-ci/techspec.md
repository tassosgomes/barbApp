# Especificação Técnica – Pipeline CI `main`

## Resumo Executivo

Implementaremos um workflow GitHub Actions dedicado que roda automaticamente a cada push na branch `main` para validar os dois pilares do repositório: o backend .NET 8 (`backend/`) e o frontend React/Vite (`barbapp-admin/`). O pipeline terá dois jobs independentes executados em paralelo: `backend-tests` (build + `dotnet test`) e `admin-tests` (build + `npm run test`). Ambos compartilham um serviço PostgreSQL compatível com o devcontainer (imagem `postgres:16-alpine`) e usam caching específico (NuGet, npm) para manter o tempo total abaixo da meta de 10 minutos. A estratégia prioriza a detecção de regressões logo após o merge em `main`, fornece logs completos via GitHub Checks e prepara o terreno para bloqueios de branch protection.

## Arquitetura do Sistema

### Visão Geral dos Componentes

- **Workflow `ci-main.yml`**: arquivo em `.github/workflows/` com gatilho `push` limitado à `main` e gatilho manual opcional (`workflow_dispatch`) para reruns controlados. Define variáveis globais (`CI=true`, `DOTNET_CLI_TELEMETRY_OPTOUT=1`) e matriz de jobs.
- **Job `backend-tests`**: executa em `ubuntu-latest`, usa `actions/setup-dotnet@v4` com `dotnet-version: 8.0.x`, restaura/builda a solução (`BarbApp.sln`) e roda `dotnet test --configuration Release --no-build --logger "trx"`. Reaproveita cache NuGet e publica artefato `backend-test-results`.
- **Job `admin-tests`**: executa em `ubuntu-latest`, usa `actions/setup-node@v4` com `node-version: 20.x` + cache npm, instala com `npm ci`, executa `npm run build` e `npm run test -- --runInBand` (reduz ruído em runners compartilhados). Exporta sumário de testes e artefato `admin-vitest-results`.
- **Serviço `postgres`**: definido como serviço compartilhado (imagem `postgres:16-alpine`) com `POSTGRES_DB=barbapp`, `POSTGRES_USER=postgres`, `POSTGRES_PASSWORD=postgres` e health check (`pg_isready`). Jobs consomem a string de conexão via `postgres://postgres:postgres@localhost:5432/barbapp` enquanto os testes de integração continuam livres para subir seus próprios containers via Testcontainers.
- **Caching**: `actions/cache@v4` para `~/.nuget/packages` (chave `nuget-${{ hashFiles('**/*.csproj') }}`) e cache npm automizado por `setup-node` (usa `package-lock.json`). Evita downloads redundantes e estabiliza tempo de execução.
- **Relatórios/Observabilidade**: artefatos `.trx` e `vitest-report.json/html`, além de resumo agregado (`steps.<name>.outputs.summary`) publicado via `GITHUB_STEP_SUMMARY`.

Fluxo: push em `main` → GitHub Actions cria execução → ambos os jobs sobem o serviço Postgres → backend job roda build/test com Testcontainers apoiando PostgreSQL → frontend job instala dependências, gera build e roda teste → resultados reportados via checks e artefatos. Falha em qualquer job marca o workflow como falho, impedindo releases ligados à `main`.

## Design de Implementação

### Interfaces Principais

Representaremos os pontos de extensão do workflow como blocos YAML (pseudocódigo) para guiar a implementação:

```yaml
# gatilho principal
on:
  push:
    branches: ["main"]
  workflow_dispatch: {}

# serviço PostgreSQL compartilhado declarado no job
jobs:
  backend-tests:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_DB: barbapp
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres
        ports: ["5432:5432"]
        options: >-
          --health-cmd="pg_isready -U postgres" --health-interval=10s --health-timeout=5s --health-retries=5
```

```yaml
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: 8.0.x
          cache: true
      - name: Restore
        run: dotnet restore BarbApp.sln
      - name: Build
        run: dotnet build BarbApp.sln --configuration Release --no-restore
      - name: Test
        env:
          ConnectionStrings__DefaultConnection: Host=localhost;Database=barbapp;Username=postgres;Password=postgres
        run: dotnet test BarbApp.sln --configuration Release --no-build --logger "trx;LogFileName=TestResults.trx"
```

```yaml
  admin-tests:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:16-alpine
        env:
          POSTGRES_DB: barbapp
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: postgres
        ports: ["5432:5432"]
        options: >-
          --health-cmd="pg_isready -U postgres" --health-interval=10s --health-timeout=5s --health-retries=5
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 20.x
          cache: npm
          cache-dependency-path: barbapp-admin/package-lock.json
      - run: npm ci
        working-directory: barbapp-admin
        env:
          PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD: "1"
      - run: npm run build
        working-directory: barbapp-admin
      - run: npm run test -- --runInBand
        working-directory: barbapp-admin
        env:
          CI: "true"
```

Estas definições articulam os pontos críticos: gatilho, configuração de serviços, instalação de runtimes, execução de comandos e escopo de caching.

### Modelos de Dados

- **Variáveis de Ambiente Globais**:
  - `CI=true` (padrão GitHub) garante que Vitest rode em modo non-watch.
  - `DOTNET_CLI_TELEMETRY_OPTOUT=1`, `DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1` reduzem ruído e aceleram build inicial.
  - `ConnectionStrings__DefaultConnection` apontando para o serviço Postgres compartilhado; reforça aderência ao padrão definido em `docs/environment-variables.md`.
- **Credenciais PostgreSQL**: `postgres`/`postgres` com base `barbapp`, replicando devcontainer esperado no PRD.
- **Cache Keys**:
  - `nuget-${{ runner.os }}-${{ hashFiles('**/*.csproj', '**/*.props', '**/*.targets') }}`
  - `npm-${{ runner.os }}-${{ hashFiles('barbapp-admin/package-lock.json') }}`
- **Artefatos**:
  - `backend-test-results/TestResults.trx` (para análise no VS Test Explorer/GitHub UI).
  - `admin-vitest-results/report.{json,html}` gerados via `vitest --reporter=json --outputFile`. Podemos adicionar passo `npm run test -- --runInBand --reporter=json --outputFile=vitest-report.json` para consolidar.

### Endpoints de API

Não aplicável. O pipeline interage exclusivamente com GitHub Actions e containers locais, sem expor endpoints HTTP. Qualquer comunicação externa (Docker daemon) ocorre via socket interno do runner.

## Pontos de Integração

- **GitHub Actions Hosted Runners**: Requer `ubuntu-latest` com Docker preinstalado. Acesso ao daemon é necessário para o Testcontainers iniciar containers auxiliares. Nenhuma credencial adicional é exigida.
- **Docker/Testcontainers**: Backend usa `Testcontainers.PostgreSql`. Garantir que o runner tenha permissão para criar containers aninhados; não usar `services: postgres` não bloqueia Testcontainers, mas manteremos ambos para alinhamento com o PRD. Caso Testcontainers detecte conflito de porta, ele usará portas aleatórias, então monitorar possíveis conflitos.
- **PostgreSQL (serviço)**: Saúde monitorada via `pg_isready`; se falhar, jobs são reiniciados. Não há dependência externa (usa container local ephemeral).
- **Actions Oficiais**: `actions/checkout@v4`, `actions/setup-dotnet@v4`, `actions/setup-node@v4`, `actions/cache@v4`, `actions/upload-artifact@v4`.
- **Branch Protection**: Após o workflow estar ativo, configurar regra que exige status `backend-tests` e `admin-tests` antes de permitir merges automáticos ou deploys encadeados (execução manual fora do escopo, mas prevista).

## Análise de Impacto

| Componente Afetado                | Tipo de Impacto            | Descrição & Nível de Risco                                                                 | Ação Requerida                          |
| --------------------------------- | -------------------------- | ------------------------------------------------------------------------------------------- | --------------------------------------- |
| `backend/` (.NET solution)        | Execução contínua          | Rodadas frequentes de `dotnet build/test`; risco baixo, mas aumenta tempo de feedback.      | Monitorar falhas e corrigir rapidamente |
| `barbapp-admin/` (React)          | Execução contínua          | Instalação npm e build/test em cada push; risco baixo, garante conformidade com `rules/react.md`. | Ajustar dependências conforme alertas  |
| GitHub Actions runners            | Uso de recursos (Docker)   | Testcontainers cria containers; risco médio se limites do runner forem atingidos.           | Limitar paralelismo simultâneo (padrão) |
| Branch Protection / Release flow  | Governança                 | Novos checks obrigatórios; risco baixo, mas requer alinhar com time antes de ativar regra.   | Atualizar documentação de release       |
| Repositório (novo workflow file)  | Estrutura                  | Adiciona `.github/workflows/ci-main.yml`; risco baixo, sem conflitos esperados.             | Revisar PR antes de merge               |

## Abordagem de Testes

### Testes Unitários

- **Backend**: `dotnet test` já cobre projetos Domain/Application/Infrastructure/Integration. Garantir que o comando rode com `--no-build` após `build` para reduzir duplicação e que falhas exponham logs detalhados. Para aderir às `rules/tests.md`, manter isolamentos e permitir que falhas de cobertura sejam investigadas manualmente.
- **Frontend**: `npm run test` (Vitest) roda em modo `CI`, utiliza JSDOM e `@testing-library`. Adicionar argumento `--runInBand` evita spawn paralelos excessivos em runners limitados. Garantir `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1` para evitar downloads desnecessários durante `npm ci`.

### Testes de Integração

- **Backend**: Projetos de integração dependem de Docker; pipeline deve validar que Testcontainers eleva e derruba containers corretamente. Monitorar logs em caso de falha de provisioning (`docker: dial unix /var/run/docker.sock`).
- **Frontend**: Não executaremos E2E (`npm run test:e2e`) nesta fase (fora de escopo); contudo, o job deve manter compatibilidade futura para extensão.
- **Workflow**: Recomenda-se usar `act` localmente ou `workflow_dispatch` antes de ativar branch protection para validar execução. Publicar artefatos auxiliares garante rastreabilidade.

## Sequenciamento de Desenvolvimento

### Ordem de Construção

1. **Criar `ci-main.yml` com skeleton**: definir gatilhos, nome do workflow e variáveis globais. Validação rápida via `act -l` antes de adicionar jobs completos.
2. **Implementar serviço Postgres compartilhado**: garantir health check e variáveis coerentes com `docs/environment-variables.md` e PRD (R9/R10).
3. **Adicionar job `backend-tests`**: incluir setup-dotnet, caching NuGet, restore/build/test, publicação de artefatos `.trx`, resumo no `GITHUB_STEP_SUMMARY`.
4. **Adicionar job `admin-tests`**: configurar setup-node com cache, `npm ci` (com `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD`), build e testes; produzir resumo e artefatos Vitest.
5. **Configurar políticas opcionais**: se desejado, habilitar `concurrency` para cancelar execuções redundantes (`concurrency: { group: main-ci, cancel-in-progress: true }`).
6. **Documentar e alinhar com time**: atualizar README ou docs internos descrevendo nova pipeline e passos para lidar com falhas. Coordenar ajustes de branch protection.

### Dependências Técnicas

- Disponibilidade de GitHub Actions com Docker habilitado (default).
- Acesso a `actions/*` listadas (todas públicas). Nenhum secret adicional é requerido.
- Capacidade de armazenamento para artefatos (limite 500 MB por run; relatórios são pequenos).
- Ferramentas locais para testes manuais (`act`, Docker) opcionais.

## Monitoramento e Observabilidade

- **Logs Estruturados**: GitHub Actions já agrega logs por step. Incluir `DOTNET_CLI_HOME=$RUNNER_TEMP` evita poluição de diretórios.
- **Resumos**: Adicionar step pós-testes que escreve no `$GITHUB_STEP_SUMMARY` o status (quantidade de testes, tempo, link para artefatos). Isso cria painel consolidado no run.
- **Artefatos e Retenção**: Configurar `retention-days: 7` nos artefatos para equilibrar custo e histórico.
- **Alertas**: Falhas no workflow aparecem como checks vermelhos; para alertas adicionais, é possível integrar com Slack/Teams posteriormente (fora de escopo, mas viável via `workflow_run`).

## Considerações Técnicas

### Decisões Principais

- **Dois jobs paralelos**: reflete requisito “jobs separados por projeto”, reduz tempo total e isola falhas (explicado ao time de DevOps).
- **PostgreSQL como serviço**: apesar de Testcontainers prover instância própria, manter serviço com credenciais padrão garante aderência ao PRD e oferece fallback se testes futuros dependerem de conexão fixa.
- **Node 20.x e .NET 8.0.x**: alinha com versões suportadas pelo projeto (`BarbApp.API` em `net8.0` e Vite 5/TypeScript 5 compatíveis com Node >=18). Node 20 é LTS e disponível nos runners.
- **Caching dedicado**: `setup-dotnet` e `setup-node` com cache integrado reduzem latência e ajudam a cumprir SLA de 10 minutos.
- **Artefatos de teste**: mantidos para diagnóstico rápido sem precisar reproduzir localmente.

### Riscos Conhecidos

- **Falta de Docker (Testcontainers)**: se runners GitHub sofrerem alteração, `dotnet test` pode falhar com `DockerException`. Mitigação: documentar que pipeline necessita de runners hospedados (não GitHub-hosted com restrições) e monitorar logs.
- **Downloads Playwright**: `npm ci` pode tentar baixar browsers (centenas de MB). Mitigação: definir `PLAYWRIGHT_SKIP_BROWSER_DOWNLOAD=1` e instruir time a baixar manualmente em jobs E2E futuros.
- **Tempo excedido**: builds lentos ou testes chatos podem quebrar meta de 10 minutos. Mitigação: caching ajustado, `--no-build` no `dotnet test`, `--runInBand` no Vitest para evitar thrashing.
- **Dados residuais**: Testcontainers cria containers temporários; falhas abruptas podem deixar resíduos. Runner GitHub limpa automaticamente, risco baixo.

### Requisitos Especiais

- **SLO de Execução**: objetivo explícito de <10 minutos por run; monitorar `duration` dos jobs. Se excedido, considerar paralelismo adicional (por exemplo, separar testes de integração) ou caching extra (NuGet restore incremental).
- **Segurança**: Nenhum secret novo; connection string usa credenciais de desenvolvimento documentadas. Certificar-se de não expor segredos reais em logs.
- **Extensibilidade**: Estrutura do workflow deve permitir, no futuro, adicionar jobs (lint, security scan) sem grande refatoração.

### Conformidade com Padrões

- **`rules/tests.md`**: Executamos `dotnet test` para todos projetos, mantendo isolamento e cobertura conforme diretrizes.
- **`rules/tests-react.md`**: `npm run test` (Vitest) segue recomendação de usar runner oficial e manter testes próximos ao código, garantindo aderência.
- **`rules/code-standard.md`**: Workflow nomeado e comentado com clareza, usando convenções de diretório (`.github/workflows`).
- **`rules/react.md` e `rules/tests.md`**: Ao garantir build/test automáticos, reforçamos padrões de qualidade exigidos para merges.
- Nenhum desvio identificado; se necessidade de desabilitar lint surgir, deve ser avaliada separadamente.
