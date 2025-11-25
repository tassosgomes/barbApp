using BarbApp.IntegrationTests;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace BarbApp.IntegrationTests.Repositories
{
    [Collection(nameof(IntegrationTestCollection))]
    public class RepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly DatabaseFixture _dbFixture;
        private IServiceScope _scope;
        private IBarbershopRepository _barbershopRepository;
        private ICustomerRepository _customerRepository;
        private IBarberRepository _barberRepository;
        private IAppointmentRepository _appointmentRepository;
        private IBarbershopServiceRepository _barbershopServiceRepository;

        public RepositoryIntegrationTests(DatabaseFixture dbFixture)
        {
            _dbFixture = dbFixture;
            _factory = dbFixture.CreateFactory();
        }

        public async Task InitializeAsync()
        {
            _factory.EnsureDatabaseInitialized();
            _scope = _factory.Services.CreateScope();

            _barbershopRepository = _scope.ServiceProvider.GetRequiredService<IBarbershopRepository>();
            _customerRepository = _scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
            _barberRepository = _scope.ServiceProvider.GetRequiredService<IBarberRepository>();
            _appointmentRepository = _scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            _barbershopServiceRepository = _scope.ServiceProvider.GetRequiredService<IBarbershopServiceRepository>();
        }

        public async Task DisposeAsync()
        {
            _scope?.Dispose();
        }

        [Fact]
        public async Task BarbershopRepository_GetByCodeAsync_WithValidCode_ReturnsBarbershop()
        {
            // Arrange
            var barbershop = await TestHelper.CreateTestBarbershop(_factory.Services);

            // Act
            var result = await _barbershopRepository.GetByCodeAsync(barbershop.Code.Value, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(barbershop.Id);
            result.Code.Value.Should().Be(barbershop.Code.Value);
        }

        [Fact]
        public async Task BarbershopRepository_GetByCodeAsync_WithInvalidCode_ReturnsNull()
        {
            // Act
            var result = await _barbershopRepository.GetByCodeAsync("INVALID", CancellationToken.None);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task CustomerRepository_GetByTelefoneAndBarbeariaIdAsync_WithValidData_ReturnsCustomer()
        {
            // Arrange
            var barbershop = await TestHelper.CreateTestBarbershop(_factory.Services);
            var customer = await TestHelper.CreateTestCustomer(_factory.Services, barbershop.Id);

            // Act
            var result = await _customerRepository.GetByTelefoneAndBarbeariaIdAsync(customer.Telefone, barbershop.Id, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(customer.Id);
            result.Telefone.Should().Be(customer.Telefone);
        }

        [Fact]
        public async Task BarberRepository_ListAsync_WithBarbeariaId_ReturnsActiveBarbers()
        {
            // Arrange
            var barbershop = await TestHelper.CreateTestBarbershop(_factory.Services);
            var barber1 = await TestHelper.CreateTestBarber(_factory.Services, barbershop.Id, true);
            var barber2 = await TestHelper.CreateTestBarber(_factory.Services, barbershop.Id, true);
            var inactiveBarber = await TestHelper.CreateTestBarber(_factory.Services, barbershop.Id, false);

            // Act
            var result = await _barberRepository.ListAsync(barbershop.Id, isActive: true, cancellationToken: CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(b => b.Id == barber1.Id);
            result.Should().Contain(b => b.Id == barber2.Id);
            result.Should().NotContain(b => b.Id == inactiveBarber.Id);
        }

        [Fact]
        public async Task AppointmentRepository_GetByBarberAndDateAsync_WithValidData_ReturnsAppointments()
        {
            // Arrange
            var barbershop = await TestHelper.CreateTestBarbershop(_factory.Services);
            var customer = await TestHelper.CreateTestCustomer(_factory.Services, barbershop.Id);
            var barber = await TestHelper.CreateTestBarber(_factory.Services, barbershop.Id);
            var service = await TestHelper.CreateTestBarbershopService(_factory.Services, barbershop.Id);
            var appointment = await TestHelper.CreateTestAppointment(_factory.Services, barbershop.Id, customer.Id, barber.Id, service.Id);

            var date = appointment.StartTime.Date;

            // Act
            var result = await _appointmentRepository.GetByBarberAndDateAsync(barbershop.Id, barber.Id, date, CancellationToken.None);

            // Assert
            result.Should().HaveCount(1);
            result[0].Id.Should().Be(appointment.Id);
        }

        [Fact]
        public async Task BarbershopServiceRepository_ListAsync_WithBarbeariaId_ReturnsActiveServices()
        {
            // Arrange
            var barbershop = await TestHelper.CreateTestBarbershop(_factory.Services);
            var service1 = await TestHelper.CreateTestBarbershopService(_factory.Services, barbershop.Id, true);
            var service2 = await TestHelper.CreateTestBarbershopService(_factory.Services, barbershop.Id, true);
            var inactiveService = await TestHelper.CreateTestBarbershopService(_factory.Services, barbershop.Id, false);

            // Act
            var result = await _barbershopServiceRepository.ListAsync(barbershop.Id, isActive: true, CancellationToken.None);

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(s => s.Id == service1.Id);
            result.Should().Contain(s => s.Id == service2.Id);
            result.Should().NotContain(s => s.Id == inactiveService.Id);
        }

        [Fact]
        public async Task MultiTenant_Isolation_BarbershopRepository_OnlyReturnsOwnData()
        {
            // Arrange
            var barbershop1 = await TestHelper.CreateTestBarbershop(_factory.Services, "BARB2345");
            var barbershop2 = await TestHelper.CreateTestBarbershop(_factory.Services, "BARB6789");

            // Act
            var result1 = await _barbershopRepository.GetByCodeAsync("BARB2345", CancellationToken.None);
            var result2 = await _barbershopRepository.GetByCodeAsync("BARB6789", CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result1!.Id.Should().Be(barbershop1.Id);
            result2.Should().NotBeNull();
            result2!.Id.Should().Be(barbershop2.Id);
            result1.Id.Should().NotBe(result2.Id);
        }

        [Fact]
        public async Task MultiTenant_Isolation_CustomerRepository_OnlyReturnsOwnBarbeariaData()
        {
            // Arrange
            var barbershop1 = await TestHelper.CreateTestBarbershop(_factory.Services);
            var barbershop2 = await TestHelper.CreateTestBarbershop(_factory.Services);
            var customer1 = await TestHelper.CreateTestCustomer(_factory.Services, barbershop1.Id, "11911111111");
            var customer2 = await TestHelper.CreateTestCustomer(_factory.Services, barbershop2.Id, "11922222222");

            // Act
            var result1 = await _customerRepository.GetByTelefoneAndBarbeariaIdAsync("11911111111", barbershop1.Id, CancellationToken.None);
            var result2 = await _customerRepository.GetByTelefoneAndBarbeariaIdAsync("11922222222", barbershop2.Id, CancellationToken.None);
            var crossResult = await _customerRepository.GetByTelefoneAndBarbeariaIdAsync("11911111111", barbershop2.Id, CancellationToken.None);

            // Assert
            result1.Should().NotBeNull();
            result1!.Id.Should().Be(customer1.Id);
            result2.Should().NotBeNull();
            result2!.Id.Should().Be(customer2.Id);
            crossResult.Should().BeNull(); // Customer from barbershop1 should not be visible in barbershop2
        }
    }
}