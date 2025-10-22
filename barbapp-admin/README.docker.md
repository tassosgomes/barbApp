# BarbApp Admin - Docker Setup

Este diretório contém a configuração Docker para o frontend BarbApp Admin, permitindo a substituição de variáveis de ambiente em tempo de execução.

## 🚀 Características

- **Variáveis em tempo de execução**: As variáveis do Vite são substituídas quando o container inicia
- **Multi-stage build**: Otimizado para produção com imagem menor
- **Nginx otimizado**: Com compressão, cache e headers de segurança
- **Health check**: Endpoint `/health` para monitoramento
- **Scripts automatizados**: Para facilitar desenvolvimento e deploy

## 📁 Arquivos Docker

- `Dockerfile` - Configuração multi-stage da imagem
- `docker-entrypoint.sh` - Script que substitui variáveis em runtime
- `nginx.conf` - Configuração otimizada do Nginx
- `docker-compose.yml` - Orquestração do container
- `.dockerignore` - Arquivos ignorados no build
- `build-and-run.sh` - Script helper para build e execução

## 🛠️ Como Usar

### Opção 1: Script Automatizado (Recomendado)

```bash
# Desenvolvimento (API local)
./build-and-run.sh dev

# Produção
API_URL=https://api.barbapp.com/api ./build-and-run.sh prod

# Outras opções
./build-and-run.sh help
```

### Opção 2: Docker Compose

```bash
# Usando arquivo .env.docker
docker-compose --env-file .env.docker up -d

# Ou definindo variáveis inline
VITE_API_URL=http://localhost:5070/api docker-compose up -d
```

### Opção 3: Comandos Docker Manuais

```bash
# Build
docker build -t barbapp-admin .

# Run com variáveis customizadas
docker run -d \
  --name barbapp-admin-container \
  -p 3000:80 \
  -e VITE_API_URL=http://localhost:5070/api \
  -e VITE_APP_NAME="BarbApp Admin" \
  barbapp-admin
```

## ⚙️ Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|---------|
| `VITE_API_URL` | URL da API backend | `http://localhost:5070/api` |
| `VITE_APP_NAME` | Nome da aplicação | `BarbApp Admin` |

## 🌍 Ambientes

### Desenvolvimento
```bash
VITE_API_URL=http://localhost:5070/api
VITE_APP_NAME="BarbApp Admin (Dev)"
```

### Staging
```bash
VITE_API_URL=https://api-staging.barbapp.com/api
VITE_APP_NAME="BarbApp Admin (Staging)"
```

### Produção
```bash
VITE_API_URL=https://api.barbapp.com/api
VITE_APP_NAME="BarbApp Admin"
```

## 🔧 Como Funciona

1. **Build Time**: A aplicação é construída com placeholders (`__VITE_API_URL__`)
2. **Runtime**: O script `docker-entrypoint.sh` substitui os placeholders pelas variáveis reais
3. **Nginx**: Serve os arquivos processados com configurações otimizadas

### Fluxo de Substituição

```
Build: import.meta.env.VITE_API_URL → __VITE_API_URL__
Runtime: __VITE_API_URL__ → valor real da variável
```

## 📊 Health Check

O container inclui um endpoint de health check:

```bash
curl http://localhost:3000/health
# Resposta: healthy
```

## 🛡️ Segurança

- Headers de segurança configurados no Nginx
- Arquivos sensíveis ignorados via `.dockerignore`
- Processo não-root no container

## 🐛 Troubleshooting

### Container não substitui variáveis
```bash
# Verificar logs do container
docker logs barbapp-admin-container

# Verificar se as variáveis estão definidas
docker exec barbapp-admin-container env | grep VITE
```

### Problemas de conectividade com API
```bash
# Verificar se a API URL está correta
docker exec barbapp-admin-container cat /usr/share/nginx/html/assets/*.js | grep -o 'http[s]*://[^"]*api'
```

### Port já está em uso
```bash
# Parar container existente
./build-and-run.sh stop

# Ou usar porta diferente
docker run -p 3001:80 barbapp-admin
```

## 📝 Exemplos de Deploy

### Docker Swarm
```bash
docker service create \
  --name barbapp-admin \
  --publish 3000:80 \
  --env VITE_API_URL=https://api.barbapp.com/api \
  barbapp-admin
```

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: barbapp-admin
spec:
  replicas: 2
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
        env:
        - name: VITE_API_URL
          value: "https://api.barbapp.com/api"
        - name: VITE_APP_NAME
          value: "BarbApp Admin"
```

## 🚀 Performance

- Imagem otimizada (~25MB para o runtime)
- Gzip compression habilitada
- Cache de assets estáticos (1 ano)
- HTML não cacheado (sempre fresh)