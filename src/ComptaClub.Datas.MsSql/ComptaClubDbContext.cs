using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ComptaClub.Datas;

public class ComptaClubDbContext : DbContext
{
    private readonly DbConfiguration _dbConfiguration;

    public ComptaClubDbContext(DbConfiguration dbConfiguration)
    {
        this._dbConfiguration = dbConfiguration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!string.IsNullOrEmpty(_dbConfiguration.EnvironmentName)
            && _dbConfiguration.EnvironmentName.IndexOf("prod", StringComparison.InvariantCultureIgnoreCase) == -1)
        {
            optionsBuilder.EnableDetailedErrors(true);
            optionsBuilder.EnableSensitiveDataLogging(true);
        }
        optionsBuilder.EnableServiceProviderCaching(true);
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        optionsBuilder.AddInterceptors(new VarcharOptimizationInterceptor());
        optionsBuilder.UseSqlServer(_dbConfiguration.ConnectionString);
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Bank> Banks { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Entry> Entries { get; set; }
    public DbSet<Exercice> Exercices { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<RoleByUser> RolesByUsers { get; set; }
    public DbSet<User> Users { get; set; }
}
