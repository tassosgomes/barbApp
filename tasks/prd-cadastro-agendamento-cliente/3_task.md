---
status: pending
parallelizable: false
blocked_by: ["1.0", "2.0"]
---

<task_context>
<domain>backend/application</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>4.0</unblocks>
</task_context>

# Tarefa 3.0: Application - DTOs, Validators e Use Cases de Autenticação (Cadastro/Login)

## Visão Geral

Implementar a camada de aplicação para autenticação de clientes, incluindo DTOs (Input/Output), validadores com FluentValidation, e os Use Cases de Cadastro e Login. Esta tarefa é responsável por orquestrar as regras de negócio e coordenar entidades e repositórios.

<requirements>
- DTOs: CadastrarClienteInput, CadastroClienteOutput, LoginClienteInput, LoginClienteOutput
- Validadores FluentValidation para inputs (telefone brasileiro, nome válido, código barbearia)
- Use Case: CadastrarClienteUseCase (verifica barbearia, valida telefone único, cria cliente, retorna token)
- Use Case: LoginClienteUseCase (valida telefone+nome, retorna token com contexto barbearia)
- Exceções customizadas: BarbeariaNotFoundException, ClienteJaExisteException, UnauthorizedException
- Testes unitários com mocks para todos os use cases
- Seguir padrão CQRS e Clean Architecture
</requirements>

## Subtarefas

- [ ] 3.1 Criar DTOs de Input: CadastrarClienteInput, LoginClienteInput
- [ ] 3.2 Criar DTOs de Output: CadastroClienteOutput, LoginClienteOutput, ClienteDto, BarbeariaDto
- [ ] 3.3 Criar validador FluentValidation para CadastrarClienteInput
- [ ] 3.4 Criar validador FluentValidation para LoginClienteInput
- [ ] 3.5 Criar exceções customizadas: BarbeariaNotFoundException, ClienteJaExisteException, UnauthorizedException
- [ ] 3.6 Implementar interface ICadastrarClienteUseCase
- [ ] 3.7 Implementar CadastrarClienteUseCase com validações de negócio
- [ ] 3.8 Implementar interface ILoginClienteUseCase
- [ ] 3.9 Implementar LoginClienteUseCase com validação de credenciais
- [ ] 3.10 Criar testes unitários para CadastrarClienteUseCase (sucesso, barbearia não encontrada, telefone duplicado)
- [ ] 3.11 Criar testes unitários para LoginClienteUseCase (sucesso, telefone inexistente, nome incorreto)
- [ ] 3.12 Registrar use cases no DI (Dependency Injection)
- [ ] 3.13 Configurar AutoMapper para mapeamento de entidades → DTOs

## Sequenciamento

- **Bloqueado por**: 1.0 (Entidades), 2.0 (Repositórios)
- **Desbloqueia**: 4.0 (Endpoints de Autenticação)
- **Paralelizável**: Parcialmente (pode começar DTOs enquanto 2.0 está em andamento)

## Detalhes de Implementação

### DTOs de Input

```csharp
public record CadastrarClienteInput(
    string CodigoBarbearia,
    string Nome,
    string Telefone
);

public record LoginClienteInput(
    string CodigoBarbearia,
    string Telefone,
    string Nome
);
```

### DTOs de Output

```csharp
public record CadastroClienteOutput(
    string Token,
    ClienteDto Cliente,
    BarbeariaDto Barbearia
);

public record LoginClienteOutput(
    string Token,
    ClienteDto Cliente,
    BarbeariaDto Barbearia
);

public record ClienteDto(
    Guid Id,
    string Nome,
    string Telefone
);

public record BarbeariaDto(
    Guid Id,
    string Nome,
    string Codigo
);
```

### Validadores FluentValidation

```csharp
public class CadastrarClienteInputValidator : AbstractValidator<CadastrarClienteInput>
{
    public CadastrarClienteInputValidator()
    {
        RuleFor(x => x.CodigoBarbearia)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório")
            .Length(8).WithMessage("Código da barbearia deve ter 8 caracteres");
        
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");
        
        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos");
    }
}

public class LoginClienteInputValidator : AbstractValidator<LoginClienteInput>
{
    public LoginClienteInputValidator()
    {
        RuleFor(x => x.CodigoBarbearia)
            .NotEmpty().WithMessage("Código da barbearia é obrigatório")
            .Length(8).WithMessage("Código da barbearia deve ter 8 caracteres");
        
        RuleFor(x => x.Telefone)
            .NotEmpty().WithMessage("Telefone é obrigatório")
            .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos");
        
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MinimumLength(2).WithMessage("Nome deve ter no mínimo 2 caracteres");
    }
}
```

