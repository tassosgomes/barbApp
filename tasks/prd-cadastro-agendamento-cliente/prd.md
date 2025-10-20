# PRD - Cadastro e Agendamento (Cliente)

## Visão Geral

O módulo de Cadastro e Agendamento para Clientes permite que usuários finais agendem serviços em barbearias de forma simples e direta através de URL específica de cada estabelecimento. O cadastro do cliente acontece automaticamente no momento do primeiro agendamento, sendo necessário apenas Nome e Telefone. Clientes podem acessar seus agendamentos posteriormente usando apenas o telefone para login, visualizar histórico, cancelar ou editar agendamentos. Um cliente pode agendar em múltiplas barbearias de forma totalmente independente, com dados e históricos isolados por estabelecimento.

## Objetivos

- **Objetivo Principal**: Permitir que clientes agendem serviços em barbearias de forma simples, rápida e sem necessidade de cadastro prévio
- **Métricas de Sucesso**:
  - Tempo médio de agendamento completo < 3 minutos
  - Taxa de conclusão de agendamento > 85%
  - Taxa de cancelamento pelo cliente < 15%
  - 100% de isolamento entre dados de barbearias diferentes
  - Zero barreiras no primeiro agendamento (sem cadastro prévio)
- **Objetivos de Negócio**:
  - Eliminar atrito no primeiro agendamento (cadastro automático)
  - Facilitar acesso dos clientes aos serviços das barbearias
  - Permitir que clientes usem múltiplas barbearias sem confusão de dados
  - Aumentar taxa de conversão (visita -> agendamento)
  - Reduzir abandono por processo de cadastro complexo

## Histórias de Usuário

### Persona: Cliente
Usuário final que deseja agendar serviços de barbearia. Pode ser cliente regular ou ocasional, com preferência por um ou múltiplos estabelecimentos.

**Histórias Principais:**

- Como Cliente, eu quero **acessar uma barbearia através de URL direta** para iniciar um agendamento sem precisar me cadastrar primeiro
- Como Cliente, eu quero **visualizar serviços disponíveis da barbearia** para escolher o que desejo fazer
- Como Cliente, eu quero **escolher múltiplos serviços de uma vez** para fazer tudo em uma única visita
- Como Cliente, eu quero **escolher um barbeiro específico ou deixar o sistema escolher** para ter flexibilidade no agendamento
- Como Cliente, eu quero **visualizar horários disponíveis de um dia específico** para encontrar um horário que se encaixe na minha agenda
- Como Cliente, eu quero **agendar um serviço informando apenas nome e telefone** para completar o agendamento rapidamente sem cadastro prévio
- Como Cliente, eu quero **fazer login usando apenas meu telefone** para acessar meus agendamentos de forma simples
- Como Cliente, eu quero **visualizar meus agendamentos futuros em um dashboard** para saber quando e onde serão meus próximos atendimentos
- Como Cliente, eu quero **cancelar ou editar agendamentos** quando não puder comparecer ou precisar mudar
- Como Cliente, eu quero **visualizar meu histórico de serviços** em cada barbearia para ter registro dos atendimentos realizados
- Como Cliente, eu quero **acessar múltiplas barbearias de forma independente** através de URLs diferentes para poder frequentar diferentes estabelecimentos sem misturar minhas informações

**Casos de Uso Secundários:**

- Como Cliente, eu quero **receber confirmação visual após criar agendamento** para ter certeza que foi registrado
- Como Cliente, eu quero **acessar meu dashboard a partir da confirmação de agendamento** para ver detalhes ou fazer alterações
- Como Cliente, eu quero **ver detalhes completos do agendamento** (barbeiro, serviços, data/hora, duração total) antes de confirmar
- Como Cliente, eu quero **ver status do agendamento** (pendente/confirmado/concluído) para saber o estado atual

## Funcionalidades Principais

### 1. Acesso Direto via URL da Barbearia

**O que faz**: Permite que cliente acesse barbearia através de URL específica sem precisar digitar ou conhecer código.

**Por que é importante**: É o ponto de entrada do cliente no sistema, deve ser simples e transparente.

**Como funciona**:
- Cliente recebe URL da barbearia (ex: compartilhada em redes sociais, WhatsApp)
- Cliente acessa URL: `/barbearia/XYZ123AB`
- Sistema valida código e carrega página de agendamento daquela barbearia
- Cliente não precisa saber detalhes do código (é transparente)

