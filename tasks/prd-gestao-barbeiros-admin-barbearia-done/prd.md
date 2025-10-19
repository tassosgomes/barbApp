# PRD - Gestão de Barbeiros (Admin da Barbearia)

## Visão Geral

O módulo de Gestão de Barbeiros permite que o Admin da Barbearia adicione, remova e gerencie barbeiros vinculados ao seu estabelecimento, além de visualizar a agenda completa de todos os barbeiros para facilitar a coordenação de atendimentos. Este módulo é fundamental para que cada barbearia mantenha sua equipe atualizada no sistema e tenha visibilidade total dos agendamentos, permitindo gestão eficiente da operação diária do estabelecimento.

## Objetivos

- **Objetivo Principal**: Permitir que Admin da Barbearia gerencie sua equipe de barbeiros e monitore agendamentos de forma centralizada
- **Métricas de Sucesso**:
  - Tempo médio para adicionar um barbeiro < 1 minuto
  - Visualização da agenda completa carregando em < 3 segundos
  - 100% dos barbeiros com informações básicas completas
- **Objetivos de Negócio**:
  - Facilitar gestão da equipe pelos proprietários de barbearias
  - Dar visibilidade total dos agendamentos para otimizar operação
  - Manter cadastro de barbeiros atualizado e organizado

## Histórias de Usuário

### Persona: Admin da Barbearia
Proprietário ou gerente do estabelecimento que gerencia a equipe de barbeiros e monitora a operação diária.

**Histórias Principais:**

- Como Admin da Barbearia, eu quero **adicionar barbeiros à minha equipe** para que eles possam atender clientes e ter agendamentos no sistema
- Como Admin da Barbearia, eu quero **remover barbeiros da equipe** para que profissionais que não trabalham mais na barbearia não apareçam nas opções de agendamento
- Como Admin da Barbearia, eu quero **visualizar a agenda completa de todos os barbeiros** para que eu possa ter visibilidade total dos atendimentos do dia e coordenar melhor a operação
- Como Admin da Barbearia, eu quero **ver informações básicas de cada barbeiro** (nome, telefone, status) para gerenciar minha equipe eficientemente

**Casos de Uso Secundários:**

- Como Admin da Barbearia, eu quero **filtrar a agenda por barbeiro específico** para que eu possa verificar a carga de trabalho individual
- Como Admin da Barbearia, eu quero **visualizar agenda por dia/semana** para planejar a operação do estabelecimento
- Como Admin da Barbearia, eu quero **ver histórico de atendimentos por barbeiro** para avaliar performance da equipe

## Funcionalidades Principais

### 1. Adicionar Barbeiro

**O que faz**: Permite cadastrar um novo barbeiro vinculado à barbearia específica.

**Por que é importante**: É essencial para montar a equipe e permitir que barbeiros aceitem agendamentos naquela barbearia.

**Como funciona**:
- Admin da Barbearia acessa painel de gestão de barbeiros
- Preenche formulário com dados básicos do barbeiro
- Barbeiro é adicionado à equipe da barbearia
- Barbeiro pode então acessar sua agenda nessa barbearia

**Requisitos Funcionais:**

1.1. O sistema deve permitir cadastro de barbeiro com os seguintes campos obrigatórios:
   - Nome completo
   - Telefone (será usado para login do barbeiro)
   - Especialidades/Serviços que oferece

1.2. O sistema deve validar formato do telefone antes do cadastro

1.3. O mesmo telefone pode ser cadastrado como barbeiro em múltiplas barbearias (permitir que um profissional trabalhe em vários estabelecimentos de forma isolada)

1.4. Após criação bem-sucedida, o sistema deve exibir mensagem de confirmação

1.5. O barbeiro criado deve aparecer imediatamente na listagem da equipe

1.6. O barbeiro adicionado deve ter status "Ativo" por padrão

### 2. Remover Barbeiro

