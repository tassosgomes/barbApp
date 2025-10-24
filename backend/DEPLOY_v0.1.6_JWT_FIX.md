# Deploy v0.1.6 - Fix JWT Authentication

## 🐛 Problema Identificado

Após login bem-sucedido no Admin Central, o usuário era redirecionado de volta para a tela de login devido a erro 401 Unauthorized na primeira chamada à API.

### Diagnóstico

1. **Login funcionava** ✅ - POST `/auth/admin-central/login` retornava 200 OK
2. **Token era armazenado** ✅ - TokenManager salvava o token
3. **Primeira chamada falhava** ❌ - GET `/barbearias` retornava 401 Unauthorized
4. **Logout automático** - Sistema deslogava o usuário

### Causa Raiz

O problema estava na configuração do JWT Authentication no backend:

1. **Configuração estava usando secret errado** - O `ServiceConfiguration.cs` tentava ler o `JWT_SECRET` diretamente da configuração ANTES do `InfisicalService` estar pronto
2. **Faltava configuração importante** - Não estava definido o `RoleClaimType` para mapear corretamente as roles do token
3. **Logs insuficientes** - Não havia logs detalhados para debug de claims

## 🔧 Correções Implementadas

### 1. ServiceConfiguration.cs

**Arquivo**: `/backend/src/BarbApp.API/Configuration/ServiceConfiguration.cs`

**Mudanças**:
- ✅ Adicionado carregamento do JWT Secret via `InfisicalService` ANTES de configurar authentication
- ✅ Adicionado `RoleClaimType = System.Security.Claims.ClaimTypes.Role` para garantir que as roles sejam reconhecidas
- ✅ Melhorado logging com detalhes de claims quando token é validado
- ✅ Adicionado logging no evento `OnChallenge` para debug
- ✅ Fallback para configuration se Infisical falhar

**Código Crítico**:
```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings.Issuer,
    ValidAudience = jwtSettings.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
    ClockSkew = TimeSpan.FromMinutes(5),
    RequireExpirationTime = true,
    RequireSignedTokens = true,
    RoleClaimType = System.Security.Claims.ClaimTypes.Role // 🔑 CRÍTICO!
};
```

## 📦 Nova Versão

- **Imagem Docker**: `tsgomes/barberapp-api:v0.1.6`
- **Data**: 2025-10-24
- **Build Status**: ✅ Sucesso
- **Push Status**: ✅ Sucesso

## 🚀 Instruções de Deploy

### Opção 1: Docker Swarm (Recomendado)

```bash
# 1. SSH no servidor
ssh usuario@seu-servidor

# 2. Ir para o diretório do projeto
cd /path/to/barbapp/backend

# 3. Atualizar docker-compose.yml (já atualizado localmente)
# Garantir que está usando: image: tsgomes/barberapp-api:v0.1.6

# 4. Pull da nova imagem
docker pull tsgomes/barberapp-api:v0.1.6

# 5. Deploy da stack
docker stack deploy -c docker-compose.yml barbapp

# 6. Verificar status
docker service ls
docker service ps barbapp_backend

# 7. Verificar logs
docker service logs -f barbapp_backend --tail 100
```

### Opção 2: Update Manual do Serviço

```bash
# Atualizar apenas o serviço backend
docker service update --image tsgomes/barberapp-api:v0.1.6 barbapp_backend

# Verificar rollout
docker service ps barbapp_backend
```

### Opção 3: Docker Compose (Standalone)

```bash
docker-compose pull backend
docker-compose up -d backend
docker-compose logs -f backend
```

## ✅ Verificação Pós-Deploy

### 1. Verificar se o serviço está rodando
```bash
docker service ps barbapp_backend
```

Espere até ver `Running` no estado.

### 2. Verificar logs de inicialização
```bash
docker service logs barbapp_backend --tail 50
```

Procure por:
- ✅ `JWT Secret loaded successfully from Infisical for authentication`
- ✅ `Starting BarbApp API`
- ✅ Sem erros de autenticação