**Requisitos Funcionais:**

1.1. O sistema deve aceitar código da barbearia via URL no formato `/barbearia/{codigo}`

1.2. O código é case-insensitive (XYZ123AB = xyz123ab)

1.3. O sistema deve validar se código existe e se barbearia está ativa

1.4. Se código inválido, sistema deve exibir mensagem de erro: "Barbearia não encontrada"

1.5. Se código válido, sistema deve carregar contexto da barbearia e manter durante toda a sessão

1.6. O sistema deve exibir nome da barbearia no header/interface

1.7. O código permanece transparente para o cliente (não precisa conhecer/memorizar)

1.8. URL pode ser compartilhada (link direto funciona sem login prévio)

### 2. Visualização de Serviços Disponíveis

**O que faz**: Exibe lista de serviços oferecidos pela barbearia para que cliente escolha o que deseja fazer.

**Por que é importante**: É a primeira etapa do fluxo de agendamento, define quais serviços serão realizados.

**Como funciona**:
- Cliente acessa URL da barbearia
- Sistema exibe lista de serviços disponíveis
- Cliente pode escolher um ou múltiplos serviços
- Sistema calcula duração total automaticamente

**Requisitos Funcionais:**

2.1. O sistema deve listar apenas serviços ativos da barbearia atual

2.2. Cada serviço deve exibir:
   - Nome do serviço
   - Descrição (opcional)
   - Duração estimada
   - Preço (opcional no MVP)

2.3. Lista deve estar ordenada por ordem de cadastro ou popularidade

2.4. Cliente pode selecionar múltiplos serviços

2.5. Sistema deve calcular e exibir duração total dos serviços selecionados

2.6. Se não houver serviços cadastrados, sistema deve exibir: "Nenhum serviço disponível no momento"

2.7. Cliente deve poder continuar para próxima etapa (escolha de barbeiro) após selecionar ao menos um serviço

### 3. Escolha de Barbeiro (ou Aleatório)

**O que faz**: Permite que cliente escolha um barbeiro específico ou deixe o sistema escolher aleatoriamente.

**Por que é importante**: Cliente pode ter preferência por profissional específico ou querer agilizar escolhendo "qualquer um".

**Como funciona**:
- Após escolher serviços, cliente vê lista de barbeiros
- Cliente pode selecionar barbeiro específico
- Ou cliente pode escolher opção "Qualquer um"
- Se escolher "Qualquer um", sistema sorteia barbeiro aleatoriamente

**Requisitos Funcionais:**

3.1. O sistema deve listar apenas barbeiros ativos da barbearia atual

3.2. Cada barbeiro deve exibir:
   - Nome
   - Foto (opcional)

3.3. Lista deve incluir opção destacada: "Qualquer barbeiro"

3.4. Se cliente escolher "Qualquer um":
   - Sistema sorteia barbeiro aleatoriamente entre os ativos
   - Sorteio deve ser feito no momento da confirmação do agendamento (não antes)

3.5. Se não houver barbeiros cadastrados, sistema deve exibir: "Nenhum barbeiro disponível no momento"

3.6. Cliente deve poder continuar para próxima etapa (escolha de data/horário)

**Nota MVP**: Sistema não verifica disponibilidade de barbeiro nesta etapa (qualquer barbeiro pode ser escolhido).

### 4. Seleção de Data e Horário

**O que faz**: Permite que cliente escolha dia e horário para o atendimento.

**Por que é importante**: Cliente precisa encontrar horário que se encaixe em sua agenda.

**Como funciona**:
- Cliente escolhe data (sistema sugere dia atual)
- Sistema exibe horários disponíveis para aquela data
- Cliente escolhe horário desejado
- Cliente avança para finalização

**Requisitos Funcionais:**

4.1. O sistema deve exibir seletor de data (calendário ou dropdown)

4.2. Data padrão sugerida deve ser o dia atual

4.3. Cliente pode escolher qualquer dia dos próximos 30 dias

4.4. Após escolher data, sistema exibe lista de horários disponíveis

4.5. Horários devem ser exibidos em intervalos de 30 minutos (ex: 09:00, 09:30, 10:00...)

4.6. O sistema deve ocultar horários que já passaram (se data escolhida for hoje)

