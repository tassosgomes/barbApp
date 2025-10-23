# barbapp-public

Landing page pública para barbearias - parte do sistema BarbApp.

## 📋 Descrição

Este é o frontend público que exibe as landing pages personalizáveis das barbearias. Cada barbearia pode escolher entre diferentes templates e personalizar informações para criar sua página pública de divulgação e agendamento.

## 🚀 Tecnologias

- **React 19** - Biblioteca UI
- **TypeScript** - Tipagem estática
- **Vite** - Build tool e dev server
- **Tailwind CSS** - Framework CSS utility-first
- **React Router DOM** - Roteamento
- **TanStack Query (React Query)** - Gerenciamento de estado server
- **Axios** - Cliente HTTP
- **Lucide React** - Ícones

## 📁 Estrutura do Projeto

```
src/
├── components/       # Componentes reutilizáveis
├── hooks/           # Hooks customizados
├── pages/           # Páginas/rotas
├── services/        # Serviços de API
├── templates/       # Templates de landing page (1-5)
├── types/           # Definições TypeScript
├── App.tsx          # Componente raiz
├── main.tsx         # Entry point
└── index.css        # Estilos globais (Tailwind)
```

## 🎨 Templates Disponíveis

1. **Clássico** - Elegante e tradicional (preto, dourado, branco)
2. **Moderno** - Limpo e minimalista (cinza escuro, azul elétrico)
3. **Vintage** - Retrô anos 50/60 (marrom, vermelho escuro, creme)
4. **Urbano** - Street/Hip-hop (preto, vermelho vibrante)
5. **Premium** - Luxuoso e sofisticado (preto, dourado metálico)

## ⚙️ Configuração

### Pré-requisitos

- Node.js >= 18
- npm ou yarn

### Instalação

```bash
# Instalar dependências
npm install

# Copiar variáveis de ambiente
cp .env.example .env
```

### Variáveis de Ambiente

```env
VITE_API_URL=http://localhost:5000/api
```

## 🏃 Executando o Projeto

### Desenvolvimento

```bash
npm run dev
```

O servidor estará disponível em `http://localhost:3001`

### Build para Produção

```bash
npm run build
```

Os arquivos otimizados serão gerados em `dist/`

### Preview da Build

```bash
npm run preview
```

## 🧪 Testes

```bash
# Executar testes
npm test

# Executar testes em modo watch
npm run test:watch

# Coverage
npm run test:coverage
```

## 📦 Build e Deploy

### Build de Produção

```bash
npm run build
```

### Deploy

O projeto pode ser deployado em qualquer serviço de hospedagem estática:

- **Vercel**: `vercel deploy`
- **Netlify**: `netlify deploy`
- **AWS S3 + CloudFront**
- **Azure Static Web Apps**

### Configuração de Proxy/Redirecionamento

Para produção, configure o proxy da API no servidor web ou use CORS adequadamente.

## 🔗 Rotas

- `/barbearia/:code` - Landing page pública da barbearia
- `/barbearia/:code/agendar` - Fluxo de agendamento (integração com barbapp-client)

## 🎯 Funcionalidades

### Landing Page Pública

- Exibição de informações da barbearia
- Lista de serviços com preços
- Seleção múltipla de serviços
- Botão WhatsApp flutuante
- Link para agendamento
- Seções: Hero, Serviços, Sobre, Contato, Redes Sociais
- Totalmente responsivo (mobile-first)

### Integração com API

- Carregamento de configuração da landing page
- Cache de 5 minutos (TanStack Query)
- Tratamento de erros
- Estados de loading

## 🛠️ Desenvolvimento

### Padrões de Código

Siga as regras definidas em `/rules`:
- `code-standard.md` - Padrões gerais de código
- `react.md` - Padrões específicos React
- `git-commit.md` - Padrões de commit

### Componentes

```typescript
// Componentes funcionais com TypeScript
export const ServiceCard: React.FC<ServiceCardProps> = ({ service }) => {
  // Lógica do componente
  return <div>...</div>;
};
```

### Hooks Customizados

```typescript
// Hooks começam com "use"
export const useLandingPageData = (code: string) => {
  return useQuery({
    queryKey: ['publicLandingPage', code],
    queryFn: () => fetchLandingPage(code),
  });
};
```

### Estilização

Use **Tailwind CSS** para estilização:

```tsx
<div className="flex items-center gap-4 p-6 bg-white rounded-lg shadow-md">
  <h1 className="text-2xl font-bold text-gray-900">Título</h1>
</div>
```

## 🎨 Customização do Tailwind

### Cores Personalizadas

```javascript
// tailwind.config.js
colors: {
  gold: '#D4AF37',
  'gold-dark': '#B8941E',
}
```

### Fontes

- **Serif**: Playfair Display (títulos)
- **Sans**: Inter (corpo do texto)

## 🐛 Debug

### Erros Comuns

**1. Erro de CORS**
- Verifique se o backend está configurado para aceitar requisições de `http://localhost:3001`

**2. Página não encontrada (404)**
- Verifique se o código da barbearia existe
- Verifique se a landing page está publicada

**3. Estilos não carregando**
- Limpe o cache do navegador
- Reconstrua o projeto: `npm run build`

## 📚 Recursos Adicionais

- [Documentação React](https://react.dev)
- [Documentação Vite](https://vitejs.dev)
- [Documentação Tailwind CSS](https://tailwindcss.com)
- [TanStack Query](https://tanstack.com/query)
- [React Router](https://reactrouter.com)

## 📝 Licença

Propriedade de BarbApp - Todos os direitos reservados.

## 👥 Equipe

Desenvolvido pela equipe BarbApp

---

**Versão**: 0.0.0
**Última Atualização**: 2025-10-23
