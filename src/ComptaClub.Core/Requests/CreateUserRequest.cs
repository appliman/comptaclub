namespace ComptaClub.Requests;

public record CreateUserRequest : IRequest<Datas.UserData>
{
	public CreateUserRequest(string name, string email)
	{
		this.Name = name;
		this.Email= email;
	}

	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
}
