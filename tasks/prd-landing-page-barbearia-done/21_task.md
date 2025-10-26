---
status: completed
parallelizable: false
blocked_by: []
completed_date: 2025-10-23
---

<task_context>
<domain>frontend-public/setup</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>none</dependencies>
<unblocks>22.0, 23.0</unblocks>
</task_context>

# Tarefa 21.0: Setup Projeto barbapp-public

## Visão Geral

Criar novo projeto React + TypeScript + Vite para a landing page pública. Configurar todas as dependências, estrutura de pastas e configurações base.

<requirements>
- Projeto Vite + React + TypeScript
- Tailwind CSS configurado
- React Router
- TanStack Query
- Axios
- Estrutura de pastas organizada
- Variáveis de ambiente
</requirements>

## Subtarefas

- [x] 21.1 Criar projeto com Vite (template react-ts) ✅
- [x] 21.2 Instalar dependências (react-router-dom, @tanstack/react-query, axios, lucide-react) ✅
- [x] 21.3 Configurar Tailwind CSS (config customizado com cores dos templates) ✅
- [x] 21.4 Criar estrutura de pastas (/templates, /components, /hooks, /types, /pages) ✅
- [x] 21.5 Configurar variáveis de ambiente (.env.example) ✅
- [x] 21.6 Configurar vite.config.ts (porta 3001, proxy se necessário) ✅
- [x] 21.7 Criar README.md do projeto ✅
- [x] 21.8 Testar build e dev server ✅

## Detalhes de Implementação

### Comandos de Setup

```bash
cd barbApp
npm create vite@latest barbapp-public -- --template react-ts
cd barbapp-public
npm install
npm install react-router-dom @tanstack/react-query axios lucide-react
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

### tailwind.config.js

```javascript
/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      colors: {
        gold: '#D4AF37',
        'gold-dark': '#B8941E',
      },
      fontFamily: {
        serif: ['Playfair Display', 'serif'],
        sans: ['Inter', 'sans-serif'],
      },
    },
  },
  plugins: [],
};
```

### package.json scripts

```json
{
  "scripts": {
    "dev": "vite --port 3001",
    "build": "tsc && vite build",
    "preview": "vite preview"
  }
}
```

### .env.example

```
VITE_API_URL=http://localhost:5000/api
```

## Sequenciamento

- **Bloqueado por**: Nenhuma (primeira tarefa public)
- **Desbloqueia**: 22.0 (Types e Hooks), 23.0 (Componentes)
- **Paralelizável**: Sim (pode rodar em paralelo com admin frontend)

## Critérios de Sucesso

- [x] Projeto criado e rodando em localhost:3001 ✅
- [x] Tailwind CSS funcionando ✅
- [x] Estrutura de pastas criada ✅
- [x] Dependências instaladas ✅
- [x] Build production funciona ✅
- [x] README documentado ✅

## Revisão Completa

- [x] Definição da tarefa, PRD e tech spec validados ✅
- [x] Análise de regras e conformidade verificadas ✅
- [x] Revisão de código completada ✅
- [x] Pronto para deploy ✅

**Relatório de Revisão**: `21_task_review.md`