### Exceções Customizadas

```csharp
public class BarbeariaNotFoundException : Exception
{
    public BarbeariaNotFoundException(string codigo) 
        : base($"Barbearia com código '{codigo}' não encontrada")
    {
    }
}

public class ClienteJaExisteException : Exception
{
    public ClienteJaExisteException(string telefone) 
        : base($"Cliente com telefone '{telefone}' já está cadastrado nesta barbearia")
    {
    }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string mensagem) 
        : base(mensagem)
    {
    }
}
```

### Interface e Implementação - CadastrarClienteUseCase

```csharp
public interface ICadastrarClienteUseCase
{
    Task<CadastroClienteOutput> Handle(CadastrarClienteInput input, CancellationToken cancellationToken = default);
}

public class CadastrarClienteUseCase : ICadastrarClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IBarbeariaRepository _barbeariaRepository;
    private readonly IJwtService _jwtService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CadastrarClienteUseCase(
        IClienteRepository clienteRepository,
        IBarbeariaRepository barbeariaRepository,
        IJwtService jwtService,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _barbeariaRepository = barbeariaRepository;
        _jwtService = jwtService;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CadastroClienteOutput> Handle(CadastrarClienteInput input, CancellationToken cancellationToken = default)
    {
        // 1. Validar se barbearia existe
        var barbearia = await _barbeariaRepository.GetByCodigoAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.Ativa)
        {
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Validar se telefone já está cadastrado nesta barbearia
        var clienteExistente = await _clienteRepository.ExisteAsync(barbearia.BarbeariaId, input.Telefone, cancellationToken);
        if (clienteExistente)
        {
            throw new ClienteJaExisteException(input.Telefone);
        }

        // 3. Criar cliente
        var telefone = new Telefone(input.Telefone);
        var cliente = new Cliente(barbearia.BarbeariaId, input.Nome, telefone);
        
        // 4. Persistir
        await _clienteRepository.AddAsync(cliente, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        // 5. Gerar token JWT com contexto da barbearia
        var token = _jwtService.GenerateToken(
            cliente.ClienteId,
            barbearia.BarbeariaId,
            "Cliente",
            cliente.Nome
        );

        // 6. Retornar output
        return new CadastroClienteOutput(
            token,
            _mapper.Map<ClienteDto>(cliente),
            _mapper.Map<BarbeariaDto>(barbearia)
        );
    }
}
```

### Interface e Implementação - LoginClienteUseCase

```csharp
public interface ILoginClienteUseCase
{
    Task<LoginClienteOutput> Handle(LoginClienteInput input, CancellationToken cancellationToken = default);
}

public class LoginClienteUseCase : ILoginClienteUseCase
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IBarbeariaRepository _barbeariaRepository;
    private readonly IJwtService _jwtService;
    private readonly IMapper _mapper;

    public LoginClienteUseCase(
        IClienteRepository clienteRepository,
        IBarbeariaRepository barbeariaRepository,
        IJwtService jwtService,
        IMapper mapper)
    {
        _clienteRepository = clienteRepository;
        _barbeariaRepository = barbeariaRepository;
        _jwtService = jwtService;
        _mapper = mapper;
    }

    public async Task<LoginClienteOutput> Handle(LoginClienteInput input, CancellationToken cancellationToken = default)
    {
        // 1. Validar se barbearia existe
        var barbearia = await _barbeariaRepository.GetByCodigoAsync(input.CodigoBarbearia, cancellationToken);
        if (barbearia == null || !barbearia.Ativa)
        {
            throw new BarbeariaNotFoundException(input.CodigoBarbearia);
        }

        // 2. Buscar cliente por telefone
        var cliente = await _clienteRepository.GetByTelefoneAsync(barbearia.BarbeariaId, input.Telefone, cancellationToken);
        if (cliente == null)
        {
            throw new UnauthorizedException("Telefone não cadastrado. Faça seu primeiro agendamento!");
        }

        // 3. Validar nome (case-insensitive)
        if (!cliente.ValidarNomeLogin(input.Nome))
        {
            throw new UnauthorizedException("Nome incorreto");
        }

        // 4. Gerar token JWT com contexto da barbearia
        var token = _jwtService.GenerateToken(
            cliente.ClienteId,
            barbearia.BarbeariaId,
            "Cliente",
            cliente.Nome
        );

        // 5. Retornar output
        return new LoginClienteOutput(
            token,
            _mapper.Map<ClienteDto>(cliente),
            _mapper.Map<BarbeariaDto>(barbearia)
        );
    }
}
```

