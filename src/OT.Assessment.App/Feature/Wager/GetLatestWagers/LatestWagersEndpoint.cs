using Cortex.Mediator;

namespace OT.Assessment.App.Feature.Wager.GetLatestWagers;

public static class GetLatestWagersEndpoint
{
    // TODO : Move this to the Appsettings.json, cant have 100s every where.
    private const int MaxPageSize = 100;

    public static void MapGetLatestWagersEndpoint(this WebApplication endpoints)
    {
        endpoints.MapGet("/get-latest-wagers/{playerId}", async (string playerId, IMediator mediator, int page = 1, int pageSize = 10) =>
        {
            if (page < 1 || pageSize < 1 || pageSize > MaxPageSize)
                return Results.BadRequest($"You see now, Stop it! Page and pageSize must be positive integers.");

            try
            {
                var response = await mediator.SendQueryAsync(new LatestWagersQuery { playerId = Guid.Parse(playerId), page = page, pageSize = pageSize });
                return Results.Ok(response);

            } catch (Exception)
            {
                return Results.BadRequest("Eish!! An error occurred while processing your request.");
            }
    
        }).WithName("GetLatestWagers").WithTags("Wager Feature");
         
    }
}
 