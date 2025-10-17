# Tasks - Onboarding Automático do Admin da Barbearia

## Visão Geral

Este documento descreve as tarefas técnicas necessárias para implementar o fluxo de onboarding automático do Admin da Barbearia, conforme definido no PRD e Tech Spec. As tarefas estão organizadas por camada e seguem uma ordem lógica de implementação.

## Estimativa Total

- **Backend**: ~13 pontos
- **Frontend**: ~5 pontos
- **Testes**: ~5 pontos
- **Total**: ~23 pontos (aproximadamente 3-4 dias de desenvolvimento)

---

## Backend

### Task 15.1: Criar Interface e Implementação do Gerador de Senha Seguro

**Descrição**: Implementar serviço para geração de senhas aleatórias seguras com 12 caracteres (maiúsculas, minúsculas, números, símbolos).

**Arquivos**:
- `backend/src/BarbApp.Application/Interfaces/IPasswordGenerator.cs` (novo)
- `backend/src/BarbApp.Infrastructure/Services/SecurePasswordGenerator.cs` (novo)

**Detalhes**:
- Interface `IPasswordGenerator` com método `Generate(int length = 12): string`
- Implementação usando `RandomNumberGenerator` (criptograficamente seguro)
- Garantir pelo menos 1 maiúscula, 1 minúscula, 1 número, 1 símbolo
- Embaralhar caracteres para evitar padrão previsível
- Lançar exceção se length < 8

**Testes**:
- Testes unitários: validar formato, comprimento, diversidade de caracteres

**Estimativa**: 2 pontos

---

### Task 15.2: Criar Interface e Implementação do Serviço de E-mail (SMTP)

**Descrição**: Implementar serviço de envio de e-mails via SMTP com retry e configuração externa.

**Arquivos**:
- `backend/src/BarbApp.Application/Interfaces/IEmailService.cs` (novo)
- `backend/src/BarbApp.Application/Models/EmailMessage.cs` (novo)
- `backend/src/BarbApp.Infrastructure/Services/SmtpEmailService.cs` (novo)
- `backend/src/BarbApp.Infrastructure/Configuration/SmtpSettings.cs` (novo)
- `backend/src/BarbApp.API/appsettings.json` (atualizar)
- `backend/src/BarbApp.API/appsettings.Development.json` (atualizar)

**Detalhes**:
- Interface `IEmailService` com método `SendAsync(EmailMessage, CancellationToken)`
- Modelo `EmailMessage` com propriedades `To`, `Subject`, `HtmlBody`, `TextBody`
- Implementação com MailKit (adicionar pacote NuGet)
- Retry: 3 tentativas com backoff exponencial (1s, 2s, 4s)
- Logging de sucesso/falha
- Configurar SMTP em `appsettings.json` (localhost:587 para dev)

**Dependências**:
- NuGet: `MailKit` (ou usar `System.Net.Mail` nativo)

**Testes**:
- Testes unitários: mock de SMTP, validar retry, validar logging

**Estimativa**: 3 pontos

---

### Task 15.3: Atualizar CreateBarbershopUseCase para Criar Admin Barbearia e Enviar E-mail

**Descrição**: Estender use case de criação de barbearia para criar automaticamente usuário Admin Barbearia e enviar e-mail com credenciais.

**Arquivos**:
- `backend/src/BarbApp.Application/UseCases/CreateBarbershopUseCase.cs` (atualizar)

**Detalhes**:
- Injetar `IPasswordGenerator`, `IEmailService`, `IAdminBarbeariaRepository`
- Após criar barbearia, gerar senha aleatória
- Criar usuário `AdminBarbeariaUser` com email, password_hash (BCrypt), barbearia_id
- Enviar e-mail com template de boas-vindas (HTML + texto)
- Envolver tudo em transação (UnitOfWork): rollback se e-mail falhar
- Logging de criação de Admin Barbearia

