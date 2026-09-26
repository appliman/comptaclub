using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Results;

using Microsoft.EntityFrameworkCore;

namespace ComptaClub.Handlers.Members;

internal class UnlinkMemberToEntryRequestHandler : IRequestHandler<UnlinkMemberToEntryRequest, CommandResult>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;

    public UnlinkMemberToEntryRequestHandler(IComptaClubDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(UnlinkMemberToEntryRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

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
