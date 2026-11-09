namespace OT.Assessment.Data.Entities;

public class AccountEntity
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountType { get; set; }
    public string? BranchCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }
    public UserEntity User { get; set; } = null!;
    public ICollection<TransactionEntity> Transactions { get; set; } = new List<TransactionEntity>();
    public ICollection<WagerEntity> Wagers { get; set; } = new List<WagerEntity>();
}
