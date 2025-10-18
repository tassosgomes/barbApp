# Documentação da API BarbApp

Este documento detalha todos os endpoints da API BarbApp, incluindo descrições, roles necessárias, parâmetros de entrada e saída.

## Autenticação (`/api/auth`)

### `POST /api/auth/admin-central/login`

- **Descrição**: Autentica um usuário com perfil de "AdminCentral". O sistema verifica o e-mail e a senha fornecidos. Se as credenciais forem válidas, um token JWT é gerado e retornado, permitindo acesso aos endpoints protegidos para administradores centrais.
- **Role Necessária**: Nenhuma (Público).
- **Parâmetros de Entrada**:
  - `Email` (string): E-mail do administrador central.
  - `Senha` (string): Senha do administrador central.
- **Parâmetros de Saída**:
  - `Token` (string): Token JWT para autenticação.
  - `TipoUsuario` (string): "AdminCentral".
  - `BarbeariaId` (Guid?): `null` para este tipo de usuário.
  - `NomeBarbearia` (string): String vazia.
  - `CodigoBarbearia` (string?): `null`.
  - `ExpiresAt` (DateTime): Data e hora de expiração do token.

### `POST /api/auth/admin-barbearia/login`

- **Descrição**: Autentica um usuário com perfil de "AdminBarbearia". O sistema valida o código da barbearia e verifica se ela está ativa. Em seguida, valida o e-mail e a senha do administrador dentro do contexto daquela barbearia. Se tudo for válido, um token JWT é gerado com o `BarbeariaId` no contexto.
- **Role Necessária**: Nenhuma (Público).
- **Parâmetros de Entrada**:
  - `Codigo` (string): Código único da barbearia.
  - `Email` (string): E-mail do administrador da barbearia.
  - `Senha` (string): Senha do administrador.
- **Parâmetros de Saída**:
  - `Token` (string): Token JWT para autenticação.
  - `TipoUsuario` (string): "AdminBarbearia".
  - `BarbeariaId` (Guid?): ID da barbearia autenticada.
  - `NomeBarbearia` (string): Nome da barbearia.
  - `CodigoBarbearia` (string?): Código da barbearia.
  - `ExpiresAt` (DateTime): Data e hora de expiração do token.

### `POST /api/auth/barbeiro/login`

- **Descrição**: Autentica um usuário com perfil de "Barbeiro". O sistema valida o código da barbearia e o telefone do barbeiro. Se o barbeiro for encontrado e estiver associado à barbearia, um token JWT é gerado.
- **Role Necessária**: Nenhuma (Público).
- **Parâmetros de Entrada**:
  - `Codigo` (string): Código único da barbearia.
  - `Telefone` (string): Número de telefone do barbeiro.
- **Parâmetros de Saída**:
  - `Token` (string): Token JWT para autenticação.
  - `TipoUsuario` (string): "Barbeiro".
  - `BarbeariaId` (Guid?): ID da barbearia autenticada.
  - `NomeBarbearia` (string): Nome da barbearia.
  - `ExpiresAt` (DateTime): Data e hora de expiração do token.

### `POST /api/auth/cliente/login`

- **Descrição**: Autentica um "Cliente". O sistema valida o código da barbearia. Se um cliente com o telefone informado já existir, o nome é validado. Se não existir, um novo cliente é criado. Um token JWT é gerado para o cliente no contexto da barbearia.
- **Role Necessária**: Nenhuma (Público).
- **Parâmetros de Entrada**:
  - `Codigo` (string): Código único da barbearia.
  - `Telefone` (string): Número de telefone do cliente.
  - `Nome` (string): Nome do cliente para validação ou criação.
- **Parâmetros de Saída**:
  - `Token` (string): Token JWT para autenticação.
  - `TipoUsuario` (string): "Cliente".
  - `BarbeariaId` (Guid?): ID da barbearia autenticada.
  - `NomeBarbearia` (string): Nome da barbearia.
  - `ExpiresAt` (DateTime): Data e hora de expiração do token.

### `GET /api/auth/barbeiros`

- **Descrição**: Lista todos os barbeiros associados à barbearia do usuário autenticado. O `BarbeariaId` é extraído do token JWT do usuário.
- **Role Necessária**: `AdminBarbearia` ou `Barbeiro` (qualquer usuário autenticado com `BarbeariaId` no token).
- **Parâmetros de Entrada**: Nenhum.
- **Parâmetros de Saída**: `IEnumerable<BarberInfo>`
  - `Id` (Guid): ID do barbeiro.
  - `Name` (string): Nome do barbeiro.

