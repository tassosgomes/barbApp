using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using FluentAssertions;

namespace BarbApp.Domain.Tests.Entities;

public class ClienteTests
{
    private readonly Guid _barbeariaId = Guid.NewGuid();

    [Fact]
    public void Create_ComDadosValidos_DeveCriarClienteComSucesso()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";

        // Act
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        cliente.Should().NotBeNull();
        cliente.Id.Should().NotBe(Guid.Empty);
        cliente.BarbeariaId.Should().Be(_barbeariaId);
        cliente.Nome.Should().Be(nome);
        cliente.Telefone.Should().Be("11987654321");
        cliente.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        cliente.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_ComNomeVazio_DeveLancarValidationException()
    {
        // Arrange
        var nome = "";
        var telefone = "11987654321";

        // Act
        var act = () => Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Nome é obrigatório");
    }

    [Fact]
    public void Create_ComNomeNull_DeveLancarValidationException()
    {
        // Arrange
        string nome = null!;
        var telefone = "11987654321";

        // Act
        var act = () => Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Nome é obrigatório");
    }

    [Fact]
    public void Create_ComNomeApenasEspacos_DeveLancarValidationException()
    {
        // Arrange
        var nome = "   ";
        var telefone = "11987654321";

        // Act
        var act = () => Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Nome é obrigatório");
    }

    [Fact]
    public void Create_ComNomeComEspacosExtras_DeveRemoverEspacos()
    {
        // Arrange
        var nome = "  João Silva  ";
        var telefone = "11987654321";

        // Act
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        cliente.Nome.Should().Be("João Silva");
    }

    [Theory]
    [InlineData("(11) 98765-4321", "11987654321")] // Com formatação
    [InlineData("11987654321", "11987654321")] // Sem formatação
    [InlineData("1198765-4321", "11987654321")] // Formatação parcial
    [InlineData("11 9 8765 4321", "11987654321")] // Com espaços
    public void Create_ComTelefoneVariosFormatos_DeveNormalizarParaApenasNumeros(
        string telefoneInput,
        string telefoneEsperado)
    {
        // Arrange
        var nome = "João Silva";

        // Act
        var cliente = Cliente.Create(_barbeariaId, nome, telefoneInput);

        // Assert
        cliente.Telefone.Should().Be(telefoneEsperado);
    }

    [Theory]
    [InlineData("1198765432")] // 10 dígitos (válido - telefone fixo)
    [InlineData("11987654321")] // 11 dígitos (válido - celular)
    public void Create_ComTelefoneValido_DeveCriarComSucesso(string telefone)
    {
        // Arrange
        var nome = "João Silva";

        // Act
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        cliente.Telefone.Should().Be(telefone);
    }

    [Theory]
    [InlineData("119876543")] // 9 dígitos
    [InlineData("119876543212")] // 12 dígitos
    [InlineData("1234")] // Muito curto
    [InlineData("")] // Vazio
    public void Create_ComTelefoneInvalido_DeveLancarValidationException(string telefone)
    {
        // Arrange
        var nome = "João Silva";

        // Act
        var act = () => Cliente.Create(_barbeariaId, nome, telefone);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Telefone inválido");
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeCorreto_DeveRetornarTrue()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("João Silva");

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeCorretoCaseInsensitive_DeveRetornarTrue()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("joão silva");

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeCorretoUpperCase_DeveRetornarTrue()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("JOÃO SILVA");

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeCorretoComEspacosExtras_DeveRetornarTrue()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("  João Silva  ");

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeIncorreto_DeveRetornarFalse()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("Maria Santos");

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void ValidarNomeLogin_ComNomeParcialmenteCorreto_DeveRetornarFalse()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var resultado = cliente.ValidarNomeLogin("João");

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void AtualizarNome_ComNomeValido_DeveAtualizarNomeEUpdateAt()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);
        var updateAtOriginal = cliente.UpdatedAt;

        // Act
        Thread.Sleep(10); // Garantir diferença de tempo
        cliente.AtualizarNome("João Santos");

        // Assert
        cliente.Nome.Should().Be("João Santos");
        cliente.UpdatedAt.Should().BeAfter(updateAtOriginal);
    }

    [Fact]
    public void AtualizarNome_ComNomeInvalido_DeveLancarValidationException()
    {
        // Arrange
        var nome = "João Silva";
        var telefone = "11987654321";
        var cliente = Cliente.Create(_barbeariaId, nome, telefone);

        // Act
        var act = () => cliente.AtualizarNome("");

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("Nome é obrigatório");
    }
}
