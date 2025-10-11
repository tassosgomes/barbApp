# Especificação Técnica - Cadastro e Agendamento (Cliente)

## Resumo Executivo

Esta especificação técnica define a implementação do módulo de Cadastro e Agendamento para Clientes no barbApp, um sistema multi-tenant SaaS para barbearias. A solução utiliza arquitetura Clean Architecture com .NET 8 no backend e React + TypeScript no frontend, garantindo isolamento total de dados entre barbearias através de filtros automáticos por `barbeariaId`. A autenticação simplificada (telefone + nome, sem senha) reduz o atrito para MVP, enquanto JWT com contexto multi-tenant garante segurança e isolamento. O algoritmo de cálculo de disponibilidade em tempo real com cache de 5 minutos e lock otimista previne conflitos de agendamento. A arquitetura prioriza simplicidade, testabilidade e escalabilidade horizontal.

**Decisões Arquiteturais Principais:**
- Multi-tenancy via shared database com discriminador `barbeariaId` e Global Query Filters
- Autenticação JWT com contexto da barbearia no payload
- Cálculo de disponibilidade em tempo real com cache em memória (5min TTL)
- Lock otimista para prevenção de conflitos de agendamento
- Frontend mobile-first com React Query para state management e cache

## Arquitetura do Sistema

### Visão Geral dos Componentes

```
┌─────────────────────────────────────────────────────────────┐
│                        Frontend (React)                      │
│  Pages: Cadastro, Login, Seleção, Calendário, Agendamentos │
│  Hooks: useAuth, useBarbearia, useAgendamentos              │
│  Context: AuthContext (token, cliente, barbearia)           │
│  HTTP Client: Axios + React Query                           │
└────────────────────┬────────────────────────────────────────┘
                     │ HTTPS/REST API
┌────────────────────┴────────────────────────────────────────┐
│                    API Layer (.NET 8)                        │
│  Controllers: AuthClienteController, BarbeirosController,   │
│              AgendamentosController, ServicosController      │
│  Middleware: JWT Authentication, Tenant Context Resolver    │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│              Application Layer (Use Cases)                   │
│  - CadastrarClienteUseCase                                  │
│  - LoginClienteUseCase                                      │
│  - ListarBarbeirosUseCase                                   │
│  - ConsultarDisponibilidadeUseCase (+ Cache)               │
│  - CriarAgendamentoUseCase (+ Lock Otimista)               │
│  - ListarAgendamentosUseCase                                │
│  - CancelarAgendamentoUseCase                               │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│                  Domain Layer (Entidades)                    │
│  Entities: Cliente, Agendamento, Barbeiro, Servico         │
│  Value Objects: Telefone, StatusAgendamento                 │
│  Domain Events: AgendamentoCriado, AgendamentoCancelado    │
└────────────────────┬────────────────────────────────────────┘
                     │
┌────────────────────┴────────────────────────────────────────┐
│            Infrastructure Layer (Persistência)               │
│  EF Core DbContext com Global Query Filters                │
│  Repositories: IClienteRepository, IAgendamentoRepository   │
│  Unit of Work: Transações e coordenação                    │
│  JWT Service: Geração e validação de tokens                │
│  Cache Service: In-memory para disponibilidade             │
└────────────────────┬────────────────────────────────────────┘
                     │
              ┌──────┴──────┐
              │  PostgreSQL  │
              │  Multi-tenant│
              └──────────────┘
```

**Fluxo de Dados Principal (Criar Agendamento):**
1. Frontend → POST /api/agendamentos (com JWT no header)
2. Middleware valida JWT e extrai barbeariaId
3. Controller injeta contexto no use case
4. CriarAgendamentoUseCase valida disponibilidade
5. Repository aplica filtro automático por barbeariaId
6. Transaction garante atomicidade (verificação + inserção)
7. Unit of Work persiste mudanças
8. Response retorna agendamento criado

**Isolamento Multi-tenant:**
- Todas as queries filtradas automaticamente por `barbeariaId`
- JWT contém `barbeariaId` no payload
- EF Core Global Query Filter: `.HasQueryFilter(e => e.BarbeariaId == _currentBarbeariaId)`
- Validação dupla: token contém barbeariaId X recurso solicitado pertence à mesma barbearia

## Design de Implementação

### Interfaces Principais

```csharp
// Application Layer - Use Cases
public interface ICadastrarClienteUseCase
{
    Task<CadastroClienteOutput> Handle(
        CadastrarClienteInput input,
        CancellationToken cancellationToken
    );
}

public interface IConsultarDisponibilidadeUseCase
{
    Task<DisponibilidadeOutput> Handle(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken
    );
}

public interface ICriarAgendamentoUseCase
{
    Task<AgendamentoOutput> Handle(
        CriarAgendamentoInput input,
        CancellationToken cancellationToken
    );
}

// Infrastructure Layer - Repositories
public interface IClienteRepository
{
    Task<Cliente?> GetByTelefoneAsync(
        string telefone,
        Guid barbeariaId,
        CancellationToken cancellationToken
    );

    Task InsertAsync(
        Cliente cliente,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsByTelefoneAsync(
        string telefone,
        Guid barbeariaId,
        CancellationToken cancellationToken
    );
}

public interface IAgendamentoRepository
{
    Task<List<Agendamento>> GetByBarbeiroAndDateRangeAsync(
        Guid barbeiroId,
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken
    );

    Task<bool> ExistsConflictAsync(
        Guid barbeiroId,
        DateTime dataHora,
        int duracaoMinutos,
        CancellationToken cancellationToken
    );

    Task InsertAsync(
        Agendamento agendamento,
        CancellationToken cancellationToken
    );

    Task<List<Agendamento>> GetByClienteAsync(
        Guid clienteId,
        StatusAgendamento? status,
        CancellationToken cancellationToken
    );
}

// Infrastructure Layer - Services
public interface IJwtService
{
    string GenerateToken(
        Guid userId,
        string role,
        Guid? barbeariaId,
        Dictionary<string, string>? additionalClaims = null
    );

    ClaimsPrincipal? ValidateToken(string token);
}

public interface IDisponibilidadeCache
{
    Task<DisponibilidadeOutput?> GetAsync(
        string key,
        CancellationToken cancellationToken
    );

    Task SetAsync(
        string key,
        DisponibilidadeOutput value,
        TimeSpan expiration,
        CancellationToken cancellationToken
    );
}
```

