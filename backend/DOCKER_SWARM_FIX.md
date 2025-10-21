# Correção do Docker Compose para Docker Swarm

## Problema Identificado

O Traefik não estava detectando os serviços `barbapp-backend` e `smtp4dev` porque:

1. **Labels no lugar errado**: No Docker Swarm, as labels do Traefik devem estar dentro de `deploy.labels`, não diretamente em `labels`
2. **Backend com imagem errada**: O serviço estava usando `image: nginx` em vez de fazer o build da aplicação
3. **Traefik sem acesso à rede interna**: O Traefik precisa estar na rede `barbapp-internal` para acessar os serviços

## Correções Aplicadas

### 1. Backend Service
```yaml
# ❌ ANTES (errado)
services:
  backend:
    image: nginx
    labels:
      - "traefik.enable=true"
      # ... outras labels

# ✅ DEPOIS (correto)
services:
  backend:
    build:
      context: .
      dockerfile: src/Dockerfile
    deploy:
      labels:
        - "traefik.enable=true"
        # ... outras labels
```

### 2. SMTP4dev Service
```yaml
# ❌ ANTES (errado)
smtp4dev:
  labels:
    - "traefik.enable=true"
    # ... outras labels

# ✅ DEPOIS (correto)
smtp4dev:
  deploy:
    labels:
      - "traefik.enable=true"
      # ... outras labels
```

### 3. Traefik Service
```yaml
# ❌ ANTES (errado)
traefik:
  networks:
    - traefik-public

# ✅ DEPOIS (correto)
traefik:
  networks:
    - traefik-public
    - barbapp-internal
```

## Como Aplicar no Servidor

### Opção 1: Usando o script automatizado
```bash
# 1. Copie o docker-compose.yml atualizado para o servidor

# 2. Execute o script de deploy
chmod +x deploy-swarm.sh
./deploy-swarm.sh
```

### Opção 2: Comandos manuais
```bash
# 1. Criar as redes (se não existirem)
docker network create --driver=overlay traefik-public
docker network create --driver=overlay barbapp-internal

# 2. Deploy da stack
docker stack deploy -c docker-compose.yml barbapp

# 3. Verificar status
docker service ls
docker service ps barbapp_backend
docker service ps barbapp_smtp4dev
docker service ps barbapp_traefik

# 4. Ver logs
docker service logs -f barbapp_traefik
docker service logs -f barbapp_backend
```

## Verificação

Após aplicar as mudanças:

1. **Dashboard do Traefik** (http://seu-servidor:8080/dashboard/)
   - Deve mostrar 5 routers HTTP (3 internos + 2 dos seus serviços)
   - Procure por: `barbapp-backend` e `smtp4dev`

2. **Teste as URLs**
   - https://dev-api-barberapp.tasso.dev.br (backend)
   - https://mail.tasso.dev.br (smtp4dev)

3. **Certificados SSL**
   - Aguarde 1-2 minutos para o Let's Encrypt gerar os certificados
   - Verifique em `/etc/traefik/acme.json` (dentro do volume)

## Comandos Úteis

```bash
# Atualizar stack (após mudanças no docker-compose.yml)
docker stack deploy -c docker-compose.yml barbapp

# Remover stack completamente
docker stack rm barbapp

# Ver logs em tempo real
docker service logs -f barbapp_traefik
docker service logs -f barbapp_backend

# Verificar configuração do Traefik
docker service inspect barbapp_traefik

# Forçar rebuild do backend
docker service update --force barbapp_backend
```

## Diferenças: Docker Compose vs Docker Swarm

| Aspecto | Docker Compose | Docker Swarm |
|---------|---------------|--------------|
| Labels do Traefik | `labels:` (nível raiz) | `deploy.labels:` |
| Restart Policy | `restart:` | `deploy.restart_policy:` |
| Replicas | - | `deploy.replicas:` |
| Placement | - | `deploy.placement:` |
| Redes | Criadas automaticamente | Devem ser criadas com `docker network create --driver=overlay` |

## Troubleshooting

### Problema: 404 Not Found
- **Causa**: Traefik não detectou os routers
- **Solução**: Verificar que as labels estão em `deploy.labels`

### Problema: 502 Bad Gateway
- **Causa**: Serviço não está respondendo ou porta incorreta
- **Solução**: Verificar logs do serviço e `loadbalancer.server.port`

### Problema: Certificado SSL não gerado
- **Causa**: DNS não aponta para o servidor ou portas 80/443 bloqueadas
- **Solução**: Verificar DNS e firewall, aguardar mais tempo

### Problema: Serviço não inicia
- **Causa**: Erro na aplicação ou configuração
- **Solução**: Verificar logs com `docker service logs`
