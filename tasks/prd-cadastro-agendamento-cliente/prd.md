# PRD - Cadastro e Agendamento (Cliente)

## Visão Geral

O módulo de Cadastro e Agendamento para Clientes permite que usuários finais se cadastrem em barbearias específicas através de um código único, agendem serviços com barbeiros disponíveis, visualizem e cancelem seus agendamentos, e mantenham histórico de atendimentos separado para cada barbearia. Um cliente pode estar cadastrado em múltiplas barbearias de forma totalmente independente, com dados e históricos isolados para cada estabelecimento, garantindo privacidade e organização.

## Objetivos

- **Objetivo Principal**: Permitir que clientes agendem serviços em barbearias de forma simples e autônoma, com cadastros isolados por estabelecimento
- **Métricas de Sucesso**:
  - Tempo médio de cadastro < 2 minutos
  - Taxa de conclusão de agendamento > 85%
  - Taxa de cancelamento pelo cliente < 15%
  - 100% de isolamento entre dados de barbearias diferentes
- **Objetivos de Negócio**:
  - Facilitar acesso dos clientes aos serviços das barbearias
  - Reduzir atrito no processo de agendamento
  - Permitir que clientes usem múltiplas barbearias sem confusão de dados
  - Aumentar taxa de conversão (visita -> agendamento)

## Histórias de Usuário

### Persona: Cliente
Usuário final que deseja agendar serviços de barbearia. Pode ser cliente regular ou ocasional, com preferência por um ou múltiplos estabelecimentos.

**Histórias Principais:**

- Como Cliente, eu quero **me cadastrar em uma barbearia usando seu código único** para que eu possa agendar serviços naquele estabelecimento específico
- Como Cliente, eu quero **fazer login usando telefone e nome** para acessar minha conta de forma simples sem precisar lembrar senha
- Como Cliente, eu quero **visualizar barbeiros disponíveis na barbearia** para escolher com quem desejo fazer o atendimento
- Como Cliente, eu quero **visualizar horários disponíveis de um barbeiro específico** para encontrar um horário que se encaixe na minha agenda
- Como Cliente, eu quero **agendar um serviço escolhendo barbeiro, serviço e horário** para garantir meu atendimento
- Como Cliente, eu quero **cancelar agendamentos** quando não puder comparecer para liberar o horário
- Como Cliente, eu quero **visualizar meu histórico de serviços** em cada barbearia para ter registro dos atendimentos realizados
- Como Cliente, eu quero **acessar múltiplas barbearias de forma independente** para poder frequentar diferentes estabelecimentos sem misturar minhas informações

**Casos de Uso Secundários:**

- Como Cliente, eu quero **visualizar meus agendamentos futuros** para saber quando e onde serão meus próximos atendimentos
- Como Cliente, eu quero **ver detalhes do serviço agendado** (barbeiro, serviço, data/hora) para confirmar informações
- Como Cliente, eu quero **ver status do agendamento** (pendente/confirmado) para saber se está garantido
- Como Cliente, eu quero **visualizar serviços oferecidos pela barbearia** com descrição e duração para escolher o adequado

## Funcionalidades Principais

### 1. Cadastro por Código da Barbearia

**O que faz**: Permite que cliente se cadastre em uma barbearia específica usando o código único do estabelecimento.

**Por que é importante**: É o ponto de entrada do cliente no sistema multi-tenant, garantindo que ele acesse apenas a barbearia correta de forma isolada.

**Como funciona**:
- Cliente recebe código da barbearia (divulgado pela barbearia)
- Cliente acessa URL com o código (ex: barbapp.com/XYZ123AB)
- Sistema valida código e exibe tela de cadastro daquela barbearia
- Cliente preenche dados básicos (nome, telefone)
- Cliente é cadastrado especificamente naquela barbearia

**Requisitos Funcionais:**

1.1. O sistema deve aceitar código da barbearia via URL (ex: `/barbearia/XYZ123AB` ou `?codigo=XYZ123AB`)

1.2. O sistema deve validar se código existe e se barbearia está ativa

1.3. Se código inválido, sistema deve exibir mensagem de erro clara: "Código de barbearia não encontrado"

1.4. O sistema deve exibir nome e informações da barbearia após validação do código (para cliente confirmar que está no lugar certo)

1.5. O formulário de cadastro deve solicitar:
   - Nome completo
   - Telefone (será usado para login)

