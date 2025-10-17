# FAQ - Perguntas Frequentes - Task 15

## ❓ Perguntas Gerais

### Por que criar automaticamente o Admin Barbearia?
Para reduzir fricção no onboarding e garantir que toda barbearia cadastrada já tenha acesso funcional ao sistema imediatamente. Evita etapas manuais propensas a erros.

### O Admin Barbearia pode usar uma senha diferente do e-mail da barbearia?
Não no MVP. O e-mail do Admin Barbearia é sempre o mesmo e-mail cadastrado na barbearia. A separação de usuários fica para versões futuras.

### Posso ter múltiplos Admin Barbearia por barbearia?
Não no MVP. A relação é **1:1** (1 barbearia = 1 Admin Barbearia). Suporte a múltiplos admins fica para versões futuras.

---

## 🔐 Segurança

### A senha enviada por e-mail é segura?
A senha gerada tem 12 caracteres com alta entropia (maiúsculas, minúsculas, números, símbolos) usando `RandomNumberGenerator` criptograficamente seguro. Porém, **enviar senha por e-mail não é ideal**. Recomendações:
1. Avisar o usuário para trocar a senha no primeiro acesso
2. Implementar fluxo de "esqueci minha senha" (futuro)
3. Implementar troca obrigatória no primeiro login (futuro)

### Como a senha é armazenada no banco?
Apenas o **hash BCrypt** da senha é armazenado. A senha nunca é salva em plain text. O custo do BCrypt (work factor) segue o padrão do projeto.

### O que acontece se o e-mail for interceptado?
Risco existe. Mitigações:
- SMTP com TLS/SSL habilitado
- Avisar usuário para trocar senha após primeiro acesso
- Implementar autenticação de dois fatores (2FA) em versões futuras

### E-mail único é garantido?
Sim. A validação é feita **na aplicação** (método `GetByEmailAsync`) antes de criar o Admin Barbearia. Conflito retorna **409 Conflict**.

---

## 📧 E-mail

### Por que não usar SendGrid, Mailgun ou AWS SES?
Para simplificar o MVP. O sistema suporta SMTP genérico, então migrar para provedores transacionais no futuro é trivial (apenas trocar configuração).

### Como testar envio de e-mail em desenvolvimento?
Use **smtp4dev** (Docker):
```bash
docker run -d -p 3000:80 -p 2525:25 rnwood/smtp4dev
```
Configure backend para `localhost:2525` e acesse UI em `http://localhost:3000`.

### E se o servidor SMTP estiver fora do ar?
O serviço tenta **3 vezes** com backoff exponencial (1s, 2s, 4s). Se falhar, a transação de cadastro é **revertida** (rollback) e o Admin Central recebe erro 500. A barbearia **não é criada**.

### Posso reenviar credenciais quantas vezes quiser?
Sim. Cada reenvio gera uma **nova senha** (a antiga fica inválida) e envia novo e-mail. Não há limite de reenvios no MVP.

### O template de e-mail pode ser personalizado?
Não no MVP. O template é fixo. Personalização (logo, cores, etc.) fica para versões futuras.

---

## 🔄 Transações e Rollback

### O que acontece se o e-mail falhar após criar a barbearia?
A transação é **revertida** (rollback). Nem a barbearia nem o Admin Barbearia são criados. Garantia de atomicidade.

### Como garantir que barbearia e Admin Barbearia são criados juntos?
Usando **UnitOfWork** (transação de banco). Ambas as operações estão dentro do mesmo escopo transacional.

### E se o rollback falhar?
O banco retorna erro e o sistema loga o problema. Inconsistência deve ser tratada manualmente (raro). Logs ajudam a identificar o problema.

---

## 🛠️ Implementação

### Por que validar e-mail único na aplicação e não no banco?
Para manter flexibilidade multi-tenant. O índice existente é **composto** `(email, barbearia_id)`, permitindo queries eficientes por barbearia. Adicionar índice único global poderia causar conflitos desnecessários.

