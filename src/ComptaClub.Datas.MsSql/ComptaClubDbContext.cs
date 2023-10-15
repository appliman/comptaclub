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

    public DbSet<AccountData> Accounts { get; set; }
    public DbSet<BankData> Banks { get; set; }
    public DbSet<DocumentData> Documents { get; set; }
    public DbSet<DocumentContentData> DocumentsContents { get; set; }
    public DbSet<DocumentByEntityData> DocumentsByEntities { get; set; }
    public DbSet<EntryData> Entries { get; set; }
    public DbSet<ExerciceData> Exercices { get; set; }
    public DbSet<MemberData> Members { get; set; }
    public DbSet<RoleByUserData> RolesByUsers { get; set; }
    public DbSet<UserData> Users { get; set; }
    public DbSet<AssociatedMemberListByEntryData> AssociatedMemberListByEntries { get; set; }
    public DbSet<IncomeStatementData> IncomeStatements { get; set; }
    public DbSet<IncomeStatementItemData> IncomeStatementItems { get; set; }
}
