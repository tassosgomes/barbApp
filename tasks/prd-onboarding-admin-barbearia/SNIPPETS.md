# Exemplos de Código - Task 15

Este documento contém snippets de código prontos para copiar e colar durante a implementação.

---

## 📦 Backend - NuGet Packages

```xml
<!-- Adicionar ao BarbApp.Infrastructure.csproj -->
<ItemGroup>
  <PackageReference Include="MailKit" Version="4.3.0" />
  <PackageReference Include="MimeKit" Version="4.3.0" />
</ItemGroup>
```

---

## ⚙️ Configuração - appsettings.json

### Development

```json
{
  "AppSettings": {
    "FrontendUrl": "https://dev-barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 2525,
    "UseSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "dev@barbapp.tasso.dev.br",
    "FromName": "BarbApp Dev"
  }
}
```

### Production

```json
{
  "AppSettings": {
    "FrontendUrl": "https://barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}",
    "FromEmail": "noreply@barbapp.tasso.dev.br",
    "FromName": "BarbApp"
  }
}
```

---

## 🔐 IPasswordGenerator.cs

```csharp
namespace BarbApp.Application.Interfaces;

public interface IPasswordGenerator
{
    string Generate(int length = 12);
}
```

---

## 🔐 SecurePasswordGenerator.cs

```csharp
using System.Security.Cryptography;
using BarbApp.Application.Interfaces;

namespace BarbApp.Infrastructure.Services;

public class SecurePasswordGenerator : IPasswordGenerator
{
    private const string UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string LowerCase = "abcdefghijklmnopqrstuvwxyz";
    private const string Digits = "0123456789";
    private const string SpecialChars = "!@#$%&*-_+=";

    public string Generate(int length = 12)
    {
        if (length < 8)
            throw new ArgumentException("Password length must be at least 8 characters.", nameof(length));

        var allChars = UpperCase + LowerCase + Digits + SpecialChars;
        var password = new char[length];

        using var rng = RandomNumberGenerator.Create();

        // Garantir pelo menos 1 de cada tipo
        password[0] = GetRandomChar(UpperCase, rng);
        password[1] = GetRandomChar(LowerCase, rng);
        password[2] = GetRandomChar(Digits, rng);
        password[3] = GetRandomChar(SpecialChars, rng);

        // Preencher restante aleatoriamente
        for (int i = 4; i < length; i++)
        {
            password[i] = GetRandomChar(allChars, rng);
        }

        // Embaralhar para não ter padrão previsível
        Shuffle(password, rng);

        return new string(password);
    }

    private static char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var data = new byte[1];
        rng.GetBytes(data);
        return chars[data[0] % chars.Length];
    }

    private static void Shuffle(char[] array, RandomNumberGenerator rng)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            var data = new byte[1];
            rng.GetBytes(data);
            int j = data[0] % (i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }
}
```

---

## 📧 IEmailService.cs

```csharp
namespace BarbApp.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public class EmailMessage
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string TextBody { get; set; } = string.Empty;
}
```

---

## 📧 SmtpSettings.cs

```csharp
namespace BarbApp.Infrastructure.Configuration;

public class SmtpSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    
    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}
```

---

## ⚙️ AppSettings.cs

```csharp
namespace BarbApp.Infrastructure.Configuration;

public class AppSettings
{
    public string FrontendUrl { get; set; } = string.Empty;
}
```

---

## 📧 SmtpEmailService.cs

```csharp
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using BarbApp.Application.Interfaces;
using BarbApp.Infrastructure.Configuration;

namespace BarbApp.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        const int maxRetries = 3;
        var retryDelays = new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) };

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_settings.Host, _settings.Port, _settings.UseSsl, cancellationToken);

                // Autenticar apenas se credenciais forem fornecidas
                if (_settings.RequiresAuthentication)
                {
                    await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
                    _logger.LogDebug("SMTP authentication successful");
                }
                else
                {
                    _logger.LogDebug("SMTP connection without authentication (dev mode)");
                }

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
                mimeMessage.To.Add(MailboxAddress.Parse(message.To));
                mimeMessage.Subject = message.Subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = message.HtmlBody,
                    TextBody = message.TextBody
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();

                await client.SendAsync(mimeMessage, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                _logger.LogInformation("Email sent successfully to {Recipient}", message.To);
                return; // Sucesso, sair do loop
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}/{MaxRetries}", 
                    message.To, attempt + 1, maxRetries);

                if (attempt == maxRetries - 1)
                {
                    _logger.LogError(ex, "Failed to send email to {Recipient} after {MaxRetries} attempts", 
                        message.To, maxRetries);
                    throw new InvalidOperationException($"Failed to send email after {maxRetries} attempts.", ex);
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }
        }
    }
}
```

---

