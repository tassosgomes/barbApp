---
status: done
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/infra/ci</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>"3.0","4.0","5.0"</unblocks>
</task_context>

# Tarefa 2.0: Provisionar serviço PostgreSQL compartilhado

## Visão Geral
Adicionar ao workflow a definição do serviço PostgreSQL (`postgres:16-alpine`) com credenciais alinhadas ao devcontainer, incluindo health check e exportação da string de conexão para uso nos jobs.

## Requisitos
- Serviço `postgres` configurado em ambos os jobs com imagem `postgres:16-alpine`.
- Variáveis `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` definidas segundo PRD (`barbapp` / `postgres`).
- Health check via `pg_isready` garantindo que jobs aguardem serviço saudável.
- Variáveis de ambiente para `ConnectionStrings__DefaultConnection` prontas para uso nos passos .NET.

## Subtarefas
- [x] 2.1 Inserir bloco `services.postgres` no job `backend-tests` com credenciais e exposição de porta 5432.
- [x] 2.2 Replicar o serviço no job `admin-tests`, garantindo consistência de configuração.
- [x] 2.3 Validar string de conexão e variáveis exportadas conforme `docs/environment-variables.md`.
- [x] 2.4 Definição da tarefa, PRD e tech spec validados ✅
- [x] 2.5 Análise de regras e conformidade verificadas ✅
- [x] 2.6 Revisão de código completada ✅
- [x] 2.7 Pronto para deploy ✅

## Sequenciamento
- Bloqueado por: 1.0
- Desbloqueia: 3.0, 4.0, 5.0
- Paralelizável: Não (ajuste comum a ambos os jobs)

## Detalhes de Implementação
- Tech Spec: “Interfaces Principais” (bloco de serviço PostgreSQL) e “Modelos de Dados”.
- PRD: “Serviço PostgreSQL via devcontainer” (Requisitos R9, R10).

## Critérios de Sucesso
- Logs do workflow mostram serviço PostgreSQL inicializado com sucesso.
- Jobs conseguem resolver a string `Host=localhost;Database=barbapp;Username=postgres;Password=postgres`.
- Falhas de health check fazem o job aguardar ou falhar com mensagem clara.
