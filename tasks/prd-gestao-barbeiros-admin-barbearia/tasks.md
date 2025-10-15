# Implementação Gestão de Barbeiros (Admin da Barbearia) - Resumo de Tarefas

## Pré-requisito Crítico

**Atenção**: Antes de iniciar este épico, é necessário implementar o fluxo de **Onboarding do Admin da Barbearia**. Isso inclui a criação do usuário `Admin` durante o cadastro de uma nova barbearia e o seu processo de primeiro login, conforme definido nas respostas do PRD.

---

## Tarefas

- [ ] 1.0 Domain: Entidades, VOs, Exceções e Repositórios (com testes)
- [ ] 2.0 Infra/DB: Migrations, Configurações EF Core, DbContext e Repositórios
- [ ] 3.0 Application: DTOs, Validators e Use Cases (com testes)
- [ ] 4.0 API: Endpoints Barbers e Barbershop Services + Autorização + Swagger
- [ ] 5.0 Agenda Consolidada: Integração com Appointments e Polling (30s)
- [ ] 6.0 Testes de Integração: CRUD, Isolamento Multi-tenant e Agenda
- [ ] 7.0 Observabilidade: Logging Estruturado e Métricas Prometheus
- [ ] 8.0 Integração Frontend: Contratos de API, Mock Data e CORS
- [ ] 9.0 Performance e Índices: Paginação, Índices e tempos de resposta
- [ ] 10.0 Segurança e LGPD: Autorização, Filtros Globais e Mascaramento

## Análise de Paralelização

- Caminho crítico: 1.0 → 2.0 → 3.0 → 4.0 → 6.0.
- Em paralelo após 1.0: 7.0 (observabilidade) e 8.0 (docs integração) podem começar.
- 5.0 depende parcialmente de repositório/agendamentos; pode iniciar com contrato e mocks após 3.0, mas finalizar após disponibilidade de IAppointmentRepository (do PRD Agendamentos).
- 9.0 pode rodar em paralelo com 4.0/6.0 focando em índices e ajustes de queries.
- 10.0 é transversal e pode ser feito em paralelo, mas validação final deve ocorrer após 4.0.