---
status: pending
parallelizable: true
blocked_by: ["6.0"]
---

<task_context>
<domain>infra/security</domain>
<type>documentation|configuration</type>
<scope>governance</scope>
<complexity>low</complexity>
<dependencies>external_apis</dependencies>
<unblocks>"7.0","9.0"</unblocks>
</task_context>

# Tarefa 8.0: Governança e Segurança - Segredos, RBAC e privacidade

## Visão Geral
Formalizar armazenamento seguro de DSNs/tokens, definir papéis de acesso no Sentry (RBAC) e políticas de privacidade/scrub para prevenir coleta de PII sensível.

## Requisitos
- Segredos via variáveis/secret manager (sem hardcode)
- Perfis de acesso no Sentry por função (dev/ops/visualização)
- Lista oficial de campos com scrub obrigatório
- Política de retenção alinhada ao plano do Sentry

## Subtarefas
- [ ] 8.1 Definir fonte/escopo de variáveis e responsáveis
- [ ] 8.2 Configurar RBAC nos projetos do Sentry
- [ ] 8.3 Publicar lista de PII a remover e validar com times
- [ ] 8.4 Registrar política de retenção e revisão periódica

## Sequenciamento
- Bloqueado por: 6.0
- Desbloqueia: 7.0, 9.0
- Paralelizável: Sim (documental e permissões)

## Detalhes de Implementação
- PRD: Governança de Dados e Segurança (5.x)
- Tech Spec: `SendDefaultPii=false` e filtros

## Critérios de Sucesso
- Segredos somente em armazenamento seguro
- Contas e papéis definidos e comunicados
- Conformidade com privacidade validada com segurança/legais (quando aplicável)

