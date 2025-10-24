#!/bin/bash

# Script para build e execução do container Docker do BarbApp Public

set -e

echo "🏗️  Building BarbApp Public Docker image..."
docker build -t barbapp-public:latest .

echo "🧹 Stopping and removing existing container (if any)..."
docker stop barbapp-public 2>/dev/null || true
docker rm barbapp-public 2>/dev/null || true

echo "🚀 Starting BarbApp Public container..."
docker run -d \
  --name barbapp-public \
  -p 3001:80 \
  -e VITE_API_URL=${VITE_API_URL:-http://localhost:5070/api} \
  --restart unless-stopped \
  barbapp-public:latest

echo "✅ BarbApp Public is running!"
echo "📍 Access at: http://localhost:3001"
echo ""
echo "Useful commands:"
echo "  docker logs -f barbapp-public     # View logs"
echo "  docker stop barbapp-public         # Stop container"
echo "  docker start barbapp-public        # Start container"
echo "  docker restart barbapp-public      # Restart container"