1.6. O sistema deve validar formato do telefone

1.7. O mesmo telefone pode ser cadastrado em múltiplas barbearias (cliente pode frequentar vários estabelecimentos)

1.8. O sistema deve validar que o telefone ainda não está cadastrado naquela barbearia específica (evitar duplicatas)

1.9. Após cadastro bem-sucedido, cliente deve ser automaticamente logado naquela barbearia

1.10. O sistema deve manter o contexto da barbearia durante toda a sessão

### 2. Login Simples (Telefone + Nome)

**O que faz**: Permite que cliente faça login usando apenas telefone e nome, sem necessidade de senha.

**Por que é importante**: Reduz atrito de acesso, eliminando necessidade de lembrar senha para MVP.

**Como funciona**:
- Cliente acessa barbearia via código/URL
- Sistema identifica que telefone já está cadastrado naquela barbearia
- Cliente informa telefone e nome
- Sistema valida e faz login

**Requisitos Funcionais:**

2.1. Cliente acessa URL com código da barbearia

2.2. Sistema exibe tela de login com campos: telefone e nome

2.3. O sistema deve validar que telefone está cadastrado naquela barbearia específica

2.4. O sistema deve validar que nome corresponde ao telefone cadastrado (match case-insensitive)

2.5. Se validação falhar, sistema deve exibir mensagem: "Telefone ou nome incorretos para esta barbearia"

2.6. Se validação bem-sucedida, cliente é logado com contexto daquela barbearia

2.7. O sistema deve manter sessão do cliente

2.8. Cliente permanece no contexto da barbearia durante toda a sessão

**Nota**: Validação por código SMS fica para Fase 2. MVP usa apenas telefone + nome.

### 3. Visualização de Barbeiros Disponíveis

**O que faz**: Exibe lista de barbeiros que trabalham na barbearia para que cliente escolha com quem deseja agendar.

**Por que é importante**: Cliente pode ter preferência por barbeiro específico ou querer conhecer os profissionais disponíveis.

**Como funciona**:
- Cliente logado acessa página de agendamento
- Sistema exibe lista de barbeiros ativos naquela barbearia
- Cliente visualiza informações básicas de cada profissional
- Cliente seleciona barbeiro desejado

**Requisitos Funcionais:**

3.1. O sistema deve listar apenas barbeiros ativos da barbearia atual

3.2. Cada barbeiro deve ser exibido com:
   - Nome
   - Especialidades/Serviços que oferece

3.3. Lista deve estar ordenada alfabeticamente

3.4. Se não houver barbeiros cadastrados, sistema deve exibir mensagem: "Nenhum barbeiro disponível no momento"

3.5. Cliente deve poder clicar em barbeiro para ver horários disponíveis

### 4. Visualização de Horários Disponíveis

**O que faz**: Exibe horários livres de um barbeiro específico para que cliente escolha quando deseja ser atendido.

**Por que é importante**: Cliente precisa encontrar horário que se encaixe em sua agenda.

**Como funciona**:
- Cliente seleciona barbeiro
- Sistema exibe calendário/lista com horários disponíveis
- Cliente escolhe data e horário
- Cliente confirma agendamento

**Requisitos Funcionais:**

4.1. O sistema deve exibir calendário com próximos 30 dias

4.2. Para cada dia, sistema deve mostrar slots de horário disponíveis (ex: 09:00, 09:30, 10:00, etc.)

4.3. O sistema deve ocultar horários que já passaram

4.4. O sistema deve ocultar horários já ocupados por outros clientes

4.5. O sistema deve considerar duração do serviço ao calcular disponibilidade

4.6. Horários devem ser exibidos no fuso horário da barbearia

4.7. Cliente deve poder navegar entre dias do calendário

4.8. Sistema deve destacar visualmente horários disponíveis vs indisponíveis

4.9. Para MVP, não há regras complexas de disponibilidade (qualquer horário entre 8h e 20h é potencialmente disponível se não ocupado)

### 5. Criar Agendamento

**O que faz**: Permite que cliente crie um novo agendamento escolhendo barbeiro, serviço, data e horário.

**Por que é importante**: É a funcionalidade core que permite ao cliente reservar seu atendimento.

