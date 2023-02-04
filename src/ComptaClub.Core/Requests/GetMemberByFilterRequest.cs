namespace ComptaClub.Requests;

public class GetMemberByFilterRequest : IRequest<Datas.MemberData?>
{
	public GetMemberByFilterRequest(Action<MemberListFilter> filter)
	{
		this.Filter = filter;
	}

	public Action<MemberListFilter> Filter { get; set; }
}