### `POST /api/auth/barbeiro/trocar-contexto`

- **Descrição**: Permite que um barbeiro que trabalha em múltiplas barbearias troque seu contexto de trabalho. O sistema valida se o barbeiro autenticado também pertence à `NovaBarbeariaId` e gera um novo token JWT com o contexto da nova barbearia.
- **Role Necessária**: `Barbeiro`.
- **Parâmetros de Entrada**:
  - `NovaBarbeariaId` (Guid): ID da nova barbearia para a qual o barbeiro deseja trocar.
- **Parâmetros de Saída**: `AuthResponse` (similar ao login, com o contexto da nova barbearia).

---

## Barbearias (`/api/barbearias`)

### `GET /api/barbearias/codigo/{codigo}`

- **Descrição**: Endpoint público para validar um código de barbearia. Usado principalmente na tela de login para confirmar que uma barbearia existe e está ativa antes de permitir a autenticação.
- **Role Necessária**: Nenhuma (Público).
- **Parâmetros de Entrada**:
  - `codigo` (string, na rota): Código de 8 caracteres da barbearia.
- **Parâmetros de Saída**: `ValidateBarbeariaCodeResponse`
  - `Id` (Guid): ID da barbearia.
  - `Nome` (string): Nome da barbearia.
  - `Codigo` (string): Código da barbearia.
  - `IsActive` (bool): Status da barbearia.

### `GET /api/barbearias/me`

- **Descrição**: Retorna os dados completos da barbearia associada ao usuário `AdminBarbearia` autenticado. O ID da barbearia é obtido a partir do token JWT.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**: Nenhum.
- **Parâmetros de Saída**: `BarbershopOutput` (detalhes completos da barbearia).

### `POST /api/barbearias`

- **Descrição**: Cria uma nova barbearia. Este processo inclui a criação de um endereço, a geração de um código único, a criação da entidade da barbearia e a criação de um usuário `AdminBarbearia` com uma senha segura gerada automaticamente. As credenciais são enviadas por e-mail para o administrador.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**: `CreateBarbershopInput` (dados da barbearia, endereço e informações do proprietário).
- **Parâmetros de Saída**: `BarbershopOutput` (detalhes da barbearia criada, incluindo o código gerado).

### `GET /api/barbearias`

- **Descrição**: Lista todas as barbearias cadastradas no sistema com suporte a paginação, filtros (ativo/inativo) e busca por termo (nome, código, documento).
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada (Query)**:
  - `page` (int): Número da página.
  - `pageSize` (int): Itens por página.
  - `searchTerm` (string): Termo para busca.
  - `isActive` (bool?): Filtrar por status.
  - `sortBy` (string): Campo para ordenação (`name`, `createdAt`).
- **Parâmetros de Saída**: `PaginatedBarbershopsOutput`.

### `GET /api/barbearias/{id}`

- **Descrição**: Retorna os detalhes completos de uma barbearia específica pelo seu ID.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
- **Parâmetros de Saída**: `BarbershopOutput`.

### `PUT /api/barbearias/{id}`

- **Descrição**: Atualiza as informações de uma barbearia existente, incluindo seus dados de endereço.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
  - `UpdateBarbershopInput` (body): Dados atualizados da barbearia.
- **Parâmetros de Saída**: `BarbershopOutput`.

### `PUT /api/barbearias/{id}/desativar`

- **Descrição**: Realiza um "soft delete" de uma barbearia, marcando-a como inativa.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
- **Parâmetros de Saída**: `204 No Content`.

### `PUT /api/barbearias/{id}/reativar`

- **Descrição**: Reativa uma barbearia que foi previamente desativada.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
- **Parâmetros de Saída**: `204 No Content`.

### `DELETE /api/barbearias/{id}`

- **Descrição**: Exclui permanentemente uma barbearia do sistema.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
- **Parâmetros de Saída**: `204 No Content`.

### `POST /api/barbearias/{id}/reenviar-credenciais`

- **Descrição**: Gera uma nova senha para o administrador da barbearia especificada e a envia por e-mail.
- **Role Necessária**: `AdminCentral`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID da barbearia.
- **Parâmetros de Saída**: Objeto JSON com mensagem de sucesso.

---

## Barbeiros (`/api/barbers`)

### `POST /api/barbers`

