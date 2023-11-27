namespace ComptaClub.Contracts.Models.Accounts;

public record CreateAccountRequest : IRequest<AccountData>
{
    public CreateAccountRequest()
    {
        Code = "A completer";
        Label = "A completer";
        Direction = Enums.AccountDirection.Credit;
    }

    public CreateAccountRequest(string code, string label, Enums.AccountDirection accountDirection)
    {
        Code = code;
        Label = label;
        Direction = accountDirection;
    }

    public string Code { get; init; } = null!;
    public string Label { get; init; } = null!;
    public Enums.AccountDirection Direction { get; init; }
    public Guid? ParentId { get; set; }
}
