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
}