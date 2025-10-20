---
status: pending
parallelizable: false
blocked_by: ["1.0"]
---

<task_context>
<domain>backend/infrastructure</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>3.0, 5.0</unblocks>
</task_context>

# Tarefa 2.0: Infra/DB - Migrations, Config EF Core, Global Filters e Repositórios

## Visão Geral

Criar a camada de infraestrutura de persistência para as entidades Cliente e Agendamento, incluindo migrations, configuração do EF Core com Global Query Filters para isolamento multi-tenant, e implementação dos repositórios.

<requirements>
- Migrations para tabelas `clientes` e `agendamentos` no PostgreSQL
- Configuração EF Core com fluent API para mapeamento das entidades
- Global Query Filters para filtrar automaticamente por `barbeariaId`
- Índices para performance: (barbeiro_id, data_hora), (telefone, barbearia_id), (cliente_id, status)
- Constraint UNIQUE para (telefone, barbearia_id)
- Repositórios: IClienteRepository e IAgendamentoRepository
- Seguir padrão Unit of Work do projeto
- Seeds para dados de teste (barbearias, barbeiros, serviços)
- Atualizar o arquivo `bd_schema.md` com os novos objetos 
</requirements>

## Subtarefas

- [ ] 2.1 Criar migration para tabela `clientes` com colunas e índices
- [ ] 2.2 Criar migration para tabela `agendamentos` com colunas, índices e FKs
- [ ] 2.3 Configurar mapeamento EF Core para entidade `Cliente` (ClienteConfiguration)
- [ ] 2.4 Configurar mapeamento EF Core para entidade `Agendamento` (AgendamentoConfiguration)
- [ ] 2.5 Implementar Global Query Filter para `clientes` (filtro por barbeariaId)
- [ ] 2.6 Implementar Global Query Filter para `agendamentos` (filtro por barbeariaId)
- [ ] 2.7 Criar interface `IClienteRepository` com métodos: GetByTelefoneAsync, GetByIdAsync, AddAsync
- [ ] 2.8 Implementar `ClienteRepository` com filtros automáticos
- [ ] 2.9 Criar interface `IAgendamentoRepository` com métodos: GetByBarbeiroAndDateRangeAsync, GetByClienteAsync, GetByIdAsync, AddAsync, UpdateAsync
- [ ] 2.10 Implementar `AgendamentoRepository` com filtros automáticos e queries otimizadas
- [ ] 2.11 Adicionar repositórios ao DbContext e configurar DI
- [ ] 2.12 Criar seeds para dados de teste (barbearias, barbeiros, serviços básicos)
- [ ] 2.13 Testar migrations (apply e rollback)
- [ ] 2.14 Criar testes de integração para repositórios com Testcontainers

## Sequenciamento

- **Bloqueado por**: 1.0 (Entidades de Domínio)
- **Desbloqueia**: 3.0 (Use Cases de Autenticação), 5.0 (Consultas)
- **Paralelizável**: Não (depende de entidades estarem prontas)

## Detalhes de Implementação

### Migration - Tabela Clientes

```sql
CREATE TABLE clientes (
    cliente_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL,
    nome VARCHAR(200) NOT NULL,
    telefone VARCHAR(11) NOT NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_clientes_barbearia FOREIGN KEY (barbearia_id) REFERENCES barbearias(barbearia_id),
    CONSTRAINT uk_clientes_telefone_barbearia UNIQUE (telefone, barbearia_id)
);

CREATE INDEX idx_clientes_barbearia ON clientes(barbearia_id);
CREATE INDEX idx_clientes_telefone_barbearia ON clientes(telefone, barbearia_id);
```

### Migration - Tabela Agendamentos

