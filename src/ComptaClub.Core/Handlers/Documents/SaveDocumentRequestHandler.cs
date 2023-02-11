using System.Threading;

namespace ComptaClub.Handlers.Banks;

internal class SaveDocumentRequestHandler : SaveRequestHandlerBase, IRequestHandler<Requests.Documents.SaveDocumentRequest, Results.PersistResult<Guid>>
{
    private readonly IValidator<DocumentData> _validator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly ILogger<SaveDocumentRequestHandler> _logger;

    public SaveDocumentRequestHandler(
        IValidator<DocumentData> validator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<SaveDocumentRequestHandler> logger)
        : base(dbContextFactory, logger)
    {
        _validator = validator;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<Results.PersistResult<Guid>> Handle(Requests.Documents.SaveDocumentRequest request, CancellationToken cancellationToken)
    {
        var result = await _validator.ValidateAsync(request.Entity,cancellationToken);
        if (!result.IsValid)
        {
            return result.ToPersistResult<Guid>()!;
        }

        request.Entity.LastUpdate = DateTime.Now.ToDayId();

        using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        db.Database.BeginTransaction();
        var saveResult = await SaveEntity<DocumentData>(db, request.Entity, cancellationToken);
        if (saveResult.HasError)
        {
            return saveResult;
        }

        byte[]? content = null;
        if (request.ContentStream != null)
        {
            content = request.ContentStream.GetBuffer();
        }
        else if (request.FileName != null)
        {
            content = await System.IO.File.ReadAllBytesAsync(request.FileName, cancellationToken);
        }
        else if (request.Content != null)
        {
            content = request.Content;
        }

        try
        {
            var existingContent = await db.DocumentsContents.FindAsync(request.Entity.Id);
            if (content == null
                && existingContent == null)
            {
                db.Database.RollbackTransaction();
                saveResult.ErrorBrokenRuleList.Add(new Results.BrokenRule
                {
                    PropertyName = "Content",
                    MessageList = new List<string> { "Un document doit avoir un contenu" }
                });
                saveResult.HasError = true;
                return saveResult;
            }

            if (content != null)
            {
                if (existingContent != null)
                {
                    existingContent.Content = content!;
                    db.Entry(existingContent).State = EntityState.Modified;
                }
                else
                {
                    var documentContent = new DocumentContentData();
                    documentContent.DocumentId = request.Entity.Id;
                    documentContent.Content = content!;
                    db.DocumentsContents.Add(documentContent);
                    db.Entry(documentContent).State = EntityState.Added;
                }
                await db.SaveChangesAsync(cancellationToken);

                var size = content.LongLength;
                request.Entity.Size = size;
                await SaveEntity<DocumentData>(db, request.Entity, cancellationToken);
            }

            db.Database.CommitTransaction();
        }
        catch(Exception ex) 
        {
            db.Database.RollbackTransaction();
            ex.Data.Add("Id", request.Entity.Id);
            _logger.LogError(ex, ex.Message);
            saveResult.HasError = true;
            saveResult.AddErrorBrokenRule("All", ex.Message);
        }

        return saveResult;
    }
}