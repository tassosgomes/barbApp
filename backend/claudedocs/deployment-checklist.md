# Checklist de Deploy - BarbApp API
**Tarefa 14.0 - Validação End-to-End e Ajustes Finais**
**Data**: 2025-10-12

## Preparação Pré-Deploy

### Banco de Dados
- [ ] PostgreSQL 16+ instalado e rodando
- [ ] Criar usuário e banco de dados de produção
```sql
CREATE USER barbapp_prod WITH PASSWORD '<senha-forte>';
CREATE DATABASE barbapp_prod OWNER barbapp_prod;
GRANT ALL PRIVILEGES ON DATABASE barbapp_prod TO barbapp_prod;
```
- [ ] Backup strategy definida (pg_dump automatizado)
- [ ] Rollback plan documentado
- [ ] Aplicar migrations: `dotnet ef database update`

### Configurações de Ambiente

#### Variáveis de Ambiente Obrigatórias
```bash
# Database
ConnectionStrings__DefaultConnection="Host=<host>;Port=5432;Database=barbapp_prod;Username=barbapp_prod;Password=<senha>"

# JWT Settings
JwtSettings__Secret="<secret-key-forte-com-minimo-32-caracteres>"
JwtSettings__Issuer="BarbApp-Production"
JwtSettings__Audience="BarbApp-Production-Users"
JwtSettings__ExpirationMinutes="1440"  # 24 horas

# Logging
ASPNETCORE_ENVIRONMENT="Production"
Serilog__MinimumLevel__Default="Information"

# CORS
AllowedOrigins="https://app.barbapp.com,https://www.barbapp.com"
```

#### Gerar Secret Key Forte
```bash
# Gerar secret JWT de 64 caracteres
openssl rand -base64 48
```

- [ ] Todas as variáveis de ambiente configuradas
- [ ] Secrets armazenados de forma segura (não em código)
- [ ] JWT secret gerado aleatoriamente (mínimo 32 caracteres)

### Segurança

- [ ] HTTPS habilitado (certificado SSL válido)
- [ ] CORS configurado apenas para domínios de produção
- [ ] Firewall configurado (apenas portas necessárias abertas)
- [ ] Logs não contêm dados sensíveis
- [ ] Rate limiting considerado (próxima fase)

### Infraestrutura

- [ ] Servidor de produção provisionado
- [ ] .NET 8 Runtime instalado
- [ ] Docker instalado (se usando containers)
- [ ] Reverse proxy configurado (Nginx/Apache)
- [ ] Load balancer configurado (se multi-instância)

### Monitoring & Logging

- [ ] Logging configurado (Serilog)
- [ ] Error tracking configurado (Sentry/AppInsights opcional)
- [ ] Health checks funcionando: `/health`
- [ ] Monitoramento de banco de dados ativo
- [ ] Alertas configurados para erros críticos

## Build & Deployment

### Build de Produção

```bash
# Navegar para o diretório do backend
cd backend

# Restaurar dependências
dotnet restore

# Build em Release
dotnet build --configuration Release

# Publicar aplicação
dotnet publish src/BarbApp.API/BarbApp.API.csproj \
    --configuration Release \
    --output ./publish \
    --self-contained false

# Verificar artefatos de publicação
ls -la ./publish
```

- [ ] Build de Release executado com sucesso
- [ ] Artefatos de publicação gerados
- [ ] Tamanho do artefato verificado

### Testes Pré-Deploy

```bash
# Executar suite completa de testes
dotnet test --configuration Release

# Verificar vulnerabilidades de segurança
dotnet list package --vulnerable --include-transitive
```

- [ ] Todos os 203 testes passando
- [ ] Nenhuma vulnerabilidade de segurança detectada
- [ ] Build sem warnings ou erros

### Deploy

**Opção 1: Deploy Manual**
```bash
# Copiar artefatos para servidor
scp -r ./publish user@server:/var/www/barbapp/

# No servidor
cd /var/www/barbapp/publish
dotnet BarbApp.API.dll
```

**Opção 2: Docker**
```bash
# Build da imagem Docker
docker build -t barbapp-api:latest -f src/BarbApp.API/Dockerfile .

# Push para registry
docker tag barbapp-api:latest registry.example.com/barbapp-api:latest
docker push registry.example.com/barbapp-api:latest

# Deploy no servidor
docker pull registry.example.com/barbapp-api:latest
docker-compose up -d
```

- [ ] Artefatos copiados para servidor de produção
- [ ] Aplicação iniciada com sucesso
- [ ] Logs de inicialização verificados (sem erros)

### Verificação Pós-Deploy

```bash
# Verificar se API está respondendo
curl https://api.barbapp.com/health

# Verificar Swagger (apenas se habilitado em produção)
curl https://api.barbapp.com/swagger/v1/swagger.json

# Testar endpoint de autenticação
curl -X POST https://api.barbapp.com/api/auth/admin-central \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@barbapp.com","password":"<senha>"}'
```

