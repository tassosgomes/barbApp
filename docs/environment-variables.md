# Variáveis de Ambiente - BarbApp

Este documento descreve todas as variáveis de ambiente necessárias para executar o projeto BarbApp.

## Configuração JWT

### Jwt:SecretKey
- **Tipo**: String (Base64)
- **Obrigatório**: Sim
- **Descrição**: Chave secreta usada para assinar e validar tokens JWT
- **Requisitos**: Mínimo 32 caracteres (44+ em Base64)
- **Exemplo**: `FTVBUj5qEbI03He2NGDJjxZRsxvxmRwCr7EYZIINZSA=`
- **Como gerar**:
  ```bash
  # Linux/Mac
  openssl rand -base64 32
  
  # PowerShell
  [Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
  ```

### Jwt:Issuer
- **Tipo**: String
- **Obrigatório**: Sim
- **Descrição**: Emissor dos tokens JWT
- **Valor Padrão**: `barbapp`
- **Exemplo**: `barbapp`

### Jwt:Audience
- **Tipo**: String
- **Obrigatório**: Sim
- **Descrição**: Audiência dos tokens JWT (quem pode consumir)
- **Valor Padrão**: `barbapp-api`
- **Exemplo**: `barbapp-api`

### Jwt:ExpirationHours
- **Tipo**: Number
- **Obrigatório**: Sim
- **Descrição**: Tempo de expiração dos tokens em horas
- **Valor Padrão**: `24`
- **Exemplo**: `24`

## Configuração da Aplicação

### AppSettings:FrontendUrl
- **Tipo**: String (URL)
- **Obrigatório**: Sim
- **Descrição**: URL base do frontend para construção de links em e-mails de boas-vindas
- **Formato**: `https://seudominio.com` ou `http://localhost:3000` (desenvolvimento)
- **Exemplo Produção**: `https://barbapp.com`
- **Exemplo Desenvolvimento**: `http://localhost:3000`

## Configuração de Banco de Dados

### ConnectionStrings:DefaultConnection
- **Tipo**: String (Connection String)
- **Obrigatório**: Sim
- **Descrição**: String de conexão com o banco de dados PostgreSQL
- **Formato**: `Host={host};Database={database};Username={username};Password={password}`
- **Exemplo Desenvolvimento**: `Host=localhost;Database=barbapp;Username=postgres;Password=postgres`
- **Exemplo Produção**: `Host=postgres.production.com;Database=barbapp_prod;Username=barbapp_user;Password=secure_password_here`

## Arquivos de Configuração

### appsettings.json
Arquivo de configuração base commitado no repositório. **NÃO** deve conter valores sensíveis (senhas, secret keys).

```json
{
  "Jwt": {
    "SecretKey": "",
    "Issuer": "barbapp",
    "Audience": "barbapp-api",
    "ExpirationHours": 24
  },
  "AppSettings": {
    "FrontendUrl": ""
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=barbapp;Username=postgres;Password=postgres"
  }
}
```

### appsettings.Development.json
Arquivo de configuração para ambiente de desenvolvimento. **NÃO** deve ser commitado (incluído no .gitignore).

```json
{
  "Jwt": {
    "SecretKey": "FTVBUj5qEbI03He2NGDJjxZRsxvxmRwCr7EYZIINZSA=",
    "Issuer": "barbapp",
    "Audience": "barbapp-api",
    "ExpirationHours": 24
  },
  "AppSettings": {
    "FrontendUrl": "http://localhost:3000"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=barbapp;Username=postgres;Password=postgres"
  }
}
```

### appsettings.Production.json
Arquivo de configuração para ambiente de produção. **NÃO** deve ser commitado (incluído no .gitignore).

```json
{
  "Jwt": {
    "SecretKey": "{{USE_ENVIRONMENT_VARIABLE_OR_SECRET_MANAGER}}",
    "Issuer": "barbapp",
    "Audience": "barbapp-api",
    "ExpirationHours": 24
  },
  "AppSettings": {
    "FrontendUrl": "{{USE_ENVIRONMENT_VARIABLE_OR_SECRET_MANAGER}}"
  },
  "ConnectionStrings": {
    "DefaultConnection": "{{USE_ENVIRONMENT_VARIABLE_OR_SECRET_MANAGER}}"
  }
}
```

## Variáveis de Ambiente em Produção

Em produção, recomenda-se usar um serviço de gerenciamento de secrets (Azure Key Vault, AWS Secrets Manager, etc.) ou variáveis de ambiente ao invés de arquivos de configuração.

### Exemplo com Docker

```dockerfile
ENV JWT__SECRETKEY="sua-secret-key-aqui"
ENV JWT__ISSUER="barbapp"
ENV JWT__AUDIENCE="barbapp-api"
ENV JWT__EXPIRATIONHOURS="24"
ENV APPSETTINGS__FRONTENDURL="https://barbapp.com"
ENV CONNECTIONSTRINGS__DEFAULTCONNECTION="Host=postgres;Database=barbapp_prod;Username=barbapp_user;Password=secure_password"
```

### Exemplo com Azure App Service

