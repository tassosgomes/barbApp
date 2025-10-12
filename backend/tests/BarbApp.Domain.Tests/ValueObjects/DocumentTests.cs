
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.ValueObjects;
using FluentAssertions;
using System;
using Xunit;

namespace BarbApp.Domain.Tests.ValueObjects
{
    public class DocumentTests
    {
        [Theory]
        [InlineData("12345678000190")] // CNPJ válido
        [InlineData("12345678901")] // CPF válido
        public void Document_ValidFormat_ShouldCreate(string documentValue)
        {
            // Act
            var document = Document.Create(documentValue);

            // Assert
            document.Should().NotBeNull();
            document.Value.Should().Be(documentValue);
        }

        [Theory]
        [InlineData("123")] // Muito curto
        [InlineData("123456789012345")] // Comprimento inválido
        [InlineData("abcd1234")] // Não numérico
        public void Document_InvalidFormat_ShouldThrowException(string documentValue)
        {
            // Act & Assert
            Action act = () => Document.Create(documentValue);
            act.Should().Throw<InvalidDocumentException>();
        }
    }
}
