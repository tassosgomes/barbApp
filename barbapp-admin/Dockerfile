# Multi-stage build para otimizar o tamanho da imagem
FROM node:18-alpine AS builder

# Definir diretório de trabalho
WORKDIR /app

# Copiar arquivos de dependências
COPY package.json package-lock.json ./

# Instalar todas as dependências (incluindo devDependencies para build)
RUN npm ci --silent

# Copiar código fonte
COPY . .

# Build da aplicação com variáveis placeholder
ENV VITE_API_URL=__VITE_API_URL__
ENV VITE_APP_NAME=__VITE_APP_NAME__

RUN npm run build

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
ENV VITE_APP_NAME="BarbApp Admin"

# Expor porta
EXPOSE 80

# Script de entrada que substitui as variáveis e inicia o nginx
ENTRYPOINT ["/docker-entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]