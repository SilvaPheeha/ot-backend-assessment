namespace OT.Assessment.Data.Entities;

public class BrandEntity
{
    public Guid BrandId { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public ICollection<WagerEntity> Wagers { get; set; } = new List<WagerEntity>();
}
