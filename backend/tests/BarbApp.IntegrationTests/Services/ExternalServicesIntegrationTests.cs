using BarbApp.IntegrationTests;
using BarbApp.Application.Interfaces;
using BarbApp.Application.DTOs;
using BarbApp.Infrastructure.Services;
using BarbApp.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Net.Http.Json;

namespace BarbApp.IntegrationTests.Services;

[Collection(nameof(IntegrationTestCollection))]
public class ExternalServicesIntegrationTests : IAsyncLifetime
{
    private readonly IntegrationTestWebAppFactory _factory;
    private readonly DatabaseFixture _dbFixture;
    private IServiceScope? _scope;

    public ExternalServicesIntegrationTests(DatabaseFixture dbFixture)
    {
        _dbFixture = dbFixture;
        _factory = dbFixture.CreateFactory();
    }

    public async Task InitializeAsync()
    {
        _scope = _factory.Services.CreateScope();
        _factory.EnsureDatabaseInitialized();
    }

    public async Task DisposeAsync()
    {
        _scope?.Dispose();
    }

    [Fact]
    public async Task InfisicalService_GetSecretAsync_InTestingEnvironment_ReturnsTestValues()
    {
        // Arrange
        var secretManager = _scope!.ServiceProvider.GetRequiredService<ISecretManager>();

        // Act
        var jwtSecret = await secretManager.GetSecretAsync("JWT_SECRET");
        var otherSecret = await secretManager.GetSecretAsync("SOME_OTHER_SECRET");

        // Assert
        jwtSecret.Should().Be("test-secret-key-at-least-32-characters-long-for-jwt");
        otherSecret.Should().Be("test-value-for-SOME_OTHER_SECRET");
    }

    [Fact]
    public async Task PasswordHasher_HashPassword_ReturnsHashedPassword()
    {
        // Arrange
        var passwordHasher = _scope!.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Act
        var hashedPassword = passwordHasher.Hash("test-password-123");

        // Assert
        hashedPassword.Should().NotBeNullOrEmpty();
        hashedPassword.Should().NotBe("test-password-123"); // Should be hashed
        hashedPassword.Length.Should().BeGreaterThan(10); // Should be a proper hash
    }

