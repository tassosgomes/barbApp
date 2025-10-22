using System.IO;

namespace BarbApp.Application.Interfaces;

/// <summary>
/// Interface para processamento de imagens
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Processa e salva uma imagem do stream de entrada para o caminho de saída
    /// </summary>
    Task ProcessAndSaveImageAsync(Stream inputStream, string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processa uma imagem e retorna como stream (para upload em cloud storage)
    /// </summary>
    /// <param name="inputStream">Stream de entrada da imagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Stream da imagem processada (redimensionada)</returns>
    Task<Stream> ProcessImageAsync(Stream inputStream, CancellationToken cancellationToken = default);
}