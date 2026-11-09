namespace OT.Assessment.App.Feature.Wager.CreateWagers.Interface;

public interface ICreateWagersPublisher
{
    Task PublishAsync<T>(T message, string? messageId = null, CancellationToken ct = default);
}