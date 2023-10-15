namespace ComptaClub.Blazor.Pages.Components;

public partial class SuperEnumLabel<E>
	where E : Enum
{
	[Parameter]
	public E Value { get; set; } = default!;

	[Parameter]
	public RenderFragment ChildContent { get; set; } = default!;

	string label = default!;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();
		label = Value.ToName();
	}
}