**Como funciona**:
- Cliente escolhe barbeiro
- Cliente escolhe serviço
- Cliente escolhe data e horário
- Cliente confirma agendamento
- Sistema cria agendamento e exibe confirmação

**Requisitos Funcionais:**

5.1. O processo de agendamento deve seguir fluxo:
   1. Escolher barbeiro
   2. Escolher serviço
   3. Escolher data e horário
   4. Confirmar agendamento

5.2. O sistema deve exibir resumo antes da confirmação:
   - Nome da barbearia
   - Barbeiro escolhido
   - Serviço escolhido
   - Data e horário
   - Duração estimada

5.3. Ao confirmar, sistema deve:
   - Validar que horário ainda está disponível (não foi ocupado por outro cliente)
   - Criar agendamento com status "Pendente"
   - Exibir confirmação de sucesso com detalhes

5.4. O sistema deve gerar identificador único para cada agendamento

5.5. Agendamento criado deve aparecer imediatamente na lista de agendamentos do cliente

5.6. O sistema deve bloquear o horário para outros clientes imediatamente

5.7. Se houver conflito (horário foi ocupado enquanto cliente estava escolhendo), sistema deve alertar e solicitar nova escolha

5.8. Cliente só pode ter um agendamento por vez com o mesmo barbeiro na mesma barbearia (não pode agendar 2 horários no mesmo dia com mesmo barbeiro)

### 6. Visualizar Meus Agendamentos

**O que faz**: Exibe lista de agendamentos futuros e passados do cliente na barbearia atual.

**Por que é importante**: Cliente precisa saber quando serão seus próximos atendimentos e ter histórico.

**Como funciona**:
- Cliente acessa seção "Meus Agendamentos"
- Sistema exibe agendamentos futuros primeiro
- Cliente pode visualizar detalhes de cada agendamento
- Cliente pode acessar histórico de atendimentos passados

**Requisitos Funcionais:**

6.1. O sistema deve separar visualização em duas abas/seções:
   - "Próximos" (agendamentos futuros)
   - "Histórico" (agendamentos passados/concluídos)

6.2. Seção "Próximos" deve exibir agendamentos com status Pendente ou Confirmado, ordenados por data/hora (mais próximo primeiro)

6.3. Seção "Histórico" deve exibir agendamentos Concluídos ou Cancelados, ordenados por data/hora (mais recente primeiro)

6.4. Cada agendamento deve mostrar:
   - Data e horário
   - Nome do barbeiro
   - Serviço
   - Status (Pendente/Confirmado/Concluído/Cancelado)
   - Duração

6.5. Cliente pode clicar em agendamento para ver detalhes completos

6.6. Se não houver agendamentos, sistema deve exibir mensagem: "Você ainda não tem agendamentos"

6.7. Agendamentos devem ser filtrados pela barbearia atual (isolamento)

### 7. Cancelar Agendamento

**O que faz**: Permite que cliente cancele um agendamento futuro.

**Por que é importante**: Cliente pode ter imprevistos e precisa liberar o horário.

**Como funciona**:
- Cliente acessa lista de agendamentos futuros
- Seleciona agendamento para cancelar
- Sistema exibe confirmação
- Cliente confirma cancelamento
- Sistema atualiza status e libera horário

**Requisitos Funcionais:**

7.1. O sistema deve permitir cancelamento apenas de agendamentos futuros (não passados)

7.2. O sistema deve permitir cancelamento de agendamentos com status Pendente ou Confirmado

7.3. O sistema deve exibir modal de confirmação: "Tem certeza que deseja cancelar este agendamento?"

7.4. Ao confirmar, sistema deve:
   - Atualizar status para "Cancelado"
   - Liberar horário para outros clientes
   - Registrar data/hora do cancelamento
   - Exibir mensagem de sucesso

7.5. Agendamento cancelado deve aparecer no histórico com indicação de cancelamento

7.6. Cancelamento não pode ser desfeito

7.7. Para MVP, não há limite de tempo para cancelamento (cliente pode cancelar até minutos antes)

### 8. Visualizar Histórico de Serviços

**O que faz**: Exibe todos os atendimentos concluídos do cliente na barbearia específica.

**Por que é importante**: Cliente pode querer revisar serviços anteriores para lembrar datas ou referência.

**Como funciona**:
- Cliente acessa seção "Histórico"
- Sistema exibe lista de atendimentos concluídos
- Cliente pode visualizar detalhes de cada serviço passado

