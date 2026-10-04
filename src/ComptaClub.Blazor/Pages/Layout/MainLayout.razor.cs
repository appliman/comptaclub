using ComptaClub.Blazor.Services.Documentation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using SuperBlazorComponents.Services;

namespace ComptaClub.Blazor.Pages.Layout;

public partial class MainLayout
{
    [CascadingParameter]
    Task<AuthenticationState> AuthenticationState { get; set; } = default!;

    [Inject]
    public NotificationService NotificationService { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Inject]
    public IWikiDocumentationService WikiDocumentationService { get; set; } = default!;

    bool loaderVisible = false;
    ViewModels.User? user = new();
    string version = $"{typeof(Program).Assembly.GetName()?.Version}";
    IReadOnlyList<WikiMenuItem> docMenuItems = [];

    protected override async Task OnInitializedAsync()
    {
        user = (await AuthenticationState).User.GetUserInfos() ?? new();
        docMenuItems = await WikiDocumentationService.GetMenuItems();
    }

    public ViewModels.User GetCurrentUser()
    {
        return user!;
    }
}
