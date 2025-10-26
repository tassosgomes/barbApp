# Task 35.0 - Corrigir Upload de Logo (Bug #4)

**Status:** 🟡 IN PROGRESS  
**Fase:** 1 - Correções Críticas de Funcionalidade  
**Prioridade:** Alta  
**Estimativa:** 2-4 horas

## 📋 Objetivo

Corrigir o erro 400 (Bad Request) que ocorre ao fazer upload do logo da barbearia na landing page.

## 🐛 Problema Identificado

### Reprodução do Bug

1. ✅ Login realizado com sucesso (luiz.gomes@gmail.com)
2. ✅ Navegação para `/CEB4XAR7/landing-page`
3. ✅ Clique no botão "Fazer Upload" na seção "Logo da Barbearia"
4. ✅ Seleção de arquivo PNG (4.6KB, dentro do limite de 2MB)
5. ❌ **ERRO**: Request `POST /admin/landing-pages/{id}/logo` retorna **400 Bad Request**
6. ❌ Notificação exibida: "Erro no upload - Erro ao fazer upload do logo"

### Evidências

```
[LOG] API Request: POST /admin/landing-pages/5e04d43f-ad74-408b-b764-05668a020e5c/logo
[LOG] Full URL: http://localhost:5070/api/admin/landing-pages/5e04d43f-ad74-408b-b764-05668a020e5c/logo
[ERROR] Failed to load resource: the server responded with a status of 400 (Bad Request)
[ERROR] API Error Response: 400 POST /admin/landing-pages/5e04d43f-ad74-408b-b764-05668a020e5c/logo
```

### Arquivo de Teste Usado

- **Path:** `C:\home\tsgomes\github-tassosgomes\barbApp\teste_logo.png`
- **Size:** 4.6KB (muito abaixo do limite de 2MB)
- **Format:** PNG (formato aceito)
- **Source:** Ubuntu logo (`/usr/share/pixmaps/ubuntu-logo-text-dark.png`)

## 🔍 Análise Técnica

### Causa Raiz Identificada ✅

**Problema:** Nome do campo FormData incorreto no frontend

**Local:** `/barbapp-admin/src/services/api/landing-page.api.ts` (linha 71)

**Código com bug:**
```typescript
const formData = new FormData();
formData.append('logo', file); // ❌ ERRADO - backend espera 'file'
```

**Correção aplicada:**
```typescript
const formData = new FormData();
formData.append('file', file); // ✅ CORRETO - conforme esperado pelo backend
```

### Evidências da Investigação

1. **Teste com curl (SUCESSO):**
   - Upload via curl com `-F "file=@test_logo.png"` → **200 OK**
   - Logo salvo em: `/uploads/logos/{barbershopId}_{guid}.png`
   - Response: `{"logoUrl": "...", "message": "Logo atualizado com sucesso"}`

2. **Teste com Playwright (FALHA):**
   - Upload via componente React → **400 Bad Request**
   - FormData enviava campo `logo` ao invés de `file`

3. **Endpoint Backend:**
   ```csharp
   [HttpPost("{barbershopId:guid}/logo")]
   public async Task<IActionResult> UploadLogo(
       [FromRoute] Guid barbershopId,
       [FromForm] IFormFile file,  // ← Espera 'file', não 'logo'
       CancellationToken cancellationToken)
   ```

### Possíveis Causas do Erro 400

1. **Validação de IFormFile:**
   - Arquivo nulo ou vazio
   - Nome do campo no formulário não corresponde ao esperado
   - Content-Type incorreto

2. **Validação no Controller:**
   - `ModelState.IsValid` falhando
   - Validação customizada de formato/tamanho
   - GUID do barbershop inválido

3. **Validação no Service:**
   - LocalLogoUploadService pode estar rejeitando o arquivo
   - Problemas com ImageSharp ao processar PNG
   - Verificação de formato de arquivo

4. **Problemas de Upload:**
   - Limite de tamanho no ASP.NET Core (default: 28.6MB)
   - Configuração de CORS
   - Configuração de multipart/form-data

