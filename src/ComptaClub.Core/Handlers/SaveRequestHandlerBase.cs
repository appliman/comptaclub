
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

    public virtual async Task<Results.PersistResult<Guid>> SaveEntity<T>(Datas.IPrimaryKey model)
        where T : class, new()
    {
        var db = await _dbContextFactory.CreateDbContextAsync();

        _logger.LogTrace("Try to save entity {Id} in table {Name}", model.Id, typeof(T).Name);

        var data = await db.Set<T>().FindAsync(model.Id);

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

        var changeCount = await db.SaveChangesAsync();

        var pResult = new Results.PersistResult<Guid>();
        var id = data as Datas.IPrimaryKey;
        if (id != null) 
        {
            pResult.Id = id.Id;
        }

        pResult.ChangeCount = changeCount;

        if (error != null)
        {
            pResult.HasError = true;
            pResult.ErrorBrokenRuleList = new List<Results.BrokenRule>
            {
                { new Results.BrokenRule("all", error!) }
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
