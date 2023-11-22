namespace ComptaClub.Contracts.Models.Users;

public record CreateUserRequest : IRequest<UserData>
{
    public CreateUserRequest()
    {

    }

    public CreateUserRequest(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
}
