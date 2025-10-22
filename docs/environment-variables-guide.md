# Variáveis de Ambiente (.env) - Guia de Configuração

## 📋 Visão Geral

O backend do BarbApp usa variáveis de ambiente para configuração sensível e específica do ambiente. Este documento explica como configurar e usar o arquivo `.env`.

## 🔧 Configuração

### 1. Criar arquivo .env

Copie o arquivo de exemplo:

```bash
cd backend
cp .env.example .env
```

### 2. Editar credenciais

Abra o arquivo `.env` e preencha as variáveis:

```bash
# Cloudflare R2 (obrigatório para upload de logos)
R2_ENDPOINT=https://YOUR_ACCOUNT_ID.r2.cloudflarestorage.com
R2_BUCKET_NAME=barbapp-assets
R2_PUBLIC_URL=https://pub-HASH.r2.dev
R2_ACCESS_KEY_ID=your_access_key_id
R2_SECRET_ACCESS_KEY=your_secret_access_key

# Sentry (opcional - tracking de erros)
SENTRY_DSN=your_sentry_dsn

# SMTP (opcional - envio de emails)
SMTP_USERNAME=your_smtp_user
SMTP_PASSWORD=your_smtp_password
```

## 📁 Estrutura de Arquivos

```
backend/
├── .env                 # Configuração local (NÃO commitado)
├── .env.example         # Template de exemplo (commitado)
└── src/
    └── BarbApp.API/
        ├── appsettings.json            # Config produção com ${VAR}
        └── appsettings.Development.json # Config dev com ${VAR}
```

## 🔐 Segurança

### ✅ O que é SEGURO

- Usar `.env` para desenvolvimento local
- Commitar `.env.example` com valores placeholder
- Usar variáveis de ambiente no container/servidor

### ❌ O que NÃO fazer

- ❌ Nunca commitar `.env` (já está no .gitignore)
- ❌ Nunca colocar credenciais reais no appsettings.json
- ❌ Nunca compartilhar `.env` em chat/email

## 🚀 Como Funciona

### Ordem de Carregamento

1. **Program.cs** carrega `.env` no startup:
   ```csharp
   Env.Load(envPath);
   ```

2. **appsettings.json** usa placeholders:
   ```json
   {
     "R2Storage": {
       "Endpoint": "${R2_ENDPOINT}",
       "AccessKeyId": "${R2_ACCESS_KEY_ID}"
     }
   }
   ```

3. **Função Get()** substitui placeholders por variáveis de ambiente:
   ```csharp
   var value = Environment.GetEnvironmentVariable("R2_ENDPOINT");
   ```

### Exemplo Completo

**backend/.env:**
```bash
R2_ENDPOINT=https://abc123.r2.cloudflarestorage.com
R2_ACCESS_KEY_ID=my_key_id
```

**appsettings.json:**
```json
{
  "R2Storage": {
    "Endpoint": "${R2_ENDPOINT}",
    "AccessKeyId": "${R2_ACCESS_KEY_ID}"
  }
}
```

**Em Runtime:**
- `${R2_ENDPOINT}` → `https://abc123.r2.cloudflarestorage.com`
- `${R2_ACCESS_KEY_ID}` → `my_key_id`

## 🐳 Docker/Produção

### Docker Compose

```yaml
services:
  backend:
    image: barbapp-backend
    env_file:
      - .env  # Carrega variáveis do .env
    environment:
      # Ou definir inline:
      R2_ENDPOINT: "https://..."
      R2_ACCESS_KEY_ID: "..."
```

### Kubernetes

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: barbapp-secrets
type: Opaque
stringData:
  R2_ENDPOINT: "https://..."
  R2_ACCESS_KEY_ID: "..."
```

### Variáveis de Ambiente do Sistema

```bash
export R2_ENDPOINT="https://..."
export R2_ACCESS_KEY_ID="..."
dotnet run
```

## 🧪 Verificar Carregamento

Ao iniciar o backend, você verá:

```
✓ Loaded .env from: /path/to/backend/.env
```

Ou se não encontrar:

```
⚠ .env file not found at: /path/to/backend/.env
  Environment variables will be loaded from system/container environment
```

## 📝 Variáveis Disponíveis

### Obrigatórias (R2)

| Variável | Descrição | Exemplo |
|----------|-----------|---------|
| `R2_ENDPOINT` | URL do R2 | `https://abc.r2.cloudflarestorage.com` |
| `R2_BUCKET_NAME` | Nome do bucket | `barbapp-assets` |
| `R2_PUBLIC_URL` | URL pública | `https://pub-xxx.r2.dev` |
| `R2_ACCESS_KEY_ID` | Access Key | `1cea5997...` |
| `R2_SECRET_ACCESS_KEY` | Secret Key | `9f5a7c4a...` |

### Opcionais

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `SENTRY_DSN` | Sentry error tracking | - |
| `SMTP_USERNAME` | SMTP user | - |
| `SMTP_PASSWORD` | SMTP password | - |

## 🔧 Troubleshooting

### Erro: "The request signature we calculated does not match"

- Verifique se `R2_ACCESS_KEY_ID` e `R2_SECRET_ACCESS_KEY` estão corretos
- Confirme que não há espaços extras nas variáveis

### Erro: ".env file not found"

- Verifique se `.env` está em `/backend/.env` (não em `/backend/src/BarbApp.API/.env`)
- O Program.cs procura 2 níveis acima: `../../.env`

### Variáveis não estão sendo substituídas

- Confirme que o appsettings.json usa o formato `${VARIAVEL}`
- Verifique se as variáveis estão definidas no `.env`
- Restart do backend após alterar `.env`

## 📚 Referências

- **DotNetEnv**: https://github.com/tonerdo/dotnet-env
- **ASP.NET Configuration**: https://learn.microsoft.com/aspnet/core/fundamentals/configuration
- **Cloudflare R2**: https://developers.cloudflare.com/r2

---

**Última atualização**: 2024-10-22  
**Mantido por**: BarbApp Team
