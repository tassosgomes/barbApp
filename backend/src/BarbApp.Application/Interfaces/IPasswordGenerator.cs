namespace BarbApp.Application.Interfaces;

/// <summary>
/// Interface para geração de senhas aleatórias seguras.
/// </summary>
public interface IPasswordGenerator
{
    /// <summary>
    /// Gera uma senha aleatória segura com o comprimento especificado.
    /// </summary>
    /// <param name="length">Comprimento da senha (mínimo 8, padrão 12).</param>
    /// <returns>Senha gerada contendo maiúsculas, minúsculas, números e símbolos.</returns>
    /// <exception cref="ArgumentException">Lançada quando length é menor que 8.</exception>
    string Generate(int length = 12);
}
