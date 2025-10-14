# Template de Documento de Requisitos de Produto (PRD)

## Visão Geral

Este PRD define a implantação de observabilidade com Sentry (sentry.io) no produto BarbApp, cobrindo backend (.NET 8.0) na pasta `backend` e frontend (React + Vite + TypeScript) na pasta `barbapp-admin`. O objetivo é capturar e monitorar erros não tratados, eventos relevantes, performance básica e estabilidade por release/ambiente antes do primeiro lançamento em produção. A iniciativa permitirá respostas rápidas a incidentes, visibilidade da qualidade e redução do tempo médio de resolução (MTTR), com foco em privacidade, baixo overhead e operação simples.

## Objetivos

- Reduzir o MTTR de incidentes críticos em produção em 50% nos primeiros 60 dias.
- Atingir cobertura de rastreamento de erros não tratados de 95% no backend e frontend.
- Permitir correlação por release e ambiente (dev, homolog, prod) com versionamento e tags padronizadas.
- Disponibilizar alertas automáticos para erros críticos (taxa e ocorrências) aos responsáveis, com tempo de detecção (MTTD) < 5 minutos.
- Estabelecer métricas base: taxa de erros por 1.000 requests/views, top endpoints/telas com falhas, e regressões por release.

## Histórias de Usuário

- Como desenvolvedor backend, quero ser notificado quando ocorrer um erro não tratado em produção para agir rapidamente e corrigir a causa raiz.
- Como desenvolvedor frontend, quero ver o stack trace com sourcemaps e contexto da tela para entender e reproduzir o erro do usuário.
- Como PM/PO, quero acompanhar a taxa de erros por release e por área do produto, para priorizar correções que mais impactam usuários.
- Como Suporte/CS, quero identificar se um usuário enfrentou erros em uma janela de tempo para orientar atendimento e coletar informações úteis.
- Como responsável por confiabilidade, quero garantir que dados sensíveis não sejam coletados e que o consumo de recursos permaneça controlado.

## Funcionalidades Principais

1) Rastreamento de Erros (Backend)
- O que: Captura automática de exceções não tratadas e registro de erros relevantes (com stack trace e breadcrumbs).
- Por que: Visibilidade de falhas em produção e priorização por impacto.
- Como (alto nível): Inicialização do SDK do Sentry no processo .NET 8.0, configurando DSN, ambiente e release; captura automática e manual de eventos.
- Requisitos funcionais:
  1.1 Capturar 100% dos erros não tratados no backend com stack trace.
  1.2 Incluir contexto básico: endpoint, método HTTP, status, user-agent, correlação de request (trace id se disponível).
  1.3 Taguear `environment` (dev/homolog/prod) e `release` (formato padronizado) em todos os eventos.
  1.4 Suportar breadcrumbs para chamadas relevantes (ex.: DB/external calls) quando disponíveis via integração nativa do SDK.
  1.5 Respeitar política de privacidade: não incluir PII sensível; permitir desabilitar campos via scrub/filters.

2) Rastreamento de Erros (Frontend)
- O que: Captura de erros JS não tratados e rejeições de promises, com contexto da página e sourcemaps.
- Por que: Entender falhas do cliente e regressões introduzidas por novas releases.
- Como (alto nível): Inicialização do SDK no React/Vite, configuração de DSN, environment e release; upload de sourcemaps no build.
- Requisitos funcionais:
  2.1 Capturar 100% dos erros JS não tratados e unhandled rejections.
  2.2 Associar eventos às rotas/telas e ao `release` e `environment` padrão.
  2.3 Disponibilizar stack trace com sourcemaps e breadcrumbs (navegação, cliques, XHR/fetch quando habilitado).
  2.4 Definir sample rate para limitar volume de dados, com controle por ambiente (ex.: dev/homolog mais alto; prod mais conservador).
  2.5 Aplicar scrubbing de PII e não registrar dados sensíveis de formulários.

3) Performance Básica e Estabilidade
- O que: Visão de performance de alto nível (ex.: tempo de carregamento chave no frontend, duração de requisições/handlers no backend) e taxa de erros.
- Por que: Acompanhar regressões e hotspots.
- Como (alto nível): Habilitar recursos leves de performance do Sentry em ambos os lados com amostragem controlada.
- Requisitos funcionais:
  3.1 Medir taxa de erros por 1.000 requests (backend) e por 1.000 page views (frontend).
  3.2 Permitir amostragem ajustável de transações/traces (ex.: 0–20% em prod) para controlar custo e overhead.
  3.3 Permitir dashboards de estabilidade por release e por área do produto.

