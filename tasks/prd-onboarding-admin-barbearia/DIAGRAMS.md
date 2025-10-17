# Diagramas de Fluxo - Task 15

## 🔄 Fluxo Principal: Cadastro de Barbearia com Criação de Admin

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CADASTRO DE BARBEARIA                               │
└─────────────────────────────────────────────────────────────────────────────┘

┌──────────────┐
│ Admin Central│
│  (Frontend)  │
└──────┬───────┘
       │
       │ 1. Preenche formulário de cadastro
       │    (nome, email, telefone, endereço, etc.)
       │
       │ 2. POST /api/barbershops
       │    { name, email, ownerName, ... }
       ▼
┌──────────────────────────────────────────┐
│        BarbershopsController             │
│         CreateBarbershopUseCase          │
└──────────────┬───────────────────────────┘
               │
               │ 3. Validar dados de entrada
               │
               │ 4. Validar e-mail único
               ├──────────────────────────────────┐
               │                                  │
               ▼                                  ▼
    ┌──────────────────────┐        ┌──────────────────────┐
    │ AdminBarbeariaRepo   │        │   CONFLITO 409       │
    │ GetByEmailAsync()    │───────▶│ "E-mail já existe"   │
    └──────────────────────┘        └──────────────────────┘
               │
               │ E-mail disponível
               │
               │ 5. BeginTransaction()
               ▼
    ┌─────────────────────────────────────────────┐
    │           TRANSACTION SCOPE                 │
    │                                             │
    │  6. Criar Barbearia                         │
    │     BarbershopRepository.AddAsync()         │
    │     ✓ Gerar código único (ABC123XY)         │
    │                                             │
    │  7. Gerar senha aleatória                   │
    │     PasswordGenerator.Generate()            │
    │     ✓ 12 caracteres                         │
    │     ✓ Maiúsculas, minúsculas, números,      │
    │       símbolos                              │
    │                                             │
    │  8. Criar Admin Barbearia                   │
    │     AdminBarbeariaRepo.AddAsync()           │
    │     ✓ Email = barbearia.email               │
    │     ✓ PasswordHash = BCrypt(senha)          │
    │     ✓ BarbeariaId = barbearia.id            │
    │     ✓ IsActive = true                       │
    │                                             │
    │  9. Enviar e-mail com credenciais           │
    │     EmailService.SendAsync()                │
    │     ┌─────────────────────────────────┐     │
    │     │ Retry: 3 tentativas             │     │
    │     │ Backoff: 1s, 2s, 4s             │     │
    │     │ ✓ Conectar SMTP                 │     │
    │     │ ✓ Autenticar (se necessário)    │     │
    │     │ ✓ Enviar HTML + texto           │     │
    │     │ ✓ Desconectar                   │     │
    │     └─────────────────────────────────┘     │
    │            │                                │
    │            ├─── SUCESSO ───┐                │
    │            │                │                │
    │            ├─── RETRY 1 ───┤                │
    │            │                │                │
    │            ├─── RETRY 2 ────┤               │
    │            │                │                │
    │            ├─── RETRY 3 ─────┤              │
    │            │                 │               │
    │            ▼                 ▼               │
    │       ┌─────────┐      ┌─────────┐          │
    │       │ SUCESSO │      │  ERRO   │          │
    │       └────┬────┘      └────┬────┘          │
    │            │                 │               │
    │            │                 │               │
    │  10a.      │                 │  10b.         │
    │  Commit()  │                 │  Rollback()   │
    │            │                 │               │
    └────────────┼─────────────────┼───────────────┘
                 │                 │
                 ▼                 ▼
         ┌──────────────┐  ┌──────────────┐
         │  201 Created │  │  500 Error   │
         │  {barbershop}│  │  Rollback OK │
         └──────┬───────┘  └──────┬───────┘
                │                 │
                ▼                 ▼
         ┌──────────────┐  ┌──────────────┐
         │   Frontend   │  │   Frontend   │
         │  Toast       │  │  Toast Erro  │
         │  Sucesso!    │  │  Tente       │
         │  Credenciais │  │  novamente   │
         │  enviadas    │  │              │
         └──────────────┘  └──────────────┘
