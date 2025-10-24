#!/bin/bash

# Quick Deploy Script - v0.1.6 JWT Authentication Fix
# Data: 2025-10-24

set -e

echo "🚀 Quick Deploy - BarbApp API v0.1.6"
echo "===================================="
echo ""

# Cores
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

# 1. Pull da nova imagem
echo -e "${BLUE}📥 Pulling new image...${NC}"
docker pull tsgomes/barberapp-api:v0.1.6

if [ $? -eq 0 ]; then
    echo -e "${GREEN}✓ Image pulled successfully${NC}"
else
    echo -e "${RED}✗ Failed to pull image${NC}"
    exit 1
fi

echo ""

# 2. Verificar se está usando Swarm ou Compose
if docker node ls &> /dev/null; then
    echo -e "${YELLOW}📦 Docker Swarm detected${NC}"
    
    # Verificar se a stack existe
    if docker stack ls | grep -q barbapp; then
        echo -e "${BLUE}🔄 Updating barbapp_backend service...${NC}"
        docker service update --image tsgomes/barberapp-api:v0.1.6 barbapp_backend
        
        echo ""
        echo -e "${BLUE}⏳ Waiting for service to update...${NC}"
        sleep 5
        
        echo ""
        echo -e "${YELLOW}📊 Service status:${NC}"
        docker service ps barbapp_backend --filter "desired-state=running" --format "table {{.Name}}\t{{.Image}}\t{{.CurrentState}}\t{{.Error}}"
    else
        echo -e "${YELLOW}Stack 'barbapp' not found. Deploying new stack...${NC}"
        docker stack deploy -c docker-compose.yml barbapp
    fi
else
    echo -e "${YELLOW}📦 Docker Compose detected${NC}"
    echo -e "${BLUE}🔄 Updating backend service...${NC}"
    docker-compose up -d backend
fi

echo ""
echo -e "${GREEN}✅ Deploy completed!${NC}"
echo ""

# 3. Mostrar logs
echo -e "${BLUE}📝 Recent logs (last 20 lines):${NC}"
echo "================================"

if docker node ls &> /dev/null; then
    docker service logs --tail 20 barbapp_backend
else
    docker-compose logs --tail 20 backend
fi

echo ""
echo -e "${YELLOW}🔍 To follow logs in real-time:${NC}"
if docker node ls &> /dev/null; then
    echo "   docker service logs -f barbapp_backend"
else
    echo "   docker-compose logs -f backend"
fi

echo ""
echo -e "${YELLOW}🌐 Test the login:${NC}"
echo "   https://dev-admbarberapp.tasso.dev.br/admin/login"
echo ""
echo -e "${YELLOW}📊 Health check:${NC}"
echo "   curl -I https://dev-api-barberapp.tasso.dev.br/health"
echo ""