4.7. Horários devem estar entre 08:00 e 20:00 (horário comercial padrão)

4.8. Sistema deve destacar visualmente horários disponíveis

4.9. **MVP Simplificado**: Sistema não verifica agenda real dos barbeiros (todos os horários são considerados disponíveis)

4.10. Cliente deve poder voltar e mudar data se desejar

4.11. Horários devem ser exibidos no fuso horário local da barbearia

**Nota MVP**: Validação de disponibilidade real de barbeiros será implementada em fase futura. MVP exibe todos os horários como disponíveis.

### 5. Finalização e Cadastro Automático

**O que faz**: Permite que cliente finalize agendamento informando nome e telefone, criando cadastro automaticamente se necessário.

**Por que é importante**: É a etapa final que concretiza o agendamento sem fricção de cadastro prévio.

**Como funciona**:
- Cliente revisa resumo do agendamento
- Cliente informa Nome e Telefone
- Sistema verifica se telefone já está cadastrado naquela barbearia
- Se não estiver, cria cadastro automaticamente
- Sistema cria agendamento e exibe confirmação

**Requisitos Funcionais:**

5.1. O sistema deve exibir resumo completo antes de solicitar dados:
   - Nome da barbearia
   - Serviço(s) escolhido(s)
   - Barbeiro (ou "Qualquer barbeiro")
   - Data e horário
   - Duração total estimada

5.2. O sistema deve solicitar:
   - **Nome**: campo texto livre (não valida nome completo)
   - **Telefone**: formato brasileiro (11) 98888-8888

5.3. O sistema deve validar formato do telefone

5.4. **Lógica de Cadastro Automático**:
   - Sistema verifica se telefone já existe na barbearia atual
   - Se SIM → usa cadastro existente, atualiza nome se diferente
   - Se NÃO → cria novo cliente com nome e telefone informados

5.5. O mesmo telefone pode estar cadastrado em múltiplas barbearias (isolamento)

5.6. Ao confirmar, sistema deve:
   - Criar/atualizar cliente
   - Se barbeiro escolhido foi "Qualquer um", sortear barbeiro aleatório neste momento
   - Criar agendamento com status "Pendente"
   - Gerar identificador único para o agendamento

5.7. O sistema deve exibir confirmação de sucesso com:
   - Mensagem: "Agendamento realizado com sucesso!"
   - Detalhes completos do agendamento
   - Número/código do agendamento
   - Botões: "Ver meus agendamentos" e "Fazer novo agendamento"

5.8. Cliente NÃO precisa fazer login para criar agendamento

5.9. Após confirmação, cliente pode optar por acessar dashboard (fazendo login com telefone)

**Nota**: Nome não precisa ser completo (aceita apenas primeiro nome). Validação por SMS vem em fase futura.

### 6. Login Simples (Apenas Telefone)

**O que faz**: Permite que cliente faça login usando apenas telefone para acessar dashboard com seus agendamentos.

**Por que é importante**: Cliente precisa acessar seus agendamentos, histórico e fazer cancelamentos/edições.

**Como funciona**:
- Cliente acessa URL da barbearia
- Cliente clica em "Meus Agendamentos" ou botão similar
- Cliente informa apenas telefone
- Sistema valida e faz login
- Cliente acessa dashboard

**Requisitos Funcionais:**

6.1. O sistema deve oferecer opção clara de "Meus Agendamentos" ou "Fazer Login" na página inicial

6.2. Tela de login deve solicitar apenas **Telefone**

6.3. O sistema deve validar que telefone está cadastrado naquela barbearia específica

6.4. Se telefone não encontrado, sistema exibe: "Telefone não cadastrado. Faça seu primeiro agendamento!"

6.5. Se telefone encontrado, sistema cria sessão e redireciona para dashboard

6.6. O sistema deve manter sessão do cliente (não precisa fazer login toda vez)

6.7. Cliente permanece no contexto da barbearia durante toda a sessão

6.8. **MVP**: Login usa apenas telefone (sem validação de nome ou senha)

6.9. **Futuro**: Validação por código SMS será adicionada em fase posterior

**Nota de Segurança MVP**: Sistema usa apenas telefone para login. Validação mais segura (SMS) será implementada em Fase 2.

### 7. Dashboard do Cliente

**O que faz**: Exibe dashboard com agendamentos futuros, histórico e opção de criar novo agendamento.

