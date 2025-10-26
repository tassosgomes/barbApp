# 🚀 Guia de Início Rápido - Correção Bugs Landing Page

**Para desenvolvedores que vão trabalhar na correção**

---

## 📦 Documentos Criados

1. **BUGS_REPORT.md** 📝
   - Relatório detalhado com evidências
   - Análise de causa raiz
   - Dependências entre bugs

2. **CORRECTION_TASKS.md** 📋
   - 18 tasks organizadas em 4 fases
   - Subtarefas detalhadas
   - Critérios de aceitação

3. **EXECUTIVE_SUMMARY.md** 📊
   - Resumo executivo para gestão
   - Top 3 bugs críticos
   - Recomendações de priorização

4. **CHECKLIST.md** ✅
   - Checklist visual de progresso
   - Acompanhamento por fase
   - Marcos e cronograma

5. **Este arquivo** 🚀
   - Guia de início rápido
   - Comandos essenciais
   - Troubleshooting

---

## ⚡ Começando AGORA - Fase 1

### Pré-requisitos

```bash
# Backend rodando
cd backend
dotnet run --project src/BarbApp.API/
# http://localhost:5070

# Frontend rodando
cd barbapp-admin
npm run dev
# http://localhost:3000
```

### Credenciais de Teste

```
Código Barbearia: CEB4XAR7
Email: luiz.gomes@gmail.com
Senha: _y4#gA$ZlJQG
```

### Acessar Landing Page Editor

```
1. Login: http://localhost:3000/CEB4XAR7/login
2. Navegar: Menu > Landing Page
3. URL: http://localhost:3000/CEB4XAR7/landing-page
```

---

## 🔧 Task 34: Corrigir Bug #5 (Erro 400 ao Salvar)

### Passo 1: Reproduzir o Bug

```bash
# 1. Acessar landing page editor (já logado)
# 2. Editar campo "Sobre a Barbearia"
# 3. Clicar "Salvar Alterações"
# 4. Observar erro 400
```

### Passo 2: Capturar Payload

```javascript
// Abrir DevTools > Network > Filter: Fetch/XHR
// Procurar request: PUT /admin/landing-pages/:id
// Copiar payload da aba "Payload" ou "Request"

// Exemplo do payload que deve ser enviado:
{
  "templateId": 3,
  "logoUrl": "...",
  "sobre": "Texto editado...",
  "horarioFuncionamento": "...",
  "whatsapp": "...",
  "instagram": "...",
  "facebook": "...",
  "services": []  // Pode estar vazio
}
```

### Passo 3: Analisar Backend

```bash
# Terminal do backend deve mostrar erro detalhado
# Procurar linha com "One or more validation errors occurred"

# Arquivos a verificar:
backend/src/BarbApp.Application/UseCases/LandingPages/UpdateLandingPage/
  - UpdateLandingPageCommand.cs  # Ver validações
  - UpdateLandingPageHandler.cs  # Ver lógica

backend/src/BarbApp.API/Controllers/LandingPagesController.cs
  - Método PUT
```

### Possíveis Causas

1. **Campo obrigatório faltando**
   ```csharp
   // No Command ou Validator
   [Required]
   public string CampoObrigatorio { get; set; }
   ```

2. **Tipo de dados incorreto**
   ```typescript
   // Frontend envia string, backend espera int
   templateId: "3"  // ❌ Errado
   templateId: 3    // ✅ Correto
   ```

3. **Array vazio rejeitado**
   ```csharp
   // Se validator exige ao menos 1 item
   [MinLength(1)]
   public List<ServiceDto> Services { get; set; }
   ```

4. **Validação de relacionamento**
   ```csharp
   // Handler valida se barbeariaId do token = barbeariaId da landing page
   if (landingPage.BarbeariaId != barbeariaIdFromToken)
       return ValidationError();
   ```

### Passo 4: Corrigir

**Opção A: Corrigir Backend (se validação estiver errada)**
```csharp
// Remover validação desnecessária ou ajustar
public class UpdateLandingPageCommandValidator : AbstractValidator<UpdateLandingPageCommand>
{
    public UpdateLandingPageCommandValidator()
    {
        // Tornar campo opcional
        RuleFor(x => x.Services).NotNull(); // Permite array vazio
    }
}
```

**Opção B: Corrigir Frontend (se payload estiver incorreto)**
```typescript
// Ajustar payload enviado
const payload = {
  templateId: Number(formData.templateId), // Converter para número
  logoUrl: formData.logoUrl || null,       // Enviar null se vazio
  sobre: formData.sobre || "",             // Enviar string vazia
  // ...
  services: formData.services || []        // Enviar array vazio
};
```

### Passo 5: Testar

```bash
# 1. Recompilar backend (se mudou C#)
cd backend
dotnet build
dotnet run --project src/BarbApp.API/

# 2. Testar no frontend
# 3. Editar campo e salvar
# 4. Verificar: Network > Status 200 OK
# 5. Verificar: Banco de dados atualizado
```

---

## 🖼️ Task 35: Corrigir Bug #4 (Upload de Logo)

### Passo 1: Preparar Arquivo de Teste

```bash
# Criar ou baixar imagem de teste
# Requisitos:
# - Formato: PNG, JPG, SVG ou WebP
# - Tamanho: < 2 MB
# - Dimensões recomendadas: 300x300px
```