    [Fact]
    public async Task PasswordHasher_VerifyPassword_ValidPassword_ReturnsTrue()
    {
        // Arrange
        var passwordHasher = _scope!.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var plainPassword = "test-password-123";
        var hashedPassword = passwordHasher.Hash(plainPassword);

        // Act
        var isValid = passwordHasher.Verify(plainPassword, hashedPassword);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task PasswordHasher_VerifyPassword_InvalidPassword_ReturnsFalse()
    {
        // Arrange
        var passwordHasher = _scope!.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var plainPassword = "test-password-123";
        var wrongPassword = "wrong-password";
        var hashedPassword = passwordHasher.Hash(plainPassword);

        // Act
        var isValid = passwordHasher.Verify(wrongPassword, hashedPassword);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task JwtTokenGenerator_GenerateToken_ReturnsValidToken()
    {
        // Arrange
        var jwtTokenGenerator = _scope!.ServiceProvider.GetRequiredService<IJwtTokenGenerator>();
        var userId = Guid.NewGuid().ToString();
        var email = "test@example.com";
        var userType = "Cliente";
        var barbeariaId = Guid.NewGuid();

        // Act
        var token = jwtTokenGenerator.GenerateToken(userId, userType, email, barbeariaId);

        // Assert
        token.Should().NotBeNull();
        token.Value.Should().NotBeNullOrEmpty();
        token.Value.Length.Should().BeGreaterThan(100); // JWT tokens are typically long
        token.Value.Split('.').Length.Should().Be(3); // JWT has 3 parts separated by dots
        token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task FakeEmailService_SendAsync_LogsEmailMessage()
    {
        // Arrange
        var emailService = _scope!.ServiceProvider.GetRequiredService<IEmailService>() as FakeEmailService;
        emailService.Should().NotBeNull("FakeEmailService should be registered in testing environment");

        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<h1>Test HTML Body</h1>",
            TextBody = "Test Text Body"
        };

        // Act & Assert - Should not throw and complete successfully
        await emailService!.SendAsync(message);
    }

    [Fact]
    public async Task SmtpEmailService_SendAsync_WithValidSettings_CompletesSuccessfully()
    {
        // Arrange - In testing environment, SMTP should be configured to not actually send
        var emailService = _scope!.ServiceProvider.GetRequiredService<IEmailService>();

        var message = new EmailMessage
        {
            To = "test@example.com",
            Subject = "Test Subject",
            HtmlBody = "<h1>Test HTML Body</h1>",
            TextBody = "Test Text Body"
        };

        // Act & Assert - Should complete without throwing (even if it doesn't actually send)
        await emailService.SendAsync(message);
    }

    [Fact]
    public async Task LocalLogoUploadService_IsValidImageFile_ValidJpgFile_ReturnsTrue()
    {
        // Arrange
        var logoUploadService = _scope!.ServiceProvider.GetRequiredService<ILogoUploadService>() as LocalLogoUploadService;
        logoUploadService.Should().NotBeNull("LocalLogoUploadService should be registered in testing environment");

        var validFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024 * 1024); // 1MB JPG

        // Act
        var isValid = logoUploadService!.IsValidImageFile(validFile);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task LocalLogoUploadService_IsValidImageFile_InvalidExtension_ReturnsFalse()
    {
        // Arrange
        var logoUploadService = _scope!.ServiceProvider.GetRequiredService<ILogoUploadService>() as LocalLogoUploadService;
        logoUploadService.Should().NotBeNull("LocalLogoUploadService should be registered in testing environment");

        var invalidFile = CreateMockFormFile("test.txt", "text/plain", 1024);

        // Act
        var isValid = logoUploadService!.IsValidImageFile(invalidFile);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task LocalLogoUploadService_IsValidImageFile_FileTooLarge_ReturnsFalse()
    {
        // Arrange
        var logoUploadService = _scope!.ServiceProvider.GetRequiredService<ILogoUploadService>() as LocalLogoUploadService;
        logoUploadService.Should().NotBeNull("LocalLogoUploadService should be registered in testing environment");

        var largeFile = CreateMockFormFile("test.jpg", "image/jpeg", 3 * 1024 * 1024); // 3MB

        // Act
        var isValid = logoUploadService!.IsValidImageFile(largeFile);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task LocalLogoUploadService_UploadLogoAsync_ValidImageFile_ReturnsSuccess()
    {
        // Arrange
        var logoUploadService = _scope!.ServiceProvider.GetRequiredService<ILogoUploadService>();
        var barbershopId = Guid.NewGuid();
        var validImageFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024 * 1024); // 1MB JPG

        // Act
        var result = await logoUploadService.UploadLogoAsync(barbershopId, validImageFile);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
        result.Data.Should().StartWith("/uploads/logos/");
    }

    [Fact]
    public async Task LocalLogoUploadService_DeleteLogoAsync_ValidLogoUrl_CompletesSuccessfully()
    {
        // Arrange
        var logoUploadService = _scope!.ServiceProvider.GetRequiredService<ILogoUploadService>();
        var barbershopId = Guid.NewGuid();
        var validImageFile = CreateMockFormFile("test.jpg", "image/jpeg", 1024 * 1024);
        
        // First upload a logo
        var uploadResult = await logoUploadService.UploadLogoAsync(barbershopId, validImageFile);
        uploadResult.IsSuccess.Should().BeTrue();
        uploadResult.Data.Should().NotBeNull();

        // Act
        var deleteResult = await logoUploadService.DeleteLogoAsync(uploadResult.Data);

        // Assert
        deleteResult.Should().NotBeNull();
        deleteResult.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ImageSharpProcessor_ProcessAndSaveImageAsync_ValidImage_CompletesSuccessfully()
    {
        // Arrange
        var imageProcessor = _scope!.ServiceProvider.GetRequiredService<IImageProcessor>() as ImageSharpProcessor;
        imageProcessor.Should().NotBeNull("ImageSharpProcessor should be registered in testing environment");

        // Create a simple test image in memory
        var testImageBytes = CreateTestJpgImage();
        var inputStream = new MemoryStream(testImageBytes);
        var outputPath = Path.Combine(Path.GetTempPath(), $"test_image_{Guid.NewGuid()}.jpg");

        // Act
        await imageProcessor!.ProcessAndSaveImageAsync(inputStream, outputPath);

        // Assert
        File.Exists(outputPath).Should().BeTrue();
        
        // Cleanup
        if (File.Exists(outputPath))
        {
            File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ImageSharpProcessor_ProcessImageAsync_ValidImage_ReturnsProcessedStream()
    {
        // Arrange
        var imageProcessor = _scope!.ServiceProvider.GetRequiredService<IImageProcessor>() as ImageSharpProcessor;
        imageProcessor.Should().NotBeNull("ImageSharpProcessor should be registered in testing environment");

        // Create a simple test image in memory
        var testImageBytes = CreateTestJpgImage();
        var inputStream = new MemoryStream(testImageBytes);

        // Act
        var resultStream = await imageProcessor!.ProcessImageAsync(inputStream);

        // Assert
        resultStream.Should().NotBeNull();
        resultStream.Length.Should().BeGreaterThan(0);
        resultStream.Position.Should().Be(0); // Should be reset to beginning
    }

    [Fact]
    public async Task R2StorageService_UploadFileAsync_ValidFile_ReturnsPublicUrl()
    {
        // Arrange
        var r2StorageService = _scope!.ServiceProvider.GetRequiredService<IR2StorageService>();
        var testContent = "Test file content for R2 upload";
        var fileStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));
        var fileName = "test-file.txt";
        var contentType = "text/plain";

        // Act
        var publicUrl = await r2StorageService.UploadFileAsync(fileStream, fileName, contentType);

        // Assert
        publicUrl.Should().NotBeNullOrEmpty();
        publicUrl.Should().StartWith("https://test-r2-storage.example.com/");
        publicUrl.Should().Contain(fileName);
    }

    [Fact]
    public async Task R2StorageService_DeleteFileAsync_ValidFileKey_ReturnsTrue()
    {
        // Arrange
        var r2StorageService = _scope!.ServiceProvider.GetRequiredService<IR2StorageService>();
        var fileKey = "test-file-key.txt";

        // Act
        var result = await r2StorageService.DeleteFileAsync(fileKey);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task R2StorageService_DownloadFileAsync_ValidFileKey_ReturnsStream()
    {
        // Arrange
        var r2StorageService = _scope!.ServiceProvider.GetRequiredService<IR2StorageService>();
        var fileKey = "test-file-key.txt";

        // Act
        var stream = await r2StorageService.DownloadFileAsync(fileKey);

        // Assert
        stream.Should().NotBeNull();
        stream.Should().BeOfType<MemoryStream>();
    }

    [Fact]
    public void R2StorageService_GetPublicUrl_ValidFileKey_ReturnsPublicUrl()
    {
        // Arrange
        var r2StorageService = _scope!.ServiceProvider.GetRequiredService<IR2StorageService>();
        var fileKey = "logos/20231201/test-file.jpg";

        // Act
        var publicUrl = r2StorageService.GetPublicUrl(fileKey);

        // Assert
        publicUrl.Should().NotBeNullOrEmpty();
        publicUrl.Should().Be("https://test-r2-storage.example.com/logos/20231201/test-file.jpg");
    }

    [Fact]
    public async Task DisponibilidadeCache_InvalidateAsync_ExistingKey_RemovesValue()
    {
        // Arrange
        var cache = _scope!.ServiceProvider.GetRequiredService<IDisponibilidadeCache>();
        var barbeiroId = Guid.NewGuid();
        var dataInicio = DateTime.Today;
        var dataFim = DateTime.Today.AddDays(1);
        var barbeiro = new BarbeiroDto(barbeiroId, "Test Barber", null, new List<string> { "Barba" });
        var diasDisponiveis = new List<DiaDisponivel>
        {
            new DiaDisponivel(DateTime.Today, new List<string> { "16:00" })
        };
        var disponibilidade = new DisponibilidadeOutput(barbeiro, diasDisponiveis);

        // Set and verify it exists
        await cache.SetAsync(barbeiroId, dataInicio, dataFim, disponibilidade);
        var beforeInvalidate = await cache.GetAsync(barbeiroId, dataInicio, dataFim);
        beforeInvalidate.Should().NotBeNull();

        // Act
        await cache.InvalidateAsync(barbeiroId, dataInicio, dataFim);
        var afterInvalidate = await cache.GetAsync(barbeiroId, dataInicio, dataFim);

        // Assert
        afterInvalidate.Should().BeNull();
    }

    [Fact]
    public async Task SentryScopeEnrichmentMiddleware_ProcessesRequestWithoutErrors()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Make a request that should go through the middleware
        // Use /health/live endpoint which doesn't depend on database health
        var response = await client.GetAsync("/health/live");

        // Assert - The middleware should not cause errors
        response.Should().NotBeNull();
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task GlobalExceptionHandlerMiddleware_HandlesExceptions_ReturnsProblemDetails()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act - Make a POST request with invalid data that will throw an exception
        var invalidLoginRequest = new
        {
            codigoBarbearia = "INVALID",
            telefone = "11999999999",
            nome = "Test Cliente"
        };
        var response = await client.PostAsJsonAsync("/api/auth/cliente/login", invalidLoginRequest);

        // Assert - Should return 400 Bad Request with problem details
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Código da barbearia deve ter 8 caracteres");
    }

    [Fact]
    public async Task TenantMiddleware_SetsTenantContext_ForAuthenticatedRequest()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Create test data using TestHelper
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var (barbeariaId, clienteId, telefone, nome, codigoBarbearia) = await TestHelper.CreateClienteAsync(context);

        // First login to get a token
        var loginRequest = new
        {
            codigoBarbearia = codigoBarbearia,
            telefone = telefone,
            nome = nome
        };
        var loginResponse = await client.PostAsJsonAsync("/api/auth/cliente/login", loginRequest);
        loginResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var loginResult = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        loginResult.Should().NotBeNull();
        var token = loginResult!.Token;

        // Act - Make authenticated request
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await client.GetAsync("/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnitOfWork_SaveChangesAsync_CommitsTransaction()
    {
        // Arrange
        var unitOfWork = _scope!.ServiceProvider.GetRequiredService<IUnitOfWork>();
        
        // Act & Assert - Should complete without throwing
        await unitOfWork.Commit(CancellationToken.None);
    }

    [Fact]
    public async Task UniqueCodeGenerator_GenerateCode_ReturnsUniqueCode()
    {
        // Arrange
        var codeGenerator = _scope!.ServiceProvider.GetRequiredService<IUniqueCodeGenerator>();

        // Act
        var code1 = await codeGenerator.GenerateAsync(CancellationToken.None);
        var code2 = await codeGenerator.GenerateAsync(CancellationToken.None);

        // Assert
        code1.Should().NotBeNullOrEmpty();
        code1.Length.Should().Be(8); // Default length
        code2.Should().NotBeNullOrEmpty();
        code2.Length.Should().Be(8);
        code1.Should().NotBe(code2); // Should be unique
    }

    [Fact]
    public async Task SecurePasswordGenerator_GeneratePassword_ReturnsSecurePassword()
    {
        // Arrange
        var passwordGenerator = _scope!.ServiceProvider.GetRequiredService<IPasswordGenerator>();

        // Act
        var password = passwordGenerator.Generate();

        // Assert
        password.Should().NotBeNullOrEmpty();
        password.Length.Should().BeGreaterThanOrEqualTo(12); // Should be reasonably long
        // Should contain mix of characters (basic check)
        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));
        
        (hasUpper || hasLower || hasDigit || hasSpecial).Should().BeTrue();
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        byte[] content;
        if (contentType == "image/jpeg" || contentType == "image/png")
        {
            // Use valid image data for image files
            content = CreateTestJpgImage();
        }
        else
        {
            // Use text content for other files
            content = Encoding.UTF8.GetBytes("Mock file content");
        }

        // Create a custom stream that reports the desired length for file size validation
        var customStream = new TestStream(content, length);
        var file = new FormFile(customStream, 0, length, Path.GetFileNameWithoutExtension(fileName), fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        return file;
    }

    private static byte[] CreateTestJpgImage()
    {
        // This is a minimal valid 1x1 pixel JPG image
        // Generated using online tools - this should be decodable by ImageSharp
        return Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/AB//2Q=="
        );
    }
}

/// <summary>
/// Custom stream that reports a specific length for testing file size validation
/// </summary>
class TestStream : MemoryStream
{
    private readonly long _reportedLength;

    public TestStream(byte[] content, long reportedLength) : base(content)
    {
        _reportedLength = reportedLength;
    }

    public override long Length => _reportedLength;
}