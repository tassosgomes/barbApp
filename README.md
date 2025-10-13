# barbApp

Sistema de gestão multi-tenant para barbearias com autenticação e agendamento online.

## 📁 Estrutura do Projeto

```
barbApp/
├── backend/              # API .NET 8 + Clean Architecture
│   ├── src/
│   │   ├── BarbApp.Domain/
│   │   ├── BarbApp.Application/
│   │   ├── BarbApp.Infrastructure/
│   │   └── BarbApp.API/
│   └── tests/
├── docs/                 # Documentação técnica
├── rules/                # Regras de código e padrões
├── tasks/                # PRDs e especificações técnicas
└── templates/            # Templates de documentação

```

## 🚀 Quick Start

### Pré-requisitos

- .NET 8 SDK
- PostgreSQL 15+
- Docker (opcional, para testes com TestContainers)

### Backend (.NET 8)

1. **Clone e navegue para o diretório:**
   ```bash
   git clone <repository-url>
   cd barbApp/backend
   ```

2. **Configure o banco de dados:**
   - Instale e inicie PostgreSQL
   - Crie um banco de dados chamado `barbapp`
   - Configure a connection string em `appsettings.json` ou variáveis de ambiente

3. **Execute as migrações:**
   ```bash
   dotnet ef database update --project src/BarbApp.API
   ```

4. **Execute a aplicação:**
   ```bash
   dotnet run --project src/BarbApp.API
   ```

Acesse:
- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### Executar Testes

```bash
# Todos os testes
dotnet test

# Apenas testes unitários
dotnet test --filter "Category=Unit"

# Apenas testes de integração (requer Docker)
dotnet test --filter "Category=Integration"
```

## 📚 Documentação

- [Estrutura do Backend](backend/src/README.md)
- [Variáveis de Ambiente](docs/environment-variables.md)
- [Regras de Código](rules/code-standard.md)
- [Regras de Commit](rules/git-commit.md)
- [Gestão de Barbearias - PRD](tasks/prd-gestao-barbearias-admin-central/prd.md)
- [Gestão de Barbearias - Tech Spec](tasks/prd-gestao-barbearias-admin-central/techspec.md)

## 🏗️ Status do Projeto

### ✅ Gestão de Barbearias (Admin Central) - Concluído
- ✅ **Tarefa 1.0**: Setup e Dependências
- ✅ **Tarefa 2.0**: Domain Layer Base
- ✅ **Tarefa 3.0**: Entidades de Usuários
- ✅ **Tarefa 4.0**: API Base e Autenticação
- ✅ **Tarefa 5.0**: Testes de Integração
- ✅ **Tarefa 6.0**: Refinamento e Logging

**Funcionalidades implementadas:**
- CRUD completo de barbearias
- Geração automática de códigos únicos
- Validação de CNPJ/CPF
- Autenticação JWT multi-tenant
- Testes abrangentes (unitários e integração)
- Logging estruturado

### Próximas Features
- ⬜ **Gestão de Barbeiros** (Admin Barbearia)
- ⬜ **Agendamentos** (Barbeiro/Cliente)
- ⬜ **Dashboard e Relatórios**

## 🛠️ Stack Tecnológica

- **Backend**: .NET 8, ASP.NET Core Web API, Entity Framework Core
- **Database**: PostgreSQL
- **Autenticação**: JWT (JSON Web Tokens)
- **Testes**: xUnit, Moq, FluentAssertions, TestContainers
- **Logging**: Serilog + Microsoft.Extensions.Logging
- **Documentação**: Swagger/OpenAPI

## 📋 Convenções

Este projeto segue convenções rigorosas de código definidas em `rules/`:
- Clean Architecture
- Commits semânticos
- Nomenclatura padrão (camelCase, PascalCase, kebab-case)
- Testes obrigatórios
- Logging estruturado (sem PII)
- Code reviews obrigatórios

## 🔧 Configuração de Desenvolvimento

### Variáveis de Ambiente

```bash
# Database
BARBAPP_CONNECTION_STRING="Host=localhost;Database=barbapp;Username=postgres;Password=password"

# JWT
BARBAPP_JWT_SECRET="your-256-bit-secret"
BARBAPP_JWT_ISSUER="BarbApp"
BARBAPP_JWT_AUDIENCE="BarbApp-Users"

# Logging
BARBAPP_LOG_LEVEL="Information"
```

### Debugging

- Use `dotnet watch run` para hot reload durante desenvolvimento
- Logs são gravados em `logs/barbapp-.txt` (formato rolling)
- Use Swagger UI para testar endpoints autenticados
