# BarbApp API Documentation

## Visão Geral
API REST para sistema de gerenciamento de barbearias com suporte multi-tenant.

## Base URL
- **Development**: `http://localhost:5000`
- **Production**: `https://api.barbapp.com`

## Autenticação
Todos os endpoints protegidos requerem autenticação JWT Bearer token.

### Obtendo um Token
1. Faça login usando um dos endpoints de autenticação
2. Use o token retornado no header `Authorization: Bearer {token}`

### Exemplo
```bash
# Login
curl -X POST http://localhost:5000/api/auth/admin-central/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@barbapp.com","senha":"Admin@123"}'

# Resposta
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminCentral",
  "barbeariaId": null,
  "nomeBarbearia": "",
  "expiresAt": "2024-01-15T18:00:00Z"
}

# Usando o token
curl -X GET http://localhost:5000/api/auth/barbeiros \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

## Endpoints

### Autenticação

#### POST /api/auth/admin-central/login
Autentica um administrador central.

**Request Body:**
```json
{
  "email": "admin@barbapp.com",
  "senha": "Admin@123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminCentral",
  "barbeariaId": null,
  "nomeBarbearia": "",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/admin-barbearia/login
Autentica um administrador de barbearia.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "email": "admin@barbearia1.com",
  "senha": "Admin@123"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "AdminBarbearia",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/barbeiro/login
Autentica um barbeiro.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "telefone": "11987654321"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Barbeiro",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### POST /api/auth/cliente/login
Autentica um cliente.

**Request Body:**
```json
{
  "codigo": "ABC12345",
  "telefone": "11987654321",
  "nome": "João Silva"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Cliente",
  "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nomeBarbearia": "Barbearia Premium",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

#### GET /api/auth/barbeiros
Lista barbeiros da barbearia do usuário autenticado. **[Requer Autenticação]**

**Response 200:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nome": "João Silva",
    "telefone": "11987654321",
    "barbeariaId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "nomeBarbearia": "Barbearia Premium"
  }
]
```

#### POST /api/auth/barbeiro/trocar-contexto
Troca contexto de barbearia para barbeiro. **[Requer Autenticação]**

**Request Body:**
```json
{
  "novaBarbeariaId": "4ea95f64-5717-4562-b3fc-2c963f66afa7"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tipoUsuario": "Barbeiro",
  "barbeariaId": "4ea95f64-5717-4562-b3fc-2c963f66afa7",
  "nomeBarbearia": "Barbearia Nova",
  "expiresAt": "2024-01-15T18:00:00Z"
}
```

## Tipos de Usuário

| Tipo | Descrição | Acesso |
|------|-----------|--------|
| AdminCentral | Administrador do sistema | Acesso total |
| AdminBarbearia | Administrador de barbearia | Dados da barbearia |
| Barbeiro | Profissional barbeiro | Dados da(s) barbearia(s) |
| Cliente | Cliente do sistema | Dados pessoais e agendamentos |

## Códigos de Status

| Código | Descrição |
|--------|-----------|
| 200 | Sucesso |
| 400 | Requisição inválida |
| 401 | Não autenticado |
| 403 | Sem permissão |
| 404 | Não encontrado |
| 500 | Erro interno |

## Postman Collection
Importe a collection do Postman: [BarbApp.postman_collection.json](./BarbApp.postman_collection.json)

## Swagger UI
Acesse a documentação interativa em: http://localhost:5000/swagger

---

## 🏗️ Arquitetura Técnica

API REST em .NET 8 seguindo Clean Architecture para sistema multi-tenant de gestão de barbearias.

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
