using ComptaClub.Datas;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.EntityFramework;

public class ComptaClubDbContext(DbContextOptions<ComptaClubDbContext> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    public DbSet<AccountData> Accounts { get; set; } = null!;
    public DbSet<BankData> Banks { get; set; } = null!;
    public DbSet<DocumentData> Documents { get; set; } = null!;
    public DbSet<DocumentContentData> DocumentsContents { get; set; } = null!;
    public DbSet<DocumentByEntityData> DocumentsByEntities { get; set; } = null!;
    public DbSet<EntryData> Entries { get; set; } = null!;
    public DbSet<ExerciceData> Exercices { get; set; } = null!;
    public DbSet<MemberData> Members { get; set; } = null!;
    public DbSet<RoleByUserData> RolesByUsers { get; set; } = null!;
    public DbSet<UserData> Users { get; set; } = null!;
    public DbSet<AssociatedMemberListByEntryData> AssociatedMemberListByEntries { get; set; } = null!;
    public DbSet<IncomeStatementData> IncomeStatements { get; set; } = null!;
    public DbSet<IncomeStatementItemData> IncomeStatementItems { get; set; } = null!;
    public DbSet<ClubData> ClubDatas { get; set; } = null!;
    public DbSet<ForecastBudgetData> ForecastBudgets { get; set; } = null!;
    public DbSet<ForecastBudgetItemData> ForecastBudgetItems { get; set; } = null!;
}