### Posso usar `System.Net.Mail` em vez de MailKit?
Sim, mas **MailKit é recomendado** por ser mais moderno, robusto e cross-platform. `System.Net.Mail` tem limitações (ex: sem suporte a OAuth2).

### Como parametrizar a URL do frontend?
Via `AppSettings.FrontendUrl` no `appsettings.json`:
- **Dev**: `https://dev-barbapp.tasso.dev.br`
- **Prod**: `https://barbapp.tasso.dev.br`

### Preciso adicionar nova migration?
Não. O schema já tem a tabela `admin_barbearia_users` com os campos necessários. Apenas adicione o método `GetByEmailAsync` no repository.

### Como registrar os novos serviços no DI?
Adicione no `Program.cs`:
```csharp
builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IPasswordGenerator, SecurePasswordGenerator>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<ResendCredentialsUseCase>();
```

---

## 🧪 Testes

### Como testar sem enviar e-mails reais?
Use **mock** do `IEmailService` nos testes unitários:
```csharp
var emailServiceMock = new Mock<IEmailService>();
emailServiceMock
    .Setup(x => x.SendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

Ou use **smtp4dev** para testes de integração.

### Como simular falha de SMTP?
Pare o servidor SMTP ou configure host inválido. O serviço deve tentar 3 vezes e falhar com exceção.

### Posso testar o endpoint de reenvio sem autenticação?
Não. O endpoint requer **role `AdminCentral`**. Você precisa de um token JWT válido. Use o endpoint de login do Admin Central para obter o token.

---

## 🐛 Troubleshooting

### Erro: "Failed to send email after 3 attempts"
**Causas possíveis**:
1. SMTP server não está rodando
2. Porta ou host incorretos no `appsettings.json`
3. Credenciais inválidas (se autenticação for necessária)
4. Firewall bloqueando porta SMTP

**Solução**:
- Verificar logs do backend
- Testar conexão: `telnet localhost 2525`
- Verificar se smtp4dev está rodando: `docker ps | grep smtp4dev`

### Erro 409: "E-mail já existe"
**Causa**: E-mail já está cadastrado para outro Admin Barbearia.

**Solução**: Use outro e-mail ou verifique se é realmente um conflito legítimo.

### Barbearia foi criada mas Admin Barbearia não
**Causa**: Transação não foi revertida corretamente (bug).

**Solução temporária**: 
1. Criar manualmente o Admin Barbearia via SQL:
```sql
INSERT INTO admin_barbearia_users (admin_barbearia_user_id, barbearia_id, email, password_hash, name, is_active, created_at, updated_at)
VALUES (gen_random_uuid(), '{barbershop_id}', 'email@barbearia.com', '{bcrypt_hash}', 'Nome', true, NOW(), NOW());
```
2. Reenviar credenciais pelo endpoint.

### Reenvio de credenciais não funciona
**Causas possíveis**:
1. Endpoint não autorizado (401/403)
2. Barbearia não existe (404)
3. Admin Barbearia não existe (400)

**Solução**:
- Verificar token JWT (role `AdminCentral`)
- Verificar se barbearia existe: `SELECT * FROM barbershops WHERE barbershop_id = '{id}'`
- Verificar se Admin Barbearia existe: `SELECT * FROM admin_barbearia_users WHERE barbearia_id = '{id}'`

### E-mail não aparece no smtp4dev
**Causas possíveis**:
1. smtp4dev não está rodando
2. Backend conectando na porta errada
3. Erro na conexão SMTP (ver logs)

**Solução**:
- Acessar `http://localhost:3000` (UI do smtp4dev)
- Verificar logs do backend: procurar "Email sent successfully"
- Verificar configuração: porta deve ser **2525** (não 25 ou 587)

---

## 🚀 Produção

