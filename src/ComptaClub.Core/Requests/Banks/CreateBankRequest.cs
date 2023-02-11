namespace ComptaClub.Requests.Banks;

public record CreateBankRequest : IRequest<BankData>
{
    public CreateBankRequest()
    {
        Code = "A Completer";
        Label = "A Completer";
    }

    public CreateBankRequest(string code, string label)
    {
        Code = code;
        Label = label;
    }

    public string Code { get; init; } = null!;
    public string Label { get; init; } = null!;
}
