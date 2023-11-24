using Microsoft.AspNetCore.WebUtilities;
using System.Web;

namespace ComptaClub.Blazor.Extensions;

public static class UriExtensions
{
    public static string RemoveQueryStringByKey(this Uri uri, string paramKey)
    {
        var newQueryString = HttpUtility.ParseQueryString(uri.Query);
        newQueryString.Remove(paramKey);

        string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

        return newQueryString.Count > 0
            ? string.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
            : pagePathWithoutQueryString;
    }

    public static string AddOrUpdateQueryParam(this Uri uri, string paramKey, string paramValue)
    {
        var uriWithoutParam = uri.RemoveQueryStringByKey(paramKey);

        return QueryHelpers.AddQueryString(uriWithoutParam, paramKey, paramValue);
    }
}
