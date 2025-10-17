using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class CreateBarbershopUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IAddressRepository> _addressRepositoryMock;
    private readonly Mock<IAdminBarbeariaUserRepository> _adminBarbeariaUserRepositoryMock;
    private readonly Mock<IPasswordGenerator> _passwordGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUniqueCodeGenerator> _codeGeneratorMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<CreateBarbershopUseCase>> _loggerMock;
    private readonly CreateBarbershopUseCase _useCase;

    public CreateBarbershopUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _addressRepositoryMock = new Mock<IAddressRepository>();
        _adminBarbeariaUserRepositoryMock = new Mock<IAdminBarbeariaUserRepository>();
        _passwordGeneratorMock = new Mock<IPasswordGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailServiceMock = new Mock<IEmailService>();
        _codeGeneratorMock = new Mock<IUniqueCodeGenerator>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CreateBarbershopUseCase>>();

        _useCase = new CreateBarbershopUseCase(
            _barbershopRepositoryMock.Object,
            _addressRepositoryMock.Object,
            _adminBarbeariaUserRepositoryMock.Object,
            _passwordGeneratorMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _codeGeneratorMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidInput_ShouldCreateBarbershop()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "joao@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            "Sala 15",
            "Bela Vista",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        _adminBarbeariaUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        _codeGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ABC23456");

        _passwordGeneratorMock
            .Setup(x => x.Generate(It.IsAny<int>()))
            .Returns("SecurePass123!");

        _passwordHasherMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("$2a$12$hashedpassword");

        _emailServiceMock
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Barbearia Teste");
        result.Document.Should().Be("12345678000190");
        result.Code.Should().Be("ABC23456");
        result.IsActive.Should().BeTrue();

        _barbershopRepositoryMock.Verify(x => x.InsertAsync(It.IsAny<Barbershop>(), It.IsAny<CancellationToken>()), Times.Once);
        _addressRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Address>(), It.IsAny<CancellationToken>()), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(x => x.AddAsync(It.IsAny<AdminBarbeariaUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateDocument_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "joao@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        var existingBarbershop = Barbershop.Create(
            "Existing",
            Document.Create("12345678000190"),
            "11987654321",
            "Existing Owner",
            "existing@test.com",
            Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP"),
            UniqueCode.Create("XYZ23456"),
            "admin");

        _barbershopRepositoryMock
            .Setup(x => x.GetByDocumentAsync("12345678000190", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBarbershop);

        _adminBarbeariaUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateDocumentException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_CodeGenerationFails_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "joao@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        _adminBarbeariaUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        _codeGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Failed to generate code"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_DuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "existing@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        var existingAdmin = AdminBarbeariaUser.Create(
            Guid.NewGuid(),
            "existing@test.com",
            "$2a$12$hashedpassword",
            "Existing Admin");

        _adminBarbeariaUserRepositoryMock
            .Setup(x => x.GetByEmailAsync("existing@test.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAdmin);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));
    }

    [Fact]
    public async Task ExecuteAsync_EmailSendingFails_ShouldThrowException()
    {
        // Arrange
        var input = new CreateBarbershopInput(
            "Barbearia Teste",
            "12345678000190",
            "11987654321",
            "João Silva",
            "joao@test.com",
            "01310100",
            "Av. Paulista",
            "1000",
            null,
            "Bela Vista",
            "São Paulo",
            "SP");

        _barbershopRepositoryMock
            .Setup(x => x.GetByDocumentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        _adminBarbeariaUserRepositoryMock
            .Setup(x => x.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        _codeGeneratorMock
            .Setup(x => x.GenerateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync("ABC23456");

        _passwordGeneratorMock
            .Setup(x => x.Generate(It.IsAny<int>()))
            .Returns("SecurePass123!");

        _passwordHasherMock
            .Setup(x => x.Hash(It.IsAny<string>()))
            .Returns("$2a$12$hashedpassword");

        _emailServiceMock
            .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Failed to send email"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _useCase.ExecuteAsync(input, CancellationToken.None));

        // Verify transaction was not committed
        _unitOfWorkMock.Verify(x => x.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }
}