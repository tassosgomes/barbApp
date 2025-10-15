# Contratos de API - Gestão de Barbeiros (Admin da Barbearia)

Este documento descreve os contratos JSON de entrada e saída para todos os endpoints relacionados à gestão de barbeiros e serviços de barbearia. Estes contratos são utilizados para integração com o frontend.

## Autenticação
Todos os endpoints requerem autenticação com role `AdminBarbearia` via JWT token no header `Authorization: Bearer {token}`.

## Endpoints de Barbeiros

### 1. Criar Barbeiro
**Endpoint:** `POST /api/barbers`  
**Descrição:** Cria um novo barbeiro na barbearia.

#### Request Body
```json
{
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "password": "Password123!",
  "phone": "(11) 98765-4321",
  "serviceIds": ["550e8400-e29b-41d4-a716-446655440000", "550e8400-e29b-41d4-a716-446655440001"]
}
```

#### Response 201 Created
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "phoneFormatted": "(11) 98765-4321",
  "services": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Corte de Cabelo",
      "description": "Corte masculino completo",
      "durationMinutes": 30,
      "price": 25.00,
      "isActive": true
    }
  ],
  "isActive": true,
  "createdAt": "2025-10-15T10:00:00Z"
}
```

### 2. Listar Barbeiros
**Endpoint:** `GET /api/barbers?isActive=true&searchName=João&page=1&pageSize=20`  
**Descrição:** Lista barbeiros da barbearia com paginação e filtros.

#### Query Parameters
- `isActive` (boolean, opcional): Filtrar por status ativo/inativo
- `searchName` (string, opcional): Buscar por nome
- `page` (int, padrão: 1): Página
- `pageSize` (int, padrão: 20, máximo: 100): Itens por página

#### Response 200 OK
```json
{
  "barbers": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "João Silva",
      "email": "joao.silva@email.com",
      "phoneFormatted": "(11) 98765-4321",
      "services": [
        {
          "id": "550e8400-e29b-41d4-a716-446655440000",
          "name": "Corte de Cabelo",
          "description": "Corte masculino completo",
          "durationMinutes": 30,
          "price": 25.00,
          "isActive": true
        }
      ],
      "isActive": true,
      "createdAt": "2025-10-15T10:00:00Z"
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### 3. Obter Barbeiro por ID
**Endpoint:** `GET /api/barbers/{id}`  
**Descrição:** Obtém detalhes de um barbeiro específico.

#### Response 200 OK
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "João Silva",
  "email": "joao.silva@email.com",
  "phoneFormatted": "(11) 98765-4321",
  "services": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Corte de Cabelo",
      "description": "Corte masculino completo",
      "durationMinutes": 30,
      "price": 25.00,
      "isActive": true
    }
  ],
  "isActive": true,
  "createdAt": "2025-10-15T10:00:00Z"
}
```

### 4. Atualizar Barbeiro
**Endpoint:** `PUT /api/barbers/{id}`  
**Descrição:** Atualiza informações de um barbeiro.

#### Request Body
```json
{
  "name": "João Silva Santos",
  "phone": "(11) 98765-4321",
  "serviceIds": ["550e8400-e29b-41d4-a716-446655440000"]
}
```

#### Response 200 OK
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "João Silva Santos",
  "email": "joao.silva@email.com",
  "phoneFormatted": "(11) 98765-4321",
  "services": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Corte de Cabelo",
      "description": "Corte masculino completo",
      "durationMinutes": 30,
      "price": 25.00,
      "isActive": true
    }
  ],
  "isActive": true,
  "createdAt": "2025-10-15T10:00:00Z"
}
```

### 5. Remover Barbeiro
**Endpoint:** `DELETE /api/barbers/{id}`  
**Descrição:** Remove um barbeiro da barbearia (desativa e cancela agendamentos futuros).

#### Response 204 No Content

### 6. Obter Agenda da Equipe
**Endpoint:** `GET /api/barbers/schedule?date=2025-10-15&barberId=550e8400-e29b-41d4-a716-446655440000`  
**Descrição:** Obtém agenda consolidada de todos os barbeiros da barbearia.

#### Query Parameters
- `date` (DateTime, opcional): Data para filtrar (formato: YYYY-MM-DD)
- `barberId` (Guid, opcional): ID do barbeiro para filtrar

