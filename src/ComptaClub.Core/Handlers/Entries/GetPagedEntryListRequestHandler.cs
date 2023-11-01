using ComptaClub.Requests;
using ComptaClub.Requests.Accounts;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ComptaClub.Handlers.Entries;

internal class GetPagedEntryListRequestHandler : GetEntityPagedListRequestHandlerBase<EntryListFilter, EntryData>
{
    private readonly IMediator _mediator;

    public GetPagedEntryListRequestHandler(IDbContextFactory<ComptaClubDbContext> dbContextFactory,
        IMediator mediator)
        : base(dbContextFactory)
    {
        _mediator = mediator;
    }

    public override async Task<PagedList<IEnumerable<EntryData>>> Handle(GetPagedEntityListRequest<EntryListFilter, EntryData> request, CancellationToken cancellationToken)
    {
        var filter = request.GetFilter(new EntryListFilter());

        filter.EnsureGoodFilter();

        var db = await DbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from entry in db.Entries
                    select entry;

        if (filter.AccountIdList != null
            && filter.AccountIdList.Any())
        {
            if (!filter.UseDeepAccount)
            {
                query = query.Where(i => filter.AccountIdList.Contains(i.AccountId));
            }
            else if (filter.AccountIdList.Count == 1
                   && filter.UseDeepAccount)
            {
                var plan = await _mediator.Send(new GetPlanRequest());
                var account = plan.DeepFind(filter.AccountIdList[0]);
                if (account is not null)
                {
                    var idList = account.GetIdListWithAllChildren();
                    query = query.Where(i => idList.Contains(i.AccountId));
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPattern = $"%{filter.Search}%";
			query = query.Where(i => EF.Functions.Like(i.ExtraInfos ?? "**************", searchPattern)
                                    || EF.Functions.Like(i.Label, searchPattern)
                                    || EF.Functions.Like(i.PartNumber, searchPattern));
        }

        if (filter.ExerciceId.HasValue)
        {
            query = query.Where(i => i.ExerciceId == filter.ExerciceId.Value);
        }

        if (filter.ImportIdList != null
            && filter.ImportIdList.Any())
        {
            query = query.Where(i => i.ImportId != null && filter.ImportIdList.Contains(i.ImportId));
        }

        if (filter.PaymentType.HasValue)
        {
            query = query.Where(i => i.PaymentType == filter.PaymentType.Value);
        }

        if (filter.FromDayId.HasValue)
        {
            query = query.Where(i => i.CreationDate >= filter.FromDayId.Value);
        }

        if (filter.ToDayId.HasValue)
        {
            query = query.Where(i => i.CreationDate <= filter.ToDayId.Value);
        }

        if (filter.DebitAmountFilter.Amount > 0)
        {
            query = query.Where(i => i.Amount >= filter.DebitAmountFilter.Min 
                                    && i.Amount <= filter.DebitAmountFilter.Max
                                    && i.AccountDirection == Enums.AccountDirection.Debit);
        }

        if (filter.CreditAmountFilter.Amount > 0)
        {
            query = query.Where(i => i.Amount >= filter.CreditAmountFilter.Min
                                    && i.Amount <= filter.CreditAmountFilter.Max
                                    && i.AccountDirection == Enums.AccountDirection.Credit);
        }

        if (filter.BalanceAmountFilter.Amount > 0)
        {
            query = query.Where(i => i.Balance >= filter.CreditAmountFilter.Min
                                    && i.Balance <= filter.CreditAmountFilter.Max);
        }

        switch (filter.Options.DeletedState)
        {
            case DeletedState.Undeleted:
                query = query.Where(i => i.DeletedDate == null);
                break;
            case DeletedState.Delete:
                query = query.Where(i => i.DeletedDate != null);
                break;
            case DeletedState.Both:
                break;
            default:
                break;
        }

        var page = await query.GetPagedDataList(i => i.CreationDate, filter, cancellationToken);

        var result = new PagedList<IEnumerable<EntryData>>()
        {
            List = page.List,
            Total = new PagedTotal
            {
                RowCount = page.Count
            }
        };

        return result;
    }
}
