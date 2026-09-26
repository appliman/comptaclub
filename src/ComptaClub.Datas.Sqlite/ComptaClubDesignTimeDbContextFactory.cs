using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ComptaClub.Datas.Sqlite;

public sealed class ComptaClubDesignTimeDbContextFactory : IDesignTimeDbContextFactory<ComptaClubDbContext>
{
    public ComptaClubDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ComptaClubDbContext>()
            .UseSqlite("Data Source=comptaclub-design-time.db", sqlite =>
                sqlite.MigrationsAssembly(typeof(ComptaClubDesignTimeDbContextFactory).Assembly.FullName))
            .Options;
        return new ComptaClubDbContext(options);
    }
}
