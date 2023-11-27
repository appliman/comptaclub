using ComptaClub.Contracts.Models.Members;

namespace ComptaClub.Handlers.Members;

internal class CreateMemberRequestHandler : IRequestHandler<CreateMemberRequest, MemberData>
{
    public Task<MemberData> Handle(CreateMemberRequest request, CancellationToken cancellationToken)
    {
        var result = new MemberData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();
        result.LastUpdate = DateTime.Today.ToDayId();
        result.State = Datas.Enums.MemberState.Active;

        return Task.FromResult(result);
    }
}
