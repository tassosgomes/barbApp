# 📋 Contratos da API - Agenda do Barbeiro

## 🎯 Visão Geral

Este documento detalha os contratos JSON completos da API de agendamentos do barbeiro, incluindo exemplos reais de requisição e resposta para facilitar a implementação do frontend.

---

## 📍 Endpoints

### Base URL

```
https://api.barbapp.com/api
```

### Autenticação

Todos os endpoints requerem autenticação via Bearer Token JWT:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

O token deve conter:
- `role`: "Barbeiro"
- `userId`: ID do barbeiro
- `barbeariaId`: ID da barbearia selecionada (contexto)

---

## 1️⃣ GET /api/schedule/my-schedule

### Descrição

Retorna a agenda completa do barbeiro autenticado para uma data específica, incluindo todos os agendamentos com informações de cliente, serviço e status.

### Role Necessária

`Barbeiro`

### Query Parameters

| Parâmetro | Tipo | Obrigatório | Descrição | Exemplo |
|-----------|------|-------------|-----------|---------|
| `date` | DateTime | Não | Data da agenda (formato ISO 8601). Se omitido, usa data atual. | `2025-10-19` |

### Request Example

```http
GET /api/schedule/my-schedule?date=2025-10-19 HTTP/1.1
Host: api.barbapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Accept: application/json
```

### Response 200 OK

**BarberScheduleOutput**

```json
{
  "date": "2025-10-19T00:00:00Z",
  "barberId": "a3d5e8f7-9c4b-4e1a-8d2c-f5b9e7a3c1d4",
  "barberName": "João Silva",
  "appointments": [
    {
      "id": "f1e2d3c4-b5a6-4d7e-8f9a-0b1c2d3e4f5a",
      "customerName": "Carlos Mendes",
      "serviceTitle": "Corte + Barba",
      "startTime": "2025-10-19T09:00:00Z",
      "endTime": "2025-10-19T10:00:00Z",
      "status": 1
    },
    {
      "id": "a2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d",
      "customerName": "Pedro Oliveira",
      "serviceTitle": "Corte Simples",
      "startTime": "2025-10-19T10:30:00Z",
      "endTime": "2025-10-19T11:00:00Z",
      "status": 0
    },
    {
      "id": "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
      "customerName": "Lucas Ferreira",
      "serviceTitle": "Barba Completa",
      "startTime": "2025-10-19T14:00:00Z",
      "endTime": "2025-10-19T14:30:00Z",
      "status": 1
    },
    {
      "id": "c4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f",
      "customerName": "Rafael Costa",
      "serviceTitle": "Corte + Barba + Sobrancelha",
      "startTime": "2025-10-19T15:00:00Z",
      "endTime": "2025-10-19T16:30:00Z",
      "status": 2
    },
    {
      "id": "d5e6f7a8-b9c0-4d1e-2f3a-4b5c6d7e8f9a",
      "customerName": "Thiago Santos",
      "serviceTitle": "Corte Premium",
      "startTime": "2025-10-19T17:00:00Z",
      "endTime": "2025-10-19T18:00:00Z",
      "status": 0
    }
  ]
}
```

### Response Fields

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `date` | DateTime (ISO 8601) | Data da agenda em UTC |
| `barberId` | UUID | ID único do barbeiro |
| `barberName` | string | Nome completo do barbeiro |
| `appointments` | Array | Lista de agendamentos do dia |
| `appointments[].id` | UUID | ID único do agendamento |
| `appointments[].customerName` | string | Nome do cliente |
| `appointments[].serviceTitle` | string | Nome do serviço agendado |
| `appointments[].startTime` | DateTime (ISO 8601) | Horário de início em UTC |
| `appointments[].endTime` | DateTime (ISO 8601) | Horário de término em UTC |
| `appointments[].status` | int (enum) | Status do agendamento (ver tabela abaixo) |

### AppointmentStatus Enum

| Valor | Nome | Descrição | Cor Sugerida |
|-------|------|-----------|--------------|
| `0` | Pending | Agendamento pendente de confirmação | 🟡 Amarelo/Laranja |
| `1` | Confirmed | Agendamento confirmado pelo barbeiro | 🟢 Verde |
| `2` | Completed | Atendimento concluído | ⚪ Cinza |
| `3` | Cancelled | Agendamento cancelado | 🔴 Vermelho |

### Response 401 Unauthorized

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Token inválido ou expirado."
}
```

### Response 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Usuário não possui permissão de Barbeiro."
}
```

### Empty State Example

Quando não há agendamentos no dia:

