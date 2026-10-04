using ComptaClub.Datas;
using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Datas.MsSql;

public sealed class McpSchemaDbContext(DbContextOptions<McpSchemaDbContext> options) : DbContext(options)
{
    public DbSet<McpApiKeyData> McpApiKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new McpApiKeyConfiguration());
    }
}
