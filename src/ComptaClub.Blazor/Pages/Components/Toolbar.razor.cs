using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels.Toolbar;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Azure;

namespace ComptaClub.Blazor.Pages.Components;

public partial class Toolbar : ComponentBase
{
    [CascadingParameter]
    public MainLayout MainLayout { get; set; } = default!;

	[Inject]
    public NavigationManager NavigationManager { get; set; } = default!;

    List<ViewModels.Toolbar.ToolbarItem> toolbarItems = new();
	string location;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            NavigationManager.LocationChanged += (s, e) =>
            {
                if (e.Location != location)
                {
                    toolbarItems.Clear();
                    StateHasChanged();
                }
            };
        }
    }

    public void ToolbarInitialized()
    {
        location = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString();
    }

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
        location = NavigationManager.ToAbsoluteUri(NavigationManager.Uri).ToString();
        StateHasChanged();
    }
}