### Modelos de Dados

#### Entidades de Domínio

```csharp
// Domain Layer
public class Cliente : Entity
{
    public Guid ClienteId { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public string Nome { get; private set; }
    public string Telefone { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Barbearia Barbearia { get; private set; }
    public List<Agendamento> Agendamentos { get; private set; }

    private Cliente() { } // EF Core

    public Cliente(Guid barbeariaId, string nome, string telefone)
    {
        ClienteId = Guid.NewGuid();
        BarbeariaId = barbeariaId;
        Nome = ValidarNome(nome);
        Telefone = ValidarTelefone(telefone);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("Nome é obrigatório");
        return nome.Trim();
    }

    private string ValidarTelefone(string telefone)
    {
        var apenasNumeros = new string(telefone.Where(char.IsDigit).ToArray());
        if (apenasNumeros.Length < 10 || apenasNumeros.Length > 11)
            throw new DomainException("Telefone inválido");
        return apenasNumeros;
    }

    public bool ValidarNomeLogin(string nome)
    {
        return Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase);
    }
}

public class Agendamento : Entity
{
    public Guid AgendamentoId { get; private set; }
    public Guid BarbeariaId { get; private set; }
    public Guid ClienteId { get; private set; }
    public Guid BarbeiroId { get; private set; }
    public Guid ServicoId { get; private set; }
    public DateTime DataHora { get; private set; }
    public int DuracaoMinutos { get; private set; }
    public StatusAgendamento Status { get; private set; }
    public DateTime? DataCancelamento { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public Cliente Cliente { get; private set; }
    public Barbeiro Barbeiro { get; private set; }
    public Servico Servico { get; private set; }

    private Agendamento() { } // EF Core

    public Agendamento(
        Guid barbeariaId,
        Guid clienteId,
        Guid barbeiroId,
        Guid servicoId,
        DateTime dataHora,
        int duracaoMinutos
    )
    {
        AgendamentoId = Guid.NewGuid();
        BarbeariaId = barbeariaId;
        ClienteId = clienteId;
        BarbeiroId = barbeiroId;
        ServicoId = servicoId;
        DataHora = ValidarDataHoraFutura(dataHora);
        DuracaoMinutos = ValidarDuracao(duracaoMinutos);
        Status = StatusAgendamento.Pendente;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    private DateTime ValidarDataHoraFutura(DateTime dataHora)
    {
        if (dataHora <= DateTime.UtcNow)
            throw new DomainException("Data/hora deve ser futura");
        return dataHora;
    }

    private int ValidarDuracao(int duracaoMinutos)
    {
        if (duracaoMinutos <= 0 || duracaoMinutos > 480)
            throw new DomainException("Duração inválida");
        return duracaoMinutos;
    }

    public void Confirmar()
    {
        if (Status != StatusAgendamento.Pendente)
            throw new DomainException("Apenas agendamentos pendentes podem ser confirmados");
        Status = StatusAgendamento.Confirmado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar()
    {
        if (Status == StatusAgendamento.Concluido || Status == StatusAgendamento.Cancelado)
            throw new DomainException("Agendamento já finalizado");
        if (DataHora <= DateTime.UtcNow)
            throw new DomainException("Não é possível cancelar agendamento passado");
        Status = StatusAgendamento.Cancelado;
        DataCancelamento = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Concluir()
    {
        if (Status != StatusAgendamento.Confirmado)
            throw new DomainException("Apenas agendamentos confirmados podem ser concluídos");
        Status = StatusAgendamento.Concluido;
        UpdatedAt = DateTime.UtcNow;
    }
}

public enum StatusAgendamento
{
    Pendente = 1,
    Confirmado = 2,
    Concluido = 3,
    Cancelado = 4
}
```

#### Esquema de Banco de Dados (PostgreSQL)

```sql
-- Tabela clientes
CREATE TABLE clientes (
    cliente_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbearias(barbearia_id),
    nome TEXT NOT NULL,
    telefone TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT uk_clientes_telefone_barbearia UNIQUE (telefone, barbearia_id)
);

CREATE INDEX idx_clientes_barbearia ON clientes(barbearia_id);
CREATE INDEX idx_clientes_telefone_barbearia ON clientes(telefone, barbearia_id);

-- Tabela agendamentos
CREATE TABLE agendamentos (
    agendamento_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL REFERENCES barbearias(barbearia_id),
    cliente_id UUID NOT NULL REFERENCES clientes(cliente_id),
    barbeiro_id UUID NOT NULL REFERENCES barbeiros(barbeiro_id),
    servico_id UUID NOT NULL REFERENCES servicos(servico_id),
    data_hora TIMESTAMP NOT NULL,
    duracao_minutos INT NOT NULL,
    status INT NOT NULL DEFAULT 1,
    data_cancelamento TIMESTAMP NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_agendamentos_barbearia ON agendamentos(barbearia_id);
CREATE INDEX idx_agendamentos_barbeiro_data ON agendamentos(barbeiro_id, data_hora);
CREATE INDEX idx_agendamentos_cliente_status ON agendamentos(cliente_id, status);
CREATE INDEX idx_agendamentos_data_hora ON agendamentos(data_hora);
```

#### DTOs (Request/Response)

```csharp
// Application Layer - Input/Output DTOs
public record CadastrarClienteInput(
    string CodigoBarbearia,
    string Nome,
    string Telefone
);

public record CadastroClienteOutput(
    string Token,
    ClienteDto Cliente,
    BarbeariaDto Barbearia
);

public record LoginClienteInput(
    string CodigoBarbearia,
    string Telefone,
    string Nome
);

public record CriarAgendamentoInput(
    Guid BarbeiroId,
    Guid ServicoId,
    DateTime DataHora
);

public record AgendamentoOutput(
    Guid Id,
    BarbeiroDto Barbeiro,
    ServicoDto Servico,
    DateTime DataHora,
    int DuracaoMinutos,
    string Status
);

public record DisponibilidadeOutput(
    BarbeiroDto Barbeiro,
    List<DiaDisponivel> DiasDisponiveis
);

public record DiaDisponivel(
    DateTime Data,
    List<string> HorariosDisponiveis
);

public record ClienteDto(
    Guid Id,
    string Nome,
    string Telefone
);

public record BarbeiroDto(
    Guid Id,
    string Nome,
    List<string> Especialidades
);

public record ServicoDto(
    Guid Id,
    string Nome,
    string Descricao,
    int DuracaoMinutos
);

public record BarbeariaDto(
    Guid Id,
    string Nome,
    string Codigo
);
```