## 📝 Próximos Passos

### 1. Investigação (30 min)

- [ ] Verificar logs do backend para ver mensagem de erro específica
- [ ] Adicionar logging temporário no método `UploadLogo`
- [ ] Verificar se o arquivo está chegando corretamente (IFormFile não é null)
- [ ] Testar com arquivo ainda menor (1KB) e diferentes formatos

### 2. Análise do Código (30 min)

- [ ] Ler código completo do método `UploadLogo` (linhas 146-213)
- [ ] Verificar validações no controller
- [ ] Analisar método `LocalLogoUploadService.UploadLogoAsync`
- [ ] Verificar se há FluentValidation para upload de arquivo
- [ ] Conferir configuração de `IFormFile` no ASP.NET Core

### 3. Correção (1-2 horas)

Dependendo da causa:

**Se for validação:**
- Ajustar validações para aceitar arquivos PNG válidos
- Verificar configuração de Content-Type

**Se for configuração:**
- Ajustar limites de upload no Program.cs
- Configurar multipart/form-data corretamente

**Se for ImageSharp:**
- Verificar se biblioteca está instalada
- Testar processamento de PNG
- Adicionar tratamento de erro mais específico

### 4. Testes (30 min)

- [ ] Testar upload com PNG (formato atual)
- [ ] Testar upload com JPG
- [ ] Testar upload com SVG
- [ ] Testar upload com WebP
- [ ] Testar upload com arquivo > 2MB (deve rejeitar com mensagem clara)
- [ ] Testar upload sem arquivo (deve rejeitar com mensagem clara)
- [ ] Verificar se logo aparece no preview após upload bem-sucedido
- [ ] Verificar se logo é salvo corretamente no banco de dados

## 🎯 Critérios de Aceitação

- [ ] Upload de arquivo PNG (< 2MB) retorna 200 OK
- [ ] Logo é processado e redimensionado para 300x300px
- [ ] Logo aparece no preview da landing page
- [ ] URL do logo é salva no banco de dados
- [ ] Mensagem de sucesso é exibida ao usuário
- [ ] Erros retornam mensagens claras (tamanho, formato, etc.)
- [ ] Todos os formatos aceitos (JPG, PNG, SVG, WebP) funcionam

## 📚 Arquivos Relacionados

### Backend
- `/backend/src/BarbApp.API/Controllers/LandingPagesController.cs` (linha 146-213)
- `/backend/src/BarbApp.Application/Services/LocalLogoUploadService.cs`
- `/backend/src/BarbApp.Application/Services/ImageSharpProcessor.cs`
- `/backend/src/BarbApp.API/Program.cs` (configuração de upload)

### Frontend
- `/barbapp-admin/src/pages/LandingPage/LandingPageEditor.tsx`
- `/barbapp-admin/src/components/LandingPage/LogoUpload.tsx` (componente de upload)
- `/barbapp-admin/src/services/api.ts` (client HTTP)

## 🔗 Referências

- [Task 34 COMPLETED](./34_task_COMPLETED.md) - Bug #5 (validação)
- [BUGS_REPORT.md](./BUGS_REPORT.md) - Relatório completo dos 5 bugs
- [CORRECTION_TASKS.md](./CORRECTION_TASKS.md) - Plano de 18 tasks

## 📊 Progresso

- [x] Bug confirmado via Playwright
- [x] Arquivo de teste criado
- [x] Upload testado (falhou com 400)
- [x] Erro reproduzido de forma consistente
- [x] Teste com curl realizado (SUCESSO - backend OK)
- [x] Causa raiz identificada (nome do campo FormData)
- [x] Correção implementada
- [ ] Testes de aceitação passando
- [ ] Documentação atualizada
- [ ] Commit criado

---

**Última atualização:** 2025-10-22 16:46  
**Testado por:** Playwright Browser Automation  
**Arquivo de teste:** `C:\home\tsgomes\github-tassosgomes\barbApp\teste_logo.png` (4.6KB PNG)
