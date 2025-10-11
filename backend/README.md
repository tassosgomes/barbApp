# BarbApp Backend

API REST em .NET 8 seguindo Clean Architecture para sistema multi-tenant de gestão de barbearias.

## 🏗️ Arquitetura

```
backend/
├── BarbApp.sln
├── src/
│   ├── BarbApp.Domain/          # Domínio (entidades, value objects, interfaces)
│   ├── BarbApp.Application/     # Casos de uso e lógica de aplicação
│   ├── BarbApp.Infrastructure/  # Implementações (EF Core, JWT, etc)
│   └── BarbApp.API/            # Controllers e endpoints
└── tests/
    ├── BarbApp.Domain.Tests/
    ├── BarbApp.Application.Tests/
    └── BarbApp.IntegrationTests/
```

## 🚀 Como Executar

### Pré-requisitos
- .NET 8 SDK
- PostgreSQL (para produção/desenvolvimento completo)

### Build
```bash
dotnet build
```

### Executar API
```bash
cd src/BarbApp.API
dotnet run
```

Ou usando watch mode (hot reload):
```bash
cd src/BarbApp.API
dotnet watch run
```

A API estará disponível em: `https://localhost:7xxx` ou `http://localhost:5xxx`

### Executar Testes
```bash
# Todos os testes
dotnet test

# Com verbosidade detalhada
dotnet test --verbosity detailed

# Apenas um projeto específico
dotnet test tests/BarbApp.Domain.Tests
```

## 📦 Pacotes Principais

| Pacote | Versão | Propósito |
|--------|--------|-----------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.0.10 | Autenticação JWT |
| BCrypt.Net-Next | 4.0.3 | Hash de senhas |
| FluentAssertions | 8.7.1 | Asserções em testes |
| Moq | 4.20.72 | Mocking em testes |

## ⚙️ Configuração

### Variáveis de Ambiente

Crie um arquivo `appsettings.Development.json` (não commitado):

```json
{
  "Jwt": {
    "SecretKey": "sua-secret-key-aqui-44-caracteres",
    "Issuer": "barbapp",
    "Audience": "barbapp-api",
    "ExpirationHours": 24
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=barbapp;Username=postgres;Password=postgres"
  }
}
```

Gerar secret key segura:
```bash
openssl rand -base64 32
```

Veja mais em: [../docs/environment-variables.md](../docs/environment-variables.md)

## 🧪 Testes

### Estrutura de Testes

- **Domain.Tests**: Testes unitários de entidades e value objects
- **Application.Tests**: Testes unitários de use cases (com mocks)
- **IntegrationTests**: Testes de integração end-to-end

### Executar com Coverage

```bash
dotnet test /p:CollectCoverage=true
```

## 📚 Documentação Adicional

- [Estrutura Detalhada](src/README.md)
- [Variáveis de Ambiente](../docs/environment-variables.md)
- [Regras de Código](../rules/code-standard.md)
- [PRD Sistema Multi-tenant](../tasks/prd-sistema-multi-tenant/prd.md)
- [Tech Spec](../tasks/prd-sistema-multi-tenant/techspec.md)

## 🛠️ Comandos Úteis

```bash
# Adicionar novo pacote
dotnet add src/BarbApp.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL

# Criar nova migration (quando EF Core estiver configurado)
dotnet ef migrations add NomeDaMigration --project src/BarbApp.Infrastructure --startup-project src/BarbApp.API

# Aplicar migrations
dotnet ef database update --project src/BarbApp.Infrastructure --startup-project src/BarbApp.API

# Limpar build
dotnet clean

# Restore de pacotes
dotnet restore
```

## 🎯 Próximos Passos

1. ✅ Setup e Dependências (Tarefa 1.0)
2. ⬜ Implementar Domain Layer Base (Tarefa 2.0)
3. ⬜ Implementar Entidades de Usuários (Tarefa 3.0)
4. ⬜ Configurar DbContext e Migrations (Tarefa 4.0)

Ver roadmap completo em: [../tasks/prd-sistema-multi-tenant/tasks.md](../tasks/prd-sistema-multi-tenant/tasks.md)
