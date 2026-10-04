namespace ComptaClub.Datas;

[Table("McpApiKeys")]
public sealed class McpApiKeyData : IPrimaryKey
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string KeyIdentifier { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonIgnore]
    public string SecretHash { get; set; } = string.Empty;
    public string SecretLastFour { get; set; } = string.Empty;
    public DateTime CreationDateUtc { get; set; }
    public DateTime? ExpirationDateUtc { get; set; }
    public DateTime? RevokedDateUtc { get; set; }
    public DateTime? ArchivedDateUtc { get; set; }
    public DateTime? LastUsedDateUtc { get; set; }
    public long UsageCount { get; set; }
    public Guid CreatedByUserId { get; set; }
    [System.ComponentModel.DataAnnotations.ConcurrencyCheck]
    public Guid Version { get; set; }
}
