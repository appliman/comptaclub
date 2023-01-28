namespace ComptaClub.Handlers;

internal class CreateUserRequestHandler : IRequestHandler<Requests.CreateUserRequest, Datas.UserData>
{
    public Task<Datas.UserData> Handle(Requests.CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = new Datas.UserData();
        result.Id = Guid.NewGuid();
        result.Name = request.Name;
        result.Email= request.Email;

        return Task.FromResult(result);
    }
}
