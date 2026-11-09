namespace OT.Assessment.Data.Entities;

public class BetEntity
{
    public Guid BetId { get; set; }
    public int NoOfBets { get; set; }
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public ICollection<WagerEntity> Wagers { get; set; } = new List<WagerEntity>();
}
