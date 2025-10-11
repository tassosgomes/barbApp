# Implementação Cadastro e Agendamento (Cliente) - Resumo de Tarefas

## Tarefas

- [ ] 1.0 Domain: Entidades Cliente/Agendamento, Enum e Regras (com testes)
- [ ] 2.0 Infra/DB: Migrations, Config EF Core, Global Filters e Repositórios
- [ ] 3.0 Application: DTOs, Validators e Use Cases de Autenticação (Cadastro/Login)
- [ ] 4.0 API: Endpoints AuthCliente (cadastro/login) + JWT + Swagger
- [ ] 5.0 Application: Listar Barbeiros e Serviços (consulta)
- [ ] 6.0 Application: Algoritmo de Disponibilidade (+ Cache 5min)
- [ ] 7.0 Application: Criar/Cancelar/Listar Agendamentos (com lock otimista)
- [ ] 8.0 API: Endpoints Barbeiros/Serviços/Agendamentos (REST + autorização)
- [ ] 9.0 Testes de Integração: Autenticação, Isolamento Multi-tenant e Agendamentos
- [ ] 10.0 Observabilidade: Logging Estruturado, Métricas Prometheus e Healthchecks

## Análise de Paralelização

- Caminho crítico sugerido: 1.0 → 2.0 → 3.0 → 4.0 → 6.0 → 7.0 → 8.0 → 9.0.
- Em paralelo após 2.0: 5.0 pode iniciar com contratos e mocks (depende de seeds do PRD 2/PRD 1), e 10.0 pode avançar com templates de logs/métricas.
- Dependências externas: PRD 5 (multi-tenant/JWT/middleware base), PRD 2 (barbeiros) e PRD 1 (barbearias), serviços/entidades devem existir ou ser seedados para desenvolvimento.
