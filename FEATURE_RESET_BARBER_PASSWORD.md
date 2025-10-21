# Feature: Reset de Senha de Barbeiros

**Branch:** `feature/reset-barber-password`  
**Data:** 20 de Outubro de 2025  
**Status:** ✅ Implementado e testado

---

## 📋 Resumo

Implementação da funcionalidade que permite ao **Admin Barbearia** redefinir a senha de seus barbeiros através da interface administrativa. O sistema gera automaticamente uma nova senha segura e a envia por email para o barbeiro.

---

## 🎯 Objetivos Alcançados

✅ Admin Barbearia pode resetar senha de barbeiros da sua barbearia  
✅ Nova senha é gerada automaticamente de forma segura  
✅ Email é enviado com a nova senha e instruções  
✅ Validação de isolamento multi-tenant (segurança)  
✅ Interface amigável com modal de confirmação  
✅ Feedback visual adequado (toast messages)  
✅ Rollback automático em caso de falha no envio de email  

---

## 🏗️ Implementação

### Backend

#### 1. **Use Case** - `ResetBarberPasswordUseCase.cs`
- **Localização:** `backend/src/BarbApp.Application/UseCases/`
- **Responsabilidades:**
  - Validar existência do barbeiro
  - Validar isolamento multi-tenant (barbeiro pertence à barbearia do admin)
  - Gerar nova senha segura usando `IPasswordGenerator`
  - Fazer hash da senha usando `IPasswordHasher`
  - Atualizar senha no banco de dados
  - Enviar email com nova senha
  - Commit de transação apenas após email enviado com sucesso
  - Rollback automático em caso de falha

#### 2. **Interface** - `IResetBarberPasswordUseCase.cs`
- **Localização:** `backend/src/BarbApp.Application/Interfaces/`
- **Assinatura:** `Task ExecuteAsync(Guid barberId, CancellationToken cancellationToken)`

#### 3. **Endpoint** - `BarbersController.cs`
- **Rota:** `POST /api/barbers/{id}/reset-password`
- **Autorização:** `[Authorize(Roles = "AdminBarbearia")]`
- **Responses:**
  - `200 OK`: Senha redefinida e email enviado com sucesso
  - `401 Unauthorized`: Usuário não autenticado
  - `403 Forbidden`: Barbeiro não pertence à barbearia do admin
  - `404 Not Found`: Barbeiro não encontrado
  - `500 Internal Server Error`: Falha no envio de email

#### 4. **Dependency Injection** - `Program.cs`
```csharp
builder.Services.AddScoped<IResetBarberPasswordUseCase, ResetBarberPasswordUseCase>();
```

#### 5. **Email Template**
Email HTML estilizado com:
- Nome do barbeiro e barbearia
- Nova senha gerada (em código destacado)
- Avisos de segurança
- Instruções para trocar senha após primeiro login
- Contato para caso não tenha solicitado

---

### Frontend

#### 1. **Serviço** - `barbeiro.service.ts`
```typescript
resetPassword: async (id: string): Promise<void> => {
  await api.post(`/barbers/${id}/reset-password`);
}
```

#### 2. **Componente** - `BarbeirosListPage.tsx`

**Adições:**
- Estado para modal de reset: `resetPasswordModalOpen`, `selectedBarberForReset`
- Mutation com `useMutation` do react-query
- Handler `handleResetPassword` para abrir modal
- Handler `confirmResetPassword` para confirmar ação
- Botão "Redefinir Senha" na coluna de ações (apenas para barbeiros ativos)
- Modal de confirmação com informações do barbeiro e email

**UX/UI:**
- Botão posicionado entre "Editar" e "Desativar/Reativar"
- Cor secundária (não destrutiva)
- Modal com aviso claro sobre email que receberá a senha
- Loading state durante requisição
- Toast de sucesso: "Senha redefinida com sucesso. Uma nova senha foi enviada para o email de {nome}."
- Toast de erro: "Erro ao redefinir senha. Não foi possível redefinir a senha. Tente novamente mais tarde."

---

## 🔒 Segurança

1. **Isolamento Multi-tenant:**
   - Validação automática via `ITenantContext`
   - Admin só pode resetar senhas de barbeiros da sua própria barbearia
   - Tentativas de acesso a outros tenants retornam 403 Forbidden

2. **Geração de Senha:**
   - Utiliza `IPasswordGenerator` (mesmo usado no onboarding)
   - Senhas seguras com caracteres especiais, números, maiúsculas e minúsculas

3. **Hash de Senha:**
   - Utiliza `IPasswordHasher` com bcrypt
   - Senhas nunca são armazenadas em texto plano

