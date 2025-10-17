# Especificação Técnica — Onboarding Automático do Admin da Barbearia

## Resumo Executivo

Esta Tech Spec detalha a implementação do fluxo de onboarding automático de Admin Barbearia: criação de usuário, geração de senha aleatória segura e envio de credenciais por e-mail no momento do cadastro da barbearia pelo Admin Central. Inclui também funcionalidade de reenvio de credenciais. A solução garante atomicidade transacional, segurança na geração de senhas e observabilidade adequada.

Decisões principais:
- Usar transação de banco de dados para garantir atomicidade (cadastro barbearia + criação usuário + envio e-mail).
- Implementar serviço de e-mail SMTP com retry (3 tentativas, backoff exponencial).
- Gerar senha aleatória com 12 caracteres (maiúsculas, minúsculas, números, símbolos especiais).
- Adicionar endpoint de reenvio de credenciais para suporte pelo Admin Central.
- Adicionar botão de reenvio na UI (Admin Central) com confirmação modal.
- Logging estruturado de eventos críticos (criação usuário, envio e-mail, reenvio).

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Backend (.NET)**
- **Domain Layer**
  - Adicionar evento de domínio: `AdminBarbeariaCreatedEvent` (opcional, para extensibilidade futura).
  - Adicionar validador de senha (geração segura).
- **Application Layer**
  - `CreateBarbershopUseCase`: estender para criar Admin Barbearia e enviar e-mail.
  - `ResendCredentialsUseCase`: novo use case para reenvio de credenciais.
  - `IEmailService`: nova interface para envio de e-mails.
  - `IPasswordGenerator`: nova interface para geração de senhas aleatórias.
- **Infrastructure Layer**
  - `SmtpEmailService`: implementação de `IEmailService` usando `MailKit` ou `System.Net.Mail`.
  - `SecurePasswordGenerator`: implementação de `IPasswordGenerator`.
  - Configurações SMTP em `appsettings.json`.
  - Repository: `IAdminBarbeariaRepository` (pode já existir; verificar).
- **API Layer**
  - Estender `BarbershopsController.Post` para retornar feedback sobre envio de e-mail.
  - Adicionar `BarbershopsController.ResendCredentials` (POST `/api/barbershops/{id}/resend-credentials`).

**Frontend (React + TypeScript)**
- **Services**
  - Estender `barbershop.service.ts` para incluir método `resendCredentials(id: string)`.
- **Pages**
  - Atualizar `BarbershopCreate.tsx`: ajustar mensagem de sucesso.
  - Atualizar `BarbershopList.tsx`: adicionar botão "Reenviar Credenciais" na tabela.
- **Components**
  - Criar `ResendCredentialsModal.tsx`: modal de confirmação para reenvio.

### Fluxo de Dados

**Cadastro de Barbearia (com criação de Admin Barbearia):**
1. Admin Central submete formulário de cadastro (POST `/api/barbershops`).
2. Backend valida dados da barbearia.
3. Backend inicia transação:
   a. Cria registro em `barbershops`.
   b. Gera senha aleatória (12 caracteres).
   c. Cria registro em `admin_barbearia_users` com email, password_hash, barbearia_id.
   d. Envia e-mail com credenciais (com retry).
   e. Commit da transação (ou rollback em caso de falha).
4. Backend retorna 201 com dados da barbearia.
5. Frontend exibe toast de sucesso com informação sobre envio de e-mail.

**Reenvio de Credenciais:**
1. Admin Central clica em "Reenviar Credenciais" na listagem.
2. Frontend exibe modal de confirmação.
3. Admin Central confirma.
4. Frontend chama POST `/api/barbershops/{id}/resend-credentials`.
5. Backend valida existência da barbearia e Admin Barbearia vinculado.
6. Backend gera nova senha aleatória.
7. Backend atualiza `password_hash` do usuário.
8. Backend envia e-mail com novas credenciais (com retry).
9. Backend loga evento de reenvio (auditoria).
10. Backend retorna 200.
11. Frontend exibe toast de sucesso.

## Design de Implementação

### Interfaces Principais (Backend)

