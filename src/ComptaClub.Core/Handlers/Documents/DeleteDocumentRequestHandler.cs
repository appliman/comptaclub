using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;
using ComptaClub.Requests.Documents;
using ComptaClub.Results;

namespace ComptaClub.Handlers.Documents;

internal class DeleteDocumentRequestHandler : IRequestHandler<DeleteDocumentRequest, CommandResult>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly ILogger<DeleteDocumentRequestHandler> _logger;

    public DeleteDocumentRequestHandler(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        ILogger<DeleteDocumentRequestHandler> logger)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(DeleteDocumentRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        db.Database.BeginTransaction();

        int changeCount = 0;
        try
        {
            changeCount = await db.Documents.Where(i => i.Id == request.DocumentId).ExecuteDeleteAsync(cancellationToken);
            changeCount += await db.DocumentsContents.Where(i => i.DocumentId == request.DocumentId).ExecuteDeleteAsync(cancellationToken);

            db.Database.CommitTransaction();
        }
        catch (Exception ex)
        {
            ex.Data.Add("Id", request.DocumentId);
            _logger.LogError(ex, ex.Message);
        }

        return new CommandResult
        {
            ChangeCount = changeCount,
            HasError = changeCount == 0
        };
    }
}
