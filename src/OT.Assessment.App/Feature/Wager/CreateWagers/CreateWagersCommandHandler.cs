using Cortex.Mediator.Commands;

namespace OT.Assessment.App.Feature.Wager.CreateWagers;

public class CreateWagersCommandHandler : ICommandHandler<CreateWagersCommand>
{
    private readonly Interface.ICreateWagersPublisher _publisher;
    private readonly ILogger<CreateWagersCommandHandler> _logger;
    public CreateWagersCommandHandler(Interface.ICreateWagersPublisher publisher, ILogger<CreateWagersCommandHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }
    public async Task Handle(CreateWagersCommand command, CancellationToken cancellationToken)
    {
        try{
            _logger.LogInformation("Publishing Wager with Id: {WagerId}", command.WagerId);
            await _publisher.PublishAsync(command, messageId: command.WagerId.ToString(), ct: cancellationToken);   
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Ohhh S***T, Error publishing Wager with Id: {WagerId}", command.WagerId);
            throw;
        }
    }
}