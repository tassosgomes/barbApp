---
status: completed
parallelizable: false
blocked_by: ["4.0"]
---

<task_context>
<domain>engine/infra/backend</domain>
<type>testing</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database,http_server</dependencies>
<unblocks>["6.0"]</unblocks>
</task_context>

# Tarefa 5.0: Fase 5: Testes de Integração

## Visão Geral
Esta fase é crucial para garantir que todos os componentes do sistema funcionem juntos corretamente. Serão criados testes de integração end-to-end que simulam requisições HTTP reais à API e verificam o comportamento completo do sistema, incluindo a interação com um banco de dados real (provisionado via TestContainers).

## Requisitos
- Configurar um projeto de testes de integração com TestContainers para provisionar um banco de dados PostgreSQL isolado para os testes.
- Implementar testes para os cenários críticos de negócio, cobrindo todo o fluxo desde a API até o banco de dados.
- Testar os fluxos completos de CRUD (Create, Read, Update, Delete).
- Validar os casos de erro, como criação de documento duplicado e acesso não autorizado.

## Subtarefas
- [x] 5.1 **Configurar TestContainers**: Adicionar o pacote TestContainers.PostgreSql e configurar a classe base de testes para iniciar e parar o contêiner do banco de dados.
- [x] 5.2 **Configurar `WebApplicationFactory`**: Usar `WebApplicationFactory` para hospedar a API em memória e sobrescrever a conexão do banco de dados para apontar para o TestContainer.
- [x] 5.3 **Implementar Teste de Criação**: Criar um teste para o endpoint `POST /api/barbearias`, validando a resposta 201 e a persistência correta dos dados no banco.
- [x] 5.4 **Implementar Teste de Listagem e Paginação**: Criar um teste que insere múltiplos registros e verifica se a paginação, filtros e ordenação do endpoint `GET /api/barbearias` funcionam.
- [x] 5.5 **Implementar Teste de Atualização e Exclusão**: Criar testes para os endpoints `PUT` e `DELETE`, verificando se os dados são atualizados e removidos (ou desativados) corretamente.
- [x] 5.6 **Implementar Testes de Erro**: Criar testes para cenários como tentar criar uma barbearia com um CNPJ duplicado (esperando 422) ou acessar um endpoint sem o token de Admin (esperando 401/403).

## Detalhes de Implementação
- **Localização**: `BarbApp.IntegrationTests`
- **Stack**: xUnit, TestContainers, `WebApplicationFactory`
- Os testes devem ser independentes e capazes de rodar em paralelo.
- O banco de dados deve ser limpo e as migrations aplicadas antes de cada execução de teste para garantir um estado inicial consistente.
- Um token JWT de `AdminCentral` falso deve ser gerado e incluído nas requisições para simular um usuário autenticado.

## Critérios de Sucesso
- Projeto de testes de integração configurado e rodando.
- Testes cobrindo todos os cenários críticos de CRUD e de erro.
- Todos os testes de integração passando, validando o comportamento end-to-end da aplicação.
- Confiança de que a API se comporta conforme o esperado em um ambiente próximo ao de produção.