Configurar Application Settings:
- `Jwt:SecretKey` = `sua-secret-key-aqui`
- `Jwt:Issuer` = `barbapp`
- `Jwt:Audience` = `barbapp-api`
- `Jwt:ExpirationHours` = `24`
- `AppSettings:FrontendUrl` = `https://barbapp.com`
- `ConnectionStrings:DefaultConnection` = `Host=...`

## Checklist de Segurança

- [ ] `Jwt:SecretKey` tem no mínimo 32 caracteres
- [ ] `Jwt:SecretKey` é diferente entre ambientes (dev, staging, prod)
- [ ] `AppSettings:FrontendUrl` é uma URL válida e segura (HTTPS em produção)
- [ ] `appsettings.Development.json` está no .gitignore
- [ ] `appsettings.Production.json` está no .gitignore
- [ ] Senhas de banco de dados são fortes e únicas por ambiente
- [ ] Credenciais de produção são armazenadas em gerenciador de secrets
- [ ] Documentação de secrets de produção está disponível apenas para equipe autorizada

## Troubleshooting

### Erro: "Unable to obtain configuration value for: Jwt:SecretKey"
- **Causa**: `Jwt:SecretKey` não está configurado
- **Solução**: Adicionar a chave no `appsettings.Development.json` ou variável de ambiente

### Erro: "IDX10503: Signature validation failed. Keys tried..."
- **Causa**: Secret key incorreta ou formato inválido
- **Solução**: Verificar se a secret key é a mesma usada na geração e validação do token

### Erro: "The Connection String property has not been initialized"
- **Causa**: Connection string não está configurada
- **Solução**: Adicionar a connection string no arquivo de configuração apropriado

### Erro: "FrontendUrl is not configured" ou links de email quebrados
- **Causa**: `AppSettings:FrontendUrl` não está configurado
- **Solução**: Adicionar a URL base do frontend no `appsettings.json` ou variável de ambiente `APPSETTINGS__FRONTENDURL`

## Variáveis de Ambiente - Frontend (barbapp-admin)

O frontend utiliza Vite como build tool, que expõe variáveis de ambiente com o prefixo `VITE_`.

### VITE_API_URL
- **Tipo**: String (URL)
- **Obrigatório**: Sim
- **Descrição**: URL base da API backend para requisições HTTP
- **Formato**: `http://localhost:5000/api` (desenvolvimento) ou `https://api.barbapp.com` (produção)
- **Exemplo Desenvolvimento**: `http://localhost:5000/api`
- **Exemplo Produção**: `https://api.barbapp.com`

### VITE_APP_NAME
- **Tipo**: String
- **Obrigatório**: Não
- **Descrição**: Nome da aplicação exibido na interface
- **Valor Padrão**: `BarbApp Admin`
- **Exemplo**: `BarbApp Admin`

### Arquivo .env (Frontend)

Crie um arquivo `.env.local` na raiz do projeto `barbapp-admin`:

```env
# API Configuration
VITE_API_URL=http://localhost:5000/api

# App Configuration
VITE_APP_NAME=BarbApp Admin
```

### Configuração por Ambiente

**.env.development** (commitado)
```env
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=BarbApp Admin [DEV]
```

**.env.production** (não commitado)
```env
VITE_API_URL=https://api.barbapp.com
VITE_APP_NAME=BarbApp Admin
```

**.env.local** (não commitado, sobrescreve outros arquivos)
```env
# Usado para desenvolvimento local, sobrescreve .env.development
VITE_API_URL=http://localhost:5000/api
```

### Multi-Tenancy e Roteamento

O frontend do Admin Barbearia utiliza roteamento baseado em tenant (código da barbearia):

**Estrutura de URLs:**
```
Admin Central:
  http://localhost:3001/admin-central/login
  http://localhost:3001/admin-central/barbearias

Admin Barbearia (Multi-tenant):
  http://localhost:3001/{codigo-barbearia}/login
  http://localhost:3001/{codigo-barbearia}/dashboard
  http://localhost:3001/{codigo-barbearia}/barbeiros
  http://localhost:3001/{codigo-barbearia}/servicos
  http://localhost:3001/{codigo-barbearia}/agenda
```

**Exemplo de Tenants:**
```
http://localhost:3001/ABC123/login
http://localhost:3001/BARBER2024/dashboard
http://localhost:3001/TEST1234/agenda
```

O código da barbearia é extraído da URL e usado para:
1. Validar existência e status da barbearia (API: `GET /barbershops/{code}/info`)
2. Identificar o tenant nas requisições subsequentes
3. Isolar dados entre barbearias diferentes

### Variáveis Internas do Frontend

Além das variáveis de ambiente, o frontend gerencia:

**LocalStorage:**
- `adminCentral_token`: Token JWT do Admin Central
- `adminBarbearia_token`: Token JWT do Admin Barbearia
- `adminBarbearia_barbeariaId`: ID da barbearia autenticada
- `adminBarbearia_codigo`: Código da barbearia

**TenantContext:**
O React Context gerencia informações do tenant atual:
```typescript
interface TenantContextType {
  codigo: string;
  barbeariaId: string;
  barbeariaNome: string;
  isLoading: boolean;
}
```

## Referências

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8725)
- [Secret Storage in Production](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Vite Environment Variables](https://vitejs.dev/guide/env-and-mode.html)
