# PRD - UI Gestão de Barbeiros e Serviços (Admin da Barbearia)

## Visão Geral

Frontend voltado ao perfil Admin da Barbearia para gerenciar equipe de barbeiros, serviços oferecidos e visualizar a agenda consolidada dos profissionais da sua barbearia. O produto atende todas as barbearias ativas cadastradas no SaaS (multi-tenant), garantindo isolamento total entre barbearias. Baseia-se nos contratos já definidos em `docs/api-contracts-barbers-management.md` e no PRD "Gestão de Barbeiros (Admin da Barbearia)".

Valor para o negócio: dar autonomia operacional às barbearias, manter cadastros atualizados e garantir visão de agenda para coordenação diária, reduzindo esforço manual e erros.

## Objetivos

- Sucesso do usuário
  - Concluir cadastro de um barbeiro em < 1 minuto
  - Carregar a lista de barbeiros em < 1s
  - Carregar a agenda do dia em < 3s
- Métricas de produto
  - % de cadastros de barbeiros com campos obrigatórios completos = 100%
  - Taxa de erro de submissão de formulário < 2%
  - Tempo médio para localizar um serviço (busca/filtro) < 5s
- Objetivos de negócio
  - Autonomia do Admin da Barbearia para gerir equipe e serviços
  - Visibilidade da operação (agenda consolidada)
  - Garantir isolamento e conformidade multi-tenant

## Histórias de Usuário

Persona: Admin da Barbearia (proprietário/gerente) que gerencia equipe, serviços e acompanha a agenda.

Principais histórias:
- Como Admin da Barbearia, quero criar/editar/listar e ativar/inativar barbeiros para manter minha equipe atualizada.
- Como Admin da Barbearia, quero criar/editar/listar e ativar/inativar serviços para controlar o catálogo ofertado.
- Como Admin da Barbearia, quero visualizar a agenda de todos os barbeiros da minha barbearia para coordenar atendimentos.

Restrições de acesso (papéis e visões):
- Uma barbearia NUNCA pode visualizar qualquer informação de outra barbearia.
- Um barbeiro NUNCA pode visualizar a agenda de outro barbeiro (fora do escopo desta UI; reforça a necessidade de filtros e guarda de acesso por papel/tenant). 
- Esta UI é destinada ao Admin da Barbearia; não contempla o fluxo do barbeiro no MVP.

## Funcionalidades Principais

### 1) Gestão de Barbeiros (CRUD + ativar/inativar)

O que faz: permite ao Admin criar, editar, listar e ativar/inativar barbeiros vinculados à sua barbearia.

Por que é importante: mantém a equipe atualizada e controlada, habilitando agendamentos corretos.

Requisitos funcionais:
1.1. Deve existir listagem paginada e filtrável por nome e status (Ativo/Inativo/Todos).
1.2. Deve permitir criar barbeiro com campos obrigatórios: nome, email, telefone, serviços (lista de IDs) conforme contratos.
1.3. Deve validar: email válido, telefone brasileiro válido, nome até 100 caracteres.
1.4. Ao salvar com sucesso, exibir confirmação e refletir imediatamente na lista.
1.5. Deve permitir editar barbeiro (nome, telefone, serviços). O email não pode gerar conflito por barbearia.
1.6. Deve permitir ativar/inativar barbeiro. 
     - Observação: o contrato de API para alteração de status poderá ser definido na Tech Spec (toggle de `isActive` ou fluxo de remoção/reativação). 
1.7. A busca por barbeiros deve manter estado via URL (query params) para permitir compartilhamento/atualização de tela.
1.8. A UI deve exibir status, serviços associados e data de criação.
1.9. Erros de validação e negócio devem ser mostrados de forma clara (ex.: email duplicado).
1.10. Todas as operações devem respeitar o contexto da barbearia do Admin autenticado (tenant do token).

### 2) Gestão de Serviços (CRUD + ativar/inativar)

O que faz: permite ao Admin criar, editar, listar e ativar/inativar os serviços oferecidos pela sua barbearia.

Por que é importante: controla o catálogo disponível para agendamentos e define duração e preço.

Requisitos funcionais:
2.1. Deve existir listagem paginada e filtrável por nome e status (Ativo/Inativo/Todos).
2.2. Deve permitir criar serviço com campos obrigatórios: nome, descrição, duração (minutos), preço.
2.3. Deve validar: nome único por barbearia, duração positiva, preço não-negativo.
2.4. Ao salvar com sucesso, exibir confirmação e refletir imediatamente na lista.
2.5. Deve permitir editar serviço (nome, descrição, duração, preço).
2.6. Deve permitir ativar/inativar serviço. 
     - Observação: o contrato de API para alteração de status poderá ser definido na Tech Spec (toggle de `isActive` vs `DELETE`).
2.7. Deve exibir claramente o status (Ativo/Inativo) e possibilitar filtros combinados.
2.8. Todas as operações devem respeitar o contexto da barbearia do Admin autenticado.

### 3) Visualização da Agenda da Equipe

O que faz: exibe a agenda consolidada dos barbeiros da barbearia, com filtros por barbeiro, data e status do agendamento.

