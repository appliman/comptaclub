namespace ComptaClub.Contracts.Models.Users;

public class UserListFilterOptions
{
	public DeletedState DeletedState { get; set; } = DeletedState.Undeleted;
}
