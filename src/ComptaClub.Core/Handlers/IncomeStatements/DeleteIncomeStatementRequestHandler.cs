using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests.IncomeStatements;
using ComptaClub.Results;

namespace ComptaClub.Handlers.IncomeStatements;
internal class DeleteIncomeStatementRequestHandler : IRequestHandler<DeleteIncomeStatementRequest, CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public DeleteIncomeStatementRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(DeleteIncomeStatementRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var incomeStatement = await db.IncomeStatements.FirstOrDefaultAsync(i => i.Id == request.IncomeStatementId, cancellationToken);
        if (incomeStatement == null)
        {
            return CommandResult.CreateInvalidResult("Ce compte de résultat n'existe pas");
        }

        db.IncomeStatements.Remove(incomeStatement);

        var items = await db.IncomeStatementItems.Where(i => i.IncomeStatementId == request.IncomeStatementId).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            db.IncomeStatementItems.Remove(item);
        }

        using var transaction = db.Database.BeginTransaction();
        try
        {
            var changeCount = await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new CommandResult()
            {
                ChangeCount = changeCount,
                HasError = false
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return CommandResult.CreateInvalidResult(ex.Message);
        }
    }
}