### Passo 2: Tentar Upload

```bash
# 1. Na landing page editor
# 2. Seção "Logo da Barbearia"
# 3. Clicar "Fazer Upload"
# 4. Selecionar arquivo
# 5. Observar erro (se houver)
```

### Passo 3: Verificar Endpoint Backend

```csharp
// backend/src/BarbApp.API/Controllers/LandingPagesController.cs

[HttpPost("{id}/logo")]
[Consumes("multipart/form-data")]
public async Task<IActionResult> UploadLogo(
    Guid id,
    IFormFile file)
{
    // Verificar se método existe
    // Verificar se aceita multipart/form-data
    // Verificar limite de tamanho
}
```

### Possíveis Problemas

1. **Endpoint não existe**
   - Criar endpoint de upload

2. **Multipart não configurado**
   ```csharp
   [Consumes("multipart/form-data")]
   ```

3. **Storage não configurado**
   ```csharp
   // Precisa de serviço de storage (local ou S3)
   private readonly IFileStorageService _storage;
   
   var url = await _storage.UploadAsync(file, "logos");
   ```

4. **Limite de tamanho**
   ```csharp
   // Program.cs ou Startup.cs
   builder.Services.Configure<FormOptions>(options =>
   {
       options.MultipartBodyLengthLimit = 2 * 1024 * 1024; // 2 MB
   });
   ```

### Passo 4: Verificar Storage

```bash
# Se storage local
backend/uploads/logos/  # Verificar se pasta existe

# Se storage S3
# Verificar credenciais AWS em appsettings.json
```

---

## 🌐 Fase 2: Landing Page Pública (Depois da Fase 1)

### Setup Rápido do barbapp-public

```bash
# 1. Criar projeto
cd barbApp
npm create vite@latest barbapp-public -- --template react-ts
cd barbapp-public

# 2. Instalar dependências
npm install
npm install react-router-dom @tanstack/react-query axios lucide-react
npm install -D tailwindcss postcss autoprefixer

# 3. Inicializar Tailwind
npx tailwindcss init -p

# 4. Configurar Tailwind (tailwind.config.js)
# Ver Task 36 em CORRECTION_TASKS.md

# 5. Criar .env
VITE_API_URL=http://localhost:5070/api

# 6. Rodar
npm run dev
# http://localhost:3001
```

### Estrutura de Pastas

```bash
barbapp-public/
├── src/
│   ├── templates/        # Template1.tsx, Template2.tsx, ...
│   ├── components/       # ServiceCard, WhatsAppButton
│   ├── hooks/            # useLandingPageData
│   ├── types/            # landing-page.types.ts
│   ├── pages/            # LandingPage.tsx
│   ├── api/              # client.ts, landingPageApi.ts
│   ├── App.tsx
│   └── main.tsx
├── public/
├── .env
├── package.json
└── vite.config.ts
```

---

## 🔍 Troubleshooting Comum

### Erro: "Sessão expirada" ao acessar /barbearia/:codigo

**Causa**: Rota pública não implementada  
**Solução**: Completar Fase 2 (Tasks 36-42)

---

### Erro 400 ao salvar landing page

**Causa**: Validação no backend rejeitando payload  
**Solução**: Task 34 - Comparar payload com DTO esperado

---

### Upload de logo não funciona

**Causa**: Endpoint ou storage não configurado  
**Solução**: Task 35 - Verificar endpoint e storage

---

### Frontend não conecta com backend

```bash
# Verificar se backend está rodando
curl http://localhost:5070/api/health

# Verificar CORS no backend (Program.cs)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

---

### TypeScript errors no frontend

```bash
# Limpar cache e reinstalar
rm -rf node_modules package-lock.json
npm install

# Verificar tsconfig.json
# Verificar versões das dependências
```

---

## 📞 Ajuda e Suporte

### Dúvidas sobre as Tasks?
- Consultar **CORRECTION_TASKS.md** para detalhes completos
- Cada task tem subtarefas e critérios de aceitação

### Dúvidas sobre os Bugs?
- Consultar **BUGS_REPORT.md** para análise detalhada
- Evidências e causa raiz documentadas

### Precisa de Visão Executiva?
- Consultar **EXECUTIVE_SUMMARY.md**
- Recomendações de priorização

---

## ✅ Checklist Antes de Começar

- [ ] Backend rodando (http://localhost:5070)
- [ ] Frontend rodando (http://localhost:3000)
- [ ] Credenciais de teste funcionando
- [ ] DevTools aberto (Network + Console)
- [ ] Git branch criada (ex: `fix/landing-page-bugs`)
- [ ] Documentos lidos:
  - [ ] BUGS_REPORT.md
  - [ ] CORRECTION_TASKS.md
  - [ ] EXECUTIVE_SUMMARY.md
  - [ ] CHECKLIST.md
  - [ ] Este guia (START_HERE.md)

---

## 🎯 Próxima Ação

**COMEÇAR AGORA**: Task 34.0 - Corrigir Bug #5 (Erro 400)

1. Reproduzir bug
2. Capturar payload
3. Analisar backend
4. Corrigir
5. Testar
6. ✅ Marcar no CHECKLIST.md

---

**Boa sorte! 🚀**

*Se tiver dúvidas, consulte os outros documentos ou peça ajuda ao time.*
