### **Diretrizes para Testes em Projetos React**

Este documento estabelece as diretrizes e melhores práticas para a escrita de testes em aplicações React, com o objetivo de garantir a qualidade, manutenibilidade e confiabilidade do código.

### **Ferramentas**

A suíte de testes padrão para aplicações React modernas consiste na combinação das seguintes ferramentas:

  * **Test Runner e Framework de Teste**: Utilize o **Jest** como principal framework. Ele é o padrão de mercado para aplicações React e inclui um test runner, uma biblioteca de asserções e funcionalidades de mocking integradas, oferecendo uma experiência de teste completa e com configuração mínima.
  * **Biblioteca de Testes para Componentes**: Utilize a **React Testing Library (RTL)** para renderizar e interagir com os componentes React. A filosofia da RTL é testar os componentes da mesma forma que o usuário os utiliza, o que leva a testes mais resilientes a refatorações.
  * **Simulação de Interações do Usuário**: Para simular eventos do usuário de forma mais realista (digitação, cliques, etc.), utilize a biblioteca **`@testing-library/user-event`**, que é construída sobre a RTL.
  * **Execução dos Testes**: Utilize o comando padrão do seu gerenciador de pacotes para rodar os testes. Normalmente, ele já vem configurado em projetos criados com `create-react-app` ou Vite:
      * `npm test`
      * ou `yarn test`

### **Estrutura e Organização**

  * **Localização dos Arquivos de Teste**: Crie os arquivos de teste próximos aos arquivos de produção. Para um componente `MeuComponente.jsx` localizado em `src/components/`, o arquivo de teste deve ser `MeuComponente.test.jsx` na mesma pasta. Isso melhora a localizabilidade e incentiva os testes.

  * **Nomeclatura**:

      * **Arquivos**: Use o sufixo `.test.js` ou `.test.jsx` (ou `.ts`/`.tsx` para TypeScript). Ex: `useCustomHook.test.js`, `ProductPage.test.jsx`.
      * **Blocos de Teste**: Use `describe` para agrupar testes relacionados a um componente ou função. Ex: `describe('<LoginComponent />', () => { ... });`.

  * **Separação dos Tipos de Teste**:

      * **Testes Unitários e de Integração**: A distinção pode ser fluida em React. A abordagem da React Testing Library naturalmente mistura os dois. Separe os arquivos de teste por componente ou hook.
      * **Testes End-to-End (E2E)**: Para testes E2E, que validam fluxos completos da aplicação em um navegador real, utilize um framework dedicado como o **Cypress** ou **Playwright**. Esses testes devem ficar em uma pasta separada, como `cypress/e2e/`.

  * **Padrão de Escrita**: Siga estritamente o padrão **Arrange, Act, Assert (AAA)** ou **Given, When, Then** para estruturar o corpo de cada teste. Isso torna o teste mais legível e fácil de entender.

    ```javascript
    import { render, screen } from '@testing-library/react';
    import userEvent from '@testing-library/user-event';
    import MyComponent from './MyComponent';

    test('deve exibir mensagem de sucesso ao submeter o formulário', async () => {
      // Arrange (Organizar)
      render(<MyComponent />);
      const inputElement = screen.getByLabelText('Nome');
      const buttonElement = screen.getByRole('button', { name: /enviar/i });

      // Act (Agir)
      await userEvent.type(inputElement, 'John Doe');
      await userEvent.click(buttonElement);

      // Assert (Afirmar)
      const successMessage = await screen.findByText(/formulário enviado com sucesso/i);
      expect(successMessage).toBeInTheDocument();
    });
    ```

### **Princípios de Teste**

  * **Isolamento**: Cada teste deve ser independente e executável sem depender de outros. O Jest garante isso por padrão, executando cada arquivo de teste em seu próprio processo. Evite criar estado compartilhado entre testes.
  * **Repetibilidade**: Testes devem produzir o mesmo resultado sempre que executados. Para dependências externas como a data atual (`new Date()`) ou APIs, utilize os recursos de *mocking* do Jest para controlá-las.
  * **Foco**: Teste um único comportamento por método de teste. A convenção de nomenclatura pode ser descritiva, usando o bloco `test` (ou `it`) para descrever o cenário e o comportamento esperado. Ex: `test('should display an error message when the API call fails')`.
  * **Asserções Claras**: Escreva asserções explícitas e legíveis. O Jest vem com a biblioteca de asserções `expect` e a **`@testing-library/jest-dom`** a estende com *matchers* focados no DOM, tornando as expectativas mais claras e declarativas.
      * `expect(element).toBeInTheDocument();`
      * `expect(button).toBeDisabled();`
      * `expect(element).toHaveTextContent('Texto esperado');`

### **Estratégia de Testes por Camada**

  * **Hooks (Lógica de Negócio/Estado)**: Crie testes de unidade para todos os custom hooks. Teste exaustivamente a lógica, as mudanças de estado e os efeitos colaterais, sem depender de um componente específico. Utilize a função `renderHook` da React Testing Library para isso.
  * **Componentes (Camada de UI)**: Crie testes para todos os componentes.
      * **Testes de Renderização**: Verifique se o componente renderiza corretamente com diferentes props.
      * **Testes de Interação**: Simule interações do usuário (cliques, digitação) e verifique se o estado do componente e a UI reagem como esperado.
      * Utilize *mocks* para isolar dependências externas (chamadas de API, hooks complexos, etc.) e focar no comportamento do componente.
  * **Páginas / Fluxos de Usuário (Testes de Integração)**: Crie testes que renderizam uma página ou um fluxo completo de componentes. O objetivo é validar a integração entre múltiplos componentes, serviços e o estado da aplicação. Utilize a **Mock Service Worker (MSW)** para interceptar e simular requisições de API a nível de rede, tornando esses testes mais robustos.
  * **API/Endpoints Layer**: No contexto do frontend, isso se traduz em testar a integração com a API. Os testes de integração com MSW são a abordagem recomendada, garantindo que sua aplicação sabe como lidar com as respostas e erros da API. Testes E2E com Cypress/Playwright oferecem uma camada adicional de confiança ao testar contra um backend real (em ambiente de teste).

### **Qualidade e Manutenção**

  * **Cobertura de Código (Code Coverage)**: Busque uma alta cobertura de código, mas foque na qualidade em vez de apenas no número. Use a flag `--coverage` do Jest para gerar relatórios e identificar áreas não testadas da sua aplicação.

  * **Setup e Teardown**:

      * Para inicializações comuns a todos os testes de um arquivo, utilize as funções `beforeEach` (executa antes de cada teste) e `afterEach` (executa após cada teste).
      * Use `beforeAll` e `afterAll` para setup/teardown que precisam ocorrer apenas uma vez por arquivo.
      * Isso é ideal para limpar mocks (`jest.clearAllMocks()`) ou reiniciar servidores de mock (como o MSW) entre os testes.

    <!-- end list -->

    ```javascript
    // Exemplo de setup e teardown com MSW
    import { server } from './mocks/server.js';

    beforeAll(() => server.listen()); // Inicia o servidor de mock antes de todos os testes
    afterEach(() => server.resetHandlers()); // Reseta os handlers após cada teste
    afterAll(() => server.close()); // Fecha o servidor após todos os testes
    ```