## 🗂️ IAdminBarbeariaRepository.cs (adicionar método)

```csharp
// Adicionar ao arquivo existente
Task<AdminBarbeariaUser?> GetByEmailAsync(string email, CancellationToken cancellationToken);
```

---

## 🗂️ AdminBarbeariaRepository.cs (implementar método)

```csharp
// Adicionar ao arquivo existente
public async Task<AdminBarbeariaUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)
{
    return await _context.AdminBarbeariaUsers
        .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower(), cancellationToken);
}
```

---

## 📝 Registrar Serviços no DI (Program.cs)

```csharp
// Adicionar em Program.cs após outros serviços

// Configurações
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Serviços
builder.Services.AddScoped<IPasswordGenerator, SecurePasswordGenerator>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ResendCredentialsUseCase>();
```

---

## 🎯 Endpoint de Reenvio (BarbershopsController.cs)

```csharp
// Adicionar ao controller existente

[HttpPost("{id}/resend-credentials")]
[Authorize(Roles = "AdminCentral")]
public async Task<IActionResult> ResendCredentials(Guid id, CancellationToken cancellationToken)
{
    try
    {
        await _resendCredentialsUseCase.ExecuteAsync(id, cancellationToken);
        return Ok(new { message = "Credenciais reenviadas com sucesso." });
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { message = ex.Message });
    }
    catch (InvalidOperationException ex)
    {
        return BadRequest(new { message = ex.Message });
    }
}
```

---

## 🌐 Frontend - barbershop.service.ts

```typescript
// Adicionar ao arquivo existente

async resendCredentials(id: string): Promise<void> {
  await api.post(`/barbershops/${id}/resend-credentials`);
}
```

---

## 🔄 Frontend - ResendCredentialsModal.tsx

```tsx
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

interface ResendCredentialsModalProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  barbershopName?: string;
  barbershopEmail?: string;
  isLoading: boolean;
}

export function ResendCredentialsModal({
  open,
  onClose,
  onConfirm,
  barbershopName,
  barbershopEmail,
  isLoading,
}: ResendCredentialsModalProps) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Reenviar Credenciais de Acesso</DialogTitle>
          <DialogDescription>
            Uma nova senha será gerada e enviada para o e-mail cadastrado.
          </DialogDescription>
        </DialogHeader>

        <div className="space-y-2">
          <p className="text-sm">
            <strong>Barbearia:</strong> {barbershopName}
          </p>
          <p className="text-sm">
            <strong>E-mail:</strong> {barbershopEmail}
          </p>
          <p className="text-sm text-amber-600">
            ⚠️ A senha atual será substituída. O Admin da Barbearia precisará usar a nova senha recebida por e-mail.
          </p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isLoading}>
            Cancelar
          </Button>
          <Button onClick={onConfirm} disabled={isLoading}>
            {isLoading ? 'Enviando...' : 'Confirmar Reenvio'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
```

---

## 🐳 Docker Compose - smtp4dev

```yaml
# docker-compose.yml (adicionar ao existente)
version: '3.8'

services:
  smtp4dev:
    image: rnwood/smtp4dev:latest
    container_name: barbapp-smtp4dev
    ports:
      - "3000:80"    # UI web
      - "2525:25"    # Porta SMTP
    networks:
      - barbapp-network

networks:
  barbapp-network:
    driver: bridge
```

**Ou comando standalone**:
```bash
docker run -d \
  --name barbapp-smtp4dev \
  -p 3000:80 \
  -p 2525:25 \
  rnwood/smtp4dev
```

---

## 🧪 Teste Manual - Cadastrar Barbearia (cURL)

```bash
curl -X POST http://localhost:5000/api/barbershops \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_ADMIN_CENTRAL_TOKEN" \
  -d '{
    "name": "Barbearia Teste",
    "document": "12345678901234",
    "ownerName": "João Silva",
    "email": "joao@barbearia.com",
    "phone": "11987654321",
    "zipCode": "01310-100",
    "street": "Av Paulista",
    "number": "1000",
    "complement": "Sala 10",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  }'
```

---

## 🧪 Teste Manual - Reenviar Credenciais (cURL)

```bash
curl -X POST http://localhost:5000/api/barbershops/{BARBERSHOP_ID}/resend-credentials \
  -H "Authorization: Bearer YOUR_ADMIN_CENTRAL_TOKEN"
```

---

## 📧 Template de E-mail - HTML Completo

