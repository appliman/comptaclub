using ComptaClub.Blazor.ViewModels;
using ComptaClub.Models;

using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;

namespace ComptaClub.Blazor.Services;

public class ListFilterQueryStringParametersService
{
    private readonly IMemoryCache _cacheService;
    private readonly NavigationManager _navigationManager;

    public ListFilterQueryStringParametersService(IMemoryCache cacheService
        , NavigationManager navigationManager)
    {
        _cacheService = cacheService;
        _navigationManager = navigationManager;
    }

    public void AddFilterToQueryString(FilterInfo filterInfo, string? newTargetPath = null)
    {
        if (filterInfo == null || filterInfo.Filter == null)
        {
            return;
        }

        var guid = Guid.NewGuid();
        var key = $"AdminListFilterInfo_{guid}";

        _cacheService.Set(key, filterInfo, DateTime.Now.AddMinutes(30));

        var currentUri = _navigationManager.Uri;
        var navigateToUri = currentUri;
        if (newTargetPath != null)
        {
            var uri = new Uri(currentUri);
            if (uri.Host == "localhost")
            {
                navigateToUri = $"{uri.Scheme}://{uri.Host}:{uri.Port}{newTargetPath}";
            }
            else
            {
                navigateToUri = $"{uri.Scheme}://{uri.Host}{newTargetPath}";
            }
        }
        navigateToUri = new Uri(navigateToUri).AddOrUpdateQueryParam($"filter", $"{guid}");

        if (!navigateToUri.Equals(currentUri, StringComparison.InvariantCultureIgnoreCase))
        {
            _navigationManager.NavigateTo(navigateToUri);
        }
    }

    public IListFilter? GetFilterFromQueryString()
    {
        return GetFilterInfoFromQueryString()?.Filter;
    }

    public FilterInfo? GetFilterInfoFromQueryString()
    {
        var query = new Uri(_navigationManager.Uri).Query;

        if (QueryHelpers.ParseQuery(query).TryGetValue($"Filters", out var filterId) &&
            _cacheService.TryGetValue($"AdminListFilterInfo_{filterId}", out FilterInfo? filter))
        {
            return filter;
        }

        return null;
    }
}
