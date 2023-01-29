using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ComptaClub.Blazor.Pages.Components;

public partial class SuperNullableEnumDropdownFilterColumn<E> : ComponentBase
{
	class SelectableItem
	{
		public int? Value { get; set; }
		public string? Name { get; set; }
		public bool IsSelected { get; set; }
	}

	[Parameter]
	public string Label { get; set; } = null!;
	[Parameter]
	public E Value { get; set; } = default!;
    [Parameter]
	public EventCallback<E> ValueChanged { get; set; }
	[Parameter]
	public System.Linq.Expressions.Expression<Func<E>> ValueExpression { get; set; } = default!;

	[Parameter]
	public string NullItemLabel { get; set; } = "Tous";
	[Parameter]
	public EventCallback ApplyFilter { get; set; } = default!;

	protected override void OnParametersSet()
	{
		var type = Nullable.GetUnderlyingType(typeof(E));
		if (type == null)
		{
			throw new ArgumentException("Enum parameter Value must be nullable");
		}
    }

    async Task SelectChanged(ChangeEventArgs args)
	{
        var type = Nullable.GetUnderlyingType(typeof(E))!;
        Enum.TryParse(type, $"{args.Value}", out object v);
		if (v != null)
		{
			Value = (E)v;
		}
		else
		{
			Value = default(E);
		}
		if (ValueChanged.HasDelegate)
		{
			await ValueChanged.InvokeAsync(Value);
		}
		if (ApplyFilter.HasDelegate)
		{
			await ApplyFilter.InvokeAsync();
		}
	}

	List<SelectableItem> GetValues()
	{
		var result = new List<SelectableItem>();
		result.Insert(0, new SelectableItem()
		{
			Value = null,
			Name = NullItemLabel
		});
		var type = Nullable.GetUnderlyingType(typeof(E));
		var values = Enum.GetValues(type);
		foreach (var value in values)
		{
			var fi = value.GetType().GetField($"{value}");
			var attr = fi.GetCustomAttribute<DisplayAttribute>();
			var isSelected = $"{Value}" == $"{value}";
			if (attr != null)
			{
				result.Add(new SelectableItem { Value = (int)value, Name = attr.Name ?? $"{value}", IsSelected = isSelected });
			}
			else
			{
				result.Add(new SelectableItem { Value = (int)value, Name = fi!.Name, IsSelected = isSelected });
			}
		}

		return result;
	}
}