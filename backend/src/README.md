# BarbApp - Estrutura do Projeto

Este documento descreve a estrutura do projeto BarbApp seguindo os princípios de Clean Architecture.

## Arquitetura do Projeto

O projeto segue Clean Architecture com separação clara de responsabilidades em camadas:

```
barbApp/
└── backend/
    ├── src/
    │   ├── BarbApp.Domain/          # Camada de Domínio (Core)
    │   ├── BarbApp.Application/     # Camada de Aplicação (Use Cases)
    │   ├── BarbApp.Infrastructure/  # Camada de Infraestrutura (Implementações)
    │   └── BarbApp.API/            # Camada de Apresentação (Web API)
    └── tests/
        ├── BarbApp.Domain.Tests/
        ├── BarbApp.Application.Tests/
        └── BarbApp.IntegrationTests/
```

## Camadas

### 🎯 BarbApp.Domain
**Responsabilidade**: Núcleo do negócio, regras de domínio, entidades e value objects.

**Características**:
- Sem dependências externas (zero acoplamento)
- Contém apenas lógica de negócio pura
- Define interfaces que serão implementadas em outras camadas
- Entidades, Value Objects, Exceções de domínio

**Exemplos**:
- `BarbeariaCode` (Value Object)
- `Barbershop`, `Barber`, `Customer` (Entidades)
- `ITenantContext` (Interface)
- `InvalidBarbeariaCodeException` (Exceção de domínio)

**Dependências**: Nenhuma

---

### 📋 BarbApp.Application
**Responsabilidade**: Casos de uso, orquestração de regras de negócio, DTOs e validações.

**Características**:
- Depende apenas do Domain
- Implementa use cases (comandos e queries)
- Define interfaces de serviços (abstrações)
- DTOs para entrada/saída
- Validações com FluentValidation

**Exemplos**:
- `AuthenticateAdminCentralUseCase` (Use Case)
- `AuthenticationInput`, `AuthenticationOutput` (DTOs)
- `IAuthenticationService`, `IJwtTokenGenerator` (Interfaces)

**Dependências**: 
- `BarbApp.Domain`

---

### 🔧 BarbApp.Infrastructure
**Responsabilidade**: Implementações concretas, acesso a dados, serviços externos.

**Características**:
- Implementa interfaces definidas no Domain e Application
- Entity Framework Core, DbContext
- Repositórios
- Serviços de autenticação, hashing, JWT
- Middlewares

**Exemplos**:
- `BarbAppDbContext` (DbContext)
- `JwtTokenGenerator` (Implementação)
- `PasswordHasher` (Implementação)
- `TenantMiddleware` (Middleware)

**Dependências**: 
- `BarbApp.Domain`
- `BarbApp.Application`
- Entity Framework Core
- BCrypt.Net-Next

---

### 🌐 BarbApp.API
**Responsabilidade**: Camada de apresentação, endpoints HTTP, configuração da aplicação.

**Características**:
- Controllers
- Configuração de DI (Dependency Injection)
- Pipeline de middlewares
- Documentação Swagger
- Program.cs (entry point)

**Exemplos**:
- `AuthController` (Controller)
- `Program.cs` (Configuração)
- appsettings.json

**Dependências**: 
- `BarbApp.Application`
- `BarbApp.Infrastructure`
- Microsoft.AspNetCore.Authentication.JwtBearer

---

## Fluxo de Dependências

```
API → Infrastructure → Application → Domain
                    ↓
                  Domain (Core - sem dependências)
```

**Princípio**: As dependências sempre apontam para dentro (em direção ao Domain).

---

## Testes

### BarbApp.Domain.Tests
- Testes unitários de Value Objects
- Testes unitários de Entidades
- Testes de regras de negócio puras

**Frameworks**: xUnit, FluentAssertions

---

### BarbApp.Application.Tests
- Testes unitários de Use Cases
- Mocks de repositórios e serviços
- Validação de DTOs

**Frameworks**: xUnit, Moq, FluentAssertions

---

### BarbApp.IntegrationTests
- Testes de integração end-to-end
- Testes com banco de dados real (TestContainers)
- Testes de API (requisições HTTP)

**Frameworks**: xUnit, Moq, FluentAssertions, TestContainers (futuro)

---

## Executando o Projeto

### Build
```bash
cd backend
dotnet build
```

### Executar API
```bash
cd backend/src/BarbApp.API
dotnet run
```

### Executar Testes
```bash
# Todos os testes
cd backend
dotnet test

# Apenas testes unitários
dotnet test tests/BarbApp.Domain.Tests
dotnet test tests/BarbApp.Application.Tests

# Apenas testes de integração
dotnet test tests/BarbApp.IntegrationTests
```

---

## Convenções de Código

Siga as regras definidas em `rules/code-standard.md`:

- **Naming**:
  - Classes/Interfaces: `PascalCase`
  - Métodos/Funções: `camelCase` (seguindo convenção do projeto)
  - Variáveis: `camelCase`
  - Constantes: `PascalCase`
  - Arquivos/Diretórios: `kebab-case`

- **Estrutura**:
  - Métodos < 50 linhas
  - Classes < 300 linhas
  - Early returns
  - Evitar aninhamento > 2 níveis

---

## Próximos Passos

1. ✅ Setup e Dependências (Tarefa 1.0)
2. ⬜ Implementar Domain Layer Base (Tarefa 2.0)
3. ⬜ Implementar Entidades de Usuários (Tarefa 3.0)
4. ⬜ Configurar DbContext e Global Query Filters (Tarefa 4.0)
5. ⬜ E assim por diante conforme `tasks/prd-sistema-multi-tenant/tasks.md`

---

## Referências

- [Clean Architecture - Uncle Bob](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
