Checklist de Code Review para Projetos dotnet
Testes Unitários e de Integração:

Execute o comando dotnet test. Todos os testes existentes devem passar.

Verifique se a cobertura de código (code coverage), medida com ferramentas como o Coverlet, atende ao mínimo definido para o projeto.

Análise Estática e Formatação:

Garanta que o código está formatado de acordo com as regras do arquivo .editorconfig. Se necessário, rode o comando dotnet format.

Verifique se não há avisos (warnings) ou erros gerados pelos Analisadores Roslyn (ex: StyleCop).

Qualidade do Código e Boas Práticas:

Analise se o código adere aos princípios de design (ex: SOLID) e às boas práticas da linguagem C#.

Procure por código comentado ou desnecessário e remova-o.

Valide se não há valores "hardcoded". Constantes devem usar const/static readonly e configurações externas devem estar no appsettings.json e serem acessadas via IConfiguration.

Limpeza e Clareza:

Remova todas as diretivas using não utilizadas.

Elimine quaisquer variáveis declaradas que não são usadas.

Busque oportunidades de refatoração para tornar o código mais legível, simples e de fácil manutenção.