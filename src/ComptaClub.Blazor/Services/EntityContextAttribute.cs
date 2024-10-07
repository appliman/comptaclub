namespace ComptaClub.Blazor.Services;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class EntityContextAttribute : Attribute
{
    public MetaEntity MetaEntity { get; set; } = new();
    // Indiquer le chemin ou les chemins séparé par des virgules
    // ou une expression régulière pour exclure des routes
    public string? ExcludeRouteLists { get; set; }
    public string Title { get; set; } = null!;
    public string? Icon { get; set; }
    public string? IconColor { get; set; }
    public int DefaultPosition { get; set; }
    public ContextLocation ContextLocation { get; set; } = ContextLocation.Bottom;
}

public enum ContextLocation
{
    Bottom = 1,
    Right= 2
}

