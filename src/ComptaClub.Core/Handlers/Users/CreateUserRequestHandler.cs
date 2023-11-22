using ComptaClub.Contracts.Models.Users;

namespace ComptaClub.Handlers.Users;

internal class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, UserData>
{
    public Task<UserData> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = new UserData();
        result.Id = Guid.NewGuid();
        result.Name = request.Name;
        result.Email = request.Email;

        return Task.FromResult(result);
    }
}
