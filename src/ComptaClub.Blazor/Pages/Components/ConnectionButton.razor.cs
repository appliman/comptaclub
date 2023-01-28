namespace ComptaClub.Blazor.Pages.Components
{
    public partial class ConnectionButton
    {
        [CascadingParameter]
        Task<AuthenticationState> AuthenticationState { get; set; } = default !;
        
        [Inject]
        NavigationManager NavigationManager { get; set; } = default !;

        bool displayButton = false;

        protected override async Task OnInitializedAsync()
        {
            var principal = (await AuthenticationState).User;
            if (principal != null
                && principal.Identity != null
                && principal.Identity.IsAuthenticated
                )
            {
                displayButton = true;
            }
        }

        void Logout()
        {
            NavigationManager.NavigateTo("/?logout=true", true);
        }
    }
}