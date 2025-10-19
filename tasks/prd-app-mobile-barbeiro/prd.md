# PRD - App Mobile do Barbeiro (iOS/Android)

## Visão Geral

Aplicativo mobile voltado exclusivamente ao perfil Barbeiro para consultar e gerenciar a própria agenda em cada barbearia onde está vinculado (multi-tenant, com isolamento total). O app prioriza uso em smartphones durante o expediente, oferecendo visualização do dia, detalhes do agendamento e ações rápidas de confirmar, cancelar e concluir atendimentos. Não contempla funcionalidades de Admin da Barbearia nem criação/edição de agendamentos pelo barbeiro no MVP.

## Objetivos

- Sucesso do usuário
  - Acessar a agenda do dia em < 3s após login/sessão válida
  - Confirmar/cancelar um agendamento em < 1s (feedback visível)
  - Taxa de confirmação de agendamentos pendentes > 90%
- Métricas de produto
  - Engajamento diário (DAU de barbeiros ativos)
  - Tempo médio de primeira visualização da agenda
  - % de ações concluídas com sucesso (confirmar/cancelar/concluir)
- Objetivos de negócio
  - Dar autonomia ao barbeiro para gerir o próprio dia
  - Reduzir no-shows por meio de confirmações oportunas
  - Garantir isolamento e segurança por barbearia (multi-tenant)

## Histórias de Usuário

Persona: Barbeiro (profissional que pode trabalhar em uma ou várias barbearias).

Principais histórias:
- Como Barbeiro, quero fazer login e selecionar a barbearia (quando aplicável) para acessar minha agenda do dia.
- Como Barbeiro, quero visualizar meus agendamentos do dia de forma clara, com status e horários, para organizar meu trabalho.
- Como Barbeiro, quero confirmar um agendamento pendente para sinalizar ao cliente que o atendimento está garantido.
- Como Barbeiro, quero cancelar um agendamento quando necessário para liberar o horário e evitar confusões.
- Como Barbeiro, quero marcar um atendimento como concluído para manter meu histórico organizado.

Casos de uso secundários:
- Visualizar detalhes do agendamento (cliente, serviço, horário, status, contatos).
- Navegar para dias anteriores/seguintes e consultar a semana.
- Trocar rapidamente de barbearia quando trabalho em mais de uma.
- Atualizar a agenda manualmente via pull-to-refresh.

## Funcionalidades Principais

### 1) Autenticação e Sessão do Barbeiro

O que faz: permite login com e-mail e senha e mantém sessão ativa.
Por que é importante: é a porta de entrada e reduz fricção no dia a dia.
Como funciona (alto nível): tela de login mobile-first; validação de campos; armazenamento seguro do token; redireciona para agenda.

Requisitos funcionais:
1.1 Tela de login com campos: e-mail e senha (teclado apropriado para e-mail; opção mostrar/ocultar senha).
1.2 Botão Entrar habilita apenas com campos válidos; exibir loading durante autenticação.
1.3 Em sucesso, armazenar token de forma segura e redirecionar para agenda (ou seleção de barbearia, se multi-vínculo).
1.4 Em erro (401/400), exibir mensagem clara e manter campos preenchidos.
1.5 Persistir sessão por 24h; em token inválido/expirado, voltar ao login.
1.6 Recuperação de senha e primeiro acesso por autoatendimento: pós-MVP.

Contrato de API (login):
Request
```json
{
  "email": "barbeiro@example.com",
  "password": "SenhaSegura123!"
}
```
Response
- 200 OK com token JWT (conforme backend) e metadados necessários ao contexto.

### 2) Seleção de Barbearia (Multi-tenant)

O que faz: permite escolher em qual barbearia operar quando o barbeiro tem múltiplos vínculos.
Por que é importante: garante isolamento total de dados entre estabelecimentos.
Como funciona: após login (ou via seletor no app), o usuário escolhe uma barbearia; o contexto é aplicado a todas as requisições.

Requisitos funcionais:
2.1 Se houver apenas uma barbearia, pular seleção e ir direto à agenda.
2.2 Se houver múltiplas, exibir lista com nome e cidade/identificador; selecionar define o contexto atual.
2.3 Mostrar seletor de contexto no topo da agenda para trocar de barbearia a qualquer momento.
2.4 Persistir última barbearia selecionada para a próxima sessão.
2.5 Impedir acesso a dados de outras barbearias (filtro por barbeiro + barbearia em todas as chamadas).

### 3) Agenda do Dia (Visualização e Navegação)

O que faz: exibe a agenda do barbeiro na barbearia atual, priorizando o dia corrente.
Por que é importante: dá visibilidade do trabalho do dia de forma rápida e confiável.
Como funciona: visão em lista/timeline com status e horários; navegação por dias e visão semanal opcional.

