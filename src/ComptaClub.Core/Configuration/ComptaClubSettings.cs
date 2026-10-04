namespace ComptaClub.Configuration;

public class ComptaClubSettings
{
	public readonly static Guid ImportAccount = new Guid("6c7f8ba2-8b49-4ec0-b96e-63a073bae5d6");

	public string ApplicationName { get; set; } = "comptaclubapp";

	public string? AdminUserEmail { get; set; }
	public string ContactEmailAdress { get; set; } = null!;
	public string ContactName { get; set; } = null!;
	// Smtp
	public string SmtpHost { get; set; } = null!;
	public int SmtpPort { get; set; }
	public string SmtpUserName { get; set; } = null!;
	public string SmtpPassword { get; set; } = null!;
	public bool SmtpEnableSsl { get; set; } = true;

	public string DatabaseProvider { get; set; } = "Sqlite";
	public string ConnectionString { get; set; } = null!;

    public string TempFolder { get; set; } = Path.Combine(Path.GetTempPath(), "ComptaClub");

	public System.Globalization.CultureInfo CultureInfo { get; init; }
		= new System.Globalization.CultureInfo("fr-FR");

	public System.TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;

	public string OtlpEndpoint { get; set; } = null!;
}