### Endpoints de API

#### Autenticação Cliente

**POST /api/auth/cliente/cadastro**
- Descrição: Cadastrar novo cliente em barbearia específica
- Headers: Nenhum (público)
- Request Body:
```json
{
  "codigoBarbearia": "XYZ123AB",
  "nome": "João Silva",
  "telefone": "11987654321"
}
```
- Response 201:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "cliente": {
    "id": "uuid",
    "nome": "João Silva",
    "telefone": "11987654321"
  },
  "barbearia": {
    "id": "uuid",
    "nome": "Barbearia XYZ",
    "codigo": "XYZ123AB"
  }
}
```
- Response 404: Código de barbearia não encontrado
- Response 422: Telefone já cadastrado nesta barbearia

**POST /api/auth/cliente/login**
- Descrição: Fazer login em barbearia específica
- Headers: Nenhum (público)
- Request Body:
```json
{
  "codigoBarbearia": "XYZ123AB",
  "telefone": "11987654321",
  "nome": "João Silva"
}
```
- Response 200: Mesmo formato do cadastro
- Response 401: Telefone ou nome incorretos
- Response 404: Código de barbearia não encontrado

#### Barbeiros

**GET /api/barbeiros**
- Descrição: Listar barbeiros ativos da barbearia
- Headers: `Authorization: Bearer {token}`
- Response 200:
```json
[
  {
    "id": "uuid",
    "nome": "Carlos Barbeiro",
    "especialidades": ["Corte", "Barba"]
  }
]
```
- Response 401: Token inválido/expirado

**GET /api/barbeiros/{barbeiroId}/disponibilidade**
- Descrição: Consultar horários disponíveis de um barbeiro
- Headers: `Authorization: Bearer {token}`
- Query Params: `dataInicio=2025-10-11&dataFim=2025-10-18`
- Response 200:
```json
{
  "barbeiro": {
    "id": "uuid",
    "nome": "Carlos Barbeiro"
  },
  "diasDisponiveis": [
    {
      "data": "2025-10-11",
      "horariosDisponiveis": ["09:00", "09:30", "10:00", "14:00"]
    }
  ]
}
```
- Response 404: Barbeiro não encontrado
- Response 403: Barbeiro não pertence à barbearia do token

#### Serviços

**GET /api/servicos**
- Descrição: Listar serviços oferecidos pela barbearia
- Headers: `Authorization: Bearer {token}`
- Response 200:
```json
[
  {
    "id": "uuid",
    "nome": "Corte",
    "descricao": "Corte de cabelo",
    "duracaoMinutos": 30
  }
]
```

#### Agendamentos

**POST /api/agendamentos**
- Descrição: Criar novo agendamento
- Headers: `Authorization: Bearer {token}`
- Request Body:
```json
{
  "barbeiroId": "uuid",
  "servicoId": "uuid",
  "dataHora": "2025-10-15T10:00:00Z"
}
```
- Response 201:
```json
{
  "id": "uuid",
  "barbeiro": { "id": "uuid", "nome": "Carlos" },
  "servico": { "id": "uuid", "nome": "Corte", "duracaoMinutos": 30 },
  "dataHora": "2025-10-15T10:00:00Z",
  "duracaoMinutos": 30,
  "status": "Pendente"
}
```
- Response 422: Horário indisponível ou cliente já tem agendamento no dia
- Response 403: Barbeiro/serviço não pertencem à barbearia do token

**GET /api/agendamentos/meus**
- Descrição: Listar agendamentos do cliente autenticado
- Headers: `Authorization: Bearer {token}`
- Query Params: `status=proximos` ou `status=historico`
- Response 200:
```json
[
  {
    "id": "uuid",
    "barbeiro": { "nome": "Carlos" },
    "servico": { "nome": "Corte" },
    "dataHora": "2025-10-15T10:00:00Z",
    "status": "Pendente",
    "duracaoMinutos": 30
  }
]
```

**DELETE /api/agendamentos/{agendamentoId}**
- Descrição: Cancelar agendamento
- Headers: `Authorization: Bearer {token}`
- Response 204: Cancelado com sucesso
- Response 404: Agendamento não encontrado
- Response 403: Agendamento não pertence ao cliente autenticado
- Response 422: Agendamento já finalizado ou data passada

## Pontos de Integração

### Dependências Internas

1. **Sistema Multi-tenant Base**
   - Integração: Utiliza JWT service e tenant context do módulo base
   - Autenticação: JWT gerado com `barbeariaId` no payload
   - Validação: Middleware valida token e extrai contexto
   - Requisito: Sistema multi-tenant deve estar implementado primeiro

2. **Tabela Barbearias**
   - Operação: Leitura apenas
   - Uso: Validação de código durante cadastro/login
   - Requisito: Barbearias devem estar pré-cadastradas

3. **Tabela Barbeiros**
   - Operação: Leitura apenas
   - Uso: Listar barbeiros disponíveis e validar agendamentos
   - Requisito: Pelo menos 1 barbeiro ativo por barbearia para testes

4. **Tabela Serviços**
   - Operação: Leitura apenas
   - Uso: Listar serviços e obter duração para agendamento
   - Requisito: Serviços básicos (Corte, Barba) devem estar cadastrados

### Tratamento de Erros

Todas as integrações seguem o padrão:
- **Erro de validação**: Retornar 422 com mensagem descritiva
- **Recurso não encontrado**: Retornar 404
- **Erro de autorização**: Retornar 403
- **Erro interno**: Retornar 500 com logging detalhado
- **Retry**: Não aplicável (todas são operações síncronas no mesmo banco)

## Análise de Impacto

| Componente Afetado | Tipo de Impacto | Descrição & Nível de Risco | Ação Requerida |
|-------------------|-----------------|----------------------------|----------------|
| Sistema Multi-tenant Base | Dependência Direta | Requer JWT service e tenant middleware implementados. Risco Médio se não estiver pronto. | Coordenar implementação sequencial. Multi-tenant deve ser desenvolvido primeiro. |
| Tabela `barbearias` | Leitura Apenas | Consulta para validar código. Risco Baixo. | Garantir migrations e seeds existem. |
| Tabela `barbeiros` | Leitura Apenas | Consulta para listar e validar. Risco Médio se vazia (cliente não pode agendar). | Seeds obrigatórios: 1+ barbeiro por barbearia. |
| Tabela `servicos` | Leitura Apenas | Consulta para listar e obter duração. Risco Médio se vazia. | Seeds obrigatórios: Corte, Barba, Corte+Barba. |
| EF Core DbContext | Mudança de Configuração | Adiciona Global Query Filters para `clientes` e `agendamentos`. Risco Baixo. | Documentar filtros globais aplicados. |
| Banco de Dados PostgreSQL | Mudança de Esquema | Adiciona tabelas `clientes` e `agendamentos` com índices. Risco Baixo. | Migrations reversíveis obrigatórias. |
| Frontend (futuro) | Nova Interface | Novas páginas e componentes React. Risco Baixo (isolado). | Seguir padrões React definidos em rules/react.md. |
| Cache In-Memory | Novo Recurso | Adiciona cache de disponibilidade. Risco Baixo. | Usar IMemoryCache do .NET, TTL 5min. |

**Recursos Compartilhados:**
- **Connection Pool PostgreSQL**: Agendamentos podem gerar carga alta. Monitorar pool usage.
- **JWT Secret Key**: Compartilhada entre todos os módulos. Gerenciar via variável de ambiente.
- **DbContext Instance**: Shared, usar scoped lifetime no DI.

**Impacto de Performance:**
- **Endpoint /api/barbeiros/{id}/disponibilidade**: Alto uso esperado. Mitigado com cache 5min.
- **POST /api/agendamentos**: Transaction com lock otimista. Contenção baixa esperada.
- **Índices obrigatórios**: `(barbeiro_id, data_hora)`, `(telefone, barbearia_id)`.

## Abordagem de Testes

### Testes Unitários

#### Domain Layer

**Classe: Cliente**
- ✅ `CriarCliente_ComDadosValidos_DeveSerCriadoComSucesso`
- ✅ `CriarCliente_ComNomeVazio_DeveLancarExcecao`
- ✅ `CriarCliente_ComTelefoneInvalido_DeveLancarExcecao` (9 dígitos, 12 dígitos)
- ✅ `ValidarNomeLogin_ComNomeCorreto_DeveRetornarTrue` (case-insensitive)
- ✅ `ValidarNomeLogin_ComNomeIncorreto_DeveRetornarFalse`

**Classe: Agendamento**
- ✅ `CriarAgendamento_ComDadosValidos_DeveSerCriadoComStatusPendente`
- ✅ `CriarAgendamento_ComDataPassada_DeveLancarExcecao`
- ✅ `Confirmar_AgendamentoPendente_DeveAtualizarStatus`
- ✅ `Confirmar_AgendamentoJaConfirmado_DeveLancarExcecao`
- ✅ `Cancelar_AgendamentoPendente_DeveAtualizarStatusEData`
- ✅ `Cancelar_AgendamentoConcluido_DeveLancarExcecao`
- ✅ `Cancelar_AgendamentoPassado_DeveLancarExcecao`

#### Application Layer

**Mocks Necessários**: `IClienteRepository`, `IBarbeariaRepository`, `IAgendamentoRepository`, `IJwtService`, `IUnitOfWork`, `IDisponibilidadeCache`

**Use Case: CadastrarClienteUseCase**
- ✅ `Handle_ComDadosValidos_DeveCriarClienteERetornarToken`
- ✅ `Handle_ComCodigoInvalido_DeveLancarBarbeariaNotFoundException`
- ✅ `Handle_ComTelefoneDuplicado_DeveLancarClienteJaExisteException`
- ✅ `Handle_DeveChamarUnitOfWorkCommit`

**Use Case: LoginClienteUseCase**
- ✅ `Handle_ComCredenciaisValidas_DeveRetornarToken`
- ✅ `Handle_ComTelefoneInexistente_DeveLancarUnauthorizedException`
- ✅ `Handle_ComNomeIncorreto_DeveLancarUnauthorizedException`

**Use Case: ConsultarDisponibilidadeUseCase**
- ✅ `Handle_SemAgendamentos_DeveRetornarTodosHorariosDisponiveis`
- ✅ `Handle_ComAgendamentos_DeveRemoverHorariosOcupados`
- ✅ `Handle_AgendamentoDe60Min_DeveBloqueaDoisSlots` (10:00 + 10:30)
- ✅ `Handle_ComCache_DeveRetornarDoCacheSemConsultarBanco`
- ✅ `Handle_SemCache_DeveConsultarBancoESalvarCache`

**Use Case: CriarAgendamentoUseCase**
- ✅ `Handle_ComHorarioDisponivel_DeveCriarAgendamento`
- ✅ `Handle_ComHorarioOcupado_DeveLancarHorarioIndisponivelException`
- ✅ `Handle_ClienteJaTemAgendamentoNoDia_DeveLancarException`
- ✅ `Handle_BarbeiroDeOutraBarbearia_DeveLancarForbiddenException`
- ✅ `Handle_DeveChamarUnitOfWorkCommit`

**Use Case: CancelarAgendamentoUseCase**
- ✅ `Handle_ComAgendamentoPendente_DeveCancelar`
- ✅ `Handle_AgendamentoDeOutroCliente_DeveLancarForbiddenException`
- ✅ `Handle_AgendamentoConcluido_DeveLancarException`

### Testes de Integração

**Setup**: Usar `WebApplicationFactory` + Testcontainers para PostgreSQL real.

**Autenticação:**
- ✅ `POST /api/auth/cliente/cadastro - Cadastro bem-sucedido retorna 201 + token válido`
- ✅ `POST /api/auth/cliente/cadastro - Telefone duplicado retorna 422`
- ✅ `POST /api/auth/cliente/cadastro - Código inválido retorna 404`
- ✅ `POST /api/auth/cliente/login - Login bem-sucedido retorna 200 + token`
- ✅ `POST /api/auth/cliente/login - Nome incorreto retorna 401`

**Isolamento Multi-tenant:**
- ✅ `GET /api/barbeiros - Retorna apenas barbeiros da barbearia do token`
- ✅ `GET /api/barbeiros - Token de barbearia A não vê barbeiros de barbearia B`
- ✅ `POST /api/agendamentos - Cliente de barbearia A não pode agendar barbeiro de barbearia B (403)`
- ✅ `GET /api/agendamentos/meus - Retorna apenas agendamentos da barbearia do token`

**Agendamentos:**
- ✅ `POST /api/agendamentos - Criação bem-sucedida retorna 201`
- ✅ `POST /api/agendamentos - Sem token retorna 401`
- ✅ `POST /api/agendamentos - Horário ocupado retorna 422`
- ✅ `POST /api/agendamentos - Dois clientes simultâneos tentam mesmo horário, apenas um sucede`
- ✅ `DELETE /api/agendamentos/{id} - Cancelamento bem-sucedido retorna 204`
- ✅ `DELETE /api/agendamentos/{id} - Cancelar agendamento de outro cliente retorna 403`

**Cenários de Carga:**
- ✅ `Disponibilidade - 10 requisições simultâneas para mesmo barbeiro usam cache`
- ✅ `Agendamentos - 5 clientes tentando agendar horários diferentes no mesmo barbeiro, todos sucedem`

## Sequenciamento de Desenvolvimento

### Ordem de Construção

**Fase 1: Foundation (Backend) - 3-4 dias**
1. **Entidades de Domínio** (1 dia)
   - Criar classes `Cliente` e `Agendamento` com validações
   - Criar `StatusAgendamento` enum
   - Testes unitários das entidades
   - **Por que primeiro**: Base para toda a lógica de negócio

2. **Migrations e Repositórios** (1 dia)
   - Criar migrations para tabelas `clientes` e `agendamentos`
   - Implementar `ClienteRepository` e `AgendamentoRepository`
   - Configurar EF Core com Global Query Filters
   - **Dependência**: Tabelas de barbearias, barbeiros e serviços devem existir

3. **Unit of Work** (0.5 dia)
   - Implementar `UnitOfWork` seguindo padrão do projeto
   - **Dependência**: DbContext configurado

4. **Seeds de Dados** (0.5 dia)
   - Criar seeds para barbearias, barbeiros e serviços de teste
   - **Por que necessário**: Testes e desenvolvimento frontend dependem de dados

**Fase 2: Autenticação (Backend) - 2 dias**
5. **JWT Service** (0.5 dia)
   - Implementar geração e validação de tokens com `barbeariaId`
   - Configurar secret key e expiração
   - **Dependência**: Sistema multi-tenant base (se não existir, implementar aqui)

6. **Use Cases de Autenticação** (1 dia)
   - `CadastrarClienteUseCase` com testes
   - `LoginClienteUseCase` com testes
   - **Dependência**: Repositórios e JWT Service

7. **Endpoints de Autenticação** (0.5 dia)
   - `AuthClienteController` com POST cadastro e login
   - Middleware JWT e extração de contexto
   - Testes de integração de autenticação

**Fase 3: Consulta (Backend) - 2-3 dias**
8. **Use Cases de Consulta** (1 dia)
   - `ListarBarbeirosUseCase`
   - `ListarServicosUseCase`
   - Testes unitários
   - **Dependência**: Seeds de dados

9. **Algoritmo de Disponibilidade** (1.5 dias)
   - Implementar `ConsultarDisponibilidadeUseCase`
   - Lógica de cálculo de slots com bloqueio por duração
   - Cache in-memory (IMemoryCache)
   - Testes unitários extensivos (cenários complexos)
   - **Por que demorado**: Lógica crítica e complexa

10. **Endpoints de Consulta** (0.5 dia)
    - `BarbeirosController` e `ServicosController`
    - Testes de integração

**Fase 4: Agendamento (Backend) - 3 dias**
11. **Use Case de Criação** (1.5 dias)
    - `CriarAgendamentoUseCase` com lock otimista
    - Validações de conflito e isolamento
    - Testes unitários de concorrência
    - **Por que demorado**: Lógica de lock e validações complexas

12. **Use Cases de Gestão** (1 dia)
    - `ListarAgendamentosUseCase` com filtros (próximos/histórico)
    - `CancelarAgendamentoUseCase`
    - Testes unitários

13. **Endpoints de Agendamento** (0.5 dia)
    - `AgendamentosController` completo
    - Testes de integração incluindo isolamento multi-tenant

**Fase 5: Frontend - 4-5 dias**
14. **Setup e Estrutura** (0.5 dia)
    - Configurar React + Vite + TypeScript
    - Instalar React Query, Tailwind, Shadcn UI
    - Configurar Axios com interceptors

15. **Autenticação Frontend** (1 dia)
    - Implementar `AuthContext` e `useAuth` hook
    - Páginas de Cadastro e Login
    - Validação de código da barbearia
    - Armazenamento de token e estado
    - **Dependência**: Endpoints de autenticação backend

16. **Fluxo de Agendamento** (2 dias)
    - Página de Seleção de Barbeiro (lista)
    - Página de Seleção de Serviço
    - Página de Calendário com disponibilidade
    - Página de Confirmação
    - State management do fluxo (4 steps)
    - **Dependência**: Todos os endpoints backend

17. **Gestão de Agendamentos** (1 dia)
    - Página "Meus Agendamentos" com abas (Próximos/Histórico)
    - Modal de cancelamento
    - React Query mutations

18. **Componentes Compartilhados** (0.5 dia)
    - Header com nome da barbearia
    - Loading states e error handling
    - Validações de formulário

**Fase 6: Testes e Refinamento - 2 dias**
19. **Testes Frontend** (1 dia)
    - Testes de componentes principais
    - Testes de hooks customizados
    - **Dependência**: Componentes implementados

20. **Testes E2E** (0.5 dia)
    - Fluxo completo: cadastro → agendamento → cancelamento
    - Teste de isolamento multi-tenant

21. **Performance e Refinamento** (0.5 dia)
    - Testes de carga no endpoint de disponibilidade
    - Validação de cache
    - Ajustes de UX mobile

**Total Estimado: 16-19 dias de desenvolvimento**

### Dependências Técnicas

**Bloqueantes (Devem existir antes de começar):**
- ✅ Banco de dados PostgreSQL configurado e acessível
- ✅ Tabelas de barbearias, barbeiros e serviços criadas
- ✅ Sistema multi-tenant base (JWT service e middleware) ou implementar na Fase 2

**Necessárias durante desenvolvimento:**
- ✅ Seeds de dados (barbearias, barbeiros, serviços) - Fase 1
- ✅ Migrations reversíveis para todas as mudanças de schema
- ✅ Variável de ambiente para JWT secret key

**Desejáveis:**
- Ambiente de desenvolvimento local com Docker Compose
- CI/CD configurado para rodar testes automaticamente

## Monitoramento e Observabilidade

### Métricas (Formato Prometheus)

```csharp
// Expor via /metrics endpoint
barbapp_agendamentos_criados_total{barbearia_id, status} - Counter
barbapp_agendamentos_cancelados_total{barbearia_id} - Counter
barbapp_disponibilidade_consultas_total{barbearia_id, cached} - Counter
barbapp_disponibilidade_cache_hit_rate{barbearia_id} - Gauge
barbapp_disponibilidade_tempo_calculo_ms{barbearia_id} - Histogram
barbapp_agendamento_conflitos_total{barbearia_id} - Counter
barbapp_clientes_cadastrados_total{barbearia_id} - Counter
barbapp_logins_clientes_total{barbearia_id, sucesso} - Counter
```

### Logs Principais

**Nível INFO:**
- Cadastro de cliente: `"Cliente {clienteId} cadastrado na barbearia {barbeariaId}"`
- Login: `"Cliente {clienteId} fez login na barbearia {barbeariaId}"`
- Agendamento criado: `"Agendamento {agendamentoId} criado para cliente {clienteId} com barbeiro {barbeiroId} em {dataHora}"`
- Agendamento cancelado: `"Agendamento {agendamentoId} cancelado por cliente {clienteId}"`

**Nível WARNING:**
- Conflito de horário: `"Conflito de horário detectado: barbeiro {barbeiroId} em {dataHora}"`
- Cache miss consecutivos: `"Cache miss para disponibilidade de {barbeiroId} por 5 requisições seguidas"`
- Telefone duplicado: `"Tentativa de cadastro com telefone duplicado na barbearia {barbeariaId}"`

**Nível ERROR:**
- Falha no Unit of Work: `"Erro ao persistir agendamento: {exception}"`
- Token inválido: `"Token JWT inválido ou expirado no endpoint {endpoint}"`
- Vazamento de dados: `"CRÍTICO: Tentativa de acesso cross-tenant detectada: token barbearia {A} acessando recurso barbearia {B}"`

**Estrutura dos Logs (JSON):**
```json
{
  "timestamp": "2025-10-11T10:00:00Z",
  "level": "INFO",
  "message": "Agendamento criado",
  "context": {
    "barbeariaId": "uuid",
    "clienteId": "uuid",
    "agendamentoId": "uuid",
    "endpoint": "/api/agendamentos"
  }
}
```

### Dashboards Grafana

**Dashboard: Agendamentos por Barbearia**
- Gráfico de linha: Agendamentos criados por dia
- Gauge: Taxa de cancelamento (%)
- Table: Top 5 barbeiros com mais agendamentos

**Dashboard: Performance Disponibilidade**
- Histogram: Tempo de cálculo de disponibilidade (p50, p95, p99)
- Gauge: Cache hit rate (%)
- Counter: Total de consultas (cached vs uncached)

**Dashboard: Isolamento Multi-tenant**
- Alert: Detecção de tentativas de acesso cross-tenant (deve ser 0)
- Counter: Requisições por barbearia
- Table: Contagem de recursos por barbearia (clientes, agendamentos)

**Alertas:**
- 🚨 `barbapp_agendamento_conflitos_total > 10 em 5min` → Possível problema no lock otimista
- 🚨 `barbapp_disponibilidade_cache_hit_rate < 0.5` → Cache não está funcionando
- 🚨 `barbapp_disponibilidade_tempo_calculo_ms p95 > 1000` → Performance degradada
- 🔴 `Tentativa de acesso cross-tenant detectada` → Falha crítica de isolamento

## Considerações Técnicas

### Decisões Principais

#### 1. Algoritmo de Disponibilidade: Cálculo em Tempo Real vs Pré-computado

**Escolha: Cálculo em tempo real com cache de 5 minutos**

**Justificativa:**
- Mais simples de implementar e manter para MVP
- Evita complexidade de sincronização entre pré-computação e agendamentos reais
- Cache de 5min reduz carga sem sacrificar precisão
- Horários disponíveis sempre refletem estado atual com delay máximo de 5min

**Trade-offs:**
- ✅ Vantagens: Simplicidade, sempre atualizado (com delay aceitável), sem estado persistente extra
- ⚠️ Desvantagens: Leve impacto de performance (mitigado com cache e índices), requer cálculo a cada cache miss

**Alternativa Rejeitada: Pré-calcular e armazenar slots**
- Por que rejeitada: Complexidade de manter sincronização entre tabela de slots e agendamentos reais, risco de inconsistência, mais armazenamento necessário

#### 2. Prevenção de Conflitos: Lock Otimista vs Lock Pessimista

**Escolha: Lock otimista com validação dupla (check-before-insert)**

**Justificativa:**
- Conflitos são raros na prática (probabilidade de 2 clientes agendarem exato mesmo horário simultaneamente é baixa)
- Lock pessimista (`SELECT FOR UPDATE`) degrada performance e aumenta contenção
- Validação dupla: check disponibilidade → inserir → verificar novamente em transaction

**Trade-offs:**
- ✅ Vantagens: Melhor performance, sem contenção de locks, throughput maior
- ⚠️ Desvantagens: Cliente pode ver horário disponível que acabou de ser ocupado (mas recebe feedback imediato com 422 e lista de horários ainda disponíveis)

**Implementação:**
```csharp
// Dentro de transaction
var conflito = await _repository.ExistsConflictAsync(barbeiroId, dataHora, duracao);
if (conflito)
    throw new HorarioIndisponivelException();