4. **Transação Segura:**
   - Commit apenas após envio bem-sucedido do email
   - Rollback automático em caso de falha
   - Logs de auditoria para todas as operações

5. **Autorização:**
   - Endpoint restrito a `AdminBarbearia`
   - JWT token validado automaticamente

---

## 📧 Email Template

### Assunto
```
BarbApp - Sua senha foi redefinida
```

### Conteúdo (HTML + Texto)
- Saudação personalizada com nome do barbeiro
- Nome da barbearia
- Email e nova senha em destaque
- Avisos de segurança em cards coloridos
- Instruções para trocar senha após primeiro login
- Aviso caso não tenha solicitado a alteração
- Footer com informações de contato

---

## 📝 Documentação

### Endpoints Atualizados
Arquivo: `backend/endpoints.md`

Adicionado:
```markdown
### `POST /api/barbers/{id}/reset-password`

- **Descrição**: Redefine a senha de um barbeiro gerando uma nova senha segura e enviando por e-mail...
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**: id (Guid, na rota)
- **Parâmetros de Saída**: Objeto JSON com mensagem de sucesso
```

---

## ✅ Testes Realizados

### Compilação
- ✅ Backend compila sem erros
- ✅ Frontend compila sem erros
- ✅ Sem warnings críticos

### Checklist de Funcionalidades
- ✅ Botão "Redefinir Senha" aparece apenas para barbeiros ativos
- ✅ Modal de confirmação exibe nome e email do barbeiro
- ✅ Requisição é feita corretamente para o backend
- ✅ Loading state funciona durante requisição
- ✅ Toast de sucesso aparece após conclusão
- ✅ Toast de erro aparece em caso de falha
- ✅ Isolamento multi-tenant funciona

---

## 🚀 Como Testar

### Pré-requisitos
1. Backend rodando: `cd backend && dotnet run --project src/BarbApp.API`
2. Frontend rodando: `cd barbapp-admin && npm run dev`
3. Login como Admin Barbearia

### Fluxo de Teste

1. **Acessar listagem de barbeiros:**
   ```
   http://localhost:3001/TEST1234/barbeiros
   ```

2. **Identificar barbeiro ativo:**
   - Verificar coluna de Status = "Ativo"

3. **Clicar em "Redefinir Senha":**
   - Modal deve abrir mostrando nome e email do barbeiro

4. **Confirmar ação:**
   - Clicar em "Confirmar"
   - Aguardar loading

5. **Verificar resultado:**
   - Toast de sucesso deve aparecer
   - Verificar inbox do email do barbeiro
   - Email deve conter nova senha

6. **Testar nova senha:**
   - Fazer logout
   - Tentar login como barbeiro com nova senha
   - Login deve funcionar

### Testes de Segurança

1. **Isolamento Multi-tenant:**
   - Tentar resetar senha de barbeiro de outra barbearia
   - Deve retornar 403 Forbidden

2. **Autorização:**
   - Tentar acessar endpoint sem autenticação
   - Deve retornar 401 Unauthorized

3. **Barbeiro Inativo:**
   - Botão não deve aparecer para barbeiros inativos

---

## 📊 Métricas

### Backend
- **Arquivos criados:** 2
  - `IResetBarberPasswordUseCase.cs`
  - `ResetBarberPasswordUseCase.cs`
- **Arquivos modificados:** 3
  - `BarbersController.cs`
  - `Program.cs`
  - `endpoints.md`

### Frontend
- **Arquivos modificados:** 2
  - `barbeiro.service.ts`
  - `BarbeirosListPage.tsx`

### Linhas de Código
- **Backend:** ~250 linhas (use case + controller + DI)
- **Frontend:** ~80 linhas (service + UI)
- **Total:** ~330 linhas

---

## 🔄 Próximos Passos (Melhorias Futuras)

### Testes Automatizados
- [ ] Testes de integração no backend
- [ ] Testes E2E no frontend (Playwright)
- [ ] Testes unitários do use case

### Features Adicionais
- [ ] Histórico de resets de senha
- [ ] Rate limiting (limite de resets por período)
- [ ] Opção de customizar template de email
- [ ] Link para trocar senha no próprio email
- [ ] Notificação para admin quando senha é resetada

### Observabilidade
- [ ] Métricas de quantidade de resets por barbearia
- [ ] Alertas para resets frequentes
- [ ] Dashboard com estatísticas

---

## 📞 Contato

Para dúvidas ou sugestões sobre esta feature, entre em contato com a equipe de desenvolvimento.

---

**Desenvolvido com ❤️ para o projeto BarbApp**
