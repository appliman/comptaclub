using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Results;

using DocumentFormat.OpenXml.Vml.Office;

namespace ComptaClub.Handlers.Members;

public class LinkMemberToEntryRequestHandler : IRequestHandler<LinkMemberToEntryRequest, PersistResult>
{
    private readonly IComptaClubDbContextFactory _dbContextFactory;
    private readonly IMediator _mediator;
    private readonly IValidator<AssociatedMemberListByEntryData> _validator;

    public LinkMemberToEntryRequestHandler(IComptaClubDbContextFactory dbContextFactory,
        IMediator mediator,
        IValidator<AssociatedMemberListByEntryData> validator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<PersistResult> Handle(LinkMemberToEntryRequest request, CancellationToken cancellationToken)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var data = await db.AssociatedMemberListByEntries.Where(i => i.MemberId == request.MemberId
                                                                  && i.EntryId == request.EntryId)
                        .SingleOrDefaultAsync(cancellationToken);

        if (data == null)
        {
            data = new AssociatedMemberListByEntryData
            {
                Id = Guid.NewGuid(),
                EntryId = request.EntryId,
                MemberId = request.MemberId,
                CreationDate = DateTime.Now.ToDayId(),
                Amount = request.Amount
            };

            db.AssociatedMemberListByEntries.Add(data);
            db.Entry(data).State = EntityState.Added;
        }
        else
        {
            data.Amount = request.Amount;
            db.Entry(data).State = EntityState.Modified;
        }

        var result = await _validator.ValidateAsync(data);
        if (!result.IsValid)
        {
            return result.ToPersistResult()!;
        }

        var changeCount = await db.SaveChangesAsync(cancellationToken);
        return new PersistResult
        {
            Id = data.Id,
            ChangeCount = changeCount,
            HasError = false
        };
    }
}