```csharp
// Application/Interfaces/IEmailService.cs
public interface IEmailService
{
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}

public class EmailMessage
{
    public string To { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
    public string TextBody { get; set; }
}

// Application/Interfaces/IPasswordGenerator.cs
public interface IPasswordGenerator
{
    string Generate(int length = 12);
}

// Application/UseCases/CreateBarbershopUseCase.cs (atualizado)
public class CreateBarbershopUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAdminBarbeariaRepository _adminBarbeariaRepository;
    private readonly IAddressRepository _addressRepository;
    private readonly IUniqueCodeGenerator _codeGenerator;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateBarbershopUseCase> _logger;
    private readonly AppSettings _appSettings;

    public CreateBarbershopUseCase(
        IBarbershopRepository barbershopRepository,
        IAdminBarbeariaRepository adminBarbeariaRepository,
        IAddressRepository addressRepository,
        IUniqueCodeGenerator codeGenerator,
        IPasswordGenerator passwordGenerator,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        ILogger<CreateBarbershopUseCase> logger,
        IOptions<AppSettings> appSettings)
    {
        _barbershopRepository = barbershopRepository;
        _adminBarbeariaRepository = adminBarbeariaRepository;
        _addressRepository = addressRepository;
        _codeGenerator = codeGenerator;
        _passwordGenerator = passwordGenerator;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _appSettings = appSettings.Value;
    }

    public async Task<BarbershopResponse> ExecuteAsync(CreateBarbershopRequest request, CancellationToken cancellationToken)
    {
        // Validações existentes...

        // Validar unicidade de e-mail (validação na aplicação)
        var existingAdminUser = await _adminBarbeariaRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingAdminUser != null)
        {
            throw new ConflictException("Já existe um usuário cadastrado com este e-mail.");
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Criar barbearia (código existente)
            var barbershop = /* ... */;
            await _barbershopRepository.AddAsync(barbershop, cancellationToken);

            // 2. Gerar senha aleatória
            var password = _passwordGenerator.Generate();

            // 3. Criar Admin Barbearia
            var adminUser = new AdminBarbeariaUser
            {
                Id = Guid.NewGuid(),
                BarbeariaId = barbershop.Id,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Name = request.OwnerName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _adminBarbeariaRepository.AddAsync(adminUser, cancellationToken);

            // 4. Enviar e-mail com credenciais
            await SendWelcomeEmailAsync(barbershop.Name, request.Email, password, cancellationToken);

            // 5. Commit transação
            await _unitOfWork.CommitAsync(cancellationToken);

            _logger.LogInformation("Barbershop {BarbershopId} created with Admin user {AdminUserId}", barbershop.Id, adminUser.Id);

            return /* response */;
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to create barbershop with Admin user");
            throw;
        }
    }

    private async Task SendWelcomeEmailAsync(string barbershopName, string email, string password, CancellationToken cancellationToken)
    {
        var emailMessage = new EmailMessage
        {
            To = email,
            Subject = "Bem-vindo ao BarbApp",
            HtmlBody = BuildWelcomeEmailHtml(barbershopName, email, password),
            TextBody = BuildWelcomeEmailText(barbershopName, email, password)
        };

        await _emailService.SendAsync(emailMessage, cancellationToken);
    }

    private string BuildWelcomeEmailHtml(string barbershopName, string email, string password)
    {
        var loginUrl = $"{_appSettings.FrontendUrl}/login";
        return $@"
            <html>
            <body>
                <h2>Bem-vindo ao BarbApp!</h2>
                <p>A barbearia <strong>{barbershopName}</strong> foi cadastrada com sucesso no BarbApp!</p>
                <p>Você pode acessar o sistema de gestão com as credenciais abaixo:</p>
                <p><strong>E-mail:</strong> {email}<br/>
                <strong>Senha:</strong> {password}</p>
                <p><a href='{loginUrl}'>Acessar o Sistema</a></p>
                <p>Por questões de segurança, recomendamos alterar sua senha após o primeiro acesso.</p>
                <p>Equipe BarbApp</p>
            </body>
            </html>
        ";
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
}

// Application/UseCases/ResendCredentialsUseCase.cs (novo)
public class ResendCredentialsUseCase
{
    private readonly IBarbershopRepository _barbershopRepository;
    private readonly IAdminBarbeariaRepository _adminBarbeariaRepository;
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly IEmailService _emailService;
    private readonly ILogger<ResendCredentialsUseCase> _logger;

    // ... constructor

    public async Task ExecuteAsync(Guid barbershopId, CancellationToken cancellationToken)
    {
        // 1. Buscar barbearia
        var barbershop = await _barbershopRepository.GetByIdAsync(barbershopId, cancellationToken);
        if (barbershop == null)
            throw new NotFoundException("Barbearia não encontrada.");

        // 2. Buscar Admin Barbearia vinculado
        var adminUser = await _adminBarbeariaRepository.GetByBarbeariaIdAsync(barbershopId, cancellationToken);
        if (adminUser == null)
            throw new InvalidOperationException("Admin da Barbearia não encontrado.");

        // 3. Gerar nova senha
        var newPassword = _passwordGenerator.Generate();

        // 4. Atualizar hash de senha
        adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        adminUser.UpdatedAt = DateTime.UtcNow;
        await _adminBarbeariaRepository.UpdateAsync(adminUser, cancellationToken);

        // 5. Enviar e-mail com novas credenciais
        await SendCredentialsEmailAsync(barbershop.Name, adminUser.Email, newPassword, cancellationToken);

        _logger.LogInformation("Credentials resent for barbershop {BarbershopId}, Admin user {AdminUserId}", barbershopId, adminUser.Id);
    }

    private async Task SendCredentialsEmailAsync(string barbershopName, string email, string password, CancellationToken cancellationToken)
    {
        var emailMessage = new EmailMessage
        {
            To = email,
            Subject = "Suas novas credenciais - BarbApp",
            HtmlBody = BuildCredentialsEmailHtml(barbershopName, email, password),
            TextBody = BuildCredentialsEmailText(barbershopName, email, password)
        };

        await _emailService.SendAsync(emailMessage, cancellationToken);
    }

    // Templates similares ao CreateBarbershopUseCase (ajustar texto)
}
```

