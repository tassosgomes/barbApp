---
status: pending
parallelizable: false
blocked_by: ["2.0"]
---

<task_context>
<domain>backend/data-access</domain>
<type>implementation</type>
<scope>core_feature</scope>
<complexity>medium</complexity>
<dependencies>database</dependencies>
<unblocks>4.0</unblocks>
</task_context>

# Tarefa 3.0: Repositórios e Unit of Work

## Visão Geral

Implementar os repositórios de acesso a dados para Landing Page seguindo o padrão Repository e Unit of Work já estabelecido no projeto. Garantir que as operações CRUD e consultas especializadas estejam disponíveis.

<requirements>
- Interface `ILandingPageConfigRepository`
- Implementação `LandingPageConfigRepository`
- Interface `ILandingPageServiceRepository`
- Implementação `LandingPageServiceRepository`
- Integração com `IUnitOfWork` existente
- Queries otimizadas com Include
- Métodos especializados (GetByBarbershopId, GetPublicByCode)
</requirements>

## Subtarefas

- [ ] 3.1 Criar interface `ILandingPageConfigRepository`
- [ ] 3.2 Implementar `LandingPageConfigRepository`
- [ ] 3.3 Criar interface `ILandingPageServiceRepository`
- [ ] 3.4 Implementar `LandingPageServiceRepository`
- [ ] 3.5 Adicionar repositórios ao `IUnitOfWork`
- [ ] 3.6 Criar queries otimizadas com navegação
- [ ] 3.7 Implementar testes de repositório

## Detalhes de Implementação

### Interface: ILandingPageConfigRepository.cs

```csharp
namespace BarbApp.Domain.Interfaces.Repositories
{
    public interface ILandingPageConfigRepository : IRepository<LandingPageConfig>
    {
        Task<LandingPageConfig?> GetByBarbershopIdAsync(Guid barbershopId, CancellationToken cancellationToken = default);
        Task<LandingPageConfig?> GetByBarbershopCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<LandingPageConfig?> GetByBarbershopIdWithServicesAsync(Guid barbershopId, CancellationToken cancellationToken = default);
        Task<LandingPageConfig?> GetPublicByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<bool> ExistsForBarbershopAsync(Guid barbershopId, CancellationToken cancellationToken = default);
    }
}
```

### Implementação: LandingPageConfigRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Repositories
{
    public class LandingPageConfigRepository : Repository<LandingPageConfig>, ILandingPageConfigRepository
    {
        public LandingPageConfigRepository(AppDbContext context) : base(context)
        {
        }
        
        public async Task<LandingPageConfig?> GetByBarbershopIdAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(lp => lp.Barbershop)
                .Include(lp => lp.Services)
                    .ThenInclude(lps => lps.Service)
                .FirstOrDefaultAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);
        }
        
        public async Task<LandingPageConfig?> GetByBarbershopCodeAsync(
            string code, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(lp => lp.Barbershop)
                .FirstOrDefaultAsync(lp => lp.Barbershop.Code == code, cancellationToken);
        }
        
        public async Task<LandingPageConfig?> GetByBarbershopIdWithServicesAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(lp => lp.Barbershop)
                .Include(lp => lp.Services.Where(s => s.IsVisible))
                    .ThenInclude(lps => lps.Service)
                .AsSplitQuery()
                .FirstOrDefaultAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);
        }
        
        public async Task<LandingPageConfig?> GetPublicByCodeAsync(
            string code, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(lp => lp.Barbershop)
                .Include(lp => lp.Services.Where(s => s.IsVisible).OrderBy(s => s.DisplayOrder))
                    .ThenInclude(lps => lps.Service)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    lp => lp.Barbershop.Code == code && lp.IsPublished, 
                    cancellationToken);
        }
        
        public async Task<bool> ExistsForBarbershopAsync(
            Guid barbershopId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(lp => lp.BarbershopId == barbershopId, cancellationToken);
        }
    }
}
```

### Interface: ILandingPageServiceRepository.cs

```csharp
namespace BarbApp.Domain.Interfaces.Repositories
{
    public interface ILandingPageServiceRepository : IRepository<LandingPageService>
    {
        Task<List<LandingPageService>> GetByLandingPageIdAsync(Guid landingPageId, CancellationToken cancellationToken = default);
        Task DeleteByLandingPageIdAsync(Guid landingPageId, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(Guid landingPageId, Guid serviceId, CancellationToken cancellationToken = default);
    }
}
```

### Implementação: LandingPageServiceRepository.cs

```csharp
using Microsoft.EntityFrameworkCore;

namespace BarbApp.Infrastructure.Repositories
{
    public class LandingPageServiceRepository : Repository<LandingPageService>, ILandingPageServiceRepository
    {
        public LandingPageServiceRepository(AppDbContext context) : base(context)
        {
        }
        
        public async Task<List<LandingPageService>> GetByLandingPageIdAsync(
            Guid landingPageId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(lps => lps.Service)
                .Where(lps => lps.LandingPageConfigId == landingPageId)
                .OrderBy(lps => lps.DisplayOrder)
                .ToListAsync(cancellationToken);
        }
        
        public async Task DeleteByLandingPageIdAsync(
            Guid landingPageId, 
            CancellationToken cancellationToken = default)
        {
            var services = await _dbSet
                .Where(lps => lps.LandingPageConfigId == landingPageId)
                .ToListAsync(cancellationToken);
            
            _dbSet.RemoveRange(services);
        }
        
        public async Task<bool> ExistsAsync(
            Guid landingPageId, 
            Guid serviceId, 
            CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(
                lps => lps.LandingPageConfigId == landingPageId && lps.ServiceId == serviceId,
                cancellationToken);
        }
    }
}
```

### Atualização: IUnitOfWork.cs

```csharp
namespace BarbApp.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        // ... repositórios existentes ...
        
        ILandingPageConfigRepository LandingPageConfigs { get; }
        ILandingPageServiceRepository LandingPageServices { get; }
        
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
```

### Atualização: UnitOfWork.cs

```csharp
namespace BarbApp.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        
        // ... repositórios existentes ...
        
        private ILandingPageConfigRepository? _landingPageConfigs;
        private ILandingPageServiceRepository? _landingPageServices;
        
        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }
        
        public ILandingPageConfigRepository LandingPageConfigs
        {
            get
            {
                _landingPageConfigs ??= new LandingPageConfigRepository(_context);
                return _landingPageConfigs;
            }
        }
        
        public ILandingPageServiceRepository LandingPageServices
        {
            get
            {
                _landingPageServices ??= new LandingPageServiceRepository(_context);
                return _landingPageServices;
            }
        }
        
        // ... métodos existentes ...
    }
}
```

## Sequenciamento

- **Bloqueado por**: 2.0 (Entities e DTOs)
- **Desbloqueia**: 4.0 (Serviços de Domínio)
- **Paralelizável**: Não

## Critérios de Sucesso

- [ ] Todos os repositórios implementados
- [ ] Queries otimizadas com Include funcionando
- [ ] Unit of Work integrado
- [ ] Testes de repositório passando
- [ ] Nenhum N+1 query problem
- [ ] Performance adequada em consultas
- [ ] Code coverage > 80%
