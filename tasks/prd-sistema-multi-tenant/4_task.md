---
status: completed
parallelizable: true
blocked_by: ["2.0"]
---

<task_context>
<domain>infrastructure/database</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>high</complexity>
<dependencies>database|entity_framework</dependencies>
<unblocks>"5.0"</unblocks>
</task_context>

# Tarefa 4.0: Configurar DbContext e Global Query Filters

## Visão Geral

Estender o `BarbAppDbContext` com as novas tabelas de usuários (AdminCentralUser, AdminBarbeariaUser, Barber, Customer) e implementar Global Query Filters para garantir isolamento multi-tenant automático. Esta é a peça crítica que garante que queries sejam filtradas por `barbeariaId` sem necessidade de filtros manuais.

<requirements>
- Adicionar DbSets para todas as entidades de usuários
- Configurar Entity Type Configurations (mapeamento tabela/colunas)
- Implementar Global Query Filters para isolamento multi-tenant
- Criar migration para criação das tabelas
- Configurar índices para performance (telefone, email, barbeariaId)
- Testar isolation via IgnoreQueryFilters() para Admin Central
</requirements>

## Subtarefas

- [x] 4.1 Adicionar DbSets no BarbAppDbContext
- [x] 4.2 Criar EntityTypeConfiguration para AdminCentralUser
- [x] 4.3 Criar EntityTypeConfiguration para AdminBarbeariaUser
- [x] 4.4 Criar EntityTypeConfiguration para Barber
- [x] 4.5 Criar EntityTypeConfiguration para Customer
- [x] 4.6 Implementar Global Query Filters no OnModelCreating
- [x] 4.7 Criar migration: `dotnet ef migrations add AddUserEntities`
- [x] 4.8 Testar migration em banco local: `dotnet ef database update`
- [x] 4.9 Validar estrutura de tabelas e índices no PostgreSQL

## Sequenciamento

- **Bloqueado por**: 2.0 (Domain Layer Base)
- **Desbloqueia**: 5.0 (Repositórios)
- **Paralelizável**: Sim (com tarefa 3.0 - entidades podem ser implementadas em paralelo)

## Detalhes de Implementação

### BarbAppDbContext

```csharp
// BarbApp.Infrastructure/Persistence/BarbAppDbContext.cs
public class BarbAppDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public BarbAppDbContext(
        DbContextOptions<BarbAppDbContext> options,
        ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Barbershop> Barbershops => Set<Barbershop>();
    public DbSet<AdminCentralUser> AdminCentralUsers => Set<AdminCentralUser>();
    public DbSet<AdminBarbeariaUser> AdminBarbeariaUsers => Set<AdminBarbeariaUser>();
    public DbSet<Barber> Barbers => Set<Barber>();
    public DbSet<Customer> Customers => Set<Customer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplicar configurações
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BarbAppDbContext).Assembly);

        // Global Query Filters para isolamento multi-tenant
        modelBuilder.Entity<AdminBarbeariaUser>().HasQueryFilter(a =>
            _tenantContext.IsAdminCentral || a.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<Barber>().HasQueryFilter(b =>
            _tenantContext.IsAdminCentral || b.BarbeariaId == _tenantContext.BarbeariaId);

        modelBuilder.Entity<Customer>().HasQueryFilter(c =>
            _tenantContext.IsAdminCentral || c.BarbeariaId == _tenantContext.BarbeariaId);
    }
}
```

### Entity Type Configurations

