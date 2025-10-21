using BarbApp.Application.Interfaces;
using BarbApp.Domain.Common;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SixLabors.ImageSharp;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

public class LocalLogoUploadServiceTests : IDisposable
{
    private readonly Mock<IWebHostEnvironment> _environmentMock;
    private readonly Mock<ILogger<LocalLogoUploadService>> _loggerMock;
    private readonly Mock<IImageProcessor> _imageProcessorMock;
    private readonly LocalLogoUploadService _service;
    private readonly string _testUploadsPath;

    public LocalLogoUploadServiceTests()
    {
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<LocalLogoUploadService>>();
        _imageProcessorMock = new Mock<IImageProcessor>();

        // Create a temporary directory for testing
        _testUploadsPath = Path.Combine(Path.GetTempPath(), "barbapp-test-uploads");
        Directory.CreateDirectory(_testUploadsPath);

        _environmentMock.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());

        _service = new LocalLogoUploadService(_environmentMock.Object, _loggerMock.Object, _imageProcessorMock.Object);
    }

    public void Dispose()
    {
        // Clean up test directory
        if (Directory.Exists(_testUploadsPath))
        {
            Directory.Delete(_testUploadsPath, true);
        }
    }

    [Fact]
    public async Task UploadLogoAsync_ValidJpgFile_ShouldReturnSuccessWithUrl()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

        // Mock image processing
        _imageProcessorMock.Setup(x => x.ProcessAndSaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().StartWith("/uploads/logos/");
        result.Data.Should().Contain(barbershopId.ToString());
        result.Data.Should().EndWith(".jpg");

        // Verify image processing was called
        _imageProcessorMock.Verify(x => x.ProcessAndSaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadLogoAsync_ValidPngFile_ShouldReturnSuccessWithUrl()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("test.png", "image/png", 2048);

        // Mock image processing
        _imageProcessorMock.Setup(x => x.ProcessAndSaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().EndWith(".png");

        // Verify image processing was called
        _imageProcessorMock.Verify(x => x.ProcessAndSaveImageAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadLogoAsync_ValidSvgFile_ShouldReturnSuccessWithUrl()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("test.svg", "image/svg+xml", 1024); // 1KB SVG

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().EndWith(".svg");
    }

    [Fact]
    public async Task UploadLogoAsync_FileTooLarge_ShouldReturnFailure()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("large.jpg", "image/jpeg", 3 * 1024 * 1024); // 3MB (too large)

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Arquivo inválido");
    }

    [Fact]
    public async Task UploadLogoAsync_InvalidFileType_ShouldReturnFailure()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("test.txt", "text/plain", 1024); // Invalid type

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Arquivo inválido");
    }

    [Fact]
    public async Task UploadLogoAsync_NullFile_ShouldReturnFailure()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, null!, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Arquivo inválido");
    }

    [Fact]
    public async Task UploadLogoAsync_EmptyFile_ShouldReturnFailure()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var file = CreateMockFormFile("empty.jpg", "image/jpeg", 0); // Empty file

        // Act
        var result = await _service.UploadLogoAsync(barbershopId, file, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Arquivo inválido");
    }

    [Fact]
    public async Task DeleteLogoAsync_ExistingFile_ShouldReturnSuccess()
    {
        // Arrange
        var logoUrl = "/uploads/logos/test.jpg";
        var filePath = Path.Combine(_environmentMock.Object.WebRootPath, logoUrl.TrimStart('/'));
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.WriteAllTextAsync(filePath, "test content");

        // Act
        var result = await _service.DeleteLogoAsync(logoUrl, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteLogoAsync_NonExistingFile_ShouldReturnSuccess()
    {
        // Arrange
        var logoUrl = "/uploads/logos/nonexistent.jpg";

        // Act
        var result = await _service.DeleteLogoAsync(logoUrl, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteLogoAsync_NullOrEmptyUrl_ShouldReturnSuccess()
    {
        // Act
        var result1 = await _service.DeleteLogoAsync(null!, CancellationToken.None);
        var result2 = await _service.DeleteLogoAsync("", CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageFile_ValidJpg_ShouldReturnTrue()
    {
        // Arrange
        var file = CreateMockFormFile("test.jpg", "image/jpeg", 1024);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageFile_ValidPng_ShouldReturnTrue()
    {
        // Arrange
        var file = CreateMockFormFile("test.png", "image/png", 1024);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageFile_ValidSvg_ShouldReturnTrue()
    {
        // Arrange
        var file = CreateMockFormFile("test.svg", "image/svg+xml", 1024);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidImageFile_FileTooLarge_ShouldReturnFalse()
    {
        // Arrange
        var file = CreateMockFormFile("large.jpg", "image/jpeg", 3 * 1024 * 1024);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_InvalidType_ShouldReturnFalse()
    {
        // Arrange
        var file = CreateMockFormFile("test.txt", "text/plain", 1024);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_NullFile_ShouldReturnFalse()
    {
        // Act
        var result = _service.IsValidImageFile(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidImageFile_EmptyFile_ShouldReturnFalse()
    {
        // Arrange
        var file = CreateMockFormFile("empty.jpg", "image/jpeg", 0);

        // Act
        var result = _service.IsValidImageFile(file);

        // Assert
        result.Should().BeFalse();
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var stream = new MemoryStream();
        if (length > 0)
        {
            var content = new byte[length];
            Random.Shared.NextBytes(content);
            stream.Write(content);
            stream.Position = 0;
        }

        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(length);
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((s, ct) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(s, ct);
            });

        return file.Object;
    }

    private static IFormFile CreateMockFormFileWithValidImage(string fileName, string contentType, byte[] imageData)
    {
        var stream = new MemoryStream(imageData);

        var file = new Mock<IFormFile>();
        file.Setup(f => f.FileName).Returns(fileName);
        file.Setup(f => f.Length).Returns(imageData.Length);
        file.Setup(f => f.ContentType).Returns(contentType);
        file.Setup(f => f.OpenReadStream()).Returns(stream);
        file.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((s, ct) =>
            {
                stream.Position = 0;
                return stream.CopyToAsync(s, ct);
            });

        return file.Object;
    }

    private static byte[] CreateValidJpgData()
    {
        // Minimal valid JPG data (1x1 pixel black image)
        return new byte[]
        {
            0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01, 0x01, 0x01, 0x00, 0x48,
            0x00, 0x48, 0x00, 0x00, 0xFF, 0xC0, 0x00, 0x11, 0x08, 0x00, 0x01, 0x00, 0x01, 0x01, 0x01, 0x11,
            0x00, 0x02, 0x11, 0x01, 0x03, 0x11, 0x01, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0xFF, 0xC4, 0x00,
            0x14, 0x10, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0xFF, 0xDA, 0x00, 0x0C, 0x03, 0x01, 0x00, 0x02, 0x11, 0x03, 0x11, 0x00, 0x3F,
            0x00, 0x00, 0xFF, 0xD9
        };
    }

    private static byte[] CreateValidPngData()
    {
        // Minimal valid PNG data (1x1 pixel)
        return new byte[]
        {
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01, 0x08, 0x02, 0x00, 0x00, 0x00, 0x90, 0x77, 0x53,
            0xDE, 0x00, 0x00, 0x00, 0x0C, 0x49, 0x44, 0x41, 0x54, 0x08, 0x99, 0x01, 0x01, 0x00, 0x00, 0x00,
            0xFF, 0xFF, 0x00, 0x00, 0x00, 0x02, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44,
            0xAE, 0x42, 0x60, 0x82
        };
    }
}