namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarBarDropdownItem : ToolbarItem
{
	public string ButtonText { get; set; } = null!; 
	public bool Disabled { get; set; }
	public Func<Task> OnClick { get; set; } = () => Task.CompletedTask;
}