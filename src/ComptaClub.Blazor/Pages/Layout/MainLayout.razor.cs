namespace ComptaClub.Blazor.Pages.Layout;

public partial class MainLayout
{
	[CascadingParameter]
	Task<AuthenticationState> AuthenticationState { get; set; } = default!;

	[Inject]
	public NotificationService NotificationService { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;


	bool sidebarExpanded = false;
	bool loaderVisible = false;
	ViewModels.User? user = new();
	string version = $"{typeof(Program).Assembly.GetName()?.Version}";

	protected override async Task OnInitializedAsync()
	{
		user = (await AuthenticationState).User.GetUserInfos() ?? new();
	}

	public ViewModels.User GetCurrentUser()
	{
		return user!;
	}
}