using ComptaClub.Blazor.Services;

namespace ComptaClub.Blazor.ViewModels;

public class ContextInfo
{
	public MetaEntity MetaEntity { get; set; }
	public Type ComponentType { get; set; } = default!;
	public string Title { get; set; } = null!;
	public string? ExcludeRoutes { get; set; }
	public string? Icon { get; set; }
	public string? IconColor { get; set; }
	public object? Parent { get; set; }
	public IMetaEntity SelectedEntity { get; set; } = default!;
	public int DefaultPosition { get; set; }
	public ContextLocation ContextLocation { get; set; }
	public EventCallback<IMetaEntity> ReloadBodyPanel { get; set; } = default!;
}