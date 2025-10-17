# Guia Rápido de Configuração - Task 15

## 🔧 Configuração do SMTP

### Desenvolvimento (smtp4dev)

**Docker Compose** (recomendado):
```yaml
# docker-compose.yml (adicionar ao existente)
services:
  smtp4dev:
    image: rnwood/smtp4dev:latest
    container_name: barbapp-smtp4dev
    ports:
      - "3000:80"    # UI web
      - "2525:25"    # Porta SMTP
    networks:
      - barbapp-network
```

**Comando Docker Standalone**:
```bash
docker run -d \
  --name barbapp-smtp4dev \
  -p 3000:80 \
  -p 2525:25 \
  rnwood/smtp4dev
```

**Acessar UI**: http://localhost:3000

### Produção

Configure servidor SMTP real (Gmail, Outlook, servidor dedicado, etc.)

---

## 📝 Configuração do Backend

### appsettings.Development.json

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

### appsettings.json (Produção)

```json
{
  "AppSettings": {
    "FrontendUrl": "https://barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-smtp-username",
    "Password": "your-smtp-password",
    "FromEmail": "noreply@barbapp.tasso.dev.br",
    "FromName": "BarbApp"
  }
}
```

**⚠️ Importante**: Em produção, considere usar variáveis de ambiente ou secrets manager para credenciais SMTP.

---

## 📦 Dependências NuGet

Adicionar ao projeto `BarbApp.Infrastructure`:

```bash
dotnet add package MailKit
dotnet add package MimeKit
```

Ou adicionar ao `.csproj`:
```xml
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="MimeKit" Version="4.3.0" />
```

---

## 🎯 Classes a Criar

### Backend

```
BarbApp.Application/
├── Interfaces/
│   ├── IPasswordGenerator.cs          (novo)
│   └── IEmailService.cs               (novo)
├── Models/
│   └── EmailMessage.cs                (novo)
└── UseCases/
    ├── CreateBarbershopUseCase.cs     (atualizar)
    └── ResendCredentialsUseCase.cs    (novo)

BarbApp.Infrastructure/
├── Configuration/
│   ├── SmtpSettings.cs                (novo)
│   └── AppSettings.cs                 (novo)
└── Services/
    ├── SecurePasswordGenerator.cs     (novo)
    └── SmtpEmailService.cs            (novo)

BarbApp.Domain/
└── Repositories/
    └── IAdminBarbeariaRepository.cs   (atualizar - adicionar GetByEmailAsync)

BarbApp.Infrastructure/
└── Repositories/
    └── AdminBarbeariaRepository.cs    (atualizar - implementar GetByEmailAsync)

BarbApp.API/
└── Controllers/
    └── BarbershopsController.cs       (atualizar - adicionar ResendCredentials)
```

### Frontend

```
barbapp-admin/src/
├── services/
│   └── barbershop.service.ts          (atualizar - adicionar resendCredentials)
├── components/barbershop/
│   ├── ResendCredentialsModal.tsx     (novo)
│   └── BarbershopTable.tsx            (atualizar - adicionar ação de reenvio)
└── pages/Barbershops/
    ├── Create.tsx                     (atualizar - mensagem de sucesso)
    └── List.tsx                       (atualizar - botão de reenvio)
```

---

## 🧪 Testando o Fluxo

### 1. Iniciar smtp4dev

```bash
docker run -d -p 3000:80 -p 2525:25 rnwood/smtp4dev
```

### 2. Cadastrar Barbearia

```bash
curl -X POST http://localhost:5000/api/barbershops \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token-admin-central}" \
  -d '{
    "name": "Barbearia Teste",
    "document": "12345678901234",
    "ownerName": "João Silva",
    "email": "joao@barbearia.com",
    "phone": "11987654321",
    "zipCode": "01310-100",
    "street": "Av Paulista",
    "number": "1000",
    "neighborhood": "Bela Vista",
    "city": "São Paulo",
    "state": "SP"
  }'
```

