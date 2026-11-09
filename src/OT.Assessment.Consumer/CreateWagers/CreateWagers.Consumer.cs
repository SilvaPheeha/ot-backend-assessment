using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using OT.Assessment.Common.Enum;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OT.Assessment.Consumer.CreateWagers;

public class CreateWagersConsumer(IOptions<CreateWagersMqSettings> rabbitMqOptions, CreateWagersRepository repository) : IAsyncDisposable
{
    private readonly CreateWagersMqSettings _settings = rabbitMqOptions.Value;
    private IConnection? _connection;
    private IChannel? _channel;
    private string? _consumerTag;

    private readonly CreateWagersRepository _repository = repository;
    private static readonly JsonSerializerOptions _JsonOptions = new(JsonSerializerDefaults.Web);
    
    public async Task StartAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password,
            VirtualHost = _settings.VirtualHost,
            ClientProvidedName = "wager-consumer",
            AutomaticRecoveryEnabled = true,  
            TopologyRecoveryEnabled = true,
        };

        _connection = await factory.CreateConnectionAsync(ct);
        _channel = await _connection.CreateChannelAsync(cancellationToken: ct);

        await _channel.ExchangeDeclareAsync(_settings.Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: ct);
        await _channel.QueueDeclareAsync(_settings.Queue,
            durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);
        await _channel.QueueBindAsync(_settings.Queue, _settings.Exchange, _settings.RoutingKey,
            cancellationToken: ct);
        await _channel.QueueDeclareAsync(_settings.ErrorQueue,
            durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: ct);

        await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: _settings.PrefetchCount,
            global: false, cancellationToken: ct);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnMessageReceivedAsync;

        _consumerTag = await _channel.BasicConsumeAsync(_settings.Queue, autoAck: false,
            consumer: consumer, cancellationToken: ct);

        Console.WriteLine($"Listening on '{_settings.Queue}' (prefetch {_settings.PrefetchCount}). Ctrl+C to stop.");
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        IChannel channel = _channel!;
        string messageId = ea.BasicProperties.MessageId ?? $"tag-{ea.DeliveryTag}";
        CancellationToken ct = ea.CancellationToken;

        try
        {
            var wager = Deserialize<CreateWagers>(ea.Body.Span);

            var result = await _repository.SaveAsync(wager, ct);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, ct);
        }
        catch (Exception ex) when (ex is JsonException)
        {
            Console.Error.WriteLine($"MJ Bad: {messageId}: {ex.Message}");
            await MoveToErrorQueueAsync(ea, ex, ct);
        }
        catch (Exception ex) when (!ea.Redelivered)
        {
            Console.Error.WriteLine($"Failed {messageId}, requeueing: {ex.Message}");
            await channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, ct);
        }
        catch (Exception ex)
        {
            // Already failed once before: park it rather than retrying forever.
            Console.Error.WriteLine($"Failed {messageId} again, moving to error queue: {ex.Message}");
            await MoveToErrorQueueAsync(ea, ex, ct);
        }
    }

    private T Deserialize<T>(ReadOnlySpan<byte> body) where T : class
    {
        var message = JsonSerializer.Deserialize<T>(body, _JsonOptions)
                      ?? throw new JsonException("Message body was null.");

        return message;
    }

    private async Task MoveToErrorQueueAsync(BasicDeliverEventArgs ea, Exception error, CancellationToken ct)
    {
        IChannel channel = _channel!;

        var properties = new BasicProperties(ea.BasicProperties)
        {
            DeliveryMode = DeliveryModes.Persistent,
        };
        var headers = properties.Headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(properties.Headers);
            
        headers["x-error-message"] = Truncate(error.Message, 1000);
        headers["x-error-type"] = error.GetType().FullName;
        headers["x-failed-at"] = DateTimeOffset.UtcNow.ToString("O");
        headers["x-original-queue"] = _settings.Queue;
        properties.Headers = headers;

        await channel.BasicPublishAsync(exchange: "", routingKey: _settings.ErrorQueue,
            mandatory: false, basicProperties: properties, body: ea.Body, cancellationToken: ct);

        await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, ct);
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null)
        {
            if (_consumerTag is not null && _channel.IsOpen)
                await _channel.BasicCancelAsync(_consumerTag);
            await _channel.CloseAsync();
            await _channel.DisposeAsync();
        }
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}