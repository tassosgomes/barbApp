# Environment Variables Guide

## Overview

O backend do BarbApp utiliza variáveis de ambiente para toda configuração sensível, seguindo a metodologia **12-Factor App**. Nenhum segredo deve estar hardcoded nos arquivos de configuração.

## Quick Start

### 1. Criar arquivo .env

```bash
cd backend
cp .env.example .env
```

### 2. Preencher valores reais

Edite `backend/.env` com os valores do seu ambiente:

```bash
# Database
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=barbapp_dev;Username=user;Password=pass

# JWT (mínimo 32 caracteres)
JWT_SECRET=your-super-secret-key-at-least-32-chars-long
JWT_ISSUER=BarbApp-Dev
JWT_AUDIENCE=BarbApp-Dev-Users

# Frontend
FRONTEND_URL=http://localhost:5173

# SMTP
SMTP_HOST=smtp.gmail.com
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_FROM_EMAIL=noreply@barbapp.com
SMTP_FROM_NAME=BarbApp

# Sentry
SENTRY_DSN=https://your-dsn@sentry.io/project
SENTRY_ENVIRONMENT=Development
SENTRY_RELEASE=barbapp-api-0.1.0

# Cloudflare R2
R2_ENDPOINT=https://your-account-id.r2.cloudflarestorage.com
R2_BUCKET_NAME=barbapp-logos
R2_PUBLIC_URL=https://pub-your-id.r2.dev
R2_ACCESS_KEY_ID=your-access-key
R2_SECRET_ACCESS_KEY=your-secret-key
```

### 3. Executar aplicação

```bash
cd src/BarbApp.API
dotnet run
```

Você verá:
```
✓ Loaded .env from: /path/to/backend/.env
✓ Applied 16 environment variable overrides
```

## Architecture

### File Structure

```
backend/
├── .env                          # Local (gitignored) - valores reais
├── .env.example                  # Template (commitado)
└── src/BarbApp.API/
    ├── appsettings.json         # Valores padrão
    └── appsettings.Development.json  # Placeholders ${VAR} (commitado)
```

### Load Order & Override Mechanism

1. `Program.cs` carrega `backend/.env` via `DotNetEnv`
2. ASP.NET Core carrega `appsettings.json`
3. ASP.NET Core carrega `appsettings.{Environment}.json`
4. **Program.cs injeta environment variables como overrides** ⭐ NEW
5. Configuration final disponível via `IConfiguration`

### Implementation Details

**Program.cs - Load .env:**
```csharp
var backendRoot = Path.Combine(Directory.GetCurrentDirectory(), "..", "..");
var envPath = Path.Combine(backendRoot, ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath);
    Console.WriteLine($"✓ Loaded .env from: {envPath}");
}
```

**Program.cs - Override configuration:**
```csharp
var overrides = new Dictionary<string, string?>
{
    ["ConnectionStrings:DefaultConnection"] = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING"),
    ["JwtSettings:Secret"] = Environment.GetEnvironmentVariable("JWT_SECRET"),
    ["JwtSettings:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
    // ... mais 13 variáveis
};

var nonNullOverrides = overrides
    .Where(kvp => kvp.Value != null)
    .Select(kvp => new KeyValuePair<string, string?>(kvp.Key, kvp.Value))
    .ToList();
    
if (nonNullOverrides.Any())
{
    builder.Configuration.AddInMemoryCollection(nonNullOverrides);
    Console.WriteLine($"✓ Applied {nonNullOverrides.Count} environment variable overrides");
}
```

**appsettings.Development.json - Placeholders:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "${DB_CONNECTION_STRING}"
  },
  "JwtSettings": {
    "Secret": "${JWT_SECRET}",
    "Issuer": "${JWT_ISSUER}",
    "Audience": "${JWT_AUDIENCE}"
  }
}
```

> **Nota**: Os placeholders `${VAR}` são apenas documentação. O Program.cs faz a substituição real via `AddInMemoryCollection()`.

## Variable Reference

### 📊 Database (1 variável)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `DB_CONNECTION_STRING` | PostgreSQL connection string completa | `Host=localhost;Port=5432;Database=barbapp_dev;Username=user;Password=pass` |

### 🔐 JWT (3 variáveis)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `JWT_SECRET` | Chave de assinatura JWT (**mínimo 32 chars**) | `development-secret-key-at-least-32-chars-long` |
| `JWT_ISSUER` | Emissor do token | `BarbApp-Dev` |
| `JWT_AUDIENCE` | Audiência do token | `BarbApp-Dev-Users` |

> ⚠️ **Segurança**: O `JWT_SECRET` deve ter no mínimo 32 caracteres e ser diferente em cada ambiente.

### 🌐 Frontend (1 variável)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `FRONTEND_URL` | URL do frontend (para CORS) | `http://localhost:5173` |

