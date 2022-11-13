namespace ComptaClub.Configuration
{
    public class ComptaClubSettings
    {
        // Azure Storage
        public string DataProtectionFileName { get; set; } = "key-compta-club.xml";
        public string AzureStorageAccountName { get; set; } = null!;
        public string AzureStorageWebAppDataProtectionContainerName { get; set; } = "compta-club-webappdataprotection";
        public string AzureStorageDocumentsContainerName { get; set; } = "compta-club-documents";


        public string AzureStorageConnectionString {get; set; } = null!;
        // Keyvault
        public string VaultAppId { get; set; } = null!;
        public string VaultCertificatePath { get; set; } = null!;
        public string VaultTenantId { get; set; } = null!;  
        public string VaultUrl { get; set; } = null!;
    }
}
