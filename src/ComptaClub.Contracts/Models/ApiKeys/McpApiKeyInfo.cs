namespace ComptaClub.Contracts.Models.ApiKeys;

public sealed record McpApiKeyInfo(
    Guid Id, string Name, string MaskedKey, Guid CreatedByUserId, string CreatorName,
    DateTime CreationDateUtc, DateTime? ExpirationDateUtc, DateTime? RevokedDateUtc,
    DateTime? ArchivedDateUtc, DateTime? LastUsedDateUtc, long UsageCount, Guid Version);
