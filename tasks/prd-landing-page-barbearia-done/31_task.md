---
status: pending
parallelizable: false
blocked_by: ["9.0", "20.0", "30.0"]
---

# Tarefa 31.0: Integração Completa e Testes E2E (End-to-End)

## Visão Geral
Esta tarefa é focada em validar o fluxo completo da funcionalidade de Landing Page, garantindo que todas as partes (Backend, Painel Admin e Landing Page Pública) se comuniquem corretamente. O objetivo é realizar testes manuais e, se possível, automatizados que simulem a jornada do usuário de ponta a ponta, identificando e corrigindo bugs de integração.

## Requisitos
- **Ambiente de Testes**: Idealmente, os testes devem ser realizados em um ambiente de `staging` que seja uma réplica do ambiente de produção, com o backend, o frontend do admin e o frontend público deployados e configurados para se comunicarem.
- **Validação de Fluxos**: Todos os fluxos de usuário definidos no PRD devem ser testados.

## Fluxos de Teste Manuais

### Fluxo 1: Criação e Personalização (Admin)
1.  **Cadastro**: Crie uma nova barbearia no painel de administração.
2.  **Verificação da Criação Automática**: 
    - Verifique no banco de dados se as tabelas `landing_page_configs` e `landing_page_services` foram populadas corretamente.
    - Acesse a URL pública (`/barbearia/:code`) e confirme que a landing page foi criada com o template padrão e as informações básicas.
3.  **Personalização Completa**:
    - Navegue para a seção "Landing Page" no painel admin.
    - **Troque o template**: Escolha um template diferente (ex: Urbano) e salve. Verifique se a página pública reflete a mudança.
    - **Edite as informações**: Faça upload de um logo, altere o texto "Sobre", o horário e as redes sociais. Salve e confirme se a página pública foi atualizada.
    - **Gerencie os serviços**: Oculte um serviço e reordene os outros. Salve e confirme as mudanças na página pública.

### Fluxo 2: Jornada do Cliente (Público)
1.  **Acesso**: Abra a URL da landing page personalizada no fluxo anterior em um navegador (desktop e mobile).
2.  **Visualização**: Verifique se todas as informações personalizadas (logo, textos, serviços na ordem correta) estão sendo exibidas corretamente no template escolhido.
3.  **Seleção e Agendamento**:
    - Selecione um ou mais serviços.
    - Verifique se o botão flutuante de agendamento aparece com o total correto.
    - Clique no botão e confirme se você é redirecionado para a página de agendamento com os serviços pré-selecionados na URL.
4.  **Contato via WhatsApp**:
    - Clique no botão do WhatsApp.
    - Confirme se uma nova aba é aberta com a URL do `wa.me` contendo o número de telefone e a mensagem padrão corretos.

## Possíveis Pontos de Falha (O que observar)
- **CORS**: Erros de CORS entre os frontends e o backend.
- **Autenticação/Autorização**: O admin consegue editar apenas a sua própria landing page?
- **Formato de Dados**: Os dados enviados pelo frontend do admin estão no formato que o backend espera? E os dados enviados pelo backend estão no formato que os frontends esperam?
- **Atualização de Cache**: Se houver cache na API ou no CDN, as atualizações feitas pelo admin estão sendo refletidas na página pública em um tempo razoável?
- **Responsividade**: O layout quebra em algum dispositivo ou tamanho de tela específico?

## Critérios de Aceitação
- [ ] O fluxo de criação automática da landing page no cadastro da barbearia funciona sem erros.
- [ ] Todas as personalizações feitas no painel do admin (template, logo, textos, serviços) são refletidas corretamente e em tempo real (ou após um refresh/limpeza de cache) na landing page pública.
- [ ] O fluxo de seleção de serviços e redirecionamento para o agendamento na página pública funciona perfeitamente.
- [ ] O botão de contato do WhatsApp funciona em diferentes dispositivos (desktop e mobile).
- [ ] Nenhum erro crítico (erros de console, crashes, falhas de rede) é encontrado durante os testes dos fluxos principais.
- [ ] Todos os bugs de integração identificados durante os testes foram documentados e corrigidos.