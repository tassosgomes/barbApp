# PRD - Telas do Admin Central para Gestão de Barbearias

## Visão Geral

Este PRD define as telas e comportamentos de UI/UX do Admin Central para realizar o trabalho já especificado nos documentos de Gestão de Barbearias: criar, editar, listar, pesquisar, filtrar, desativar e reativar barbearias. O objetivo é disponibilizar uma interface administrativa clara, eficiente e segura para o Admin Central operar o ciclo de vida das barbearias no modelo SaaS multi-tenant do barbApp. Este documento foca no que o usuário precisa fazer (O QUÊ e POR QUÊ) por meio das telas, sem prescrever implementações técnicas.

## Objetivos

- Disponibilizar uma interface web responsiva para o Admin Central gerenciar barbearias (criar, editar, desativar/reativar).
- Prover listagem com busca, filtros, ordenação e paginação para operação eficiente.
- Tornar o código único da barbearia facilmente visível e copiável.
- Minimizar erros com validações, feedbacks e confirmações claras.
- Nota: Métricas de sucesso não são necessárias neste PRD (por solicitação).

## Histórias de Usuário

- Como Admin Central, eu quero criar uma nova barbearia preenchendo seus dados para que ela comece a usar o sistema com um código único.
- Como Admin Central, eu quero ver uma lista de barbearias com busca e filtros para localizar estabelecimentos rapidamente.
- Como Admin Central, eu quero editar dados de uma barbearia existente para manter o cadastro atualizado.
- Como Admin Central, eu quero desativar (e futuramente reativar) uma barbearia com confirmação para evitar ações acidentais.
- Como Admin Central, eu quero copiar o código único de uma barbearia com 1 clique para compartilhar com o proprietário.
- Como Admin Central, eu quero ver o detalhe resumido de uma barbearia (incluindo status e endereço) para confirmar informações antes de agir.

## Funcionalidades Principais

### 1. Listagem e Busca de Barbearias (Tela Principal)

O que faz: Exibe uma lista paginada de barbearias com busca, filtros, ordenação e ações por item.

Por que é importante: É a vista de trabalho diária do Admin Central para localizar, revisar e operar barbearias com agilidade.

Como funciona (alto nível): Tabela com colunas-chave e ações contextuais; topo com busca e filtros; paginação no rodapé.

Requisitos Funcionais:
1.1. Exibir tabela com colunas: Nome, Código, Cidade/UF, Status (Ativa/Inativa), Data de Criação, Ações.
1.2. Permitir busca textual case-insensitive por Nome, Código ou Documento (CNPJ/CPF) via campo único.
1.3. Permitir filtro por Status (Todas, Ativas, Inativas).
1.4. Permitir ordenação por Nome e Data de Criação (asc/desc), com indicador de coluna ordenada.
1.5. Implementar paginação com padrão 20 itens por página (controles de próxima/anterior e indicador de página).
1.6. Exibir ação “Editar” e ação de estado “Desativar” (se ativa) ou “Reativar” (se inativa) em cada linha.
1.7. Exibir ação “Copiar Código” por item, com confirmação visual de cópia bem-sucedida.
1.8. Exibir estado de carregamento (skeleton/loader) durante consultas e mensagem de vazio quando não houver resultados.
1.9. Preservar parâmetros de busca/filtro/ordenar/página ao navegar entre telas e retornar à listagem.
1.10. Exibir badges de Status com diferenciação visual clara (por exemplo: verde “Ativa”, cinza “Inativa”).

### 2. Cadastro de Barbearia (Tela)

O que faz: Permite cadastrar uma barbearia com dados obrigatórios e gerar/exibir o código único.

Por que é importante: Onboarding de novos clientes no sistema multi-tenant depende deste fluxo.

Como funciona (alto nível): Ação “Nova Barbearia” abre tela/etapa com formulário; após salvar com sucesso, mostra confirmação e código.

Requisitos Funcionais:
2.1. Campo obrigatórios: Nome da Barbearia, Documento (CNPJ ou CPF), Telefone de Contato, Email, Proprietário, Endereço (CEP, Rua, Número, Complemento opcional, Bairro, Cidade, Estado).
2.2. Validar formato de Documento (CNPJ/CPF), Telefone e Email antes de permitir salvar; exibir mensagens claras de erro campo a campo.
2.3. Após criação bem-sucedida, exibir confirmação com o Código Único (8 caracteres) em destaque e ação “Copiar Código”.
2.4. Impedir envio múltiplo acidental (desabilitar botão “Salvar” enquanto processa).
2.5. Em caso de erro de validação/duplicidade, exibir mensagem contextual e não limpar os campos válidos.
2.6. Ao concluir, oferecer ações “Voltar à lista” e “Criar outra barbearia”.

