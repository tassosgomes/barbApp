using BarbApp.Application.DTOs;
using BarbApp.Application.Interfaces;
using BarbApp.Application.Interfaces.UseCases;
using BarbApp.Domain.Entities;
using BarbApp.Domain.Exceptions;
using BarbApp.Domain.Interfaces.Repositories;
using BarbApp.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace BarbApp.Application.UseCases;

public class CreateBarbershopUseCase : ICreateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUniqueCodeGenerator _codeGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBarbershopUseCase> _logger;

    public CreateBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        IAddressRepository addressRepository,
        IUniqueCodeGenerator codeGenerator,
        IUnitOfWork unitOfWork,
        ILogger<CreateBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _addressRepository = addressRepository;
        _codeGenerator = codeGenerator;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BarbershopOutput> ExecuteAsync(CreateBarbershopInput input, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting creation of new barbershop with name {BarbershopName}", input.Name);

        // Validate if document already exists
        var existingBarbershop = await _barbershopRepository.GetByDocumentAsync(input.Document, cancellationToken);
        if (existingBarbershop != null)
        {
            _logger.LogWarning("Attempted to create barbershop with duplicate document");
            throw new DuplicateDocumentException("Já existe uma barbearia cadastrada com este documento");
        }

        // Create address
        var address = Address.Create(
            input.ZipCode.Replace("-", ""),
            input.Street,
            input.Number,
            input.Complement,
            input.Neighborhood,
            input.City,
            input.State);

        await _addressRepository.AddAsync(address, cancellationToken);
        _logger.LogInformation("Address created for barbershop");

        // Generate unique code
        var code = await _codeGenerator.GenerateAsync(cancellationToken);
        var uniqueCode = UniqueCode.Create(code);
        _logger.LogInformation("Unique code generated for barbershop");

        // Create document
        var document = Document.Create(input.Document);

        // Create barbershop
        var barbershop = Barbershop.Create(
            input.Name,
            document,
            input.Phone.Replace("(", "").Replace(")", "").Replace(" ", "").Replace("-", ""),
            input.OwnerName,
            input.Email,
            address,
            uniqueCode,
            "AdminCentral"); // TODO: Get from context

        await _barbershopRepository.InsertAsync(barbershop, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);

        _logger.LogInformation("Barbershop created successfully with ID {BarbershopId} and code {BarbershopCode}", barbershop.Id, barbershop.Code.Value);

        return new BarbershopOutput(
            barbershop.Id,
            barbershop.Name,
            barbershop.Document.Value,
            barbershop.Phone,
            barbershop.OwnerName,
            barbershop.Email,
            barbershop.Code.Value,
            barbershop.IsActive,
            new AddressOutput(
                barbershop.Address.ZipCode,
                barbershop.Address.Street,
                barbershop.Address.Number,
                barbershop.Address.Complement,
                barbershop.Address.Neighborhood,
                barbershop.Address.City,
                barbershop.Address.State),
            barbershop.CreatedAt,
            barbershop.UpdatedAt);
    }
}