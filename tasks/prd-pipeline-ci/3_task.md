---
status: pending
parallelizable: true
blocked_by: ["1.0","2.0"]
---

<task_context>
<domain>engine/infra/ci</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 3.0: Implementar job `backend-tests` (.NET)

## Visão Geral
Construir o job de CI responsável por restaurar, buildar e executar `dotnet test` na solução `BarbApp.sln`, reutilizando cache NuGet e configurando logs compatíveis com GitHub Actions.

## Requisitos
- Job executa em `ubuntu-latest` com `actions/setup-dotnet@v4` e versão `8.0.x`.
- Caching de pacotes NuGet habilitado para acelerar execuções subsequentes.
- Passos `dotnet restore`, `dotnet build --configuration Release --no-restore` e `dotnet test --no-build` com logger TRX.
- Variável `ConnectionStrings__DefaultConnection` apontando para o serviço PostgreSQL.
- Falha nos testes deve marcar o job como `failure`.

## Subtarefas
- [ ] 3.1 Adicionar passos de checkout e `actions/setup-dotnet@v4` com cache habilitado.
- [ ] 3.2 Incluir comandos de restore e build (`dotnet restore`, `dotnet build` em Release).
- [ ] 3.3 Configurar `dotnet test` com `--no-build`, logger TRX e env `ConnectionStrings__DefaultConnection`.
- [ ] 3.4 Executar o workflow (ou `act`) para garantir que o job conclui com sucesso quando os testes passam.

## Sequenciamento
- Bloqueado por: 1.0, 2.0
- Desbloqueia: 5.0
- Paralelizável: Sim (pode evoluir em paralelo ao job frontend após configuração do serviço)

## Detalhes de Implementação
- Tech Spec: “Visão Geral dos Componentes” (job `backend-tests`), “Interfaces Principais” (bloco de steps .NET), “Modelos de Dados” (variáveis de ambiente).
- PRD: “Funcionalidades Principais – Job de backend (C#)” requisitos R3-R5.

## Critérios de Sucesso
- Job `backend-tests` completa com status `success` quando `dotnet test` passa e falha quando um teste quebra.
- Logs exibem restauração e execução de testes, incluindo saída do logger TRX.
- Cache NuGet verificado nos runs subsequentes (log `Cache restored from key`).
