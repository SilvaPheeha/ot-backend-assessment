
using Cortex.Mediator;

namespace OT.Assessment.App.Feature.Wager.GetTopSpanders;
public static class GetTopSpandersEndpoint
{
    public static void MapGetTopSpandersEndpoint(this WebApplication endpoints)
    {
        endpoints.MapGet("/get-top-spanders", async (IMediator mediator) =>
        {
            try
            {
                var response = await mediator.SendQueryAsync(new TopSpandersQuery());
                return Results.Ok(response);

            } catch (Exception)
            {
                return Results.BadRequest("YOOH!! An error occurred while processing your request.");
            }
    
        }).WithName("GetTopSpanders").WithTags("Wager Feature");
         
    }
}
 