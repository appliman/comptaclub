using System.Linq.Expressions;

namespace ComptaClub.Blazor.Extensions;

public static class ViewModelsExtensions
{
    public static List<ViewModels.Account> ToFlatList(this List<ViewModels.Account> plan)
    {
        var result = new List<ViewModels.Account>();
        while (true)
        {
            var item = plan.FirstOrDefault();
            if (item == null)
            {
                break;
            }
            plan.Remove(item);
            result.Add(item);
            if (item.Children.Any())
            {
                var flat = ToFlatList(item.Children);
                result.AddRange(flat);
            }
        }
        return result;
    }

    public static List<ViewModels.SelectOption<K>> ToSelectOptionList<K,T>(this IEnumerable<T> items, Func<T, K> keySelector, Func<T, string> textSelector, Func<T, bool> selected)
    {
        var result = new List<ViewModels.SelectOption<K>>();
        foreach (var item in items)
        {
            var so = new ViewModels.SelectOption<K>(keySelector(item), textSelector(item), selected(item));
			result.Add(so);
        }
        return result;
	}
}
