# RabbitMQ Implementation Guidelines

This document provides guidelines for implementing RabbitMQ in new projects or extending existing ones, ensuring consistency, reliability, and maintainability.

## 1. Core Concepts

Before implementing, ensure you understand the fundamental RabbitMQ concepts:

*   **Producers**: Applications that send messages.
*   **Consumers**: Applications that receive and process messages.
*   **Exchanges**: Receive messages from producers and route them to queues.
    *   **Direct Exchange**: Routes messages to queues whose binding key exactly matches the routing key of the message.
    *   **Fanout Exchange**: Routes messages to all bound queues, ignoring the routing key.
    *   **Topic Exchange**: Routes messages to queues based on wildcard matches between the routing key and the binding key.
    *   **Headers Exchange**: Routes messages based on message headers.
*   **Queues**: Store messages until they are consumed.
    *   **Quorum Queues**: Recommended for high availability and data safety. They replicate messages across multiple nodes.
*   **Bindings**: Rules that exchanges use to route messages to queues.
*   **Routing Key**: A message attribute that the exchange uses to decide how to route the message.
*   **Publisher Confirmations**: A mechanism for producers to ensure messages have reached the broker.
*   **Acknowledgments (ACK/NACK)**: Consumers explicitly tell RabbitMQ when a message has been successfully processed (ACK) or if it failed (NACK).
*   **Dead-Letter Exchanges (DLX) and Dead-Letter Queues (DLQ)**: Messages that cannot be delivered or processed (e.g., rejected, expired, or hit a max retry limit) are routed to a DLX, which then routes them to a DLQ.

## 2. Configuration

All RabbitMQ-related configurations should be externalized, typically in `appsettings.json` (for .NET applications) or similar configuration files.

**Example `appsettings.json` structure:**

```json
{
  "RabbitMQ": {
    "Hostname": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "VirtualHost": "/",
    "Exchange": "your.application.events"
  }
}
```

*   **`Hostname`**: The RabbitMQ broker's hostname or IP address.
*   **`Port`**: The port for RabbitMQ (default is 5672).
*   **`Username`**: Username for connecting to RabbitMQ.
*   **`Password`**: Password for connecting to RabbitMQ.
*   **`VirtualHost`**: The virtual host to connect to (default is `/`).
*   **`Exchange`**: The default exchange name for your application's events.

## 3. Dependency Injection Setup

Use Dependency Injection (DI) to manage RabbitMQ components.

**Example `MessagingConfiguration.cs` (or similar setup in `Program.cs`):**

```csharp
public static class MessagingConfiguration
{
    public static IServiceCollection AddRabbitMQ(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMQConfiguration>(configuration.GetSection(RabbitMQConfiguration.ConfigurationSection));

        services.AddSingleton<IConnection>(sp =>
        {
            var config = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
            var factory = new ConnectionFactory
            {
                HostName = config.Hostname,
                UserName = config.Username,
                Password = config.Password,
                VirtualHost = config.VirtualHost,
                Port = config.Port ?? 5672
            };
            return factory.CreateConnection(); // Use CreateConnectionAsync for async
        });

        services.AddSingleton<IRabbitMqChannelProvider, RabbitMqChannelProvider>();
        services.AddSingleton<IRabbitMqTopologyInitializer, RabbitMqTopologyInitializer>();
        services.AddSingleton<IMessageProducer, RabbitMQProducer>(); // Register your producer
        services.AddHostedService<RabbitMqTopologyHostedService>(); // Ensure topology is initialized

        return services;
    }
}
```

## 4. Topology Initialization

The RabbitMQ topology (exchanges, queues, and bindings) should be declared programmatically on application startup. This ensures that the necessary messaging infrastructure exists before messages are sent or consumed.

*   **Hosted Service**: Use an `IHostedService` to trigger the topology initialization.
*   **Idempotency**: Ensure your topology declaration logic is idempotent (running it multiple times should not cause issues). RabbitMQ's `ExchangeDeclare`, `QueueDeclare`, and `QueueBind` methods are idempotent by default.
*   **Dead-Lettering**: Always configure dead-letter exchanges and queues for your main queues. This is crucial for handling messages that fail processing.
    *   Set `x-dead-letter-exchange` and `x-dead-letter-routing-key` arguments on your main queues.
    *   Consider `x-delivery-limit` for retries before dead-lettering.
*   **Quorum Queues**: Prefer quorum queues for durability and consistency. Set `"x-queue-type": "quorum"` in `QueueDeclare` arguments.

**Example `RabbitMqTopologyInitializer.cs`:**

