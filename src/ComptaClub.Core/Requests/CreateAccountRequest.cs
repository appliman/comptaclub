
namespace ComptaClub.Requests;

public record CreateAccountRequest : IRequest<Datas.AccountData>
{
    public CreateAccountRequest() 
    {
        this.Code = "A completer";
        this.Label = "A completer";
        this.Direction = AccountDirection.Credit;
    }

    public CreateAccountRequest(string code, string label, AccountDirection accountDirection)
    {
        this.Code = code;
        this.Label  = label;
        this.Direction = accountDirection; 
    }

    public string Code { get; init; } = null!;
    public string Label { get; init; } = null!;
    public AccountDirection Direction { get;init; }
    public Guid? ParentId { get; set; }
}
