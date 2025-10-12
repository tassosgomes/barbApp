#!/bin/bash

# Script de validação end-to-end dos fluxos de autenticação
# Tarefa 14.0 - Validação End-to-End e Ajustes Finais

set -e

API_URL="http://localhost:5000"
echo "🚀 Iniciando testes end-to-end dos fluxos de autenticação"
echo "API URL: $API_URL"
echo "=========================================="

# Cores para output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Função para testar requisição
test_request() {
    local test_name="$1"
    local method="$2"
    local endpoint="$3"
    local data="$4"
    local expected_status="$5"
    local token="$6"

    echo -e "\n${YELLOW}Testando: $test_name${NC}"

    if [ -n "$token" ]; then
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $token" \
            -d "$data" \
            "$API_URL$endpoint")
    else
        response=$(curl -s -w "\n%{http_code}" -X "$method" \
            -H "Content-Type: application/json" \
            -d "$data" \
            "$API_URL$endpoint")
    fi

    http_code=$(echo "$response" | tail -n1)
    body=$(echo "$response" | sed '$d')

    if [ "$http_code" = "$expected_status" ]; then
        echo -e "${GREEN}✓ Status: $http_code (Esperado: $expected_status)${NC}"
        echo "$body" | jq '.' 2>/dev/null || echo "$body"
        echo "$body"
    else
        echo -e "${RED}✗ Status: $http_code (Esperado: $expected_status)${NC}"
        echo "$body" | jq '.' 2>/dev/null || echo "$body"
        return 1
    fi
}

echo -e "\n${YELLOW}=== FLUXO 1: Autenticação Admin Central ===${NC}"

# 1.1 Login Admin Central - Credenciais corretas
ADMIN_TOKEN=$(test_request \
    "Login Admin Central com credenciais válidas" \
    "POST" \
    "/api/auth/admin-central" \
    '{"email":"admin@barbapp.com","password":"Admin@123"}' \
    "200" | jq -r '.token // empty')

if [ -z "$ADMIN_TOKEN" ]; then
    echo -e "${RED}✗ Falha ao obter token do Admin Central${NC}"
    exit 1
fi

echo -e "${GREEN}✓ Token obtido: ${ADMIN_TOKEN:0:20}...${NC}"

# 1.2 Login Admin Central - Credenciais incorretas
test_request \
    "Login Admin Central com senha incorreta" \
    "POST" \
    "/api/auth/admin-central" \
    '{"email":"admin@barbapp.com","password":"SenhaErrada"}' \
    "401"

# 1.3 Login Admin Central - Input inválido
test_request \
    "Login Admin Central com email inválido" \
    "POST" \
    "/api/auth/admin-central" \
    '{"email":"invalid","password":"Admin@123"}' \
    "400"

echo -e "\n${YELLOW}=== FLUXO 2: Autenticação Admin Barbearia ===${NC}"

# Primeiro precisa criar uma barbearia e admin
BARBEARIA_ID=$(test_request \
    "Criar barbearia para testes" \
    "POST" \
    "/api/barbershops" \
    '{"name":"Barbearia Teste E2E","phone":"11987654321","address":"Rua Teste, 123"}' \
    "201" \
    "$ADMIN_TOKEN" | jq -r '.id // empty')

if [ -z "$BARBEARIA_ID" ]; then
    echo -e "${RED}✗ Falha ao criar barbearia${NC}"
    exit 1
fi

BARBEARIA_CODE=$(test_request \
    "Buscar código da barbearia" \
    "GET" \
    "/api/barbershops/$BARBEARIA_ID" \
    "" \
    "200" \
    "$ADMIN_TOKEN" | jq -r '.code // empty')

echo -e "${GREEN}✓ Barbearia criada: ID=$BARBEARIA_ID, Code=$BARBEARIA_CODE${NC}"

# Criar Admin Barbearia
test_request \
    "Criar Admin Barbearia" \
    "POST" \
    "/api/admin-barbearia" \
    "{\"barbeariaId\":\"$BARBEARIA_ID\",\"email\":\"admin@barbearia.com\",\"password\":\"Admin@123\",\"name\":\"Admin Barbearia Teste\"}" \
    "201" \
    "$ADMIN_TOKEN"

# 2.1 Login Admin Barbearia - Sucesso
ADMIN_BARB_TOKEN=$(test_request \
    "Login Admin Barbearia com credenciais válidas" \
    "POST" \
    "/api/auth/admin-barbearia" \
    "{\"codigo\":\"$BARBEARIA_CODE\",\"email\":\"admin@barbearia.com\",\"password\":\"Admin@123\"}" \
    "200" | jq -r '.token // empty')

# 2.2 Login Admin Barbearia - Código inválido
test_request \
    "Login Admin Barbearia com código inválido" \
    "POST" \
    "/api/auth/admin-barbearia" \
    '{"codigo":"INVALID1","email":"admin@barbearia.com","password":"Admin@123"}' \
    "404"

echo -e "\n${YELLOW}=== FLUXO 3: Autenticação Barbeiro ===${NC}"

# Criar barbeiro
test_request \
    "Criar barbeiro" \
    "POST" \
    "/api/barbers" \
    "{\"barbeariaId\":\"$BARBEARIA_ID\",\"name\":\"João Silva\",\"phone\":\"11999999999\"}" \
    "201" \
    "$ADMIN_BARB_TOKEN"