```html
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Bem-vindo ao BarbApp</title>
  <style>
    body {
      font-family: Arial, sans-serif;
      line-height: 1.6;
      color: #333;
      margin: 0;
      padding: 0;
      background-color: #f4f4f4;
    }
    .container {
      max-width: 600px;
      margin: 20px auto;
      background-color: #ffffff;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    }
    .header {
      background-color: #4A5568;
      color: white;
      padding: 30px 20px;
      text-align: center;
    }
    .header h1 {
      margin: 0;
      font-size: 24px;
    }
    .content {
      padding: 30px 20px;
    }
    .credentials {
      background-color: #E2E8F0;
      padding: 20px;
      border-radius: 5px;
      margin: 20px 0;
      border-left: 4px solid #4299E1;
    }
    .credentials p {
      margin: 8px 0;
      font-size: 14px;
    }
    .credentials strong {
      color: #2D3748;
    }
    .button-container {
      text-align: center;
      margin: 30px 0;
    }
    .button {
      display: inline-block;
      padding: 14px 32px;
      background-color: #4299E1;
      color: white;
      text-decoration: none;
      border-radius: 5px;
      font-weight: bold;
      transition: background-color 0.3s;
    }
    .button:hover {
      background-color: #3182CE;
    }
    .security-note {
      font-size: 13px;
      color: #718096;
      margin-top: 20px;
      padding: 15px;
      background-color: #FFF5F5;
      border-left: 4px solid #F56565;
      border-radius: 4px;
    }
    .footer {
      background-color: #F7FAFC;
      text-align: center;
      padding: 20px;
      font-size: 12px;
      color: #718096;
      border-top: 1px solid #E2E8F0;
    }
    .footer p {
      margin: 5px 0;
    }
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>✂️ Bem-vindo ao BarbApp!</h1>
    </div>
    
    <div class="content">
      <p>Olá!</p>
      <p>A barbearia <strong>{{BARBERSHOP_NAME}}</strong> foi cadastrada com sucesso no BarbApp!</p>
      <p>Você pode acessar o sistema de gestão com as credenciais abaixo:</p>
      
      <div class="credentials">
        <p><strong>E-mail:</strong> {{EMAIL}}</p>
        <p><strong>Senha:</strong> <code style="background: #CBD5E0; padding: 4px 8px; border-radius: 3px; font-family: monospace;">{{PASSWORD}}</code></p>
      </div>
      
      <div class="button-container">
        <a href="{{LOGIN_URL}}" class="button">Acessar o Sistema</a>
      </div>
      
      <div class="security-note">
        🔒 <strong>Segurança:</strong> Por questões de segurança, recomendamos alterar sua senha após o primeiro acesso.
      </div>
    </div>
    
    <div class="footer">
      <p>Esta é uma mensagem automática. Por favor, não responda este e-mail.</p>
      <p>&copy; 2025 BarbApp. Todos os direitos reservados.</p>
    </div>
  </div>
</body>
</html>
```

---

## 📝 Exemplo de Uso no CreateBarbershopUseCase

```csharp
private string BuildWelcomeEmailHtml(string barbershopName, string email, string password)
{
    var loginUrl = $"{_appSettings.FrontendUrl}/login";
    var template = File.ReadAllText("Templates/WelcomeEmail.html"); // ou inline como no exemplo acima
    
    return template
        .Replace("{{BARBERSHOP_NAME}}", barbershopName)
        .Replace("{{EMAIL}}", email)
        .Replace("{{PASSWORD}}", password)
        .Replace("{{LOGIN_URL}}", loginUrl);
}

private string BuildWelcomeEmailText(string barbershopName, string email, string password)
{
    var loginUrl = $"{_appSettings.FrontendUrl}/login";
    return $@"
Bem-vindo ao BarbApp!

A barbearia {barbershopName} foi cadastrada com sucesso no BarbApp!

Você pode acessar o sistema de gestão com as credenciais abaixo:

E-mail: {email}
Senha: {password}

Acesse: {loginUrl}

Por questões de segurança, recomendamos alterar sua senha após o primeiro acesso.

Equipe BarbApp
    ";
}
```

---

## 🎯 Checklist de Cópia

```
Backend:
[ ] Copiar IPasswordGenerator.cs
[ ] Copiar SecurePasswordGenerator.cs
[ ] Copiar IEmailService.cs + EmailMessage.cs
[ ] Copiar SmtpSettings.cs
[ ] Copiar AppSettings.cs
[ ] Copiar SmtpEmailService.cs
[ ] Atualizar IAdminBarbeariaRepository.cs
[ ] Atualizar AdminBarbeariaRepository.cs
[ ] Adicionar DI em Program.cs
[ ] Adicionar endpoint em BarbershopsController.cs
[ ] Configurar appsettings.Development.json
[ ] Configurar appsettings.json

Frontend:
[ ] Copiar ResendCredentialsModal.tsx
[ ] Atualizar barbershop.service.ts

Infra:
[ ] Adicionar smtp4dev ao docker-compose.yml (ou rodar standalone)
```

---

Data: 2025-10-16  
Task: #15 - Onboarding Automático do Admin da Barbearia
