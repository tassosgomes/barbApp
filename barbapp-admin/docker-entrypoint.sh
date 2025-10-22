#!/bin/bash
set -e

# Função para substituir variáveis de ambiente nos arquivos JS
replace_env_vars() {
    local file="$1"
    echo "Substituindo variáveis em: $file"
    
    # Substituir variáveis do Vite nos arquivos JavaScript
    sed -i "s|__VITE_API_URL__|${VITE_API_URL}|g" "$file"
    sed -i "s|__VITE_APP_NAME__|${VITE_APP_NAME}|g" "$file"
    
    echo "Variáveis substituídas em: $file"
}

echo "=== Iniciando configuração do BarbApp Admin ==="
echo "VITE_API_URL: ${VITE_API_URL}"
echo "VITE_APP_NAME: ${VITE_APP_NAME}"

# Encontrar e processar todos os arquivos JS no diretório de assets
echo "Procurando arquivos JavaScript para substituição de variáveis..."

find /usr/share/nginx/html -name "*.js" -type f | while read -r file; do
    # Verificar se o arquivo contém placeholders antes de processá-lo
    if grep -q "__VITE_" "$file"; then
        replace_env_vars "$file"
    fi
done

# Processar também o index.html se necessário
if [ -f "/usr/share/nginx/html/index.html" ]; then
    echo "Verificando index.html para substituição de variáveis..."
    if grep -q "__VITE_" "/usr/share/nginx/html/index.html"; then
        replace_env_vars "/usr/share/nginx/html/index.html"
    fi
fi

echo "=== Configuração concluída ==="
echo "Iniciando Nginx..."

# Executar o comando original (nginx)
exec "$@"