### Testes Unitários (Exemplos)

```csharp
public class CadastrarClienteUseCaseTests
{
    private readonly Mock<IClienteRepository> _clienteRepositoryMock;
    private readonly Mock<IBarbeariaRepository> _barbeariaRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly CadastrarClienteUseCase _useCase;

    public CadastrarClienteUseCaseTests()
    {
        _clienteRepositoryMock = new Mock<IClienteRepository>();
        _barbeariaRepositoryMock = new Mock<IBarbeariaRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        
        _useCase = new CadastrarClienteUseCase(
            _clienteRepositoryMock.Object,
            _barbeariaRepositoryMock.Object,
            _jwtServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task Handle_ComDadosValidos_DeveCriarClienteERetornarToken()
    {
        // Arrange
        var input = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");
        var barbearia = new Barbearia { BarbeariaId = Guid.NewGuid(), Codigo = "TEST123", Ativa = true };
        
        _barbeariaRepositoryMock
            .Setup(x => x.GetByCodigoAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);
        
        _clienteRepositoryMock
            .Setup(x => x.ExisteAsync(barbearia.BarbeariaId, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        
        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("fake-jwt-token");

        // Act
        var result = await _useCase.Handle(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("fake-jwt-token", result.Token);
        _clienteRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Cliente>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ComCodigoInvalido_DeveLancarBarbeariaNotFoundException()
    {
        // Arrange
        var input = new CadastrarClienteInput("INVALID", "João Silva", "11987654321");
        
        _barbeariaRepositoryMock
            .Setup(x => x.GetByCodigoAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Barbearia)null);

        // Act & Assert
        await Assert.ThrowsAsync<BarbeariaNotFoundException>(() => _useCase.Handle(input));
    }

    [Fact]
    public async Task Handle_ComTelefoneDuplicado_DeveLancarClienteJaExisteException()
    {
        // Arrange
        var input = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");
        var barbearia = new Barbearia { BarbeariaId = Guid.NewGuid(), Codigo = "TEST123", Ativa = true };
        
        _barbeariaRepositoryMock
            .Setup(x => x.GetByCodigoAsync(input.CodigoBarbearia, It.IsAny<CancellationToken>()))
            .ReturnsAsync(barbearia);
        
        _clienteRepositoryMock
            .Setup(x => x.ExisteAsync(barbearia.BarbeariaId, input.Telefone, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ClienteJaExisteException>(() => _useCase.Handle(input));
    }
}

public class LoginClienteUseCaseTests
{
    // Similar structure with tests for:
    // - Handle_ComCredenciaisValidas_DeveRetornarToken
    // - Handle_ComTelefoneInexistente_DeveLancarUnauthorizedException
    // - Handle_ComNomeIncorreto_DeveLancarUnauthorizedException
}
```

### Configuração AutoMapper

```csharp
public class ClienteProfile : Profile
{
    public ClienteProfile()
    {
        CreateMap<Cliente, ClienteDto>()
            .ForMember(dest => dest.Telefone, opt => opt.MapFrom(src => src.Telefone.Formatado));
        
        CreateMap<Barbearia, BarbeariaDto>();
    }
}
```

## Critérios de Sucesso

- ✅ Todos os DTOs criados e compilando
- ✅ Validadores FluentValidation implementados e testados
- ✅ CadastrarClienteUseCase implementado com todas as validações
- ✅ LoginClienteUseCase implementado com validação de credenciais
- ✅ Exceções customizadas criadas e utilizadas corretamente
- ✅ Testes unitários com cobertura > 90% nos use cases
- ✅ Mocks configurados corretamente (sem dependências reais)
- ✅ AutoMapper configurado e funcionando
- ✅ Use cases registrados no DI
- ✅ Todos os testes passando
