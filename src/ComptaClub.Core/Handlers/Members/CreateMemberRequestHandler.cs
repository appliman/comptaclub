using ComptaClub.Requests.Members;

namespace ComptaClub.Handlers.Members;

internal class CreateMemberRequestHandler : IRequestHandler<CreateMemberRequest, MemberData>
{
    public Task<MemberData> Handle(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = new MemberData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();

        return Task.FromResult(result);
    }
}
