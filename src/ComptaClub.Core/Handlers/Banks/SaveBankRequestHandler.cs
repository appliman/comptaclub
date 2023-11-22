using ComptaClub.Contracts.Models;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers.Banks;

internal class SaveBankRequestHandler : SaveRequestHandlerBase, IRequestHandler<SaveEntityRequest<BankData>, PersistResult>
{
    private readonly IValidator<BankData> _validator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public SaveBankRequestHandler(
        IValidator<BankData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveBankRequestHandler> logger)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PersistResult> Handle(SaveEntityRequest<BankData> request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request.Entity);
        if (!result.IsValid)
        {
            return result.ToPersistResult()!;
        }

        var db = await _dbContextFactory.CreateDbContextAsync();
        var bankCount = await db.Banks.CountAsync();

        // S'il n'y a aucun exercice, le nouveau doit etre actif
        if (bankCount == 0)
        {
            request.Entity.Active = true;
        }

        var saveResult = await SaveEntity<BankData>(request.Entity, cancellationToken);
        if (!saveResult.HasError)
        {
            bankCount = await db.Banks.CountAsync();
            // Si c'est le seul exercice, il doit etre actif impérativement
            if (bankCount == 1
                && !request.Entity.Active)
            {
                request.Entity.Active = true;
                saveResult = await SaveEntity<BankData>(request.Entity, cancellationToken);
            }
        }

        return saveResult;
    }
}