### Implementação do Serviço de E-mail (SMTP)

```csharp
// Infrastructure/Services/SmtpEmailService.cs
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
                _logger.LogWarning(ex, "Failed to send email to {Recipient} on attempt {Attempt}/{MaxRetries}", message.To, attempt + 1, maxRetries);

                if (attempt == maxRetries - 1)
                {
                    _logger.LogError(ex, "Failed to send email to {Recipient} after {MaxRetries} attempts", message.To, maxRetries);
                    throw new InvalidOperationException($"Failed to send email after {maxRetries} attempts.", ex);
                }

                await Task.Delay(retryDelays[attempt], cancellationToken);
            }
        }
    }
}

// Infrastructure/Configuration/SmtpSettings.cs
public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FromEmail { get; set; }
    public string FromName { get; set; }
    public bool RequiresAuthentication => !string.IsNullOrWhiteSpace(Username);
}

// Infrastructure/Configuration/AppSettings.cs
public class AppSettings
{
    public string FrontendUrl { get; set; }
}

// appsettings.json (Produção)
{
  "AppSettings": {
    "FrontendUrl": "https://barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-username",
    "Password": "your-password",
    "FromEmail": "noreply@barbapp.tasso.dev.br",
    "FromName": "BarbApp"
  }
}

// appsettings.Development.json (Desenvolvimento)
{
  "AppSettings": {
    "FrontendUrl": "https://dev-barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 3000,
    "UseSsl": false,
    "Username": "",
    "Password": "",
    "FromEmail": "dev@barbapp.tasso.dev.br",
    "FromName": "BarbApp Dev"
  }
}
```

### Gerador de Senha Seguro

```csharp
// Infrastructure/Services/SecurePasswordGenerator.cs
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

    private char GetRandomChar(string chars, RandomNumberGenerator rng)
    {
        var data = new byte[1];
        rng.GetBytes(data);
        return chars[data[0] % chars.Length];
    }

    private void Shuffle(char[] array, RandomNumberGenerator rng)
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

### API Endpoints (Backend)

```csharp
// API/Controllers/BarbershopsController.cs (atualizado)

