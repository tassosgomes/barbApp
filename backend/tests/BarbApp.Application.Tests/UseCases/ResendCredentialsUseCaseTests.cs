using BarbApp.Application.Interfaces;
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

public class ResendCredentialsUseCaseTests
{
    private readonly Mock<IBarbershopRepository> _barbershopRepositoryMock;
    private readonly Mock<IAdminBarbeariaUserRepository> _adminBarbeariaUserRepositoryMock;
    private readonly Mock<IPasswordGenerator> _passwordGeneratorMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<ResendCredentialsUseCase>> _loggerMock;
    private readonly ResendCredentialsUseCase _useCase;

    public ResendCredentialsUseCaseTests()
    {
        _barbershopRepositoryMock = new Mock<IBarbershopRepository>();
        _adminBarbeariaUserRepositoryMock = new Mock<IAdminBarbeariaUserRepository>();
        _passwordGeneratorMock = new Mock<IPasswordGenerator>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _emailServiceMock = new Mock<IEmailService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ResendCredentialsUseCase>>();

        _useCase = new ResendCredentialsUseCase(
            _barbershopRepositoryMock.Object,
            _adminBarbeariaUserRepositoryMock.Object,
            _passwordGeneratorMock.Object,
            _passwordHasherMock.Object,
            _emailServiceMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldResendCredentialsSuccessfully_WhenBarbershopAndAdminExist()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("12345678", "Rua Teste", "123", null, "Centro", "Cidade", "SP");
        var uniqueCode = UniqueCode.Create("ABCD2345");
        var document = Document.Create("12345678901234");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11999999999",
            "João da Silva",
            "joao@teste.com",
            address,
            uniqueCode,
            "AdminCentral");

        var adminUser = AdminBarbeariaUser.Create(
            barbershopId,
            "admin@barbearia.com",
            "oldPasswordHash",
            "Admin Teste");

        var newPassword = "NewSecure123!";
        var newPasswordHash = "newPasswordHash";

        _barbershopRepositoryMock
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _passwordGeneratorMock
            .Setup(g => g.Generate(It.IsAny<int>()))
            .Returns(newPassword);

        _passwordHasherMock
            .Setup(h => h.Hash(newPassword))
            .Returns(newPasswordHash);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AdminBarbeariaUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        _barbershopRepositoryMock.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _passwordGeneratorMock.Verify(g => g.Generate(It.IsAny<int>()), Times.Once);
        _passwordHasherMock.Verify(h => h.Hash(newPassword), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.UpdateAsync(It.Is<AdminBarbeariaUser>(u => u.PasswordHash == newPasswordHash), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendAsync(
            It.Is<EmailMessage>(m =>
                m.To == adminUser.Email &&
                m.Subject.Contains("novas credenciais", StringComparison.OrdinalIgnoreCase) &&
                m.HtmlBody.Contains(newPassword) &&
                m.TextBody.Contains(newPassword)),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowNotFoundException_WhenBarbershopDoesNotExist()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();

        _barbershopRepositoryMock
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbershop?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Barbearia com ID '{barbershopId}' não encontrada");

        _barbershopRepositoryMock.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.GetByBarbershopIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordGeneratorMock.Verify(g => g.Generate(It.IsAny<int>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldThrowValidationException_WhenAdminUserDoesNotExist()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("12345678", "Rua Teste", "123", null, "Centro", "Cidade", "SP");
        var uniqueCode = UniqueCode.Create("ABCD2345");
        var document = Document.Create("12345678901234");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11999999999",
            "João da Silva",
            "joao@teste.com",
            address,
            uniqueCode,
            "AdminCentral");

        _barbershopRepositoryMock
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminBarbeariaUser?)null);

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage($"Administrador da barbearia '{barbershop.Name}' não encontrado");

        _barbershopRepositoryMock.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _passwordGeneratorMock.Verify(g => g.Generate(It.IsAny<int>()), Times.Never);
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRollbackTransaction_WhenEmailFails()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("12345678", "Rua Teste", "123", null, "Centro", "Cidade", "SP");
        var uniqueCode = UniqueCode.Create("ABCD2345");
        var document = Document.Create("12345678901234");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11999999999",
            "João da Silva",
            "joao@teste.com",
            address,
            uniqueCode,
            "AdminCentral");

        var adminUser = AdminBarbeariaUser.Create(
            barbershopId,
            "admin@barbearia.com",
            "oldPasswordHash",
            "Admin Teste");

        var newPassword = "NewSecure123!";
        var newPasswordHash = "newPasswordHash";

        _barbershopRepositoryMock
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _passwordGeneratorMock
            .Setup(g => g.Generate(It.IsAny<int>()))
            .Returns(newPassword);

        _passwordHasherMock
            .Setup(h => h.Hash(newPassword))
            .Returns(newPasswordHash);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AdminBarbeariaUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("SMTP server error"));

        // Act
        Func<Task> act = async () => await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Falha ao enviar e-mail com as novas credenciais. Por favor, tente novamente ou contate o suporte.");

        _barbershopRepositoryMock.Verify(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()), Times.Once);
        _passwordGeneratorMock.Verify(g => g.Generate(It.IsAny<int>()), Times.Once);
        _passwordHasherMock.Verify(h => h.Hash(newPassword), Times.Once);
        _adminBarbeariaUserRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<AdminBarbeariaUser>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.Commit(It.IsAny<CancellationToken>()), Times.Never); // Should not commit on error
    }

    [Fact]
    public async Task ExecuteAsync_ShouldLogAllKeyEvents()
    {
        // Arrange
        var barbershopId = Guid.NewGuid();
        var address = Address.Create("12345678", "Rua Teste", "123", null, "Centro", "Cidade", "SP");
        var uniqueCode = UniqueCode.Create("ABCD2345");
        var document = Document.Create("12345678901234");
        var barbershop = Barbershop.Create(
            "Barbearia Teste",
            document,
            "11999999999",
            "João da Silva",
            "joao@teste.com",
            address,
            uniqueCode,
            "AdminCentral");

        var adminUser = AdminBarbeariaUser.Create(
            barbershopId,
            "admin@barbearia.com",
            "oldPasswordHash",
            "Admin Teste");

        var newPassword = "NewSecure123!";
        var newPasswordHash = "newPasswordHash";

        _barbershopRepositoryMock
            .Setup(r => r.GetByIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbershop);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.GetByBarbershopIdAsync(barbershopId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminUser);

        _passwordGeneratorMock
            .Setup(g => g.Generate(It.IsAny<int>()))
            .Returns(newPassword);

        _passwordHasherMock
            .Setup(h => h.Hash(newPassword))
            .Returns(newPasswordHash);

        _adminBarbeariaUserRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<AdminBarbeariaUser>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(e => e.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(u => u.Commit(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _useCase.ExecuteAsync(barbershopId, CancellationToken.None);

        // Assert - Verify logging occurred (checking at least Information level was called)
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting credential resend")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Generated new password")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Password updated")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("email sent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Credentials resent successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