**Templates de E-mail**:
- Método privado `BuildWelcomeEmailHtml(barbershopName, email, password): string`
- Método privado `BuildWelcomeEmailText(barbershopName, email, password): string`

**Testes**:
- Testes unitários: mock de dependências, validar criação de usuário, validar envio de e-mail
- Testes de integração: criar barbearia e verificar registro em `admin_barbearia_users`

**Estimativa**: 3 pontos

---

### Task 15.4: Criar ResendCredentialsUseCase

**Descrição**: Implementar use case para reenvio de credenciais (gera nova senha e envia e-mail).

**Arquivos**:
- `backend/src/BarbApp.Application/UseCases/ResendCredentialsUseCase.cs` (novo)

**Detalhes**:
- Injetar `IBarbershopRepository`, `IAdminBarbeariaRepository`, `IPasswordGenerator`, `IEmailService`
- Buscar barbearia (retornar 404 se não existir)
- Buscar Admin Barbearia vinculado (retornar 400 se não existir)
- Gerar nova senha aleatória
- Atualizar `password_hash` e `updated_at` do usuário
- Enviar e-mail com template de reenvio (similar ao de boas-vindas, ajustar texto)
- Logging de reenvio (auditoria)

**Testes**:
- Testes unitários: validar fluxo completo, erros 404/400, logging
- Testes de integração: reenviar credenciais e verificar atualização de senha

**Estimativa**: 2 pontos

---

### Task 15.5: Adicionar Endpoint de Reenvio de Credenciais no BarbershopsController

**Descrição**: Adicionar rota `POST /api/barbershops/{id}/resend-credentials` restrita a Admin Central.

**Arquivos**:
- `backend/src/BarbApp.API/Controllers/BarbershopsController.cs` (atualizar)

**Detalhes**:
- Injetar `ResendCredentialsUseCase`
- Endpoint: `[HttpPost("{id}/resend-credentials")]`
- Autorização: `[Authorize(Roles = "AdminCentral")]`
- Retornar 200 com mensagem de sucesso
- Tratamento de erros: 404, 400, 500

**Testes**:
- Testes de integração: chamar endpoint e verificar resposta
- Validar autorização (401 sem token, 403 sem role)

**Estimativa**: 1 ponto

---

### Task 15.6: Registrar Dependências no Container de DI

**Descrição**: Registrar novos serviços no container de injeção de dependências.

**Arquivos**:
- `backend/src/BarbApp.API/Program.cs` (ou arquivo de configuração de DI)

**Detalhes**:
- Registrar `IPasswordGenerator` → `SecurePasswordGenerator` (Scoped ou Transient)
- Registrar `IEmailService` → `SmtpEmailService` (Scoped)
- Configurar `SmtpSettings` via `IOptions<SmtpSettings>`
- Registrar `ResendCredentialsUseCase` (Scoped)

**Estimativa**: 1 ponto

---

### Task 15.7: Adicionar Validação de E-mail Único no Repository

**Descrição**: Adicionar método no repository para validar unicidade de e-mail (validação na aplicação).

**Arquivos**:
- `backend/src/BarbApp.Domain/Repositories/IAdminBarbeariaRepository.cs` (atualizar)
- `backend/src/BarbApp.Infrastructure/Repositories/AdminBarbeariaRepository.cs` (atualizar)

**Detalhes**:
- Adicionar método `Task<AdminBarbeariaUser?> GetByEmailAsync(string email, CancellationToken cancellationToken)`
- Implementar query que busca por e-mail (case-insensitive): `context.AdminBarbeariaUsers.FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower())`
- Usar no `CreateBarbershopUseCase` antes de criar Admin Barbearia
- Lançar `ConflictException` (409) se e-mail já existir
- **Nota**: Mantemos índice composto existente `(email, barbearia_id)` para performance em queries

**Estimativa**: 1 ponto

---

## Frontend