```

---

## 📧 Fluxo de E-mail

```
┌────────────────────────────────────────────────────────────────┐
│                    ENVIO DE E-MAIL                             │
└────────────────────────────────────────────────────────────────┘

Backend (.NET)                    SMTP Server             Admin Barbearia
──────────────                    ───────────             ───────────────

EmailService
    │
    │ 1. Criar MimeMessage
    │    ├─ From: noreply@barbapp.tasso.dev.br
    │    ├─ To: email@barbearia.com
    │    ├─ Subject: Bem-vindo ao BarbApp
    │    ├─ HtmlBody: Template HTML
    │    └─ TextBody: Template texto
    │
    │ 2. Conectar SMTP
    ├────────────────────────▶  localhost:2525 (dev)
    │                          smtp.example.com:587 (prod)
    │
    │ 3. Autenticar (se necessário)
    │    RequiresAuthentication == true?
    ├────────────────────────▶  Username + Password
    │                          (dev: sem autenticação)
    │
    │ 4. Enviar mensagem
    ├────────────────────────▶  SMTP SEND
    │                              │
    │                              │ Processando...
    │                              │
    │ 5. Confirmar envio       ◀───┘
    ◀────────────────────────
    │
    │ 6. Desconectar
    ├────────────────────────▶  SMTP QUIT
    │
    │ 7. Log de sucesso
    │    "Email sent successfully to {email}"
    │
    └─────────────────────────────────────────────▶  📬 Inbox
                                                    
                                                    ┌──────────────┐
                                                    │ ✂️ Bem-vindo  │
                                                    │ ao BarbApp!  │
                                                    │              │
                                                    │ Email: xxx   │
                                                    │ Senha: yyy   │
                                                    │              │
                                                    │ [Acessar]    │
                                                    └──────────────┘
```

---

## 🔄 Fluxo de Reenvio de Credenciais

```
┌────────────────────────────────────────────────────────────────┐
│                  REENVIO DE CREDENCIAIS                        │
└────────────────────────────────────────────────────────────────┘

Admin Central (Frontend)      Backend (.NET)           SMTP Server
────────────────────────      ──────────────          ────────────

1. Clicar em "Reenviar
   Credenciais" na lista
         │
         │ 2. Modal de confirmação
         │    ┌─────────────────────────┐
         │    │ Barbearia: Teste        │
         │    │ E-mail: test@test.com   │
         │    │ ⚠️ Nova senha gerada     │
         │    │ [Cancelar] [Confirmar]  │
         │    └─────────────────────────┘
         │
         │ 3. Confirmar
         ▼
    POST /api/barbershops/{id}/resend-credentials
         │
         ▼
    ResendCredentialsUseCase
         │
         │ 4. Buscar barbearia
         ├───────▶ BarbershopRepo.GetByIdAsync()
         │               │
         │               ├─ Encontrou? ──▶ OK
         │               └─ Não? ────────▶ 404 Not Found
         │
         │ 5. Buscar Admin Barbearia
         ├───────▶ AdminBarbeariaRepo.GetByBarbeariaIdAsync()
         │               │
         │               ├─ Encontrou? ──▶ OK
         │               └─ Não? ────────▶ 400 Bad Request
         │
         │ 6. Gerar nova senha
         ├───────▶ PasswordGenerator.Generate()
         │            ✓ Nova senha: XYZ789abc!@#
         │
         │ 7. Atualizar Admin Barbearia
         ├───────▶ AdminBarbeariaRepo.UpdateAsync()
         │            ✓ PasswordHash = BCrypt(nova_senha)
         │            ✓ UpdatedAt = NOW()
         │
         │ 8. Enviar e-mail (retry 3x)
         ├───────▶ EmailService.SendAsync()
         │                  │
         │                  ├──────────────▶ SMTP SEND
         │                  │                     │
         │                  │                     │ ✓ Enviado
         │                  ◀──────────────       │
         │                  │
         │                  │ Log: "Credentials resent for {id}"
         │
         │ 9. Retornar 200 OK
         ▼
    { message: "Credenciais reenviadas com sucesso." }
         │
         ▼
    Toast de sucesso
    ┌─────────────────────────┐
    │ ✓ Credenciais           │
    │   reenviadas!           │
    │   Enviadas para:        │
    │   test@test.com         │
    └─────────────────────────┘