**Por que é importante**: Cliente precisa gerenciar seus agendamentos de forma centralizada.

**Como funciona**:
- Cliente faz login com telefone
- Sistema exibe dashboard com informações personalizadas
- Cliente pode ver agendamentos futuros, histórico, cancelar ou editar
- Cliente pode criar novo agendamento

**Requisitos Funcionais:**

7.1. Dashboard deve exibir:
   - Nome do cliente e telefone
   - Nome da barbearia (sempre visível no header)
   - Botão destacado: "Novo Agendamento"
   - Seção: "Próximos Agendamentos"
   - Seção: "Histórico"

7.2. **Seção "Próximos Agendamentos"**:
   - Lista agendamentos futuros (Pendente ou Confirmado)
   - Ordenados por data/hora (mais próximo primeiro)
   - Cada item mostra: Data, Horário, Barbeiro, Serviço(s), Status
   - Botões por agendamento: "Detalhes", "Cancelar", "Editar"

7.3. **Seção "Histórico"**:
   - Lista agendamentos passados (Concluído ou Cancelado)
   - Ordenados por data/hora (mais recente primeiro)
   - Cada item mostra: Data, Horário, Barbeiro, Serviço(s), Status
   - Botão: "Ver detalhes"

7.4. Se não houver agendamentos futuros:
   - Exibir mensagem: "Você não tem agendamentos futuros"
   - Destacar botão "Novo Agendamento"

7.5. Se não houver histórico:
   - Exibir mensagem: "Você ainda não tem serviços realizados"

7.6. Dashboard deve ter botão de "Sair" (logout)

7.7. Todos os dados são filtrados pela barbearia atual (isolamento)

7.8. Botão "Novo Agendamento" inicia fluxo de agendamento completo

### 8. Cancelar Agendamento

**O que faz**: Permite que cliente cancele um agendamento futuro a partir do dashboard.

**Por que é importante**: Cliente pode ter imprevistos e precisa liberar o horário.

**Como funciona**:
- Cliente acessa dashboard (logado)
- Seleciona agendamento futuro
- Clica em "Cancelar"
- Sistema exibe confirmação
- Cliente confirma cancelamento
- Sistema atualiza status e libera horário

**Requisitos Funcionais:**

8.1. O sistema deve permitir cancelamento apenas de agendamentos futuros

8.2. O sistema deve permitir cancelamento de agendamentos com status Pendente ou Confirmado

8.3. Botão "Cancelar" deve estar visível em cada agendamento futuro no dashboard

8.4. Ao clicar em "Cancelar", sistema deve exibir modal de confirmação:
   - "Tem certeza que deseja cancelar este agendamento?"
   - Mostrar detalhes do agendamento
   - Botões: "Sim, cancelar" e "Não, manter"

8.5. Ao confirmar cancelamento, sistema deve:
   - Atualizar status para "Cancelado"
   - Registrar data/hora do cancelamento
   - Mover agendamento para seção "Histórico"
   - Exibir mensagem: "Agendamento cancelado com sucesso"

8.6. Agendamento cancelado aparece no histórico com status "Cancelado"

8.7. Cancelamento não pode ser desfeito

8.8. **MVP**: Não há limite de tempo para cancelamento (cliente pode cancelar até minutos antes)

8.9. **Futuro**: Política de cancelamento com prazos será implementada em fase posterior

### 9. Editar Agendamento

**O que faz**: Permite que cliente altere detalhes de um agendamento futuro.

**Por que é importante**: Cliente pode precisar mudar horário, barbeiro ou serviços sem precisar cancelar e criar novo.

**Como funciona**:
- Cliente acessa dashboard (logado)
- Seleciona agendamento futuro
- Clica em "Editar"
- Sistema permite alterar: serviços, barbeiro, data/horário
- Cliente confirma alterações
- Sistema atualiza agendamento

**Requisitos Funcionais:**

9.1. O sistema deve permitir edição apenas de agendamentos futuros

9.2. O sistema deve permitir edição de agendamentos com status Pendente ou Confirmado

9.3. Botão "Editar" deve estar visível em cada agendamento futuro no dashboard

9.4. Ao clicar em "Editar", sistema abre fluxo de agendamento com dados pré-preenchidos:
   - Serviços já selecionados (pode adicionar/remover)
   - Barbeiro já selecionado (pode trocar)
   - Data e horário já selecionados (pode mudar)