### 3. Verificar E-mail

Acesse: http://localhost:3000

Você verá o e-mail com as credenciais!

### 4. Reenviar Credenciais

```bash
curl -X POST http://localhost:5000/api/barbershops/{id}/resend-credentials \
  -H "Authorization: Bearer {token-admin-central}"
```

---

## ✅ Checklist de Implementação

### Backend - Fase 1: Infraestrutura
- [ ] Adicionar pacotes NuGet (MailKit, MimeKit)
- [ ] Criar `SmtpSettings.cs` e `AppSettings.cs`
- [ ] Criar `IPasswordGenerator.cs` e `SecurePasswordGenerator.cs`
- [ ] Criar `IEmailService.cs` e `SmtpEmailService.cs`
- [ ] Configurar `appsettings.Development.json`
- [ ] Registrar serviços no DI (Program.cs)

### Backend - Fase 2: Repositório
- [ ] Adicionar método `GetByEmailAsync` em `IAdminBarbeariaRepository`
- [ ] Implementar `GetByEmailAsync` em `AdminBarbeariaRepository`

### Backend - Fase 3: Use Cases
- [ ] Atualizar `CreateBarbershopUseCase`:
  - [ ] Injetar dependências (IPasswordGenerator, IEmailService, AppSettings)
  - [ ] Adicionar validação de e-mail único
  - [ ] Gerar senha aleatória
  - [ ] Criar Admin Barbearia
  - [ ] Enviar e-mail com credenciais
  - [ ] Adicionar transação (rollback se e-mail falhar)
- [ ] Criar `ResendCredentialsUseCase`:
  - [ ] Buscar barbearia e Admin Barbearia
  - [ ] Gerar nova senha
  - [ ] Atualizar password_hash
  - [ ] Enviar e-mail

### Backend - Fase 4: API
- [ ] Adicionar endpoint `POST /api/barbershops/{id}/resend-credentials`
- [ ] Autorização: role `AdminCentral`

### Frontend - Fase 1: Serviço
- [ ] Adicionar método `resendCredentials` em `barbershop.service.ts`

### Frontend - Fase 2: Componentes
- [ ] Criar `ResendCredentialsModal.tsx`
- [ ] Atualizar `BarbershopTable.tsx` (adicionar ação de reenvio)

### Frontend - Fase 3: Páginas
- [ ] Atualizar `BarbershopList.tsx` (botão e lógica de reenvio)
- [ ] Atualizar `BarbershopCreate.tsx` (mensagem de sucesso)

### Testes
- [ ] Testes unitários (backend): PasswordGenerator, EmailService, UseCases
- [ ] Testes de integração (backend): endpoint de reenvio
- [ ] Testes de integração (frontend): componentes e páginas
- [ ] Testes E2E: fluxo completo de cadastro e reenvio

---

## 🐛 Troubleshooting

### E-mail não está sendo enviado

1. Verificar se smtp4dev está rodando: `docker ps | grep smtp4dev`
2. Verificar logs do backend: procurar por "Email sent" ou erros
3. Verificar configuração SMTP no `appsettings.Development.json`
4. Testar conexão manual: `telnet localhost 2525`

### Erro 409 (Conflito) ao cadastrar barbearia

- E-mail já está cadastrado. Verifique no banco: `SELECT * FROM admin_barbearia_users WHERE email = 'email@teste.com'`

### Rollback de transação

- Se e-mail falhar, barbearia e Admin Barbearia **não devem** ser criados
- Verificar logs: "Failed to create barbershop with Admin user"

---

## 📚 Referências

- **smtp4dev**: https://github.com/rnwood/smtp4dev
- **MailKit**: https://github.com/jstedfast/MailKit
- **BCrypt.Net**: https://github.com/BcryptNet/bcrypt.net

---

Data: 2025-10-16  
Task: #15 - Onboarding Automático do Admin da Barbearia
