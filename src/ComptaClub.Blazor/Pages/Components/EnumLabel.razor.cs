using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ComptaClub.Blazor.Pages.Components;

public partial class EnumLabel<E> : ComponentBase
{
    [Parameter]
    public E Value { get; set; } = default!;

    [Parameter]
    public EventCallback<E> ValueChanged { get; set; }

    [Parameter]
    public System.Linq.Expressions.Expression<Func<E>> ValueExpression { get; set; } = default !;
    string? enumDescription;
    string? enumName;
    protected override void OnParametersSet()
    {
        var fi = Value.GetType().GetField($"{Value}")!;
        var attr = fi.GetCustomAttribute<DisplayAttribute>();
        if (attr != null)
        {
            enumName = attr.Name;
            enumDescription = attr.Description;
        }
        else
        {
            enumName = $"{Value}";
        }
    }
}