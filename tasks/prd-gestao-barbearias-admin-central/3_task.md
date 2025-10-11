---
status: pending
parallelizable: false
blocked_by: ["2.0"]
---

<task_context>
<domain>engine/infra/backend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database</dependencies>
<unblocks>["4.0"]</unblocks>
</task_context>

# Tarefa 3.0: Fase 3: Application Layer

## Visão Geral
Esta é a fase onde a lógica de negócio da aplicação é implementada. Inclui a criação dos DTOs para transferência de dados, os validadores para os inputs, os casos de uso (use cases) que orquestram a lógica, e os serviços de infraestrutura como o gerador de código único e a unidade de trabalho (Unit of Work).

## Requisitos
- Criar todos os DTOs de input e output para as operações de CRUD.
- Implementar validadores com FluentValidation para os DTOs de input.
- Implementar todos os casos de uso (`Create`, `Update`, `Delete`, `Get`, `List`).
- Implementar o serviço `UniqueCodeGenerator` com lógica de retry.
- Implementar o padrão `UnitOfWork` para garantir a atomicidade das operações.
- Criar testes unitários para todos os casos de uso e serviços.

## Subtarefas
- [ ] 3.1 **Criar DTOs**: Implementar `CreateBarbershopInput`, `UpdateBarbershopInput`, `BarbershopOutput`, etc.
- [ ] 3.2 **Implementar Validadores**: Criar classes de validação com FluentValidation para os inputs de criação e atualização.
- [ ] 3.3 **Implementar Casos de Uso**: Criar as classes de use case para cada operação, injetando as dependências necessárias (repositórios, serviços).
- [ ] 3.4 **Implementar `UniqueCodeGenerator`**: Desenvolver o serviço que gera códigos únicos, incluindo a lógica de verificação de duplicidade e retry.
- [ ] 3.5 **Implementar `UnitOfWork`**: Criar a interface `IUnitOfWork` e sua implementação para gerenciar as transações do `DbContext`.
- [ ] 3.6 **Testes Unitários da Aplicação**: Escrever e passar testes para todos os casos de uso, cobrindo cenários de sucesso e de erro (com mocks para os repositórios).

## Detalhes de Implementação
- **Localização**: `BarbApp.Application`, `BarbApp.Infrastructure`, `BarbApp.Application.Tests`
- **Stack**: .NET 8, FluentValidation, Moq
- Os casos de uso devem orquestrar a interação entre o domínio e a infraestrutura, mas sem conter lógica de domínio complexa.
- O `UnitOfWork` deve ser injetado nos casos de uso e chamado ao final das operações de escrita (`Create`, `Update`, `Delete`).

## Critérios de Sucesso
- DTOs e validadores implementados e testados.
- Todos os casos de uso implementados e com cobertura de testes unitários > 90%.
- `UniqueCodeGenerator` funcionando e testado, incluindo o cenário de colisão.
- `UnitOfWork` implementado e sendo utilizado corretamente nos casos de uso.
- Lógica de negócio funcionando corretamente nos testes unitários.
