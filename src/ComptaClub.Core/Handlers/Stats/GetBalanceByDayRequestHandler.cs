using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Requests.Stats;

namespace ComptaClub.Handlers.Stats;

internal class GetBalanceByDayRequestHandler : IRequestHandler<GetBalanceByDayRequest, IEnumerable<BalanceByDay>>
{
    private readonly IMediator _mediator;
    private readonly IDbContextFactory<ComptaClubDbContext> _dbContextFactory;

    public GetBalanceByDayRequestHandler(IMediator mediator,
        IDbContextFactory<ComptaClubDbContext> dbContextFactory)
    {
        _mediator = mediator;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IEnumerable<BalanceByDay>> Handle(GetBalanceByDayRequest request, CancellationToken cancellationToken)
    {
        var currentExercice = await _mediator.Send(new Requests.Exercices.GetActiveExerciceRequest());
        if (currentExercice == null)
        {
            return new List<BalanceByDay>();
        }

        var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = from entry in db.Entries
                    where entry.ValueDate >= currentExercice.StartDate
                    && entry.ValueDate <= currentExercice.EndDate
                    && entry.DeletedDate == null
                    group entry by entry.ValueDate into g
                    orderby g.Key
                    select new
                    {
                        DayId = g.Key,
                        CreditAmount = g.Sum(i => i.Amount * (i.AccountDirection == AccountDirection.Credit ? 1 : 0)),
                        DebitAmount = g.Sum(i => i.Amount * (i.AccountDirection == AccountDirection.Debit ? 1 : 0)),
                    };

        var list = await query.ToListAsync(cancellationToken);

        var result = new List<BalanceByDay>();
        var balance = currentExercice.InitialAmount / 1000000m;
        for (int dayId = currentExercice.StartDate; dayId <= currentExercice.EndDate; dayId++)
        {
            var date = dayId.FromDayId();
            var activeDay = list.SingleOrDefault(i => i.DayId == dayId);
            var bbd = new BalanceByDay();
            bbd.DayId = dayId;
            bbd.Year = date.Year;
            bbd.Month = date.Month;
            bbd.DayOfMonth = date.Day;
            bbd.Day = date;
            if (activeDay != null)
            {
                bbd.DebitAmount = activeDay.DebitAmount / 1000000m;
                bbd.CreditAmount = activeDay.CreditAmount / 1000000m;
                balance = bbd.BalanceAmount = balance + (activeDay.CreditAmount - activeDay.DebitAmount) / 1000000m;
            }
            else
            {
                bbd.BalanceAmount = balance;
            }
            result.Add(bbd);
        }

        return result;
    }
}