4) Releases, Ambientes e Alertas
- O que: Padronização de `release` e `environment`, e alertas para incidentes críticos.
- Por que: Facilitar correlação e resposta a incidentes.
- Como (alto nível): Naming consistente para releases, variáveis de ambiente para DSN e environment, regras de alertas no Sentry.
- Requisitos funcionais:
  4.1 Definir convenção de release: `barbapp-<app>-<semver>-<commit>` ou equivalente aprovado.
  4.2 Definir ambientes oficiais: `development`, `staging` (homolog) e `production`.
  4.3 Configurar alertas para erros de nível alto (ex.: erro novo com >N ocorrências em 5min, erro que afeta >X usuários, spike de taxa).
  4.4 Direcionar alertas para canais definidos (e-mail/Slack) e incluir owners por área quando aplicável.

5) Governança de Dados e Segurança
- O que: Controles de privacidade e acesso.
- Por que: Conformidade, minimização de dados e segurança operacional.
- Como (alto nível): Scrubbing de PII, retenção conforme política do Sentry, gestão de credenciais via variáveis/secret manager.
- Requisitos funcionais:
  5.1 Não coletar dados pessoais sensíveis nem conteúdo de campos confidenciais; aplicar scrub por padrão.
  5.2 Armazenar DSNs e chaves em variáveis de ambiente; não versionar segredos.
  5.3 Segregar acessos por papéis no Sentry (dev/ops/visualização) quando disponível.

## Experiência do Usuário

- Personas: Dev Backend, Dev Frontend, PM/PO, Suporte/CS, Confiabilidade/Segurança.
- Fluxos principais:
  - Deteção: erro ocorre no backend ou frontend → evento chega ao Sentry com contexto básico.
  - Triagem: alerta notifica time → análise do evento com stack trace, release, ambiente e breadcrumbs.
  - Ação: issue vinculada ao repositório/tarefa (fora do escopo técnico aqui) → correção priorizada por impacto.
- UI/UX: Não há alterações de UI para usuários finais; no admin, nenhum bloqueio visual além do comportamento atual de erro. Considerar mensagens de erro genéricas e seguras.
- Acessibilidade: Garantir que a coleta não afete tecnologias assistivas nem degrade performance perceptível para usuários finais.

## Restrições Técnicas de Alto Nível

- Integrações: Uso de SDKs oficiais do Sentry para .NET 8.0 e para React/Vite/TS; integração opcional com provedores de alerta (e-mail/Slack).
- Compliance/Segurança: Obediência às políticas de privacidade; scrub de PII; não registrar tokens/segredos.
- Performance: Overhead máximo aceitável de ~1–3% CPU/memória; amostragem configurável para traces/eventos.
- Privacidade: Evitar coleta de dados sensíveis ou conteúdo de formulários; anonimizar identificadores quando necessário.
- Requisitos não negociáveis: Variáveis de ambiente para DSN e `environment`; naming consistente de `release`.

## Não-Objetivos (Fora de Escopo)

- Implementar plataforma completa de logs (observabilidade de logs centralizados fora do Sentry).
- Session Replay/Heatmaps e gravação de tela de usuários (não incluído nesta fase).
- Monitoramento sintético e testes de disponibilidade externos.
- Telemetria de negócios (ex.: funis/analytics detalhados) além de métricas de estabilidade/performance básicas.
- Qualquer alteração de arquitetura de serviços apenas para viabilizar observabilidade (apenas integrações leves do SDK).

## Premissas e Dependências

- A conta/projeto no Sentry estará provisionada e acessível ao time.
- DSNs serão fornecidos de forma segura (secrets/variáveis) para cada ambiente.
- Pipelines de build/release (CI/CD) poderão injetar `release` e `environment` e, no frontend, publicar sourcemaps.
- As aplicações possuem versionamento consistente para compor `release`.
- Dependências: SDKs oficiais do Sentry e acesso à internet de saída em ambientes de execução para envio de eventos.

## Riscos e Mitigações

- Volume excessivo de eventos (custo/ruído): mitigar com sample rates, filtros e deduplicação.
- Coleta inadvertida de PII: mitigar com scrub por padrão, revisões de campos e testes.
- Overhead de performance: mitigar com configurações conservadoras e monitorar impacto após ativação.
- Falta de adesão ao fluxo de releases: mitigar definindo convenções e validações no CI.

## Questões em Aberto

- DSNs e projeto(s) no Sentry: teremos um projeto por app (backend e frontend) ou unificado? Quem provisiona?
- Convenção final de `release` (formato e fonte do versionamento) e nomes de `environment` oficiais.
- Política de amostragem (errors e performance) por ambiente e limites de custo.
- Política de retenção de dados e acesso (RBAC) dentro do Sentry.
- Canal de alertas (e-mail, Slack, ambos) e severidades/alvos por equipe.
- Lista de campos que exigem scrub obrigatório (ex.: documentos, telefone, e-mail).