```csharp
public class RabbitMqTopologyInitializer : IRabbitMqTopologyInitializer
{
    private readonly IRabbitMqChannelProvider _channelProvider;
    private readonly RabbitMQConfiguration _config;

    public RabbitMqTopologyInitializer(IRabbitMqChannelProvider channelProvider, IOptions<RabbitMQConfiguration> config)
    {
        _channelProvider = channelProvider;
        _config = config.Value;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        var channel = await _channelProvider.GetChannelAsync();

        // Declare main exchange
        await channel.ExchangeDeclareAsync(_config.Exchange, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);

        // Declare dead-letter exchange
        var dlqExchangeName = $"{_config.Exchange}.dlq";
        await channel.ExchangeDeclareAsync(dlqExchangeName, ExchangeType.Direct, durable: true, cancellationToken: cancellationToken);

        // Example: Declare a queue for a specific event type
        // Assuming EventsMapping provides routing keys
        foreach (var mapping in EventsMapping.GetAllRoutingKey()) // Replace with your actual event mappings
        {
            var queueName = $"{mapping.Value}.queue";
            var dlqQueueName = $"{queueName}.dlq";
            var routingKey = mapping.Value;
            var dlqRoutingKey = $"{routingKey}.dlq";

            // Declare main queue with dead-lettering arguments
            await channel.QueueDeclareAsync(
                queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-queue-type", "quorum" },
                    { "x-dead-letter-exchange", dlqExchangeName },
                    { "x-dead-letter-routing-key", dlqRoutingKey },
                    { "x-delivery-limit", 1 } // Example: message dead-letters after 1 delivery attempt
                },
                cancellationToken: cancellationToken);

            // Declare dead-letter queue
            await channel.QueueDeclareAsync(
                dlqQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: new Dictionary<string, object?>
                {
                    { "x-queue-type", "quorum" }
                },
                cancellationToken: cancellationToken);

            // Bind main queue to main exchange
            await channel.QueueBindAsync(queueName, _config.Exchange, routingKey, cancellationToken: cancellationToken);

            // Bind dead-letter queue to dead-letter exchange
            await channel.QueueBindAsync(dlqQueueName, dlqExchangeName, dlqRoutingKey, cancellationToken: cancellationToken);
        }
    }
}
```

## 5. Message Production

*   **Abstraction**: Use an `IMessageProducer` interface to decouple your application logic from RabbitMQ specifics.
*   **Serialization**: Messages should be serialized into a common format (e.g., JSON). Use a consistent naming policy (e.g., snake_case) for properties.
*   **Publisher Confirmations**: Always enable and handle publisher confirmations to ensure messages are reliably received by the broker. This typically involves waiting for `BasicAcks` or handling `BasicNacks` for failed deliveries.
*   **Message Properties**: Set relevant message properties like `ContentType` and `DeliveryMode` (e.g., `Persistent` for durability).

**Example `RabbitMQProducer.cs`:**

```csharp
public class RabbitMQProducer : IMessageProducer
{
    private readonly IChannel _channel;
    private readonly string _exchangeName;

    public RabbitMQProducer(IRabbitMqChannelProvider channelProvider, IOptions<RabbitMQConfiguration> options)
    {
        _channel = channelProvider.GetChannelAsync().GetAwaiter().GetResult(); // Get channel synchronously for constructor
        _exchangeName = options.Value.Exchange;
    }

    public async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken)
    {
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new JsonSnakeCasePolicy() // Ensure consistent naming
        };

        var routingKey = EventsMapping.GetRoutingKey<T>(); // Your logic to get routing key
        var body = JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions);

        var properties = _channel.CreateBasicProperties();
        properties.ContentType = "application/json";
        properties.DeliveryMode = DeliveryModes.Persistent; // Make message durable

        // Publish the message
        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: routingKey,
            mandatory: true, // Return message to publisher if it cannot be routed
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);

        // For critical messages, implement logic to wait for publisher confirmations
        // Example:
        // var sequenceNumber = _channel.NextPublishSeqNo;
        // if (!await _channel.WaitForConfirmsAsync(TimeSpan.FromSeconds(5)))
        // {
        //     // Handle NACK: log, retry, or move to a separate error queue
        //     Console.WriteLine($"Message with sequence {sequenceNumber} was NACKed or timed out.");
        // }
    }
}
```

## 6. Message Consumption (Conceptual)

While not fully implemented in the provided codebase, a robust consumer implementation is vital.

*   **Dedicated Consumers**: Create dedicated consumer services or background workers for each queue.
*   **Acknowledgments**: Always acknowledge messages (`channel.BasicAck`) only after successful processing. If processing fails, `channel.BasicNack` (with `requeue: false` to send to DLQ) or `channel.BasicReject` should be used.
*   **Error Handling**: Implement comprehensive error handling within consumer logic. Catch exceptions, log details, and decide whether to retry or dead-letter the message.
*   **Concurrency**: Manage consumer concurrency carefully to avoid overwhelming downstream systems.
*   **Idempotency**: Ensure your message processing logic is idempotent, as messages might be redelivered (e.g., after a consumer crash).

