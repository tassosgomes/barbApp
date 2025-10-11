Diretrizes para APIs REST/HTTP em dotnet/asp.net
Mapeamento de Endpoints

Utilize ASP.NET Core MVC (Controllers) ou Minimal APIs para mapear os endpoints.

Padrões de Roteamento e Nomenclatura

Utilize o padrão REST para consultas, mantendo o nome dos recursos em inglês e no plural. Permita a navegabilidade em recursos aninhados. Exemplo: /playlists/{playlistId}/videos ou /customers/{customerId}/invoices.

Para as URLs dos recursos, prefira o padrão kebab-case para melhor legibilidade. Exemplo: /scheduled-events. Configure o roteamento do ASP.NET Core para seguir este padrão.

Evite criar endpoints com mais de 3 níveis de aninhamento de recursos.

Tratamento de Mutações (Operações de Escrita)

Para ações que não se encaixam claramente no modelo CRUD (Create, Read, Update, Delete), utilize o verbo POST com URLs que descrevam a ação (estilo RPC). Exemplo: POST /users/{userId}/change-password em vez de PUT /users/{userId} com um payload complexo.

Formato de Dados e Segurança

O formato do payload de requisição e resposta deve ser sempre JSON.

Sempre valide a autenticação (quem o usuário é) e a autorização (o que o usuário pode fazer) em todos os endpoints que requerem proteção, utilizando os middlewares do ASP.NET Core.

Códigos de Status de Retorno

200 OK: Sucesso na requisição.

201 Created: Recurso criado com sucesso (usar em conjunto com o header Location).

204 No Content: Sucesso, mas sem conteúdo para retornar (comum em operações de DELETE).

400 Bad Request: A requisição está mal formatada (ex: JSON inválido, parâmetros faltando).

401 Unauthorized: O usuário não está autenticado.

403 Forbidden: O usuário está autenticado, mas não tem permissão para acessar o recurso.

404 Not Found: O recurso solicitado não foi encontrado.

422 Unprocessable Entity: A requisição estava bem formatada, mas contém erros de negócio (ex: e-mail já cadastrado).

500 Internal Server Error: Erro inesperado no servidor.

Documentação e Features Avançadas

Documente todos os endpoints, métodos e códigos de status utilizando o padrão OpenAPI. Utilize bibliotecas como Swashbuckle para gerar a documentação automaticamente.

Implemente paginação baseada em limit e offset (ou page e pageSize) passados via query string para consultas que retornam listas de recursos.

Considere implementar partial responses para consultas que podem retornar grandes volumes de dados, permitindo que o cliente especifique os campos desejados (ex: via query string ?fields=id,name).

Comunicação com APIs Externas

Utilize a IHttpClientFactory e a classe HttpClient para realizar chamadas a APIs externas de forma segura e eficiente.