```json
{
  "date": "2025-10-20T00:00:00Z",
  "barberId": "a3d5e8f7-9c4b-4e1a-8d2c-f5b9e7a3c1d4",
  "barberName": "João Silva",
  "appointments": []
}
```

---

## 2️⃣ GET /api/appointments/{id}

### Descrição

Retorna os detalhes completos de um agendamento específico, incluindo informações estendidas do cliente e serviço.

### Role Necessária

`Barbeiro`

### Path Parameters

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | UUID | ID do agendamento |

### Request Example

```http
GET /api/appointments/f1e2d3c4-b5a6-4d7e-8f9a-0b1c2d3e4f5a HTTP/1.1
Host: api.barbapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Accept: application/json
```

### Response 200 OK

**AppointmentDetailsOutput**

```json
{
  "id": "f1e2d3c4-b5a6-4d7e-8f9a-0b1c2d3e4f5a",
  "customerName": "Carlos Mendes",
  "customerPhone": "+5511987654321",
  "serviceTitle": "Corte + Barba",
  "servicePrice": 65.00,
  "serviceDurationMinutes": 60,
  "startTime": "2025-10-19T09:00:00Z",
  "endTime": "2025-10-19T10:00:00Z",
  "status": 1,
  "createdAt": "2025-10-18T14:23:10Z",
  "confirmedAt": "2025-10-18T18:45:32Z",
  "cancelledAt": null,
  "completedAt": null
}
```

### Response Fields

| Campo | Tipo | Descrição |
|-------|------|-----------|
| `id` | UUID | ID único do agendamento |
| `customerName` | string | Nome completo do cliente |
| `customerPhone` | string | Telefone do cliente (formato: +5511987654321) |
| `serviceTitle` | string | Nome do serviço |
| `servicePrice` | decimal | Preço do serviço em reais |
| `serviceDurationMinutes` | int | Duração estimada do serviço em minutos |
| `startTime` | DateTime (ISO 8601) | Horário de início em UTC |
| `endTime` | DateTime (ISO 8601) | Horário de término em UTC |
| `status` | int (enum) | Status atual (0=Pending, 1=Confirmed, 2=Completed, 3=Cancelled) |
| `createdAt` | DateTime (ISO 8601) | Data/hora de criação do agendamento |
| `confirmedAt` | DateTime? (ISO 8601) | Data/hora de confirmação (null se não confirmado) |
| `cancelledAt` | DateTime? (ISO 8601) | Data/hora de cancelamento (null se não cancelado) |
| `completedAt` | DateTime? (ISO 8601) | Data/hora de conclusão (null se não concluído) |

### Response 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Agendamento não encontrado."
}
```

### Response 403 Forbidden

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Este agendamento não pertence ao barbeiro autenticado."
}
```

---

## 3️⃣ POST /api/appointments/{id}/confirm

### Descrição

Confirma um agendamento que está com status `Pending`. A confirmação é irreversível.

### Role Necessária

`Barbeiro`

### Path Parameters

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | UUID | ID do agendamento |

### Request Example

```http
POST /api/appointments/a2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d/confirm HTTP/1.1
Host: api.barbapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Length: 0
```

**Nota**: Não há body na requisição.

### Response 200 OK

Retorna o agendamento atualizado com status `Confirmed`:

```json
{
  "id": "a2b3c4d5-e6f7-4a8b-9c0d-1e2f3a4b5c6d",
  "customerName": "Pedro Oliveira",
  "customerPhone": "+5511976543210",
  "serviceTitle": "Corte Simples",
  "servicePrice": 45.00,
  "serviceDurationMinutes": 30,
  "startTime": "2025-10-19T10:30:00Z",
  "endTime": "2025-10-19T11:00:00Z",
  "status": 1,
  "createdAt": "2025-10-18T16:12:43Z",
  "confirmedAt": "2025-10-19T08:23:15Z",
  "cancelledAt": null,
  "completedAt": null
}
```

### Response 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Agendamento não encontrado."
}
```

### Response 409 Conflict

Retornado quando o agendamento não está em status `Pending`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Não é possível confirmar um agendamento que não está pendente. Status atual: Cancelled"
}
```

**Possíveis causas**:
- Agendamento já foi confirmado
- Agendamento foi cancelado
- Agendamento já foi concluído

**Ação recomendada**: Atualizar a lista de agendamentos (refetch).

---

## 4️⃣ POST /api/appointments/{id}/cancel

### Descrição

Cancela um agendamento que está com status `Pending` ou `Confirmed`. O cancelamento é irreversível.

### Role Necessária

`Barbeiro`

### Path Parameters

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | UUID | ID do agendamento |

