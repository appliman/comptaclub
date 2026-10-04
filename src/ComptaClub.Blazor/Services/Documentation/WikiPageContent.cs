namespace ComptaClub.Blazor.Services.Documentation;

public sealed record WikiPageContent(
    string Title,
    string Slug,
    string Markdown,
    string Html,
    bool FromFallback = false);
