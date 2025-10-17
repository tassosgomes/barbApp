namespace BarbApp.Application.Configuration;

/// <summary>
/// Configurações gerais da aplicação.
/// </summary>
public class AppSettings
{
    /// <summary>
    /// URL base do frontend para construção de links em e-mails.
    /// Exemplo: "https://barbapp.tasso.dev.br" (produção) ou "https://dev-barbapp.tasso.dev.br" (desenvolvimento).
    /// </summary>
    public string FrontendUrl { get; set; } = string.Empty;
}