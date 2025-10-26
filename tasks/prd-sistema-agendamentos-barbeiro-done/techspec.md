# Especificação Técnica - Sistema de Agendamentos (Barbeiro)

## Resumo Executivo

Este documento detalha a arquitetura e implementação do módulo de agendamentos para o perfil "Barbeiro". A solução permite que barbeiros gerenciem suas agendas de forma independente para cada barbearia onde trabalham, garantindo total isolamento de dados através do sistema multi-tenant existente. O barbeiro poderá visualizar sua agenda diária, confirmar, cancelar e marcar atendimentos como concluídos. A arquitetura segue o padrão Clean Architecture, utilizando uma API RESTful no backend .NET 8 e uma interface reativa no frontend. A atualização da agenda será feita via polling a cada 10 segundos para garantir que o barbeiro tenha uma visão próxima do tempo real. A concorrência de status (ex: cliente cancela enquanto barbeiro confirma) será tratada com otimismo, retornando um erro de conflito para o cliente resolver.

## Arquitetura do Sistema

### Visão Geral dos Componentes

**Domain Layer** (`BarbApp.Domain`)
- **Entidades**: `Appointment` (representa um agendamento)
- **Enums**: `AppointmentStatus` (Pending, Confirmed, Completed, Cancelled)
- **Interfaces**: `IAppointmentRepository` (contrato para persistência de agendamentos)
- **Exceções**: `AppointmentNotFoundException`, `InvalidAppointmentStatusTransitionException`

**Application Layer** (`BarbApp.Application`)
- **Use Cases**:
  - `GetBarberScheduleUseCase`: Busca a agenda de um barbeiro para uma data específica.
  - `ConfirmAppointmentUseCase`: Confirma um agendamento pendente.
  - `CancelAppointmentUseCase`: Cancela um agendamento.
  - `CompleteAppointmentUseCase`: Marca um agendamento como concluído.
  - `GetAppointmentDetailsUseCase`: Busca detalhes de um agendamento específico.
- **DTOs**: Inputs e Outputs para todas as operações (ex: `BarberScheduleOutput`, `AppointmentDetailsOutput`).
- **Validators**: FluentValidation para os inputs dos use cases.

**Infrastructure Layer** (`BarbApp.Infrastructure`)
- **Repositories**: `AppointmentRepository` (implementação com EF Core e Global Query Filters).
- **DbContext**: Extensão do `BarbAppDbContext` para incluir a entidade `Appointment`.
- **Mappings**: Entity Type Configuration para a tabela `appointments`.

**API Layer** (`BarbApp.API`)
- **Controllers**: `ScheduleController` (para visualização) e `AppointmentsController` (para ações).
- **Authorization**: `[Authorize(Roles = "Barbeiro")]` em todos os endpoints.
- **Polling**: Frontend fará requisições a cada 10 segundos para o endpoint de agenda.

### Fluxo de Dados

```
Frontend React (Barbeiro)
    ↓ HTTP Request (GET /api/schedule/my-schedule?date=...)
ScheduleController
    ↓ [Authorize] + TenantMiddleware (extrai barbeariaId e barberId do JWT)
GetBarberScheduleUseCase
    ↓ Business Logic (valida data, etc.)
AppointmentRepository (Infrastructure)
    ↓ EF Core + Global Query Filter (WHERE barbearia_id = ? AND barber_id = ?)
PostgreSQL Database
```

**Fluxo de Confirmação de Agendamento:**
1. Barbeiro clica em "Confirmar" em um agendamento pendente.
2. Frontend envia `POST /api/appointments/{id}/confirm`.
3. `AppointmentsController` valida autorização e que o barbeiro é o dono do agendamento.
4. `ConfirmAppointmentUseCase` é invocado.
5. O use case busca o agendamento, verifica se o status atual é "Pending".
6. Se for, atualiza o status para "Confirmed". Se não for, lança `InvalidAppointmentStatusTransitionException` (tratado como 409 Conflict).
7. `AppointmentRepository` persiste a mudança.
8. `UnitOfWork.Commit()` salva a transação.
9. Retorna `200 OK` com os dados do agendamento atualizado.

## Design de Implementação

### Interfaces Principais

```csharp
// Domain - IAppointmentRepository
public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Appointment>> GetByBarberAndDateAsync(
        Guid barbeariaId,
        Guid barberId,
        DateTime date,
        CancellationToken cancellationToken);

    Task InsertAsync(Appointment appointment, CancellationToken cancellationToken);
    Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken);
}
```

### Modelos de Dados

**Entidade Appointment (Domain)**
```csharp
public class Appointment
{
    public Guid Id { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid BarberId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid ServiceId { get; private set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ConfirmedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private Appointment() { } // EF Core

    // Fábrica para criação (será usada pelo use case de agendamento do cliente)
    public static Appointment Create(...) { /* ... */ }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Pending)
            throw new InvalidAppointmentStatusTransitionException(Status, AppointmentStatus.Confirmed);

        Status = AppointmentStatus.Confirmed;
        ConfirmedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
            throw new InvalidAppointmentStatusTransitionException(Status, AppointmentStatus.Cancelled);

        Status = AppointmentStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new InvalidAppointmentStatusTransitionException(Status, AppointmentStatus.Completed);
        
        if (StartTime > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot complete an appointment that has not started yet.");

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    Completed,
    Cancelled
}
```

