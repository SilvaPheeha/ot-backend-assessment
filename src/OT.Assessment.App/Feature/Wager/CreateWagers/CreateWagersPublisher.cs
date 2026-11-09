using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace OT.Assessment.App.Feature.Wager.CreateWagers;

public class CreateWagersPublisher(ILogger<CreateWagersPublisher> logger, IOptions<RabbitMqSettings> rabbitMqOptions) : Interface.ICreateWagersPublisher, IAsyncDisposable
{
    private readonly ILogger<CreateWagersPublisher> _logger = logger;
    private readonly RabbitMqSettings _rabbitMqSettings = rabbitMqOptions.Value;
    private readonly SemaphoreSlim _publishLock = new(1, 1);
    private readonly JsonSerializerOptions _JsonOptions = new(JsonSerializerDefaults.Web);
    private IConnection? _connection;
    private IChannel? _channel;

    private async Task CreateAsync(
        RabbitMqSettings settings, CancellationToken ct = default)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitMqSettings.HostName,
            Port = _rabbitMqSettings.Port,
            UserName = _rabbitMqSettings.UserName,
            Password = _rabbitMqSettings.Password,
            VirtualHost = _rabbitMqSettings.VirtualHost,
            ClientProvidedName = "wagers-publisher",       
            AutomaticRecoveryEnabled = true,             
            TopologyRecoveryEnabled = true,
        };

        _connection = await factory.CreateConnectionAsync(ct);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        _channel = await _connection.CreateChannelAsync(channelOptions, ct);

        await _channel.ExchangeDeclareAsync(
            exchange: _rabbitMqSettings.Exchange, type: ExchangeType.Topic,
            durable: true, autoDelete: false, cancellationToken: ct);

        await _channel.QueueDeclareAsync(
            queue: _rabbitMqSettings.Queue, durable: true, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: ct);

        await _channel.QueueBindAsync(
            queue: _rabbitMqSettings.Queue, exchange: _rabbitMqSettings.Exchange,
            routingKey: _rabbitMqSettings.RoutingKey, cancellationToken: ct);
    
    }


    public async ValueTask DisposeAsync()
    {
        try
        {
            if(_channel is not null)
                await _channel.CloseAsync();

            if(_connection is not null)
                await _connection.CloseAsync();
        }
        finally
        {
            if(_channel is not null)
                await _channel.DisposeAsync();
            
            if(_connection is not null)
                await _connection.DisposeAsync();

            _publishLock.Dispose();
        }
    }

    public async Task PublishAsync<T>(
        T message, string? messageId = null, CancellationToken ct = default)
    {
        await CreateAsync(_rabbitMqSettings, ct);
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(message, _JsonOptions);

        var properties = new BasicProperties
        {
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = messageId ?? Guid.NewGuid().ToString(),
            Type = typeof(T).Name,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        await _publishLock.WaitAsync(ct);
        try
        {
            if(_channel is null)
                throw new InvalidOperationException("Channel is not initialized, Push Start and try again");
                
            await _channel.BasicPublishAsync(
                exchange: _rabbitMqSettings.Exchange,
                routingKey: _rabbitMqSettings.RoutingKey,
                mandatory: true,
                basicProperties: properties,
                body: body,
                cancellationToken: ct);
        }
        catch (PublishException ex) when (ex.IsReturn)
        {
            throw new InvalidOperationException($"Eish, Message {properties.MessageId} was unroutable: no queue bound to {_rabbitMqSettings.Exchange} with routing key {_rabbitMqSettings.RoutingKey}.", ex);
        }
        catch (PublishException ex)
        {
            throw new InvalidOperationException($"Broker rejected (nacked) message {properties.MessageId}.", ex);
        }
        finally
        {
            _publishLock.Release();
        }
    }
}