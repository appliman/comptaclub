using ComptaClub.Requests;

namespace ComptaClub.Handlers;

internal class CreateMemberRequestHandler : IRequestHandler<Requests.CreateMemberRequest, Datas.MemberData>
{
    public Task<MemberData> Handle(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = new MemberData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();

        return Task.FromResult(result);
    }
}