### 📧 SMTP (5 variáveis)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `SMTP_HOST` | Servidor SMTP | `smtp.gmail.com` |
| `SMTP_USERNAME` | Usuário SMTP (pode ser vazio) | `your-email@gmail.com` |
| `SMTP_PASSWORD` | Senha SMTP (pode ser vazia) | `your-app-password` |
| `SMTP_FROM_EMAIL` | Email remetente | `noreply@barbapp.com` |
| `SMTP_FROM_NAME` | Nome do remetente | `BarbApp` |

> 💡 **Desenvolvimento**: Use `FakeEmailService` (ativo automaticamente em Development) para não precisar de SMTP configurado.

### 🐛 Sentry (3 variáveis)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `SENTRY_DSN` | Sentry Data Source Name | `https://key@org.ingest.sentry.io/project` |
| `SENTRY_ENVIRONMENT` | Nome do ambiente | `Development` / `Production` |
| `SENTRY_RELEASE` | Versão da release | `barbapp-api-0.1.0` |

### ☁️ Cloudflare R2 (5 variáveis)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `R2_ENDPOINT` | Endpoint da API R2 | `https://your-account-id.r2.cloudflarestorage.com` |
| `R2_BUCKET_NAME` | Nome do bucket | `barbapp-logos` |
| `R2_PUBLIC_URL` | URL pública do CDN | `https://pub-your-id.r2.dev` |
| `R2_ACCESS_KEY_ID` | R2 Access Key ID | `a1b2c3d4e5f6...` |
| `R2_SECRET_ACCESS_KEY` | R2 Secret Access Key | `secretkey123...` |

> 📚 **Setup R2**: Veja [cloudflare-r2-setup.md](cloudflare-r2-setup.md) para instruções completas.

## Environment-Specific Configuration

### Development

```bash
# backend/.env
DB_CONNECTION_STRING=Host=localhost;Port=5432;Database=barbapp_dev;Username=barbapp_user;Password=barbapp_password
JWT_SECRET=development-secret-key-at-least-32-chars-long
JWT_ISSUER=BarbApp-Dev
JWT_AUDIENCE=BarbApp-Dev-Users
FRONTEND_URL=http://localhost:5173
SMTP_HOST=smtp.gmail.com
SMTP_USERNAME=
SMTP_PASSWORD=
SMTP_FROM_EMAIL=dev@barbapp.com
SMTP_FROM_NAME=BarbApp Dev
SENTRY_DSN=https://your-dsn@sentry.io/project
SENTRY_ENVIRONMENT=Development
SENTRY_RELEASE=barbapp-api-0.1.0
R2_ENDPOINT=https://dev-account-id.r2.cloudflarestorage.com
R2_BUCKET_NAME=barbapp-logos-dev
R2_PUBLIC_URL=https://dev-pub-id.r2.dev
R2_ACCESS_KEY_ID=dev-access-key
R2_SECRET_ACCESS_KEY=dev-secret-key
```

### Production

```bash
# Via Docker Secrets / Kubernetes ConfigMap
DB_CONNECTION_STRING=Host=postgres.prod.svc;Port=5432;Database=barbapp;Username=${DB_USER};Password=${DB_PASS}
JWT_SECRET=${JWT_SECRET_FROM_VAULT}
JWT_ISSUER=BarbApp
JWT_AUDIENCE=BarbApp-Users
FRONTEND_URL=https://barbapp.com.br
SMTP_HOST=smtp.sendgrid.net
SMTP_USERNAME=${SMTP_USER}
SMTP_PASSWORD=${SMTP_PASS}
SMTP_FROM_EMAIL=noreply@barbapp.com.br
SMTP_FROM_NAME=BarbApp
SENTRY_DSN=${SENTRY_DSN}
SENTRY_ENVIRONMENT=Production
SENTRY_RELEASE=barbapp-api-${CI_COMMIT_SHA}
R2_ENDPOINT=https://prod-account-id.r2.cloudflarestorage.com
R2_BUCKET_NAME=barbapp-logos
R2_PUBLIC_URL=https://cdn.barbapp.com.br
R2_ACCESS_KEY_ID=${R2_KEY}
R2_SECRET_ACCESS_KEY=${R2_SECRET}
```

### Docker

No `docker-compose.yml`, as variáveis podem vir de:

1. **Arquivo .env na raiz** (docker-compose.yml level)
2. **env_file** no serviço
3. **environment** hardcoded (não recomendado)

```yaml
services:
  api:
    image: barbapp-api
    env_file:
      - backend/.env
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
```

## Security Best Practices

### ✅ DO

- ✅ Use `.env` para valores sensíveis (gitignored)
- ✅ Commit `.env.example` como template
- ✅ Use variáveis de ambiente em CI/CD
- ✅ Rotate secrets periodicamente (90 dias)
- ✅ Use diferentes secrets por ambiente
- ✅ JWT_SECRET com mínimo 32 caracteres
- ✅ Use secrets management (Vault, AWS Secrets Manager)

