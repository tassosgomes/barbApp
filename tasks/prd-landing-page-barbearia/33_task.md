---
status: pending
parallelizable: false
blocked_by: ["32.0"]
---

# Tarefa 33.0: Documentação e Preparação para o Deploy

## Visão Geral
Esta é a tarefa final de preparação do projeto para a entrega e manutenção futura. O objetivo é garantir que a documentação esteja completa, que o processo de deploy esteja claro e que todas as configurações de ambiente estejam preparadas.

## Requisitos
- **Documentação Abrangente**: O projeto deve ser fácil de entender e configurar por um novo desenvolvedor.
- **Processo de Deploy Claro**: As etapas para fazer o deploy das aplicações (`barbapp-admin` e `barbapp-public`) em um ambiente de produção devem estar documentadas.
- **Configuração de Ambiente**: As variáveis de ambiente necessárias para rodar o projeto devem estar claramente definidas.

## Detalhes da Implementação

### 1. Documentação

- **`README.md` do Projeto Principal (`/barbApp`)**:
  - Atualizar o `README.md` principal para incluir a nova aplicação `barbapp-public`.
  - Adicionar instruções sobre como rodar os dois frontends e o backend simultaneamente (talvez com um script `npm run dev:all`).

- **`README.md` do `barbapp-admin`**:
  - Adicionar uma seção descrevendo o novo módulo de "Landing Page", explicando brevemente sua funcionalidade e os componentes envolvidos.

- **`README.md` do `barbapp-public` (Criado na Tarefa 21.0)**:
  - **Revisar e Completar**: Garantir que o `README.md` esteja completo.
  - **Seções a Incluir**:
    - **Visão Geral**: Breve descrição do propósito da aplicação.
    - **Stack Tecnológica**: Lista das principais tecnologias usadas (React, Vite, Tailwind, etc.).
    - **Como Rodar Localmente**: Passos detalhados para clonar, instalar dependências e iniciar o servidor de desenvolvimento.
    - **Variáveis de Ambiente**: Descrever cada variável no arquivo `.env.example` e o que ela faz.
    - **Estrutura de Pastas**: Uma visão geral da arquitetura do projeto (ex: `/templates`, `/components`, `/hooks`).
    - **Scripts Disponíveis**: Explicar o que cada script no `package.json` faz (`dev`, `build`, `test`, `lint`).

### 2. Preparação para o Deploy

- **Scripts de Build**:
  - Verificar se o script `npm run build` em ambos os projetos (`barbapp-admin` e `barbapp-public`) está funcionando corretamente, gerando uma pasta `dist` otimizada para produção.

- **Configuração do Servidor Web**:
  - Documentar a configuração necessária para servir as aplicações, que são SPAs (Single-Page Applications). Isso geralmente envolve redirecionar todas as requisições para o `index.html` para que o roteamento do lado do cliente funcione.
  - **Exemplo para Nginx**:
    ```nginx
    server {
      listen 80;
      server_name public.barbapp.com;

      root /var/www/barbapp-public/dist;
      index index.html;

      location / {
        try_files $uri $uri/ /index.html;
      }
    }
    ```

- **Variáveis de Ambiente de Produção**:
  - Garantir que o processo de build e o ambiente de produção saibam como acessar as variáveis de ambiente corretas (ex: `VITE_API_URL` apontando para a API de produção).
  - Documentar como essas variáveis devem ser configuradas no ambiente de deploy (ex: segredos do GitHub Actions, variáveis de ambiente do Vercel/Netlify/AWS).

### 3. Finalização

- **Revisão de Código**: Fazer uma última revisão do código em busca de `console.log`s, comentários desnecessários ou código morto.
- **Linting e Formatação**: Rodar os linters e formatadores (`ESLint`, `Prettier`) em ambos os projetos para garantir a consistência do código.
  - `npm run lint -- --fix`
  - `npm run format`

## Critérios de Aceitação
- [ ] O `README.md` de cada projeto (`admin` e `public`) está completo e atualizado.
- [ ] O processo para rodar o ambiente de desenvolvimento local completo está documentado.
- [ ] As variáveis de ambiente necessárias para produção estão documentadas em um arquivo `.env.example`.
- [ ] As instruções de deploy, incluindo a configuração do servidor para uma SPA, estão documentadas.
- [ ] O código foi limpo (sem logs ou comentários desnecessários).
- [ ] Os scripts de `lint` e `build` rodam sem erros em ambos os projetos.