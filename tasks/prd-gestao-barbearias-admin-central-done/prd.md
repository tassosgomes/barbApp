# PRD - Gestão de Barbearias (Admin Central)

## Visão Geral

O módulo de Gestão de Barbearias permite que o Admin Central do barbApp crie, edite, visualize e exclua barbearias no sistema. Cada barbearia criada recebe um código único que será usado pelos seus clientes para acessar seus serviços de forma isolada. Este é o primeiro ponto de entrada para onboarding de novas barbearias no sistema SaaS multi-tenant, garantindo que cada estabelecimento tenha sua própria instância isolada com dados e operações independentes.

## Objetivos

- **Objetivo Principal**: Permitir o cadastro e gerenciamento completo de barbearias no sistema, habilitando o modelo SaaS multi-tenant
- **Métricas de Sucesso**:
  - Tempo médio de cadastro de uma nova barbearia < 2 minutos
  - Taxa de sucesso no cadastro > 95%
  - 100% das barbearias criadas com código único gerado automaticamente
- **Objetivos de Negócio**:
  - Facilitar onboarding rápido de novos clientes (barbearias)
  - Manter base de dados organizada e atualizada
  - Garantir isolamento completo entre barbearias desde a criação

## Histórias de Usuário

### Persona: Admin Central
Responsável técnico/operacional do barbApp que gerencia todas as barbearias clientes do sistema.

**Histórias Principais:**

- Como Admin Central, eu quero **criar uma nova barbearia com seus dados básicos** para que ela possa começar a usar o sistema imediatamente com seu código único de acesso
- Como Admin Central, eu quero **editar informações das barbearias** para que os dados cadastrais estejam sempre corretos e atualizados
- Como Admin Central, eu quero **visualizar a lista de todas as barbearias** com filtros de busca para que eu possa encontrar e gerenciar estabelecimentos específicos rapidamente
- Como Admin Central, eu quero **excluir barbearias com confirmação** para que possa remover clientes que não usam mais o sistema, evitando exclusões acidentais

**Casos de Uso Secundários:**

- Como Admin Central, eu quero **visualizar o código único de cada barbearia** para que possa compartilhá-lo com os proprietários quando necessário
- Como Admin Central, eu quero **ver informações resumidas de cada barbearia** (data de criação, status) para ter visibilidade do estado geral da base de clientes

## Funcionalidades Principais

### 1. Criação de Barbearia

**O que faz**: Permite cadastrar uma nova barbearia no sistema com geração automática de código único.

**Por que é importante**: É o ponto de entrada para novos clientes no sistema SaaS. O código único gerado é essencial para o isolamento multi-tenant.

**Como funciona**:
- Admin Central acessa o painel administrativo
- Preenche formulário com dados básicos da barbearia
- Sistema gera automaticamente um código único alfanumérico
- Barbearia é criada e código é exibido para compartilhamento

**Requisitos Funcionais:**

1.1. O sistema deve permitir cadastro de barbearia com os seguintes campos obrigatórios:
   - Nome da barbearia
   - CNPJ ou CPF (validação de formato)
   - Telefone de contato
   - Endereço completo (CEP, rua, número, complemento, bairro, cidade, estado)
   - Nome do proprietário
   - Email de contato

1.2. O sistema deve gerar automaticamente um código único de 8 caracteres alfanuméricos (maiúsculas, sem caracteres ambíguos: O, 0, I, 1) para cada barbearia criada

1.3. O código gerado deve ser validado como único no banco de dados antes da criação

1.4. Após criação bem-sucedida, o sistema deve exibir mensagem de confirmação com o código gerado em destaque

1.5. O sistema deve validar formato de CNPJ, CPF, email e telefone antes de permitir o cadastro

1.6. Todos os campos obrigatórios devem ser validados (não vazios, formatos corretos)

### 2. Edição de Barbearia

**O que faz**: Permite atualizar dados cadastrais de uma barbearia existente.

**Por que é importante**: Mantém as informações atualizadas conforme mudanças nos estabelecimentos.

**Como funciona**:
- Admin Central acessa lista de barbearias
- Seleciona barbearia para editar
- Modifica campos desejados
- Sistema valida e salva alterações