**O que faz**: Remove um barbeiro da equipe da barbearia específica.

**Por que é importante**: Mantém a equipe atualizada quando profissionais deixam o estabelecimento, evitando que apareçam em agendamentos.

**Como funciona**:
- Admin da Barbearia acessa listagem de barbeiros
- Seleciona barbeiro para remover
- Sistema exibe confirmação
- Admin confirma e barbeiro é removido da equipe

**Requisitos Funcionais:**

2.1. O sistema deve exigir confirmação antes de remover barbeiro

2.2. O modal de confirmação deve exibir:
   - Nome do barbeiro
   - Número de agendamentos futuros que serão afetados (se houver)
   - Aviso sobre impacto da remoção

2.3. A remoção deve ser isolada (barbeiro é removido apenas dessa barbearia, não afeta outros estabelecimentos onde trabalha)

2.4. Sistema deve tratar agendamentos futuros do barbeiro removido:
   - Opção 1: Cancelar automaticamente todos os agendamentos futuros
   - Opção 2: Manter agendamentos existentes mas não permitir novos
   - (Decisão a ser definida em Questões em Aberto)

2.5. Barbeiro removido não deve mais aparecer na listagem da equipe

2.6. Barbeiro removido não deve mais aparecer como opção em novos agendamentos de clientes

### 3. Visualizar Equipe de Barbeiros

**O que faz**: Exibe lista de todos os barbeiros vinculados à barbearia.

**Por que é importante**: Permite gestão visual da equipe e acesso rápido aos profissionais.

**Como funciona**:
- Admin da Barbearia acessa painel de equipe
- Sistema exibe lista de barbeiros com informações resumidas
- Admin pode visualizar detalhes de cada profissional

**Requisitos Funcionais:**

3.1. O sistema deve exibir lista de barbeiros com as seguintes informações:
   - Nome completo
   - Telefone
   - Especialidades/Serviços
   - Status (Ativo/Inativo)
   - Data de adição à equipe

3.2. O sistema deve permitir busca por nome do barbeiro

3.3. O sistema deve permitir filtro por status (Ativo/Inativo/Todos)

3.4. A lista deve ser ordenada alfabeticamente por padrão

3.5. Cada item da lista deve ter ações: Visualizar, Editar, Remover

### 4. Visualizar Agenda Completa

**O que faz**: Exibe agenda consolidada de todos os barbeiros da barbearia com todos os agendamentos.

**Por que é importante**: Dá visibilidade total ao Admin da Barbearia para coordenar operação, identificar horários livres e gerenciar fluxo de clientes.

**Como funciona**:
- Admin da Barbearia acessa visualização de agenda
- Sistema exibe agenda de todos os barbeiros organizadamente
- Admin pode filtrar por barbeiro, data ou período
- Admin visualiza detalhes de cada agendamento

**Requisitos Funcionais:**

4.1. O sistema deve exibir agenda com visualização em grade (estilo calendário) ou lista

4.2. Cada agendamento deve mostrar:
   - Nome do cliente
   - Nome do barbeiro
   - Horário (início e fim)
   - Serviço solicitado
   - Status (Pendente/Confirmado/Concluído/Cancelado)

4.3. O sistema deve permitir filtros:
   - Por barbeiro específico ou todos
   - Por data (dia específico)
   - Por período (semana, mês)
   - Por status do agendamento

4.4. A visualização padrão deve ser "Hoje" com todos os barbeiros

4.5. O sistema deve atualizar a agenda em tempo real quando houver novos agendamentos

4.6. O sistema deve permitir navegação entre dias (anterior/próximo)

4.7. Sistema deve destacar visualmente:
   - Horário atual
   - Agendamentos confirmados vs pendentes
   - Agendamentos cancelados (com indicação visual diferenciada)

4.8. Admin da Barbearia pode visualizar detalhes do agendamento mas não pode confirmar/cancelar (apenas barbeiro pode fazer isso no MVP)

