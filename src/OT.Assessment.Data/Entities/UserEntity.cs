namespace OT.Assessment.Data.Entities;

public class UserEntity
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? IdNumber { get; set; }
    public string? IdType { get; set; }
    public string? CountryCode { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedDateTime { get; set; }
    public DateTimeOffset? UpdatedDateTime { get; set; }

    public ICollection<AccountEntity> Accounts { get; set; } = new List<AccountEntity>();
}
