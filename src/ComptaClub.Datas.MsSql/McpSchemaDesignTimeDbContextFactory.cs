using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ComptaClub.Datas.MsSql;

public sealed class McpSchemaDesignTimeDbContextFactory : IDesignTimeDbContextFactory<McpSchemaDbContext>
{
    public McpSchemaDbContext CreateDbContext(string[] args)
    {
        var _options = new DbContextOptionsBuilder<McpSchemaDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ComptaClubDesignTime;Trusted_Connection=True;",
                sql => sql.MigrationsHistoryTable("__McpMigrationsHistory")).Options;
        return new McpSchemaDbContext(_options);
    }
}
