# BarbApp Admin

Painel administrativo do sistema BarbApp para gestão centralizada de barbearias e barbeiros.

## Tecnologias

- **React 18** - Biblioteca JavaScript para interfaces de usuário
- **TypeScript** - JavaScript com tipagem estática
- **Vite** - Build tool e dev server ultrarrápido
- **TailwindCSS** - Framework CSS utilitário
- **React Router** - Roteamento para aplicações React
- **React Hook Form** - Gerenciamento de formulários
- **Zod** - Validação de schemas TypeScript
- **Axios** - Cliente HTTP para requisições API
- **Radix UI** - Componentes primitivos acessíveis
- **Lucide React** - Ícones modernos

## Scripts Disponíveis

```bash
# Iniciar servidor de desenvolvimento
npm run dev

# Build para produção
npm run build

# Preview do build de produção
npm run preview

# Executar linting
npm run lint

# Formatar código com Prettier
npm run format

# Executar testes unitários
npm run test

# Executar testes com interface visual
npm run test:ui

# Executar testes com coverage
npm run test:coverage

# Executar testes E2E com Playwright
npm run test:e2e
```

## Estrutura do Projeto

```
src/
├── components/     # Componentes reutilizáveis
├── pages/         # Páginas da aplicação
├── hooks/         # Hooks customizados
├── lib/           # Utilitários e configurações
├── types/         # Definições de tipos TypeScript
└── styles/        # Estilos globais
```

## Desenvolvimento

1. Instale as dependências:
   ```bash
   npm install
   ```

2. Inicie o servidor de desenvolvimento:
   ```bash
   npm run dev
   ```

3. Acesse [http://localhost:3000](http://localhost:3000) no navegador.

## Build

Para gerar os arquivos de produção:

```bash
npm run build
```

Os arquivos serão gerados na pasta `dist/`.

## Testes

### Unitários
```bash
npm run test
```

### E2E
```bash
npm run test:e2e
```

## Configuração

### Variáveis de Ambiente

Crie um arquivo `.env` baseado no `.env.example`:

```env
VITE_API_URL=http://localhost:5000/api
VITE_APP_NAME=BarbApp Admin
```

### ESLint

O projeto utiliza ESLint para manter a qualidade do código. Para executar:

```bash
npm run lint
```

### Prettier

Para formatar o código:

```bash
npm run format
```

## Contribuição

1. Siga os padrões de código definidos no `rules/code-standard.md`
2. Mantenha os commits seguindo o padrão em `rules/git-commit.md`
3. Execute os testes antes de fazer commit
4. Atualize a documentação quando necessário