await _repository.InsertAsync(agendamento);
await _unitOfWork.Commit(); // Se outro agendamento foi criado entre check e insert, DB constraint falhará
```

**Alternativa Rejeitada: Lock Pessimista**
- Por que rejeitada: Overhead de performance desnecessário para MVP com baixa contenção esperada

#### 3. Autenticação Cliente: Telefone + Nome (sem senha)

**Escolha: Login simples com telefone + nome, sem senha**

**Justificativa:**
- Alinhado com requisito do PRD para reduzir atrito no MVP
- Comum em aplicativos de agendamento locais (barbearias confiam em validação simples)
- Validação de nome (case-insensitive match) adiciona camada mínima de segurança
- SMS validation planejada para Fase 2

**Trade-offs:**
- ✅ Vantagens: UX simplificada, onboarding rápido, sem esquecimento de senha
- ⚠️ Desvantagens: Segurança reduzida (qualquer pessoa com telefone+nome pode acessar), dependente de validação por SMS futura

**Mitigações de Segurança:**
- Validação de nome obrigatória (não apenas telefone)
- Rate limiting para prevenir brute force de nomes
- Logs de auditoria de todos os logins
- Planejamento de SMS validation para Fase 2

**Alternativa Rejeitada: Senha ou SMS desde o início**
- Por que rejeitada: Aumenta atrito para MVP, SMS requer integração externa (custo/complexidade), PRD explicitamente exclui do MVP

#### 4. Storage de Token Frontend: localStorage vs Cookie HttpOnly

**Escolha: Cookie HttpOnly**

**Justificativa:**
- Alinhado com a especificação técnica principal do sistema multi-tenant (prd-sistema-multi-tenant/techspec.md).
- **Segurança**: Cookies HttpOnly não são acessíveis via JavaScript, o que mitiga significativamente o risco de ataques XSS (Cross-Site Scripting).
- **Automático**: O browser envia o cookie automaticamente em cada requisição para o mesmo domínio.
- **Padrão de Indústria**: É a abordagem recomendada para armazenar tokens de autenticação na web.

**Trade-offs:**
- ✅ Vantagens: Segurança aprimorada contra XSS, gerenciamento automático pelo browser.
- ⚠️ Desvantagens: Requer configuração de CORS (Cross-Origin Resource Sharing) mais cuidadosa se o frontend e o backend estiverem em domínios diferentes.

**Implementação:**
- O backend deve configurar o header `Set-Cookie` com as flags `HttpOnly`, `Secure` (para produção), e `SameSite=Strict`.
- O frontend não precisa de lógica para anexar o token; o browser faz isso automaticamente.

**Alternativa Rejeitada: localStorage**
- Por que rejeitada: Vulnerável a ataques XSS, o que representa um risco de segurança inaceitável para tokens de autenticação.

#### 5. Multi-tenant Strategy: Shared Database com Discriminador

**Escolha: Shared database com `barbeariaId` como discriminador**

**Justificativa:**
- Alinhado com PRD de sistema multi-tenant
- Operacionalmente mais simples (uma única instância de BD)
- Custo reduzido vs database por tenant
- EF Core Global Query Filters garantem isolamento automático

**Implementação:**
```csharp
// DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Cliente>()
        .HasQueryFilter(c => c.BarbeariaId == _currentBarbeariaId);

    modelBuilder.Entity<Agendamento>()
        .HasQueryFilter(a => a.BarbeariaId == _currentBarbeariaId);
}
```

**Trade-offs:**
- ✅ Vantagens: Operacionalmente simples, custo reduzido, migrations unificadas
- ⚠️ Desvantagens: Risco de vazamento de dados se filtro falhar (mitigado por testes extensivos e validação dupla)

**Validação Dupla:**
1. Global Query Filter aplica `WHERE barbearia_id = {contextoBarbeariaId}` automaticamente
2. Controllers validam que recurso solicitado pertence à barbearia do token

**Alternativas Rejeitadas:**
- Database por tenant: Complexidade operacional alta, custo maior
- Schema por tenant: Migração complexa, não escala bem

### Riscos Conhecidos

**Risco 1: Vazamento de Dados Multi-tenant (Severidade: CRÍTICA)**
- **Descrição**: Global Query Filter pode ser bypassado acidentalmente
- **Probabilidade**: Baixa (com testes adequados)
- **Impacto**: Alto (violação de privacidade, perda de confiança)
- **Mitigação:**
  - Testes de integração específicos para isolamento (cliente A não vê dados de barbearia B)
  - Code review obrigatório em todos os repositórios
  - Validação dupla: filtro global + validação em controller
  - Logging de todas as queries com `barbeariaId`
  - Alertas para detecção de tentativas de acesso cross-tenant

**Risco 2: Conflitos de Agendamento em Alta Concorrência**
- **Descrição**: Lock otimista pode falhar sob alta carga simultânea
- **Probabilidade**: Baixa (conflitos raros)
- **Impacto**: Médio (cliente recebe erro 422 e tenta novamente)
- **Mitigação:**
  - Testes de carga para validar comportamento sob concorrência
  - Mensagem clara de erro com horários alternativos
  - Retry automático no frontend (1 tentativa)
  - Monitoramento de taxa de conflitos (alerta se > 5%)

**Risco 3: Performance de Consulta de Disponibilidade**
- **Descrição**: Cálculo de slots pode ser lento com muitos agendamentos
- **Probabilidade**: Média (depende do volume)
- **Impacto**: Médio (UX degradada)
- **Mitigação:**
  - Cache de 5 minutos para reduzir carga
  - Índices compostos: `(barbeiro_id, data_hora, status)`
  - Limitar consulta a 30 dias (requisito do PRD)
  - Query otimizada: buscar apenas agendamentos no range de datas
  - Monitoramento de tempo de resposta (alerta se p95 > 1s)

**Risco 4: Autenticação Fraca (Telefone + Nome)**
- **Descrição**: Segurança reduzida pode permitir acesso não autorizado
- **Probabilidade**: Média (especialmente se nomes comuns)
- **Impacto**: Médio (acesso indevido a dados de outro cliente)
- **Mitigação:**
  - Rate limiting: 5 tentativas por 15 minutos por IP
  - Logging detalhado de tentativas de login falhadas
  - Alertas para múltiplas tentativas falhadas
  - Implementação de SMS validation na Fase 2 (prioridade)
  - Educação de usuários: não compartilhar telefone/nome

**Risco 5: Escalabilidade do Cache In-Memory**
- **Descrição**: Cache em memória não escala horizontalmente (múltiplas instâncias)
- **Probabilidade**: Alta (se escalar horizontalmente)
- **Impacto**: Baixo (cache miss mais frequente, mas funcional)
- **Mitigação:**
  - Para MVP, instância única aceitável
  - Fase 2: migrar para Redis distribuído se necessário
  - Cache TTL curto (5min) limita inconsistência
  - Monitorar cache hit rate (alerta se < 50%)

### Requisitos Especiais

#### Performance

**Requisitos Obrigatórios:**
- ✅ Autenticação (cadastro/login): < 1 segundo (p95)
- ✅ Listagem de barbeiros: < 500ms (p95)
- ✅ Consulta de disponibilidade: < 2 segundos (p95) primeiro acesso, < 200ms com cache
- ✅ Criação de agendamento: < 1.5 segundos (p95)
- ✅ Listagem de agendamentos do cliente: < 1 segundo (p95)
- ✅ Cancelamento: < 800ms (p95)

**Otimizações Implementadas:**
- Índices compostos em queries frequentes
- Cache in-memory para disponibilidade (5min TTL)
- Queries paginadas para listas longas (limit/offset)
- Conexão pool do PostgreSQL otimizado
- Select apenas colunas necessárias (não SELECT *)

**Testes de Carga Necessários:**
- 10 clientes simultâneos consultando disponibilidade do mesmo barbeiro
- 5 clientes simultâneos tentando criar agendamentos
- 100 requisições de listagem de barbeiros em 10 segundos

#### Segurança

**Além de Autenticação Padrão:**
- ✅ Rate Limiting por IP: 100 req/min para endpoints públicos, 500 req/min autenticados
- ✅ Rate Limiting de Login: 5 tentativas por 15min por IP
- ✅ Validação de Input: sanitização de telefone, nome (prevenir SQL injection via ORM)
- ✅ HTTPS Obrigatório: TLS 1.2+ em produção
- ✅ CORS Restritivo: aceitar apenas domínios conhecidos
- ✅ JWT Secret Key: mínimo 256 bits, armazenada em variável de ambiente
- ✅ Token Expiration: 24h com validação rigorosa
- ✅ Logging de Segurança: todas as tentativas de acesso não autorizado
- ✅ Headers de Segurança: X-Content-Type-Options, X-Frame-Options, CSP

**LGPD/Privacidade:**
- Telefone é dado sensível: criptografia em trânsito (HTTPS), controle de acesso
- Direito ao esquecimento: endpoint futuro para exclusão de dados do cliente
- Logs não devem conter telefones completos (mascarar: 11987****21)

#### Observabilidade Adicional

**Tracing Distribuído (Fase 2):**
- OpenTelemetry para rastreamento de requisições end-to-end
- Correlation ID em todas as requisições
- Propagação de contexto entre serviços (se microsserviços futuros)

**Health Checks:**
- `/health` endpoint: verifica conexão com PostgreSQL, cache disponível
- `/health/ready` endpoint: sistema pronto para receber tráfego

**Debugging:**
- Logs estruturados (JSON) com contexto completo
- Request ID em todas as respostas para rastreamento
- Ferramentas: Serilog para logging estruturado

### Conformidade com Padrões

Este módulo segue rigorosamente os padrões do projeto definidos em `/rules`:

**code-standard.md:**
- ✅ camelCase para métodos/funções/variáveis, PascalCase para classes
- ✅ kebab-case para nomes de arquivos e diretórios
- ✅ Métodos começam com verbos (CadastrarCliente, ConsultarDisponibilidade)
- ✅ Máximo 3 parâmetros (uso de DTOs para mais)
- ✅ Early returns, máximo 2 níveis de if/else aninhados
- ✅ Métodos < 50 linhas, classes < 300 linhas
- ✅ Dependency Inversion em todos os use cases e repositories

**http.md:**
- ✅ REST padrão: recursos no plural, kebab-case nas URLs
- ✅ Códigos de status corretos: 200, 201, 204, 400, 401, 403, 404, 422, 500
- ✅ JSON para todas as requisições/respostas
- ✅ OpenAPI/Swagger para documentação automática
- ✅ Paginação com limit/offset para listas

**sql.md:**
- ✅ Nomes de tabelas/colunas em inglês, plural, snake_case
- ✅ Chaves primárias: `{tabela_singular}_id` (ex: `cliente_id`)
- ✅ Uppercase para palavras-chave SQL: SELECT, FROM, WHERE
- ✅ JOINs explícitos, nunca WHERE para joins
- ✅ Nunca SELECT *, sempre especificar colunas
- ✅ Tipos: TEXT para strings, INT/NUMERIC para números
- ✅ Índices em colunas de busca frequente
- ✅ Timestamps: `created_at`, `updated_at` em todas as tabelas
- ✅ Migrations reversíveis para todas as mudanças

**unit-of-work.md:**
- ✅ Interface `IUnityOfWork` com `Commit` e `Rollback`
- ✅ Implementação coordena transações e persistência
- ✅ Use cases chamam `await _unitOfWork.Commit()` após operações
- ✅ Suporte a eventos de domínio (preparado, não usado no MVP)

**tests.md:**
- ✅ xUnit para testes, Moq/NSubstitute para mocks
- ✅ Projetos separados: `BarbApp.UnitTests`, `BarbApp.IntegrationTests`
- ✅ Padrão AAA (Arrange, Act, Assert) em todos os testes
- ✅ Nomenclatura: `MetodoTestado_Cenario_ComportamentoEsperado`
- ✅ FluentAssertions para asserções legíveis
- ✅ Testes de domínio sem dependências externas
- ✅ Testes de use cases com mocks de repositories
- ✅ Testes de integração com WebApplicationFactory

**react.md:**
- ✅ Componentes funcionais, TypeScript, extensão .tsx
- ✅ Estado local próximo de onde é usado
- ✅ Propriedades explícitas, sem spread operator
- ✅ Componentes < 300 linhas
- ✅ Context API para comunicação entre componentes
- ✅ Tailwind para estilização (não styled-components)
- ✅ React Query para comunicação com API
- ✅ useMemo para otimizações
- ✅ Hooks com prefixo "use" (useAuth, useAgendamentos)
- ✅ Shadcn UI sempre que possível
- ✅ Testes para todos os componentes

---

**Data de Criação**: 2025-10-11
**Versão**: 1.0
**Status**: Rascunho para Revisão
**Autor**: Tech Spec Agent (Baseado em PRD v1.0)
