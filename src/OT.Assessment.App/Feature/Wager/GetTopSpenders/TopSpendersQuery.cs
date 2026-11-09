
using Cortex.Mediator.Queries;

namespace OT.Assessment.App.Feature.Wager.GetTopSpanders;

public class TopSpandersQuery : IQuery<List<TopSpandersResponse>>
{
}

public class TopSpandersResponse
{
    public Guid AccountId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal TotalAmountSpend { get; set; }
}