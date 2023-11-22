using ComptaClub.Contracts.Models;

namespace ComptaClub.Blazor.ViewModels;

public class FilterInfo
{
    public FilterInfo(IListFilter filter)
    {
        Filter = filter;
    }

    public string Title { get; set; } = null!;
    public IListFilter Filter { get; set; }
    public Dictionary<string, object> ExtraDatas { get; set; } = new();

}