### 5. Editar Informações do Barbeiro

**O que faz**: Permite atualizar dados cadastrais de um barbeiro específico.

**Por que é importante**: Mantém informações atualizadas quando há mudanças (telefone, especialidades).

**Como funciona**:
- Admin da Barbearia seleciona barbeiro da lista
- Clica em editar
- Modifica campos desejados
- Sistema salva alterações

**Requisitos Funcionais:**

5.1. O sistema deve permitir edição dos campos:
   - Nome completo
   - Telefone
   - Especialidades/Serviços

5.2. O sistema deve pré-preencher formulário com dados atuais

5.3. O sistema deve validar campos com mesmas regras da criação

5.4. Alteração de telefone deve ser permitida (telefone é usado para login do barbeiro)

5.5. O sistema deve exibir mensagem de confirmação após atualização

## Experiência do Usuário

### Persona: Admin da Barbearia
- **Necessidades**: Gestão simples da equipe, visibilidade dos agendamentos, controle sobre quem atende na barbearia
- **Contexto de Uso**: Acessa múltiplas vezes ao dia para verificar agenda e gerenciar equipe
- **Nível Técnico**: Básico a intermediário
- **Dispositivos**: Principalmente smartphone e tablet (interface deve ser responsiva)

### Fluxos Principais

**Fluxo 1: Adicionar Novo Barbeiro**
1. Admin acessa painel da barbearia
2. Navega para seção "Equipe" ou "Barbeiros"
3. Clica em "Adicionar Barbeiro"
4. Preenche formulário (nome, telefone, especialidades)
5. Clica em "Salvar"
6. Sistema valida, cria barbeiro e exibe confirmação
7. Barbeiro aparece na lista da equipe

**Fluxo 2: Visualizar Agenda do Dia**
1. Admin acessa painel da barbearia
2. Navega para seção "Agenda"
3. Sistema exibe agenda do dia atual com todos os barbeiros
4. Admin visualiza horários ocupados e disponíveis
5. Admin pode clicar em agendamento para ver detalhes

**Fluxo 3: Remover Barbeiro da Equipe**
1. Admin acessa lista de barbeiros
2. Localiza barbeiro a ser removido
3. Clica em "Remover" ou ícone de exclusão
4. Sistema exibe confirmação com avisos
5. Admin confirma remoção
6. Sistema remove barbeiro e atualiza listagem

**Fluxo 4: Filtrar Agenda por Barbeiro**
1. Admin está visualizando agenda completa
2. Seleciona filtro "Barbeiro"
3. Escolhe barbeiro específico da lista
4. Sistema atualiza visualização mostrando apenas agendamentos daquele barbeiro
5. Admin pode remover filtro para voltar à visualização completa

### Requisitos de UI/UX

- Interface deve ser intuitiva para usuários com nível técnico básico
- Agenda deve ter visualização clara e organizada (calendário ou timeline)
- Ações principais (adicionar barbeiro, ver agenda) devem estar acessíveis em 1-2 cliques
- Formulários devem ter validação em tempo real
- Status dos agendamentos devem ter cores distintas (código de cores consistente)
- Interface responsiva otimizada para mobile (touch-friendly)
- Loading states durante carregamento da agenda
- Refresh automático da agenda sem recarregar página completa

### Considerações de Acessibilidade

Para o MVP não há requisitos específicos de acessibilidade, mas seguir boas práticas básicas:
- Contraste adequado nas cores de status
- Texto legível em dispositivos móveis
- Botões com área de toque adequada (mínimo 44x44px)

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript (web responsiva, mobile-first)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (relacional)

### Isolamento Multi-tenant
- Cada barbearia deve ver apenas seus próprios barbeiros
- Um barbeiro pode estar vinculado a múltiplas barbearias de forma completamente isolada
- Dados de agenda são isolados por barbearia (cada barbearia vê apenas agendamentos de seus clientes)