Requisitos funcionais:
3.1 Visualização padrão: dia atual, ordenada cronologicamente; destaque para o próximo atendimento e horário atual.
3.2 Cada item mostra: cliente (nome), horário (início/fim), serviço, status (Pendente/Confirmado/Concluído/Cancelado).
3.3 Exibir indicador quando não houver agendamentos no dia (ex.: "Nenhum agendamento").
3.4 Permitir navegar para dia anterior/próximo e visualizar semana (lista compacta) quando ativada.
3.5 Pull-to-refresh para atualizar manualmente; atualização automática por polling a cada 30s (MVP).
3.6 Mostrar contador de agendamentos do dia.

### 4) Detalhes do Agendamento

O que faz: mostra informações completas de um agendamento e ações contextuais.
Por que é importante: prepara o barbeiro para o atendimento e agiliza decisões.
Como funciona: ao tocar no item, abre modal/painel com dados e botões de ação.

Requisitos funcionais:
4.1 Exibir: cliente (nome e telefone), serviço, duração/horário, status atual, datas de criação/alteração de status.
4.2 Ações disponíveis conforme status: Confirmar (se Pendente); Cancelar (se Pendente/Confirmado); Concluir (se Confirmado e horário já passou).
4.3 Link de contato por telefone/WhatsApp do cliente (a confirmar no escopo) sem manter histórico no app.
4.4 Fechar por gesto (swipe down) ou botão.

### 5) Confirmar Agendamento

O que faz: muda status de Pendente para Confirmado.
Por que é importante: reduz no-show e alinha expectativas com o cliente.
Como funciona: ação rápida no detalhe (ou atalho na lista, se habilitado) com feedback imediato.

Requisitos funcionais:
5.1 Exibir botão Confirmar apenas para status Pendente.
5.2 Atualizar status imediatamente e registrar data/hora da confirmação.
5.3 Mostrar feedback de sucesso; refletir na lista sem recarregar a tela inteira.
5.4 Em caso de `409 Conflict` (status alterado por concorrência), exibir aviso e atualizar a agenda (refetch imediato).
5.5 Não permitir desfazer confirmação (permanecerá até concluir ou cancelar).

### 6) Cancelar Agendamento

O que faz: permite cancelar atendimentos Pendentes ou Confirmados.
Por que é importante: libera o horário e evita deslocamentos desnecessários.
Como funciona: ação com modal de confirmação e atualização do status.

Requisitos funcionais:
6.1 Disponível para agendamentos Pendente/Confirmado; bloquear para Concluído/Cancelado.
6.2 Modal de confirmação com aviso de impacto ao cliente (notificações push fora do MVP).
6.3 Ao confirmar: atualizar status para Cancelado, registrar data/hora e refletir visualmente (ex.: cinza/vermelho com indicação).
6.4 Em caso de `409 Conflict` (status já concluído/cancelado), exibir aviso e atualizar a agenda (refetch).
6.5 Cancelamento não pode ser desfeito.

### 7) Concluir Atendimento

O que faz: marca o serviço como finalizado após o horário.
Por que é importante: organiza histórico e métricas de atendimento.
Como funciona: ação disponível quando o horário já passou e status é Confirmado.

Requisitos funcionais:
7.1 Exibir botão Concluir apenas em atendimentos Confirmados e elegíveis (pós-horário).
7.2 Atualizar status para Concluído e registrar data/hora.
7.3 Remover demais ações e ajustar destaque visual do item (estado finalizado).
7.4 Em caso de `400 Bad Request` (horário ainda não iniciou) ou `409 Conflict` (transição inválida), exibir mensagem e atualizar a agenda.

### 8) Navegação e Usabilidade Mobile

O que faz: otimiza a experiência para smartphone.
Por que é importante: o barbeiro usa o app entre atendimentos e com pouco tempo.
Como funciona: interações touch-friendly, feedbacks claros e baixa fricção.

Requisitos funcionais:
8.1 Botões e alvos de toque mínimos 44x44px; tipografia legível (>=16px).
8.2 Cores de status consistentes: Pendente (amarelo/laranja), Confirmado (verde), Concluído (cinza), Cancelado (vermelho).
8.3 Estados de carregamento (skeleton/spinner) e toasts de sucesso/erro.
8.4 Ações rápidas opcionais na lista (swipe para Confirmar/Cancelar) — confirmar se incluídas no MVP.
8.5 Acessibilidade básica: contraste adequado, labels claros, navegação por foco.

## Experiência do Usuário

- Personas e necessidades: Barbeiro precisa de acesso rápido à agenda, decisões ágeis (confirmar/cancelar) e visibilidade do dia.
- Fluxos principais:
  - Login -> (Seleção de Barbearia se multi-vínculo) -> Agenda do Dia
  - Agenda -> Detalhe -> Confirmar/Cancelar
  - Agenda -> Detalhe (pós-horário) -> Concluir
  - Header -> Seletor -> Trocar barbearia -> Agenda atualizada
