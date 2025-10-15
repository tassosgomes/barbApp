# Documentação do Pipeline de CI

Este documento descreve o pipeline de Integração Contínua (CI) configurado para o projeto BarbApp.

## Visão Geral

O pipeline de CI é definido no arquivo `.github/workflows/ci-main.yml`. Ele é acionado em cada `push` para a branch `main` e também pode ser executado manualmente.

O objetivo principal do pipeline é garantir que o código do backend e do frontend seja compilado, testado e verificado antes de ser integrado à branch principal.

## Jobs

O pipeline consiste em dois jobs principais que rodam em paralelo:

### 1. `backend-tests`

Este job é responsável por testar a aplicação backend .NET.

- **Ambiente:** `ubuntu-latest`
- **Serviços:** Um container PostgreSQL (`postgres:16-alpine`) é provisionado para os testes.
- **Passos:**
  1. **Checkout do código:** Baixa o código do repositório.
  2. **Setup do .NET:** Configura o ambiente .NET 8.0.x com cache de pacotes NuGet.
  3. **Restore, Build, Test:** Restaura as dependências, compila a solução e executa os testes `dotnet test`.
  4. **Upload dos resultados:** Os resultados dos testes em formato TRX são salvos como artefatos.
  5. **Resumo:** Um resumo da execução dos testes é adicionado ao sumário do workflow.

### 2. `admin-tests`

Este job é responsável por testar a aplicação frontend `barbapp-admin`.

- **Ambiente:** `ubuntu-latest`
- **Serviços:** Um container PostgreSQL (`postgres:16-alpine`) é provisionado para os testes.
- **Passos:**
  1. **Checkout do código:** Baixa o código do repositório.
  2. **Setup do Node.js:** Configura o ambiente Node.js 20.x com cache de pacotes npm.
  3. **Install, Build, Test:** Instala as dependências com `npm ci`, compila a aplicação e executa os testes com `npm run test`.
  4. **Upload dos resultados:** Os relatórios de teste do Vitest (HTML e JSON) são salvos como artefatos.
  5. **Resumo:** Um resumo da execução dos testes é adicionado ao sumário do workflow.

## Artefatos e Resumos

Ao final de cada execução, o workflow disponibiliza os seguintes artefatos para download:

- `backend-test-results`: Contém o arquivo `test-results.trx` com os resultados detalhados dos testes do backend.
- `frontend-test-results`: Contém os relatórios de teste do frontend em formatos HTML e JSON.

O sumário da execução do workflow no GitHub Actions também exibe um resumo de alto nível dos resultados dos testes.

## Governança da Branch (Próximos Passos)

Para garantir a estabilidade da branch `main`, as seguintes regras de proteção de branch (branch protection) serão ativadas:

- **Checks obrigatórios:** Os jobs `backend-tests` e `admin-tests` deverão ser concluídos com sucesso antes que um pull request possa ser mesclado.
- **Revisão de código:** Será exigida a aprovação de pelo menos um membro da equipe.

Esta configuração garante que nenhum código com falha de teste ou que não compile seja introduzido na branch principal.
