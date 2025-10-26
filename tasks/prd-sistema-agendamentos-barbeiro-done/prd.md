# PRD - Sistema de Agendamentos (Barbeiro)

## Visão Geral

O módulo de Sistema de Agendamentos para Barbeiros permite que profissionais acessem suas agendas de forma isolada em cada barbearia onde trabalham, confirmem ou cancelem atendimentos, e gerenciem seus compromissos diários. Um barbeiro pode trabalhar em múltiplas barbearias e terá uma agenda completamente separada para cada estabelecimento, garantindo isolamento total dos dados e facilitando a gestão de múltiplos vínculos de trabalho.

## Objetivos

- **Objetivo Principal**: Permitir que barbeiros gerenciem suas agendas de forma independente em cada barbearia onde trabalham
- **Métricas de Sucesso**:
  - Barbeiro consegue acessar agenda em < 5 segundos após login
  - Taxa de confirmação de agendamentos > 90%
  - 100% de isolamento entre agendas de barbearias diferentes
- **Objetivos de Negócio**:
  - Facilitar gestão de compromissos para barbeiros multi-vinculados
  - Dar autonomia aos profissionais para gerenciar seus atendimentos
  - Reduzir no-shows através de confirmação de agendamentos

## Histórias de Usuário

### Persona: Barbeiro
Profissional que presta serviços de barbearia, podendo trabalhar em um ou múltiplos estabelecimentos simultaneamente.

**Histórias Principais:**

- Como Barbeiro, eu quero **acessar minhas agendas separadamente em cada barbearia** onde trabalho para que eu possa gerenciar meus compromissos de forma organizada e isolada
- Como Barbeiro, eu quero **visualizar meus agendamentos do dia** para que eu saiba quais clientes vou atender e em que horários
- Como Barbeiro, eu quero **confirmar agendamentos** para que os clientes saibam que o atendimento está garantido
- Como Barbeiro, eu quero **cancelar agendamentos** quando necessário para liberar horários e permitir que outros clientes agendem
- Como Barbeiro, eu quero **trocar de contexto entre minhas barbearias** facilmente para acessar diferentes agendas sem confusão

**Casos de Uso Secundários:**

- Como Barbeiro, eu quero **visualizar detalhes do agendamento** (nome do cliente, serviço, horário) para me preparar para o atendimento
- Como Barbeiro, eu quero **visualizar agendamentos futuros** (próximos dias/semanas) para planejar minha agenda
- Como Barbeiro, eu quero **visualizar histórico de atendimentos passados** para ter registro do que já foi realizado
- Como Barbeiro, eu quero **ver informações de contato do cliente** para poder entrar em contato se necessário

## Funcionalidades Principais

### 1. Acesso Multi-Agenda Isolado

**O que faz**: Permite que barbeiro acesse agendas separadas para cada barbearia onde está vinculado.

**Por que é importante**: Barbeiros podem trabalhar em múltiplos estabelecimentos e precisam gerenciar compromissos de forma isolada, evitando confusão e conflitos.

**Como funciona**:
- Barbeiro faz login no sistema usando telefone
- Sistema identifica em quais barbearias o barbeiro está vinculado
- Barbeiro seleciona qual barbearia deseja acessar
- Sistema exibe agenda específica daquela barbearia
- Barbeiro pode trocar de barbearia usando seletor de contexto

**Requisitos Funcionais:**

1.1. O sistema deve identificar automaticamente todas as barbearias onde o barbeiro está vinculado após login

1.2. Se barbeiro trabalha em apenas uma barbearia, deve ser direcionado diretamente para sua agenda

1.3. Se barbeiro trabalha em múltiplas barbearias, deve ver tela de seleção com lista de estabelecimentos

1.4. O sistema deve exibir seletor de contexto (dropdown ou menu) sempre visível para trocar entre barbearias

1.5. Ao trocar de barbearia, o sistema deve carregar agenda específica daquele estabelecimento

1.6. O contexto selecionado deve ser persistido na sessão do usuário

1.7. A agenda de uma barbearia não deve mostrar nenhum dado de outras barbearias (isolamento total)

1.8. O sistema deve destacar visualmente em qual barbearia o barbeiro está operando (nome no header)

### 2. Visualização de Agenda

**O que faz**: Exibe agendamentos do barbeiro na barbearia selecionada com informações completas.

**Por que é importante**: Barbeiro precisa saber quem vai atender, quando e qual serviço para organizar seu dia.

**Como funciona**:
- Barbeiro acessa agenda de uma barbearia específica
- Sistema exibe lista/calendário de agendamentos
- Barbeiro pode visualizar detalhes de cada atendimento
- Barbeiro pode navegar entre dias/períodos

**Requisitos Funcionais:**

2.1. O sistema deve exibir agenda com visualização padrão do dia atual

