using System.ComponentModel.DataAnnotations;

namespace ComptaClub.Blazor.ViewModels;

public sealed class ApiKeyFormModel
{
    [Required(ErrorMessage = "Le nom est requis.")]
    [MaxLength(100, ErrorMessage = "Le nom ne doit pas dépasser 100 caractères.")]
    public string Name { get; set; } = string.Empty;
    public string ExpirationUtc { get; set; } = string.Empty;

    public DateTime? GetExpiration()
    {
        if (string.IsNullOrWhiteSpace(ExpirationUtc))
        {
            return null;
        }
        if (!DateTime.TryParseExact(ExpirationUtc, ["yyyy-MM-ddTHH:mm", "yyyy-MM-ddTHH:mm:ss"],
            System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var _date))
        {
            throw new ArgumentException("La date d’expiration est invalide.");
        }
        return DateTime.SpecifyKind(_date, DateTimeKind.Utc);
    }
}