### Como configurar SMTP em produção?
1. Escolher provedor SMTP (Gmail, Outlook, servidor dedicado, etc.)
2. Obter credenciais (username/password ou API key)
3. Atualizar `appsettings.json` (ou usar variáveis de ambiente):
```json
{
  "SmtpSettings": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "UseSsl": true,
    "Username": "your-email@gmail.com",
    "Password": "app-password"
  }
}
```
4. **Importante**: Usar **variáveis de ambiente** ou **secrets manager** para credenciais.

### Como usar variáveis de ambiente?
No `appsettings.json`, use placeholders:
```json
{
  "SmtpSettings": {
    "Username": "${SMTP_USERNAME}",
    "Password": "${SMTP_PASSWORD}"
  }
}
```

No ambiente (Linux):
```bash
export SMTP_USERNAME="your-email@gmail.com"
export SMTP_PASSWORD="your-app-password"
```

Ou no Docker Compose:
```yaml
environment:
  - SMTP_USERNAME=your-email@gmail.com
  - SMTP_PASSWORD=your-app-password
```

### Gmail bloqueia SMTP?
Gmail pode bloquear apps "menos seguros". Solução:
1. Habilitar autenticação de 2 fatores
2. Gerar **App Password** específica para SMTP
3. Usar App Password no lugar da senha real

### Quantos e-mails posso enviar?
Depende do provedor:
- **Gmail**: ~500/dia (conta gratuita)
- **Outlook**: ~300/dia (conta gratuita)
- **Provedores transacionais** (SendGrid, SES): milhares/mês (planos pagos)

Para produção com alto volume, considere migrar para provedor transacional.

---

## 📊 Performance

### Envio de e-mail deixa o cadastro lento?
Pode adicionar 1-3 segundos. Se impactar UX, considerar:
1. Processamento assíncrono com fila (RabbitMQ, Azure Queue, etc.)
2. Aumentar timeout do frontend
3. Retornar resposta antes de enviar e-mail (sem garantia)

**MVP**: aceitar impacto (< 5s), otimizar em versões futuras.

### Retry de e-mail impacta performance?
Sim, mas apenas em caso de falha. Retry adiciona:
- Tentativa 2: +1s
- Tentativa 3: +2s
- Tentativa 4: +4s
Total: até ~7s adicionais (raro).

---

## 🔮 Futuro (Fora do MVP)

### Quando teremos "esqueci minha senha"?
Planejado para Fase 2. Requer:
- Token de reset temporário
- Endpoint de solicitação de reset
- Página de definição de nova senha
- E-mail com link de reset

### Quando teremos troca obrigatória de senha?
Planejado para versão futura. Requer:
- Flag `must_change_password` no banco
- Middleware/interceptor para forçar troca
- Página de troca de senha

### Quando teremos 2FA (autenticação de dois fatores)?
Planejado para versões futuras. Requer:
- TOTP (Time-Based One-Time Password)
- QR Code para setup
- Validação de código 2FA no login

### Quando teremos múltiplos Admin Barbearia?
Planejado para versões futuras. Requer:
- Gestão de permissões (quem pode adicionar/remover admins)
- UI para convidar novos admins
- E-mail de convite com link de ativação

---

## 📚 Referências

### Documentação
- **MailKit**: https://github.com/jstedfast/MailKit
- **smtp4dev**: https://github.com/rnwood/smtp4dev
- **BCrypt.Net**: https://github.com/BcryptNet/bcrypt.net

### Boas Práticas
- **OWASP Password Storage**: https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html
- **Email Security Best Practices**: https://www.owasp.org/index.php/Email_Security

### Tutoriais
- **MailKit with ASP.NET Core**: https://kenhaggerty.com/articles/article/aspnet-core-22-smtp-emailsender-implementation
- **BCrypt in .NET**: https://www.c-sharpcorner.com/article/hashing-passwords-in-net-core-with-bcrypt/

---

**Última Atualização**: 2025-10-16  
**Task**: #15 - Onboarding Automático do Admin da Barbearia

---

💡 **Tem outra pergunta?** Adicione aqui ou abra uma issue no repositório!
