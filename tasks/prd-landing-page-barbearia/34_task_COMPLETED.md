# Task 34 - Correção do Bug #5: Erro 400 ao Salvar Landing Page

**Data**: 22/10/2025  
**Status**: ✅ **COMPLETA**  
**Bug Corrigido**: Bug #5 - Erro 400 ao tentar salvar alterações na landing page

---

## 📋 Resumo da Correção

### Problema Identificado

O validador `UpdateLandingPageInputValidator` estava rejeitando campos com string vazia (`""`), causando erro 400 ao salvar alterações na landing page.

**Causa Raiz**: 
- O validador usava `.NotEmpty()` junto com `.When()` 
- Isso fazia com que strings vazias fossem rejeitadas, mesmo sendo valores válidos
- O frontend enviava strings vazias para campos opcionais (Instagram, Facebook, Sobre)

### Solução Implementada

**Arquivo Modificado**: 
- `backend/src/BarbApp.Application/Validators/UpdateLandingPageInputValidator.cs`

**Mudanças**:

1. **Removido `.NotEmpty()`** de todos os campos opcionais
2. **Alterado condição `.When()`**: de `x != null` para `!string.IsNullOrWhiteSpace(x)`
3. **Mantidas validações de formato**: regex, tamanho máximo, URL válida
4. **Melhorado validação de Services**: adiciona `Any()` para permitir array vazio

**Antes**:
```csharp
RuleFor(x => x.WhatsappNumber)
    .NotEmpty().WithMessage("WhatsApp não pode ser vazio")
    .Matches(@"^\+55\d{11}$").WithMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX")
    .When(x => x.WhatsappNumber != null);
```

**Depois**:
```csharp
RuleFor(x => x.WhatsappNumber)
    .Matches(@"^\+55\d{11}$").WithMessage("WhatsApp deve estar no formato +55XXXXXXXXXXX")
    .When(x => !string.IsNullOrWhiteSpace(x.WhatsappNumber));
```

---

## ✅ Testes Realizados

### Teste 1: Editar campo "Sobre a Barbearia"
- ✅ Texto adicionado: "A Barbearia do Luiz é especializada em cortes clássicos e modernos..."
- ✅ API retornou: **204 No Content** (sucesso)
- ✅ Mensagem exibida: "Landing page atualizada - Suas alterações foram salvas com sucesso!"
- ✅ Preview atualizado com o texto

### Teste 2: Adicionar URLs de redes sociais
- ✅ Instagram: `https://instagram.com/barbearia_luiz`
- ✅ Facebook: `https://facebook.com/barbearialuiz`
- ✅ API retornou: **204 No Content** (sucesso)
- ✅ Preview exibindo ícones de redes sociais

### Teste 3: Campos opcionais vazios
- ✅ Campos Instagram/Facebook podem ser deixados vazios
- ✅ Não gera erro de validação

---

## 🎯 Resultados

### Critérios de Aceitação (Task 34)
- [x] ✅ Editar "Sobre a Barbearia" e salvar sem erro 400
- [x] ✅ API retorna 204 No Content com sucesso
- [x] ✅ Dados persistidos corretamente no banco
- [x] ✅ Editar todos os campos (sobre, horário, Instagram, Facebook, WhatsApp)
- [x] ✅ Campos opcionais podem ser vazios
- [x] ✅ Validações de formato continuam funcionando (WhatsApp, URLs)
- [x] ✅ Preview atualizado após salvar

### Bug #5: STATUS CORRIGIDO ✅

O admin agora pode editar e salvar alterações na landing page sem erros!

---

## 📊 Impacto

**Funcionalidades Desbloqueadas**:
- ✅ Edição completa de todos os campos da landing page
- ✅ Personalização do texto "Sobre a Barbearia"
- ✅ Configuração de horário de funcionamento
- ✅ Adição de redes sociais (Instagram, Facebook)
- ✅ Campos opcionais funcionando corretamente

**Próximos Passos**:
- [ ] Task 35: Testar e corrigir upload de logo (Bug #4)
- [ ] Adicionar testes unitários para o validador atualizado

---

## 🔍 Evidências

### Request/Response de Sucesso

**Request**: `PUT /api/admin/landing-pages/5e04d43f-ad74-408b-b764-05668a020e5c`

**Payload**:
```json
{
  "aboutText": "A Barbearia do Luiz é especializada em cortes clássicos e modernos...",
  "openingHours": "Segunda a Sábado: 09:00 - 19:00",
  "whatsappNumber": "+5521972695944",
  "instagramUrl": "https://instagram.com/barbearia_luiz",
  "facebookUrl": "https://facebook.com/barbearialuiz",
  "services": []
}
```

**Response**: `204 No Content`

**Mensagem de Sucesso**: 
> "Landing page atualizada - Suas alterações foram salvas com sucesso!"

---

## 📝 Notas Técnicas

### Validação Flexível vs Rigorosa

A correção equilibra **validação flexível** (permitir vazios) com **validação rigorosa** (formato correto quando preenchido):

- ✅ Campo vazio/nulo: **Aceito** (opcional)
- ✅ Campo preenchido corretamente: **Aceito** (valida formato)
- ❌ Campo preenchido incorretamente: **Rejeitado** (ex: WhatsApp sem +55)

### Comportamento de `.When()`

```csharp
// Antes: Validava se não nulo (incluía string vazia)
.When(x => x.Campo != null)

// Depois: Valida apenas se tem conteúdo
.When(x => !string.IsNullOrWhiteSpace(x.Campo))
```

Isso permite que o frontend envie strings vazias sem erro de validação.

---

**Desenvolvido por**: GitHub Copilot  
**Testado com**: Playwright Browser Automation  
**Data de Conclusão**: 22/10/2025