## 7. Testing

*   **Unit Tests**: Test your `IMessageProducer` and `IRabbitMqTopologyInitializer` in isolation. Use in-memory mocks for `IConnection` and `IChannel` where appropriate.
*   **Integration Tests**: Set up a local RabbitMQ instance (e.g., using Docker Compose) for integration tests. This allows you to verify the end-to-end flow of messages.
*   **Topology Verification**: In integration tests, verify that the topology is correctly declared (exchanges, queues, bindings exist as expected).
*   **Message Content Verification**: Verify that messages are published with the correct content and properties.
*   **Consumer Behavior**: For consumers, test their ability to process messages correctly, handle errors, and acknowledge/dead-letter messages as expected.

**Example `RabbitMQProducerTest.cs` (Integration Test Snippet):**

```csharp
public class RabbitMQProducerTest
{
    [Fact]
    public async Task SendMessageAsync_PublishesMessageToRabbitMQ()
    {
        // Arrange
        // Use a test-specific RabbitMQ configuration (e.g., from appsettings.EndToEndTest.json)
        var config = new RabbitMQConfiguration
        {
            Hostname = "localhost", // Or your test RabbitMQ host
            Port = 5672,
            Username = "guest",
            Password = "guest",
            VirtualHost = "/",
            Exchange = "test.video.events"
        };
        var options = Options.Create(config);

        var factory = new ConnectionFactory
        {
            HostName = config.Hostname,
            Port = config.Port ?? 5672,
            UserName = config.Username,
            Password = config.Password,
            VirtualHost = config.VirtualHost
        };

        using var connection = await factory.CreateConnectionAsync();
        var channelProvider = new RabbitMqChannelProvider(connection);
        var topologyInitializer = new RabbitMqTopologyInitializer(channelProvider, options);

        // Initialize topology for the test
        await topologyInitializer.InitializeAsync(CancellationToken.None);

        var producer = new RabbitMQProducer(channelProvider, options);
        var videoUploadedEvent = new VideoUploadedEvent(Guid.NewGuid(), "video/test_file.mp4");

        // Act
        await producer.SendMessageAsync(videoUploadedEvent, CancellationToken.None);

        // Assert (Consume the message to verify)
        var consumerChannel = await connection.CreateChannelAsync();
        var queueName = $"{EventsMapping.GetRoutingKey<VideoUploadedEvent>()}.queue"; // Get the expected queue name

        // BasicGet is for simple testing; for real consumers, use BasicConsume
        var result = consumerChannel.BasicGet(queueName, autoAck: true);

        result.Should().NotBeNull();
        var receivedMessage = JsonSerializer.Deserialize<VideoUploadedEvent>(result.Body.ToArray(), new JsonSerializerOptions { PropertyNamingPolicy = new JsonSnakeCasePolicy() });
        receivedMessage.Should().BeEquivalentTo(videoUploadedEvent);

        consumerChannel.Close();
        connection.Close();
    }
}
```

## 8. Best Practices

*   **Small, Focused Messages**: Messages should be small and contain only the necessary data for the consumer to perform its task. Avoid sending large payloads.
*   **Event-Driven Architecture**: Design your system around events. Producers publish events, and consumers react to them.
*   **Asynchronous Operations**: Use `async/await` throughout your RabbitMQ interactions to avoid blocking threads.
*   **Connection and Channel Management**: Reuse `IConnection` instances. For `IChannel` instances, it's common to have one channel per thread or to pool channels, but for simple producers, a single shared channel with publisher confirmations can work. Be mindful of thread safety if sharing channels.
*   **Graceful Shutdown**: Implement graceful shutdown logic for your applications to ensure all pending messages are processed or returned to the queue before the application exits.
*   **Monitoring and Alerting**: Monitor key RabbitMQ metrics (e.g., queue size, message rates, connection status, consumer count, unacknowledged messages) and set up alerts for anomalies.
*   **Logging**: Implement comprehensive logging for all RabbitMQ operations, including message publishing, consumption, and error handling.
*   **Security**: Secure your RabbitMQ instance with strong passwords, TLS, and appropriate user permissions.
*   **Error Handling and Retries**:
    *   **Transient Errors**: For temporary issues (e.g., network glitches), implement retry mechanisms with exponential backoff.
    *   **Permanent Errors**: For unrecoverable errors, dead-letter the message.
    *   **Circuit Breaker**: Consider implementing a circuit breaker pattern for calls to external services from consumers to prevent cascading failures.
