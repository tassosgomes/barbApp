using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BarbApp.IntegrationTests;

public static class TestHelper
{
    private static long _documentCounter;

    public static async Task<(Guid id, string email, string senha)> CreateAdminCentralAsync(
        BarbAppDbContext context,
        IPasswordHasher passwordHasher)
    {
        var email = $"admin-{Guid.NewGuid()}@test.com";
        const string senha = "Test@123";

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
        var document = Document.Create(GenerateUniqueDocument());
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
            "admin-user-id");

        await context.Barbershops.AddAsync(barbearia);

        var email = $"admin-barb-{Guid.NewGuid()}@test.com";
        const string senha = "Test@123";

        var admin = AdminBarbeariaUser.Create(barbearia.Id, email, passwordHasher.Hash(senha), "Test Admin Barbearia");
        await context.AdminBarbeariaUsers.AddAsync(admin);
        await context.SaveChangesAsync();

        return (barbearia.Id, admin.Id, email, senha, codigo.Value);
    }

    public static async Task<(Guid barbeariaId, Guid barbeiroId, string email, string senha)>
        CreateBarbeiroAsync(BarbAppDbContext context)
    {
        var passwordHasher = new PasswordHasher();
        var document = Document.Create(GenerateUniqueDocument());
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
            "admin-user-id");

        await context.Barbershops.AddAsync(barbearia);

        var email = $"barbeiro-{Guid.NewGuid()}@test.com";
        var senha = "Test@123";
        var telefone = $"119{Random.Shared.Next(10000000, 99999999)}";

        var barbeiro = Barber.Create(barbearia.Id, "Test Barber", email, passwordHasher.Hash(senha), telefone);
        await context.Barbers.AddAsync(barbeiro);
        await context.SaveChangesAsync();

        return (barbearia.Id, barbeiro.Id, email, senha);
    }

    public static async Task<(Guid barbeariaId, Guid clienteId, string telefone, string nome, string codigo)>
        CreateClienteAsync(BarbAppDbContext context)
    {
        var document = Document.Create(GenerateUniqueDocument());
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
            "admin-user-id");

        await context.Barbershops.AddAsync(barbearia);

        var telefone = $"119{Random.Shared.Next(10000000, 99999999)}";
        const string nome = "Test Cliente";

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

    public static string GenerateAdminCentralToken(IServiceProvider services)
    {
        var jwtGenerator = services.GetRequiredService<IJwtTokenGenerator>();
        var token = jwtGenerator.GenerateToken(
            userId: Guid.NewGuid().ToString(),
            userType: "AdminCentral",
            email: "admin@test.com",
            barbeariaId: null);
        return token.Value;
    }

    public static async Task<Barbershop> CreateTestBarbershop(IServiceProvider services, string? codigo = null)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var document = Document.Create(GenerateUniqueDocument());
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        var uniqueCode = UniqueCode.Create(codigo ?? GenerateUniqueCode());
        var barbershop = Barbershop.Create(
            $"Test Barbearia {Guid.NewGuid()}",
            document,
            "11987654321",
            "João Silva",
            "joao@test.com",
            address,
            uniqueCode,
            "admin-user-id");

        await context.Barbershops.AddAsync(barbershop);
        await context.SaveChangesAsync();

        return barbershop;
    }

    public static async Task<Customer> CreateTestCustomer(IServiceProvider services, Guid barbershopId, string? telefone = null, string? nome = null)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var customer = Customer.Create(
            barbershopId,
            telefone ?? $"119{Random.Shared.Next(10000000, 99999999)}",
            nome ?? "Test Cliente");

        await context.Customers.AddAsync(customer);
        await context.SaveChangesAsync();

        return customer;
    }

    public static async Task<Barber> CreateTestBarber(IServiceProvider services, Guid barbershopId, bool ativo = true, string? email = null)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var barber = Barber.Create(
            barbershopId,
            "Test Barber",
            email ?? $"barber-{Guid.NewGuid()}@test.com",
            passwordHasher.Hash("Test@123"),
            $"119{Random.Shared.Next(10000000, 99999999)}");

        if (!ativo)
            barber.Deactivate();
        await context.Barbers.AddAsync(barber);
        await context.SaveChangesAsync();

        return barber;
    }

    public static async Task<BarbershopService> CreateTestBarbershopService(IServiceProvider services, Guid barbershopId, bool ativo = true)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var service = BarbershopService.Create(
            barbershopId,
            "Corte de Cabelo",
            "Corte masculino completo",
            30,
            25.00m);

        if (!ativo)
            service.Deactivate();
        await context.BarbershopServices.AddAsync(service);
        await context.SaveChangesAsync();

        return service;
    }

    public static async Task<Appointment> CreateTestAppointment(IServiceProvider services, Guid barbeariaId, Guid customerId, Guid barberId, Guid serviceId)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BarbAppDbContext>();

        var startTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        var endTime = startTime.AddMinutes(30);

        var appointment = Appointment.Create(
            barbeariaId,
            barberId,
            customerId,
            serviceId,
            startTime,
            endTime);

        await context.Appointments.AddAsync(appointment);
        await context.SaveChangesAsync();

        return appointment;
    }

    private static string GenerateUniqueCode()
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
    }

    private static string GenerateUniqueDocument()
    {
        var counter = Interlocked.Increment(ref _documentCounter);
        const long baseNumber = 10_000_000_000_000;
        return (baseNumber + counter).ToString();
    }
}
