---
status: pending
parallelizable: true
blocked_by: ["4.0"]
---

<task_context>
<domain>Infraestrutura de Segurança</domain>
<type>Implementação</type>
<scope>JWT, Criptografia, Tenant Context</scope>
<complexity>média</complexity>
<dependencies>System.IdentityModel.Tokens.Jwt, BCrypt.Net</dependencies>
<unblocks>"7.0", "9.0"</unblocks>
</task_context>

# Tarefa 8.0: Implementar JWT e Serviços de Segurança

## Visão Geral
Implementar os serviços fundamentais de segurança: geração e validação de tokens JWT, hash de senhas com BCrypt e gerenciamento de contexto de tenant (TenantContext).

<requirements>
- JwtTokenGenerator com geração e validação de tokens
- PasswordHasher usando BCrypt para hash seguro
- TenantContext para gerenciamento de contexto de tenant
- Configurações de JWT (secret, issuer, audience, expiration)
- Claims customizados para tipo de usuário e tenant
- Thread-safe tenant context usando AsyncLocal
</requirements>

## Subtarefas
- [ ] 8.1 Implementar JwtTokenGenerator
- [ ] 8.2 Implementar PasswordHasher com BCrypt
- [ ] 8.3 Implementar TenantContext com AsyncLocal
- [ ] 8.4 Criar configurações de JWT (appsettings.json)
- [ ] 8.5 Criar testes unitários para cada serviço
- [ ] 8.6 Validar thread-safety do TenantContext

## Sequenciamento
- **Bloqueado por**: 4.0 (Interfaces do domínio)
- **Desbloqueia**: 7.0 (Use Cases), 9.0 (Middlewares)
- **Paralelizável**: Sim (pode ser desenvolvido em paralelo com 5.0, 6.0)

## Detalhes de Implementação

### JwtTokenGenerator

```csharp
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public JwtToken GenerateToken(
        Guid userId,
        string userType,
        string email,
        Guid? barbeariaId)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("user_type", userType),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (barbeariaId.HasValue)
        {
            claims.Add(new Claim("barbearia_id", barbeariaId.Value.ToString()));
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.Secret));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

        return new JwtToken(tokenValue, expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }
}

// JwtToken record
public record JwtToken(string Value, DateTime ExpiresAt);

// JwtSettings
public class JwtSettings
{
    public string Secret { get; init; } = string.Empty;
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 60;
}
```

### PasswordHasher

```csharp
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password cannot be null or whitespace",
                nameof(password));
        }

        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException(
                "Password cannot be null or whitespace",
                nameof(password));
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException(
                "Password hash cannot be null or whitespace",
                nameof(passwordHash));
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }
        catch
        {
            return false;
        }
    }
}
```

### TenantContext

```csharp
public class TenantContext : ITenantContext
{
    private static readonly AsyncLocal<TenantInfo?> _currentTenant = new();

    public void SetCurrentTenant(
        Guid? barbeariaId,
        string userType,
        Guid userId,
        string email)
    {
        _currentTenant.Value = new TenantInfo
        {
            BarbeariaId = barbeariaId,
            UserType = userType,
            UserId = userId,
            Email = email
        };
    }

    public Guid? GetCurrentBarbeariaId()
    {
        return _currentTenant.Value?.BarbeariaId;
    }

    public string? GetCurrentUserType()
    {
        return _currentTenant.Value?.UserType;
    }

    public Guid? GetCurrentUserId()
    {
        return _currentTenant.Value?.UserId;
    }

    public string? GetCurrentUserEmail()
    {
        return _currentTenant.Value?.Email;
    }

    public void Clear()
    {
        _currentTenant.Value = null;
    }

    private class TenantInfo
    {
        public Guid? BarbeariaId { get; init; }
        public string UserType { get; init; } = string.Empty;
        public Guid UserId { get; init; }
        public string Email { get; init; } = string.Empty;
    }
}
```

### Configuração JWT (appsettings.json)

```json
{
  "JwtSettings": {
    "Secret": "your-super-secret-key-min-32-characters-long-for-security",
    "Issuer": "BarbApp",
    "Audience": "BarbApp-Users",
    "ExpirationMinutes": 480
  }
}
```

### Configuração JWT Development (appsettings.Development.json)

```json
{
  "JwtSettings": {
    "Secret": "development-secret-key-at-least-32-chars",
    "Issuer": "BarbApp-Dev",
    "Audience": "BarbApp-Dev-Users",
    "ExpirationMinutes": 60
  }
}
```

## Critérios de Sucesso
- ✅ JwtTokenGenerator gera tokens válidos com claims corretos
- ✅ JwtTokenGenerator valida tokens corretamente
- ✅ PasswordHasher gera hashes BCrypt seguros (work factor 12)
- ✅ PasswordHasher verifica senhas corretamente
- ✅ TenantContext é thread-safe usando AsyncLocal
- ✅ TenantContext armazena e recupera contexto corretamente
- ✅ Configurações JWT carregadas do appsettings.json
- ✅ Testes unitários cobrem todos os cenários
- ✅ Tokens expiram conforme configurado
- ✅ Tratamento de erros apropriado em validação

## Tempo Estimado
**4 horas**

## Referências
- TechSpec: Seção "4.4 Fase 1.4: JWT, Middlewares e Context"
- PRD: Seção "Requisitos de Segurança"
- JWT Best Practices
- BCrypt Documentation