**Requisitos Funcionais:**

2.1. O sistema deve permitir edição de todos os campos cadastrais exceto o código único da barbearia

2.2. O sistema deve pré-preencher formulário de edição com dados atuais

2.3. O sistema deve validar os campos com as mesmas regras da criação

2.4. O sistema deve exibir mensagem de confirmação após atualização bem-sucedida

2.5. O sistema deve registrar data/hora da última modificação

2.6 Não permitir alterar CNPJ ou CPF após criação


### 3. Visualização e Listagem de Barbearias

**O que faz**: Exibe lista paginada de todas as barbearias com recursos de busca e filtros.

**Por que é importante**: Facilita localização e gestão de barbearias específicas em uma base crescente.

**Como funciona**:
- Admin Central acessa painel principal
- Sistema exibe lista paginada com informações resumidas
- Admin pode buscar por nome, código ou CNPJ/CPF
- Admin pode filtrar por status ou ordenar resultados

**Requisitos Funcionais:**

3.1. O sistema deve exibir lista de barbearias com as seguintes informações visíveis:
   - Nome da barbearia
   - Código único
   - Cidade/Estado
   - Data de criação
   - Status (Ativa/Inativa)

3.2. O sistema deve implementar paginação com 20 itens por página

3.3. O sistema deve permitir busca textual por: nome da barbearia, código único ou CNPJ

3.4. O sistema deve permitir filtro por status (Ativa/Inativa/Todas)

3.5. O sistema deve permitir ordenação por: nome, data de criação (mais recente/antiga)

3.6. A busca deve ser case-insensitive e aceitar buscas parciais

### 4. Exclusão de Barbearia

**O que faz**: Remove uma barbearia do sistema após confirmação explícita.

**Por que é importante**: Mantém base de dados limpa removendo clientes inativos, com salvaguardas contra exclusão acidental.

**Como funciona**:
- Admin Central seleciona barbearia para excluir
- Sistema exibe modal de confirmação com aviso sobre consequências
- Admin confirma exclusão
- Sistema remove barbearia e todos os dados relacionados

**Requisitos Funcionais:**

4.1. O sistema deve exigir confirmação explícita antes de executar exclusão

4.2. O modal de confirmação deve exibir:
   - Nome da barbearia
   - Código único
   - Aviso: "Esta ação é irreversível e removerá todos os dados relacionados"

4.3. O sistema deve implementar exclusão em cascata de todos os dados relacionados:
   - Barbeiros vinculados
   - Agendamentos
   - Histórico de serviços
   - Clientes cadastrados na barbearia

4.4. O sistema deve exibir mensagem de sucesso após exclusão

4.5. O sistema deve remover a barbearia imediatamente da listagem após exclusão

## Experiência do Usuário

### Persona: Admin Central
- **Necessidades**: Interface administrativa eficiente, visibilidade clara da base de barbearias, operações rápidas
- **Contexto de Uso**: Acessa diariamente para onboarding de novos clientes e manutenção de cadastros
- **Nível Técnico**: Intermediário a avançado

### Fluxos Principais

**Fluxo 1: Cadastro de Nova Barbearia**
1. Admin acessa painel administrativo
2. Clica em "Nova Barbearia"
3. Preenche formulário com dados básicos
4. Clica em "Criar Barbearia"
5. Sistema valida, gera código e exibe confirmação com código em destaque
6. Admin copia código para compartilhar com proprietário da barbearia

**Fluxo 2: Busca e Edição**
1. Admin acessa lista de barbearias
2. Utiliza busca ou filtros para localizar barbearia específica
3. Clica no botão de editar na linha da barbearia
4. Modifica dados necessários
5. Salva alterações e recebe confirmação

**Fluxo 3: Exclusão Segura**
1. Admin localiza barbearia na lista
2. Clica no botão de excluir
3. Sistema exibe modal de confirmação com avisos
4. Admin confirma exclusão digitando confirmação ou clicando em botão
5. Sistema remove dados e atualiza listagem

### Requisitos de UI/UX

