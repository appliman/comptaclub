using ComptaClub.Requests;
using ComptaClub.Results;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers;

internal class UnlinkMemberToEntryRequestHandler : IRequestHandler<Requests.UnlinkMemberToEntryRequest, Results.CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public UnlinkMemberToEntryRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(UnlinkMemberToEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var changeCount = await db.AssociatedMemberListByEntries
                                .Where(i => i.Id == request.Id)
                                .ExecuteDeleteAsync(cancellationToken);

        return new CommandResult
        {
            ChangeCount = changeCount,
            HasError = false
        };
    }
}
