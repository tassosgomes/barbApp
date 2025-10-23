#!/bin/bash

# Script para testar upload de logo via curl
# Bug #4 - Upload de imagens dando erro

echo "=========================================="
echo "Teste de Upload de Logo - Bug #4"
echo "=========================================="
echo ""

# Configurações
BARBERSHOP_ID="5e04d43f-ad74-408b-b764-05668a020e5c"
API_URL="http://localhost:5070/api/admin/landing-pages/${BARBERSHOP_ID}/logo"
TOKEN_FILE="/tmp/barbapp_token.txt"
IMAGE_PATH="$1"

# Verificar se a imagem foi fornecida
if [ -z "$IMAGE_PATH" ]; then
    echo "❌ Erro: Caminho da imagem não fornecido"
    echo "Uso: $0 <caminho-da-imagem>"
    echo "Exemplo: $0 /tmp/test_logo.png"
    exit 1
fi

# Verificar se o arquivo existe
if [ ! -f "$IMAGE_PATH" ]; then
    echo "❌ Erro: Arquivo não encontrado: $IMAGE_PATH"
    exit 1
fi

# Obter informações do arquivo
FILE_SIZE=$(stat -f%z "$IMAGE_PATH" 2>/dev/null || stat -c%s "$IMAGE_PATH" 2>/dev/null)
FILE_TYPE=$(file -b --mime-type "$IMAGE_PATH")

echo "📁 Arquivo: $IMAGE_PATH"
echo "📏 Tamanho: $(numfmt --to=iec-i --suffix=B $FILE_SIZE 2>/dev/null || echo "$FILE_SIZE bytes")"
echo "🎨 Tipo MIME: $FILE_TYPE"
echo ""

# Fazer login e obter token
echo "🔑 Fazendo login..."
LOGIN_RESPONSE=$(curl -s -X POST "http://localhost:5070/api/auth/admin-barbearia/login" \
  -H "Content-Type: application/json" \
  -d '{
    "codigo": "CEB4XAR7",
    "email": "luiz.gomes@gmail.com",
    "senha": "_y4#gA$ZlJQG"
  }')

# Extrair token do response
TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"token":"[^"]*' | sed 's/"token":"//')

if [ -z "$TOKEN" ]; then
    echo "❌ Erro: Falha ao obter token de autenticação"
    echo "Response:"
    echo "$LOGIN_RESPONSE"
    exit 1
fi

echo "✅ Login realizado com sucesso"
echo "🎫 Token: ${TOKEN:0:50}..."
echo ""
echo "$TOKEN" > "$TOKEN_FILE"

# Fazer upload da imagem
echo "📤 Fazendo upload do logo..."
echo "🔗 URL: $API_URL"
echo ""

UPLOAD_RESPONSE=$(curl -v -X POST "$API_URL" \
  -H "Authorization: Bearer $TOKEN" \
  -F "file=@$IMAGE_PATH" \
  2>&1)

# Extrair status code
HTTP_CODE=$(echo "$UPLOAD_RESPONSE" | grep "< HTTP" | tail -1 | awk '{print $3}')

echo ""
echo "=========================================="
echo "RESULTADO"
echo "=========================================="
echo "Status Code: $HTTP_CODE"
echo ""

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "204" ]; then
    echo "✅ Upload realizado com sucesso!"
    echo ""
    echo "Response:"
    echo "$UPLOAD_RESPONSE" | grep -A 100 "^{" || echo "$UPLOAD_RESPONSE" | tail -20
elif [ "$HTTP_CODE" = "400" ]; then
    echo "❌ Erro 400 - Bad Request (BUG CONFIRMADO)"
    echo ""
    echo "Response Headers:"
    echo "$UPLOAD_RESPONSE" | grep "^<" | head -20
    echo ""
    echo "Response Body:"
    echo "$UPLOAD_RESPONSE" | grep -A 100 "^{" || echo "$UPLOAD_RESPONSE" | tail -30
else
    echo "❌ Erro inesperado"
    echo ""
    echo "Full Response:"
    echo "$UPLOAD_RESPONSE"
fi

echo ""
echo "=========================================="
echo "Log salvo em: /tmp/logo_upload_test.log"
echo "$UPLOAD_RESPONSE" > /tmp/logo_upload_test.log
echo "Token salvo em: $TOKEN_FILE"
echo "=========================================="