- Diretrizes de UI/UX: mobile-first, poucos toques para ações principais, feedback imediato, pull-to-refresh, polling leve, sem distrações.

## Restrições Técnicas de Alto Nível

- Multi-tenant e segurança: todas as chamadas devem incluir contexto (barbeiro + barbearia); backend valida vínculo e escopo; zero vazamento entre tenants.
- Autenticação: login por e-mail + senha; token com expiração (~24h); armazenamento seguro; interceptador nas chamadas.
- Integrações (back-end atual):
  - `POST /api/auth/barbeiro/login` (input: `{ email, password }`) — autenticação do barbeiro.
  - `GET /api/barbeiro/barbearias` — lista barbearias do barbeiro autenticado (para seleção de contexto, quando multi-vínculo).
  - `POST /api/auth/barbeiro/trocar-contexto` — troca de contexto para outra barbearia (gera novo token com `NovaBarbeariaId`).
  - `GET /api/schedule/my-schedule?date=YYYY-MM-DD` — agenda do barbeiro autenticado (por dia).
  - `GET /api/appointments/{id}` — detalhes do agendamento.
  - `POST /api/appointments/{id}/confirm` — confirmar agendamento pendente.
  - `POST /api/appointments/{id}/cancel` — cancelar agendamento pendente/confirmado.
  - `POST /api/appointments/{id}/complete` — concluir agendamento confirmado (após horário).
- Novos endpoints recomendados (pós-MVP para aprimorar UX/performance):
  - `GET /api/schedule/my-schedule-range?from=YYYY-MM-DD&to=YYYY-MM-DD` — retorna agenda do barbeiro no intervalo; otimiza visão semanal sem múltiplas chamadas/dia.
  - `GET /api/auth/me` — valida token e retorna perfil/claims básicos (role, userId, barbeariaId, nomeBarbearia).
- Performance: primeira pintura útil da agenda < 3s; troca de barbearia < 2s; ações < 1s; lista eficiente para dezenas de atendimentos/mês.
- Privacidade/LGPD: exibir apenas dados mínimos do cliente (nome/telefone); evitar PII sensível; sanitizar logs.
- Observabilidade (básica): registrar falhas de rede e ações críticas; crashes/reporting poderão ser detalhados em PRD específico de observabilidade mobile.
- Plataformas alvo: iOS e Android (forma de implementação — nativo/híbrido — será definida na Tech Spec; PRD permanece agnóstico à tecnologia).
 - Fuso horário: backend retorna datas/horários em UTC; app deve exibir no fuso local do usuário (ex.: America/Sao_Paulo) e considerar horário de verão quando aplicável.

## Não-Objetivos (Fora de Escopo)

- Criação/edição de agendamentos pelo barbeiro no MVP.
- Gestão de serviços, disponibilidade/folgas e catálogo.
- Funções de Admin da Barbearia (gestão de equipe/serviços/agenda global).
- Notificações push e comunicação ativa com cliente (SMS/WhatsApp) no MVP.
- Chat/mensageria, pagamentos, comissionamento, relatórios/analytics.
- Integração com calendários externos (Google/Apple) no MVP.
- Modo offline e sincronização diferida no MVP.
- Biometria/2FA e login social (podem ser considerados pós-MVP).
 - Login por telefone + código de barbearia (pós-MVP).

## Considerações Futuras (Pós-MVP)

- Suporte a login por telefone + código e/ou verificação por SMS.
- Endpoint `my-schedule-range` para visão semanal mais performática.
- Endpoint `auth/me` para validação/refresh de sessão e recuperação de perfil.
- Push notifications (novo agendamento, cancelamento, lembretes).
- Ações por swipe na lista (confirmar/cancelar) e quick actions de contato (ligar/WhatsApp).

## Questões em Aberto

Negócio e escopo
1) Incluir ação de contato rápido (ligar/WhatsApp) no detalhe do agendamento no MVP?
2) Há política de cancelamento (ex.: impedir cancelamento a menos de X minutos do atendimento)?
3) Conclusão automática após horário deve existir ou manter apenas ação manual?
4) Visual padrão: somente dia atual ou listar também próximos 2–3 dias?

Técnico e UX
5) Push notifications entram no MVP ou ficam para Fase 2?
6) Ações por swipe na lista entram no MVP ou ficam para Fase 2?
7) Persistir última barbearia selecionada entre reinstalações? Onde (armazenamento seguro)?
8) Necessidade de suporte a múltiplos idiomas ou apenas pt-BR no lançamento?

Operação
9) Observabilidade/crash reporting mobile: incluir Sentry/Crashlytics já no MVP?
10) Políticas de sessão: permitir múltiplos dispositivos por barbeiro simultaneamente?

---

Data de Criação: 2025-10-19  
Versão: 1.0 (MVP)  
Status: Rascunho para Revisão