9.5. Cliente pode alterar qualquer campo do agendamento

9.6. Sistema deve exibir resumo das alterações antes de confirmar

9.7. Ao confirmar, sistema deve:
   - Validar disponibilidade (se mudou horário)
   - Atualizar agendamento existente
   - Manter mesmo ID de agendamento
   - Exibir mensagem: "Agendamento atualizado com sucesso"

9.8. Agendamento editado continua na lista de "Próximos" com novos dados

9.9. Sistema registra data/hora da última edição

**Nota**: Cliente NÃO pode editar nome ou telefone do cadastro (apenas dados do agendamento).

### 10. Visualizar Histórico de Serviços

**O que faz**: Exibe todos os atendimentos concluídos e cancelados do cliente na barbearia específica.

**Por que é importante**: Cliente pode querer revisar serviços anteriores para lembrar datas ou referência.

**Como funciona**:
- Cliente acessa dashboard (logado)
- Visualiza seção "Histórico" automaticamente
- Sistema exibe lista de atendimentos passados
- Cliente pode clicar para ver detalhes

**Requisitos Funcionais:**

10.1. O sistema deve exibir agendamentos com status "Concluído" ou "Cancelado"

10.2. Lista deve estar ordenada do mais recente para o mais antigo

10.3. Cada item deve mostrar:
   - Data e horário
   - Barbeiro
   - Serviço(s) realizado(s)
   - Status (Concluído ou Cancelado)
   - Duração total

10.4. Cliente pode clicar em item para ver detalhes completos

10.5. Histórico é isolado por barbearia (não mostra atendimentos de outras barbearias)

10.6. Se não houver histórico, sistema exibe: "Você ainda não tem serviços realizados"

10.7. Histórico fica sempre visível no dashboard (não requer navegação extra)

10.8. **Futuro**: Cliente poderá reagendar serviço diretamente do histórico (repetir agendamento)

### 11. Acesso Multi-Barbearia Isolado

**O que faz**: Permite que cliente acesse diferentes barbearias através de URLs específicas, cada uma com cadastro, agendamentos e histórico independentes.

**Por que é importante**: Cliente pode frequentar múltiplas barbearias sem misturar informações.

**Como funciona**:
- Cliente acessa Barbearia A via `/barbearia/CODIGO_A`
- Faz agendamento/login na Barbearia A
- Para acessar Barbearia B, acessa `/barbearia/CODIGO_B`
- Dados são completamente isolados entre barbearias

**Requisitos Funcionais:**

11.1. Cada barbearia é acessada exclusivamente via URL específica

11.2. O sistema mantém contexto da barbearia durante toda a sessão

11.3. Nome da barbearia deve estar sempre visível no header/interface

11.4. **Isolamento de Dados**:
   - Agendamentos da Barbearia A não aparecem quando logado na Barbearia B
   - Histórico é separado por barbearia
   - Dashboard mostra apenas dados da barbearia atual
   - Mesmo telefone pode ter cadastros independentes em múltiplas barbearias

11.5. Cliente NÃO vê opção de "trocar de barbearia" dentro da aplicação

11.6. Para acessar outra barbearia, cliente deve usar URL específica daquela barbearia

11.7. Sistema não oferece visualização consolidada de todas as barbearias no MVP

11.8. Cada sessão é vinculada a uma barbearia específica

11.9. Logout em uma barbearia não afeta sessão em outras barbearias (sessões independentes)

**Exemplo de Uso**:
- Cliente mora em Bairro A e trabalha em Bairro B
- Frequenta Barbearia X (Bairro A) e Barbearia Y (Bairro B)
- Acessa `/barbearia/CODIGOX` para agendar na Barbearia X
- Acessa `/barbearia/CODIGOY` para agendar na Barbearia Y
- Cada barbearia tem seu próprio cadastro, agendamentos e histórico do cliente

## Experiência do Usuário

### Persona: Cliente
- **Necessidades**: Processo de agendamento rápido sem cadastro prévio, clareza sobre serviços e horários, facilidade de cancelamento/edição
- **Contexto de Uso**: Acessa quando precisa agendar (1-2 vezes por mês) ou gerenciar agendamentos, geralmente pelo smartphone
- **Nível Técnico**: Variado (básico a avançado) - interface deve ser intuitiva para todos
- **Dispositivos**: Primariamente smartphone (mobile-first essencial)
- **Motivação**: Quer agendar rápido sem complicações, não quer criar "mais uma conta"

