# BarbApp Public - Docker Setup

Este documento descreve como executar o frontend público do BarbApp usando Docker.

## 📋 Pré-requisitos

- Docker instalado
- Docker Compose (opcional)

## 🚀 Início Rápido

### Opção 1: Script Build and Run

```bash
./build-and-run.sh
```

O script irá:
1. Construir a imagem Docker
2. Parar e remover o container existente (se houver)
3. Iniciar um novo container
4. Expor a aplicação na porta 3001

### Opção 2: Docker Compose

```bash
docker-compose up -d
```

### Opção 3: Comandos Docker Manuais

```bash
# Build
docker build -t barbapp-public:latest .

# Run
docker run -d \
  --name barbapp-public \
  -p 3001:80 \
  -e VITE_API_URL=http://localhost:5070/api \
  barbapp-public:latest
```

## 🔧 Variáveis de Ambiente

| Variável | Descrição | Padrão |
|----------|-----------|--------|
| `VITE_API_URL` | URL da API backend | `http://localhost:5070/api` |

### Exemplo de uso:

```bash
docker run -d \
  --name barbapp-public \
  -p 3001:80 \
  -e VITE_API_URL=https://api.barbapp.com/api \
  barbapp-public:latest
```

## 📦 Arquitetura do Docker

### Multi-stage Build

O Dockerfile utiliza multi-stage build para otimizar o tamanho da imagem:

1. **Stage 1 (Builder)**: 
   - Base: `node:20-alpine` (Node.js 20+ necessário para Vite)
   - Instala dependências de build (python3, make, g++) para bindings nativos
   - Instala dependências npm
   - Executa o build da aplicação com Vite (usando `tsconfig.build.json` que exclui testes)
   - Gera os arquivos estáticos em `/app/dist`

2. **Stage 2 (Production)**:
   - Base: `nginx:alpine`
   - Copia apenas os arquivos de build
   - Configura nginx para servir a SPA
   - Substitui variáveis de ambiente em runtime

### Substituição de Variáveis em Runtime

O script `docker-entrypoint.sh` substitui os placeholders das variáveis de ambiente nos arquivos JavaScript compilados antes de iniciar o nginx. Isso permite:

- Mudar configurações sem rebuild da imagem
- Usar a mesma imagem em diferentes ambientes (dev, staging, prod)
- Facilitar deployments com diferentes configurações

## 🌐 Nginx

O nginx está configurado para:

- ✅ Servir a aplicação React como SPA
- ✅ Fallback para `index.html` em rotas não encontradas
- ✅ Cache otimizado para assets estáticos (1 ano)
- ✅ Compressão gzip habilitada
- ✅ Health check endpoint em `/health`

## 📝 Comandos Úteis

```bash
# Ver logs
docker logs -f barbapp-public

# Parar container
docker stop barbapp-public

# Iniciar container
docker start barbapp-public

# Reiniciar container
docker restart barbapp-public

# Remover container
docker rm -f barbapp-public

# Acessar shell do container
docker exec -it barbapp-public sh

# Verificar health
curl http://localhost:3001/health
```

## 🏗️ Estrutura de Arquivos Docker

```
barbapp-public/
├── Dockerfile              # Multi-stage build
├── docker-compose.yml      # Configuração Docker Compose
├── docker-entrypoint.sh    # Script de inicialização
├── nginx.conf              # Configuração do nginx
├── .dockerignore           # Arquivos ignorados no build
└── build-and-run.sh        # Script de build e execução
```

## 🔍 Troubleshooting

### Container não inicia

```bash
# Verificar logs
docker logs barbapp-public

# Verificar se a porta está em uso
sudo lsof -i :3001
```

### Variáveis de ambiente não aplicadas

```bash
# Verificar variáveis dentro do container
docker exec barbapp-public env | grep VITE

# Verificar se a substituição foi feita
docker exec barbapp-public cat /usr/share/nginx/html/assets/index-*.js | grep -o "http[s]*://[^\"]*api"
```

### Rebuild necessário

```bash
# Rebuild sem cache
docker build --no-cache -t barbapp-public:latest .

# Ou usando o script
./build-and-run.sh
```

## 🌍 Deploy em Produção

### Docker Swarm / Kubernetes

A imagem está preparada para orquestração:

```yaml
# Exemplo para Docker Swarm
services:
  barbapp-public:
    image: barbapp-public:latest
    deploy:
      replicas: 3
      update_config:
        parallelism: 1
        delay: 10s
    ports:
      - "3001:80"
    environment:
      - VITE_API_URL=https://api.barbapp.com/api
```

### Variáveis por Ambiente

**Development:**
```bash
VITE_API_URL=http://localhost:5070/api
```

**Staging:**
```bash
VITE_API_URL=https://api-staging.barbapp.com/api
```

**Production:**
```bash
VITE_API_URL=https://api.barbapp.com/api
```

## 📊 Otimizações

- ✅ Build em duas etapas (reduz tamanho final)
- ✅ Apenas arquivos necessários no container final
- ✅ Nginx otimizado para servir SPAs
- ✅ Gzip habilitado para reduzir transferência
- ✅ Cache agressivo para assets estáticos
- ✅ Health check endpoint
- ✅ Logs estruturados

## 🔗 Acesso

Após iniciar o container, acesse:

- **Aplicação**: http://localhost:3001
- **Health Check**: http://localhost:3001/health
