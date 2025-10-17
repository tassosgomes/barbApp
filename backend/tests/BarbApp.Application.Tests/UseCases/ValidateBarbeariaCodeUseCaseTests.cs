using BarbApp.Application.DTOs;
using BarbApp.Application.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace BarbApp.Application.Tests.UseCases;

public class ValidateBarbeariaCodeUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly ValidateBarbeariaCodeUseCase _useCase;

    public ValidateBarbeariaCodeUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _useCase = new ValidateBarbeariaCodeUseCase(_barbershopRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ValidCode_ReturnsValidateBarbeariaCodeResponse()
    {
        // Arrange
        var codigo = "ABC23456";
        var barbershop = CreateBarbershop(codigo, "Barbearia Teste", true);

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        // Act
        var result = await _useCase.ExecuteAsync(codigo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(barbershop.Id, result.Id);
        Assert.Equal(barbershop.Name, result.Nome);
        Assert.Equal(barbershop.Code.Value, result.Codigo);
        Assert.Equal(barbershop.IsActive, result.IsActive);
        _barbershopRepositoryMock.Verify(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ExecuteAsync_NullOrEmptyCode_ThrowsValidationException(string codigo)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(codigo));

        Assert.Equal("Código é obrigatório", exception.Message);
        _barbershopRepositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("ABC23")]         // Menos de 8 caracteres
    [InlineData("ABC234567")]     // Mais de 8 caracteres
    [InlineData("abc23456")]      // Letras minúsculas
    [InlineData("ABC 2345")]      // Espaço
    [InlineData("ABC-2345")]      // Caractere especial
    [InlineData("ÁBC23456")]      // Caractere acentuado
    public async Task ExecuteAsync_InvalidCodeFormat_ThrowsValidationException(string codigo)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _useCase.ExecuteAsync(codigo));

        Assert.Equal("Código inválido. Deve conter 8 caracteres alfanuméricos maiúsculos", exception.Message);
        _barbershopRepositoryMock.Verify(x => x.GetByCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_NonExistentCode_ThrowsNotFoundException()
    {
        // Arrange
        var codigo = "ABC23456";

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _useCase.ExecuteAsync(codigo));

        Assert.Equal("Barbearia não encontrada", exception.Message);
        _barbershopRepositoryMock.Verify(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_InactiveBarbershop_ThrowsForbiddenException()
    {
        // Arrange
        var codigo = "ABC23456";
        var barbershop = CreateBarbershop(codigo, "Barbearia Teste", false); // Inativa

        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _useCase.ExecuteAsync(codigo));

        Assert.Equal("Barbearia temporariamente indisponível", exception.Message);
        _barbershopRepositoryMock.Verify(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldNotExposeSensitiveData()
    {
        // Arrange
        var codigo = "BARBEAR2";
        var barbershop = CreateBarbershop(codigo, "Barbearia Teste", true);
        
        _barbershopRepositoryMock
            .Setup(x => x.GetByCodeAsync(codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);
        
        // Act
        var result = await _useCase.ExecuteAsync(codigo);
        
        // Assert
        var properties = result.GetType().GetProperties();
        properties.Should().NotContain(p => p.Name == "Document");
        properties.Should().NotContain(p => p.Name == "Phone");
        properties.Should().NotContain(p => p.Name == "Email");
        properties.Should().HaveCount(4); // Id, Nome, Codigo, IsActive
    }

    private Barbershop CreateBarbershop(string code, string name, bool isActive)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01234567", "Rua Teste", "123", null, "Centro", "São Paulo", "SP");
        var uniqueCode = UniqueCode.Create(code);
        
        var barbershop = Barbershop.Create(
            name,
            document,
            "11987654321",
            "Admin Teste",
            "admin@barbershop.com",
            address,
            uniqueCode,
            "admin-user-id"
        );
        
        if (!isActive)
        {
            barbershop.Deactivate();
        }
        
        return barbershop;
    }
}