```sql
CREATE TABLE agendamentos (
    agendamento_id UUID PRIMARY KEY,
    barbearia_id UUID NOT NULL,
    cliente_id UUID NOT NULL,
    barbeiro_id UUID NOT NULL,
    servicos_ids UUID[] NOT NULL,
    data_hora TIMESTAMP NOT NULL,
    duracao_total INT NOT NULL,
    status INT NOT NULL,
    data_cancelamento TIMESTAMP NULL,
    data_criacao TIMESTAMP NOT NULL DEFAULT NOW(),
    data_atualizacao TIMESTAMP NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_agendamentos_barbearia FOREIGN KEY (barbearia_id) REFERENCES barbearias(barbearia_id),
    CONSTRAINT fk_agendamentos_cliente FOREIGN KEY (cliente_id) REFERENCES clientes(cliente_id),
    CONSTRAINT fk_agendamentos_barbeiro FOREIGN KEY (barbeiro_id) REFERENCES barbeiros(barbeiro_id)
);

CREATE INDEX idx_agendamentos_barbearia ON agendamentos(barbearia_id);
CREATE INDEX idx_agendamentos_barbeiro_data ON agendamentos(barbeiro_id, data_hora);
CREATE INDEX idx_agendamentos_cliente_status ON agendamentos(cliente_id, status);
CREATE INDEX idx_agendamentos_data_hora ON agendamentos(data_hora);
```

### Configuração EF Core - Cliente

```csharp
public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");
        
        builder.HasKey(c => c.ClienteId);
        builder.Property(c => c.ClienteId).HasColumnName("cliente_id");
        builder.Property(c => c.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(c => c.Nome).HasColumnName("nome").HasMaxLength(200).IsRequired();
        
        // Value Object Telefone
        builder.OwnsOne(c => c.Telefone, telefone =>
        {
            telefone.Property(t => t.Numero).HasColumnName("telefone").HasMaxLength(11).IsRequired();
        });
        
        builder.Property(c => c.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(c => c.DataAtualizacao).HasColumnName("data_atualizacao").IsRequired();
        
        // Global Query Filter para multi-tenancy
        builder.HasQueryFilter(c => c.BarbeariaId == BarbeariaContext.BarbeariaId);
        
        // Índices e constraints
        builder.HasIndex(c => c.BarbeariaId).HasDatabaseName("idx_clientes_barbearia");
        builder.HasIndex(c => new { c.Telefone.Numero, c.BarbeariaId })
            .HasDatabaseName("idx_clientes_telefone_barbearia")
            .IsUnique();
    }
}
```

### Configuração EF Core - Agendamento

```csharp
public class AgendamentoConfiguration : IEntityTypeConfiguration<Agendamento>
{
    public void Configure(EntityTypeBuilder<Agendamento> builder)
    {
        builder.ToTable("agendamentos");
        
        builder.HasKey(a => a.AgendamentoId);
        builder.Property(a => a.AgendamentoId).HasColumnName("agendamento_id");
        builder.Property(a => a.BarbeariaId).HasColumnName("barbearia_id").IsRequired();
        builder.Property(a => a.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(a => a.BarbeiroId).HasColumnName("barbeiro_id").IsRequired();
        
        // Array de GUIDs no PostgreSQL
        builder.Property(a => a.ServicosIds)
            .HasColumnName("servicos_ids")
            .HasColumnType("uuid[]")
            .IsRequired();
        
        builder.Property(a => a.DataHora).HasColumnName("data_hora").IsRequired();
        builder.Property(a => a.DuracaoTotal).HasColumnName("duracao_total").IsRequired();
        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();
        builder.Property(a => a.DataCancelamento).HasColumnName("data_cancelamento");
        builder.Property(a => a.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(a => a.DataAtualizacao).HasColumnName("data_atualizacao").IsRequired();
        
        // Relacionamentos
        builder.HasOne(a => a.Cliente)
            .WithMany()
            .HasForeignKey(a => a.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(a => a.Barbeiro)
            .WithMany()
            .HasForeignKey(a => a.BarbeiroId)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Global Query Filter para multi-tenancy
        builder.HasQueryFilter(a => a.BarbeariaId == BarbeariaContext.BarbeariaId);
        
        // Índices
        builder.HasIndex(a => a.BarbeariaId).HasDatabaseName("idx_agendamentos_barbearia");
        builder.HasIndex(a => new { a.BarbeiroId, a.DataHora })
            .HasDatabaseName("idx_agendamentos_barbeiro_data");
        builder.HasIndex(a => new { a.ClienteId, a.Status })
            .HasDatabaseName("idx_agendamentos_cliente_status");
        builder.HasIndex(a => a.DataHora).HasDatabaseName("idx_agendamentos_data_hora");
    }
}
```

