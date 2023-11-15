namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarBarDropdown : ToolbarItem
{
	public string DropdownId { get; set; } = null!;
	public List<ToolbarBarDropdownItem> Items { get; set; } = new();
}
