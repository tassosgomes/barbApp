---
status: pending
parallelizable: false
blocked_by: ["31.0"]
---

# Tarefa 32.0: Otimizações de Performance e SEO

## Visão Geral
Com a funcionalidade completa e integrada, esta tarefa foca em otimizar a performance da landing page pública (`barbapp-public`) e implementar as bases para uma boa indexação em mecanismos de busca (SEO).

## Requisitos de Performance (prd.md Seção 5.8)
- **Tempo de Carregamento**: A landing page deve ter um First Contentful Paint (FCP) inferior a 2 segundos em uma conexão 3G rápida.
- **Pontuação Lighthouse**: Atingir uma pontuação de performance no Google Lighthouse de 90 ou mais para a versão mobile.
- **Otimização de Assets**: Imagens, CSS e JavaScript devem ser minificados e otimizados.

## Requisitos de SEO (prd.md Seção 5.9)
- **Meta Tags Dinâmicas**: A página deve ter tags `<title>` e `<meta name="description">` que sejam preenchidas dinamicamente com as informações da barbearia.
- **Acessibilidade**: Garantir que a página siga as boas práticas de acessibilidade (WCAG AA), o que também contribui para o SEO.

## Detalhes da Implementação

### Otimização de Performance

1.  **Lazy Loading de Imagens e Componentes**:
    - **Imagens**: Todas as imagens, especialmente o logo da barbearia e as imagens de fundo dos templates, devem ser carregadas usando `loading="lazy"`.
    - **Componentes de Template**: Os componentes dos templates (ex: `Template1Classic`, `Template2Modern`, etc.) são candidatos perfeitos para lazy loading, pois o usuário só precisará de um deles por vez. Isso pode ser feito com `React.lazy()` e `Suspense`.

    ```typescript
    // Em pages/LandingPage.tsx
    const Template1 = React.lazy(() => import('@/templates/Template1Classic'));
    const Template2 = React.lazy(() => import('@/templates/Template2Modern'));
    // ...

    <Suspense fallback={<div>Carregando...</div>}>
      <TemplateComponent data={data} />
    </Suspense>
    ```

2.  **Otimização de Imagens**:
    - **Compressão**: Garantir que as imagens dos templates (fundos, etc.) estejam comprimidas (usando ferramentas como TinyPNG) antes de serem adicionadas ao projeto.
    - **Formatos Modernos**: Se possível, usar formatos de imagem modernos como `.webp` que oferecem melhor compressão.
    - **Logo do Usuário**: O backend (Tarefa 8.0) deve redimensionar o logo para um tamanho razoável (ex: 300x300px) no upload, para que não seja servido um arquivo desnecessariamente grande.

3.  **Análise de Bundle**:
    - Usar uma ferramenta como `rollup-plugin-visualizer` para analisar o que está ocupando mais espaço no bundle final do JavaScript e identificar oportunidades de otimização (ex: remover bibliotecas pesadas ou não utilizadas).

### Otimização de SEO

1.  **Server-Side Rendering (SSR) ou Static Site Generation (SSG)**:
    - A abordagem mais eficaz para SEO é pré-renderizar a página no servidor. Dado que o projeto usa Vite + React (CSR - Client-Side Rendering), uma mudança completa para SSR (como Next.js) está fora de escopo. 
    - **Solução de Compromisso (CSR com Head Management)**: Usar uma biblioteca como `react-helmet-async` para gerenciar as meta tags no lado do cliente. Embora não seja tão eficaz quanto o SSR para o primeiro rastreamento dos bots, é uma melhoria significativa.

2.  **Implementação com `react-helmet-async`**:
    - Instalar o pacote: `npm install react-helmet-async`.
    - Envolver a aplicação com o `HelmetProvider`.
    - No componente `LandingPage.tsx`, usar o `Helmet` para definir as tags dinamicamente.

    ```typescript
    // Em pages/LandingPage.tsx
    import { Helmet } from 'react-helmet-async';

    // ... dentro do componente
    const barbershopName = data.barbershop.name;
    const description = data.landingPage.aboutText?.substring(0, 150) || `Agende seu horário na ${barbershopName}`;

    return (
      <>
        <Helmet>
          <title>{`${barbershopName} - Agendamento Online`}</title>
          <meta name="description" content={description} />
        </Helmet>
        <TemplateComponent data={data} />
      </>
    );
    ```

## Critérios de Aceitação
- [ ] A pontuação de Performance no Google Lighthouse para a landing page pública é de 90+ (mobile).
- [ ] As imagens da página utilizam o atributo `loading="lazy"`.
- [ ] Os componentes de template são carregados sob demanda usando `React.lazy()`.
- [ ] O bundle de produção foi analisado e não contém dependências desnecessariamente grandes.
- [ ] A tag `<title>` da página é preenchida dinamicamente com o nome da barbearia.
- [ ] A tag `<meta name="description">` é preenchida dinamicamente com a descrição da barbearia.
- [ ] A página atinge uma pontuação de SEO no Lighthouse de 90+.