```

---

## 🗄️ Modelo de Dados

```
┌──────────────────────────────────────────────────────────────┐
│                    TABELAS AFETADAS                          │
└──────────────────────────────────────────────────────────────┘

barbershops
├─ barbershop_id (PK, UUID)
├─ code (UNIQUE, VARCHAR(8))
├─ name
├─ email ◀────────────────────────┐ (mesmo e-mail)
├─ phone                          │
├─ owner_name                     │
├─ document                       │
├─ address_id (FK)                │
├─ is_active                      │
└─ created_at                     │
                                  │
admin_barbearia_users             │
├─ admin_barbearia_user_id (PK)   │
├─ barbearia_id (FK) ─────────────┤
├─ email ─────────────────────────┘
├─ password_hash (BCrypt)
├─ name (= barbershop.owner_name)
├─ is_active
├─ created_at
└─ updated_at


RELACIONAMENTO:
1 Barbearia ──── 1 Admin Barbearia (1:1)

ÍNDICES:
- ix_admin_barbearia_users_email_barbearia_id (UNIQUE composto)
- ix_admin_barbearia_users_barbearia_id (FK)

VALIDAÇÃO:
- E-mail único: validado na aplicação (GetByEmailAsync)
- Não há índice UNIQUE global em email
```

---

## 🔐 Fluxo de Geração de Senha

```
┌────────────────────────────────────────────────────────────────┐
│               GERAÇÃO DE SENHA SEGURA                          │
└────────────────────────────────────────────────────────────────┘

PasswordGenerator.Generate(12)
    │
    │ 1. Definir conjuntos de caracteres
    ├─ UpperCase: ABCDEFGHIJKLMNOPQRSTUVWXYZ
    ├─ LowerCase: abcdefghijklmnopqrstuvwxyz
    ├─ Digits:    0123456789
    └─ Special:   !@#$%&*-_+=
    
    │ 2. Garantir pelo menos 1 de cada tipo
    │    (4 primeiros caracteres)
    ├─ [0] = RandomChar(UpperCase)    → 'A'
    ├─ [1] = RandomChar(LowerCase)    → 'x'
    ├─ [2] = RandomChar(Digits)       → '7'
    └─ [3] = RandomChar(Special)      → '@'
    
    │ 3. Preencher restante (8 caracteres)
    │    com caracteres aleatórios de qualquer conjunto
    ├─ [4] = 'b'
    ├─ [5] = '3'
    ├─ [6] = 'Z'
    ├─ [7] = '!'
    ├─ [8] = 'm'
    ├─ [9] = '9'
    ├─ [10] = 'K'
    └─ [11] = '#'
    
    │ 4. Embaralhar array (shuffle)
    │    para remover padrão previsível
    │
    │    Antes:  ['A','x','7','@','b','3','Z','!','m','9','K','#']
    │    Depois: ['3','K','@','x','m','#','A','9','b','!','Z','7']
    │
    ▼
Senha final: "3K@xm#A9b!Z7"

✅ Requisitos atendidos:
   ✓ 12 caracteres
   ✓ Pelo menos 1 maiúscula
   ✓ Pelo menos 1 minúscula
   ✓ Pelo menos 1 número
   ✓ Pelo menos 1 símbolo
   ✓ Criptograficamente seguro (RandomNumberGenerator)
   ✓ Não previsível (shuffle)