// Endpoint existente (atualizado)
[HttpPost]
[Authorize(Roles = "AdminCentral")]
public async Task<IActionResult> Create([FromBody] CreateBarbershopRequest request, CancellationToken cancellationToken)
{
    var result = await _createBarbershopUseCase.ExecuteAsync(request, cancellationToken);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}

// Novo endpoint
[HttpPost("{id}/resend-credentials")]
[Authorize(Roles = "AdminCentral")]
public async Task<IActionResult> ResendCredentials(Guid id, CancellationToken cancellationToken)
{
    await _resendCredentialsUseCase.ExecuteAsync(id, cancellationToken);
    return Ok(new { message = "Credenciais reenviadas com sucesso." });
}
```

### Frontend - Service (TypeScript)

```ts
// src/services/barbershop.service.ts (atualizado)

export interface BarbershopService {
  // ... métodos existentes
  resendCredentials(id: string): Promise<void>;
}

class BarbershopServiceImpl implements BarbershopService {
  // ... métodos existentes

  async resendCredentials(id: string): Promise<void> {
    await api.post(`/barbershops/${id}/resend-credentials`);
  }
}

export const barbershopService = new BarbershopServiceImpl();
```

### Frontend - Modal de Confirmação

```tsx
// src/components/barbershop/ResendCredentialsModal.tsx

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

### Frontend - Atualização da Listagem

```tsx
// src/pages/Barbershops/List.tsx (trecho atualizado)

import { ResendCredentialsModal } from '@/components/barbershop/ResendCredentialsModal';

export function BarbershopList() {
  // ... código existente

  // Estado para modal de reenvio
  const [resendModalOpen, setResendModalOpen] = useState(false);
  const [selectedBarbershopForResend, setSelectedBarbershopForResend] = useState<{
    id: string;
    name: string;
    email: string;
  } | null>(null);
  const [isResending, setIsResending] = useState(false);

  const handleResendCredentials = (id: string) => {
    const barbershop = data?.items.find(b => b.id === id);
    if (barbershop) {
      setSelectedBarbershopForResend({
        id: barbershop.id,
        name: barbershop.name,
        email: barbershop.email,
      });
      setResendModalOpen(true);
    }
  };

  const confirmResendCredentials = async () => {
    if (!selectedBarbershopForResend) return;

    setIsResending(true);
    try {
      await barbershopService.resendCredentials(selectedBarbershopForResend.id);
      toast({
        title: 'Credenciais reenviadas com sucesso!',
        description: `Novas credenciais enviadas para ${selectedBarbershopForResend.email}`,
      });
      setResendModalOpen(false);
    } catch (error) {
      toast({
        title: 'Erro ao reenviar credenciais',
        description: handleApiError(error),
        variant: 'destructive',
      });
    } finally {
      setIsResending(false);
    }
  };

  return (
    <div className="space-y-6">
      {/* ... código existente da listagem */}

      {/* Passar handleResendCredentials para BarbershopTable */}
      <BarbershopTable
        barbershops={data.items}
        onView={(id) => navigate(`/barbearias/${id}`)}
        onEdit={(id) => navigate(`/barbearias/${id}/editar`)}
        onDeactivate={handleDeactivate}
        onReactivate={handleReactivate}
        onResendCredentials={handleResendCredentials} // NOVO
        onCopyCode={handleCopyCode}
      />

      {/* Modal de Reenvio */}
      <ResendCredentialsModal
        open={resendModalOpen}
        onClose={() => setResendModalOpen(false)}
        onConfirm={confirmResendCredentials}
        barbershopName={selectedBarbershopForResend?.name}
        barbershopEmail={selectedBarbershopForResend?.email}
        isLoading={isResending}
      />

      {/* ... modais existentes */}
    </div>
  );
}
```

### Frontend - Atualização da Tabela

