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
    private readonly IAdminBarbeariaUserRepository _adminBarbeariaUserRepository;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IUniqueCodeGenerator _codeGenerator;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBarbershopUseCase> _logger;

    public CreateBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        IAddressRepository addressRepository,
        IAdminBarbeariaUserRepository adminBarbeariaUserRepository,
        IPasswordGenerator passwordGenerator,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IUniqueCodeGenerator codeGenerator,
        IUnitOfWork unitOfWork,
        ILogger<CreateBarbershopUseCase> logger)
    {
        _barbershopRepository = barbershopRepository;
        _addressRepository = addressRepository;
        _adminBarbeariaUserRepository = adminBarbeariaUserRepository;
        _passwordGenerator = passwordGenerator;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
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

        // Validate if email already exists
        var existingAdmin = await _adminBarbeariaUserRepository.GetByEmailAsync(input.Email, cancellationToken);
        if (existingAdmin != null)
        {
            _logger.LogWarning("Attempted to create barbershop with duplicate admin email {Email}", input.Email);
            throw new ConflictException($"Já existe um administrador cadastrado com o e-mail '{input.Email}'");
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

        // Generate secure password for Admin Barbearia
        var password = _passwordGenerator.Generate();
        _logger.LogInformation("Generated secure password for Admin Barbearia of barbershop {BarbershopId}", barbershop.Id);

        // Hash the password
        var passwordHash = _passwordHasher.Hash(password);

        // Create Admin Barbearia user
        var adminUser = AdminBarbeariaUser.Create(
            barbershop.Id,
            input.Email,
            passwordHash,
            input.OwnerName);

        await _adminBarbeariaUserRepository.AddAsync(adminUser, cancellationToken);
        _logger.LogInformation("Admin Barbearia user created with ID {AdminId} for barbershop {BarbershopId}", adminUser.Id, barbershop.Id);

        try
        {
            // Send welcome email with credentials
            var emailMessage = new EmailMessage
            {
                To = input.Email,
                Subject = "Bem-vindo ao BarbApp - Suas credenciais de acesso",
                HtmlBody = BuildWelcomeEmailHtml(barbershop.Name, input.Email, password),
                TextBody = BuildWelcomeEmailText(barbershop.Name, input.Email, password)
            };

            await _emailService.SendAsync(emailMessage, cancellationToken);
            _logger.LogInformation("Welcome email sent successfully to {Email} for barbershop {BarbershopId}", input.Email, barbershop.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email} for barbershop {BarbershopId}. Rolling back transaction.", input.Email, barbershop.Id);
            throw new InvalidOperationException($"Falha ao enviar e-mail de boas-vindas. Por favor, tente novamente ou contate o suporte.", ex);
        }

        // Commit transaction only after email is sent successfully
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

    private static string BuildWelcomeEmailHtml(string barbershopName, string email, string password)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Bem-vindo ao BarbApp</title>
</head>
<body style=""font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;"">
    <div style=""background-color: #f8f9fa; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h1 style=""color: #2c3e50; margin-bottom: 10px;"">Bem-vindo ao BarbApp! 💈</h1>
        <p style=""font-size: 16px; margin-bottom: 20px;"">
            Olá! Sua barbearia <strong>{barbershopName}</strong> foi cadastrada com sucesso.
        </p>
    </div>

    <div style=""background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h2 style=""color: #2c3e50; margin-bottom: 15px;"">Suas Credenciais de Acesso</h2>
        <p style=""margin-bottom: 15px;"">Use as credenciais abaixo para acessar o painel administrativo:</p>
        
        <div style=""background-color: #f8f9fa; border-left: 4px solid #007bff; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 5px 0;""><strong>E-mail:</strong> {email}</p>
            <p style=""margin: 5px 0;""><strong>Senha:</strong> <code style=""background-color: #e9ecef; padding: 5px 10px; border-radius: 3px; font-size: 14px; font-family: 'Courier New', monospace;"">{password}</code></p>
        </div>

        <div style=""background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 5px;"">
            <p style=""margin: 0; color: #856404;"">
                <strong>⚠️ Importante:</strong> Guarde sua senha em local seguro. Recomendamos alterá-la após o primeiro acesso.
            </p>
        </div>
    </div>

    <div style=""background-color: #fff; border: 1px solid #dee2e6; border-radius: 10px; padding: 30px; margin-bottom: 20px;"">
        <h2 style=""color: #2c3e50; margin-bottom: 15px;"">Próximos Passos</h2>
        <ol style=""margin: 0; padding-left: 20px;"">
            <li style=""margin-bottom: 10px;"">Acesse o painel administrativo do BarbApp</li>
            <li style=""margin-bottom: 10px;"">Cadastre seus barbeiros</li>
            <li style=""margin-bottom: 10px;"">Configure seus serviços e horários</li>
            <li>Comece a receber agendamentos!</li>
        </ol>
    </div>

    <div style=""text-align: center; color: #6c757d; font-size: 14px; padding-top: 20px; border-top: 1px solid #dee2e6;"">
        <p>Caso tenha alguma dúvida, entre em contato com nosso suporte.</p>
        <p style=""margin: 5px 0;"">BarbApp - Gestão de Barbearias</p>
    </div>
</body>
</html>";
    }

    private static string BuildWelcomeEmailText(string barbershopName, string email, string password)
    {
        return $@"Bem-vindo ao BarbApp! 

Sua barbearia ""{barbershopName}"" foi cadastrada com sucesso.

=== CREDENCIAIS DE ACESSO ===

E-mail: {email}
Senha: {password}

⚠️ IMPORTANTE: Guarde sua senha em local seguro. Recomendamos alterá-la após o primeiro acesso.

=== PRÓXIMOS PASSOS ===

1. Acesse o painel administrativo do BarbApp
2. Cadastre seus barbeiros
3. Configure seus serviços e horários
4. Comece a receber agendamentos!

Caso tenha alguma dúvida, entre em contato com nosso suporte.

BarbApp - Gestão de Barbearias";
    }
}