### Fluxos Principais

**Fluxo 1: Primeiro Agendamento (Novo Cliente)**
1. Cliente recebe URL da barbearia (ex: WhatsApp, Instagram): `/barbearia/XYZ123AB`
2. Cliente clica no link
3. Sistema valida código e exibe página com nome da barbearia: "Barbearia do João"
4. Sistema exibe lista de serviços disponíveis:
   - ✂️ Corte Social (30min) - R$ 35
   - 🧔 Barba (20min) - R$ 25
   - ✂️🧔 Corte + Barba (50min) - R$ 55
5. Cliente seleciona "Corte + Barba" (pode selecionar múltiplos)
6. Sistema mostra duração total: "50 minutos" e avança
7. Sistema exibe lista de barbeiros:
   - João Silva
   - Pedro Santos
   - **Qualquer barbeiro** ⭐
8. Cliente seleciona "Qualquer barbeiro"
9. Sistema exibe seletor de data (padrão: hoje)
10. Cliente escolhe "Amanhã (20/10)"
11. Sistema exibe horários disponíveis: 09:00, 09:30, 10:00, 10:30, ..., 19:30
12. Cliente escolhe "14:00"
13. Sistema exibe resumo:
    - 📍 Barbearia do João
    - ✂️🧔 Corte + Barba
    - 👤 Qualquer barbeiro
    - 📅 20/10/2025 às 14:00
    - ⏱️ Duração: 50 minutos
14. Sistema solicita: "Para confirmar, precisamos de alguns dados:"
    - Nome: [____]
    - Telefone: [(11) _____-____]
15. Cliente preenche: "Carlos" e "(11) 98765-4321"
16. Cliente clica "Confirmar Agendamento"
17. Sistema:
    - Verifica que telefone não existe → cria novo cliente
    - Sorteia barbeiro aleatório: "João Silva"
    - Cria agendamento #12345
18. Sistema exibe confirmação:
    - ✅ "Agendamento realizado com sucesso!"
    - Agendamento #12345
    - João Silva
    - 20/10/2025 às 14:00
    - Corte + Barba
    - Botões: [Ver meus agendamentos] [Novo agendamento]

**Fluxo 2: Login e Dashboard**
1. Cliente (já cadastrado) acessa URL da barbearia
2. Cliente clica em "Meus Agendamentos" (botão no header)
3. Sistema exibe tela de login: "Acesse seus agendamentos"
4. Cliente informa telefone: "(11) 98765-4321"
5. Sistema valida telefone e faz login
6. Sistema exibe dashboard:
   ```
   Olá, Carlos!
   Barbearia do João
   
   [+ Novo Agendamento]
   
   📅 Próximos Agendamentos
   ┌─────────────────────────────┐
   │ 20/10/2025 - 14:00         │
   │ João Silva                  │
   │ Corte + Barba              │
   │ Status: Pendente           │
   │ [Detalhes] [Editar] [Cancelar] │
   └─────────────────────────────┘
   
   📜 Histórico
   └─ Você ainda não tem serviços realizados
   
   [Sair]
   ```

**Fluxo 3: Novo Agendamento (Cliente Já Cadastrado)**
1. Cliente está logado no dashboard
2. Clica em "+ Novo Agendamento"
3. Sistema inicia fluxo de agendamento (igual ao Fluxo 1, passos 4-13)
4. Cliente escolhe serviços, barbeiro, data e horário
5. Sistema exibe resumo (sem solicitar nome/telefone, pois já está logado)
6. Cliente confirma
7. Sistema cria agendamento e volta ao dashboard

**Fluxo 4: Cancelar Agendamento**
1. Cliente está no dashboard vendo "Próximos Agendamentos"
2. Cliente clica em "Cancelar" no agendamento #12345
3. Sistema exibe modal:
   ```
   ⚠️ Cancelar Agendamento?
   
   20/10/2025 às 14:00
   João Silva - Corte + Barba
   
   Tem certeza que deseja cancelar?
   
   [Não, manter] [Sim, cancelar]
   ```
