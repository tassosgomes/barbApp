# PRD - Onboarding Automático do Admin da Barbearia

## Visão Geral

Automatizar a criação de credenciais de acesso para o Admin da Barbearia no momento do cadastro da barbearia pelo Admin Central, enviando as credenciais por e-mail e permitindo acesso imediato ao sistema. Esta funcionalidade elimina a necessidade de criação manual de usuários e garante que cada barbearia tenha seu administrador configurado desde o início.

Valor para o negócio: reduzir fricção no onboarding de novas barbearias, garantir que toda barbearia cadastrada já tenha acesso ao sistema, e automatizar processos manuais que são propensos a erros.

## Objetivos

- Sucesso do usuário
  - Admin Central cadastra barbearia sem precisar criar manualmente o usuário Admin Barbearia
  - Admin Barbearia recebe credenciais de acesso imediatamente após cadastro
  - Admin Barbearia consegue fazer login no sistema em < 2 minutos após cadastro
  - Admin Central pode reenviar credenciais em caso de perda/necessidade
- Métricas de produto
  - Taxa de sucesso no envio de e-mails = 100%
  - Taxa de erro no cadastro de barbearias < 2%
  - Tempo médio de onboarding (cadastro → primeiro login) < 5 minutos
- Objetivos de negócio
  - Automatizar completamente o onboarding de Admin Barbearia
  - Reduzir suporte relacionado a credenciais de acesso
  - Garantir que toda barbearia cadastrada tenha acesso funcional ao sistema

## Histórias de Usuário

Personas:
- **Admin Central**: usuário responsável por cadastrar e gerenciar barbearias no sistema
- **Admin Barbearia**: proprietário/gerente da barbearia que precisa acessar o sistema para gerenciar sua operação

Principais histórias:
- Como Admin Central, quero que ao cadastrar uma barbearia seja automaticamente criado um usuário Admin Barbearia para que a barbearia tenha acesso imediato ao sistema.
- Como Admin Central, quero que as credenciais sejam enviadas por e-mail para o email cadastrado da barbearia para que o Admin Barbearia possa acessar o sistema.
- Como Admin Central, quero poder reenviar as credenciais de acesso em caso de perda ou necessidade para dar suporte ao Admin Barbearia.
- Como Admin Barbearia, quero receber um e-mail com minhas credenciais após o cadastro para que eu possa acessar o sistema imediatamente.
- Como Admin Barbearia, quero fazer login usando o e-mail da minha barbearia e a senha recebida para começar a usar o sistema.

## Funcionalidades Principais

### 1) Criação Automática de Admin Barbearia no Cadastro

O que faz: ao cadastrar uma barbearia, o sistema cria automaticamente um usuário com role `AdminBarbearia` e envia as credenciais por e-mail.

Por que é importante: elimina etapa manual e garante que toda barbearia tenha acesso desde o início.

Requisitos funcionais:
1.1. Ao criar barbearia com sucesso (POST `/api/barbershops`), o sistema deve automaticamente criar um usuário Admin Barbearia vinculado.
1.2. O e-mail do Admin Barbearia deve ser o mesmo e-mail cadastrado na barbearia.
1.3. A senha deve ser gerada automaticamente com 12 caracteres (letras maiúsculas, minúsculas, números e símbolos).
1.4. O usuário deve ser criado com status `isActive = true` e role `AdminBarbearia`.
1.5. O sistema deve validar unicidade de e-mail em todo o sistema (não apenas por barbearia).
1.6. Após criação bem-sucedida, o sistema deve enviar e-mail com as credenciais.
1.7. Se o envio de e-mail falhar, a transação de cadastro deve ser revertida (rollback) para garantir consistência.
1.8. A UI deve exibir mensagem de sucesso informando que as credenciais foram enviadas.

### 2) Serviço de Envio de E-mail (SMTP)

O que faz: envia e-mails transacionais do sistema usando servidor SMTP.

Por que é importante: habilita comunicação automatizada com usuários.