**DTOs (Application)**
```csharp
// Output para a lista de agendamentos do dia
public record BarberAppointmentOutput(
    Guid Id,
    string CustomerName,
    string ServiceTitle,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status
);

// Output para a agenda completa do barbeiro
public record BarberScheduleOutput(
    DateTime Date,
    Guid BarberId,
    string BarberName,
    List<BarberAppointmentOutput> Appointments
);

// Output para detalhes de um agendamento
public record AppointmentDetailsOutput(
    Guid Id,
    string CustomerName,
    string CustomerPhone,
    string ServiceTitle,
    decimal ServicePrice,
    int ServiceDurationMinutes,
    DateTime StartTime,
    DateTime EndTime,
    AppointmentStatus Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CancelledAt,
    DateTime? CompletedAt
);
```

**Schema do Banco de Dados**
```sql
-- Tabela de Agendamentos
CREATE TABLE appointments (
    appointment_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbershops(barbershop_id) ON DELETE CASCADE,
    barber_id UUID NOT NULL REFERENCES barbers(barber_id) ON DELETE CASCADE,
    customer_id UUID NOT NULL REFERENCES customers(customer_id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES barbershop_services(barbershop_service_id),
    start_time TIMESTAMP WITH TIME ZONE NOT NULL,
    end_time TIMESTAMP WITH TIME ZONE NOT NULL,
    status INT NOT NULL, -- Mapeado para o Enum AppointmentStatus
    created_at TIMESTAMP WITH TIME ZONE NOT NULL,
    updated_at TIMESTAMP WITH TIME ZONE NOT NULL,
    confirmed_at TIMESTAMP WITH TIME ZONE NULL,
    cancelled_at TIMESTAMP WITH TIME ZONE NULL,
    completed_at TIMESTAMP WITH TIME ZONE NULL
);

-- Índices para performance
CREATE INDEX idx_appointments_barbearia_id ON appointments(barbearia_id);
CREATE INDEX idx_appointments_barber_id_start_time ON appointments(barber_id, start_time);
CREATE INDEX idx_appointments_customer_id ON appointments(customer_id);
CREATE INDEX idx_appointments_status ON appointments(status);
```

### Endpoints de API

**Visualização de Agenda**
```
GET /api/schedule/my-schedule
Authorization: Bearer {token com role Barbeiro}
Query: ?date=2025-10-11
Response 200: BarberScheduleOutput
Response 401: Unauthorized
```

**Ações de Agendamento**
```
GET /api/appointments/{id}
Authorization: Bearer {token com role Barbeiro}
Response 200: AppointmentDetailsOutput
Response 404: Appointment not found
Response 403: Forbidden (agendamento não pertence ao barbeiro)
```

```
POST /api/appointments/{id}/confirm
Authorization: Bearer {token com role Barbeiro}
Response 200: OK
Response 404: Appointment not found
Response 409: Conflict (status já foi alterado)
```

```
POST /api/appointments/{id}/cancel
Authorization: Bearer {token com role Barbeiro}
Response 200: OK
Response 404: Appointment not found
Response 409: Conflict (status já foi alterado)
```

```
POST /api/appointments/{id}/complete
Authorization: Bearer {token com role Barbeiro}
Response 200: OK
Response 400: Bad Request (agendamento ainda não começou)
Response 404: Appointment not found
Response 409: Conflict (status não era 'Confirmado')
```

## Pontos de Integração

- **Autenticação e Contexto**: O sistema dependerá do `ITenantContext` populado pelo `TenantMiddleware` para obter o `barbeariaId` e o `userId` (que corresponde ao `barberId`) do token JWT do barbeiro logado.
- **Global Query Filter**: A entidade `Appointment` terá um Global Query Filter para garantir que as consultas sejam sempre isoladas pela `barbeariaId` do contexto.
- **Repositórios de Outros Domínios**: O `GetBarberScheduleUseCase` precisará consultar `IBarberRepository` e `ICustomerRepository` para enriquecer os dados do DTO de saída com nomes.

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|---|---|---|---|
| `BarbAppDbContext` | Extensão | Adicionar `DbSet<Appointment>` e novo Global Query Filter. **Baixo risco**. | Criar nova migration. |
| `RemoveBarberUseCase` | Modificação | Deverá consultar `IAppointmentRepository` para verificar agendamentos futuros antes de desativar um barbeiro. **Médio risco**. | Adicionar dependência e lógica de validação. |
| `BarbersController` | Nenhum | As ações de agendamento estarão em controllers dedicados. | Nenhuma. |
| Frontend (React) | Nova Funcionalidade | Criar a tela de agenda do barbeiro, com a lógica de polling e as ações. **Médio risco**. | Implementar nova view e componentes. |

## Abordagem de Testes

### Testes Unitários