#### Response 200 OK
```json
{
  "appointments": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440001",
      "barberId": "550e8400-e29b-41d4-a716-446655440000",
      "barberName": "João Silva",
      "customerId": "550e8400-e29b-41d4-a716-446655440002",
      "customerName": "Carlos Oliveira",
      "startTime": "2025-10-15T14:00:00Z",
      "endTime": "2025-10-15T14:30:00Z",
      "serviceName": "Corte de Cabelo",
      "status": "Confirmed"
    }
  ]
}
```

## Endpoints de Serviços de Barbearia

### 1. Criar Serviço
**Endpoint:** `POST /api/barbershop-services`  
**Descrição:** Cria um novo serviço oferecido pela barbearia.

#### Request Body
```json
{
  "name": "Corte de Cabelo",
  "description": "Corte masculino completo com lavagem",
  "durationMinutes": 30,
  "price": 25.00
}
```

#### Response 201 Created
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Corte de Cabelo",
  "description": "Corte masculino completo com lavagem",
  "durationMinutes": 30,
  "price": 25.00,
  "isActive": true
}
```

### 2. Listar Serviços
**Endpoint:** `GET /api/barbershop-services?isActive=true&searchName=Corte&page=1&pageSize=20`  
**Descrição:** Lista serviços oferecidos pela barbearia com paginação e filtros.

#### Query Parameters
- `isActive` (boolean, opcional): Filtrar por status ativo/inativo
- `searchName` (string, opcional): Buscar por nome
- `page` (int, padrão: 1): Página
- `pageSize` (int, padrão: 20, máximo: 100): Itens por página

#### Response 200 OK
```json
{
  "services": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Corte de Cabelo",
      "description": "Corte masculino completo com lavagem",
      "durationMinutes": 30,
      "price": 25.00,
      "isActive": true
    }
  ],
  "totalCount": 1,
  "page": 1,
  "pageSize": 20
}
```

### 3. Obter Serviço por ID
**Endpoint:** `GET /api/barbershop-services/{id}`  
**Descrição:** Obtém detalhes de um serviço específico.

#### Response 200 OK
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Corte de Cabelo",
  "description": "Corte masculino completo com lavagem",
  "durationMinutes": 30,
  "price": 25.00,
  "isActive": true
}
```

### 4. Atualizar Serviço
**Endpoint:** `PUT /api/barbershop-services/{id}`  
**Descrição:** Atualiza informações de um serviço.

#### Request Body
```json
{
  "name": "Corte de Cabelo Premium",
  "description": "Corte masculino completo com lavagem e barba",
  "durationMinutes": 45,
  "price": 35.00
}
```

#### Response 200 OK
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "Corte de Cabelo Premium",
  "description": "Corte masculino completo com lavagem e barba",
  "durationMinutes": 45,
  "price": 35.00,
  "isActive": true
}
```

### 5. Deletar Serviço
**Endpoint:** `DELETE /api/barbershop-services/{id}`  
**Descrição:** Remove um serviço oferecido pela barbearia.

#### Response 204 No Content

## Códigos de Status HTTP

- `200 OK`: Requisição bem-sucedida
- `201 Created`: Recurso criado com sucesso
- `204 No Content`: Operação bem-sucedida sem conteúdo de resposta
- `400 Bad Request`: Dados de entrada inválidos
- `401 Unauthorized`: Token inválido ou ausente
- `403 Forbidden`: Usuário não tem permissão
- `404 Not Found`: Recurso não encontrado
- `409 Conflict`: Conflito (ex: email duplicado)
- `422 Unprocessable Entity`: Erro de validação de negócio

## Validações

### Campos Obrigatórios
- `name`: Não vazio, máximo 100 caracteres
- `email`: Formato válido de email
- `password`: Mínimo 8 caracteres (apenas na criação)
- `phone`: Formato brasileiro válido
- `serviceIds`: Lista de GUIDs válidos

### Regras de Negócio
- Email deve ser único por barbearia
- Nome do serviço deve ser único por barbearia
- Duração deve ser positiva
- Preço deve ser não-negativo

## Notas para Frontend

- Todos os endpoints requerem autenticação JWT
- O `barbeariaId` é extraído automaticamente do token
- Datas são retornadas em formato ISO 8601 (UTC)
- Telefones são formatados no padrão brasileiro
- Paginação usa `page` e `pageSize` para controle
- Filtros são aplicados via query parameters
- Agenda usa polling de 30 segundos conforme especificado</content>
<parameter name="filePath">/home/tsgomes/github-tassosgomes/barbApp/docs/api-contracts-barbers-management.md