Requisitos funcionais:
2.1. Implementar serviço de e-mail usando SMTP (localhost:587 para MVP).
2.2. Configurações SMTP devem estar em `appsettings.json` (host, porta, credenciais, remetente).
2.3. Suportar templates de e-mail em HTML básico.
2.4. Template de boas-vindas deve incluir: nome da barbearia, e-mail de acesso, senha gerada, link para login.
2.5. Logs de sucesso/falha no envio de e-mails devem ser registrados.
2.6. Implementar retry automático em caso de falha temporária (até 3 tentativas com backoff exponencial).
2.7. Erros permanentes de e-mail devem ser logados e a transação deve falhar.

### 3) Reenvio de Credenciais pelo Admin Central

O que faz: permite ao Admin Central reenviar credenciais de acesso para uma barbearia.

Por que é importante: dá suporte a casos de perda de credenciais ou necessidade de reset.

Requisitos funcionais:
3.1. Adicionar endpoint `POST /api/barbershops/{id}/resend-credentials` (restrito a Admin Central).
3.2. Ao acionar reenvio, o sistema deve gerar nova senha aleatória para o Admin Barbearia.
3.3. Atualizar `password_hash` do usuário Admin Barbearia no banco.
3.4. Enviar e-mail com as novas credenciais usando o mesmo template.
3.5. Retornar erro 404 se barbearia não existir.
3.6. Retornar erro 400 se barbearia não tiver Admin Barbearia vinculado (cenário de inconsistência).
3.7. Logar evento de reenvio de credenciais (auditoria).

### 4) UI - Botão de Reenvio de Credenciais (Admin Central)

O que faz: adiciona botão na listagem de barbearias para reenviar credenciais.

Por que é importante: facilita suporte e gestão pelo Admin Central.

Requisitos funcionais:
4.1. Adicionar botão "Reenviar Credenciais" na tabela de barbearias (junto com ações de editar/desativar).
4.2. Ao clicar, exibir modal de confirmação com aviso de que nova senha será gerada e enviada.
4.3. Após confirmação, chamar endpoint de reenvio.
4.4. Exibir toast de sucesso: "Novas credenciais enviadas com sucesso para [email]".
4.5. Exibir toast de erro em caso de falha.
4.6. Desabilitar botão durante processamento (loading state).

### 5) UI - Feedback no Cadastro de Barbearia

O que faz: atualiza mensagem de sucesso no cadastro para informar sobre envio de credenciais.

Por que é importante: deixa claro ao Admin Central que a barbearia já está pronta para uso.

Requisitos funcionais:
5.1. Atualizar toast de sucesso no cadastro de barbearia para incluir informação sobre envio de credenciais.
5.2. Mensagem sugerida: "Barbearia cadastrada com sucesso! Credenciais de acesso enviadas para [email]."
5.3. Tela de sucesso (após cadastro) deve mencionar envio de e-mail.

### 6) Autenticação do Admin Barbearia

O que faz: permite login do Admin Barbearia usando e-mail da barbearia e senha recebida.

Por que é importante: habilita acesso ao sistema pelo Admin Barbearia.

Requisitos funcionais:
6.1. Endpoint de autenticação já existente deve suportar login de Admin Barbearia.
6.2. Validar e-mail e senha contra tabela `admin_barbearia_users`.
6.3. Retornar JWT com role `AdminBarbearia` e `barbearia_id` no contexto.
6.4. Verificar campo `isActive = true` antes de autenticar.

## Experiência do Usuário

### Fluxo Completo (Happy Path):

**Admin Central cadastra barbearia:**
1. Acessa tela de Nova Barbearia
2. Preenche formulário (incluindo e-mail da barbearia)
3. Clica em "Salvar"
4. Sistema cria barbearia + Admin Barbearia + envia e-mail
5. Exibe mensagem: "Barbearia cadastrada com sucesso! Credenciais enviadas para email@barbearia.com"
6. Volta para listagem