**Requisitos Funcionais:**

8.1. O sistema deve exibir apenas agendamentos com status "Concluído" da barbearia atual

8.2. Lista deve estar ordenada do mais recente para o mais antigo

8.3. Cada item deve mostrar:
   - Data e horário do atendimento
   - Barbeiro que atendeu
   - Serviço realizado
   - Duração

8.4. O histórico é isolado por barbearia (não mostra atendimentos de outras barbearias)

8.5. Cliente pode clicar para ver detalhes completos de atendimento passado

8.6. Se não houver histórico, sistema deve exibir: "Você ainda não tem serviços realizados"

### 9. Acesso Multi-Barbearia Isolado

**O que faz**: Permite que cliente acesse diferentes barbearias de forma independente, cada uma com seu próprio cadastro, agendamentos e histórico.

**Por que é importante**: Cliente pode frequentar múltiplas barbearias (diferentes bairros, viagens, etc.) sem misturar informações.

**Como funciona**:
- Cliente acessa barbearia A via código específico
- Cliente se cadastra/loga na barbearia A
- Cliente usa sistema no contexto da barbearia A
- Para acessar barbearia B, cliente precisa acessar via código da barbearia B
- Dados são completamente isolados

**Requisitos Funcionais:**

9.1. Cada acesso à barbearia deve ser via URL com código específico

9.2. O sistema deve manter contexto da barbearia durante toda a sessão

9.3. Cliente não vê opção de "trocar de barbearia" dentro da aplicação (deve acessar via novo código/URL)

9.4. Dados do cliente são isolados por barbearia:
   - Agendamentos de barbearia A não aparecem quando logado em barbearia B
   - Histórico é separado por barbearia
   - Mesmo telefone pode ter nomes diferentes em barbearias diferentes (caso cliente prefira)

9.5. O sistema deve exibir sempre o nome da barbearia atual no header/interface

9.6. Não há visualização consolidada (cliente não vê todos seus agendamentos de todas as barbearias em um só lugar no MVP)

## Experiência do Usuário

### Persona: Cliente
- **Necessidades**: Processo de agendamento rápido e simples, clareza sobre horários disponíveis, facilidade de cancelamento
- **Contexto de Uso**: Acessa quando precisa agendar (1-2 vezes por mês), geralmente pelo smartphone
- **Nível Técnico**: Variado (básico a avançado) - interface deve ser intuitiva para todos
- **Dispositivos**: Primariamente smartphone (mobile-first essencial)

### Fluxos Principais

**Fluxo 1: Primeiro Acesso e Cadastro**
1. Cliente recebe código da barbearia (ex: WhatsApp, Instagram da barbearia)
2. Cliente acessa link: barbapp.com/XYZ123AB
3. Sistema valida código e exibe: "Bem-vindo à Barbearia XYZ!"
4. Sistema pergunta: "Já é cadastrado?" → Cliente escolhe "Não, quero me cadastrar"
5. Cliente preenche: Nome e Telefone
6. Sistema valida e cria cadastro
7. Cliente é automaticamente logado e vê página inicial da barbearia

**Fluxo 2: Login em Barbearia Conhecida**
1. Cliente acessa link da barbearia que já frequenta
2. Sistema identifica que telefone já está cadastrado
3. Cliente informa telefone e nome
4. Sistema valida e faz login
5. Cliente acessa dashboard da barbearia

**Fluxo 3: Criar Agendamento Completo**
1. Cliente está logado na barbearia
2. Clica em "Agendar Serviço" ou botão similar
3. Sistema exibe lista de barbeiros disponíveis
4. Cliente escolhe barbeiro "João Silva"
5. Sistema exibe lista de serviços: Corte, Barba, Corte + Barba
6. Cliente escolhe "Corte + Barba"
7. Sistema exibe calendário com próximos 30 dias
8. Cliente escolhe data: "Amanhã, 15/10"
9. Sistema exibe horários disponíveis: 09:00, 10:00, 11:00, 14:00, etc.
10. Cliente escolhe "10:00"
11. Sistema exibe resumo:
    - Barbearia XYZ
    - João Silva
    - Corte + Barba
    - 15/10/2025 às 10:00
    - Duração: 1h
12. Cliente clica "Confirmar Agendamento"
13. Sistema cria agendamento e exibe: "Agendamento realizado com sucesso!"
14. Cliente vê detalhes e botão "Ver meus agendamentos"

