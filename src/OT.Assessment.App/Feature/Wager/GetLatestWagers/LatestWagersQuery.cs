
using Cortex.Mediator.Queries;

namespace OT.Assessment.App.Feature.Wager.GetLatestWagers;

public class LatestWagersQuery : IQuery<LatestWagersResponse>
{
    public Guid playerId { get; set; }
    public int page { get; set; } = 1;
    public int pageSize { get; set; } = 10;
}

public class LatestWagersResponse
{
    public List<LatestWagers> Data { get; set; } = new List<LatestWagers>();
    public int page { get; set; }
    public int pageSize { get; set; }
    public int total { get; set; }
    public int totalPages { get; set; }
}


public class LatestWagers
{
    public Guid WagerId { get; set; }
    public string Game { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime CreatedDate { get; set; }
}