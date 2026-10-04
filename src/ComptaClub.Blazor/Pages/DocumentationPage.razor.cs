using ComptaClub.Blazor.Services.Documentation;
using Microsoft.AspNetCore.Components;

namespace ComptaClub.Blazor.Pages;

public partial class DocumentationPage
{
    [Parameter]
    public string? Slug { get; set; }

    [Inject]
    public IWikiDocumentationService DocumentationService { get; set; } = default!;

    [Inject]
    public NavigationManager Navigation { get; set; } = default!;

    private WikiPageContent? _pageContent;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string? _previousSlug;

    protected override async Task OnParametersSetAsync()
    {
        var _targetSlug = string.IsNullOrWhiteSpace(Slug) ? "Home" : Slug;
        if (_targetSlug != _previousSlug)
        {
            _previousSlug = _targetSlug;
            await LoadPage();
        }
    }

    private async Task LoadPage()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            var _targetSlug = string.IsNullOrWhiteSpace(Slug) ? "Home" : Slug;
            _pageContent = await DocumentationService.GetPage(_targetSlug);

            if (_pageContent is null)
            {
                _errorMessage = $"Le document « {_targetSlug} » n'a pas pu être trouvé dans le wiki.";
            }
        }
        catch (Exception _exception)
        {
            _errorMessage = $"Une erreur est survenue lors du chargement : {_exception.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task RefreshPage()
    {
        DocumentationService.InvalidateCache();
        await LoadPage();
    }
}