```csharp
// BarbApp.Infrastructure/Persistence/Configurations/AdminCentralUserConfiguration.cs
public class AdminCentralUserConfiguration : IEntityTypeConfiguration<AdminCentralUser>
{
    public void Configure(EntityTypeBuilder<AdminCentralUser> builder)
    {
        builder.ToTable("admin_central_users");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("admin_central_user_id")
            .ValueGeneratedNever();

        builder.Property(a => a.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Índices
        builder.HasIndex(a => a.Email)
            .IsUnique()
            .HasDatabaseName("idx_admin_central_users_email");
    }
}

// BarbApp.Infrastructure/Persistence/Configurations/AdminBarbeariaUserConfiguration.cs
public class AdminBarbeariaUserConfiguration : IEntityTypeConfiguration<AdminBarbeariaUser>
{
    public void Configure(EntityTypeBuilder<AdminBarbeariaUser> builder)
    {
        builder.ToTable("admin_barbearia_users");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasColumnName("admin_barbearia_user_id")
            .ValueGeneratedNever();

        builder.Property(a => a.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(a => a.Email)
            .HasColumnName("email")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(a => a.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(a => a.Barbearia)
            .WithMany()
            .HasForeignKey(a => a.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(a => new { a.BarbeariaId, a.Email })
            .IsUnique()
            .HasDatabaseName("idx_admin_barbearia_users_barbearia_email");

        builder.HasIndex(a => a.BarbeariaId)
            .HasDatabaseName("idx_admin_barbearia_users_barbearia_id");
    }
}

// BarbApp.Infrastructure/Persistence/Configurations/BarberConfiguration.cs
public class BarberConfiguration : IEntityTypeConfiguration<Barber>
{
    public void Configure(EntityTypeBuilder<Barber> builder)
    {
        builder.ToTable("barbers");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasColumnName("barber_id")
            .ValueGeneratedNever();

        builder.Property(b => b.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(b => b.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(b => b.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(b => b.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(b => b.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(b => b.Barbearia)
            .WithMany()
            .HasForeignKey(b => b.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(b => new { b.BarbeariaId, b.Telefone })
            .IsUnique()
            .HasDatabaseName("idx_barbers_barbearia_telefone");

        builder.HasIndex(b => b.Telefone)
            .HasDatabaseName("idx_barbers_telefone");
    }
}

// BarbApp.Infrastructure/Persistence/Configurations/CustomerConfiguration.cs
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasColumnName("customer_id")
            .ValueGeneratedNever();

        builder.Property(c => c.BarbeariaId)
            .HasColumnName("barbearia_id")
            .IsRequired();

        builder.Property(c => c.Telefone)
            .HasColumnName("telefone")
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        // Relacionamentos
        builder.HasOne(c => c.Barbearia)
            .WithMany()
            .HasForeignKey(c => c.BarbeariaId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices e constraints
        builder.HasIndex(c => new { c.BarbeariaId, c.Telefone })
            .IsUnique()
            .HasDatabaseName("idx_customers_barbearia_telefone");

        builder.HasIndex(c => c.Telefone)
            .HasDatabaseName("idx_customers_telefone");
    }
}
```

## Critérios de Sucesso

- ✅ Todas as tabelas criadas corretamente no PostgreSQL
- ✅ Global Query Filters implementados e funcionando
- ✅ Admin Central consegue acessar dados de todas as barbearias (IgnoreQueryFilters)
- ✅ Usuários normais só veem dados da própria barbearia
- ✅ Índices criados: telefone, email, barbearia_id
- ✅ Constraints UNIQUE funcionando: (barbearia_id, telefone), (barbearia_id, email)
- ✅ Foreign keys configuradas com CASCADE DELETE
- ✅ Migration executada sem erros: `dotnet ef database update`
- ✅ Validação manual no banco: `SELECT * FROM admin_central_users;`

## Tempo Estimado

**5 horas**

## Referências

- TechSpec: Seção "Pontos de Integração" - Global Query Filters
- TechSpec: Seção "Schema do Banco de Dados"
- PRD: Funcionalidade 2 - Isolamento de Dados Multi-tenant
- TechSpec: Decisão #6 - Global Query Filters vs Filtros Manuais

---

## 📋 Revisão e Aprovação

### Status da Revisão
- ✅ **APROVADA COM LOUVOR**
- 📅 Data: 2025-10-11
- 👤 Revisor: GitHub Copilot (IA)

### Checklist de Validação
- [x] ✅ Todas as 5 entidades mapeadas com DbSets no BarbAppDbContext
- [x] ✅ EntityTypeConfigurations criadas para todas as entidades
- [x] ✅ Global Query Filters implementados para isolamento multi-tenant
- [x] ✅ Migration AddUserEntities criada com estrutura correta
- [x] ✅ Índices configurados: telefone, email, barbearia_id
- [x] ✅ Constraints UNIQUE funcionando: (barbearia_id, telefone), (barbearia_id, email)
- [x] ✅ Foreign keys configuradas com CASCADE DELETE
- [x] ✅ Value converter para BarbeariaCode funcionando
- [x] ✅ Build executando sem erros: `dotnet build`
- [x] ✅ Todos os testes passando (74/74)
- [x] ✅ Alinhamento com PRD validado
- [x] ✅ Conformidade com TechSpec verificada
- [x] ✅ Regras de código analisadas (aplicáveis)

### Relatório Completo
Veja o relatório detalhado de revisão em: `4_task_review.md`

### Próximos Passos
✅ Pronto para iniciar **Tarefa 5.0** - Repositórios