2.2. Cada agendamento deve mostrar:
   - Nome do cliente
   - Horário de início e fim
   - Serviço solicitado
   - Status (Pendente/Confirmado/Concluído/Cancelado)
   - Telefone de contato do cliente

2.3. O sistema deve permitir navegação entre dias (dia anterior/próximo dia)

2.4. O sistema deve permitir visualização por período:
   - Dia específico (padrão)
   - Semana
   - Mês (visualização reduzida)

2.5. O sistema deve destacar visualmente:
   - Horário atual na timeline
   - Status de cada agendamento (cores diferentes)
   - Próximo agendamento do dia

2.6. Os agendamentos devem ser ordenados cronologicamente

2.7. O sistema deve exibir mensagem "Nenhum agendamento" quando não houver atendimentos no período

2.8. O sistema deve mostrar contador de agendamentos do dia (ex: "5 agendamentos hoje")

### 3. Confirmar Agendamento

**O que faz**: Permite que barbeiro confirme um agendamento pendente.

**Por que é importante**: Confirmação dá segurança ao cliente de que o atendimento está garantido e reduz no-shows.

**Como funciona**:
- Barbeiro visualiza agendamento com status "Pendente"
- Clica em botão "Confirmar"
- Sistema atualiza status para "Confirmado"
- Cliente recebe indicação visual de que foi confirmado (em sua interface)

**Requisitos Funcionais:**

3.1. O sistema deve exibir botão "Confirmar" apenas para agendamentos com status "Pendente"

3.2. Ao clicar em confirmar, sistema deve atualizar status imediatamente para "Confirmado"

3.3. O sistema deve exibir feedback visual de sucesso

3.4. Agendamento confirmado deve ter indicação visual clara (ex: ícone de check, cor verde)

3.5. O sistema deve registrar data/hora da confirmação

3.6. Confirmação não pode ser desfeita (agendamento permanece confirmado até ser concluído ou cancelado)

3.7. Agendamentos já confirmados devem mostrar status "Confirmado" sem botão de ação

### 4. Cancelar Agendamento

**O que faz**: Permite que barbeiro cancele um agendamento (pendente ou confirmado).

**Por que é importante**: Barbeiro precisa poder cancelar quando há imprevistos, liberando horário para outros clientes.

**Como funciona**:
- Barbeiro visualiza agendamento
- Clica em botão "Cancelar"
- Sistema exibe confirmação
- Barbeiro confirma cancelamento
- Sistema atualiza status e libera horário

**Requisitos Funcionais:**

4.1. O sistema deve permitir cancelamento de agendamentos com status "Pendente" ou "Confirmado"

4.2. O sistema deve exibir modal de confirmação antes de cancelar com texto: "Tem certeza que deseja cancelar este agendamento? O cliente será notificado." (nota: notificação é para Fase 2, mas texto já avisa)

4.3. Ao confirmar cancelamento, sistema deve:
   - Atualizar status para "Cancelado"
   - Registrar data/hora do cancelamento
   - Liberar horário para novos agendamentos

4.4. Agendamentos cancelados devem permanecer visíveis na agenda com indicação visual de cancelamento (ex: texto riscado, cor vermelha)

4.5. O sistema deve exibir mensagem de sucesso após cancelamento

4.6. Cancelamento não pode ser desfeito (agendamento permanece cancelado)

4.7. Agendamentos com status "Concluído" ou "Cancelado" não podem ser cancelados novamente

### 5. Visualizar Detalhes do Agendamento

**O que faz**: Exibe informações completas sobre um agendamento específico.

**Por que é importante**: Barbeiro pode precisar de detalhes adicionais (contato do cliente, observações) para se preparar para o atendimento.

**Como funciona**:
- Barbeiro clica em um agendamento na agenda
- Sistema abre modal ou painel com detalhes completos
- Barbeiro visualiza todas as informações e pode fechar

**Requisitos Funcionais:**

5.1. O sistema deve exibir detalhes do agendamento ao clicar:
   - Nome completo do cliente
   - Telefone de contato
   - Serviço solicitado
   - Horário de início e fim
   - Status atual
   - Data de criação do agendamento
   - Data de confirmação (se confirmado)
   - Data de cancelamento (se cancelado)

5.2. O modal/painel de detalhes deve ter botões de ação disponíveis conforme status:
   - "Confirmar" (se pendente)
   - "Cancelar" (se pendente ou confirmado)
   - "Fechar" (sempre disponível)

5.3. O sistema deve permitir fechar o modal clicando fora dele ou em botão "Fechar"

5.4. Detalhes devem ser de leitura apenas (barbeiro não edita dados do agendamento)

### 6. Marcar Agendamento como Concluído

**O que faz**: Permite que barbeiro marque um atendimento como finalizado após realizar o serviço.

