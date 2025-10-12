// BarbApp.Infrastructure.Tests/Repositories/BarbershopRepositoryTests.cs
using System.Linq;
using System.Threading;
using BarbApp.Domain.Entities;
using BarbApp.Domain.ValueObjects;
using BarbApp.Infrastructure.Persistence;
using BarbApp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Tests.Repositories;

public class BarbershopRepositoryTests : IDisposable
{
    private readonly TestBarbAppDbContext _context;
    private readonly BarbershopRepository _repository;

    public BarbershopRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BarbAppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestBarbAppDbContext(options);
        _repository = new BarbershopRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_WhenBarbershopExists_ReturnsEntityWithAddress()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Barbearia Central",
            code: "ABCDEF23",
            document: "12345678000190",
            email: "central@test.com");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(barbershop.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(barbershop.Id);
        result.Address.Should().NotBeNull();
        result.Address.ZipCode.Should().Be(barbershop.Address.ZipCode);
    }

    [Fact]
    public async Task GetByCodeAsync_WhenBarbershopExists_ReturnsEntity()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Barbearia Código",
            code: "ZXCVBN23",
            document: "22345678000191",
            email: "code@test.com");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByCodeAsync(barbershop.Code.Value, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Code.Value.Should().Be(barbershop.Code.Value);
    }

    [Fact]
    public async Task GetByDocumentAsync_WhenBarbershopExists_ReturnsEntity()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Barbearia Documento",
            code: "QWERTY45",
            document: "32345678000192",
            email: "document@test.com");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByDocumentAsync(barbershop.Document.Value, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Document.Value.Should().Be(barbershop.Document.Value);
    }

    [Fact]
    public async Task ListAsync_WithFiltersAndSorting_ReturnsPagedResult()
    {
        // Arrange
        var activeBarbershop = CreateBarbershop(
            name: "Alpha Cuts",
            code: "ACTUVS23",
            document: "42345678000193",
            email: "alpha@test.com");

        var inactiveBarbershop = CreateBarbershop(
            name: "Beta Groomers",
            code: "NACTVU24",
            document: "52345678000194",
            email: "beta@test.com");
        inactiveBarbershop.Deactivate();

        var anotherActive = CreateBarbershop(
            name: "Gamma Styles",
            code: "ACRTUV35",
            document: "62345678000195",
            email: "gamma@test.com");

        await _context.Barbershops.AddRangeAsync(activeBarbershop, inactiveBarbershop, anotherActive);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.ListAsync(
            page: 1,
            pageSize: 2,
            searchTerm: "a",
            isActive: true,
            sortBy: "name",
            cancellationToken: CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(item => item.Address.Should().NotBeNull());
        result.Items.Select(item => item.Name).Should().ContainInOrder("Alpha Cuts", "Gamma Styles");
    }

    [Fact]
    public async Task ListAsync_WhenPageIsInvalid_ThrowsArgumentException()
    {
        // Act
        var act = async () => await _repository.ListAsync(0, 10, null, null, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Page must be >= 1*");
    }

    [Fact]
    public async Task ListAsync_WhenPageSizeIsInvalid_ThrowsArgumentException()
    {
        // Act
        var act = async () => await _repository.ListAsync(1, 0, null, null, null, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("PageSize must be between 1 and 100*");
    }

    [Fact]
    public async Task InsertAsync_AddsBarbershopToContext()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Nova Barbearia",
            code: "NEWRCD24",
            document: "72345678000196",
            email: "nova@test.com");

        // Act
        await _repository.InsertAsync(barbershop, CancellationToken.None);

        // Assert
        var entry = _context.ChangeTracker.Entries<Barbershop>().Single();
        entry.State.Should().Be(EntityState.Added);
        entry.Entity.Address.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_MarksEntityAsModified()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Barbearia Original",
            code: "NRGCADE2",
            document: "82345678000197",
            email: "original@test.com");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        var updatedAddress = Address.Create(
            "02020020",
            "Rua Atualizada",
            "200",
            "Sala 2",
            "Centro",
            "Rio de Janeiro",
            "RJ");

        barbershop.Update(
            name: "Barbearia Atualizada",
            phone: "11988887777",
            ownerName: "Novo Dono",
            email: "atualizada@test.com",
            address: updatedAddress,
            updatedBy: "tester");

        // Act
        await _repository.UpdateAsync(barbershop, CancellationToken.None);

        // Assert
        var entry = _context.ChangeTracker.Entries<Barbershop>().Single();
        entry.State.Should().Be(EntityState.Modified);
        entry.Entity.Address.Should().Be(updatedAddress);
    }

    [Fact]
    public async Task DeleteAsync_MarksEntityAsDeleted()
    {
        // Arrange
        var barbershop = CreateBarbershop(
            name: "Barbearia Para Remover",
            code: "DELRCD23",
            document: "92345678000198",
            email: "remover@test.com");

        await _context.Barbershops.AddAsync(barbershop);
        await _context.SaveChangesAsync();

        // Act
        await _repository.DeleteAsync(barbershop, CancellationToken.None);

        // Assert
        var entry = _context.ChangeTracker.Entries<Barbershop>().Single();
        entry.State.Should().Be(EntityState.Deleted);
    }

    private static Barbershop CreateBarbershop(string name, string code, string document, string email)
    {
        var documentVo = Document.Create(document);
        var address = Address.Create(
            zipCode: "01001000",
            street: "Av. Principal",
            number: Guid.NewGuid().ToString()[..4],
            complement: null,
            neighborhood: "Centro",
            city: "São Paulo",
            state: "SP");
        var uniqueCode = UniqueCode.Create(code);

        return Barbershop.Create(
            name: name,
            document: documentVo,
            phone: "11999999999",
            ownerName: "Owner Test",
            email: email,
            address: address,
            code: uniqueCode,
            createdBy: "tester");
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
