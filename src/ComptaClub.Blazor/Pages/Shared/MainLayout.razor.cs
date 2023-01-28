using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.ViewModels.Toolbar;

namespace ComptaClub.Blazor.Pages.Shared
{
    public partial class MainLayout
    {
        [CascadingParameter]
        Task<AuthenticationState> AuthenticationState { get; set; } = default!;

        bool sidebarExpanded = false;
        bool loaderVisible = false;
        RadzenBody? body;
        Toolbar? toolbar;
        ViewModels.User? user = new();

        protected override async Task OnInitializedAsync()
        {
            user = (await AuthenticationState).User.GetUserInfos() ?? new();
        }

        public Toolbar AddToolbarItem(ViewModels.Toolbar.ToolbarItem item)
        {
            toolbar!.InitializeToolbar();
            toolbar.AddItem(item);
            return toolbar;
        }
    }
}