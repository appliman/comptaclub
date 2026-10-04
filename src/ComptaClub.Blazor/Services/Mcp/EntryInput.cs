namespace ComptaClub.Blazor.Services.Mcp;

public sealed record EntryInput(string PartNumber, string Label, DateOnly CreationDate, DateOnly ValueDate,
    decimal AmountEuros, AccountDirection Direction, Guid BankId, Guid AccountId, Guid ExerciceId,
    PaymentType PaymentType, string? ExtraInfos = null);
