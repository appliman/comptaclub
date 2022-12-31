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
}
