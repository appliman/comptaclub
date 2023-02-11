using ComptaClub.Requests.Members;
using ComptaClub.Results;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers.Members;

internal class UnlinkMemberToEntryRequestHandler : IRequestHandler<UnlinkMemberToEntryRequest, CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public UnlinkMemberToEntryRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory)
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
