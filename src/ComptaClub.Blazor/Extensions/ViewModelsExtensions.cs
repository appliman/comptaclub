namespace ComptaClub.Blazor.Extensions;

public static class ViewModelsExtensions
{
    public static void Levelize(this IEnumerable<ViewModels.Account> list, int level = 0)
    {
        var unlevelizedList = list.Where(i => i.Level == -1);
        foreach (var item in unlevelizedList)
        {
            var parent = list.SingleOrDefault(i => i.Id == item.ParentAccountId
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

    public static void Hierarchize(this IList<ViewModels.Account> list)
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
            if (!first.ParentAccountId.HasValue)
            {
                if (!list.Contains(first))
                {
                    list.Add(first);
                }
            }
            else
            {
                var parent = list.DeepFindParent(first.ParentAccountId.Value);
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

    public static ViewModels.Account? DeepFindParent(this IList<ViewModels.Account> list, Guid parentId)
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

    public static List<ViewModels.Account> ToFlatList(List<ViewModels.Account> plan)
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
}