- **Descrição**: Cria um novo barbeiro, associando-o à barbearia do `AdminBarbearia` autenticado. O sistema verifica se o e-mail já está em uso na mesma barbearia e faz o hash da senha antes de salvar.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**: `CreateBarberInput`
  - `Name` (string): Nome do barbeiro.
  - `Email` (string): E-mail do barbeiro.
  - `Password` (string): Senha para o barbeiro.
  - `Phone` (string): Telefone do barbeiro.
  - `ServiceIds` (List<Guid>): Lista de IDs dos serviços que o barbeiro realiza.
- **Parâmetros de Saída**: `BarberOutput`.

### `GET /api/barbers`

- **Descrição**: Lista os barbeiros da barbearia do `AdminBarbearia` autenticado. Suporta paginação, filtro por status (ativo/inativo) e busca por nome.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada (Query)**:
  - `isActive` (bool?): Filtrar por barbeiros ativos ou inativos.
  - `searchName` (string): Termo para buscar no nome do barbeiro.
  - `page` (int): Número da página.
  - `pageSize` (int): Itens por página.
- **Parâmetros de Saída**: `PaginatedBarbersOutput`.

### `GET /api/barbers/{id}`

- **Descrição**: Retorna os detalhes de um barbeiro específico, garantindo que ele pertença à barbearia do `AdminBarbearia` autenticado.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do barbeiro.
- **Parâmetros de Saída**: `BarberOutput`.

### `PUT /api/barbers/{id}`

- **Descrição**: Atualiza as informações de um barbeiro (nome, telefone e serviços associados).
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do barbeiro.
  - `UpdateBarberInput` (body): Dados a serem atualizados.
- **Parâmetros de Saída**: `BarberOutput`.

### `DELETE /api/barbers/{id}`

- **Descrição**: Desativa um barbeiro (soft delete) e cancela todos os seus agendamentos futuros.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do barbeiro.
- **Parâmetros de Saída**: `204 No Content`.

### `GET /api/barbers/schedule`

- **Descrição**: Retorna a agenda consolidada de todos os barbeiros da barbearia para uma data específica. Pode ser filtrada por um barbeiro específico.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada (Query)**:
  - `date` (DateTime?): Data para a qual a agenda deve ser retornada.
  - `barberId` (Guid?): ID opcional para filtrar a agenda de um único barbeiro.
- **Parâmetros de Saída**: `TeamScheduleOutput`.

---

## Serviços da Barbearia (`/api/barbershop-services`)

### `POST /api/barbershop-services`

- **Descrição**: Cria um novo serviço (ex: "Corte de Cabelo", "Barba") para a barbearia do `AdminBarbearia` autenticado.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**: `CreateBarbershopServiceInput`
  - `Name` (string): Nome do serviço.
  - `Description` (string?): Descrição opcional.
  - `DurationMinutes` (int): Duração do serviço em minutos.
  - `Price` (decimal): Preço do serviço.
- **Parâmetros de Saída**: `BarbershopServiceOutput`.

### `GET /api/barbershop-services`

- **Descrição**: Lista os serviços da barbearia do `AdminBarbearia` autenticado. Suporta paginação, filtro por status (ativo/inativo) e busca por nome.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada (Query)**:
  - `isActive` (bool?): Filtrar por serviços ativos ou inativos.
  - `searchName` (string): Termo para buscar no nome do serviço.
  - `page` (int): Número da página.
  - `pageSize` (int): Itens por página.
- **Parâmetros de Saída**: `PaginatedBarbershopServicesOutput`.

### `GET /api/barbershop-services/{id}`

- **Descrição**: Retorna os detalhes de um serviço específico, garantindo que ele pertença à barbearia do `AdminBarbearia` autenticado.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do serviço.
- **Parâmetros de Saída**: `BarbershopServiceOutput`.

### `PUT /api/barbershop-services/{id}`

- **Descrição**: Atualiza as informações de um serviço.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do serviço.
  - `UpdateBarbershopServiceInput` (body): Dados a serem atualizados.
- **Parâmetros de Saída**: `BarbershopServiceOutput`.

### `DELETE /api/barbershop-services/{id}`

- **Descrição**: Desativa um serviço (soft delete), tornando-o indisponível para novos agendamentos.
- **Role Necessária**: `AdminBarbearia`.
- **Parâmetros de Entrada**:
  - `id` (Guid, na rota): ID do serviço.
- **Parâmetros de Saída**: `204 No Content`.