### ❌ DON'T

- ❌ Commit `.env` no git
- ❌ Hardcode secrets em appsettings.json
- ❌ Reutilize secrets entre ambientes
- ❌ Compartilhe secrets via email/chat
- ❌ Use JWT_SECRET curto (<32 chars)
- ❌ Commit logs com valores de variáveis

## Troubleshooting

### Erro: "Format of initialization string does not conform"

**Causa**: `DB_CONNECTION_STRING` não foi carregada ou está inválida.

**Solução**:
```bash
# Verifique se .env existe
ls -la backend/.env

# Verifique se DB_CONNECTION_STRING está definida
cat backend/.env | grep DB_CONNECTION_STRING

# Teste manualmente
cd backend/src/BarbApp.API
dotnet run
# Deve exibir: "✓ Applied 16 environment variable overrides"
```

### Erro: "✓ Applied 0 environment variable overrides"

**Causa**: `.env` não encontrado ou variáveis não definidas.

**Solução**:
```bash
cd backend
cp .env.example .env
# Edite .env com valores reais
nano .env
```

### SMTP não funciona em desenvolvimento

**Isso é esperado!** O `FakeEmailService` está ativo automaticamente em Development. Emails são logados no console, não enviados.

Para testar SMTP real em desenvolvimento:
```csharp
// Program.cs - Comente temporariamente:
if (builder.Environment.IsDevelopment())
{
    // builder.Services.AddScoped<IEmailService, FakeEmailService>(); // <-- Comente
    builder.Services.AddScoped<IEmailService, SmtpEmailService>(); // <-- Adicione
}
```

### Variáveis do R2 não estão sendo aplicadas

**Causa**: Typo no nome da variável ou valores não definidos.

**Solução**:
```bash
# Verifique todas as variáveis do R2
cd backend
cat .env | grep R2_

# Deve exibir 5 linhas:
# R2_ENDPOINT=...
# R2_BUCKET_NAME=...
# R2_PUBLIC_URL=...
# R2_ACCESS_KEY_ID=...
# R2_SECRET_ACCESS_KEY=...
```

### Erro: "Signature mismatch" no upload R2

**Causa**: Credenciais inválidas ou endpoint incorreto.

**Solução**:
1. Verifique credenciais no Cloudflare Dashboard
2. Verifique `R2_ENDPOINT` (deve incluir account ID)
3. Teste com curl:
```bash
curl -X PUT "https://your-account-id.r2.cloudflarestorage.com/barbapp-logos/test.txt" \
  -H "Authorization: Bearer $R2_ACCESS_KEY_ID" \
  -d "test"
```

## Migration Guide

### Para desenvolvedores existentes

Se você tinha valores hardcoded em `appsettings.Development.json` antes do commit `556d720`:

1. **Backup dos valores antigos**:
   ```bash
   git show 556d720~1:backend/src/BarbApp.API/appsettings.Development.json > /tmp/old-appsettings.json
   ```

2. **Extrair valores para .env**:
   ```bash
   cd backend
   cp .env.example .env
   # Copie valores de /tmp/old-appsettings.json para .env
   nano .env
   ```

3. **Testar**:
   ```bash
   cd src/BarbApp.API
   dotnet run
   # Deve exibir: "✓ Applied 16 environment variable overrides"
   ```

### Para CI/CD (GitHub Actions)

Configure secrets no GitHub:

**Settings → Secrets and variables → Actions → New repository secret**

```yaml
# .github/workflows/build.yml
jobs:
  build:
    runs-on: ubuntu-latest
    env:
      DB_CONNECTION_STRING: ${{ secrets.DB_CONNECTION_STRING }}
      JWT_SECRET: ${{ secrets.JWT_SECRET }}
      JWT_ISSUER: BarbApp-CI
      JWT_AUDIENCE: BarbApp-CI-Users
      FRONTEND_URL: ${{ secrets.FRONTEND_URL }}
      SMTP_HOST: ${{ secrets.SMTP_HOST }}
      SMTP_USERNAME: ${{ secrets.SMTP_USERNAME }}
      SMTP_PASSWORD: ${{ secrets.SMTP_PASSWORD }}
      SMTP_FROM_EMAIL: ${{ secrets.SMTP_FROM_EMAIL }}
      SMTP_FROM_NAME: BarbApp CI
      SENTRY_DSN: ${{ secrets.SENTRY_DSN }}
      SENTRY_ENVIRONMENT: CI
      SENTRY_RELEASE: barbapp-api-${{ github.sha }}
      R2_ENDPOINT: ${{ secrets.R2_ENDPOINT }}
      R2_BUCKET_NAME: ${{ secrets.R2_BUCKET_NAME }}
      R2_PUBLIC_URL: ${{ secrets.R2_PUBLIC_URL }}
      R2_ACCESS_KEY_ID: ${{ secrets.R2_ACCESS_KEY_ID }}
      R2_SECRET_ACCESS_KEY: ${{ secrets.R2_SECRET_ACCESS_KEY }}
    
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: 8.0.x
      - name: Build
        run: |
          cd backend/src/BarbApp.API
          dotnet build
```

