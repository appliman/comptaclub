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

    public static long DeepSum(this ViewModels.Account account, Expression<Func<ViewModels.Account, long>> expression)
    {
        var member = expression.Compile();
        var result = member.Invoke(account);
        foreach (var subAccount in account.Children)
        {
            result = result + subAccount.DeepSum(expression);
        }
        return result;
    }

    public static string ToFileSize(this long fileSize)
    {
        var units = new[] { "o", "Ko", "Mo", "Go", "To" };
        var index = 0;
        var size = Convert.ToDouble(fileSize);
        while (size > 1024)
        {
            size /= 1024d;
            index++;
        }
        return string.Format("{0:F2} {1}", size, units[index]);
    }

	public static void Levelize(this IEnumerable<ViewModels.IncomeStatementItem> list, int level = 0)
	{
		var unlevelizedList = list.Where(i => i.Level == -1);
		foreach (var item in unlevelizedList)
		{
			var parent = list.SingleOrDefault(i => i.Id == item.ParentIncomeStatementItemId
													&& i.Level > -1);
			if (parent != null)
			{
				item.Level = parent.Level + 1;
			}
		}
		if (unlevelizedList.Any())
		{
			list.Levelize(level++);
		}
	}

	public static void Hierarchize(this IList<ViewModels.IncomeStatementItem> list)
	{
		var flatList = list.OrderBy(i => i.Level).ToList();
		list.Clear();
		while (true)
		{
			var first = flatList.FirstOrDefault();
			if (first == null)
			{
				break;
			}

			// Recherche du parent
			if (!first.ParentIncomeStatementItemId.HasValue)
			{
				if (!list.Contains(first))
				{
					list.Add(first);
				}
			}
			else
			{
				var parent = list.DeepFindParent(first.ParentIncomeStatementItemId.Value);
				if (parent != null)
				{
					if (!parent.Children.Contains(first))
					{
						first.Level = parent.Level + 1;
						parent.Children.Add(first);
					}
				}
			}

			flatList.Remove(first);
		}
	}

	public static ViewModels.IncomeStatementItem? DeepFindParent(this IList<ViewModels.IncomeStatementItem> list, Guid parentId)
	{
		foreach (var item in list)
		{
			if (item.Id == parentId)
			{
				return item;
			}
			if (item.Children.Any())
			{
				var result = item.Children.DeepFindParent(parentId);
				if (result != null)
				{
					return result;
				}
			}
		}
		return null;
	}


}
