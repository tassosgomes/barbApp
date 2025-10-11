# Guia: Implementação Genérica do Unit of Work (UoW)

Este guia tem como objetivo ensinar como implementar o padrão Unit of Work (UoW) em qualquer projeto .NET seguindo as práticas adotadas no projeto FC.CodeFlixCatalog.

## O que é Unit of Work?

O Unit of Work (UoW) é um padrão de design que coordena as operações de persistência de dados como uma única transação. Ele garante que todas as operações sejam confirmadas (commit) ou revertidas (rollback) como uma unidade atômica.

## Benefícios

- **Consistência Transacional**: Garante que todas as operações sejam aplicadas como uma única transação.
- **Controle de Eventos de Domínio**: Permite publicar eventos de domínio em conjunto com as operações de persistência.
- **Separação de Responsabilidades**: Separa a lógica de negócios da lógica de persistência.

## Estrutura Básica

### 1. Interface IUnityOfWork

Primeiro, crie a interface base para o UoW:

```csharp
public interface IUnityOfWork
{
    Task Commit(CancellationToken cancellationToken);
    Task Rollback(CancellationToken cancellationToken);
}
```

### 2. Implementação Concreta

A implementação concreta do UoW dependerá do seu ORM/Contexto de dados. Aqui está um exemplo genérico:

```csharp
public class UnityOfWork : IUnityOfWork
{
    private readonly SeuDbContext _context;
    private readonly IDomainEventPublisher _publisher; // Opcional
    private readonly ILogger<UnityOfWork> _logger;     // Opcional

    public UnityOfWork(
        SeuDbContext context,
        IDomainEventPublisher publisher, // Opcional
        ILogger<UnityOfWork> logger)     // Opcional
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        // Opcional: Publicar eventos de domínio
        var aggregateRoots = _context.ChangeTracker
            .Entries<AggregateRoot>()
            .Where(entry => entry.Entity.Events.Any())
            .Select(entry => entry.Entity);

        _logger?.LogInformation("Commit: {AggregateCounts} events", aggregateRoots.Count());

        var events = aggregateRoots.SelectMany(aggregate => aggregate.Events);

        _logger?.LogInformation("Commit: {events.Count} events raised", events.Count());

        foreach (var @event in events)
        {
            await _publisher.PublishAsync((dynamic)@event, cancellationToken);
        }

        foreach (var aggregate in aggregateRoots)
        {
            aggregate.ClearEvents();
        }

        // Persistir as mudanças
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken)
    {
        // Implementação específica depende do ORM e estratégia de controle de transações
        return Task.CompletedTask;
    }
}
```

### 3. Registro no DI Container

Registre o UoW no contêiner de injeção de dependência (geralmente em `Program.cs` ou em uma classe de configuração):

```csharp
services.AddTransient<IUnityOfWork, UnityOfWork>();
```

## Integração com Use Cases

O UoW é utilizado nos *use cases* da aplicação para garantir a consistência transacional. Exemplo:

```csharp
public class CreateEntityUseCase : ICreateEntityUseCase
{
    private readonly IEntityRepository _repository;
    private readonly IUnityOfWork _unitOfWork;

    public CreateEntityUseCase(
        IEntityRepository repository,
        IUnityOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EntityOutput> Handle(CreateEntityInput input, CancellationToken cancellationToken)
    {
        var entity = new Entity(input.Name, input.Description);

        await _repository.Insert(entity, cancellationToken);
        await _unitOfWork.Commit(cancellationToken); // Persiste e publica eventos

        return EntityOutput.FromEntity(entity);
    }
}
```

## Considerações Específicas por ORM

### Entity Framework

- Utilize `DbContext.SaveChanges()` ou `DbContext.SaveChangesAsync()` para persistir as mudanças.
- Use `ChangeTracker` para identificar entidades modificadas.
- Eventos de domínio podem ser capturados através de `ChangeTracker.Entries<AggregateRoot>()`.

### Dapper ou ADO.NET

- Implemente controle transacional manual com `TransactionScope` ou transações do banco de dados.
- Gerencie manualmente as operações de insert/update/delete.

## Boas Práticas

1. **Manter o UoW Focado**: O UoW deve se concentrar apenas em transações e eventos de domínio.
2. **Utilizar Transações Curtas**: Mantenha as transações o menor tempo possível para evitar locks.
3. **Tratamento de Erros**: Implemente tratamento adequado de exceções no método `Commit`.
4. **Logs e Monitoramento**: Utilize logging para monitorar as operações do UoW.
5. **Testabilidade**: Mantenha a implementação testável permitindo fácil substituição por mocks.

## Exemplo de Implementação Sem Eventos de Domínio

Se você não estiver utilizando eventos de domínio, a implementação pode ser simplificada:

```csharp
public class UnityOfWork : IUnityOfWork
{
    private readonly SeuDbContext _context;

    public UnityOfWork(SeuDbContext context)
    {
        _context = context;
    }

    public async Task Commit(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task Rollback(CancellationToken cancellationToken)
    {
        // Implementação depende da estratégia de controle transacional
        return Task.CompletedTask;
    }
}
```

## Conclusão

Esta implementação genérica do UoW pode ser adaptada para qualquer projeto .NET. Lembre-se de ajustar a implementação de acordo com o ORM utilizado e os requisitos específicos do seu domínio, especialmente em relação ao tratamento de eventos de domínio e controle transacional.