### Task 15.8: Adicionar Método de Reenvio de Credenciais no Serviço de Barbearias

**Descrição**: Estender `barbershop.service.ts` para incluir método `resendCredentials`.

**Arquivos**:
- `barbapp-admin/src/services/barbershop.service.ts` (atualizar)

**Detalhes**:
- Adicionar método `resendCredentials(id: string): Promise<void>`
- Chamar `POST /api/barbershops/${id}/resend-credentials`
- Tratamento de erros padrão (Axios)

**Estimativa**: 1 ponto

---

### Task 15.9: Criar Modal de Confirmação de Reenvio de Credenciais

**Descrição**: Criar componente modal para confirmar reenvio de credenciais.

**Arquivos**:
- `barbapp-admin/src/components/barbershop/ResendCredentialsModal.tsx` (novo)

**Detalhes**:
- Props: `open`, `onClose`, `onConfirm`, `barbershopName`, `barbershopEmail`, `isLoading`
- Exibir nome da barbearia e e-mail
- Aviso sobre geração de nova senha
- Botões: "Cancelar" e "Confirmar Reenvio"
- Loading state em "Confirmar Reenvio"

**Testes**:
- Testes unitários: renderização, interações (cancelar, confirmar)

**Estimativa**: 1 ponto

---

### Task 15.10: Adicionar Botão de Reenvio de Credenciais na Listagem de Barbearias

**Descrição**: Adicionar ação de reenvio de credenciais na tabela de barbearias.

**Arquivos**:
- `barbapp-admin/src/pages/Barbershops/List.tsx` (atualizar)
- `barbapp-admin/src/components/barbershop/BarbershopTable.tsx` (atualizar)

**Detalhes**:
- Adicionar estado para modal de reenvio e dados selecionados
- Adicionar handler `handleResendCredentials(id)`
- Adicionar handler `confirmResendCredentials()` com chamada ao serviço
- Passar prop `onResendCredentials` para `BarbershopTable`
- Em `BarbershopTable`, adicionar item "Reenviar Credenciais" no dropdown de ações
- Exibir `ResendCredentialsModal` com controle de estado

**Testes**:
- Testes de integração: clicar em reenviar, confirmar modal, verificar toast

**Estimativa**: 2 pontos

---

### Task 15.11: Atualizar Mensagem de Sucesso no Cadastro de Barbearia

**Descrição**: Ajustar toast de sucesso após cadastro para informar sobre envio de credenciais.

**Arquivos**:
- `barbapp-admin/src/pages/Barbershops/Create.tsx` (atualizar)

**Detalhes**:
- Atualizar mensagem de toast de sucesso
- Texto sugerido: "Barbearia cadastrada com sucesso! Credenciais de acesso enviadas para {email}"

**Estimativa**: 1 ponto

---

## Testes

### Task 15.12: Escrever Testes Unitários (Backend)

**Descrição**: Cobrir novos serviços e use cases com testes unitários.

**Arquivos**:
- `backend/tests/BarbApp.UnitTests/Infrastructure/Services/SecurePasswordGeneratorTests.cs` (novo)
- `backend/tests/BarbApp.UnitTests/Infrastructure/Services/SmtpEmailServiceTests.cs` (novo)
- `backend/tests/BarbApp.UnitTests/Application/UseCases/CreateBarbershopUseCaseTests.cs` (atualizar)
- `backend/tests/BarbApp.UnitTests/Application/UseCases/ResendCredentialsUseCaseTests.cs` (novo)

**Cobertura**:
- `SecurePasswordGenerator`: formato, comprimento, diversidade
- `SmtpEmailService`: retry, logging, exceções
- `CreateBarbershopUseCase`: criação de Admin Barbearia, envio de e-mail, rollback
- `ResendCredentialsUseCase`: fluxo completo, erros 404/400, logging

**Estimativa**: 2 pontos

---

### Task 15.13: Escrever Testes de Integração (Backend)

