
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using System;
using Xunit;

namespace BarbApp.Domain.Tests.ValueObjects
{
    public class UniqueCodeTests
    {
        [Fact]
        public void Create_ValidCode_ShouldCreate()
        {
            // Act
            var code = UniqueCode.Create("ABC82345");

            // Assert
            code.Value.Should().Be("ABC82345");
        }

        [Theory]
        [InlineData("ABC")] // Muito curto
        [InlineData("ABC123456")] // Muito longo
        [InlineData("ABC1234O")] // Contém O
        [InlineData("ABC1234I")] // Contém I
        [InlineData("ABC12340")] // Contém 0
        [InlineData("ABC12341")] // Contém 1
        public void Create_InvalidCode_ShouldThrowException(string codeValue)
        {
            // Act & Assert
            Action act = () => UniqueCode.Create(codeValue);
            act.Should().Throw<ArgumentException>();
        }
    }
}
