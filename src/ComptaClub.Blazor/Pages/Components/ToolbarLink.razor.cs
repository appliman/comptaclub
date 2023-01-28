namespace ComptaClub.Blazor.Pages.Components;

public partial class ToolbarLink
{
	[Parameter]
	public ViewModels.Toolbar.ToolbarLink ToolbarItem { get; set; } = default!;

}