- **Domain**: Testar a máquina de estados da entidade `Appointment`, garantindo que as transições de status (`Confirm`, `Cancel`, `Complete`) funcionem como esperado e lancem exceções para transições inválidas.
- **Application**: Testar cada use case isoladamente com mocks para os repositórios.
  - `ConfirmAppointmentUseCase`: Testar cenário de sucesso e cenário de falha (ex: tentar confirmar um agendamento já cancelado).
  - `GetBarberScheduleUseCase`: Testar que os agendamentos retornados são corretamente filtrados por data e barbeiro.

### Testes de Integração

- **Repositório**: Testar o `AppointmentRepository` com um banco de dados em memória ou TestContainers para garantir que o Global Query Filter funciona e que os dados são persistidos corretamente.
- **API**:
  - Testar cada endpoint para garantir que a autorização por role "Barbeiro" está funcionando.
  - Testar que um barbeiro não pode acessar ou modificar agendamentos de outro barbeiro (deve retornar 403 Forbidden).
  - Simular um conflito de status (409 Conflict) atualizando um agendamento e tentando modificá-lo novamente com dados antigos.

## Sequenciamento de Desenvolvimento

1.  **Domain (3h)**: Implementar a entidade `Appointment`, o enum `AppointmentStatus` e a interface `IAppointmentRepository`. Escrever testes unitários para a máquina de estados.
2.  **Infrastructure (4h)**: Criar a migration para a tabela `appointments`. Implementar o `AppointmentRepository` e o Global Query Filter no `DbContext`.
3.  **Application (6h)**: Implementar os DTOs e os use cases (`GetBarberScheduleUseCase`, `ConfirmAppointmentUseCase`, `CancelAppointmentUseCase`, `CompleteAppointmentUseCase`, `GetAppointmentDetailsUseCase`). Escrever testes unitários para os use cases.
4.  **API (4h)**: Implementar os controllers `ScheduleController` e `AppointmentsController` com os respectivos endpoints e documentação Swagger.
5.  **Modificação (2h)**: Atualizar o `RemoveBarberUseCase` (do PRD de Gestão de Barbeiros) para incluir a validação de agendamentos futuros.
6.  **Testes de Integração (5h)**: Implementar testes de integração para a API, cobrindo cenários de sucesso, autorização e conflito.
7.  **Frontend (Estimativa separada)**: Implementar a UI da agenda do barbeiro.

**Total Estimado (Backend)**: ~24 horas.

## Monitoramento e Observabilidade

- **Logging Estruturado**:
  - `Information`: Ações bem-sucedidas (agendamento confirmado, cancelado, etc.). Logar `appointmentId` e `barberId`.
  - `Warning`: Tentativas de transição de status inválida (ex: tentar concluir um agendamento pendente).
  - `Error`: Falhas inesperadas durante o processamento.
- **Métricas (Prometheus)**:
  - `barbapp_appointments_status_changed_total`: Contador para cada mudança de status (confirm, cancel, complete), com labels para `barbearia_id` e `status`.
  - `barbapp_schedule_load_duration_seconds`: Histograma para medir o tempo de carregamento da agenda do barbeiro.
- **Alertas**:
  - `Warning`: Se o tempo de carregamento da agenda (`barbapp_schedule_load_duration_seconds`) exceder 2 segundos.
  - `Error`: Se a taxa de erros 500 nos endpoints de agendamento for > 1% em 5 minutos.

## Considerações Técnicas

### Decisões Principais

- **Atualização da Agenda**: Será via **polling de 10 segundos** no frontend. É uma solução simples e eficaz para o MVP, evitando a complexidade de WebSockets/SignalR.
- **Tratamento de Concorrência**: Utilizar uma abordagem de **concorrência otimista**. O use case verificará o status do agendamento antes de modificá-lo. Se o status mudou, uma exceção será lançada, resultando em um `HTTP 409 Conflict`.
- **Armazenamento de Horários**: Todos os horários (`start_time`, `end_time`, etc.) serão armazenados em **UTC** no banco de dados para evitar problemas com fuso horário. O frontend será responsável por exibir na hora local do usuário.
- **Escopo de Visualização**: O MVP focará na visualização do **dia atual e dias futuros**. A navegação permitirá "Dia Anterior" e "Próximo Dia", mas um histórico completo e pesquisável fica para uma fase futura.
- **Permissões**: As ações de modificar status de agendamento são exclusivas do **Barbeiro** (para seus agendamentos) e do **Cliente** (para os seus). O Admin da Barbearia tem apenas permissão de visualização no MVP.

### Riscos Conhecidos

- **Performance do Polling**: Com muitos barbeiros online, o polling pode aumentar a carga no servidor. O intervalo de 10s é um balanço entre tempo real e carga. A performance do endpoint de busca de agenda deve ser otimizada com índices corretos.
- **Dependência de PRDs**: A funcionalidade completa depende da criação de clientes e serviços, que são de outros PRDs. Esta Tech Spec assume que os repositórios e entidades (`Customer`, `BarbershopService`) estarão disponíveis.

### Conformidade com Padrões

Esta especificação segue todos os padrões definidos nos diretórios `@rules`, incluindo `code-standard.md`, `http.md`, `sql.md`, `tests.md`, e `logging.md`, garantindo consistência com o restante do projeto.