**Fluxo 4: Visualizar e Cancelar Agendamento**
1. Cliente acessa "Meus Agendamentos"
2. Sistema exibe aba "Próximos" com agendamento de amanhã às 10:00
3. Cliente clica no agendamento
4. Sistema exibe detalhes completos
5. Cliente clica "Cancelar Agendamento"
6. Sistema pergunta: "Tem certeza que deseja cancelar?"
7. Cliente confirma
8. Sistema cancela e exibe: "Agendamento cancelado"
9. Agendamento move para aba "Histórico" com status Cancelado

**Fluxo 5: Acessar Múltiplas Barbearias**
1. Cliente já é cadastrado na Barbearia A (código XYZ123)
2. Cliente descobre Barbearia B em outro bairro
3. Cliente recebe código da Barbearia B (código ABC456)
4. Cliente acessa: barbapp.com/ABC456
5. Sistema exibe: "Barbearia ABC - Novo cadastro"
6. Cliente se cadastra novamente (pode usar mesmo telefone)
7. Cliente agora tem acesso independente a ambas as barbearias
8. Para acessar Barbearia A novamente, usa link com código XYZ123

**Fluxo 6: Visualizar Histórico**
1. Cliente acessa "Meus Agendamentos"
2. Clica em aba "Histórico"
3. Sistema exibe lista de atendimentos concluídos:
   - 05/10/2025 - João Silva - Corte - Concluído
   - 20/09/2025 - Maria Souza - Barba - Concluído
   - 10/09/2025 - João Silva - Corte + Barba - Cancelado
4. Cliente pode clicar para ver detalhes de cada atendimento

### Requisitos de UI/UX

- **Mobile-First Obrigatório**: Interface otimizada primariamente para smartphones
- **Onboarding Simples**: Explicações claras em primeiro acesso
- **Navegação Intuitiva**: Máximo 3 cliques para criar agendamento
- **Feedback Visual Constante**: Cliente sempre sabe em que etapa está
- **Calendário Touch-Friendly**: Fácil navegação por datas em mobile
- **Código de Cores**:
  - Pendente: Amarelo
  - Confirmado: Verde
  - Concluído: Cinza
  - Cancelado: Vermelho
- **Informações Claras**: Nome da barbearia sempre visível
- **CTA Destacado**: Botão "Agendar Serviço" em evidência
- **Loading States**: Indicadores durante operações
- **Confirmações**: Feedback claro após todas as ações
- **Responsivo**: Funcionar bem em tablet e desktop também

### Considerações de Acessibilidade

Para o MVP não há requisitos específicos de acessibilidade, mas seguir boas práticas básicas:
- Contraste adequado de cores
- Textos legíveis (mínimo 14px em mobile)
- Botões com área de toque adequada (44x44px)
- Labels descritivos em formulários

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript (web responsiva, mobile-first)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (relacional)

### Arquitetura Multi-tenant
- **Requisito Crítico**: Isolamento total de dados por barbearia
- Código da barbearia define contexto de toda a sessão
- Todas as queries devem filtrar por barbearia atual
- Cliente pode ter cadastros independentes em múltiplas barbearias
- Zero vazamento de dados entre barbearias

### Autenticação
- Login por telefone + nome (sem senha no MVP)
- Validação por código SMS fica para Fase 2
- Sessão mantém contexto da barbearia
- Token JWT deve incluir: ID do cliente + ID da barbearia

### Integrações
- API REST para cadastro e login
- API REST para consulta de barbeiros e horários disponíveis
- API REST para criação e cancelamento de agendamentos
- API REST para consulta de histórico

### Performance
- Carregamento da página inicial < 2 segundos
- Calendário de disponibilidade < 3 segundos
- Criação de agendamento < 2 segundos
- Interface responsiva mesmo em 3G

### Segurança
- Cliente só acessa dados da barbearia atual
- Cliente só vê seus próprios agendamentos
- Validação de autorização em todas as requisições
- Proteção contra CSRF e ataques comuns
- LGPD: dados pessoais protegidos