### 3. Edição de Barbearia (Tela)

O que faz: Permite atualizar dados cadastrais da barbearia, exceto campos imutáveis.

Por que é importante: Cadastros precisam refletir mudanças reais do estabelecimento.

Como funciona (alto nível): Acessada pela ação “Editar” na lista; formulário pré-preenchido; salvar atualiza cadastro.

Requisitos Funcionais:
3.1. Carregar formulário com dados atuais da barbearia (read-only para Código e Documento).
3.2. Permitir editar: Nome, Telefone, Proprietário, Email e Endereço (CEP, Rua, Número, Complemento, Bairro, Cidade, Estado).
3.3. Aplicar mesmas validações de formato do cadastro.
3.4. Exibir confirmação de sucesso e retornar à lista preservando os filtros anteriores.
3.5. Exibir aviso de alterações não salvas ao tentar sair com mudanças pendentes.

### 4. Desativar/Reativar Barbearia (Confirmação)

O que faz: Altera o status da barbearia entre Ativa e Inativa com salvaguardas de confirmação.

Por que é importante: Evita perda de dados acidental e permite reversão futura (alinhado à decisão de soft delete).

Como funciona (alto nível): Ação por item na lista abre modal de confirmação com consequências; após confirmar, status é atualizado.

Requisitos Funcionais:
4.1. Ao clicar “Desativar”, exibir modal de confirmação com: Nome, Código e texto explicando que a barbearia ficará indisponível até reativação.
4.2. Exigir confirmação explícita (botão “Confirmar desativação”) para concluir; cancelar fecha modal sem efeito.
4.3. Atualizar imediatamente o item na lista (status, ações) após operação bem-sucedida e exibir feedback de sucesso.
4.4. Quando Inativa, exibir ação “Reativar” no lugar de “Desativar”, com modal e confirmação equivalentes.
4.5. Garantir que filtros/ordem/página sejam preservados após a ação.

### 5. Detalhe Resumido da Barbearia (Painel/Lateral ou Página)

O que faz: Exibe informações-chave antes de ações críticas (revisão rápida do cadastro).

Por que é importante: Ajuda a confirmar a identidade do estabelecimento e reduz erros operacionais.

Como funciona (alto nível): Acessado via “Ver detalhes” ou clique no nome; apresenta dados e ações rápidas (Copiar Código, Editar, Desativar/Reativar).

Requisitos Funcionais:
5.1. Exibir: Nome, Código (com botão “Copiar”), Documento (mascarado), Status, Data de Criação/Atualização, Proprietário, Contato (telefone/email) e Endereço completo.
5.2. Disponibilizar atalhos: Editar e Desativar/Reativar.
5.3. Suportar navegação por teclado (foco coerente ao abrir/fechar painel/modal).

### 6. Feedbacks e Estados do Sistema

O que faz: Garante comunicação clara com o usuário durante operações.

Por que é importante: Reduz incerteza, previne ações repetidas e melhora a experiência.

Requisitos Funcionais:
6.1. Exibir loaders/skeletons em carregamentos; toasts/alertas para sucesso e erro.
6.2. Textos de erro devem orientar a correção (ex.: “Documento em formato inválido. Use apenas números.”).
6.3. Exibir estados vazios com instruções, p.ex.: “Nenhuma barbearia encontrada. Ajuste filtros ou crie uma nova.”
6.4. Botões de ação devem indicar progresso (spinner) e ficar desabilitados durante processamento.

### 7. Acessibilidade e Responsividade

O que faz: Garante uso inclusivo e em múltiplos dispositivos.

Por que é importante: Admins podem usar diferentes telas; acessibilidade é requisito básico de produto.

Requisitos Funcionais:
7.1. Navegação completa por teclado, foco visível e ordem lógica em todos os componentes interativos.
7.2. Labels e descrições semânticas para leitores de tela (ex.: “Copiar código da barbearia XYZ123AB”).
7.3. Contraste de cores suficiente para texto e ícones de status.
7.4. Layout responsivo para breakpoints desktop e tablet (mínimo), mantendo legibilidade da tabela e formulários.

### 8. Acesso e Segurança Básica

O que faz: Restringe o uso das telas a perfis autorizados.

Por que é importante: Evita acesso indevido ao painel administrativo.

Requisitos Funcionais:
8.1. As telas devem estar protegidas para acesso exclusivo do Admin Central; usuários não autorizados veem tela de acesso negado.
8.2. Em sessão expirada, redirecionar para tela de autenticação com mensagem de contexto (“Sessão expirada, por favor, acesse novamente.”).

### 9. Tela de Login do Admin Central

