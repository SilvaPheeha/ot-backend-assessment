namespace OT.Assessment.Data.Entities;

public class WagerEntity
{
    public Guid WagerId { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public string CountryCode { get; set; } = string.Empty;
    public Guid TransactionId { get; set; }
    public Guid BrandId { get; set; }
    public Guid AccountId { get; set; }
    public Guid ExternalReferenceId { get; set; }
    public Guid TransactionTypeId { get; set; }
    public Guid BetId { get; set; }
    public string? SessionData { get; set; }
    public long? Duration { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public TransactionEntity Transaction { get; set; } = null!;
    public BrandEntity Brand { get; set; } = null!;
    public AccountEntity Account { get; set; } = null!;
    public TransactionTypeEntity TransactionType { get; set; } = null!;
    public BetEntity Bet { get; set; } = null!;
}