Por que é importante: dá visibilidade operacional para planejamento e coordenação do dia.

Requisitos funcionais:
3.1. Visualização padrão: agenda do dia atual, de todos os barbeiros, em modo lista ou calendário.
3.2. Cada agendamento deve mostrar: cliente, barbeiro, horário (início/fim), serviço, status.
3.3. Deve permitir filtros por barbeiro (ou todos), data (dia), período (semana opcional), status.
3.4. Deve permitir navegação por dias (anterior/próximo).
3.5. Atualização periódica: a agenda deve atualizar automaticamente por polling a cada 30s (conforme notas para frontend dos contratos).
3.6. Destaques visuais: horário atual; status confirmados/pendentes/cancelados diferenciados por cor/ícone.
3.7. No MVP, Admin apenas visualiza; não edita/confirmar/cancelar agendamentos.
3.8. Somente dados da barbearia do Admin autenticado podem ser apresentados.

### 4) Autenticação, Autorização e Contexto da Barbearia

O que faz: garante que apenas Admins da Barbearia autenticados acessem a UI e que toda interação respeite o tenant.

Requisitos funcionais:
4.1. Acesso somente com role `AdminBarbearia` via JWT. 
4.2. O contexto de barbearia deve ser derivado do token; a UI não deve solicitar `barbeariaId`.
4.3. Todas as chamadas devem incluir o token e seguir as políticas de autorização dos endpoints.
4.4. Ao detectar token inválido/expirado, a UI deve redirecionar para fluxo de autenticação.

### 5) Usabilidade, Feedbacks e Estados de Carregamento

Requisitos funcionais:
5.1. Formulários com validação em tempo real (exibição de erros junto aos campos).
5.2. Loading states visíveis em operações de carga/salvamento.
5.3. Mensagens de sucesso/erro consistentes e acessíveis.
5.4. Filtros com estado persistido (URL) e reset rápido.

## Experiência do Usuário

- Dispositivos alvo: mobile e tablet em prioridade (responsivo, touch-friendly), desktop suportado.
- Navegação proposta (alto nível):
  - Barbeiros: lista -> criar/editar (modal ou página dedicada)
  - Serviços: lista -> criar/editar (modal ou página dedicada)
  - Agenda: visão do dia (lista/calendário) com filtros
- Acessibilidade (MVP): contraste adequado, foco visível, navegação por teclado básica, labels claros.
- Conteúdo e idioma: pt-BR, labels e mensagens consistentes.

Fluxos principais (resumo):
- Criar Barbeiro: Lista -> Adicionar -> Preencher -> Salvar -> Confirmação -> Atualiza lista
- Criar Serviço: Lista -> Adicionar -> Preencher -> Salvar -> Confirmação -> Atualiza lista
- Visualizar Agenda: Acessar Agenda -> Ver dia atual -> Filtrar por barbeiro/status -> Navegar dias

## Restrições Técnicas de Alto Nível

- Integração: utilizar os endpoints descritos em `docs/api-contracts-barbers-management.md`.
- Segurança e LGPD: usar autenticação JWT; não exibir dados de outras barbearias; minimizar exposição de dados pessoais (telefone, email) e restringir apenas ao necessário para o Admin.
- Multi-tenant: todo dado exibido deve ser filtrado pelo tenant do token; proibir acesso cruzado.
- Performance: metas descritas em Objetivos; paginação obrigatória em listas.
- Observabilidade: logs de erro de UI e eventos críticos (ex.: falha ao salvar) deverão ser definidos na Tech Spec; notificações push estão fora do MVP.
- Conformidade com padrões do repositório (frontend): seguir `rules/react.md` e `rules/tests-react.md` (sem detalhar implementação neste PRD).

## Não-Objetivos (Fora de Escopo)

- Notificações (push/toast externas, e-mail/SMS) no MVP.
- Edição/gestão de agenda pelo Admin (confirmar/cancelar) no MVP.
- Gestão de horário de trabalho (disponibilidades) dos barbeiros no MVP.
- Relatórios/analytics de performance no MVP.
- Permissões granulares além de Admin da Barbearia (MVP mantém papéis simples).
- Upload de fotos de perfil no MVP.
- Integrações com calendários externos no MVP.

## Questões em Aberto

- Ativar/Inativar vs Remover: status de barbeiro/serviço será implementado via toggle `isActive` ou `DELETE`? Definição final na Tech Spec e/ou ajuste de API.
- Impacto em agendamentos: ao inativar/remover barbeiro/serviço, como lidar com agendamentos futuros já criados? (visual somente no MVP, mas mensagem/indicação na UI pode ser necessária).
- Visualização padrão da agenda: lista vs calendário como padrão? (este PRD admite ambos; decidir na Tech Spec/Design).
- Moeda e formatação de preço: padrão pt-BR (R$) e casas decimais; confirmar política de arredondamento.
- Onboarding do barbeiro: comunicação de primeiro acesso (fora do MVP) afeta textos/mensagens na UI?

---

Data de Criação: 2025-10-15  
Versão: 1.0 (MVP)  
Status: Para Revisão