### Para Docker Swarm / Kubernetes

Use secrets management do orquestrador:

**Docker Swarm:**
```bash
# Criar secrets
echo "Host=postgres;Port=5432;..." | docker secret create db_connection_string -
echo "super-secret-key-32-chars-min" | docker secret create jwt_secret -

# docker-compose.yml
services:
  api:
    image: barbapp-api
    secrets:
      - db_connection_string
      - jwt_secret
    environment:
      DB_CONNECTION_STRING_FILE: /run/secrets/db_connection_string
      JWT_SECRET_FILE: /run/secrets/jwt_secret
```

**Kubernetes:**
```yaml
apiVersion: v1
kind: Secret
metadata:
  name: barbapp-secrets
type: Opaque
stringData:
  db-connection-string: "Host=postgres;Port=5432;..."
  jwt-secret: "super-secret-key-32-chars-min"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: barbapp-api
spec:
  template:
    spec:
      containers:
      - name: api
        image: barbapp-api
        env:
        - name: DB_CONNECTION_STRING
          valueFrom:
            secretKeyRef:
              name: barbapp-secrets
              key: db-connection-string
        - name: JWT_SECRET
          valueFrom:
            secretKeyRef:
              name: barbapp-secrets
              key: jwt-secret
```

## Testing

### Unit Tests

Os testes unitários não precisam de `.env` porque usam configuração in-memory:

```csharp
// Program.cs
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["JwtSettings:Secret"] = "test-secret-key-at-least-32-characters-long-for-jwt",
        ["JwtSettings:Issuer"] = "BarbApp-Test",
        ["JwtSettings:Audience"] = "BarbApp-Test-Users",
        ["JwtSettings:ExpirationMinutes"] = "60",
        ["Sentry:Dsn"] = string.Empty
    }!);
}
```

### Integration Tests

Para testes de integração, crie `backend/.env.test`:

```bash
# backend/.env.test
DB_CONNECTION_STRING=Host=localhost;Port=5433;Database=barbapp_test;Username=test;Password=test
JWT_SECRET=test-secret-key-at-least-32-chars-long
JWT_ISSUER=BarbApp-Test
JWT_AUDIENCE=BarbApp-Test-Users
FRONTEND_URL=http://localhost:5173
# ... outras variáveis
```

E carregue no setup do teste:

```csharp
[SetUp]
public void Setup()
{
    var envPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", ".env.test");
    Env.Load(envPath);
}
```

## References

- [12-Factor App - Config](https://12factor.net/config)
- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [DotNetEnv Library](https://github.com/tonerdo/dotnet-env)
- [Cloudflare R2 Setup](cloudflare-r2-setup.md)
- [GitHub Secrets Documentation](https://docs.github.com/en/actions/security-guides/encrypted-secrets)

## Changelog

### 2024-01-XX - Comprehensive .env Migration (commit: 556d720)

**BREAKING CHANGE**: All configuration now loaded from environment variables

- ✅ Migrated ALL configuration to .env pattern (16 variables)
- ✅ Updated appsettings.Development.json with ${VAR} placeholders
- ✅ Updated Program.cs to load and override configuration
- ✅ Created comprehensive .env.example template
- ✅ Removed appsettings.Development.json from .gitignore
- ✅ Zero hardcoded secrets in appsettings files

**Variables migrated:**
- Database: `DB_CONNECTION_STRING`
- JWT: `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`
- Frontend: `FRONTEND_URL`
- SMTP: `SMTP_HOST`, `SMTP_USERNAME`, `SMTP_PASSWORD`, `SMTP_FROM_EMAIL`, `SMTP_FROM_NAME`
- Sentry: `SENTRY_DSN`, `SENTRY_ENVIRONMENT`, `SENTRY_RELEASE`
- R2: `R2_ENDPOINT`, `R2_BUCKET_NAME`, `R2_PUBLIC_URL`, `R2_ACCESS_KEY_ID`, `R2_SECRET_ACCESS_KEY`

### 2024-01-XX - Initial .env Support (commit: 28f61fb)

- Added DotNetEnv package
- Implemented .env loading in Program.cs
- Created basic .env.example
- Documented R2 variables

### 2024-01-XX - R2 Migration (commit: f29fe6a)

- Migrated logo upload to Cloudflare R2
- Added R2 configuration section
- Implemented R2StorageService