4. Cliente clica "Sim, cancelar"
5. Sistema:
   - Atualiza status para "Cancelado"
   - Move para "Histórico"
   - Exibe mensagem: "Agendamento cancelado com sucesso"
6. Dashboard atualiza automaticamente

**Fluxo 5: Editar Agendamento**
1. Cliente está no dashboard vendo "Próximos Agendamentos"
2. Cliente clica em "Editar" no agendamento #12345
3. Sistema abre fluxo de agendamento com dados pré-preenchidos:
   - Serviços: ✅ Corte + Barba
   - Barbeiro: Qualquer barbeiro
   - Data: 20/10/2025
   - Horário: 14:00
4. Cliente muda horário para "15:00"
5. Cliente confirma alterações
6. Sistema atualiza agendamento
7. Sistema exibe: "Agendamento atualizado com sucesso"
8. Cliente volta ao dashboard com dados atualizados

**Fluxo 6: Acessar Múltiplas Barbearias**
1. Cliente frequenta "Barbearia do João" (Bairro A)
2. Cliente recebe link de "Barbearia da Maria" (Bairro B): `/barbearia/ABC789XY`
3. Cliente acessa novo link
4. Sistema detecta nova barbearia (contexto diferente)
5. Cliente faz primeiro agendamento na "Barbearia da Maria" (Fluxo 1)
6. Sistema cria cadastro independente (mesmo telefone, nova barbearia)
7. Cliente agora pode acessar ambas as barbearias via seus respectivos links
8. Cada dashboard mostra apenas dados daquela barbearia específica

### Requisitos de UI/UX

- **Mobile-First Obrigatório**: Interface otimizada primariamente para smartphones
- **Zero Fricção**: Agendamento sem necessidade de cadastro prévio
- **Onboarding Invisível**: Cliente não percebe que está sendo cadastrado
- **Navegação Linear**: Fluxo de agendamento sequencial e claro (serviços → barbeiro → data/hora → confirmar)
- **Máximo 6 Telas**: Do acesso à confirmação do agendamento
- **Feedback Visual Constante**: Cliente sempre sabe em que etapa está do processo
- **Progressão Clara**: Indicador visual de progresso (ex: 1/4, 2/4, 3/4, 4/4)
- **Botão Primário Sempre Visível**: CTA principal sempre acessível
- **Código de Cores Consistente**:
  - Pendente: Amarelo/Laranja
  - Confirmado: Verde
  - Concluído: Cinza/Azul
  - Cancelado: Vermelho
- **Nome da Barbearia Sempre Visível**: Header fixo com nome do estabelecimento
- **CTAs Descritivos**: 
  - "Escolher serviços" em vez de "Próximo"
  - "Escolher barbeiro" em vez de "Continuar"
  - "Confirmar agendamento" em vez de "Finalizar"
- **Loading States**: Indicadores durante todas as operações assíncronas
- **Confirmações Claras**: Feedback visual após todas as ações importantes
- **Botão "Voltar" Sempre Presente**: Cliente pode voltar em qualquer etapa do fluxo
- **Responsivo**: Funcionar bem em mobile, tablet e desktop
- **Toque Otimizado**: Botões com área mínima de 44x44px
- **Cards para Seleção**: Serviços e barbeiros em cards clicáveis (não dropdowns)
- **Calendário Touch-Friendly**: Fácil navegação por datas em mobile
- **Dashboard Limpo**: Informações organizadas em cards/seções claras
- **Empty States**: Mensagens amigáveis quando não há dados ("Seu primeiro agendamento está a poucos cliques!")

### Considerações de Acessibilidade

Para o MVP, seguir boas práticas básicas:
- Contraste adequado de cores (mínimo WCAG AA)
- Textos legíveis (mínimo 16px em mobile)
- Botões com área de toque adequada (44x44px)
- Labels descritivos em todos os formulários
- Navegação por teclado funcional (desktop)
- Estados de foco visíveis
- Textos alternativos em ícones importantes

**Futuro**: WCAG AAA, leitores de tela, alto contraste, redução de movimento

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
   - **MVP**: Sem limite de tempo

2. **Confirmação de Barbeiro**: Agendamento é automaticamente confirmado ou requer confirmação do barbeiro/admin?
   - **Decisão Necessária**: Status inicial é "Pendente" ou "Confirmado"?

