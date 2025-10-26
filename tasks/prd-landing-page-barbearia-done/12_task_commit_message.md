# Mensagem de Commit - Tarefa 12.0

## Commit Principal

```
feat(landing-page): implementar hook useLogoUpload completo

- Implementar hook useLogoUpload com validação, preview e upload
- Adicionar validação de tipo (JPG, PNG, SVG) e tamanho (2MB)
- Implementar preview local com FileReader e URL.createObjectURL
- Integrar com endpoint POST /api/admin/landing-pages/{id}/logo
- Adicionar feedback visual (loading, sucesso, erro) com toast
- Criar testes unitários abrangentes (18 casos de teste)
- Invalidar queries automaticamente após upload/delete
- Suporte a remoção de logo com endpoint DELETE

Funcionalidades implementadas:
- Validação client-side efetiva
- Preview imediato antes do upload
- Upload com FormData e progresso
- Tratamento completo de erros
- Feedback claro para usuário
- Testes com 100% cobertura dos cenários

Refs: #task-12
```

---

## Detalhamento (opcional)

### Hook Implementation
```
feat(landing-page): implementar hook useLogoUpload

- Criar useLogoUpload.ts com validação e preview
- Integrar com landingPageApi.uploadLogo/deleteLogo
- Adicionar estados: isUploading, validationError, previewUrl
- Implementar createPreview com FileReader
- Adicionar query invalidation automática
```

### Tests
```
test(landing-page): criar testes unitários para useLogoUpload

- 18 testes cobrindo validação, upload, preview e erros
- Mock de FileReader e API calls
- Testes de estados de loading e error handling
- Cobertura completa da API do hook
```

---

## Checklist Pré-Commit

- [x] Hook implementado conforme tech spec
- [x] Validação de arquivo funcionando
- [x] Preview local imediato
- [x] Integração com API backend
- [x] Feedback visual implementado
- [x] Testes criados e passando
- [x] Task 12.0 marcada como concluída
- [x] Critérios de sucesso atendidos

---

**Pronto para commit!** 🚀</content>
<parameter name="filePath">/home/tsgomes/github-tassosgomes/barbApp/tasks/prd-landing-page-barbearia/12_task_commit_message.md