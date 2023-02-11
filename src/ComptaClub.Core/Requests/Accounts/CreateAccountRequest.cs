namespace ComptaClub.Requests.Accounts;

public record CreateAccountRequest : IRequest<AccountData>
{
    public CreateAccountRequest()
    {
        Code = "A completer";
        Label = "A completer";
        Direction = AccountDirection.Credit;
    }

    public CreateAccountRequest(string code, string label, AccountDirection accountDirection)
    {
        Code = code;
        Label = label;
        Direction = accountDirection;
    }

    public string Code { get; init; } = null!;
    public string Label { get; init; } = null!;
    public AccountDirection Direction { get; init; }
    public Guid? ParentId { get; set; }
}