O que faz: Autentica o Admin Central para acesso ao painel de gestão de barbearias.

Por que é importante: Garante que apenas usuários autorizados do perfil Admin Central acessem as funcionalidades administrativas.

Como funciona (alto nível): Tela dedicada com formulário simples de credenciais e feedback de validação; ao autenticar com sucesso, redireciona para a listagem de barbearias.

Requisitos Funcionais:
9.1. Exibir formulário com campos: Email (obrigatório) e Senha (obrigatória), com opção de mostrar/ocultar senha.
9.2. Validar formato de email e presença de senha antes do envio; exibir mensagens claras de erro.
9.3. Em sucesso, redirecionar para a Tela Principal de Listagem de Barbearias, mantendo sessão autenticada.
9.4. Em falha (credenciais inválidas), exibir mensagem “Email ou senha inválidos” sem detalhes sensíveis.
9.5. Indicar progresso durante autenticação (spinner no botão “Entrar”) e desabilitar o botão enquanto processa.
9.6. Não incluir “Esqueci minha senha” no escopo atual; recuperação de senha poderá ser considerada futuramente.

## Experiência do Usuário

- Persona principal: Admin Central (perfil técnico/operacional, foco em produtividade).
- Necessidades: Descobrir, validar e operar barbearias com poucos cliques, segurança e clareza.

Fluxos e Interações Principais:
0) Login: Acessar Tela de Login → Informar email e senha → Entrar → Redirecionar para Listagem de Barbearias.
1) Cadastro: Lista → Nova Barbearia → Preencher formulário → Salvar → Ver confirmação com Código → Copiar Código → Voltar à lista.
2) Edição: Lista → Editar → Ajustar campos permitidos → Salvar → Confirmação → Voltar à lista (preservar filtros).
3) Desativar: Lista → Desativar → Modal de confirmação → Confirmar → Feedback de sucesso → Item atualizado.
4) Reativar: Lista → Reativar → Modal de confirmação → Confirmar → Feedback de sucesso → Item atualizado.
5) Busca/Filtro: Digitar termo e/ou ajustar filtros → Ver resultados paginados → Usar ações por item.
6) Copiar Código: Ação “Copiar Código” na linha ou detalhe → Toast “Código copiado”.

Requisitos de UI/UX:
- Tabela clara, colunas essenciais visíveis sem rolagem horizontal em desktop.
- Botões de ação consistentes (texto, ícone, tooltip). “Copiar Código” deve ter confirmação visual.
- Microcopy de confirmação explicando efeitos de desativar/reativar.
- Estados de erro com linguagem simples e orientação de correção.

Requisitos de Acessibilidade:
- Atalhos de teclado para foco rápido em busca e paginação (opcional, recomendado).
- Mensagens de feedback anunciadas para leitores de tela.
- Campos de formulário com exemplos (placeholders) e máscaras apenas se não prejudicarem leitores de tela.

## Restrições Técnicas de Alto Nível

- Interface web responsiva, focada em produtividade no desktop e uso aceitável em tablet.
- Devem existir estados de carregamento, vazio e erro em todas as telas que consultam/operam dados.
- Cópia do Código deve funcionar via interação padrão do navegador (sem dependências externas).
- Não prescrever tecnologias específicas neste PRD; detalhes de implementação ficam na Tech Spec.

## Não-Objetivos (Fora de Escopo)

- Gestão de usuários das barbearias (barbeiros/clientes) e seus perfis.
- Notificações, relatórios e analytics.
- Temas/branding customizáveis ou white-label.
- Importação/exportação em massa.
- Integrações externas (pagamentos, SMS, email transacional).
- Internacionalização (i18n) além de português.
- Métricas de produto neste momento.

## Questões em Aberto

1. A ação será rotulada como “Desativar/Ativar” (refletindo soft delete) ou manter “Excluir/Reativar” com microcopy explicando que é reversível?
2. O Documento (CNPJ/CPF) deve ser exibido mascarado em todas as telas? Formato preferido?
3. Ordenação padrão na listagem: por Nome (asc) ou Data de Criação (desc)?
4. A busca deve priorizar correspondência por Código quando o termo tiver 8 caracteres?
5. Confirmar rótulos finais de botões e mensagens (ex.: texto exato dos modais de confirmação).
6. Exibir o Código na listagem e também no detalhe, sempre com botão de cópia?
7. Em caso de erro no salvamento, manter o formulário aberto com valores e foco no primeiro campo com erro?
8. Precisa de “visualização rápida” por painel lateral na lista, ou página de detalhe dedicada é suficiente?

---

Data de Criação: 2025-10-13  
Versão: 1.0  
Status: Rascunho para Revisão
