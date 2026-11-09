namespace OT.Assessment.Data.Entities;

public class TransactionTypeEntity
{
    public Guid TransactionTypeId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public ICollection<TransactionEntity> Transactions { get; set; } = new List<TransactionEntity>();
    public ICollection<WagerEntity> Wagers { get; set; } = new List<WagerEntity>();
}
