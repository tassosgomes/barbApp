# Auditoria de Segurança: Segredos Codificados

## 1. Resumo Executivo

Uma auditoria de segurança do repositório BarbApp foi realizada para identificar segredos codificados (hardcoded secrets). A auditoria revelou várias ocorrências de informações sensíveis, incluindo credenciais de banco de dados, segredos JWT e senhas de aplicação, armazenadas em texto simples dentro do código-fonte. Essa prática apresenta um risco de segurança significativo, pois expõe a aplicação e seus dados a acesso não autorizado.

Este relatório detalha as descobertas da auditoria e fornece recomendações para remediar essas vulnerabilidades.

## 2. Resultados

Segredos codificados foram encontrados nos seguintes arquivos:

| Arquivo | Localização | Segredo(s) |
|---|---|---|
| `backend/docker-compose.yml` | `/home/tsgomes/github-tassosgomes/barbApp/backend/docker-compose.yml` | - `ConnectionStrings__DefaultConnection`: senha do banco de dados `4cd7e2e32c5994fcba50555430efe763`<br>- `JwtSettings__Secret`: segredo JWT `c3df06950022492321151d5fdff463d351350166a50d3f94d4a86daa363d52e0`<br>- `POSTGRES_PASSWORD`: senha do banco de dados `4cd7e2e32c5994fcba50555430efe763` |
| `backend/src/BarbApp.API/appsettings.json` | `/home/tsgomes/github-tassosgomes/barbApp/backend/src/BarbApp.API/appsettings.json` | - `ConnectionStrings.DefaultConnection`: credenciais do banco de dados `Username=barbapp_user`, `Password=barbapp_password` |
| `README.md` | `/home/tsgomes/github-tassosgomes/barbApp/README.md` | - `BARBAPP_CONNECTION_STRING`: credenciais do banco de dados `Username=postgres`, `Password=password` |
| `backend/scripts/test-e2e-flows.sh` | `/home/tsgomes/github-tassosgomes/barbApp/backend/scripts/test-e2e-flows.sh` | - Credenciais de administrador: `admin@barbapp.com`/`Admin@123`<br>- Credenciais de administrador da barbearia: `admin@barbearia.com`/`Admin@123` |
| `backend/tests/BarbApp.IntegrationTests/DatabaseFixture.cs` | `/home/tsgomes/github-tassosgomes/barbApp/backend/tests/BarbApp.IntegrationTests/DatabaseFixture.cs` | - Credenciais do banco de dados de teste: `test_user`/`test_password` |
| `backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | `/home/tsgomes/github-tassosgomes/barbApp/backend/tests/BarbApp.IntegrationTests/IntegrationTestWebAppFactory.cs` | - Segredo JWT de teste: `test-secret-key-at-least-32-characters-long-for-jwt`<br>- Senha SMTP de teste: `test` |
| `docs/environment-variables.md` | `/home/tsgomes/github-tassosgomes/barbApp/docs/environment-variables.md` | - Exemplos de credenciais do banco de dados: `postgres`/`postgres`<br>- Exemplo de segredo JWT: `FTVBUj5qEbI03He2NGDJjxZRsxvxmRwCr7EYZIINZSA=` |

## 3. Recomendações

Recomenda-se fortemente remover todos os segredos codificados do código-fonte e utilizar um método seguro para gerenciá-los. As seguintes ações devem ser tomadas:

1. **Usar Variáveis de Ambiente:** Para desenvolvimento local, armazene segredos em arquivos `.env`. O arquivo `.gitignore` existente já previne que esses arquivos sejam versionados no repositório. Para produção, use variáveis de ambiente fornecidas pela plataforma de hospedagem (por exemplo, Docker, Azure App Service).

2. **Utilizar um Serviço de Gerenciamento de Segredos:** Para ambientes de produção, a melhor prática é usar um serviço dedicado de gerenciamento de segredos, como [Azure Key Vault](https://azure.microsoft.com/pt-br/products/key-vault), [AWS Secrets Manager](https://aws.amazon.com/secrets-manager/) ou [HashiCorp Vault](https://www.vaultproject.io/). Esses serviços fornecem uma forma segura de armazenar, gerenciar e rotacionar segredos.

3. **Atualizar a Documentação:** O arquivo `docs/environment-variables.md` deve ser atualizado para remover quaisquer exemplos de segredos e para fornecer instruções claras sobre como configurar o ambiente usando variáveis de ambiente ou um serviço de gerenciamento de segredos.

4. **Rotacionar Segredos Expostos:** Como os segredos foram expostos no repositório, eles devem ser considerados comprometidos. Todas as credenciais expostas devem ser rotacionadas imediatamente. Isso inclui senhas de banco de dados, segredos JWT e quaisquer outras informações sensíveis.

## 4. Conclusão

A presença de segredos codificados no repositório BarbApp é uma vulnerabilidade de segurança crítica que requer atenção imediata. Ao seguir as recomendações descritas neste relatório, a segurança da aplicação pode ser significativamente melhorada.