### Dados e Privacidade
- Telefone é dado sensível (LGPD)
- Cliente pode se cadastrar em múltiplas barbearias com mesmo telefone
- Dados são isolados por barbearia (barbearia A não vê que cliente está em barbearia B)

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Validação por SMS**: Código de verificação por telefone fica para Fase 2
- **Senha**: Cliente não cria senha no MVP (apenas telefone + nome)
- **Notificações Push**: Lembretes e confirmações via push ficam para Fase 2
- **Notificações Email/SMS**: Confirmações por email/SMS ficam para Fase 2
- **Pagamento Online**: Sistema não processa pagamentos no MVP
- **Avaliações**: Cliente não pode avaliar barbeiro/serviço no MVP
- **Fotos**: Upload de foto de perfil fica fora do MVP
- **Preferências**: Sistema de preferências (barbeiro favorito, serviço preferido) fica para versão futura
- **Programa de Fidelidade**: Pontos, cashback, etc. ficam fora do MVP
- **Promoções**: Sistema de cupons/descontos fica para versão futura
- **Lista de Espera**: Se horário não disponível, entrar em lista de espera fica para versão futura
- **Reagendamento**: Alterar agendamento existente (MVP apenas cancelar e criar novo)
- **Agendamento Recorrente**: Agendar múltiplas datas de uma vez fica fora do MVP
- **Compartilhamento**: Compartilhar agendamento com outra pessoa fica fora do MVP
- **Chat**: Comunicação com barbearia/barbeiro fica para versão futura
- **Mapa/Localização**: Mostrar localização da barbearia no mapa fica para versão futura
- **Multi-idioma**: Suporte a outros idiomas é da Fase 3
- **Acessibilidade Avançada**: Leitores de tela, navegação por voz ficam para versão futura
- **Modo Offline**: Aplicação requer conexão constante no MVP

### Considerações Futuras (Pós-MVP)

- Validação por SMS (Fase 2)
- Notificações push e email (Fase 2)
- Sistema de pagamento online
- Avaliações e reviews
- Programa de fidelidade
- Sistema de promoções
- Reagendamento direto (sem cancelar)
- Lista de espera para horários ocupados
- Chat com barbearia
- Integração com mapas

## Questões em Aberto

### Questões de Negócio

1. **Política de Cancelamento**: Deve haver limite de tempo para cliente cancelar? (ex: não pode cancelar com menos de 2h de antecedência) Penalidade para cancelamentos frequentes?

2. **Confirmação de Barbeiro**: Agendamento requer confirmação do barbeiro ou é automaticamente confirmado?

3. **Múltiplos Agendamentos**: Cliente pode ter múltiplos agendamentos futuros na mesma barbearia? Ou apenas um por vez?

4. **No-Show**: Como tratar clientes que não comparecem? Há penalidade? Bloqueio temporário?

5. **Duração dos Serviços**: Quem define duração (barbearia, barbeiro)? É fixa ou variável?

### Questões Técnicas (para Tech Spec)

6. **Disponibilidade de Horários**: Como calcular slots disponíveis? Qual intervalo entre slots (30min, 1h)? Há horário de almoço fixo?

7. **Conflito de Agendamento**: Como garantir que dois clientes não agendem mesmo horário simultaneamente? Lock otimista ou pessimista?

8. **Persistência de Sessão**: Cliente precisa fazer login toda vez ou sessão persiste? Por quanto tempo?

9. **Cache de Disponibilidade**: Horários disponíveis devem ser cacheados ou sempre consultar banco?

10. **Nome Duplicado**: Se mesmo telefone está cadastrado com nomes diferentes em duas barbearias, como tratar no login?

### Questões de UX

11. **Visualização de Disponibilidade**: Mostrar apenas horários disponíveis ou mostrar todos os horários (com indisponíveis disabled)?

12. **Primeiro Acesso**: Como diferenciar claramente entre "Cadastrar" e "Login" para não confundir usuário?

13. **Onboarding**: Deve haver tutorial em primeiro acesso? Tour guiado?

14. **Confirmação de Barbeiro**: Como cliente saberá se agendamento foi confirmado pelo barbeiro? Deve aguardar confirmação ou já está garantido?

15. **Mudança de Barbearia**: Cliente pode ter confusão ao acessar múltiplas barbearias. Como deixar claro em qual barbearia está?

16. **Código da Barbearia**: Código deve ser case-sensitive? Aceitar hífens/espaços?

17. **Histórico Vazio**: Como incentivar primeiro agendamento quando cliente novo não tem histórico?

---

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunho para Revisão
