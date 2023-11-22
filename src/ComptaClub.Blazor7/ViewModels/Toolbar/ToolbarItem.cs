namespace ComptaClub.Blazor.ViewModels.Toolbar;

public abstract class ToolbarItem
{
	public string Name { get; set; } = null!;
	public string IconName { get; set; } = null!;
    public string Text { get; set; } = null!; 
	public string Title { get; set; } = null!; 
	public string Description { get; set; } = null!;
	public bool ReloadList { get; set; } = true;
	public object? Entity { get; set; }
	public int SelectedItemCount { get; set; }
	public bool DisplayLoader { get; set; } = true;
	public Func<bool> IsDisable { get; set; } = () => false;
	public bool Visible { get; set; } = true;
	public Func<bool> IsVisible { get; set; } = () => true;
}