using BarbApp.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace BarbApp.Infrastructure.Services;

/// <summary>
/// Implementação do processador de imagens usando SixLabors.ImageSharp
/// </summary>
public class ImageSharpProcessor : IImageProcessor
{
    /// <inheritdoc />
    public async Task ProcessAndSaveImageAsync(Stream inputStream, string outputPath, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(inputStream, cancellationToken);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(300, 300),
            Mode = ResizeMode.Crop
        }));

        await image.SaveAsync(outputPath, cancellationToken);
    }
}