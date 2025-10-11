Diretrizes de Logging para dotnet/asp.net
Utilize os Níveis de Log Adequadamente: Faça uso correto dos níveis de log (Trace, Debug, Information, Warning, Error, Critical) fornecidos pela abstração Microsoft.Extensions.Logging para categorizar a severidade e a importância de cada registro.

Desacople a Aplicação do Destino do Log: Configure provedores de log ("sinks") para direcionar os logs para destinos apropriados, como o Console (saída padrão), plataformas de log centralizado (Seq, Datadog, etc.) ou serviços de nuvem (Application Insights). Evite que a aplicação gerencie diretamente arquivos de log.

Nunca Registre Dados Sensíveis: Jamais inclua informações sensíveis (PII - Personally Identifiable Information), como nomes, documentos, endereços ou dados financeiros nos logs, para cumprir com as normas de segurança e privacidade.

Adote Logging Estruturado: Escreva mensagens de log claras e concisas utilizando templates de mensagem. Isso permite que os coletores de log capturem os dados variáveis como campos estruturados, facilitando a filtragem e a análise.

Exemplo: _logger.LogInformation("Produto {ProductId} adicionado ao carrinho {CartId}", productId, cartId);

Utilize a Abstração ILogger: Nunca escreva logs diretamente com Console.WriteLine(). Em vez disso, injete a interface ILogger<T> nos seus serviços e controllers para registrar logs.

Sempre Registre Exceções Capturadas: Nunca silencie uma exceção com um bloco catch vazio. No mínimo, registre a exceção completa (com seu stack trace) utilizando _logger.LogError(ex, "Mensagem de contexto").