**Por que é importante**: Mantém histórico organizado e permite distinguir agendamentos futuros de realizados.

**Como funciona**:
- Barbeiro termina o atendimento
- Marca agendamento como "Concluído"
- Agendamento vai para histórico

**Requisitos Funcionais:**

6.1. O sistema deve exibir botão "Concluir" para agendamentos confirmados cujo horário já passou

6.2. Ao clicar em "Concluir", sistema atualiza status para "Concluído"

6.3. O sistema deve registrar data/hora da conclusão

6.4. Agendamentos concluídos permanecem visíveis na agenda do dia mas com indicação visual clara (ex: cor cinza, ícone de check)

6.5. Agendamentos concluídos não podem ser mais modificados (não há ações disponíveis)

6.6. Conclusão não pode ser desfeita

## Experiência do Usuário

### Persona: Barbeiro
- **Necessidades**: Acesso rápido à agenda, visibilidade de compromissos, facilidade para confirmar/cancelar
- **Contexto de Uso**: Acessa múltiplas vezes ao dia, principalmente de manhã para ver agenda e entre atendimentos
- **Nível Técnico**: Básico a intermediário
- **Dispositivos**: Principalmente smartphone (interface deve ser mobile-first)

### Fluxos Principais

**Fluxo 1: Login e Acesso à Agenda (Barbeiro Multi-vinculado)**
1. Barbeiro acessa sistema e faz login com telefone
2. Sistema identifica que barbeiro trabalha em 3 barbearias
3. Sistema exibe tela de seleção: "Em qual barbearia deseja trabalhar hoje?"
4. Barbeiro seleciona "Barbearia XYZ"
5. Sistema carrega agenda da Barbearia XYZ com agendamentos do dia
6. Barbeiro visualiza 5 agendamentos para hoje

**Fluxo 2: Login e Acesso à Agenda (Barbeiro Único)**
1. Barbeiro acessa sistema e faz login com telefone
2. Sistema identifica que barbeiro trabalha apenas em 1 barbearia
3. Sistema carrega diretamente agenda daquela barbearia
4. Barbeiro visualiza seus agendamentos

**Fluxo 3: Confirmar Agendamento**
1. Barbeiro está visualizando agenda do dia
2. Vê agendamento com status "Pendente" para 14h00
3. Clica no agendamento para ver detalhes
4. Clica em botão "Confirmar"
5. Sistema atualiza status para "Confirmado" e exibe mensagem de sucesso
6. Agendamento aparece com indicação visual de confirmado (verde/check)

**Fluxo 4: Cancelar Agendamento**
1. Barbeiro visualiza agendamento confirmado para 16h00
2. Clica no agendamento
3. Clica em "Cancelar"
4. Sistema exibe modal: "Tem certeza que deseja cancelar?"
5. Barbeiro confirma
6. Sistema cancela, exibe sucesso e atualiza visualização

**Fluxo 5: Trocar de Barbearia**
1. Barbeiro está visualizando agenda da "Barbearia XYZ"
2. Precisa verificar agenda de outra barbearia
3. Clica no seletor de contexto no header
4. Seleciona "Barbearia ABC" da lista
5. Sistema carrega agenda da Barbearia ABC
6. Header atualiza mostrando "Barbearia ABC"
7. Barbeiro visualiza agendamentos dessa outra barbearia

**Fluxo 6: Concluir Atendimento**
1. Barbeiro termina atendimento das 10h00
2. Acessa agenda e clica no agendamento
3. Clica em "Concluir Atendimento"
4. Sistema atualiza status para "Concluído"
5. Agendamento aparece com indicação visual de concluído

### Requisitos de UI/UX

- **Mobile-First**: Interface otimizada para smartphones (principal dispositivo de uso)
- **Navegação Simples**: Máximo 2 cliques para ações principais
- **Código de Cores Consistente**:
  - Pendente: Amarelo/Laranja
  - Confirmado: Verde
  - Concluído: Cinza
  - Cancelado: Vermelho
- **Seletor de Contexto**: Sempre visível e destacado (dropdown no header)
- **Timeline Visual**: Linha do tempo mostrando horários de forma clara
- **Botões de Ação**: Grandes, touch-friendly (mínimo 44x44px)
- **Loading States**: Indicadores durante carregamento de dados
- **Feedback Imediato**: Confirmações visuais de todas as ações
- **Refresh Pull-to-Refresh**: Atualizar agenda com gesto de arrastar para baixo (mobile)

### Considerações de Acessibilidade

Para o MVP não há requisitos específicos de acessibilidade, mas seguir boas práticas básicas:
- Contraste adequado em todas as cores de status
- Texto legível em telas pequenas (tamanho mínimo 14px)
- Botões com área de toque adequada
- Labels descritivos para ações

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript (web responsiva, mobile-first)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (relacional)