```tsx
// src/components/barbershop/BarbershopTable.tsx (adicionar nova prop e botão)

interface BarbershopTableProps {
  // ... props existentes
  onResendCredentials: (id: string) => void; // NOVO
}

export function BarbershopTable({
  barbershops,
  onView,
  onEdit,
  onDeactivate,
  onReactivate,
  onResendCredentials, // NOVO
  onCopyCode,
}: BarbershopTableProps) {
  return (
    <Table>
      {/* ... cabeçalho e linhas existentes */}
      
      {/* Adicionar botão na coluna de ações */}
      <TableCell>
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="sm">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => onView(barbershop.id)}>
              Visualizar
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onEdit(barbershop.id)}>
              Editar
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onResendCredentials(barbershop.id)}>
              Reenviar Credenciais
            </DropdownMenuItem>
            <DropdownMenuItem onClick={() => onCopyCode(barbershop.code)}>
              Copiar Código
            </DropdownMenuItem>
            {barbershop.isActive ? (
              <DropdownMenuItem
                onClick={() => onDeactivate(barbershop.id)}
                className="text-destructive"
              >
                Desativar
              </DropdownMenuItem>
            ) : (
              <DropdownMenuItem onClick={() => onReactivate(barbershop.id)}>
                Reativar
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </TableCell>
    </Table>
  );
}
```

### Frontend - Atualização da Mensagem de Sucesso (Cadastro)

```tsx
// src/pages/Barbershops/Create.tsx (trecho atualizado)

const onSubmit = async (data: BarbershopFormData) => {
  setIsSubmitting(true);
  try {
    // ... código de criação existente
    const result = await barbershopService.create(requestData);
    setCreatedBarbershop(result);
    toast({
      title: 'Barbearia cadastrada com sucesso!',
      description: `Código gerado: ${result.code || 'N/A'}. Credenciais de acesso enviadas para ${requestData.email}`, // ATUALIZADO
    });
  } catch (error) {
    // ... tratamento de erro existente
  } finally {
    setIsSubmitting(false);
  }
};
```

## Pontos de Integração

- **Banco de Dados**: PostgreSQL com Entity Framework Core
  - Tabelas afetadas: `barbershops`, `admin_barbearia_users`
  - Relacionamento: `admin_barbearia_users.barbearia_id` → `barbershops.barbershop_id`
  - Validação de unicidade: `admin_barbearia_users.email` (índice único global)
- **SMTP**: Servidor de e-mail (localhost:587 para dev/MVP)
  - Configuração via `appsettings.json`
  - Biblioteca: MailKit (recomendada) ou System.Net.Mail
- **Autenticação**: JWT com role `AdminCentral` para reenvio de credenciais
- **Logging**: ILogger (padrão .NET) para eventos críticos

## Análise de Impacto

| Componente Afetado                      | Tipo de Impacto              | Descrição & Nível de Risco                                    | Ação Requerida                     |
| --------------------------------------- | ---------------------------- | ------------------------------------------------------------- | ---------------------------------- |
| `CreateBarbershopUseCase` (backend)     | Modificação significativa    | Adiciona criação de Admin Barbearia e envio de e-mail. Médio.| Atualizar com transação e e-mail   |
| `BarbershopsController` (backend)       | Adição de endpoint           | Novo endpoint de reenvio. Baixo.                              | Implementar                        |
| `IEmailService` (backend)               | Nova interface/serviço       | Novo serviço de infraestrutura. Médio (config SMTP).         | Implementar                        |
| `IPasswordGenerator` (backend)          | Nova interface/serviço       | Utilitário simples. Baixo.                                    | Implementar                        |
| `admin_barbearia_users` (banco)         | Índice único                 | Garantir índice único em email (global). Médio (migração).   | Verificar migração                 |
| `BarbershopList` (frontend)             | Adição de funcionalidade     | Botão de reenvio. Baixo.                                      | Implementar                        |
| `BarbershopCreate` (frontend)           | Ajuste de mensagem           | Atualizar toast. Baixo.                                       | Atualizar                          |
| `barbershop.service.ts` (frontend)      | Novo método                  | Adicionar resendCredentials. Baixo.                           | Implementar                        |
| Testes (unit/integration/e2e)           | Ampliação da suíte           | Cobrir criação de usuário, envio de e-mail, reenvio. Médio.  | Escrever testes                    |

## Abordagem de Testes

### Testes Unitários (Backend)

- **SecurePasswordGenerator**
  - Gera senha com 12 caracteres
  - Contém pelo menos 1 maiúscula, 1 minúscula, 1 número, 1 símbolo
  - Lança exceção se length < 8
