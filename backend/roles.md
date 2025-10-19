# Mapeamento de Acessos e Permissões do Sistema BarbApp

Este documento detalha as permissões de cada perfil de usuário (role) no sistema BarbApp, indicando quais controllers e ações cada um pode acessar.

## Perfis de Usuário (Roles)

O sistema define os seguintes perfis de usuário:

- **AdminCentral**: Gerencia todas as barbearias cadastradas no sistema.
- **AdminBarbearia**: Gerencia uma barbearia específica, incluindo seus barbeiros e serviços.
- **Barbeiro**: Profissional que trabalha em uma ou mais barbearias.
- **Cliente**: Cliente final que utiliza os serviços da barbearia.
- **Usuário Autenticado**: Qualquer usuário que tenha feito login no sistema, independentemente do seu perfil.

## Acesso aos Controllers por Perfil

A seguir, a lista de controllers e as permissões de acesso para cada perfil.

---

### 1. `AuthController`

Este controller é responsável pela autenticação dos usuários e outras ações relacionadas à sessão.

| Rota                                | Método | Perfil Permitido      | Descrição                                                                 |
| ----------------------------------- | ------ | --------------------- | ------------------------------------------------------------------------- |
| `/api/auth/admin-central/login`     | `POST` | Público               | Autentica um administrador central.                                       |
| `/api/auth/admin-barbearia/login`   | `POST` | Público               | Autentica um administrador de barbearia.                                  |
| `/api/auth/barbeiro/login`          | `POST` | Público               | Autentica um barbeiro.                                                    |
| `/api/auth/cliente/login`           | `POST` | Público               | Autentica um cliente.                                                     |
| `/api/auth/barbeiros`               | `GET`  | Usuário Autenticado   | Lista os barbeiros da barbearia do usuário autenticado.                   |
| `/api/auth/barbeiro/trocar-contexto`| `POST` | Usuário Autenticado   | Permite que um barbeiro troque o contexto para outra barbearia em que trabalha. |

---

### 2. `BarbershopsController`

Este controller gerencia o cadastro e as informações das barbearias. O acesso é restrito ao perfil `AdminCentral`.

| Rota                          | Método   | Perfil Permitido | Descrição                               |
| ----------------------------- | -------- | ---------------- | --------------------------------------- |
| `/api/barbearias`             | `POST`   | `AdminCentral`   | Cria uma nova barbearia.                |
| `/api/barbearias`             | `GET`    | `AdminCentral`   | Lista todas as barbearias.              |
| `/api/barbearias/{id}`        | `GET`    | `AdminCentral`   | Obtém os dados de uma barbearia.        |
| `/api/barbearias/{id}`        | `PUT`    | `AdminCentral`   | Atualiza os dados de uma barbearia.     |
| `/api/barbearias/{id}`        | `DELETE` | `AdminCentral`   | Exclui uma barbearia.                   |
| `/api/barbearias/{id}/desativar`| `PUT`    | `AdminCentral`   | Desativa uma barbearia (soft delete).   |
| `/api/barbearias/{id}/reativar` | `PUT`    | `AdminCentral`   | Reativa uma barbearia.                  |

---

### 3. `BarbersController`

Este controller gerencia os barbeiros de uma barbearia. O acesso é restrito ao perfil `AdminBarbearia`.

| Rota                   | Método   | Perfil Permitido   | Descrição                                                                 |
| ---------------------- | -------- | ------------------ | ------------------------------------------------------------------------- |
| `/api/barbers`         | `POST`   | `AdminBarbearia`   | Cria um novo barbeiro na barbearia.                                       |
| `/api/barbers`         | `GET`    | `AdminBarbearia`   | Lista os barbeiros da barbearia.                                          |
| `/api/barbers/{id}`    | `GET`    | `AdminBarbearia`   | Obtém os detalhes de um barbeiro.                                         |
| `/api/barbers/{id}`    | `PUT`    | `AdminBarbearia`   | Atualiza as informações de um barbeiro.                                   |
| `/api/barbers/{id}`    | `DELETE` | `AdminBarbearia`   | Remove um barbeiro da barbearia.                                          |
| `/api/barbers/schedule`| `GET`    | `AdminBarbearia`   | Obtém a agenda consolidada de todos os barbeiros da barbearia.            |

---

### 4. `BarbershopServicesController`

Este controller gerencia os serviços oferecidos por uma barbearia. O acesso é restrito ao perfil `AdminBarbearia`.

| Rota                         | Método   | Perfil Permitido   | Descrição                               |
| ---------------------------- | -------- | ------------------ | --------------------------------------- |
| `/api/barbershop-services`   | `POST`   | `AdminBarbearia`   | Cria um novo serviço na barbearia.      |
| `/api/barbershop-services`   | `GET`    | `AdminBarbearia`   | Lista os serviços da barbearia.         |
| `/api/barbershop-services/{id}`| `GET`    | `AdminBarbearia`   | Obtém os detalhes de um serviço.        |
| `/api/barbershop-services/{id}`| `PUT`    | `AdminBarbearia`   | Atualiza as informações de um serviço.  |
| `/api/barbershop-services/{id}`| `DELETE` | `AdminBarbearia`   | Remove um serviço da barbearia.         |

---

### 5. `ScheduleController`

Este controller permite que barbeiros visualizem suas agendas. O acesso é restrito ao perfil `Barbeiro`.

| Rota                        | Método | Perfil Permitido | Descrição                                                                 |
| --------------------------- | ------ | ---------------- | ------------------------------------------------------------------------- |
| `/api/schedule/my-schedule` | `GET`  | `Barbeiro`       | Obtém a agenda do barbeiro autenticado para uma data específica.          |

---

### 6. `AppointmentsController`

Este controller permite que barbeiros gerenciem seus agendamentos (visualizar, confirmar, cancelar e concluir). O acesso é restrito ao perfil `Barbeiro`.

| Rota                            | Método | Perfil Permitido | Descrição                                                                 |
| ------------------------------- | ------ | ---------------- | ------------------------------------------------------------------------- |
| `/api/appointments/{id}`        | `GET`  | `Barbeiro`       | Obtém detalhes de um agendamento específico do barbeiro.                  |
| `/api/appointments/{id}/confirm`| `POST` | `Barbeiro`       | Confirma um agendamento pendente.                                         |
| `/api/appointments/{id}/cancel` | `POST` | `Barbeiro`       | Cancela um agendamento (pendente ou confirmado).                          |
| `/api/appointments/{id}/complete`| `POST` | `Barbeiro`      | Marca um agendamento confirmado como concluído.                           |
