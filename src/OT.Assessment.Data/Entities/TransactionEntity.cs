namespace OT.Assessment.Data.Entities;

public class TransactionEntity
{
    public Guid TransactionId { get; set; }
    public Guid AccountId { get; set; }
    public Guid TransactionTypeId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public AccountEntity Account { get; set; } = null!;
    public TransactionTypeEntity TransactionType { get; set; } = null!;
    public ICollection<WagerEntity> Wagers { get; set; } = new List<WagerEntity>();
}
