---
status: completed
parallelizable: true
blocked_by: ["4.0"]
completed_at: 2025-10-11
---

<task_context>
<domain>Camada de Dados</domain>
<type>Implementação</type>
<scope>Repositories</scope>
<complexity>média</complexity>
<dependencies>Entity Framework Core, Interfaces do domínio</dependencies>
<unblocks>"7.0"</unblocks>
</task_context>

# Tarefa 5.0: Implementar Repositórios de Usuários

## Visão Geral
Implementar os repositórios concretos para gerenciamento de usuários de todos os tipos (Admin Central, Admin Barbearia, Barbeiro, Cliente), fornecendo a camada de persistência para operações de autenticação e gerenciamento.

<requirements>
- IAdminCentralUserRepository com operações de busca por email e criação
- IAdminBarbeariaUserRepository com suporte a tenant e operações CRUD
- IBarberRepository com filtragem por barbearia e listagem
- ICustomerRepository com operações básicas de cliente
- Isolamento de dados por tenant onde aplicável
- Queries otimizadas com EF Core
</requirements>

## Subtarefas
- [x] 5.1 Implementar AdminCentralUserRepository ✅
- [x] 5.2 Implementar AdminBarbeariaUserRepository com filtros de tenant ✅
- [x] 5.3 Implementar BarberRepository com queries otimizadas ✅
- [x] 5.4 Implementar CustomerRepository ✅
- [x] 5.5 Adicionar índices e otimizações de performance ✅
- [x] 5.6 Criar testes unitários para cada repositório ✅

## Sequenciamento
- **Bloqueado por**: 4.0 (Domínio, Entidades e Interfaces)
- **Desbloqueia**: 7.0 (Use Cases de Autenticação)
- **Paralelizável**: Sim (pode ser desenvolvido em paralelo com 6.0)

## Detalhes de Implementação

### AdminCentralUserRepository
```csharp
public class AdminCentralUserRepository : IAdminCentralUserRepository
{
    private readonly AppDbContext _context;

    public AdminCentralUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminCentralUser?> GetByEmailAsync(string email)
    {
        return await _context.AdminCentralUsers
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<AdminCentralUser> AddAsync(AdminCentralUser user)
    {
        await _context.AdminCentralUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
```

### AdminBarbeariaUserRepository
```csharp
public class AdminBarbeariaUserRepository : IAdminBarbeariaUserRepository
{
    private readonly AppDbContext _context;

    public AdminBarbeariaUserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AdminBarbeariaUser?> GetByEmailAndBarbeariaIdAsync(
        string email, Guid barbeariaId)
    {
        return await _context.AdminBarbeariaUsers
            .Include(u => u.Barbearia)
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.BarbeariaId == barbeariaId);
    }

    public async Task<AdminBarbeariaUser> AddAsync(AdminBarbeariaUser user)
    {
        await _context.AdminBarbeariaUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }
}
```

### BarberRepository
```csharp
public class BarberRepository : IBarberRepository
{
    private readonly AppDbContext _context;

    public BarberRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Barber?> GetByEmailAndBarbeariaIdAsync(
        string email, Guid barbeariaId)
    {
        return await _context.Barbers
            .Include(b => b.Barbearia)
            .FirstOrDefaultAsync(b =>
                b.Email == email &&
                b.BarbeariaId == barbeariaId);
    }

    public async Task<IEnumerable<Barber>> GetByBarbeariaIdAsync(Guid barbeariaId)
    {
        return await _context.Barbers
            .Where(b => b.BarbeariaId == barbeariaId)
            .ToListAsync();
    }

    public async Task<Barber> AddAsync(Barber barber)
    {
        await _context.Barbers.AddAsync(barber);
        await _context.SaveChangesAsync();
        return barber;
    }
}
```

### CustomerRepository
```csharp
public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByEmailAsync(string email)
    {
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == email);
    }

    public async Task<Customer> AddAsync(Customer customer)
    {
        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
        return customer;
    }
}
```

## Critérios de Sucesso
- ✅ Todos os repositórios implementam suas interfaces corretamente
- ✅ Isolamento de tenant funciona corretamente (AdminBarbearia, Barbeiro)
- ✅ Queries otimizadas com Include apropriado
- ✅ Testes unitários cobrem cenários principais
- ✅ Não há vazamento de dados entre tenants
- ✅ Performance adequada com índices apropriados

## Tempo Estimado
**3 horas**

## Referências
- TechSpec: Seção "4.1 Fase 1.1: Domínio, Entidades e Interfaces"
- PRD: Seção "Requisitos de Isolamento Multi-tenant"
- Padrão Repository Pattern