### Request Example

```http
POST /api/appointments/b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e/cancel HTTP/1.1
Host: api.barbapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Length: 0
```

**Nota**: Não há body na requisição.

### Response 200 OK

Retorna o agendamento atualizado com status `Cancelled`:

```json
{
  "id": "b3c4d5e6-f7a8-4b9c-0d1e-2f3a4b5c6d7e",
  "customerName": "Lucas Ferreira",
  "customerPhone": "+5511965432109",
  "serviceTitle": "Barba Completa",
  "servicePrice": 40.00,
  "serviceDurationMinutes": 30,
  "startTime": "2025-10-19T14:00:00Z",
  "endTime": "2025-10-19T14:30:00Z",
  "status": 3,
  "createdAt": "2025-10-18T19:34:22Z",
  "confirmedAt": "2025-10-18T20:15:08Z",
  "cancelledAt": "2025-10-19T09:12:35Z",
  "completedAt": null
}
```

### Response 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Agendamento não encontrado."
}
```

### Response 409 Conflict

Retornado quando o agendamento já foi concluído ou cancelado:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Não é possível cancelar um agendamento que já foi concluído ou cancelado. Status atual: Completed"
}
```

**Possíveis causas**:
- Agendamento já foi cancelado anteriormente
- Agendamento já foi marcado como concluído

**Ação recomendada**: Atualizar a lista de agendamentos (refetch).

---

## 5️⃣ POST /api/appointments/{id}/complete

### Descrição

Marca um agendamento como concluído. Somente agendamentos com status `Confirmed` e cujo horário de início já passou podem ser concluídos.

### Role Necessária

`Barbeiro`

### Path Parameters

| Parâmetro | Tipo | Descrição |
|-----------|------|-----------|
| `id` | UUID | ID do agendamento |

### Request Example

```http
POST /api/appointments/c4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f/complete HTTP/1.1
Host: api.barbapp.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Length: 0
```

**Nota**: Não há body na requisição.

### Response 200 OK

Retorna o agendamento atualizado com status `Completed`:

```json
{
  "id": "c4d5e6f7-a8b9-4c0d-1e2f-3a4b5c6d7e8f",
  "customerName": "Rafael Costa",
  "customerPhone": "+5511954321098",
  "serviceTitle": "Corte + Barba + Sobrancelha",
  "servicePrice": 85.00,
  "serviceDurationMinutes": 90,
  "startTime": "2025-10-19T15:00:00Z",
  "endTime": "2025-10-19T16:30:00Z",
  "status": 2,
  "createdAt": "2025-10-17T11:22:33Z",
  "confirmedAt": "2025-10-17T15:40:11Z",
  "cancelledAt": null,
  "completedAt": "2025-10-19T16:35:42Z"
}
```

### Response 400 Bad Request

Retornado quando o horário de início do agendamento ainda não chegou:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Não é possível concluir um agendamento que ainda não começou. Horário de início: 2025-10-19T17:00:00Z"
}
```

### Response 404 Not Found

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Agendamento não encontrado."
}
```

### Response 409 Conflict

Retornado quando o agendamento não está em status `Confirmed`:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.8",
  "title": "Conflict",
  "status": 409,
  "detail": "Não é possível concluir um agendamento que não está confirmado. Status atual: Pending"
}
```

**Possíveis causas**:
- Agendamento ainda não foi confirmado (status `Pending`)
- Agendamento já foi concluído anteriormente
- Agendamento foi cancelado

**Ação recomendada**: Atualizar a lista de agendamentos (refetch).

---

## 📊 Códigos de Resposta HTTP

| Código | Nome | Quando Ocorre |
|--------|------|---------------|
| `200` | OK | Requisição bem-sucedida |
| `400` | Bad Request | Dados inválidos ou ação não permitida (ex: completar agendamento que não começou) |
| `401` | Unauthorized | Token ausente, inválido ou expirado |
| `403` | Forbidden | Usuário sem permissão (não é barbeiro ou tentando acessar agendamento de outro) |
| `404` | Not Found | Agendamento não encontrado |
| `409` | Conflict | Transição de status inválida (concorrência otimista) |
| `500` | Internal Server Error | Erro inesperado no servidor |

---

## 🔄 Fluxo de Estados

```
┌─────────┐
│ Pending │ (Status 0)
└────┬────┘
     │
     ├──► Confirm ──┐
     │              │
     │         ┌────▼────┐
     │         │Confirmed│ (Status 1)
     │         └────┬────┘
     │              │
     │              └──► Complete ──┐
     │                               │
     │                          ┌────▼────┐
     ├──► Cancel ──────────────►│Completed│ (Status 2)
     │                          └─────────┘
     │                               
     └──────────────────────────────┐
                                    │
                               ┌────▼────┐
                               │Cancelled│ (Status 3)
                               └─────────┘
