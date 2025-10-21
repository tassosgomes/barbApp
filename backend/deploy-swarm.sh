#!/bin/bash

# Script para deploy da stack BarbApp no Docker Swarm
# Autor: GitHub Copilot
# Data: 2025-10-21

set -e

echo "🚀 Iniciando deploy da stack BarbApp no Docker Swarm..."

# Cores para output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# 1. Criar redes se não existirem
echo -e "${YELLOW}📡 Verificando redes...${NC}"
docker network ls | grep -q traefik-public || {
    echo "Criando rede traefik-public..."
    docker network create --driver=overlay traefik-public
}

docker network ls | grep -q barbapp-internal || {
    echo "Criando rede barbapp-internal..."
    docker network create --driver=overlay barbapp-internal
}

echo -e "${GREEN}✓ Redes criadas/verificadas${NC}"

# 2. Fazer deploy da stack
echo -e "${YELLOW}📦 Fazendo deploy da stack...${NC}"
docker stack deploy -c docker-compose.yml barbapp

echo -e "${GREEN}✓ Stack deployed${NC}"

# 3. Aguardar alguns segundos
echo -e "${YELLOW}⏳ Aguardando serviços iniciarem...${NC}"
sleep 5

# 4. Verificar status dos serviços
echo -e "${YELLOW}📊 Status dos serviços:${NC}"
docker service ls

echo ""
echo -e "${YELLOW}🔍 Detalhes dos serviços BarbApp:${NC}"
docker service ps barbapp_backend barbapp_smtp4dev barbapp_traefik 2>/dev/null || true

# 5. Instruções finais
echo ""
echo -e "${GREEN}✅ Deploy concluído!${NC}"
echo ""
echo -e "${YELLOW}📝 Próximos passos:${NC}"
echo "1. Aguarde alguns minutos para os certificados SSL serem gerados"
echo "2. Verifique os logs dos serviços:"
echo "   - docker service logs barbapp_backend"
echo "   - docker service logs barbapp_smtp4dev"
echo "   - docker service logs barbapp_traefik"
echo ""
echo "3. Acesse o dashboard do Traefik:"
echo "   - http://seu-servidor:8080/dashboard/"
echo ""
echo "4. Teste as URLs:"
echo "   - https://dev-api-barberapp.tasso.dev.br"
echo "   - https://mail.tasso.dev.br"
echo ""
echo -e "${YELLOW}🔧 Para atualizar a stack:${NC}"
echo "   docker stack deploy -c docker-compose.yml barbapp"
echo ""
echo -e "${YELLOW}🗑️  Para remover a stack:${NC}"
echo "   docker stack rm barbapp"
