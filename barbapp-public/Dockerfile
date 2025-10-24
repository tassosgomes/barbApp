# Multi-stage build para otimizar o tamanho da imagem
FROM node:20-alpine AS builder

# Definir diretório de trabalho
WORKDIR /app

# Instalar dependências de build necessárias para bindings nativos
RUN apk add --no-cache python3 make g++

# Copiar arquivos de dependências
COPY package.json package-lock.json ./

# Instalar todas as dependências (incluindo devDependencies para build)
RUN npm ci --silent

# Copiar código fonte
COPY . .

# Build da aplicação com variáveis placeholder
ENV VITE_API_URL=__VITE_API_URL__

# Build usando tsconfig específico para produção (exclui testes)
RUN npm run build:docker

# Estágio de produção
FROM nginx:alpine

# Instalar ferramentas necessárias para substituição de variáveis
RUN apk add --no-cache bash gettext

# Copiar configuração customizada do nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar build da aplicação
COPY --from=builder /app/dist /usr/share/nginx/html

# Copiar script de inicialização
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN chmod +x /docker-entrypoint.sh

# Variáveis de ambiente padrão
ENV VITE_API_URL=http://localhost:5070/api

# Expor porta
EXPOSE 80

# Script de entrada que substitui as variáveis e inicia o nginx
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
