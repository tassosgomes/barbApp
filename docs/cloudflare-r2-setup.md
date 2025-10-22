# Cloudflare R2 Setup - Guia Passo a Passo

## 🎯 Objetivo

Criar bucket no Cloudflare R2 para armazenar logos das landing pages.

## 📝 Passo a Passo

### 1. Acessar Cloudflare Dashboard

1. Acesse: https://dash.cloudflare.com/
2. Faça login com sua conta
3. No menu lateral esquerdo, clique em **R2**

### 2. Criar Bucket

1. Clique em **"Create bucket"**
2. **Nome do bucket:** `barbapp-assets`
3. **Location:** Automatic (recomendado) ou escolha região próxima
4. Clique em **"Create bucket"**

### 3. Configurar CORS

1. No bucket `barbapp-assets`, clique na aba **"Settings"**
2. Role até a seção **"CORS Policy"**
3. Clique em **"Edit CORS Policy"**
4. Cole a seguinte configuração:

```json
[
  {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5070",
      "https://app.barbapp.com",
      "https://barbapp.com"
    ],
    "AllowedMethods": [
      "GET",
      "PUT",
      "POST",
      "DELETE",
      "HEAD"
    ],
    "AllowedHeaders": [
      "*"
    ],
    "ExposeHeaders": [
      "ETag"
    ],
    "MaxAgeSeconds": 3600
  }
]
```

5. Clique em **"Save"**

### 4. Gerar API Token (Access Keys)

1. No dashboard do R2, clique em **"Manage R2 API Tokens"**
2. Clique em **"Create API Token"**
3. **Token Name:** `barbapp-backend-upload`
4. **Permissions:**
   - ✅ Object Read & Write
   - ✅ (Selecione apenas estas permissões)
5. **TTL:** Never expire (ou escolha duração adequada)
6. **Bucket restrictions:** 
   - Selecione: `Apply to specific buckets only`
   - Escolha: `barbapp-assets`
7. Clique em **"Create API Token"**

### 5. Copiar Credenciais (⚠️ IMPORTANTE)

Após criar, você verá uma tela com as credenciais. **COPIE AGORA**, pois não poderá vê-las novamente!

Você precisa copiar:
- ✅ **Access Key ID** (exemplo: `abc123def456...`)
- ✅ **Secret Access Key** (exemplo: `xyz789abc123...`)
- ✅ **Endpoint URL** (exemplo: `https://1234567890abcdef.r2.cloudflarestorage.com`)

**Formato para salvar:**
```bash
# Cloudflare R2 Credentials - barbapp-assets
R2_ENDPOINT=https://1234567890abcdef.r2.cloudflarestorage.com
R2_BUCKET_NAME=barbapp-assets
R2_ACCESS_KEY_ID=abc123def456...
R2_SECRET_ACCESS_KEY=xyz789abc123...

# Public URL (será atualizado após configurar custom domain)
R2_PUBLIC_URL=https://pub-xyz.r2.dev/barbapp-assets
```

### 6. Obter Public URL

1. No bucket `barbapp-assets`, vá para **"Settings"**
2. Em **"Public Access"**, você verá a URL pública
3. Copie a URL (formato: `https://pub-xyz.r2.dev`)
4. A URL completa dos arquivos será: `https://pub-xyz.r2.dev/barbapp-assets/logos/...`

**OU** (Recomendado):

### 7. Configurar Custom Domain (Opcional mas Recomendado)

1. No bucket, vá para **"Settings"**
2. Role até **"Custom Domains"**
3. Clique em **"Connect Domain"**
4. Digite: `assets.barbapp.com`
5. Siga as instruções para adicionar CNAME no DNS:
   ```
   Type: CNAME
   Name: assets
   Target: barbapp-assets.r2.cloudflarestorage.com (ou conforme instruções)
   ```
6. Aguarde propagação DNS (~5 minutos)
7. URL final: `https://assets.barbapp.com/logos/...`

## ✅ Verificação

Após completar, você deve ter:

- [ ] Bucket `barbapp-assets` criado
- [ ] CORS configurado
- [ ] API Token criado
- [ ] Credenciais salvas em local seguro
- [ ] Public URL identificada
- [ ] (Opcional) Custom domain configurado

## 📝 Próximo Passo

Após obter as credenciais, informe:

```
R2_ENDPOINT=<seu-endpoint>
R2_BUCKET_NAME=barbapp-assets
R2_ACCESS_KEY_ID=<seu-access-key-id>
R2_SECRET_ACCESS_KEY=<seu-secret-access-key>
R2_PUBLIC_URL=<sua-url-publica>
```

Vou usá-las para configurar o backend!

---

## 🔐 Segurança

**⚠️ NUNCA:**
- Commite credenciais no Git
- Compartilhe Access Keys publicamente
- Use as mesmas keys em dev e produção

**✅ SEMPRE:**
- Guarde credenciais em gerenciador de senhas
- Use `.env` files (no `.gitignore`)
- Use Docker Secrets em produção
- Rotacione keys periodicamente

---

**Documento criado em:** 2025-10-22  
**Próxima task:** Implementação backend com as credenciais