- **CreateBarbershopUseCase**
  - Cria barbearia, Admin Barbearia e envia e-mail (mock de IEmailService)
  - Rollback em caso de falha no envio de e-mail
  - Loga eventos corretamente
- **ResendCredentialsUseCase**
  - Gera nova senha e envia e-mail
  - Retorna 404 se barbearia não existir
  - Retorna 400 se Admin Barbearia não existir
  - Loga evento de reenvio
- **SmtpEmailService**
  - Envia e-mail com sucesso (mock SMTP)
  - Retry em caso de falha temporária (3 tentativas)
  - Lança exceção após 3 tentativas falhadas

### Testes de Integração (Backend)

- **Cadastro de Barbearia com Criação de Admin**
  - POST `/api/barbershops` cria barbearia e Admin Barbearia
  - Verifica registro em `admin_barbearia_users`
  - Verifica envio de e-mail (mock ou MailHog)
  - Verifica rollback em caso de falha
- **Reenvio de Credenciais**
  - POST `/api/barbershops/{id}/resend-credentials` atualiza senha e envia e-mail
  - Verifica 404 para barbearia inexistente
  - Verifica 401 se não autenticado como Admin Central

### Testes de Integração (Frontend)

- **BarbershopCreate**
  - Submeter formulário e verificar toast com mensagem de envio de e-mail
  - MSW intercepta POST `/api/barbershops` e retorna sucesso
- **BarbershopList - Reenvio de Credenciais**
  - Clicar em "Reenviar Credenciais" abre modal
  - Confirmar reenvio chama endpoint e exibe toast de sucesso
  - MSW intercepta POST `/api/barbershops/{id}/resend-credentials`

### Testes E2E (Playwright)

- **Fluxo de Cadastro de Barbearia**
  - Admin Central acessa tela de Nova Barbearia
  - Preenche formulário com dados válidos
  - Clica em "Salvar"
  - Verifica toast de sucesso com mensagem sobre credenciais
  - Verifica e-mail enviado (MailHog ou mock)
- **Fluxo de Reenvio de Credenciais**
  - Admin Central acessa listagem de barbearias
  - Clica em "Reenviar Credenciais" em uma barbearia
  - Confirma modal
  - Verifica toast de sucesso
  - Verifica e-mail reenviado (MailHog ou mock)

## Sequenciamento de Desenvolvimento

### Ordem de Construção (Backend)

1. **Infraestrutura de E-mail**
   - Criar `IEmailService` e `SmtpEmailService`
   - Configurar `SmtpSettings` em `appsettings.json`
   - Adicionar MailKit ao projeto (NuGet)
2. **Gerador de Senha**
   - Criar `IPasswordGenerator` e `SecurePasswordGenerator`
3. **Use Case de Criação de Barbearia**
   - Atualizar `CreateBarbershopUseCase` para criar Admin Barbearia e enviar e-mail
   - Adicionar transação (UnitOfWork)
4. **Use Case de Reenvio de Credenciais**
   - Criar `ResendCredentialsUseCase`
5. **API Endpoints**
   - Adicionar endpoint de reenvio em `BarbershopsController`
6. **Testes Unitários e Integração (Backend)**
   - Testar gerador de senha, serviço de e-mail, use cases

### Ordem de Construção (Frontend)

1. **Serviço de API**
   - Adicionar método `resendCredentials` em `barbershop.service.ts`
2. **Modal de Confirmação**
   - Criar `ResendCredentialsModal.tsx`
3. **Atualização da Listagem**
   - Atualizar `BarbershopList.tsx` para incluir botão e modal de reenvio
   - Atualizar `BarbershopTable.tsx` para adicionar ação de reenvio
4. **Atualização do Cadastro**
   - Atualizar mensagem de sucesso em `BarbershopCreate.tsx`
5. **Testes Unitários e Integração (Frontend)**
   - Testar componentes com MSW
6. **Testes E2E**
   - Fluxos críticos com Playwright

### Dependências Técnicas

**Backend:**
- NuGet: `MailKit` (ou usar `System.Net.Mail` nativo)
- Configuração SMTP em ambiente de dev/staging/prod

**Frontend:**
- Nenhuma nova dependência necessária (usa bibliotecas existentes)

## Monitoramento e Observabilidade

