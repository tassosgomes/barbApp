# Mock Data para Integração Frontend

Esta pasta contém dados mock em formato JSON para facilitar a integração do frontend com a API de gestão de barbeiros.

## Arquivos Disponíveis

### `barbers-list.json`
Dados mock para a resposta da listagem de barbeiros (`GET /api/barbers`).
Contém uma lista paginada com 3 barbeiros (2 ativos, 1 inativo) com seus serviços associados.

### `barbershop-services-list.json`
Dados mock para a resposta da listagem de serviços (`GET /api/barbershop-services`).
Contém uma lista paginada com 5 serviços ativos oferecidos pela barbearia.

### `team-schedule.json`
Dados mock para a resposta da agenda consolidada (`GET /api/barbers/schedule`).
Contém uma lista de agendamentos para o dia atual com diferentes status (Confirmed, Pending, Cancelled).

## Como Usar

1. **Desenvolvimento Frontend**: Use estes arquivos para simular respostas da API durante o desenvolvimento
2. **Testes de Integração**: Utilize como dados de referência para validar a integração
3. **Documentação**: Servem como exemplos concretos dos contratos de API

## Notas

- IDs são GUIDs fictícios para exemplo
- Datas estão em formato ISO 8601 (UTC)
- Valores monetários estão em reais (BRL)
- Telefones estão no formato brasileiro
- Status possíveis: "Confirmed", "Pending", "Cancelled"