using ComptaClub.Blazor.Pages.Shared;

namespace ComptaClub.Blazor.Pages.Components;

public partial class Toolbar : ComponentBase
{
    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = default!;

	[Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    List<ViewModels.Toolbar.ToolbarItem> toolbarItems = new();
	string? location;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            location = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString();
            NavigationManager.LocationChanged += (s, e) =>
            {
                if (e.IsNavigationIntercepted
                   &&  e.Location != location)
                {
                    toolbarItems.Clear();
                    StateHasChanged();
                }
            };
        }
    }

    //public void ToolbarInitialized()
    //{
    //    location = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString();
    //}

    public void InitializeToolbar()
    {
        toolbarItems.Clear();
    }

	public Toolbar AddItem(ViewModels.Toolbar.ToolbarItem item)
	{
		toolbarItems.Add(item);
		return this;
	}

    public void Display()
    {
        if (NavigationManager is null)
        {
            return;
        }
        location = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString();
        StateHasChanged();
    }
}
