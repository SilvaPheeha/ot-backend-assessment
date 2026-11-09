using Cortex.Mediator;

namespace OT.Assessment.App.Feature.Wager.CreateWagers;

public static class CreateWagersEndpoint
{
    public static void MapCreateWagersEndpoint(this WebApplication endpoints)
    {
        endpoints.MapPost("/", async (CreateWagersCommand command, IMediator mediator) =>
        {
            try
            {
                await mediator.SendAsync(command);
                return Results.Ok("Wager created successfully.");

            } catch (Exception)
            {
                return Results.BadRequest("We got nothing, An error occurred while processing your request.");
            }
    
        }).WithName("CreateWagers").WithTags("Wager Feature");
         
    }
}
 