---
status: pending
parallelizable: false
blocked_by: ["3.0"]
---

<task_context>
<domain>backend/api</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>low</complexity>
<dependencies>http_server</dependencies>
<unblocks>7.0, 14.0</unblocks>
</task_context>

# Tarefa 4.0: API - Endpoints AuthCliente (cadastro/login) + JWT + Swagger

## Visão Geral

Criar os endpoints REST de autenticação de clientes, configurar middleware JWT para validação de tokens, e documentar endpoints no Swagger. Esta camada expõe os use cases através de HTTP e gerencia autenticação/autorização.

<requirements>
- Controller AuthClienteController com endpoints POST cadastro e login
- Middleware de autenticação JWT
- Middleware de resolução de contexto tenant (barbeariaId do token)
- Tratamento global de exceções (Problem Details)
- Documentação Swagger com exemplos de request/response
- Retornar códigos HTTP corretos: 201 (cadastro), 200 (login), 404, 422, 401
- Validação automática de DTOs com FluentValidation
- Testes de integração para endpoints
</requirements>

## Subtarefas

- [ ] 4.1 Criar AuthClienteController com endpoints vazios
- [ ] 4.2 Implementar POST /api/auth/cliente/cadastro
- [ ] 4.3 Implementar POST /api/auth/cliente/login
- [ ] 4.4 Configurar middleware JWT Authentication
- [ ] 4.5 Criar middleware TenantContextMiddleware para extrair barbeariaId do token
- [ ] 4.6 Implementar ExceptionHandlerMiddleware com Problem Details
- [ ] 4.7 Configurar validação automática com FluentValidation
- [ ] 4.8 Documentar endpoints no Swagger com exemplos e descrições
- [ ] 4.9 Adicionar atributos [ProducesResponseType] para documentação
- [ ] 4.10 Criar testes de integração para POST cadastro (201, 404, 422)
- [ ] 4.11 Criar testes de integração para POST login (200, 401, 404)
- [ ] 4.12 Testar middleware de autenticação (token válido/inválido)
- [ ] 4.13 Configurar CORS se necessário

## Sequenciamento

- **Bloqueado por**: 3.0 (Use Cases de Autenticação)
- **Desbloqueia**: 7.0 (Endpoints de Agendamento), 14.0 (Frontend Setup)
- **Paralelizável**: Não (depende dos use cases estarem prontos)

## Detalhes de Implementação

### Controller AuthClienteController

```csharp
[ApiController]
[Route("api/auth/cliente")]
[Produces("application/json")]
public class AuthClienteController : ControllerBase
{
    private readonly ICadastrarClienteUseCase _cadastrarClienteUseCase;
    private readonly ILoginClienteUseCase _loginClienteUseCase;
    private readonly ILogger<AuthClienteController> _logger;

    public AuthClienteController(
        ICadastrarClienteUseCase cadastrarClienteUseCase,
        ILoginClienteUseCase loginClienteUseCase,
        ILogger<AuthClienteController> logger)
    {
        _cadastrarClienteUseCase = cadastrarClienteUseCase;
        _loginClienteUseCase = loginClienteUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Cadastrar novo cliente em uma barbearia
    /// </summary>
    /// <param name="input">Dados de cadastro (código barbearia, nome, telefone)</param>
    /// <returns>Token JWT e dados do cliente</returns>
    [HttpPost("cadastro")]
    [ProducesResponseType(typeof(CadastroClienteOutput), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<CadastroClienteOutput>> Cadastro(
        [FromBody] CadastrarClienteInput input,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tentativa de cadastro de cliente para barbearia {CodigoBarbearia}", input.CodigoBarbearia);
        
        var result = await _cadastrarClienteUseCase.Handle(input, cancellationToken);
        
        _logger.LogInformation("Cliente {ClienteId} cadastrado com sucesso na barbearia {BarbeariaId}", 
            result.Cliente.Id, result.Barbearia.Id);
        
        return CreatedAtAction(nameof(Cadastro), result);
    }

    /// <summary>
    /// Fazer login de cliente em uma barbearia
    /// </summary>
    /// <param name="input">Credenciais (código barbearia, telefone, nome)</param>
    /// <returns>Token JWT e dados do cliente</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginClienteOutput), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<LoginClienteOutput>> Login(
        [FromBody] LoginClienteInput input,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Tentativa de login de cliente para barbearia {CodigoBarbearia}", input.CodigoBarbearia);
        
        var result = await _loginClienteUseCase.Handle(input, cancellationToken);
        
        _logger.LogInformation("Cliente {ClienteId} logou com sucesso na barbearia {BarbeariaId}", 
            result.Cliente.Id, result.Barbearia.Id);
        
        return Ok(result);
    }
}
```

