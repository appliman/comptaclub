using ComptaClub.Mail;

namespace ComptaClub.Backups;

public static class DatabaseBackupServiceExtensions
{
    public static IServiceCollection AddComptaClubDatabaseBackups(this IServiceCollection services)
    {
        services.AddSingleton<DatabaseBackupEmailSender>();
        services.AddSingleton<DatabaseArchiveStore>();
        services.AddHostedService(provider => provider.GetRequiredService<DatabaseArchiveStore>());
        return services;
    }
}
