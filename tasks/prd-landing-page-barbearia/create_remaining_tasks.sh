#!/bin/bash

# Tasks 14-20 (Admin Components and Pages)
cat > 14_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["10.0", "12.0"]
---
# Tarefa 14.0: Componente LogoUploader
Implementar componente de upload de logo com drag & drop, preview e validação.
Ver techspec-frontend.md seção 1.4.
EOF

cat > 15_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["10.0"]
---
# Tarefa 15.0: Componente ServiceManager
Criar componente para gerenciar serviços exibidos com drag & drop e visibilidade.
Ver techspec-frontend.md seção 1.4.
EOF

cat > 16_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["10.0"]
---
# Tarefa 16.0: Componente PreviewPanel
Criar painel de preview da landing page em tempo real (mobile/desktop).
EOF

cat > 17_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["10.0", "11.0", "14.0", "15.0"]
---
# Tarefa 17.0: Componente LandingPageForm
Criar formulário principal integrando todos os componentes (logo, serviços, campos texto).
Ver techspec-frontend.md seção 1.4.
EOF

cat > 18_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["11.0", "13.0", "17.0"]
---
# Tarefa 18.0: Página LandingPageEditor
Criar página principal do editor com tabs (Templates, Edição, Preview).
Ver techspec-frontend.md seção 1.5.
EOF

cat > 19_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["18.0"]
---
# Tarefa 19.0: Integração com Rotas e Navegação
Adicionar rotas do módulo Landing Page ao router do admin.
EOF

cat > 20_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["11.0", "12.0", "13.0", "14.0", "15.0", "16.0", "17.0", "18.0"]
---
# Tarefa 20.0: Testes Admin Frontend
Criar testes unitários e de integração para componentes e hooks do admin.
EOF

# Tasks 24-28 (Public Templates)
cat > 24_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---
# Tarefa 24.0: Template 1 - Clássico
Implementar template clássico (preto, dourado, serif).
Ver techspec-frontend.md seção 2.5.
EOF

cat > 25_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---
# Tarefa 25.0: Template 2 - Moderno
Implementar template moderno (cinza escuro, azul elétrico, minimalista).
EOF

cat > 26_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---
# Tarefa 26.0: Template 3 - Vintage
Implementar template vintage (marrom, creme, retrô anos 50/60).
EOF

cat > 27_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---
# Tarefa 27.0: Template 4 - Urbano
Implementar template urbano (preto, vermelho vibrante, street/hip-hop).
EOF

cat > 28_task.md << 'EOF'
---
status: pending
parallelizable: true
blocked_by: ["22.0", "23.0"]
---
# Tarefa 28.0: Template 5 - Premium
Implementar template premium (preto, dourado metálico, luxuoso).
EOF

# Tasks 29-30 (Public Integration)
cat > 29_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["24.0", "25.0", "26.0", "27.0", "28.0"]
---
# Tarefa 29.0: Página LandingPage e Router
Criar página principal que carrega template dinâmico e configurar router.
Ver techspec-frontend.md seção 2.6.
EOF

cat > 30_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["29.0"]
---
# Tarefa 30.0: Testes E2E Landing Page Pública
Criar testes end-to-end com Playwright para fluxos da landing page.
Ver techspec-frontend.md seção 6.
EOF

# Tasks 31-33 (Final Integration)
cat > 31_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["9.0", "20.0", "30.0"]
---
# Tarefa 31.0: Integração Completa Backend ↔ Admin ↔ Público
Validar integração end-to-end entre todas as camadas, corrigir bugs de integração.
EOF

cat > 32_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["31.0"]
---
# Tarefa 32.0: Otimizações de Performance e SEO
- Lazy loading de templates
- Otimização de imagens
- SEO meta tags
- Lighthouse score > 90
EOF

cat > 33_task.md << 'EOF'
---
status: pending
parallelizable: false
blocked_by: ["32.0"]
---
# Tarefa 33.0: Documentação e Deployment
- Documentação completa (README, API docs)
- Guia de deploy
- Variáveis de ambiente
- Scripts de deployment
EOF

echo "All task files created successfully!"
