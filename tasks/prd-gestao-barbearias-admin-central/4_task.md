---
status: pending
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>engine/infra/backend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>http_server</dependencies>
<unblocks>["5.0"]</unblocks>
</task_context>

# Tarefa 4.0: Fase 4: API Layer

## Visão Geral
Esta tarefa consiste em expor a lógica de negócio implementada na camada de aplicação através de uma API RESTful. Envolve a criação do `BarbershopsController`, a configuração do roteamento, a implementação da autorização e a configuração da documentação da API com Swagger/OpenAPI.

## Requisitos
- Implementar o `BarbershopsController` com endpoints para todas as operações de CRUD.
- Configurar o roteamento e os métodos HTTP conforme a especificação REST.
- Aplicar a autorização para garantir que apenas o `Admin Central` possa acessar os endpoints.
- Implementar um middleware de tratamento de exceções para converter exceções de domínio e aplicação em respostas HTTP apropriadas.
- Configurar e documentar a API usando Swagger/OpenAPI.

## Subtarefas
- [ ] 4.1 **Implementar Controller**: Criar `BarbershopsController.cs` e injetar os casos de uso.
- [ ] 4.2 **Criar Endpoints**: Implementar os métodos para `POST`, `PUT`, `GET` (single e list) e `DELETE`.
- [ ] 4.3 **Configurar Autorização**: Adicionar o atributo `[Authorize(Roles = "AdminCentral")]` ao controller ou aos endpoints.
- [ ] 4.4 **Middleware de Exceções**: Criar um middleware que capture exceções (`NotFoundException`, `ValidationException`, etc.) e retorne os status codes corretos (404, 400, 422, etc.).
- [ ] 4.5 **Configurar DI**: Registrar todas as dependências (repositórios, use cases, serviços) no `Program.cs`.
- [ ] 4.6 **Documentação da API**: Configurar o Swagger e adicionar documentação (XML comments) aos endpoints para descrever o que eles fazem, seus parâmetros e respostas.

## Detalhes de Implementação
- **Localização**: `BarbApp.API`
- **Stack**: .NET 8, ASP.NET Core, Swashbuckle
- Seguir estritamente a definição dos endpoints, incluindo URLs, métodos HTTP, e formatos de request/response, conforme a Especificação Técnica.
- A autorização depende da implementação do PRD-5 (Multi-tenant).

## Critérios de Sucesso
- Todos os endpoints do CRUD de barbearias estão implementados e funcionando.
- A autorização está bloqueando o acesso para usuários não autorizados.
- O tratamento de exceções está convertendo erros em respostas HTTP significativas.
- A API está documentada no Swagger e pode ser testada manualmente através da UI do Swagger.
