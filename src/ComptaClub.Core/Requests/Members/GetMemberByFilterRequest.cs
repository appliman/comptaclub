namespace ComptaClub.Requests.Members;

public class GetMemberByFilterRequest : IRequest<MemberData?>
{
    public GetMemberByFilterRequest(Action<MemberListFilter> filter)
    {
        Filter = filter;
    }

    public Action<MemberListFilter> Filter { get; set; }
}
