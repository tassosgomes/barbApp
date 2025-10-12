using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Services;
using System.Net.Http.Json;

namespace BarbApp.IntegrationTests;

public static class TestHelper
{
    public static async Task<(Guid id, string email, string senha)> CreateAdminCentralAsync(
        BarbAppDbContext context,
        IPasswordHasher passwordHasher)
    {
        var email = $"admin-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";

        var admin = AdminCentralUser.Create(email, passwordHasher.Hash(senha), "Test Admin");
        await context.AdminCentralUsers.AddAsync(admin);
        await context.SaveChangesAsync();

        return (admin.Id, email, senha);
    }

    public static async Task<(Guid barbeariaId, Guid adminId, string email, string senha, string codigo)>
        CreateAdminBarbeariaAsync(
            BarbAppDbContext context,
            IPasswordHasher passwordHasher)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var codigo = UniqueCode.Create(GenerateUniqueCode());
        var barbearia = Barbershop.Create(
            $"Test Barbearia {Guid.NewGuid()}",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            codigo,
            "admin-user-id"
        );
        await context.Barbershops.AddAsync(barbearia);

        var email = $"admin-barb-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";

        var admin = AdminBarbeariaUser.Create(barbearia.Id, email, passwordHasher.Hash(senha), "Test Admin Barbearia");
        await context.AdminBarbeariaUsers.AddAsync(admin);
        await context.SaveChangesAsync();

        return (barbearia.Id, admin.Id, email, senha, codigo.Value);
    }

    public static async Task<(Guid barbeariaId, Guid barbeiroId, string telefone, string codigo)>
        CreateBarbeiroAsync(
            BarbAppDbContext context)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var codigo = UniqueCode.Create(GenerateUniqueCode());
        var barbearia = Barbershop.Create(
            $"Test Barbearia {Guid.NewGuid()}",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            codigo,
            "admin-user-id"
        );
        await context.Barbershops.AddAsync(barbearia);

        var telefone = $"119{new Random().Next(10000000, 99999999)}";

        var barbeiro = Barber.Create(barbearia.Id, telefone, "Test Barber");
        await context.Barbers.AddAsync(barbeiro);
        await context.SaveChangesAsync();

        return (barbearia.Id, barbeiro.Id, telefone, codigo.Value);
    }

    public static async Task<(Guid barbeariaId, Guid clienteId, string telefone, string nome, string codigo)>
        CreateClienteAsync(
            BarbAppDbContext context)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var codigo = UniqueCode.Create(GenerateUniqueCode());
        var barbearia = Barbershop.Create(
            $"Test Barbearia {Guid.NewGuid()}",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            codigo,
            "admin-user-id"
        );
        await context.Barbershops.AddAsync(barbearia);

        var telefone = $"119{new Random().Next(10000000, 99999999)}";
        var nome = "Test Cliente";

        var cliente = Customer.Create(barbearia.Id, telefone, nome);
        await context.Customers.AddAsync(cliente);
        await context.SaveChangesAsync();

        return (barbearia.Id, cliente.Id, telefone, nome, codigo.Value);
    }

    public static async Task<string> GetAuthTokenAsync(
        HttpClient client,
        string endpoint,
        object loginData)
    {
        var response = await client.PostAsJsonAsync(endpoint, loginData);
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return authResponse!.Token;
    }

    private static string GenerateUniqueCode()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}