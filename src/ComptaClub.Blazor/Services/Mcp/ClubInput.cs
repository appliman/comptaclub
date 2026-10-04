namespace ComptaClub.Blazor.Services.Mcp;

public sealed record ClubInput(string Name, string? Object = null, string? Address = null, string? SirenNumber = null,
    string? SiretNumber = null, string? Rna = null, string? Email = null, string? WebSite = null,
    string? PhoneNumber = null, string? ContactName = null, string? LogoBase64 = null, string? LogoContentType = null);