- [ ] Health check retorna 200 OK
- [ ] Swagger acessível (se habilitado)
- [ ] Autenticação funciona corretamente
- [ ] Database conectado com sucesso

### Smoke Tests

- [ ] Login Admin Central funciona
- [ ] Login Admin Barbearia funciona
- [ ] Login Barbeiro funciona
- [ ] Login Cliente funciona
- [ ] Isolamento multi-tenant validado
- [ ] JWT tokens sendo gerados corretamente

## Configurações de Produção Específicas

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Será sobrescrito por variável de ambiente"
  },
  "JwtSettings": {
    "Secret": "Será sobrescrito por variável de ambiente",
    "Issuer": "BarbApp-Production",
    "Audience": "BarbApp-Production-Users",
    "ExpirationMinutes": 1440
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] appsettings.Production.json configurado
- [ ] Variáveis sensíveis não hardcoded
- [ ] Logging level apropriado para produção

### Program.cs - Configurações de Produção

- [ ] Detailed errors desabilitado em produção
- [ ] Developer exception page desabilitado
- [ ] HTTPS redirection habilitado
- [ ] HSTS habilitado
- [ ] Security headers configurados

## Rollback Plan

### Em caso de falha no deploy:

1. **Parar a aplicação nova**
```bash
# Docker
docker-compose down

# Manual
pkill -f "dotnet BarbApp.API"
```

2. **Reverter para versão anterior**
```bash
# Docker
docker-compose up -d barbapp-api:previous-tag

# Manual
cd /var/www/barbapp/previous
dotnet BarbApp.API.dll
```

3. **Reverter migrations do banco (se necessário)**
```bash
dotnet ef database update <migration-anterior>
```

4. **Verificar rollback**
```bash
curl https://api.barbapp.com/health
```

- [ ] Plan de rollback documentado
- [ ] Backup da versão anterior mantido
- [ ] Procedimento de rollback testado

## Documentação

- [ ] README.md atualizado com instruções de deploy
- [ ] Swagger acessível (se habilitado em produção)
- [ ] Postman collection disponível
- [ ] Variáveis de ambiente documentadas
- [ ] Troubleshooting guide preparado

## Comunicação

- [ ] Stakeholders notificados sobre deploy
- [ ] Janela de manutenção comunicada (se aplicável)
- [ ] Documentação de release notes preparada
- [ ] Support team briefado sobre mudanças

## Pós-Deploy (Primeiras 24 horas)

### Monitoramento Intensivo

- [ ] Monitorar logs de erro a cada hora
- [ ] Verificar métricas de performance
- [ ] Verificar taxa de sucesso de autenticação
- [ ] Monitorar uso de recursos (CPU, memória, disco)
- [ ] Verificar conexões de banco de dados

### Métricas de Sucesso

- [ ] Taxa de erro < 1%
- [ ] Tempo de resposta de API < 500ms (p95)
- [ ] Taxa de sucesso de autenticação > 95%
- [ ] Zero incidentes críticos de segurança
- [ ] Zero vazamento de dados entre tenants

## Checklist de Segurança Final

- [ ] Nenhuma credencial hardcoded no código
- [ ] JWT secret forte (>32 caracteres, aleatório)
- [ ] BCrypt password hashing configurado (work factor 12)
- [ ] HTTPS obrigatório
- [ ] CORS restrito a domínios de produção
- [ ] SQL injection prevention (EF Core parametrizado)
- [ ] XSS prevention (input validation)
- [ ] Rate limiting planejado (próxima fase)
- [ ] Logs não contêm senhas ou tokens completos
- [ ] Vulnerabilidades de pacotes corrigidas

## Assinatura

- [ ] Deploy revisado por: ______________________
- [ ] Deploy aprovado por: ______________________
- [ ] Data do deploy: ______________________
- [ ] Versão deployada: ______________________
- [ ] Próxima revisão agendada para: ______________________

---

## Comandos Úteis de Troubleshooting

### Verificar logs da aplicação
```bash
# Docker
docker logs barbapp-api -f --tail 100

# Manual (Serilog file)
tail -f /var/log/barbapp/log.txt
```

### Verificar status do banco de dados
```bash
psql -U barbapp_prod -d barbapp_prod -c "SELECT version();"
psql -U barbapp_prod -d barbapp_prod -c "SELECT * FROM __EFMigrationsHistory ORDER BY migration_id DESC LIMIT 5;"
```

### Verificar recursos do sistema
```bash
# CPU e Memória
top -b -n 1 | grep -E "(dotnet|postgres)"

# Disco
df -h
```

### Testar conectividade do banco
```bash
telnet <postgres-host> 5432
```

## Contatos de Emergência

- **DevOps Lead**: [Nome] - [Email] - [Telefone]
- **Backend Lead**: [Nome] - [Email] - [Telefone]
- **DBA**: [Nome] - [Email] - [Telefone]
- **Support On-call**: [Telefone]

---

**Versão do Checklist**: 1.0
**Última Atualização**: 2025-10-12
**Próxima Revisão**: Após primeiro deploy em produção
