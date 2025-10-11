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

### Backend (.NET 8)

```bash
cd backend
dotnet build
dotnet run --project src/BarbApp.API
```

Acesse: `http://localhost:5000`

## 📚 Documentação

- [Estrutura do Backend](backend/src/README.md)
- [Variáveis de Ambiente](docs/environment-variables.md)
- [Regras de Código](rules/code-standard.md)
- [Regras de Commit](rules/git-commit.md)

## 🏗️ Status do Projeto

### Multi-tenant e Autenticação
- ✅ **Tarefa 1.0**: Setup e Dependências
- ⬜ **Tarefa 2.0**: Domain Layer Base
- ⬜ **Tarefa 3.0**: Entidades de Usuários
- *Ver mais em [tasks/prd-sistema-multi-tenant/tasks.md](tasks/prd-sistema-multi-tenant/tasks.md)*

## 🛠️ Stack Tecnológica

- **Backend**: .NET 8, ASP.NET Core Web API, Entity Framework Core
- **Database**: PostgreSQL
- **Autenticação**: JWT (JSON Web Tokens)
- **Testes**: xUnit, Moq, FluentAssertions

## 📋 Convenções

Este projeto segue convenções rigorosas de código definidas em `rules/`:
- Clean Architecture
- Commits semânticos
- Nomenclatura padrão (camelCase, PascalCase, kebab-case)
- Testes obrigatórios
