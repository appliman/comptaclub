using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.ViewModels.Toolbar;

namespace ComptaClub.Blazor.Pages.Shared
{
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
        RadzenBody? body;
        Toolbar? toolbar;
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

        public Toolbar AddToolbarItem(ViewModels.Toolbar.ToolbarItem item)
        {
            toolbar!.InitializeToolbar();
            toolbar.AddItem(item);
            return toolbar;
        }
    }
}