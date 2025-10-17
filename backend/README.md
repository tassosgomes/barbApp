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

## 📧 Template de Email de Boas-Vindas

Quando uma nova barbearia é cadastrada, um email de boas-vindas é enviado automaticamente para o administrador da barbearia contendo:

- Credenciais de acesso (email e senha temporária)
- Link personalizado para acesso ao painel administrativo
- Instruções de primeiros passos

### Formato do Link de Acesso

O link segue o padrão: `{FRONTEND_URL}/{CODIGO}/login`

**Exemplo:**
```
http://app.barbapp.com/ABC12345/login
```

Onde:
- `FRONTEND_URL`: URL base do frontend (configurado em `appsettings.json`)
- `CODIGO`: Código único de 8 caracteres alfanuméricos da barbearia

### Template HTML (Exemplo)

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Bem-vindo ao BarbApp</title>
</head>
<body style="font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;">
    <div style="background-color: #f8f9fa; border-radius: 10px; padding: 30px; margin-bottom: 20px;">
        <h1 style="color: #2c3e50; margin-bottom: 10px;">Bem-vindo ao BarbApp! 💈</h1>
        <p style="font-size: 16px; margin-bottom: 20px;">
            Olá! Sua barbearia <strong>Barbearia do Tasso Zé</strong> foi cadastrada com sucesso.
        </p>
    </div>

    <div style="background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;">
        <h2 style="color: #2c3e50; margin-bottom: 15px;">Suas Credenciais de Acesso</h2>
        <p style="margin-bottom: 15px;">Use as credenciais abaixo para acessar o painel administrativo:</p>
        
        <div style="background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; border-radius: 5px;">
            <p style="margin: 5px 0;"><strong>E-mail:</strong> tasso.gomes@outlook.com</p>
            <p style="margin: 5px 0;"><strong>Senha:</strong> <code style="background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; font-size: 14px; font-family: 'Courier New', monospace;">96z7ZBK#DXNn</code></p>
        </div>

        <div style="text-align: center; margin: 30px 0;">
            <a href="http://app.barbapp.com/6SJJRFPD/login" 
               style="background-color: #007bff; 
                      color: white; 
                      padding: 12px 30px; 
                      text-decoration: none; 
                      border-radius: 5px;
                      display: inline-block;
                      font-weight: bold;">
                Acessar Painel Administrativo
            </a>
        </div>

        <p style="text-align: center; color: #6c757d; font-size: 14px;">
            Ou copie e cole este link no seu navegador:<br>
            <span style="background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; word-break: break-all; font-family: 'Courier New', monospace; font-size: 12px;">http://app.barbapp.com/6SJJRFPD/login</span>
        </p>

        <div style="background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px;">
            <p style="margin: 0; color: #856404;">
                <strong>⚠️ Importante:</strong> Guarde sua senha em local seguro. Recomendamos alterá-la após o primeiro acesso.
            </p>
        </div>
    </div>

    <div style="background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;">
        <h2 style="color: #2c3e50; margin-bottom: 15px;">Próximos Passos</h2>
        <ol style="margin: 0; padding-left: 20px;">
            <li style="margin-bottom: 10px;">Clique no link acima ou copie e cole no seu navegador</li>
            <li style="margin-bottom: 10px;">Faça login com seu e-mail e senha</li>
            <li style="margin-bottom: 10px;">Cadastre seus barbeiros</li>
            <li style="margin-bottom: 10px;">Configure seus serviços e horários</li>
            <li>Comece a receber agendamentos!</li>
        </ol>
    </div>

    <div style="text-align: center; color: #6c757d; font-size: 14px; padding-top: 20px; border-top: 1px solid #dee2e6;">
        <p>Caso tenha alguma dúvida, entre em contato com nosso suporte.</p>
        <p style="margin: 5px 0;">BarbApp - Gestão de Barbearias</p>
    </div>
</body>
</html>
```

### Template Texto Plano (Fallback)

```
Bem-vindo ao BarbApp! 

Sua barbearia "Barbearia do Tasso Zé" foi cadastrada com sucesso.

=== CREDENCIAIS DE ACESSO ===

E-mail: tasso.gomes@outlook.com
Senha: 96z7ZBK#DXNn

=== ACESSO AO SISTEMA ===

Acesse o painel administrativo em:
http://app.barbapp.com/6SJJRFPD/login

⚠️ IMPORTANTE: Guarde sua senha em local seguro. Recomendamos alterá-la após o primeiro acesso.

=== PRÓXIMOS PASSOS ===

1. Clique no link acima ou copie e cole no seu navegador
2. Faça login com seu e-mail e senha
3. Cadastre seus barbeiros
4. Configure seus serviços e horários
5. Comece a receber agendamentos!

Caso tenha alguma dúvida, entre em contato com nosso suporte.

BarbApp - Gestão de Barbearias
```

### Configuração

O URL base do frontend é configurado em `appsettings.json`:

```json
{
  "AppSettings": {
    "FrontendUrl": "http://app.barbapp.com"
  }
}
```

Para desenvolvimento local, use:
```json
{
  "AppSettings": {
    "FrontendUrl": "http://localhost:3000"
  }
}
```

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

### Observabilidade (Sentry)

SDK do Sentry inicializado via `UseSentry` no host. Configure via `appsettings.json` (seção `Sentry`) ou variáveis de ambiente. Por padrão, dados PII não são enviados (`SendDefaultPii = false`) e o modo global está habilitado (`IsGlobalModeEnabled = true`).

- Variáveis esperadas (ambiente):
  - `SENTRY_DSN` (obrigatório em ambientes que reportam)
  - `SENTRY_ENVIRONMENT` (opcional; default: `ASPNETCORE_ENVIRONMENT`)
  - `SENTRY_RELEASE` (recomendado; definido pelo CI/CD)
  - `SENTRY_TRACES_SAMPLE_RATE` (opcional; default: `0.05`)

- appsettings (`src/BarbApp.API/appsettings.json`):
  - `Sentry:Dsn`, `Sentry:Environment`, `Sentry:Release`, `Sentry:TracesSampleRate`
  - Exemplo:
    ```json
    {
      "Sentry": {
        "Dsn": "${SENTRY_DSN}",
        "Environment": "${ASPNETCORE_ENVIRONMENT}",
        "Release": "${SENTRY_RELEASE}",
        "TracesSampleRate": 0.05
      }
    }
    ```

Precedência de configuração: variáveis de ambiente sobrescrevem `appsettings`. Se usar `.env`, mapeie `Sentry__Dsn`, `Sentry__Environment` etc., ou exporte `SENTRY_*` (suportado pelo código de bootstrap).

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
