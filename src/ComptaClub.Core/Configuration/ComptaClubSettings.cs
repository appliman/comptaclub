namespace ComptaClub.Configuration
{
    public class ComptaClubSettings
    {
        // Azure Storage
        public string DataProtectionFileName { get; set; } = "key-compta-club.xml";
        public string AzureStorageAccountName { get; set; } = null!;
        public string AzureStorageWebAppDataProtectionContainerName { get; set; } = "compta-club-webappdataprotection";
        public string AzureStorageDocumentsContainerName { get; set; } = "compta-club-documents";


        public string AzureStorageConnectionString {get; private set; } = null!;
        public void SetAzureStorageConnectionString(string azureStorageConnectionString) => this.AzureStorageConnectionString = azureStorageConnectionString;
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
