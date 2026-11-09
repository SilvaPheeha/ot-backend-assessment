using Cortex.Mediator.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OT.Assessment.Data;


namespace OT.Assessment.App.Feature.Wager.GetLatestWagers;
public class GetLatestWagersQueryHandler : IQueryHandler<LatestWagersQuery, LatestWagersResponse>
{
    // TODO : Move this to the Appsettings.json, cant have 300s every where.
    private static readonly TimeSpan CacheDuration = TimeSpan.FromSeconds(300);

    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
    private readonly IMemoryCache _cache;

    public GetLatestWagersQueryHandler(IDbContextFactory<AppDbContext> dbContextFactory, IMemoryCache cache)
    {
        _dbContextFactory = dbContextFactory;
        _cache = cache;
    }

    public async Task<LatestWagersResponse> Handle(LatestWagersQuery query, CancellationToken cancellationToken)
    {
        var cacheKey = $"latest-wagers:{query.playerId}:{query.page}:{query.pageSize}";

        var response = await _cache.GetOrCreateAsync(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return LoadPageAsync(query, cancellationToken);
        });

        return response!;
    }

    private async Task<LatestWagersResponse> LoadPageAsync(LatestWagersQuery query, CancellationToken cancellationToken)
    {
       try {
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            var playerWagers = db.Wagers.Where(w => w.AccountId == query.playerId);
            var total = await playerWagers.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(total / (double)query.pageSize);

            var response = new LatestWagersResponse
            {
                page = query.page,
                pageSize = query.pageSize,
                total = total,
                totalPages = totalPages
            };

            if (query.page > totalPages)
                return response;

            response.Data = await playerWagers
                .OrderByDescending(w => w.Bet.Amount)
                .ThenBy(w => w.WagerId) 
                .Skip((query.page - 1) * query.pageSize)
                .Take(query.pageSize)
                .Select(w => new LatestWagers
                {
                    WagerId = w.WagerId,
                    Game = w.GameName,
                    Provider = w.Provider,
                    Amount = w.Bet.Amount,
                    CreatedDate = w.CreatedDateTime.UtcDateTime
                }).ToListAsync(cancellationToken);

            return response;
        } catch (Exception ex)
        {
            throw new Exception($"Yooohhh!, {nameof(LoadPageAsync)} failed.", ex);
        }
    }
}
