namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarBarDropdown : ToolbarItem
{
	public string DropdownId { get; set; }
	public List<ToolbarBarDropdownItem> Items { get; set; }
}
