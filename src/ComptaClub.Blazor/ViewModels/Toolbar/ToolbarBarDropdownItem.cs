namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarBarDropdownItem : ToolbarItem
{
	public string ButtonText { get; set; }
	public bool Disabled { get; set; }
	public Func<Task> Action { get; set; } = () => Task.CompletedTask;
}