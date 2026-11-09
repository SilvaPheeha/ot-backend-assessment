using System;
using System.Text.Json.Serialization;

namespace OT.Assessment.Consumer.CreateWagers;

public class CreateWagers 
{
    [JsonPropertyName("wagerId")]
    public Guid WagerId { get; set; }

    [JsonPropertyName("theme")]
    public string Theme { get; set; } = string.Empty;

    [JsonPropertyName("provider")]
    public string Provider { get; set; } = string.Empty;

    [JsonPropertyName("gameName")]
    public string GameName { get; set; } = string.Empty;

    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("brandId")]
    public Guid BrandId { get; set; }

    [JsonPropertyName("accountId")]
    public Guid AccountId { get; set; }

    [JsonPropertyName("Username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("externalReferenceId")]
    public Guid ExternalReferenceId { get; set; }

    [JsonPropertyName("transactionTypeId")]
    public Guid TransactionTypeId { get; set; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("createdDateTime")]
    public DateTimeOffset CreatedDateTime { get; set; }

    [JsonPropertyName("numberOfBets")]
    public int NumberOfBets { get; set; }

    [JsonPropertyName("countryCode")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("sessionData")]
    public string SessionData { get; set; } = string.Empty;

    [JsonPropertyName("duration")]
    public long Duration { get; set; }
}