**Descrição**: Testar fluxo completo de cadastro de barbearia e reenvio de credenciais.

**Arquivos**:
- `backend/tests/BarbApp.IntegrationTests/API/BarbershopsControllerTests.cs` (atualizar)

**Cobertura**:
- POST `/api/barbershops`: verificar criação de Admin Barbearia
- POST `/api/barbershops/{id}/resend-credentials`: verificar atualização de senha
- Validar autorização (401, 403)
- Mock de envio de e-mail (ou usar MailHog)

**Estimativa**: 2 pontos

---

### Task 15.14: Escrever Testes de Integração (Frontend)

**Descrição**: Testar componentes e páginas com MSW.

**Arquivos**:
- `barbapp-admin/tests/components/barbershop/ResendCredentialsModal.test.tsx` (novo)
- `barbapp-admin/tests/pages/Barbershops/List.test.tsx` (atualizar)
- `barbapp-admin/tests/pages/Barbershops/Create.test.tsx` (atualizar)

**Cobertura**:
- `ResendCredentialsModal`: renderização, interações
- `BarbershopList`: clicar em reenviar, confirmar, verificar toast
- `BarbershopCreate`: verificar mensagem de sucesso atualizada

**Estimativa**: 1 ponto

---

### Task 15.15: Escrever Testes E2E (Playwright)

**Descrição**: Testar fluxos críticos de ponta a ponta.

**Arquivos**:
- `barbapp-admin/tests/e2e/barbershops/create-and-onboarding.spec.ts` (novo)
- `barbapp-admin/tests/e2e/barbershops/resend-credentials.spec.ts` (novo)

**Cobertura**:
- Cadastro de barbearia: preencher formulário, verificar toast, verificar e-mail (MailHog ou mock)
- Reenvio de credenciais: acessar listagem, clicar em reenviar, confirmar, verificar toast

**Estimativa**: 0 pontos (pode ser feito como parte das tarefas anteriores ou como refinamento posterior)

---

## Checklist de Implementação

### Backend
- [ ] Task 15.1: Gerador de senha seguro
- [ ] Task 15.2: Serviço de e-mail (SMTP)
- [ ] Task 15.3: Atualizar CreateBarbershopUseCase
- [ ] Task 15.4: Criar ResendCredentialsUseCase
- [ ] Task 15.5: Endpoint de reenvio no controller
- [ ] Task 15.6: Registrar dependências (DI)
- [ ] Task 15.7: Validação de e-mail único no repository

### Frontend
- [ ] Task 15.8: Método de reenvio no serviço
- [ ] Task 15.9: Modal de confirmação
- [ ] Task 15.10: Botão de reenvio na listagem
- [ ] Task 15.11: Atualizar mensagem de sucesso no cadastro

### Testes
- [ ] Task 15.12: Testes unitários (backend)
- [ ] Task 15.13: Testes de integração (backend)
- [ ] Task 15.14: Testes de integração (frontend)
- [ ] Task 15.15: Testes E2E (Playwright)

---

## Dependências Entre Tarefas

```
Backend:
15.1 (Gerador de senha) → 15.3, 15.4
15.2 (Serviço de e-mail) → 15.3, 15.4
15.3 (CreateBarbershopUseCase) → 15.5, 15.6
15.4 (ResendCredentialsUseCase) → 15.5, 15.6
15.5 (Endpoint) → 15.8 (Frontend)
15.6 (DI) → 15.12, 15.13 (Testes)
15.7 (Migração) → 15.3

Frontend:
15.8 (Serviço) → 15.10
15.9 (Modal) → 15.10
15.10 (Listagem) → 15.14, 15.15
15.11 (Mensagem) → 15.14, 15.15

Testes:
15.12, 15.13, 15.14 → 15.15 (E2E como validação final)
```

---

## Ordem de Implementação Sugerida