### 3. Testar Login no Frontend

1. Acesse: `https://dev-admbarberapp.tasso.dev.br/admin/login`
2. Faça login com:
   - Email: `gomestasso@gmail.com`
   - Senha: `Neide@9090`
3. **Sucesso esperado**: Redirecionamento para dashboard com lista de barbearias

### 4. Verificar Logs do JWT

```bash
docker service logs barbapp_backend | grep -i "jwt\|token"
```

Deve mostrar:
```
JWT Secret loaded successfully from Infisical for authentication
JWT Token validated successfully. Claims: http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier=..., http://schemas.microsoft.com/ws/2008/06/identity/claims/role=AdminCentral, ...
```

## 🔍 Troubleshooting

### Problema: Ainda recebo 401 após login

**Verifique**:
```bash
# 1. Confirme que a nova versão está rodando
docker service ps barbapp_backend | head -5

# 2. Verifique os logs em tempo real
docker service logs -f barbapp_backend

# 3. Verifique as variáveis de ambiente
docker service inspect barbapp_backend --format='{{json .Spec.TaskTemplate.ContainerSpec.Env}}' | jq
```

**Verifique no browser**:
1. Abra DevTools (F12)
2. Vá para Network
3. Faça login
4. Verifique o header `Authorization` na chamada GET `/barbearias`
5. Deve conter: `Bearer eyJ...`

### Problema: JWT Secret não carrega do Infisical

**Sintoma**: Logs mostram `Failed to load JWT Secret from Infisical`

**Solução**:
1. Verificar variáveis de ambiente do Infisical:
   ```bash
   docker service inspect barbapp_backend | grep -i infisical
   ```
2. Garantir que as credenciais estão corretas no `.env`
3. Verificar conectividade com Infisical

### Problema: Claims não estão corretas

**Debug**:
```bash
# Ativar logs de debug (se necessário)
docker service update --env-add ASPNETCORE_ENVIRONMENT=Development barbapp_backend
```

Depois verificar logs que mostram claims completas.

## 📝 Checklist de Deploy

- [ ] Build local com sucesso
- [ ] Push para Docker Hub com sucesso
- [ ] docker-compose.yml atualizado com v0.1.6
- [ ] SSH no servidor de produção
- [ ] Pull da nova imagem
- [ ] Deploy da stack/serviço
- [ ] Verificar logs de inicialização
- [ ] Testar login no frontend
- [ ] Confirmar acesso ao dashboard
- [ ] Verificar que não há erros 401
- [ ] Monitorar por 5-10 minutos

## 🎯 Resultado Esperado

Após o deploy bem-sucedido:

1. ✅ Login do Admin Central funciona
2. ✅ Token é validado corretamente
3. ✅ Roles são reconhecidas pelo ASP.NET Core
4. ✅ Endpoint `/barbearias` retorna 200 OK com lista de barbearias
5. ✅ Usuário é redirecionado para dashboard
6. ✅ Sem logout automático

## 📊 Monitoramento

Após deploy, monitore por alguns minutos:

```bash
# Logs em tempo real
watch -n 2 'docker service ps barbapp_backend | head -10'

# Métricas de CPU/Memória
docker stats $(docker ps -q -f name=barbapp_backend)

# Health check
curl -I https://dev-api-barberapp.tasso.dev.br/health
```

## 🔄 Rollback (Se Necessário)

Se algo der errado:

```bash
# Voltar para versão anterior
docker service update --image tsgomes/barberapp-api:v0.1.5 barbapp_backend

# OU remover e redeployar
docker stack rm barbapp
# Aguardar 30 segundos
docker stack deploy -c docker-compose-backup.yml barbapp
```

## 📞 Suporte

Se precisar de ajuda:
1. Copie os logs: `docker service logs barbapp_backend > logs.txt`
2. Verifique se há erros específicos
3. Documente os passos reproduzidos