### Logs Estruturados

- **Criação de Admin Barbearia**
  - Evento: `AdminBarbeariaCreated`
  - Campos: `BarbershopId`, `AdminUserId`, `Email`, `Timestamp`
- **Envio de E-mail**
  - Evento: `EmailSent` (sucesso)
  - Campos: `To`, `Subject`, `Timestamp`
  - Evento: `EmailSendFailed` (falha)
  - Campos: `To`, `Subject`, `Error`, `Attempt`, `Timestamp`
- **Reenvio de Credenciais**
  - Evento: `CredentialsResent`
  - Campos: `BarbershopId`, `AdminUserId`, `Email`, `Timestamp`

### Métricas (Futuro)

- Taxa de sucesso de envio de e-mails
- Tempo médio de cadastro de barbearia (incluindo envio de e-mail)
- Quantidade de reenvios de credenciais (indicador de suporte)

### UX Feedback

- Toasts para sucesso/erro em cadastro e reenvio
- Loading states em botões de ação
- Modal de confirmação para ações críticas (reenvio)

## Considerações Técnicas

### Decisões Principais

- **Transação Atômica**: Usar UnitOfWork para garantir que cadastro de barbearia, criação de Admin Barbearia e envio de e-mail sejam atômicos. Rollback em caso de falha.
- **Retry de E-mail**: Implementar 3 tentativas com backoff exponencial (1s, 2s, 4s) para lidar com falhas temporárias de SMTP.
- **Geração de Senha Segura**: Usar `RandomNumberGenerator` (criptograficamente seguro) para garantir entropia adequada.
- **SMTP no MVP**: Usar servidor SMTP simples (localhost:587) para MVP. Migrar para provedor transacional (SendGrid, SES) em produção.
- **Validação de E-mail Único**: Garantir índice único global em `admin_barbearia_users.email` para evitar conflitos.

### Riscos Conhecidos

- **Falha de SMTP**: Se servidor SMTP estiver indisponível, cadastro de barbearia falhará. Mitigação: retry e logging adequado; considerar fila assíncrona no futuro.
- **E-mail Inválido**: Se e-mail cadastrado for inválido/inexistente, cadastro falhará. Mitigação: validação de formato no frontend; validação de existência fica para versão futura.
- **Conflito de E-mail**: Se e-mail já existir (Admin Barbearia de outra barbearia), cadastro falhará. Mitigação: mensagem clara de erro 409; Admin Central deve usar e-mail diferente.
- **LGPD**: Senha em plain text no e-mail. Mitigação: avisar usuário para trocar senha; implementar fluxo de troca de senha no futuro.

### Requisitos Especiais

- **Performance**: Envio de e-mail pode adicionar 1-3s ao tempo de resposta de cadastro. Considerar processamento assíncrono (fila) no futuro se necessário.
- **Segurança**:
  - Nunca logar senha em plain text
  - Usar HTTPS para comunicação com SMTP (TLS/SSL)
  - Armazenar apenas hash de senha no banco (BCrypt)
- **LGPD**:
  - E-mails devem ser enviados apenas para destinatário correto
  - Logs não devem conter dados sensíveis (PII)
  - Considerar consentimento explícito para envio de e-mails em versões futuras

### Conformidade com Padrões

- **Backend**: Seguir padrões de Clean Architecture (Domain, Application, Infrastructure, API)
- **Logging**: Usar `ILogger` com níveis adequados (Information, Warning, Error)
- **Testes**: Seguir `rules/tests.md` (AAA/GWT, cobertura, mocks)
- **Frontend**: Seguir `rules/react.md` (componentes funcionais, hooks, TypeScript)
- **Git**: Seguir `rules/git-commit.md` (conventional commits)

## Endpoints de API (Detalhamento)

### POST `/api/barbershops` (atualizado)

**Request Body** (não muda):
```json
{
  "name": "Barbearia Exemplo",
  "document": "12345678901234",
  "ownerName": "João Silva",
  "email": "contato@barbearia.com",
  "phone": "11987654321",
  "zipCode": "01310-100",
  "street": "Avenida Paulista",
  "number": "1000",
  "complement": "Sala 10",
  "neighborhood": "Bela Vista",
  "city": "São Paulo",
  "state": "SP"
}
```

