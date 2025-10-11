---
status: pending
parallelizable: false
blocked_by: []
---

<task_context>
<domain>engine/infra/backend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>["2.0"]</unblocks>
</task_context>

# Tarefa 1.0: Fase 1: Fundação (Domain + Infrastructure Base)

## Visão Geral
Esta tarefa estabelece a base do projeto, configurando a solução, os projetos individuais e as dependências essenciais. Também inclui a implementação do núcleo do domínio, com entidades, value objects e exceções, garantindo uma base sólida e testável para o resto da aplicação.

## Requisitos
- Criar a estrutura de projetos da solução .NET (`Domain`, `Application`, `Infrastructure`, `API`, `Tests`).
- Configurar todas as dependências necessárias via NuGet (EF Core, FluentValidation, xUnit, TestContainers).
- Implementar as entidades de domínio `Barbershop` e `Address`.
- Implementar os Value Objects `Document` e `UniqueCode`.
- Definir e implementar as exceções de domínio customizadas.
- Criar e passar em todos os testes unitários para o Domain Layer.

## Subtarefas
- [ ] 1.1 **Setup de Projetos**: Criar a solution e os projetos no .NET.
- [ ] 1.2 **Configurar Dependências**: Adicionar pacotes NuGet (EF Core, FluentValidation, xUnit, Moq, FluentAssertions, TestContainers).
- [ ] 1.3 **Implementar Value Objects**: Criar `Document.cs` e `UniqueCode.cs` com suas validações.
- [ ] 1.4 **Implementar Entidades**: Criar `Address.cs` e `Barbershop.cs` com seus métodos de fábrica e de negócio.
- [ ] 1.5 **Implementar Exceções**: Criar exceções como `DuplicateDocumentException` e `InvalidDocumentException`.
- [ ] 1.6 **Testes Unitários do Domínio**: Escrever e passar testes para todas as entidades, value objects e lógicas de negócio no Domain Layer.

## Detalhes de Implementação
- **Localização**: `BarbApp.Domain`, `BarbApp.Domain.Tests`
- **Stack**: .NET 8, xUnit, FluentAssertions
- Seguir estritamente os modelos de dados e value objects definidos na Especificação Técnica.
- Garantir que a camada de domínio seja completamente independente de frameworks de infraestrutura (como EF Core).

## Critérios de Sucesso
- Estrutura de projetos criada e compilando sem erros.
- Todas as dependências configuradas.
- Entidades e Value Objects implementados conforme a especificação.
- Cobertura de testes unitários para o Domain Layer acima de 95%.
- Todos os testes unitários do domínio passando.
