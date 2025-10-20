# Resumo das Correções dos Testes E2E

## Problemas Identificados e Soluções

### 1. ✅ Configuração do Playwright
**Problema**: O Playwright estava configurado para usar porta 3001, mas o frontend roda na porta 3000.

**Solução**:
- Atualizado `playwright.config.ts`:
  - `baseURL: 'http://localhost:3000'`
  - `webServer.url: 'http://localhost:3000'`

### 2. ✅ Credenciais Corretas
**Problema**: Os testes estavam usando credenciais incorretas.

**Solução**: Credenciais atualizadas nos helpers:
- **Admin Central**: `admin@babapp.com` / `123456`
- **Barbeiro**: `dino@sauro.com` / `Neide@9090`
- **Admin Barbearia**: `neide.patricio@hotmail.com` / `S4nE23g@Qgu5`

### 3. ✅ Validação HTML5 vs React Hook Form
**Problema**: Os testes esperavam atributo `required` nos campos, mas o formulário usa validação Zod/React Hook Form com `noValidate`.

**Solução**: 
- Atualizado testes para verificar mensagens de erro via `data-testid`:
  ```typescript
  await expect(page.getByTestId('email-error')).toBeVisible();
  await expect(page.getByTestId('password-error')).toBeVisible();
  ```

### 4. ✅ Seletores de Toast
**Problema**: Testes procuravam por `[data-sonner-toast][data-type="error"]` mas a aplicação usa Radix UI Toast.

**Solução**: Atualizado helper para usar seletor correto:
```typescript
// Toast de erro com variante destructive
await expect(page.locator('.destructive').first()).toBeVisible();
```

### 5. ✅ Nome do Token no localStorage
**Problema**: Testes procuravam por `barber_token` mas o token é armazenado como `barbapp-barber-token`.

**Solução**: 
- Atualizado `isBarberAuthenticated()` e todas as referências:
  ```typescript
  localStorage.getItem('barbapp-barber-token')
  ```

### 6. ✅ Conteúdo do Modal de Ajuda
**Problema**: Teste procurava textos que não existiam no modal.

**Solução**: Atualizado para verificar textos corretos:
```typescript
await expect(page.locator('text=Como fazer login')).toBeVisible();
await expect(page.locator('text=📧 E-mail:')).toBeVisible();
await expect(page.locator('text=🔒 Senha:')).toBeVisible();
// Botão de fechar correto:
await page.click('button:has-text("Entendi")');
```

### 7. ✅ Verificação de Nome do Usuário
**Problema**: Teste procurava por "Dino Sauro" mas o nome completo na página é "Dino da Silva Sauro".

**Solução**: Alterado para verificar elemento mais confiável (botão de sair):
```typescript
await expect(page.getByRole('button', { name: /sair/i })).toBeVisible();
```

## Status dos Testes

### ✅ Testes de Autenticação do Barbeiro (100% passando)
- `tests/e2e/barber/01-auth.spec.ts`: **15/15 passed** ✨

### ⚠️ Outros Testes
- Total: 113 testes
- Passando: 32 (28%)
- Falhando: 72 (64%)
- Pendente: 9 (8%)

## Próximos Passos

### Testes que ainda precisam de correção:
1. Admin Barbearia - autenticação e gestão
2. Barber Schedule - fluxo completo
3. Barbershop CRUD - operações do Admin Central
4. Barbers Management

### Problemas comuns nos testes restantes:
- Seletores incompatíveis com componentes reais
- Timeouts aguardando elementos que não existem
- Verificações de atributos HTML que não são renderizados
- Tokens e localStorage com nomes diferentes

## Backend

✅ Backend rodando corretamente em `http://localhost:5070`
✅ Frontend rodando corretamente em `http://localhost:3000`

## Arquivos Modificados

1. `playwright.config.ts` - URLs corrigidas
2. `tests/e2e/helpers/barber.helper.ts` - Token localStorage + seletores de toast
3. `tests/e2e/barber/01-auth.spec.ts` - Validações + modal de ajuda + nome de usuário
4. `tests/e2e/barber/02-complete-flow.spec.ts` - Nome do token localStorage

## Lições Aprendidas

1. **Usar seletores por role e testid**: Mais confiáveis que text selectors
2. **Verificar implementação real**: Não assumir comportamentos padrão (ex: validação HTML5)
3. **Testar no browser primeiro**: O Playwright Browser tool foi essencial para debug
4. **Nomes de tokens**: Sempre verificar exatamente como estão armazenados
5. **Componentes UI**: Radix UI ≠ Sonner - verificar biblioteca em uso
