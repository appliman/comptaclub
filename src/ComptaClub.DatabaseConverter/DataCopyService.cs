using ComptaClub.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace ComptaClub.DatabaseConverter;

internal sealed class DataCopyService
{
    public async Task<IReadOnlyList<TableCopyResult>> InspectSourceAsync(
        ComptaClubDbContext source,
        CancellationToken cancellationToken)
    {
        var mappedTables = source.Model.GetEntityTypes()
            .Select(entity => entity.GetTableName())
            .Where(name => name is not null)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var catalogTables = TableCatalog.Tables.Select(table => table.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (!mappedTables.SetEquals(catalogTables))
        {
            throw new InvalidOperationException("Le catalogue du convertisseur ne correspond plus aux tables du modèle ComptaClub.");
        }

        var tables = new List<TableCopyResult>(TableCatalog.Tables.Count);
        foreach (var table in TableCatalog.Tables)
        {
            try
            {
                await table.ValidateAsync(source, cancellationToken);
                tables.Add(new TableCopyResult(table.Name, await table.CountAsync(source, cancellationToken)));
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"La table source {table.Name} est absente ou incompatible : {ex.Message}", ex);
            }
        }

        return tables;
    }

    public async Task EnsureDestinationEmptyAsync(
        ComptaClubDbContext destination,
        CancellationToken cancellationToken)
    {
        foreach (var table in TableCatalog.Tables)
        {
            try
            {
                await table.ValidateAsync(destination, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"Le schéma SQL Server de {table.Name} est incompatible : {ex.Message}", ex);
            }

            if (await table.CountAsync(destination, cancellationToken) != 0)
            {
                throw new InvalidOperationException($"La table SQL Server {table.Name} contient déjà des données.");
            }
        }
    }

    public async Task CopyAsync(
        ComptaClubDbContext source,
        ComptaClubDbContext destination,
        IReadOnlyList<TableCopyResult> sourceTables,
        bool destinationIsSqlServer,
        CancellationToken cancellationToken)
    {
        await destination.Database.OpenConnectionAsync(cancellationToken);
        await using var transaction = await destination.Database.BeginTransactionAsync(cancellationToken);
        var progress = new TableProgress();

        for (var index = 0; index < TableCatalog.Tables.Count; index++)
        {
            var table = TableCatalog.Tables[index];
            var expected = sourceTables[index];
            if (table.Name != expected.Name)
            {
                throw new InvalidOperationException("Le catalogue des tables a changé pendant la conversion.");
            }

            if (destinationIsSqlServer && table.HasIdentityKey)
            {
                await destination.Database.ExecuteSqlRawAsync(
                    "SET IDENTITY_INSERT [dbo].[DataProtectionKeys] ON", cancellationToken);
            }

            try
            {
                await table.CopyAsync(source, destination, expected.RowCount, progress, cancellationToken);
            }
            finally
            {
                if (destinationIsSqlServer && table.HasIdentityKey)
                {
                    await destination.Database.ExecuteSqlRawAsync(
                        "SET IDENTITY_INSERT [dbo].[DataProtectionKeys] OFF", CancellationToken.None);
                }
            }
        }

        foreach (var table in TableCatalog.Tables)
        {
            var expected = sourceTables.First(result => result.Name == table.Name).RowCount;
            var actual = await table.CountAsync(destination, cancellationToken);
            if (actual != expected)
            {
                throw new InvalidOperationException(
                    $"Vérification impossible pour {table.Name} : {expected} lignes attendues, {actual} trouvées.");
            }
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
