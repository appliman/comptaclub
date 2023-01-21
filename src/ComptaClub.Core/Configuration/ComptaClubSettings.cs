namespace ComptaClub.Configuration
{
    public class ComptaClubSettings
    {
        public readonly static Guid ImportAccount = new Guid("6c7f8ba2-8b49-4ec0-b96e-63a073bae5d6");

        public string ApplicationName { get; set; } = "comptaclubapp";

        // Azure Storage
        public string DataProtectionFileName { get; set; } = "key-compta-club.xml";
        public string AzureStorageAccountName { get; set; } = "comptaclub";
        public string AzureStorageAccountKey { get; private set; } = null!;
        public string SetAzureStorageAccountKey(string key) => AzureStorageAccountKey = key;
        public string AzureStorageWebAppDataProtectionContainerName { get; set; } = "compta-club-webappdataprotection";
        public string AzureStorageDocumentsContainerName { get; set; } = "compta-club-documents";
        public string CookieName { get; set; } = "comptaclub";

        public string? AdminUserEmail { get; set; }

        public string AzureStorageConnectionString {get; private set; } = null!;
        public void SetAzureStorageConnectionString(string azureStorageConnectionString) => this.AzureStorageConnectionString = azureStorageConnectionString;


        public string SqlConnectionString { get; private set; } = null!;
        public void SetSqlConnectionString(string sqlConnectionString) => this.SqlConnectionString = sqlConnectionString;

        // Keyvault

        public string KeyVaultTenantId { get; set; } = null!;
        public string KeyVaultClientId { get; set; } = null!;
        public string KeyVaultClientSecret { get; set; } = null!;
        public string KeyVaultName { get; set; } = null!;

        public System.Globalization.CultureInfo CultureInfo { get; init; } 
            = new System.Globalization.CultureInfo("fr-FR");

        public System.TimeZoneInfo TimeZoneInfo { get; set; } = TimeZoneInfo.Local;
    }
}
