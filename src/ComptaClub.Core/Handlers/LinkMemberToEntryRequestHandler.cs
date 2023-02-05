
using ComptaClub.Requests;
using ComptaClub.Results;

using DocumentFormat.OpenXml.Vml.Office;

namespace ComptaClub.Handlers;

public class LinkMemberToEntryRequestHandler : IRequestHandler<Requests.LinkMemberToEntryRequest, Results.PersistResult<Guid>>
{
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;
    private readonly IMediator _mediator;
    private readonly IValidator<AssociatedMemberListByEntryData> _validator;

    public LinkMemberToEntryRequestHandler(IDbContextFactory<Datas.ComptaClubDbContext> dbContextFactory,
        IMediator mediator,
        IValidator<Datas.AssociatedMemberListByEntryData> validator)
    {
        _dbContextFactory = dbContextFactory;
        _mediator = mediator;
        _validator = validator;
    }

    public async Task<PersistResult<Guid>> Handle(LinkMemberToEntryRequest request, CancellationToken cancellationToken)
    {
        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var data = await db.AssociatedMemberListByEntries.Where(i => i.MemberId == request.MemberId
                                                                  && i.EntryId == request.EntryId)
                        .SingleOrDefaultAsync(cancellationToken);

        if (data == null)
        {
            data = new Datas.AssociatedMemberListByEntryData
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
            return result.ToPersistResult<Guid>()!;
        }

        var changeCount = await db.SaveChangesAsync(cancellationToken);
        return new PersistResult<Guid>
        {
            Id = data.Id,
            ChangeCount = changeCount,
            HasError = false
        };
    }
}