- Interface deve ser clean e focada em produtividade
- Formulários devem ter validação em tempo real com feedback claro
- Código único gerado deve ser facilmente copiável (botão "Copiar")
- Lista deve ser responsiva e funcionar bem em tablets
- Mensagens de erro devem ser específicas e orientar correção
- Confirmações de ações destrutivas devem ser visuais e explícitas
- Loading states durante operações assíncronas

### Considerações de Acessibilidade

Para o MVP não há requisitos específicos de acessibilidade, mas seguir boas práticas básicas:
- Labels semânticos em formulários
- Contraste adequado de cores
- Navegação por teclado funcional

## Restrições Técnicas de Alto Nível

### Stack Tecnológica
- **Frontend**: React + Vite + TypeScript (web responsiva)
- **Backend**: .NET 8 (API REST)
- **Banco de Dados**: PostgreSQL (relacional)

### Integrações
- Sistema de geração de código único deve ser implementado no backend
- API REST deve expor endpoints para CRUD completo de barbearias

### Segurança
- Acesso ao painel administrativo deve ser restrito e autenticado
- Códigos únicos devem ser gerados de forma segura (sem colisões)
- Exclusão de barbearia deve implementar soft delete ou hard delete (definir na Tech Spec)

### Performance
- Listagem de barbearias deve responder em < 2 segundos para até 1000 registros
- Busca e filtros devem ser eficientes com índices apropriados no banco

### Dados e Privacidade
- CNPJ deve ser armazenado de forma validada
- Dados de contato devem ser protegidos conforme LGPD
- Exclusão deve considerar implicações de dados pessoais (LGPD)

## Não-Objetivos (Fora de Escopo)

### Explicitamente Excluído do MVP

- **Gestão de Planos e Pagamentos**: Admin Central não gerencia assinaturas ou pagamentos no MVP
- **Comunicação Direta com Barbeiros/Clientes**: Admin Central não interage com usuários finais das barbearias
- **Notificações Administrativas**: Sistema de alertas fica para Fase 2
- **Relatórios e Analytics**: Dashboards analíticos são da Fase 3
- **Customização de Barbearias**: Branding e configurações personalizadas são da Fase 3
- **Importação em Massa**: Cadastro múltiplo de barbearias fica para versões futuras
- **Auditoria Detalhada**: Logs avançados de operações são da Fase 3
- **Status Avançados**: MVP terá apenas Ativa/Inativa

### Considerações Futuras (Pós-MVP)

- Sistema de notificações para Admin Central (Fase 2)
- Relatórios de uso e métricas por barbearia (Fase 3)
- Gestão de planos e faturamento (Fase 3)
- Suporte a múltiplos idiomas (Fase 3)
- Importação/exportação de dados
- API pública para integrações externas

## Questões em Aberto

### Questões de Negócio

1. **Política de Exclusão**: Qual é a política para exclusão de barbearias com dados existentes? Devemos implementar soft delete ou hard delete? Há período de retenção?

2. **Reativação**: Se uma barbearia for excluída, é possível reativá-la depois? Ou seria necessário novo cadastro?

3. **Limite de Barbearias**: Há limite de barbearias que podem ser cadastradas no sistema (para MVP)?

4. **Código Único**: O código pode ser customizado pelo Admin Central ou sempre será gerado automaticamente?

### Questões Técnicas (para Tech Spec)

5. **Validação de CNPJ**: Devemos apenas validar formato ou também consultar API da Receita Federal?

6. **Unicidade de CNPJ**: Devemos impedir cadastro de mesmo CNPJ duas vezes?

7. **Status Inicial**: Toda barbearia é criada como "Ativa" por padrão?

8. **Campos Opcionais**: Há campos adicionais opcionais que seriam úteis (logo, horário de funcionamento, descrição)?

### Questões de UX

9. **Compartilhamento do Código**: Como o Admin Central compartilhará o código com o proprietário da barbearia? Email automático? Manual?

10. **Visualização de Código**: O código deve ser visível na listagem ou apenas no detalhe/edição?

---

**Data de Criação**: 2025-10-10  
**Versão**: 1.0  
**Status**: Rascunho para Revisão
