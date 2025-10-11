---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>engine/infra/backend</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>["3.0"]</unblocks>
</task_context>

# Tarefa 2.0: Fase 2: Infraestrutura de Dados

## Visão Geral
Esta tarefa foca em conectar o domínio com o banco de dados. Envolve a configuração do `DbContext` do Entity Framework Core, a criação das migrations para o schema do banco de dados e a implementação dos repositórios que farão a ponte entre a lógica de aplicação e a persistência dos dados.

## Requisitos
- Configurar o `BarbAppDbContext` com os `DbSet`s para `Barbershop` e `Address`.
- Definir os mapeamentos das entidades e value objects para o schema do banco de dados.
- Criar a migration inicial do banco de dados.
- Implementar as interfaces de repositório (`IBarbershopRepository`, `IAddressRepository`) na camada de infraestrutura.
- Implementar as queries de listagem com suporte a filtros e paginação.

## Subtarefas
- [ ] 2.1 **Configurar DbContext**: Criar `BarbAppDbContext.cs` e adicionar os `DbSet`s.
- [ ] 2.2 **Mapeamento de Entidades**: Usar `EntityTypeConfiguration` para mapear as entidades `Barbershop` e `Address` para as tabelas `barbershops` e `addresses`.
- [ ] 2.3 **Mapeamento de Value Objects**: Configurar o mapeamento para `Document` e `UniqueCode`.
- [ ] 2.4 **Criar Migration**: Gerar a primeira migration do EF Core e verificar o script SQL gerado.
- [ ] 2.5 **Implementar Repositórios**: Criar as classes `BarbershopRepository` e `AddressRepository` que implementam as interfaces do domínio.
- [ ] 2.6 **Implementar Queries**: Desenvolver a lógica de listagem com busca, filtros e paginação no `BarbershopRepository`.

## Detalhes de Implementação
- **Localização**: `BarbApp.Infrastructure`
- **Stack**: .NET 8, EF Core, PostgreSQL
- Seguir o schema de banco de dados definido na Especificação Técnica, incluindo nomes de tabelas/colunas em snake_case e a criação de índices.
- A migration deve ser testada em um banco de dados PostgreSQL local.

## Critérios de Sucesso
- `DbContext` configurado e conectando com o banco de dados.
- Migration inicial criada e aplicada com sucesso.
- Repositórios implementados, permitindo operações de CRUD básicas.
- A query de listagem funciona conforme especificado, com paginação e filtros.
