
using ComptaClub.Contracts.Results;

namespace ComptaClub.Handlers;

internal abstract class SaveRequestHandlerBase
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly ILogger _logger;

    protected SaveRequestHandlerBase(IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveRequestHandlerBase> logger)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
    }

    public virtual async Task<PersistResult> SaveEntity<T>(Datas.IPrimaryKey model, CancellationToken cancellationToken)
        where T : class, new()
    {
        using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await SaveEntity<T>(db, model, cancellationToken);
    }

    public virtual async Task<PersistResult> SaveEntity<T>(ComptaClubDbContext db, Datas.IPrimaryKey model, CancellationToken cancellationToken)
    where T : class, new()
    {
        _logger.LogTrace("Try to save entity {Id} in table {Name}", model.Id, typeof(T).Name);

        var data = await db.Set<T>().FindAsync(model.Id, cancellationToken);

        string? error = null;

        if (data == null)
        {
            _logger.LogTrace("Try to insert new entity {Id} in table {Name}", model.Id, typeof(T).Name);
            db.Set<T>().Add((T)model);
            db.Entry(model).State = EntityState.Added;
        }
        else
        {
            _logger.LogTrace("Try to update new entity {Id} in table {Name}", model.Id, typeof(T).Name);
            db.Set<T>().Attach((T)model);
            db.Entry(model).State = EntityState.Modified;
        }

		var pResult = new PersistResult();

		try
		{
            var changeCount = await db.SaveChangesAsync(cancellationToken);

            pResult.Id = model.Id;
            pResult.ChangeCount = changeCount;
        }
        catch(Exception ex)
        {
            error = ex.Message;
        }

        if (error is not null)
        {
            pResult.HasError = true;
            pResult.ErrorBrokenRuleList = new List<BrokenRule>
            {
                { new BrokenRule("all", error!) }
            };
            _logger.LogValidationFailedResult("Failed to save entity", pResult);
        }
        else
        {
            _logger.LogTrace("Save entity {Id} in table {Name} (succes)", model.Id, typeof(T).Name);
        }

        return pResult;
    }

}