**Admin Barbearia recebe acesso:**
1. Recebe e-mail com assunto "Bem-vindo ao BarbApp"
2. E-mail contém: nome da barbearia, e-mail de acesso, senha, link para login
3. Acessa link de login
4. Faz login com e-mail e senha recebidos
5. Acessa painel da barbearia

**Admin Central reenvia credenciais (se necessário):**
1. Acessa listagem de barbearias
2. Localiza barbearia
3. Clica em "Reenviar Credenciais"
4. Confirma ação no modal
5. Sistema gera nova senha e reenvia e-mail
6. Exibe mensagem de sucesso

### Template de E-mail (Estrutura):

```
Assunto: Bem-vindo ao BarbApp

Olá!

A barbearia [Nome da Barbearia] foi cadastrada com sucesso no BarbApp!

Você pode acessar o sistema de gestão com as credenciais abaixo:

E-mail: [email]
Senha: [senha]

Acesse: [URL do sistema]

Por questões de segurança, recomendamos alterar sua senha após o primeiro acesso.

Equipe BarbApp
```

## Restrições Técnicas de Alto Nível

- Segurança:
  - Senha gerada deve ter entropia suficiente (12 caracteres, alfanumérico + símbolos)
  - Armazenar apenas hash da senha no banco (BCrypt ou similar)
  - E-mail deve ser único em todo o sistema
  - Conexão SMTP deve ser segura (TLS/SSL)
- Transações:
  - Cadastro de barbearia + criação de usuário + envio de e-mail devem ser atômicos
  - Falha em qualquer etapa deve reverter transação completa
- Performance:
  - Envio de e-mail não deve bloquear resposta da API (considerar fila assíncrona se necessário)
  - Tempo total de cadastro < 5 segundos
- Observabilidade:
  - Logar criação de usuário Admin Barbearia
  - Logar tentativas e resultados de envio de e-mail
  - Logar reenvios de credenciais
- LGPD:
  - Não logar senha em plain text
  - E-mails devem ser enviados apenas para destinatário correto
  - Logs não devem conter dados sensíveis (PII)

## Não-Objetivos (Fora de Escopo)

- Fluxo de troca de senha pelo Admin Barbearia (MVP usa senha enviada por e-mail)
- Página de "esqueci minha senha" (futuro)
- Validação de e-mail (confirmação de recebimento/clique) no MVP
- Integração com serviços de e-mail transacional (SendGrid, SES, etc.) no MVP (usar SMTP simples)
- Personalização de templates de e-mail no MVP
- Notificações além de e-mail (SMS, push) no MVP
- Múltiplos Admin Barbearia por barbearia no MVP (1:1)
- Permissões granulares além das roles básicas no MVP

## Decisões Técnicas Definidas

✅ **Validação de e-mail único**: Validação feita na aplicação (não requer índice único global no banco), garantindo flexibilidade multi-tenant.

✅ **SMTP em todos os ambientes**: Usar SMTP direto (sem provedores externos no MVP). Configurar com suporte a autenticação opcional (username/password podem estar vazios).

✅ **URLs parametrizadas**:
- **Produção**: `https://barbapp.tasso.dev.br/login`
- **Desenvolvimento**: `https://dev-barbapp.tasso.dev.br/login`
- Configuradas via `AppSettings.FrontendUrl` no `appsettings.json`

✅ **SMTP Development**: smtp4dev (porta 2525, sem autenticação) - https://github.com/rnwood/smtp4dev

✅ **Retry de e-mail**: 3 tentativas com backoff exponencial (1s, 2s, 4s)

## Questões em Aberto

- Política de expiração de senhas: devemos forçar troca após X dias? (Fora do MVP, mas considerar na arquitetura)
- E-mail inválido/inexistente: como tratar? (MVP: falha na transação; futuro: validar antes?)
- Formato de log de auditoria: padronizar eventos de criação/reenvio de credenciais?

---

Data de Criação: 2025-10-16  
Versão: 1.0 (MVP)  
Status: Para Revisão
