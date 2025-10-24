#!/bin/bash
set -e

# Função para substituir variáveis de ambiente nos arquivos JS
replace_env_vars() {
    echo "🔧 Substituindo variáveis de ambiente..."
    
    # Encontrar todos os arquivos JS no diretório de build
    for file in /usr/share/nginx/html/assets/*.js; do
        if [ -f "$file" ]; then
            echo "  📝 Processando: $(basename $file)"
            
            # Substituir placeholders pelas variáveis de ambiente reais
            sed -i "s|__VITE_API_URL__|${VITE_API_URL}|g" "$file"
        fi
    done
    
    echo "✅ Variáveis de ambiente substituídas com sucesso!"
    echo "   VITE_API_URL=${VITE_API_URL}"
}

# Executar substituição
replace_env_vars

# Executar o comando passado (nginx)
echo "🚀 Iniciando nginx..."
exec "$@"