### Isolamento Multi-tenant
- **Requisito Crítico**: Isolamento total de dados entre barbearias
- Cada requisição deve incluir contexto da barbearia selecionada
- Backend deve validar que barbeiro tem acesso àquela barbearia
- Queries devem filtrar por barbeiro + barbearia
- Zero vazamento de dados entre barbearias

### Autenticação
- Login por telefone (sem senha no MVP - validação fica para Fase 2)
- Sessão deve manter contexto da barbearia selecionada
- Token JWT deve incluir ID do barbeiro e barbearia atual

### Integrações
- API REST para consulta de agendamentos (filtrado por barbeiro + barbearia)
- API REST para atualização de status (confirmar/cancelar/concluir)
- Possível necessidade de atualização em tempo real (WebSockets ou polling)

### Performance
- Carregamento da agenda deve ser < 3 segundos
- Troca de contexto entre barbearias deve ser < 2 segundos
- Ações (confirmar/cancelar) devem responder em < 1 segundo
- Lista deve ser otimizada mesmo com 50+ agendamentos no mês

### Segurança
- Barbeiro só pode acessar agendamentos onde ele é o prestador
- Barbeiro só pode acessar dados de barbearias onde está vinculado
- Validação de autorização em todas as requisições
- Proteção contra acesso não autorizado a outras agendas

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Notificações Push**: Alertas de novos agendamentos ficam para Fase 2
- **Notificação ao Cliente**: Sistema não notifica cliente quando barbeiro confirma/cancela (Fase 2)
- **Edição de Agendamento**: Barbeiro não pode alterar horário/serviço, apenas confirmar/cancelar
- **Bloqueio de Horários**: Barbeiro não pode bloquear horários para pausas/almoço no MVP
- **Agenda Integrada**: Não há visualização consolidada de todas as barbearias em uma única agenda
- **Criação de Agendamento**: Barbeiro não cria agendamentos diretamente (apenas clientes podem agendar)
- **Relatórios**: Estatísticas de atendimentos, performance, etc. ficam fora do MVP
- **Chat com Cliente**: Comunicação direta com cliente fica para versão futura
- **Histórico Detalhado**: Visualização completa de histórico por cliente fica para versão futura
- **Configuração de Disponibilidade**: Definir horários de trabalho por dia da semana fica para Fase 2
- **Sincronização com Calendário Externo**: Integração com Google Calendar, etc. fica fora do MVP
- **Modo Offline**: Aplicação requer conexão constante no MVP

### Considerações Futuras (Pós-MVP)

- Notificações em tempo real (Fase 2)
- Gestão de disponibilidade e bloqueio de horários
- Relatórios e analytics de atendimentos
- Chat/comunicação interna
- Visualização consolidada multi-barbearia
- Integração com calendários externos
- Modo offline com sincronização

## Questões em Aberto

### Questões de Negócio

1. **Política de Cancelamento**: Há limite de tempo para barbeiro cancelar? (ex: não pode cancelar com menos de 2h de antecedência)

2. **Comunicação de Cancelamento**: Como cliente será informado quando barbeiro cancela? Apenas na interface ou há comunicação externa? (nota: notificações são Fase 2, mas definir expectativa)

3. **Conclusão Automática**: Agendamentos devem ser marcados como concluídos automaticamente após horário passar, ou sempre manual?

4. **Reagendamento**: Se barbeiro cancela, sistema deve oferecer opção de reagendar automaticamente?

### Questões Técnicas (para Tech Spec)

5. **Autenticação Inicial**: Como barbeiro faz primeiro login? Há senha ou validação por código SMS? (validação fica para Fase 2, mas definir fluxo mínimo)

6. **Persistência de Contexto**: Ao reabrir aplicação, sistema deve lembrar última barbearia selecionada?

7. **Atualização em Tempo Real**: Agenda deve atualizar automaticamente quando há novos agendamentos ou requer refresh manual?

8. **Conflito de Status**: O que acontece se barbeiro tenta confirmar agendamento que cliente acabou de cancelar? Como tratar?

9. **Timezone**: Como tratar barbeiros que trabalham em barbearias de fusos horários diferentes?

### Questões de UX

10. **Visualização Padrão**: Mostrar apenas dia atual ou incluir próximos dias?

11. **Indicador de Novos Agendamentos**: Como barbeiro saberá que há novos agendamentos sem notificações? Badge visual? Indicador?

12. **Histórico de Cancelados**: Agendamentos cancelados devem sumir da visualização ou permanecer visíveis?

13. **Onboarding**: Como barbeiro é instruído a fazer primeiro acesso após ser adicionado por Admin da Barbearia?

14. **Ação Rápida**: Permitir confirmar/cancelar direto da lista sem abrir detalhes (swipe actions em mobile)?

---

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunho para Revisão
