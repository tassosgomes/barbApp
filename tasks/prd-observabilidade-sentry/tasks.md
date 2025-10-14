# Implementação Observabilidade Sentry - Resumo de Tarefas

## Tarefas

- [ ] 1.0 Backend: Bootstrap Sentry SDK e configuração
- [ ] 2.0 Backend: Captura global e enriquecimento de escopo
- [ ] 3.0 Backend: Scrub de PII e política de amostragem
- [ ] 4.0 Frontend: Inicialização @sentry/react no bootstrap
- [ ] 5.0 Frontend: Upload de sourcemaps com @sentry/vite-plugin (CI)
- [ ] 6.0 Releases e Ambientes: Convenções e variáveis de CI/CD
- [ ] 7.0 Alertas: Regras e canais (Sentry)
- [ ] 8.0 Governança e Segurança: Segredos, RBAC e privacidade
- [ ] 9.0 Testes e Validação: Backend + Frontend
- [ ] 10.0 Dashboards e Métricas Base de Estabilidade

## Análise de Paralelização

- Caminho crítico: 6.0 → 5.0 → 9.0; 1.0 → 2.0 → 9.0; 4.0 → 9.0
- Paralelizáveis imediatamente: 1.0, 4.0, 6.0, 8.0
- Dependências chave:
  - 5.0 depende de 6.0 (release/env e auth para upload)
  - 7.0 depende de 1.0, 4.0 e 6.0 (eventos + convenções)
  - 9.0 depende de 1.0, 2.0, 4.0, 5.0 (captura e sourcemaps)
  - 10.0 depende de 1.0, 4.0 e 6.0 (eventos e releases)

## Referências

- PRD: tasks/prd-observabilidade-sentry/prd.md
- Tech Spec: tasks/prd-observabilidade-sentry/techspec.md