3. **Múltiplos Agendamentos**: Cliente pode ter múltiplos agendamentos futuros na mesma barbearia? Ou apenas um por vez?
   - **Decisão Necessária**: Limitar agendamentos simultâneos?

4. **No-Show**: Como tratar clientes que não comparecem? Há penalidade? Bloqueio temporário?
   - **MVP**: Sem controle de no-show

5. **Preço dos Serviços**: Sistema exibe preços? É obrigatório? Soma total ao final?
   - **Decisão Necessária**: Mostrar preços no MVP?

6. **Duração Variável**: Serviços têm duração fixa ou barbeiro pode ajustar?
   - **MVP**: Duração fixa por serviço

### Questões Técnicas (para Tech Spec)

7. **Seleção Aleatória de Barbeiro**: 
   - Quando exatamente sortear? (no momento da confirmação ou ao escolher data/hora?)
   - Critérios de sorteio? (todos iguais? peso por especialidade?)
   - **Definido MVP**: Sorteio no momento da confirmação, todos barbeiros ativos têm mesma chance

8. **Horários Disponíveis (MVP Simplificado)**:
   - MVP não valida agenda real de barbeiros
   - Todos os horários 08:00-20:00 são exibidos como disponíveis
   - **Futuro**: Validação real de disponibilidade por barbeiro

9. **Conflito de Agendamento**: 
   - Como garantir que dois clientes não agendem mesmo horário com mesmo barbeiro simultaneamente?
   - Lock otimista ou pessimista?
   - **Decisão Necessária**: Estratégia de concorrência

10. **Persistência de Sessão**: 
    - Cliente precisa fazer login toda vez ou sessão persiste?
    - Por quanto tempo? (localStorage, cookie, sessão?)
    - **Sugestão**: localStorage com token JWT, 30 dias de validade

11. **Atualização de Nome**: 
    - Se telefone já existe mas nome é diferente, atualizar ou manter original?
    - **Sugestão**: Manter primeiro nome cadastrado, não atualizar

12. **Múltiplos Serviços**:
    - Como calcular horário de término? (soma das durações)
    - Validar se horário de fim não ultrapassa horário de fechamento (20:00)?
    - **Decisão Necessária**: Lógica de validação

### Questões de UX

13. **Visualização de Horários**: Mostrar apenas slots disponíveis ou todos (com indisponíveis disabled)?
   - **MVP**: Mostrar todos como disponíveis (simplificado)

14. **Indicador de Progresso**: Usar steps (1/4, 2/4...) ou breadcrumbs ou ambos?
   - **Decisão Necessária**: Padrão visual

15. **Confirmação Imediata**: Após confirmação, mostrar dashboard ou apenas mensagem de sucesso com opção de acessar?
   - **Sugestão**: Mostrar confirmação + 2 botões: "Ver meus agendamentos" e "Novo agendamento"

16. **Login vs Agendamento**: Como diferenciar claramente acesso ao dashboard vs novo agendamento?
   - **Sugestão**: Página inicial com 2 CTAs: "Agendar Agora" (primário) e "Meus Agendamentos" (secundário)

17. **Edição de Cadastro**: Cliente pode editar nome/telefone no dashboard?
   - **MVP**: Não permite edição de dados pessoais
   - **Futuro**: Permitir com validação

18. **Seleção de Múltiplos Serviços**: UI de checkbox ou cards selecionáveis? Limite máximo?
   - **Sugestão**: Cards com checkbox, sem limite no MVP

19. **Empty State Histórico**: Como incentivar segundo agendamento quando histórico está vazio?
   - **Sugestão**: Mensagem amigável + botão "Agendar agora"

20. **URL Amigável**: Além de `/barbearia/CODIGO`, oferecer slug? (ex: `/barbearia/nome-da-barbearia`)
   - **MVP**: Apenas código
   - **Futuro**: Slug opcional

---

**Decisões Pendentes Críticas para Tech Spec:**
1. Status inicial do agendamento (Pendente ou Confirmado?)
2. Limites de agendamentos simultâneos por cliente
3. Exibição de preços
4. Estratégia de concorrência para evitar double-booking
5. Persistência e duração de sessão
6. Validação de horário de fim com múltiplos serviços

---

**Data de Criação**: 2025-10-10  
**Data de Atualização**: 2025-10-19  
**Versão**: 2.0  
**Status**: Revisado - Aguardando Decisões de Negócio
