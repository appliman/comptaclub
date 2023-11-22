using ComptaClub.Blazor.Extensions;

namespace ComptaClub.Blazor.Pages.Components;

public partial class UserInfo
{
    [CascadingParameter]
    Task<AuthenticationState> AuthenticationState { get; set; } = default!;

    ViewModels.User? user = new();

    protected override async Task OnInitializedAsync()
    {
        user = (await AuthenticationState).User.GetUserInfos() ?? new();
    }
}