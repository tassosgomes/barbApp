Diretrizes para Testes em Projetos dotnet
Ferramentas

Utilize o framework xUnit para estruturar os cenários de teste e fazer as asserções.

Para implementar padrões de teste como Mocks, Stubs e Spies, utilize bibliotecas como Moq ou NSubstitute.

Para rodar os testes, utilize o comando padrão da CLI: dotnet test.

Estrutura e Organização

Crie projetos de teste separados na solução (ex: MeuProjeto.Tests) para isolar o código de produção do código de teste.

Nomeie as classes de teste com o sufixo Tests (ex: UserServiceTests.cs).

Separe os testes de unidade e de integração em projetos distintos (ex: MeuProjeto.UnitTests e MeuProjeto.IntegrationTests) ou utilize o atributo [Trait] do xUnit para categorizá-los.

Siga estritamente o padrão Arrange, Act, Assert (AAA) ou Given, When, Then para estruturar o corpo de cada teste.

Princípios de Teste

Isolamento: Não crie dependências entre testes. Cada teste deve ser capaz de rodar de forma independente. O xUnit ajuda a garantir isso ao criar uma nova instância da classe para cada teste.

Repetibilidade: Se um comportamento depende do tempo (DateTime.Now), abstraia-o por trás de uma interface (IClock) ou utilize a classe TimeProvider (.NET 8+) para permitir o controle do tempo nos testes.

Foco: Teste um único comportamento por método de teste. Use a convenção de nomenclatura MetodoTestado_Cenario_ComportamentoEsperado para deixar claro o objetivo do teste.

Asserções Claras: Escreva asserções explícitas e legíveis. Utilize a biblioteca FluentAssertions para tornar as expectativas mais claras (ex: resultado.Should().NotBeNull();).

Estratégia de Testes por Camada

Domain Layer: Crie testes de unidade para todas as entidades e objetos de valor. Teste exaustivamente todas as regras de negócio, variações e invariantes, sem depender de recursos externos.

Application/Use Cases Layer: Crie testes para todos os serviços ou casos de uso. Teste os fluxos principais e os alternativos (exceções). Utilize mocks para isolar dependências externas (banco de dados, APIs, etc.).

API/Endpoints Layer: Crie testes de integração para os endpoints HTTP. Utilize a biblioteca Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory) para fazer requisições à sua API em memória. Foque em validar o fluxo da requisição, status codes, contratos de entrada/saída e cenários de erro principais.

Qualidade e Manutenção

Busque uma alta cobertura de código (code coverage), utilizando ferramentas como o Coverlet para medir e garantir a qualidade da suíte de testes.

Gerencie o ciclo de vida de recursos externos (como contêineres de banco de dados para testes) utilizando o padrão IDisposable e as Fixtures do xUnit.

Utilize o construtor da classe de teste para executar a inicialização (Arrange) que for comum a todos os testes daquela classe.