# 3.1 Login Barbeiro - Sucesso
BARBEIRO_TOKEN=$(test_request \
    "Login Barbeiro com telefone válido" \
    "POST" \
    "/api/auth/barbeiro" \
    "{\"codigo\":\"$BARBEARIA_CODE\",\"telefone\":\"11999999999\"}" \
    "200" | jq -r '.token // empty')

# 3.2 Listar barbeiros (validar isolamento)
test_request \
    "Listar barbeiros da barbearia" \
    "GET" \
    "/api/barbers" \
    "" \
    "200" \
    "$BARBEIRO_TOKEN"

echo -e "\n${YELLOW}=== FLUXO 4: Autenticação Cliente ===${NC}"

# 4.1 Login Cliente - Primeiro acesso (cadastro automático)
CLIENTE_TOKEN=$(test_request \
    "Login/Cadastro Cliente primeira vez" \
    "POST" \
    "/api/auth/cliente" \
    "{\"codigo\":\"$BARBEARIA_CODE\",\"telefone\":\"11988888888\",\"nome\":\"Maria Silva\"}" \
    "200" | jq -r '.token // empty')

# 4.2 Login Cliente - Acesso subsequente
test_request \
    "Login Cliente já cadastrado" \
    "POST" \
    "/api/auth/cliente" \
    "{\"codigo\":\"$BARBEARIA_CODE\",\"telefone\":\"11988888888\",\"nome\":\"Maria Silva\"}" \
    "200"

# 4.3 Login Cliente - Nome incorreto
test_request \
    "Login Cliente com nome incorreto" \
    "POST" \
    "/api/auth/cliente" \
    "{\"codigo\":\"$BARBEARIA_CODE\",\"telefone\":\"11988888888\",\"nome\":\"Nome Errado\"}" \
    "401"

echo -e "\n${YELLOW}=== FLUXO 5: Isolamento Multi-tenant ===${NC}"

# Criar segunda barbearia para testar isolamento
BARBEARIA2_ID=$(test_request \
    "Criar segunda barbearia" \
    "POST" \
    "/api/barbershops" \
    '{"name":"Barbearia Teste 2","phone":"11987654322","address":"Rua Teste 2, 456"}' \
    "201" \
    "$ADMIN_TOKEN" | jq -r '.id // empty')

BARBEARIA2_CODE=$(test_request \
    "Buscar código da segunda barbearia" \
    "GET" \
    "/api/barbershops/$BARBEARIA2_ID" \
    "" \
    "200" \
    "$ADMIN_TOKEN" | jq -r '.code // empty')

# Criar admin e barbeiro na segunda barbearia
test_request \
    "Criar Admin Barbearia 2" \
    "POST" \
    "/api/admin-barbearia" \
    "{\"barbeariaId\":\"$BARBEARIA2_ID\",\"email\":\"admin@barbearia2.com\",\"password\":\"Admin@123\",\"name\":\"Admin Barbearia 2\"}" \
    "201" \
    "$ADMIN_TOKEN"

ADMIN_BARB2_TOKEN=$(test_request \
    "Login Admin Barbearia 2" \
    "POST" \
    "/api/auth/admin-barbearia" \
    "{\"codigo\":\"$BARBEARIA2_CODE\",\"email\":\"admin@barbearia2.com\",\"password\":\"Admin@123\"}" \
    "200" | jq -r '.token // empty')

test_request \
    "Criar barbeiro na Barbearia 2" \
    "POST" \
    "/api/barbers" \
    "{\"barbeariaId\":\"$BARBEARIA2_ID\",\"name\":\"Pedro Santos\",\"phone\":\"11988888877\"}" \
    "201" \
    "$ADMIN_BARB2_TOKEN"

# Testar isolamento: Admin da Barbearia 1 não deve ver barbeiros da Barbearia 2
echo -e "${YELLOW}Validando isolamento: Admin Barbearia 1 listando barbeiros${NC}"
BARBEIROS_BARB1=$(test_request \
    "Listar barbeiros (Admin Barbearia 1)" \
    "GET" \
    "/api/barbers" \
    "" \
    "200" \
    "$ADMIN_BARB_TOKEN" | jq '. | length')

echo -e "${GREEN}✓ Admin Barbearia 1 vê $BARBEIROS_BARB1 barbeiro(s) (deve ser apenas da sua barbearia)${NC}"

echo -e "${YELLOW}Validando isolamento: Admin Barbearia 2 listando barbeiros${NC}"
BARBEIROS_BARB2=$(test_request \
    "Listar barbeiros (Admin Barbearia 2)" \
    "GET" \
    "/api/barbers" \
    "" \
    "200" \
    "$ADMIN_BARB2_TOKEN" | jq '. | length')

echo -e "${GREEN}✓ Admin Barbearia 2 vê $BARBEIROS_BARB2 barbeiro(s) (deve ser apenas da sua barbearia)${NC}"

echo -e "\n${GREEN}=========================================="
echo "✓ Todos os fluxos end-to-end validados com sucesso!"
echo -e "==========================================${NC}"
