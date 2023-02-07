using System.Linq.Expressions;

using AutoMapper;

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

    public static List<ViewModels.Account> MapToAccountList(this IEnumerable<Datas.AccountData> list, AutoMapper.IMapper mapper)
    {
        var result = new List<ViewModels.Account>();
        foreach (var item in list)
        {
            var account = mapper.Map<ViewModels.Account>(item);
            account.Children = MapToAccountList(item.Children, mapper);
            result.Add(account);
        }
        return result;
    }

    public static ViewModels.Account? DeepFirstOrDefault(this IEnumerable<ViewModels.Account> list, Func<ViewModels.Account, bool> predicate)
    {
        var result = list.FirstOrDefault(predicate);
        if (result == null)
        {
            foreach (var item in list)
            {
                if (item.Children.Any())
                {
                    result = item.Children.DeepFirstOrDefault(predicate);
                    if (result != null)
                    {
                        break;
                    }
                }
            }
        }
        return result;
    }

    public static decimal DeepSum(this ViewModels.Account account, Expression<Func<ViewModels.Account, decimal>> expression)
    {
        var member = expression.Compile();
        var result = member.Invoke(account);
        foreach (var subAccount in account.Children)
        {
            result = result + subAccount.DeepSum(expression);
        }
        return result;
    }
}
