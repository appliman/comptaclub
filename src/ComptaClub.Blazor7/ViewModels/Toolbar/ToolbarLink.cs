namespace ComptaClub.Blazor.ViewModels.Toolbar;

public class ToolbarLink : ToolbarItem
{
	public string Url { get; set; } = string.Empty;
	public string Target { get; set; } = "_self";
}

