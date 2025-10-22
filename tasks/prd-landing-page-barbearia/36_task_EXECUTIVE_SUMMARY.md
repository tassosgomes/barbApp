# Task 36 - Upload para Cloudflare R2: Resumo Executivo

## 🎯 Por que migrar para R2?

### Problema Atual (LocalLogoUploadService)

```
❌ Container reinicia → Logos são perdidos
❌ Docker Swarm com 3 replicas → Cada uma tem arquivos diferentes
❌ Backup manual → Sem redundância
❌ CDN? → Não tem
❌ Custo de manutenção → Alto (volumes, NFS, etc)
```

### Solução (Cloudflare R2)

```
✅ Persistência automática
✅ Funciona em clusters (3, 10, 100 containers)
✅ CDN global integrado
✅ 99.9% SLA
✅ Custo: ~$0.05/mês (100 barbearias)
✅ Zero egress fees (transferência grátis!)
```

## 💰 Comparação de Custos (mensal)

| Solução | Storage | Egress | CDN | Total/mês |
|---------|---------|--------|-----|-----------|
| **Local (Container)** | Grátis* | N/A | Não tem | Grátis* |
| **AWS S3 + CloudFront** | $0.45 | $10-50 | $3-10 | $13-60 |
| **Cloudflare R2** | $0.0005 | **$0** | **Incluído** | **$0.05** |

*Grátis mas com perdas de dados e sem escalabilidade

## 🏗️ Arquitetura Proposta

### Antes (Atual)

```
┌─────────────┐
│  Frontend   │
└──────┬──────┘
       │ POST /logo
       ▼
┌─────────────────┐
│  ASP.NET API    │
└──────┬──────────┘
       │
       ▼
┌─────────────────────────┐
│  /uploads/logos/        │
│  (Container Filesystem) │  ❌ Perdido ao reiniciar
└─────────────────────────┘
```

### Depois (R2)

```
┌─────────────┐
│  Frontend   │
└──────┬──────┘
       │ POST /logo
       ▼
┌─────────────────┐
│  ASP.NET API    │
│  + R2Storage    │
└──────┬──────────┘
       │ S3 API
       ▼
┌─────────────────────────┐
│  Cloudflare R2          │
│  barbapp-assets bucket  │  ✅ Persistente
│                         │
│  + CDN Global          │  ✅ Fast
│  assets.barbapp.com    │
└─────────────────────────┘
```

## 📋 Checklist de Implementação

### 1️⃣ Setup Cloudflare R2 (30 min)
- [ ] Criar conta Cloudflare (se não tiver)
- [ ] Criar bucket `barbapp-assets`
- [ ] Gerar Access Key ID + Secret
- [ ] Configurar custom domain: `assets.barbapp.com`
- [ ] Configurar CORS

### 2️⃣ Backend .NET (2-3h)
- [ ] Instalar `AWSSDK.S3` NuGet
- [ ] Criar `R2StorageService.cs`
- [ ] Criar `R2LogoUploadService.cs`
- [ ] Atualizar `appsettings.json`
- [ ] Atualizar DI em `Program.cs`

### 3️⃣ Testes (1-2h)
- [ ] Testes unitários `R2StorageService`
- [ ] Testes integração upload
- [ ] Testes E2E Playwright
- [ ] Teste manual em dev

### 4️⃣ Deploy (30 min)
- [ ] Adicionar secrets no Docker Swarm
- [ ] Deploy staging
- [ ] Validar produção
- [ ] Documentar

## 🚀 Plano de Execução

### Dia 1 - Manhã (4h)
- Setup R2 bucket
- Implementação backend
- Testes unitários

### Dia 1 - Tarde (2h)
- Testes integração
- Deploy staging
- Validação

**Total: 1 dia útil (6h)**

## 🔐 Segurança

### Secrets Management

**Desenvolvimento:**
```bash
# .env (não commitado)
R2Storage__AccessKeyId=abc123...
R2Storage__SecretAccessKey=xyz789...
```

**Produção:**
```bash
# Docker Swarm Secrets
docker secret create r2_access_key <key>
docker secret create r2_secret_key <secret>
```

### CORS Configuration

```json
{
  "AllowedOrigins": [
    "https://app.barbapp.com",
    "https://barbapp.com"
  ],
  "AllowedMethods": ["GET", "PUT", "POST"],
  "AllowedHeaders": ["*"],
  "MaxAgeSeconds": 3600
}
```

## 📊 Métricas de Sucesso

| Métrica | Atual | Meta com R2 |
|---------|-------|-------------|
| **Perda de dados** | Frequente (reiniciar = perda) | 0% |
| **Tempo de upload** | ~500ms | ~300ms (CDN) |
| **Disponibilidade** | 95% (depende container) | 99.9% (R2 SLA) |
| **Escalabilidade** | Não escala | Ilimitado |
| **Custo mensal** | N/A | $0.05 |

## 📚 Documentação a Criar

1. **`docs/cloudflare-r2-setup.md`**
   - Passo a passo criar bucket
   - Configurar custom domain
   - Gerar access keys

2. **`docs/migration-local-to-r2.md`**
   - Script migrar logos existentes
   - Rollback plan

3. **`backend/appsettings.R2.json`**
   - Exemplo de configuração

## ❓ FAQ

### Q: E se o R2 ficar fora do ar?
**A:** SLA de 99.9% = ~43 min/mês. Raro. Plano B: cache local temporário.

### Q: E os logos que já existem no filesystem?
**A:** Script de migração incluído na task. Upload batch para R2.

### Q: Frontend precisa mudar algo?
**A:** NÃO! Mudança é transparente. Mesma API, só a URL do logo muda.

### Q: Posso testar antes de colocar em produção?
**A:** SIM! Criar bucket separado `barbapp-assets-dev` para testes.

### Q: E se precisar voltar atrás?
**A:** Rollback simples: trocar DI para `LocalLogoUploadService` novamente.

## 🎬 Próximos Passos

1. **Aprovar task** (este documento)
2. **Agendar implementação** (1 dia)
3. **Criar bucket no Cloudflare**
4. **Implementar backend**
5. **Testar**
6. **Deploy staging**
7. **Deploy produção**

---

**Documento criado em:** 2025-10-22  
**Estimativa:** 6 horas (1 dia útil)  
**Prioridade:** Alta (bloqueia escalabilidade)  
**Impacto:** Alto (resolve persistência + performance + custo)
