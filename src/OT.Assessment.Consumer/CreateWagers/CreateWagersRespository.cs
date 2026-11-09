using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OT.Assessment.Common.Enum;
using OT.Assessment.Data;

namespace OT.Assessment.Consumer.CreateWagers;

public class CreateWagersRepository
{
    private const string InsertWagersSql = """
        EXEC @Result = dbo.InsertWagers
            @WagerId = @WagerId,
            @Theme = @Theme,
            @Provider = @Provider,
            @GameName = @GameName,
            @TransactionId = @TransactionId,
            @BrandId = @BrandId,
            @AccountId = @AccountId,
            @Username = @Username,
            @ExternalReferenceId = @ExternalReferenceId,
            @TransactionTypeId = @TransactionTypeId,
            @Amount = @Amount,
            @CreatedDateTime = @CreatedDateTime,
            @NumberOfBets = @NumberOfBets,
            @CountryCode = @CountryCode,
            @SessionData = @SessionData,
            @Duration = @Duration
        """;

    private readonly ILogger<CreateWagersRepository> _logger;
    private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

    public CreateWagersRepository(ILogger<CreateWagersRepository> logger, IDbContextFactory<AppDbContext> dbContextFactory)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<DbState> SaveAsync(CreateWagers wager, CancellationToken ct)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(ct);

        var result = new SqlParameter("@Result", SqlDbType.Int) { Direction = ParameterDirection.Output };

        SqlParameter[] parameters =
        [
            result,
            new("@WagerId", SqlDbType.UniqueIdentifier) { Value = wager.WagerId },
            new("@Theme", SqlDbType.NVarChar, 100) { Value = wager.Theme },
            new("@Provider", SqlDbType.NVarChar, 200) { Value = wager.Provider },
            new("@GameName", SqlDbType.NVarChar, 200) { Value = wager.GameName },
            new("@TransactionId", SqlDbType.UniqueIdentifier) { Value = wager.TransactionId },
            new("@BrandId", SqlDbType.UniqueIdentifier) { Value = wager.BrandId },
            new("@AccountId", SqlDbType.UniqueIdentifier) { Value = wager.AccountId },
            new("@Username", SqlDbType.NVarChar, 100) { Value = wager.Username },
            new("@ExternalReferenceId", SqlDbType.UniqueIdentifier) { Value = wager.ExternalReferenceId },
            new("@TransactionTypeId", SqlDbType.UniqueIdentifier) { Value = wager.TransactionTypeId },
            new("@Amount", SqlDbType.Decimal) { Precision = 18, Scale = 6, Value = wager.Amount },
            new("@CreatedDateTime", SqlDbType.DateTimeOffset) { Value = wager.CreatedDateTime },
            new("@NumberOfBets", SqlDbType.Int) { Value = wager.NumberOfBets },
            new("@CountryCode", SqlDbType.Char, 10) { Value = wager.CountryCode },
            new("@SessionData", SqlDbType.NVarChar, -1) { Value = wager.SessionData },
            new("@Duration", SqlDbType.BigInt) { Value = wager.Duration },
        ];

        await db.Database.ExecuteSqlRawAsync(InsertWagersSql, parameters, ct);

        var state = (DbState)(int)result.Value;

        if (state == DbState.DUPLICATE)
        {
            _logger.LogInformation("DON'T!! Wager {WagerId} was already saved; we skipping this one.", wager.WagerId);
        }

        return state;
    }
}