### Middleware TenantContextMiddleware

```csharp
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Extrair barbeariaId do token JWT
        var user = context.User;
        
        if (user?.Identity?.IsAuthenticated == true)
        {
            var barbeariaIdClaim = user.FindFirst("barbeariaId")?.Value;
            
            if (Guid.TryParse(barbeariaIdClaim, out var barbeariaId))
            {
                tenantContext.BarbeariaId = barbeariaId;
            }
        }

        await _next(context);
    }
}
```

### ExceptionHandlerMiddleware

```csharp
public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Erro não tratado: {Message}", exception.Message);

        var problemDetails = exception switch
        {
            BarbeariaNotFoundException ex => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Barbearia não encontrada",
                Detail = ex.Message,
                Instance = context.Request.Path
            },
            ClienteJaExisteException ex => new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Cliente já existe",
                Detail = ex.Message,
                Instance = context.Request.Path
            },
            UnauthorizedException ex => new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Não autorizado",
                Detail = ex.Message,
                Instance = context.Request.Path
            },
            ValidationException ex => new ValidationProblemDetails(
                ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray())
            )
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Erro de validação",
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Erro interno do servidor",
                Detail = "Ocorreu um erro inesperado. Por favor, tente novamente mais tarde.",
                Instance = context.Request.Path
            }
        };

        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

### Configuração Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "BarbApp API - Cliente", 
        Version = "v1",
        Description = "API para cadastro e agendamento de clientes em barbearias"
    });
    
    // JWT no Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando Bearer scheme. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
    
    // Incluir XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
        };
    });

builder.Services.AddAuthorization();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CadastrarClienteInputValidator>();
builder.Services.AddFluentValidationAutoValidation();

// AutoMapper
builder.Services.AddAutoMapper(typeof(ClienteProfile));

// Registrar Use Cases e Repositórios (já feito anteriormente)

var app = builder.Build();

// Middleware Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantContextMiddleware>();

app.MapControllers();

app.Run();
```

### Testes de Integração

```csharp
public class AuthClienteControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthClienteControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostCadastro_ComDadosValidos_DeveRetornar201()
    {
        // Arrange
        var request = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<CadastroClienteOutput>();
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Cliente.Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task PostCadastro_ComCodigoInvalido_DeveRetornar404()
    {
        // Arrange
        var request = new CadastrarClienteInput("INVALID", "João Silva", "11987654321");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem.Title.Should().Be("Barbearia não encontrada");
    }

    [Fact]
    public async Task PostCadastro_ComTelefoneDuplicado_DeveRetornar422()
    {
        // Arrange - Primeiro cadastro
        var request = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");
        await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", request);

        // Act - Segundo cadastro com mesmo telefone
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Fact]
    public async Task PostLogin_ComCredenciaisValidas_DeveRetornar200()
    {
        // Arrange - Cadastrar cliente primeiro
        var cadastroRequest = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");
        await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", cadastroRequest);
        
        var loginRequest = new LoginClienteInput("TEST123", "11987654321", "João Silva");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<LoginClienteOutput>();
        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostLogin_ComNomeIncorreto_DeveRetornar401()
    {
        // Arrange
        var cadastroRequest = new CadastrarClienteInput("TEST123", "João Silva", "11987654321");
        await _client.PostAsJsonAsync("/api/auth/cliente/cadastro", cadastroRequest);
        
        var loginRequest = new LoginClienteInput("TEST123", "11987654321", "Nome Errado");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/cliente/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
```

## Critérios de Sucesso

- ✅ Endpoints POST cadastro e login funcionando
- ✅ Retornando códigos HTTP corretos (201, 200, 404, 401, 422)
- ✅ Middleware JWT configurado e validando tokens
- ✅ TenantContextMiddleware extraindo barbeariaId do token
- ✅ ExceptionHandlerMiddleware tratando exceções e retornando Problem Details
- ✅ Validação automática de DTOs funcionando
- ✅ Swagger documentado com exemplos de request/response
- ✅ Testes de integração passando (cadastro e login)
- ✅ Logs estruturados implementados
- ✅ CORS configurado se necessário
