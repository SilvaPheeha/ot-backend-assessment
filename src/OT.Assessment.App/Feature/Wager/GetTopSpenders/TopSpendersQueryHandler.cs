using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using OT.Assessment.Data;

namespace OT.Assessment.App.Feature.Wager.GetTopSpanders;

public class TopSpandersQueryHandler : IQueryHandler<TopSpandersQuery, List<TopSpandersResponse>>
{
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public TopSpandersQueryHandler(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<List<TopSpandersResponse>> Handle(TopSpandersQuery _, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.Wagers
            .GroupBy(w => new { w.AccountId, w.Account.User.Username })
            .Select(g => new TopSpandersResponse
            {
                AccountId = g.Key.AccountId,
                Username = g.Key.Username,
                TotalAmountSpend = g.Sum(w => w.Bet.Amount)
            })
            .OrderByDescending(r => r.TotalAmountSpend)
            .ToListAsync(cancellationToken);
    }
}