# 🐳 Exemplo Completo - Docker com Variáveis Runtime

Este exemplo demonstra como usar o sistema de variáveis de ambiente em tempo de execução.

## 📋 Cenário

Você tem o mesmo frontend que precisa se conectar a diferentes APIs:
- **Desenvolvimento**: `http://localhost:5070/api`
- **Staging**: `https://api-staging.barbapp.com/api`
- **Produção**: `https://api.barbapp.com/api`

## 🚀 Como Funciona

### 1. Durante o Build
```typescript
// src/config/env.ts
export const env = {
  API_URL: import.meta.env.VITE_API_URL || 'http://localhost:5070/api',
  APP_NAME: import.meta.env.VITE_APP_NAME || 'BarbApp Admin',
};
```

No build, isso vira:
```javascript
// dist/assets/index-abc123.js (simplificado)
const env = {
  API_URL: "__VITE_API_URL__",
  APP_NAME: "__VITE_APP_NAME__"
};
```

### 2. Durante o Runtime (Docker)
O script `docker-entrypoint.sh` substitui os placeholders:

```bash
# Antes
API_URL: "__VITE_API_URL__"

# Depois (com VITE_API_URL=https://api.barbapp.com/api)
API_URL: "https://api.barbapp.com/api"
```

## 💻 Exemplos Práticos

### Desenvolvimento Local
```bash
# Build e run para desenvolvimento
./build-and-run.sh dev

# Ou manualmente
docker run -p 3000:80 \
  -e VITE_API_URL=http://localhost:5070/api \
  -e VITE_APP_NAME="BarbApp Admin (Dev)" \
  barbapp-admin
```

### Staging
```bash
# Para staging
API_URL=https://api-staging.barbapp.com/api \
APP_NAME="BarbApp Admin (Staging)" \
./build-and-run.sh prod

# Ou com docker-compose
VITE_API_URL=https://api-staging.barbapp.com/api \
VITE_APP_NAME="BarbApp Admin (Staging)" \
docker-compose up -d
```

### Produção
```bash
# Para produção
API_URL=https://api.barbapp.com/api \
./build-and-run.sh prod

# Ou definindo no arquivo .env.docker
echo "VITE_API_URL=https://api.barbapp.com/api" > .env.docker
echo "VITE_APP_NAME=BarbApp Admin" >> .env.docker
docker-compose --env-file .env.docker up -d
```

## 🔍 Verificação

### 1. Verificar se as variáveis foram aplicadas
```bash
# Ver logs do container durante inicialização
docker logs barbapp-admin-container

# Verificar conteúdo processado
docker exec barbapp-admin-container \
  cat /usr/share/nginx/html/assets/*.js | \
  grep -o 'https\?://[^"]*api'
```

### 2. Testar a aplicação
```bash
# Abrir no navegador
open http://localhost:3000

# Verificar no DevTools (Console)
# Deve mostrar: "✅ Configurações de ambiente validadas"
```

### 3. Health check
```bash
curl http://localhost:3000/health
# Resposta: healthy
```

## 🐛 Troubleshooting

### Problema: Variáveis não foram substituídas
```bash
# Verificar se o container está usando as variáveis corretas
docker inspect barbapp-admin-container | grep -A 10 "Env"

# Verificar logs de inicialização
docker logs barbapp-admin-container | grep "Substituindo"
```

### Problema: API não conecta
```bash
# Verificar se a URL foi aplicada corretamente
docker exec barbapp-admin-container \
  grep -r "api\." /usr/share/nginx/html/assets/

# Testar conectividade da API
curl -f https://api.barbapp.com/api/health
```

### Problema: Container não inicia
```bash
# Verificar logs detalhados
docker logs barbapp-admin-container --details

# Verificar se o script tem permissão
docker exec barbapp-admin-container ls -la /docker-entrypoint.sh
```

## 📊 Performance

### Métricas da Imagem
```bash
# Tamanho da imagem
docker images barbapp-admin

# Layers da imagem
docker history barbapp-admin

# Tempo de startup
time docker run --rm barbapp-admin nginx -t
```

### Comparação de Abordagens

| Método | Pros | Contras | Uso |
|--------|------|---------|-----|
| **Build-time** | Mais rápido runtime | Imagem por ambiente | CI/CD simples |
| **Runtime** | Uma imagem, N ambientes | Startup ligeiramente mais lento | Deploy flexível |
| **ConfigMap/Secrets** | Kubernetes nativo | Mais complexo | Orquestração K8s |

## 🚀 Deploy em Produção

### Docker Swarm
```bash
docker service create \
  --name barbapp-admin \
  --publish 3000:80 \
  --env VITE_API_URL=https://api.barbapp.com/api \
  --env VITE_APP_NAME="BarbApp Admin" \
  --replicas 3 \
  barbapp-admin
```

### Kubernetes
```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: barbapp-admin-config
data:
  VITE_API_URL: "https://api.barbapp.com/api"
  VITE_APP_NAME: "BarbApp Admin"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: barbapp-admin
spec:
  replicas: 3
  selector:
    matchLabels:
      app: barbapp-admin
  template:
    metadata:
      labels:
        app: barbapp-admin
    spec:
      containers:
      - name: barbapp-admin
        image: barbapp-admin:latest
        ports:
        - containerPort: 80
        envFrom:
        - configMapRef:
            name: barbapp-admin-config
        resources:
          requests:
            memory: "64Mi"
            cpu: "50m"
          limits:
            memory: "128Mi"
            cpu: "100m"
```

### Docker Compose (Produção)
```yaml
version: '3.8'
services:
  barbapp-admin:
    image: barbapp-admin:latest
    ports:
      - "80:80"
    environment:
      - VITE_API_URL=https://api.barbapp.com/api
      - VITE_APP_NAME=BarbApp Admin
    restart: unless-stopped
    deploy:
      replicas: 2
      resources:
        limits:
          memory: 128M
        reservations:
          memory: 64M
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
```

## 🎯 Próximos Passos

1. **Automatização**: Integrar no pipeline CI/CD
2. **Monitoramento**: Adicionar métricas de health
3. **Segurança**: Implementar HTTPS e headers de segurança
4. **Performance**: Otimizar cache e compressão
5. **Observabilidade**: Adicionar logs estruturados