**Response** (201 Created):
```json
{
  "id": "uuid",
  "code": "ABC123XY",
  "name": "Barbearia Exemplo",
  "email": "contato@barbearia.com",
  "isActive": true,
  "createdAt": "2025-10-16T10:00:00Z"
}
```

**Comportamento Adicional**:
- Cria Admin Barbearia com e-mail `contato@barbearia.com`
- Envia e-mail com credenciais para `contato@barbearia.com`
- Rollback se envio de e-mail falhar

**Erros**:
- `400`: Dados inválidos
- `409`: E-mail já cadastrado
- `500`: Falha no envio de e-mail (após 3 tentativas)

### POST `/api/barbershops/{id}/resend-credentials` (novo)

**Authorization**: `Bearer {token}` (role: `AdminCentral`)

**Path Parameters**:
- `id` (uuid): ID da barbearia

**Response** (200 OK):
```json
{
  "message": "Credenciais reenviadas com sucesso."
}
```

**Erros**:
- `401`: Não autenticado
- `403`: Usuário não é Admin Central
- `404`: Barbearia não encontrada
- `400`: Admin Barbearia não encontrado (inconsistência)
- `500`: Falha no envio de e-mail

## Contratos de Dados (Frontend)

```ts
// Sem mudanças nos tipos existentes de Barbershop

// Apenas adicionar método no serviço
interface BarbershopService {
  create(data: CreateBarbershopRequest): Promise<Barbershop>;
  // ... outros métodos existentes
  resendCredentials(id: string): Promise<void>; // NOVO
}
```

## Template de E-mail (HTML Completo)

```html
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Bem-vindo ao BarbApp</title>
  <style>
    body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
    .container { max-width: 600px; margin: 0 auto; padding: 20px; }
    .header { background-color: #4A5568; color: white; padding: 20px; text-align: center; }
    .content { padding: 20px; background-color: #f9f9f9; }
    .credentials { background-color: #E2E8F0; padding: 15px; border-radius: 5px; margin: 20px 0; }
    .button { display: inline-block; padding: 12px 24px; background-color: #4299E1; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }
    .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; }
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>Bem-vindo ao BarbApp!</h1>
    </div>
    <div class="content">
      <p>Olá!</p>
      <p>A barbearia <strong>{{BARBERSHOP_NAME}}</strong> foi cadastrada com sucesso no BarbApp!</p>
      <p>Você pode acessar o sistema de gestão com as credenciais abaixo:</p>
      <div class="credentials">
        <p><strong>E-mail:</strong> {{EMAIL}}</p>
        <p><strong>Senha:</strong> {{PASSWORD}}</p>
      </div>
      <p style="text-align: center;">
        <a href="{{LOGIN_URL}}" class="button">Acessar o Sistema</a>
      </p>
      <p style="font-size: 14px; color: #666;">Por questões de segurança, recomendamos alterar sua senha após o primeiro acesso.</p>
    </div>
    <div class="footer">
      <p>Esta é uma mensagem automática. Por favor, não responda este e-mail.</p>
      <p>&copy; 2025 BarbApp. Todos os direitos reservados.</p>
    </div>
  </div>
</body>
</html>
```

**Nota**: O template será populado dinamicamente com os valores:
- `{{BARBERSHOP_NAME}}`: Nome da barbearia
- `{{EMAIL}}`: E-mail de acesso
- `{{PASSWORD}}`: Senha gerada
- `{{LOGIN_URL}}`: URL parametrizada do frontend (`AppSettings.FrontendUrl + "/login"`)

---

## Checklist de Qualidade

- [x] PRD revisado e requisitos técnicos mapeados
- [x] Análise do repositório e das regras concluída
- [x] Esclarecimentos técnicos documentados
- [x] Tech Spec gerada usando o template
- [x] Fluxos de dados detalhados
- [x] Endpoints de API especificados
- [x] Contratos de componentes definidos
- [x] Abordagem de testes descrita
- [x] Riscos e mitigações identificados
- [x] Conformidade com padrões verificada

---

Data de Criação: 2025-10-16  
Versão: 1.0 (MVP)  
Status: Para Implementação