### Interface IClienteRepository

```csharp
public interface IClienteRepository
{
    Task<Cliente?> GetByTelefoneAsync(Guid barbeariaId, string telefone, CancellationToken cancellationToken = default);
    Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default);
    Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default);
    Task<bool> ExisteAsync(Guid barbeariaId, string telefone, CancellationToken cancellationToken = default);
}
```

### Implementação ClienteRepository

```csharp
public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByTelefoneAsync(Guid barbeariaId, string telefone, CancellationToken cancellationToken = default)
    {
        // Global Query Filter já aplica filtro por barbeariaId
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.Telefone.Numero == telefone, cancellationToken);
    }

    public async Task<Cliente?> GetByIdAsync(Guid clienteId, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId, cancellationToken);
    }

    public async Task AddAsync(Cliente cliente, CancellationToken cancellationToken = default)
    {
        await _context.Clientes.AddAsync(cliente, cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid barbeariaId, string telefone, CancellationToken cancellationToken = default)
    {
        return await _context.Clientes
            .AnyAsync(c => c.Telefone.Numero == telefone, cancellationToken);
    }
}
```

### Interface IAgendamentoRepository

```csharp
public interface IAgendamentoRepository
{
    Task<List<Agendamento>> GetByBarbeiroAndDateRangeAsync(
        Guid barbeiroId, 
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default);
    
    Task<List<Agendamento>> GetByClienteAsync(
        Guid clienteId, 
        StatusAgendamento? status = null, 
        CancellationToken cancellationToken = default);
    
    Task<Agendamento?> GetByIdAsync(Guid agendamentoId, CancellationToken cancellationToken = default);
    
    Task AddAsync(Agendamento agendamento, CancellationToken cancellationToken = default);
    
    Task<bool> ExisteConflito(
        Guid barbeiroId, 
        DateTime dataHora, 
        int duracaoMinutos, 
        Guid? agendamentoIdParaIgnorar = null, 
        CancellationToken cancellationToken = default);
}
```

### Seeds de Dados

```csharp
public static class ClienteAgendamentoSeeds
{
    public static void SeedData(ModelBuilder modelBuilder)
    {
        // Barbearia de Teste
        var barbeariaId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        modelBuilder.Entity<Barbearia>().HasData(
            new { BarbeariaId = barbeariaId, Nome = "Barbearia Teste", Codigo = "TEST123", Ativa = true }
        );
        
        // Barbeiros de Teste
        var barbeiro1Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var barbeiro2Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
        modelBuilder.Entity<Barbeiro>().HasData(
            new { BarbeiroId = barbeiro1Id, BarbeariaId = barbeariaId, Nome = "João Silva", Ativo = true },
            new { BarbeiroId = barbeiro2Id, BarbeariaId = barbeariaId, Nome = "Maria Santos", Ativo = true }
        );
        
        // Serviços de Teste
        var servicoCorteId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var servicoBarbaId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        modelBuilder.Entity<Servico>().HasData(
            new { ServicoId = servicoCorteId, BarbeariaId = barbeariaId, Nome = "Corte", DuracaoMinutos = 30, Preco = 50.0m, Ativo = true },
            new { ServicoId = servicoBarbaId, BarbeariaId = barbeariaId, Nome = "Barba", DuracaoMinutos = 20, Preco = 30.0m, Ativo = true }
        );
    }
}
```

## Critérios de Sucesso

- ✅ Migrations aplicadas com sucesso no PostgreSQL
- ✅ Tabelas `clientes` e `agendamentos` criadas com todos os índices
- ✅ Global Query Filters funcionando (consultas filtradas automaticamente por barbeariaId)
- ✅ Constraint UNIQUE impedindo telefones duplicados na mesma barbearia
- ✅ Repositórios implementados e registrados no DI
- ✅ Seeds de dados executados (barbearia, barbeiros, serviços de teste)
- ✅ Testes de integração passando (Testcontainers)
- ✅ Migrations reversíveis (rollback funciona)
- ✅ Queries otimizadas usando índices corretos (verificar EXPLAIN ANALYZE)
