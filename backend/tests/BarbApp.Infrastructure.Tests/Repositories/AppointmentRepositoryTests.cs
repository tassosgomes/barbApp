// BarbApp.Infrastructure.Tests/Repositories/AppointmentRepositoryTests.cs
using BarbApp.Domain.Entities;
using BarbApp.Domain.Enums;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class AppointmentRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly AppointmentRepository _repository;

    public AppointmentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new AppointmentRepository(_context);
    }

    private static Barbershop CreateTestBarbershop(string name, UniqueCode code)
    {
        var document = Document.Create("12345678000190");
        var address = Address.Create("01310100", "Av. Paulista", "1000", null, "Bela Vista", "São Paulo", "SP");
        return Barbershop.Create(name, document, "11987654321", "Test Owner", "test@test.com", address, code, "test-user");
    }

    private static Customer CreateTestCustomer(Guid barbeariaId, string name, string phone)
    {
        return Customer.Create(barbeariaId, phone, name);
    }

    private static BarbershopService CreateTestService(Guid barbeariaId, string name, decimal price, int duration)
    {
        return BarbershopService.Create(barbeariaId, name, null, duration, price);
    }

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentExists_ReturnsAppointment()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber = Barber.Create(barbearia.Id, "João Barber", "joao@test.com", "hashedpassword", "11999999999");
        await _context.Barbers.AddAsync(barber);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente Teste", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(30);
        var appointment = Appointment.Create(
            barbearia.Id,
            barber.Id,
            customer.Id,
            service.Id,
            startTime,
            endTime);

        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(appointment.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(appointment.Id);
        result.BarbeariaId.Should().Be(barbearia.Id);
        result.BarberId.Should().Be(barber.Id);
        result.CustomerId.Should().Be(customer.Id);
        result.ServiceId.Should().Be(service.Id);
        result.Status.Should().Be(AppointmentStatus.Pending);
        result.Service.Should().NotBeNull();
        result.Barber.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByIdAsync_WhenAppointmentDoesNotExist_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBarberAndDateAsync_ReturnsAppointmentsForBarberOnSpecificDate()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber1 = Barber.Create(barbearia.Id, "João", "joao@test.com", "hash1", "11999999991");
        var barber2 = Barber.Create(barbearia.Id, "Maria", "maria@test.com", "hash2", "11999999992");
        await _context.Barbers.AddRangeAsync(barber1, barber2);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2025, 10, 25, 0, 0, 0, DateTimeKind.Utc);
        var previousDate = targetDate.AddDays(-1);
        var nextDate = targetDate.AddDays(1);

        // Appointments for barber1 on target date
        var appointment1 = Appointment.Create(
            barbearia.Id, barber1.Id, customer.Id, service.Id,
            targetDate.AddHours(10), targetDate.AddHours(10.5));

        var appointment2 = Appointment.Create(
            barbearia.Id, barber1.Id, customer.Id, service.Id,
            targetDate.AddHours(14), targetDate.AddHours(14.5));

        // Appointment for barber1 on previous date (should not be returned)
        var appointment3 = Appointment.Create(
            barbearia.Id, barber1.Id, customer.Id, service.Id,
            previousDate.AddHours(10), previousDate.AddHours(10.5));

        // Appointment for barber2 on target date (should not be returned)
        var appointment4 = Appointment.Create(
            barbearia.Id, barber2.Id, customer.Id, service.Id,
            targetDate.AddHours(10), targetDate.AddHours(10.5));

        // Appointment for barber1 on next date (should not be returned)
        var appointment5 = Appointment.Create(
            barbearia.Id, barber1.Id, customer.Id, service.Id,
            nextDate.AddHours(10), nextDate.AddHours(10.5));

        await _context.Appointments.AddRangeAsync(appointment1, appointment2, appointment3, appointment4, appointment5);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByBarberAndDateAsync(barbearia.Id, barber1.Id, targetDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == appointment1.Id);
        result.Should().Contain(a => a.Id == appointment2.Id);
        result.Should().NotContain(a => a.Id == appointment3.Id);
        result.Should().NotContain(a => a.Id == appointment4.Id);
        result.Should().NotContain(a => a.Id == appointment5.Id);
        result.Should().BeInAscendingOrder(a => a.StartTime);
    }

    [Fact]
    public async Task GetByBarberAndDateAsync_WhenNoAppointments_ReturnsEmptyList()
    {
        // Arrange
        var barbeariaId = Guid.NewGuid();
        var barberId = Guid.NewGuid();
        var date = DateTime.UtcNow.Date;

        // Act
        var result = await _repository.GetByBarberAndDateAsync(barbeariaId, barberId, date);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task InsertAsync_AddsAppointmentToDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber = Barber.Create(barbearia.Id, "João", "joao@test.com", "hash", "11999999999");
        await _context.Barbers.AddAsync(barber);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(30);
        var appointment = Appointment.Create(
            barbearia.Id, barber.Id, customer.Id, service.Id,
            startTime, endTime);

        // Act
        await _repository.InsertAsync(appointment);
        await _context.SaveChangesAsync();

        // Assert
        var savedAppointment = await _context.Appointments.FindAsync(appointment.Id);
        savedAppointment.Should().NotBeNull();
        savedAppointment!.BarberId.Should().Be(barber.Id);
        savedAppointment.Status.Should().Be(AppointmentStatus.Pending);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesAppointmentInDatabase()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber = Barber.Create(barbearia.Id, "João", "joao@test.com", "hash", "11999999999");
        await _context.Barbers.AddAsync(barber);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(30);
        var appointment = Appointment.Create(
            barbearia.Id, barber.Id, customer.Id, service.Id,
            startTime, endTime);

        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();

        // Act
        appointment.Confirm();
        await _repository.UpdateAsync(appointment);
        await _context.SaveChangesAsync();

        // Assert
        var updatedAppointment = await _context.Appointments.FindAsync(appointment.Id);
        updatedAppointment.Should().NotBeNull();
        updatedAppointment!.Status.Should().Be(AppointmentStatus.Confirmed);
        updatedAppointment.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFutureAppointmentsByBarberAsync_ReturnsFutureAppointmentsOnly()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber = Barber.Create(barbearia.Id, "João", "joao@test.com", "hash", "11999999999");
        await _context.Barbers.AddAsync(barber);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var now = DateTime.UtcNow;

        // Future appointment
        var futureAppointment = Appointment.Create(
            barbearia.Id, barber.Id, customer.Id, service.Id,
            now.AddDays(1), now.AddDays(1).AddMinutes(30));

        // Past appointment
        var pastAppointment = Appointment.Create(
            barbearia.Id, barber.Id, customer.Id, service.Id,
            now.AddDays(-1), now.AddDays(-1).AddMinutes(30));

        await _context.Appointments.AddRangeAsync(futureAppointment, pastAppointment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetFutureAppointmentsByBarberAsync(barber.Id);

        // Assert
        result.Should().HaveCount(1);
        result.Should().Contain(a => a.Id == futureAppointment.Id);
        result.Should().NotContain(a => a.Id == pastAppointment.Id);
    }

    [Fact]
    public async Task GetAppointmentsByBarbeariaAndDateAsync_ReturnsAllAppointmentsForBarbeariaOnDate()
    {
        // Arrange
        var barbeariaCode1 = UniqueCode.Create("ABCD2345");
        var barbearia1 = CreateTestBarbershop("Barbearia 1", barbeariaCode1);
        var barbeariaCode2 = UniqueCode.Create("XYZ98765");
        var barbearia2 = CreateTestBarbershop("Barbearia 2", barbeariaCode2);
        await _context.Barbershops.AddRangeAsync(barbearia1, barbearia2);

        var barber1 = Barber.Create(barbearia1.Id, "João", "joao@test.com", "hash1", "11999999991");
        var barber2 = Barber.Create(barbearia1.Id, "Maria", "maria@test.com", "hash2", "11999999992");
        await _context.Barbers.AddRangeAsync(barber1, barber2);

        var customer1 = CreateTestCustomer(barbearia1.Id, "Cliente 1", "11988888881");
        var customer2 = CreateTestCustomer(barbearia2.Id, "Cliente 2", "11988888882");
        await _context.Customers.AddRangeAsync(customer1, customer2);

        var service1 = CreateTestService(barbearia1.Id, "Corte", 50.00m, 30);
        var service2 = CreateTestService(barbearia2.Id, "Barba", 30.00m, 20);
        await _context.BarbershopServices.AddRangeAsync(service1, service2);

        await _context.SaveChangesAsync();

        var targetDate = new DateTime(2025, 10, 25, 0, 0, 0, DateTimeKind.Utc);

        // Appointments for barbearia1 on target date
        var appointment1 = Appointment.Create(
            barbearia1.Id, barber1.Id, customer1.Id, service1.Id,
            targetDate.AddHours(10), targetDate.AddHours(10.5));

        var appointment2 = Appointment.Create(
            barbearia1.Id, barber2.Id, customer1.Id, service1.Id,
            targetDate.AddHours(14), targetDate.AddHours(14.5));

        // Appointment for barbearia2 on target date (should not be returned)
        var appointment3 = Appointment.Create(
            barbearia2.Id, barber1.Id, customer2.Id, service2.Id,
            targetDate.AddHours(10), targetDate.AddHours(10.5));

        await _context.Appointments.AddRangeAsync(appointment1, appointment2, appointment3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAppointmentsByBarbeariaAndDateAsync(barbearia1.Id, targetDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(a => a.Id == appointment1.Id);
        result.Should().Contain(a => a.Id == appointment2.Id);
        result.Should().NotContain(a => a.Id == appointment3.Id);
        result.Should().BeInAscendingOrder(a => a.StartTime);
    }

    [Fact]
    public async Task UpdateStatusAsync_UpdatesAppointmentStatusUsingDomainMethods()
    {
        // Arrange
        var barbeariaCode = UniqueCode.Create("ABCD2345");
        var barbearia = CreateTestBarbershop("Barbearia Teste", barbeariaCode);
        await _context.Barbershops.AddAsync(barbearia);

        var barber = Barber.Create(barbearia.Id, "João", "joao@test.com", "hash", "11999999999");
        await _context.Barbers.AddAsync(barber);

        var customer = CreateTestCustomer(barbearia.Id, "Cliente", "11988888888");
        await _context.Customers.AddAsync(customer);

        var service = CreateTestService(barbearia.Id, "Corte", 50.00m, 30);
        await _context.BarbershopServices.AddAsync(service);

        await _context.SaveChangesAsync();

        var startTime = DateTime.UtcNow.AddDays(1);
        var endTime = startTime.AddMinutes(30);
        var appointment = Appointment.Create(
            barbearia.Id, barber.Id, customer.Id, service.Id,
            startTime, endTime);

        await _context.Appointments.AddAsync(appointment);
        await _context.SaveChangesAsync();

        // Act
        await _repository.UpdateStatusAsync(new[] { appointment }, "confirmed");
        await _context.SaveChangesAsync();

        // Assert
        var updatedAppointment = await _context.Appointments.FindAsync(appointment.Id);
        updatedAppointment.Should().NotBeNull();
        updatedAppointment!.Status.Should().Be(AppointmentStatus.Confirmed);
        updatedAppointment.ConfirmedAt.Should().NotBeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
