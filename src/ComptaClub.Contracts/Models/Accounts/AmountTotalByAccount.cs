namespace ComptaClub.Contracts.Models.Accounts;

public class AmountTotalByAccount
{
    public Guid Id { get; set; }
    public string Code { get; set; } = null!;
    public string Label { get; set; } = null!;
    public long Total { get; set; }
}
