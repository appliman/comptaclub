using System;
using System.ComponentModel;
using System.Linq.Expressions;

using ComptaClub.Models;

namespace ComptaClub.Pages.Shared.Components;

public partial class BrokenRuleMessage<TValue> : ComponentBase
{
    [Parameter]
    public Expression<Func<TValue>> For { get; set; }

    [Parameter]
    public List<BrokenRule> BrokenRuleList { get; set; } = new();

    IEnumerable<string> errors = new List<string>();

    protected override void OnParametersSet()
    {
        var memberExpression = For.Body as MemberExpression;
        if (memberExpression != null)
        {
			var memberName = memberExpression.Member.Name;
            errors = BrokenRuleList.Where(i => i.PropertyName == memberName && i.Severity == Severity.Error)
                    .SelectMany(i => i.MessageList);
			if (errors.Any())
            {
				StateHasChanged();
			}
		}
    }
}