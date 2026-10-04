namespace ComptaClub.Blazor.Services.Documentation;

public interface IWikiDocumentationService
{
    Task<IReadOnlyList<WikiMenuItem>> GetMenuItems(CancellationToken cancellationToken = default);

    Task<WikiPageContent?> GetPage(string slug, CancellationToken cancellationToken = default);

    void InvalidateCache();
}
