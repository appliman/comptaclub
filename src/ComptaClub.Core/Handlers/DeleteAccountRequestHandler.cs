using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class DeleteAccountRequestHandler : IRequestHandler<DeleteAccountRequest, CommandResult>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public DeleteAccountRequestHandler(IMediator mediator,
        IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        // Recherche des entrées associées
        var entriesRequest = new Requests.GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(f =>
        {
            f.PageSize = 1;
            f.AccountIdList = new List<Guid>{ request.AccountId };
        });
        var entries = await _mediator.Send(entriesRequest);
        if (entries.List.Any())
        {
            return CommandResult.CreateInvalidResult("Des écritures sont déjà associées à ce compte");
        }

        // Recherche des enfants

        var plan = await _mediator.Send(new GetPlanRequest());
        var account = plan.DeepFind(request.AccountId);
        if (account == null)
        {
            return CommandResult.CreateWarningResult("Ce compte n'existe pas");
        }

        if (account.Children.Any())
        {
            return CommandResult.CreateInvalidResult("Ce compte contient d'autres comptes");
        }

        var db = await _dbContextFactory.CreateDbContextAsync();

        var data = await db.Accounts.FindAsync(request.AccountId);
        db.Accounts.Remove(data!);
        db.Entry(data!).State = EntityState.Deleted;

        var changeCount = await db.SaveChangesAsync();
        return new CommandResult
        {
            ChangeCount = changeCount,
            HasError = changeCount != 1
        };
    }
}
