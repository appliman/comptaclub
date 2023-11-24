namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarButton : ToolbarItem
{
	public Func<Task> OnClick { get; set; } = default!;
	public bool NeedConfirmation { get; set; } = false;
	public string ConfirmationMessage { get; set; } = null!;
	public bool Disabled { get; set; } = false;
}

