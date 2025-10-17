using System.Security.Cryptography;
using BarbApp.Application.Interfaces;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Implementação de gerador de senhas aleatórias criptograficamente seguras.
/// Utiliza RandomNumberGenerator para garantir entropia adequada.
/// </summary>
public class SecurePasswordGenerator : IPasswordGenerator
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string SpecialChars = "!@#$%&*-_+=";

    /// <summary>
    /// Gera uma senha aleatória segura com o comprimento especificado.
    /// Garante pelo menos 1 caractere de cada tipo (maiúscula, minúscula, dígito, símbolo).
    /// Utiliza algoritmo de embaralhamento (Fisher-Yates) para evitar padrões previsíveis.
    /// </summary>
    /// <param name="length">Comprimento da senha (mínimo 8, padrão 12).</param>
    /// <returns>Senha gerada de forma criptograficamente segura.</returns>
    /// <exception cref="ArgumentException">Lançada quando length é menor que 8.</exception>
    public string Generate(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));

        var allChars = UpperCase + LowerCase + Digits + SpecialChars;
        var password = new char[length];

        using var rng = RandomNumberGenerator.Create();

        // Garantir pelo menos 1 de cada tipo (primeiros 4 caracteres)
        password[0] = GetRandomChar(UpperCase, rng);
        password[1] = GetRandomChar(LowerCase, rng);
        password[2] = GetRandomChar(Digits, rng);
        password[3] = GetRandomChar(SpecialChars, rng);

        // Preencher restante aleatoriamente de qualquer tipo
        for (int i = 4; i < length; i++)
        {
            password[i] = GetRandomChar(allChars, rng);
        }

        // Embaralhar usando Fisher-Yates para não ter padrão previsível
        Shuffle(password, rng);

        return new string(password);
    }

    /// <summary>
    /// Obtém um caractere aleatório de uma string usando RandomNumberGenerator.
    /// </summary>
    /// <param name="chars">String contendo os caracteres disponíveis.</param>
    /// <param name="rng">Instância de RandomNumberGenerator.</param>
    /// <returns>Caractere aleatório selecionado.</returns>
    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var data = new byte[1];
        rng.GetBytes(data);
        return chars[data[0] % chars.Length];
    }

    /// <summary>
    /// Embaralha um array de caracteres usando o algoritmo Fisher-Yates
    /// com RandomNumberGenerator para segurança criptográfica.
    /// </summary>
    /// <param name="array">Array de caracteres a ser embaralhado.</param>
    /// <param name="rng">Instância de RandomNumberGenerator.</param>
    private static void Shuffle(char[] array, RandomNumberGenerator rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            var data = new byte[1];
            rng.GetBytes(data);
            int j = data[0] % (i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
