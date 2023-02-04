
using ComptaClub.Requests;
using ComptaClub.Results;

namespace ComptaClub.Handlers;

public class LinkMemberToEntryRequestHandler : IRequestHandler<Requests.LinkMemberToEntryRequest, Results.CommandResult>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public LinkMemberToEntryRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<CommandResult> Handle(LinkMemberToEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.AssociatedMemberListByEntries.Where(i => i.MemberId == request.MemberId
                                                                  && i.EntryId == request.EntryId).SingleOrDefaultAsync();

        if (existing != null)
        {
            return CommandResult.CreateWarningResult("Ce membre est déjà associé à cette écriture");
        }

        var assoc = new Datas.AssociatedMemberListByEntryData
        {
            Id = Guid.NewGuid(),
            EntryId = request.EntryId,
            MemberId = request.MemberId,
            CreationDate = DateTime.Now.ToDayId()
        };

        db.AssociatedMemberListByEntries.Add(assoc);
        db.Entry(assoc).State = EntityState.Added;

        var changeCount = await db.SaveChangesAsync(cancellationToken);
        return new CommandResult
        {
            ChangeCount = changeCount,
            HasError = false
        };
    }
}
