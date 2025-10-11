// BarbApp.Infrastructure.Tests/Services/TenantContextTests.cs
using BarbApp.Domain.Interfaces;
using BarbApp.Infrastructure.Services;
using FluentAssertions;
using Xunit;

namespace BarbApp.Infrastructure.Tests.Services;

public class TenantContextTests
{
    [Fact]
    public void Properties_WhenNoContextSet_ShouldReturnDefaultValues()
    {
        // Arrange
        var context = new TenantContext();

        // Act & Assert
        context.BarbeariaId.Should().BeNull();
        context.BarbeariaCode.Should().BeNull();
        context.IsAdminCentral.Should().BeTrue(); // Empty code means admin central
        context.UserId.Should().BeEmpty();
        context.Role.Should().BeEmpty();
    }

    [Fact]
    public void SetContext_WithBarbearia_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var context = new TenantContext();
        var userId = "user123";
        var role = "Barbeiro";
        var barbeariaId = Guid.NewGuid();
        var barbeariaCode = "ABC12345";

        // Act
        context.SetContext(userId, role, barbeariaId, barbeariaCode);

        // Assert
        context.BarbeariaId.Should().Be(barbeariaId);
        context.BarbeariaCode.Should().Be(barbeariaCode);
        context.IsAdminCentral.Should().BeFalse();
        context.UserId.Should().Be(userId);
        context.Role.Should().Be(role);
    }

    [Fact]
    public void SetContext_AdminCentral_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var context = new TenantContext();
        var userId = "admin123";
        var role = "AdminCentral";

        // Act
        context.SetContext(userId, role, null, null);

        // Assert
        context.BarbeariaId.Should().BeNull();
        context.BarbeariaCode.Should().BeNull();
        context.IsAdminCentral.Should().BeTrue();
        context.UserId.Should().Be(userId);
        context.Role.Should().Be(role);
    }

    [Fact]
    public void Clear_ShouldResetAllProperties()
    {
        // Arrange
        var context = new TenantContext();
        context.SetContext("user123", "Barbeiro", Guid.NewGuid(), "ABC12345");

        // Act
        context.Clear();

        // Assert
        context.BarbeariaId.Should().BeNull();
        context.BarbeariaCode.Should().BeNull();
        context.IsAdminCentral.Should().BeTrue();
        context.UserId.Should().BeEmpty();
        context.Role.Should().BeEmpty();
    }

    [Fact]
    public async Task AsyncLocal_ExecutionContextIsolation()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";
        var barbeariaId1 = Guid.NewGuid();
        var barbeariaId2 = Guid.NewGuid();

        string? capturedUserId1 = null;
        string? capturedUserId2 = null;

        // Act - Each task runs in its own execution context
        var task1 = Task.Run(() =>
        {
            var context = new TenantContext();
            context.SetContext(userId1, "Barbeiro", barbeariaId1, "CODE1");
            capturedUserId1 = context.UserId;
        });

        var task2 = Task.Run(() =>
        {
            var context = new TenantContext();
            context.SetContext(userId2, "AdminBarbearia", barbeariaId2, "CODE2");
            capturedUserId2 = context.UserId;
        });

        await Task.WhenAll(task1, task2);

        // Assert - Each execution context has its own AsyncLocal value
        capturedUserId1.Should().Be(userId1);
        capturedUserId2.Should().Be(userId2);
    }

    [Fact]
    public void AsyncLocal_SharedAcrossInstancesInSameExecutionContext()
    {
        // Arrange
        var context1 = new TenantContext();
        var context2 = new TenantContext();

        var userId1 = "user1";
        var barbeariaId1 = Guid.NewGuid();

        // Act
        context1.SetContext(userId1, "Barbeiro", barbeariaId1, "CODE1");

        // Assert - AsyncLocal is shared across instances in the same execution context
        context1.UserId.Should().Be(userId1);
        context1.BarbeariaId.Should().Be(barbeariaId1);

        // context2 sees the same value because AsyncLocal is static and shared
        context2.UserId.Should().Be(userId1);
        context2.BarbeariaId.Should().Be(barbeariaId1);
    }
}