using BarbApp.Domain.Exceptions;

namespace BarbApp.Domain.Entities;

public class Cliente
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public string Nome { get; private set; }
    public string Telefone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Barbershop? Barbearia { get; private set; }
    public ICollection<Agendamento> Agendamentos { get; private set; } = new List<Agendamento>();

    private Cliente()
    {
        // EF Core constructor
        Nome = string.Empty;
        Telefone = string.Empty;
    }

    public static Cliente Create(Guid barbeariaId, string nome, string telefone)
    {
        var nomeValidado = ValidarNome(nome);
        var telefoneValidado = ValidarTelefone(telefone);

        var now = DateTime.UtcNow;

        return new Cliente
        {
            Id = Guid.NewGuid(),
            BarbeariaId = barbeariaId,
            Nome = nomeValidado,
            Telefone = telefoneValidado,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static Cliente Create(Guid id, Guid barbeariaId, string nome, string telefone)
    {
        var nomeValidado = ValidarNome(nome);
        var telefoneValidado = ValidarTelefone(telefone);

        var now = DateTime.UtcNow;

        return new Cliente
        {
            Id = id,
            BarbeariaId = barbeariaId,
            Nome = nomeValidado,
            Telefone = telefoneValidado,
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ValidationException("Nome é obrigatório");

        return nome.Trim();
    }

    private static string ValidarTelefone(string telefone)
    {
        var apenasNumeros = new string(telefone.Where(char.IsDigit).ToArray());
        
        if (apenasNumeros.Length < 10 || apenasNumeros.Length > 11)
            throw new ValidationException("Telefone inválido");

        return apenasNumeros;
    }

    public bool ValidarNomeLogin(string nome)
    {
        return Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public void AtualizarNome(string nome)
    {
        Nome = ValidarNome(nome);
        UpdatedAt = DateTime.UtcNow;
    }
}