```

---

## 📊 Estados do Sistema

```
┌────────────────────────────────────────────────────────────────┐
│                 ESTADOS DO ADMIN BARBEARIA                     │
└────────────────────────────────────────────────────────────────┘

┌─────────────────┐
│ NÃO EXISTE      │  ← Barbearia ainda não foi cadastrada
└────────┬────────┘
         │
         │ Admin Central cadastra barbearia
         │ POST /api/barbershops
         ▼
┌─────────────────┐
│ CRIADO          │  ← Admin Barbearia criado automaticamente
│ (IsActive=true) │     Senha gerada e enviada por e-mail
└────────┬────────┘
         │
         │ Admin Barbearia recebe e-mail
         │ E-mail: xxx@barbearia.com
         │ Senha: ABC123xyz!@#
         ▼
┌─────────────────┐
│ ATIVO           │  ← Admin pode fazer login imediatamente
│ (IsActive=true) │     POST /api/auth/login (role: AdminBarbearia)
└────────┬────────┘
         │
         │
         ├───────────────┐
         │               │
         │               │ Admin Central clica "Reenviar Credenciais"
         │               │ POST /api/barbershops/{id}/resend-credentials
         │               ▼
         │        ┌─────────────────┐
         │        │ NOVA SENHA      │  ← Nova senha gerada
         │        │ GERADA          │     Antigo password_hash substituído
         │        └────────┬────────┘     Novo e-mail enviado
         │                 │
         │                 │ Admin Barbearia recebe novo e-mail
         │                 │ Senha antiga INVÁLIDA
         │                 │ Senha nova: XYZ789def$%^
         │                 ▼
         │        ┌─────────────────┐
         │        │ ATIVO (nova     │  ← Login com nova senha
         │        │ senha)          │
         │        └─────────────────┘
         │
         │ Admin Central desativa barbearia (futuro)
         │ PUT /api/barbershops/{id}/deactivate
         ▼
┌─────────────────┐
│ INATIVO         │  ← Login bloqueado
│ (IsActive=false)│     
└─────────────────┘
```

---

## 🌐 Ambientes e URLs

```
┌────────────────────────────────────────────────────────────────┐
│                    AMBIENTES                                   │
└────────────────────────────────────────────────────────────────┘

DESENVOLVIMENTO (DEV)
─────────────────────
Frontend:   https://dev-barbapp.tasso.dev.br
Backend:    http://localhost:5000
SMTP:       localhost:2525 (smtp4dev)
SMTP UI:    http://localhost:3000

┌─────────────────────────────┐
│ smtp4dev (Docker)           │
│ ─────────────────           │
│ Porta SMTP: 2525            │
│ Porta HTTP: 3000            │
│ Sem autenticação            │
│ Captura todos os e-mails    │
└─────────────────────────────┘

appsettings.Development.json:
{
  "AppSettings": {
    "FrontendUrl": "https://dev-barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "localhost",
    "Port": 2525,
    "UseSsl": false,
    "Username": "",
    "Password": ""
  }
}


PRODUÇÃO (PROD)
───────────────
Frontend:   https://barbapp.tasso.dev.br
Backend:    https://api.barbapp.tasso.dev.br
SMTP:       smtp.provider.com:587
SMTP UI:    N/A (e-mails reais enviados)

┌─────────────────────────────┐
│ Servidor SMTP Real          │
│ ─────────────────           │
│ Porta SMTP: 587             │
│ Com autenticação            │
│ TLS/SSL habilitado          │
│ Credenciais em secrets      │
└─────────────────────────────┘

appsettings.json:
{
  "AppSettings": {
    "FrontendUrl": "https://barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "smtp.example.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}"
  }
}
```

---

Data: 2025-10-16  
Task: #15 - Onboarding Automático do Admin da Barbearia
