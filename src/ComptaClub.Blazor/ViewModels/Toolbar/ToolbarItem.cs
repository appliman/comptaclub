namespace ComptaClub.Blazor.ViewModels.Toolbar;

public abstract class ToolbarItem
{
	public string Name { get; set; }
	public string IconName { get; set; }
	public string Text { get; set; }
	public string Title { get; set; }
	public string Description { get; set; }
	public bool ReloadList { get; set; } = true;
	public object Entity { get; set; }
	public int SelectedItemCount { get; set; }
	public bool DisplayLoader { get; set; } = true;
}