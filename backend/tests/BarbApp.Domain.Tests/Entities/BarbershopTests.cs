
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using Xunit;

namespace BarbApp.Domain.Tests.Entities
{
    public class BarbershopTests
    {
        [Fact]
        public void Create_ValidData_ShouldCreateBarbershop()
        {
            // Arrange
            var document = Document.Create("12345678000190");
            var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
            var code = UniqueCode.Create("ABC82345");

            // Act
            var barbershop = Barbershop.Create(
                "Barbearia Teste",
                document,
                "11987654321",
                "João Silva",
                "joao@test.com",
                address,
                code,
                "admin-user-id"
            );

            // Assert
            barbershop.Should().NotBeNull();
            barbershop.Name.Should().Be("Barbearia Teste");
            barbershop.IsActive.Should().BeTrue();
            barbershop.CreatedBy.Should().Be("admin-user-id");
        }
    }
}