1. **Backend - Infraestrutura**: Tasks 15.1, 15.2, 15.7 (paralelo)
2. **Backend - Use Cases**: Tasks 15.3, 15.4 (sequencial)
3. **Backend - API**: Tasks 15.5, 15.6 (sequencial)
4. **Frontend - Serviço e Componentes**: Tasks 15.8, 15.9 (paralelo)
5. **Frontend - Integração**: Tasks 15.10, 15.11 (sequencial)
6. **Testes**: Tasks 15.12, 15.13, 15.14 (paralelo), depois 15.15

---

## Configuração Necessária

### Backend (appsettings.json - Produção)

```json
{
  "AppSettings": {
    "FrontendUrl": "https://barbapp.tasso.dev.br"
  },
  "SmtpSettings": {
    "Host": "smtp.your-provider.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-smtp-username",
    "Password": "your-smtp-password",
    "FromEmail": "noreply@barbapp.tasso.dev.br",
    "FromName": "BarbApp"
  }
}
```

### Backend (appsettings.Development.json - Desenvolvimento)

```json
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

### Ambiente de Desenvolvimento

- **smtp4dev**: Para capturar e-mails em dev/test (https://github.com/rnwood/smtp4dev)
  - Docker: `docker run -d -p 3000:80 -p 2525:25 rnwood/smtp4dev`
  - Configurar SMTP no backend: `localhost:2525` (porta SMTP do container)
  - Acessar UI web: `http://localhost:3000` (porta HTTP do container)
  - **Nota**: Ajustar porta no `appsettings.Development.json` para `2525` (porta SMTP do smtp4dev)

### Ambiente de Produção

- Configurar servidor SMTP de produção (pode ser Gmail, Outlook, SendGrid, AWS SES, etc.)
- Garantir que credenciais SMTP estejam em variáveis de ambiente ou secrets manager
- URL do frontend: `https://barbapp.tasso.dev.br`

---

## Critérios de Aceitação

### Backend
- [ ] Admin Barbearia é criado automaticamente ao cadastrar barbearia
- [ ] E-mail com credenciais é enviado com sucesso
- [ ] Transação é revertida se e-mail falhar
- [ ] Endpoint de reenvio funciona corretamente
- [ ] Nova senha é gerada e enviada ao reenviar
- [ ] Logs registram eventos críticos (criação, envio, reenvio)
- [ ] Testes unitários e integração passam (cobertura > 80%)

### Frontend
- [ ] Mensagem de sucesso informa sobre envio de credenciais
- [ ] Botão "Reenviar Credenciais" aparece na listagem
- [ ] Modal de confirmação exibe informações corretas
- [ ] Toast de sucesso/erro é exibido após reenvio
- [ ] Testes de integração passam

### E2E
- [ ] Fluxo de cadastro cria Admin Barbearia e envia e-mail
- [ ] Fluxo de reenvio gera nova senha e envia e-mail
- [ ] E-mails são recebidos corretamente (MailHog ou mock)

---

## Notas Adicionais

- **Segurança**: Nunca logar senha em plain text; usar apenas logging de eventos (sucesso/falha).
- **LGPD**: E-mails devem conter apenas dados necessários; considerar opt-out em versões futuras.
- **Performance**: Se envio de e-mail impactar tempo de resposta (>5s), considerar processamento assíncrono (fila) em versão futura.
- **URLs Parametrizadas**: URLs do frontend são configuradas via `AppSettings.FrontendUrl` (prod: `barbapp.tasso.dev.br`, dev: `dev-barbapp.tasso.dev.br`).
- **SMTP sem Autenticação**: O serviço suporta SMTP com e sem autenticação (útil para smtp4dev em desenvolvimento).
- **Validação de E-mail Único**: Validação feita na aplicação (não há índice único global no banco), garantindo flexibilidade para cenários multi-tenant.

---

Data de Criação: 2025-10-16  
Versão: 1.0  
Status: Pronto para Implementação
