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
├── components/        # Componentes reutilizáveis
│   ├── ui/           # Componentes de UI (shadcn/ui)
│   └── ...           # Modais, formulários, tabelas
├── pages/            # Páginas da aplicação
│   ├── AdminCentral/ # Interface Admin Central
│   │   ├── Barbearias/     # Gestão de barbearias
│   │   └── Dashboard/      # Dashboard Admin Central
│   └── AdminBarbearia/     # Interface Admin Barbearia
│       ├── Barbeiros/      # Gestão de barbeiros
│       ├── Servicos/       # Gestão de serviços
│       ├── Agenda/         # Visualização de agendamentos
│       └── Dashboard/      # Dashboard Admin Barbearia
├── hooks/            # Hooks customizados
├── lib/              # Utilitários e configurações
├── services/         # Serviços de API
├── types/            # Definições de tipos TypeScript
└── styles/           # Estilos globais

tests/
├── e2e/              # Testes End-to-End (Playwright)
│   ├── helpers/      # Helpers compartilhados
│   └── admin-barbearia/ # Testes da interface Admin Barbearia
└── unit/             # Testes unitários (Vitest)
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

## Interfaces Administrativas

### Admin Central
Interface para gestão centralizada de todas as barbearias do sistema.

**Acesso:** `http://localhost:3001/admin-central`

**Funcionalidades:**
- Gestão de barbearias (criar, editar, visualizar)
- Dashboard com métricas gerais
- Controle de acesso centralizado

### Admin Barbearia
Interface específica para administradores de cada barbearia gerenciar seu estabelecimento.

**Acesso:** `http://localhost:3001/{codigo-barbearia}`

**Autenticação:**
1. Acesse com o código único da barbearia (ex: `TEST1234`)
2. Faça login com email e senha do administrador
3. O sistema valida automaticamente o tenant (multi-tenant)

**Funcionalidades:**
- Gestão de barbeiros (criar, editar, desativar)
- Gestão de serviços (criar, editar, desativar, definir preços/duração)
- Visualização de agenda (filtros por barbeiro, data, status)
- Dashboard com métricas da barbearia

**Rotas Principais:**
```
/{codigo}/login              # Login do Admin Barbearia
/{codigo}/dashboard          # Dashboard
/{codigo}/barbeiros          # Gestão de barbeiros
/{codigo}/servicos           # Gestão de serviços
/{codigo}/agenda             # Visualização de agendamentos
```

## Testes

### Unitários
Execute testes unitários com Vitest:

```bash
# Executar todos os testes
npm run test

# Modo watch
npm run test:watch

# Com interface visual
npm run test:ui

# Com coverage
npm run test:coverage
```

### E2E (End-to-End)
Execute testes E2E com Playwright:

```bash
# Executar todos os testes E2E
npm run test:e2e

# Modo UI (recomendado para desenvolvimento)
npm run test:e2e:ui

# Rodar em modo debug
npm run test:e2e:debug

# Gerar relatório HTML
npm run test:e2e:report
```

**Cobertura de Testes E2E - Admin Barbearia:**

1. **Autenticação** (`01-auth.spec.ts`)
   - Validação de código da barbearia
   - Login com credenciais
   - Persistência de autenticação
   - Redirecionamento para rotas protegidas

2. **Casos de Erro** (`02-error-cases.spec.ts`)
   - Código de barbearia inválido
   - Credenciais inválidas
   - Validação de email
   - Bloqueio de rotas protegidas
   - Isolamento multi-tenant

3. **Gestão de Barbeiros** (`03-barbeiros.spec.ts`)
   - Listar barbeiros existentes
   - Criar novo barbeiro
   - Editar barbeiro
   - Desativar barbeiro
   - Filtrar por nome
   - Validações de campos obrigatórios

4. **Gestão de Serviços** (`04-servicos.spec.ts`)
   - Listar serviços existentes
   - Criar novo serviço
   - Editar serviço
   - Desativar serviço
   - Validações de preço e duração
   - Formatação de moeda e tempo

5. **Visualização de Agenda** (`05-agenda.spec.ts`)
   - Listar agendamentos
   - Filtrar por barbeiro, data, status
   - Visualizar detalhes do agendamento
   - Formatação de data/hora
   - Status com badges coloridos

6. **Fluxo Completo** (`06-complete-flow.spec.ts`)
   - Login → Criar Barbeiro → Criar Serviço → Visualizar Agenda → Logout
   - Validação end-to-end de toda jornada do usuário

**Credenciais de Teste:**
```typescript
Código: TEST1234
Email: admin@test.com
Senha: Test@123
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