### Integrações
- API REST para CRUD de barbeiros com filtro por barbearia
- API REST para consulta de agenda consolidada
- Possível necessidade de WebSockets ou polling para atualização em tempo real da agenda (definir na Tech Spec)

### Performance
- Listagem de barbeiros deve carregar em < 1 segundo
- Visualização de agenda deve carregar em < 3 segundos (mesmo com múltiplos barbeiros e agendamentos)
- Filtros na agenda devem responder instantaneamente (< 500ms)

### Segurança
- Admin da Barbearia só pode gerenciar barbeiros da sua própria barbearia
- Autenticação e autorização devem validar contexto da barbearia em todas as requisições
- Dados sensíveis (telefone) devem ser protegidos conforme LGPD

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Notificações Push para Admin**: Alertas de novos agendamentos ficam para Fase 2
- **Gestão de Horário de Trabalho**: Definir horários disponíveis por barbeiro fica para versão futura
- **Relatórios de Performance**: Analytics de atendimentos por barbeiro são da Fase 3
- **Comissionamento**: Cálculo de comissão por barbeiro fica fora do MVP
- **Avaliações de Barbeiros**: Sistema de ratings/reviews fica para versão futura
- **Fotos dos Barbeiros**: Upload de imagem/foto de perfil não está no MVP
- **Permissões Granulares**: MVP tem apenas dois perfis: Admin da Barbearia (controle total) e Barbeiro (apenas sua agenda)
- **Edição de Agendamento pelo Admin**: Admin apenas visualiza; apenas barbeiros podem confirmar/cancelar no MVP
- **Chat/Comunicação Interna**: Comunicação entre Admin e Barbeiros fica para versão futura
- **Integração com Calendários Externos**: Sincronização com Google Calendar, etc. fica fora do MVP

### Considerações Futuras (Pós-MVP)

- Notificações para Admin da Barbearia (Fase 2)
- Gestão de horários disponíveis por barbeiro
- Relatórios de produtividade e performance
- Sistema de permissões mais granular
- Upload de fotos de perfil
- Avaliações e reviews de clientes sobre barbeiros
- Bloqueio de horários (folgas, intervalos)

## Questões em Aberto

### Questões de Negócio

1. **Política de Remoção**: Quando um barbeiro é removido da equipe, o que acontece com agendamentos futuros? Cancelar automaticamente? Manter mas não permitir novos?

2. **Limite de Barbeiros**: Há limite de barbeiros por barbearia no MVP?

3. **Status Inativo**: Precisamos de status "Inativo" para barbeiros ou apenas "Ativo" e "Removido"? Caso de uso para inativo temporário (férias)?

4. **Edição pelo Admin**: Admin da Barbearia pode confirmar/cancelar agendamentos ou apenas visualizar? (Sugestão: apenas visualizar no MVP)

### Questões Técnicas (para Tech Spec)

5. **Autenticação de Barbeiro**: Como barbeiro fará login pela primeira vez após ser adicionado? Há senha ou apenas telefone?

6. **Atualização em Tempo Real**: Agenda deve atualizar em tempo real (WebSockets) ou refresh manual/periódico é suficiente para MVP?

7. **Vinculação de Serviços**: Como serviços oferecidos são cadastrados? Texto livre ou lista pré-definida? Há preços?

8. **Unicidade de Telefone**: Devemos validar se telefone já existe na mesma barbearia para evitar duplicatas?

### Questões de UX

9. **Visualização de Agenda**: Preferência entre visualização em calendário (grade) vs lista? Ou ambas?

10. **Período de Visualização**: Qual período de agenda mostrar por padrão? Apenas dia atual? Semana?

11. **Notificação de Novo Agendamento**: Como Admin saberá que há novos agendamentos sem sistema de notificações? Badge visual? Indicador?

12. **Onboarding de Barbeiro**: Como barbeiro é informado que foi adicionado? Admin comunica manualmente?

---

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunho para Revisão
