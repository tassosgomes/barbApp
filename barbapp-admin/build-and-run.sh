#!/bin/bash

# Script para facilitar o build e execução do container Docker

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}=== BarbApp Admin Docker Build & Run ===${NC}"

# Função para mostrar ajuda
show_help() {
    echo "Uso: $0 [OPÇÃO]"
    echo ""
    echo "Opções:"
    echo "  build     Apenas fazer o build da imagem"
    echo "  run       Apenas executar o container (imagem deve existir)"
    echo "  dev       Build e run para desenvolvimento"
    echo "  prod      Build e run para produção"
    echo "  stop      Parar o container"
    echo "  logs      Mostrar logs do container"
    echo "  clean     Remover imagem e container"
    echo "  help      Mostrar esta ajuda"
    echo ""
    echo "Variáveis de ambiente (opcional):"
    echo "  API_URL   URL da API backend (padrão: http://localhost:5070/api)"
    echo "  APP_NAME  Nome da aplicação (padrão: BarbApp Admin)"
    echo ""
    echo "Exemplos:"
    echo "  $0 dev"
    echo "  API_URL=https://api.barbapp.com/api $0 prod"
}

# Configurações
IMAGE_NAME="barbapp-admin"
CONTAINER_NAME="barbapp-admin-container"
HOST_PORT=${HOST_PORT:-"3001"}
CONTAINER_PORT="80"

# Variáveis de ambiente com valores padrão
API_URL=${API_URL:-"http://localhost:5070/api"}
APP_NAME=${APP_NAME:-"BarbApp Admin"}

build_image() {
    echo -e "${YELLOW}Building Docker image...${NC}"
    docker build -t $IMAGE_NAME .
    echo -e "${GREEN}Build completed!${NC}"
}

run_container() {
    local env_type=$1
    
    echo -e "${YELLOW}Starting container...${NC}"
    echo -e "${YELLOW}API URL: ${API_URL}${NC}"
    echo -e "${YELLOW}App Name: ${APP_NAME}${NC}"
    
    # Parar container existente se estiver rodando
    docker stop $CONTAINER_NAME 2>/dev/null || true
    docker rm $CONTAINER_NAME 2>/dev/null || true
    
    docker run -d \
        --name $CONTAINER_NAME \
        -p $HOST_PORT:$CONTAINER_PORT \
        -e VITE_API_URL="$API_URL" \
        -e VITE_APP_NAME="$APP_NAME" \
        $IMAGE_NAME
    
    echo -e "${GREEN}Container started!${NC}"
    echo -e "${GREEN}Acesse: http://localhost:$HOST_PORT${NC}"
    
    # Mostrar logs iniciais
    echo -e "${YELLOW}Logs iniciais:${NC}"
    docker logs $CONTAINER_NAME
}

stop_container() {
    echo -e "${YELLOW}Stopping container...${NC}"
    docker stop $CONTAINER_NAME 2>/dev/null || echo "Container não estava rodando"
    docker rm $CONTAINER_NAME 2>/dev/null || echo "Container já foi removido"
    echo -e "${GREEN}Container stopped!${NC}"
}

show_logs() {
    echo -e "${YELLOW}Showing container logs...${NC}"
    docker logs -f $CONTAINER_NAME
}

clean_all() {
    echo -e "${YELLOW}Cleaning up...${NC}"
    docker stop $CONTAINER_NAME 2>/dev/null || true
    docker rm $CONTAINER_NAME 2>/dev/null || true
    docker rmi $IMAGE_NAME 2>/dev/null || true
    echo -e "${GREEN}Cleanup completed!${NC}"
}

# Verificar se Docker está rodando
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}Erro: Docker não está rodando ou não está acessível${NC}"
    exit 1
fi

# Processar argumentos
case "${1:-help}" in
    "build")
        build_image
        ;;
    "run")
        run_container
        ;;
    "dev")
        API_URL=${API_URL:-"http://localhost:5070/api"}
        build_image
        run_container "dev"
        ;;
    "prod")
        # Para produção, você pode sobrescrever a URL da API
        API_URL=${API_URL:-"https://api.barbapp.com/api"}
        build_image
        run_container "prod"
        ;;
    "stop")
        stop_container
        ;;
    "logs")
        show_logs
        ;;
    "clean")
        clean_all
        ;;
    "help"|*)
        show_help
        ;;
esac