using Microsoft.AspNetCore.Components;

namespace ComptaClub.Tests;

internal class AccountingTestNavigationManager : NavigationManager
{
    public AccountingTestNavigationManager()
    {
        Initialize("http://localhost/", "http://localhost/");
    }

    protected override void NavigateToCore(string uri, bool forceLoad)
    {
        Uri = ToAbsoluteUri(uri).ToString();
    }
}