```

### Transições Válidas

| Estado Atual | Ações Permitidas |
|--------------|------------------|
| `Pending` (0) | Confirmar, Cancelar |
| `Confirmed` (1) | Cancelar, Concluir (após horário de início) |
| `Completed` (2) | Nenhuma (estado final) |
| `Cancelled` (3) | Nenhuma (estado final) |

---

## 🌍 Fuso Horário

**IMPORTANTE**: Todos os horários são retornados em **UTC** (Coordinated Universal Time).

### Conversão para Timezone Local (Frontend)

```typescript
// Converter UTC para timezone do usuário
const localTime = new Date(appointment.startTime).toLocaleString('pt-BR', {
  timeZone: 'America/Sao_Paulo',
  hour: '2-digit',
  minute: '2-digit',
});
// Resultado: "09:00"

// Converter UTC para Date object local
const startDate = new Date(appointment.startTime);
// Automaticamente convertido para timezone local do navegador
```

### Exemplos de Conversão

| UTC | São Paulo (GMT-3) | Brasília |
|-----|-------------------|----------|
| `2025-10-19T09:00:00Z` | `06:00` | `06:00` |
| `2025-10-19T12:00:00Z` | `09:00` | `09:00` |
| `2025-10-19T18:00:00Z` | `15:00` | `15:00` |

**Recomendação**: Use bibliotecas como `date-fns` ou `dayjs` com timezone plugins para conversões consistentes.

---

## 🧪 Exemplos de Uso (TypeScript)

### Fetching da Agenda

```typescript
interface BarberScheduleOutput {
  date: string;
  barberId: string;
  barberName: string;
  appointments: BarberAppointmentOutput[];
}

interface BarberAppointmentOutput {
  id: string;
  customerName: string;
  serviceTitle: string;
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
}

enum AppointmentStatus {
  Pending = 0,
  Confirmed = 1,
  Completed = 2,
  Cancelled = 3,
}

const fetchSchedule = async (date: Date): Promise<BarberScheduleOutput> => {
  const formattedDate = date.toISOString().split('T')[0]; // "2025-10-19"
  
  const response = await fetch(
    `/api/schedule/my-schedule?date=${formattedDate}`,
    {
      headers: {
        'Authorization': `Bearer ${getToken()}`,
      },
    }
  );

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}: ${response.statusText}`);
  }

  return await response.json();
};
```

### Confirmar Agendamento

```typescript
const confirmAppointment = async (appointmentId: string): Promise<AppointmentDetailsOutput> => {
  const response = await fetch(
    `/api/appointments/${appointmentId}/confirm`,
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
      },
    }
  );

  if (response.status === 409) {
    throw new Error('Agendamento já foi modificado. Atualize a lista.');
  }

  if (!response.ok) {
    throw new Error(`HTTP ${response.status}`);
  }

  return await response.json();
};
```

### Tratamento de Erro 409 (Conflict)

```typescript
try {
  await confirmAppointment(appointmentId);
  showSuccessMessage('Agendamento confirmado!');
} catch (error) {
  if (error.message.includes('409')) {
    // Status mudou - atualizar lista
    await refetchSchedule();
    showWarningMessage('O status deste agendamento foi alterado. Lista atualizada.');
  } else {
    showErrorMessage('Erro ao confirmar agendamento.');
  }
}
```

---

## ✅ Validações do Cliente (Frontend)

Antes de chamar as APIs de ação, valide:

### Confirmar
- ✅ Status atual é `Pending` (0)
- ✅ Agendamento não foi cancelado

### Cancelar
- ✅ Status atual é `Pending` (0) ou `Confirmed` (1)
- ✅ Agendamento não foi concluído

### Concluir
- ✅ Status atual é `Confirmed` (1)
- ✅ Horário de início já passou (`startTime < now`)

---

## 📚 Referências

- [RFC 7231 - HTTP Status Codes](https://tools.ietf.org/html/rfc7231)
- [RFC 7235 - HTTP Authentication](https://tools.ietf.org/html/rfc7235)
- [ISO 8601 - Date and Time Format](https://en.wikipedia.org/wiki/ISO_8601)
- [JSON API Best Practices](https://jsonapi.org/)

---

**Última Atualização**: 2025-10-19  
**Versão**: 1.0  
**Autor**: Task 5.0 - Sistema de Agendamentos (Barbeiro)
