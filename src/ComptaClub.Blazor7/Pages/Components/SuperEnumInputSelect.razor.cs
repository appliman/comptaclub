namespace ComptaClub.Blazor.Pages.Components;

public partial class SuperEnumInputSelect<E>
	where E : struct
{
	[Parameter]
	public string Label { get; set; } = null!;

	[Parameter]
	public E Value { get; set; } = default!;

	[Parameter]
	public EventCallback<E> ValueChanged { get; set; }

	[Parameter]
	public System.Linq.Expressions.Expression<Func<E>> ValueExpression { get; set; } = default!;

	[Parameter]
	public bool Required { get; set; }

	[Parameter]
	public RenderFragment ChildContent { get; set; } = default!;
}