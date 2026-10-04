using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Models.Stats;

using ChannelMediator;

namespace ComptaClub.Blazor.Pages;

public partial class Index
{
    [Inject]
    IMediator Mediator { get; set; } = default!;

    ViewModels.Exercice currentExercice = new();
    ViewModels.Exercice? _previousExercice;
    IEnumerable<BalanceByDay> balanceByDayList = new List<BalanceByDay>();
    decimal _chargesAmount;
    decimal _produitsAmount;
    IEnumerable<ViewModels.Account> plan = new List<ViewModels.Account>();
    System.Globalization.CultureInfo ci = new("fr-FR");
    int memberCount = 0;

    protected override async Task OnInitializedAsync()
    {
        var _exerciceTask = Mediator.Send(new GetActiveExerciceRequest());
        var _exercicesTask = Mediator.Send(new GetAllExercicesRequest());
        var _planTask = Mediator.Send(new GetPlanRequest());
        var _memberCountTask = Mediator.Send(new GetMemberCountRequest());

        await Task.WhenAll(_exerciceTask, _exercicesTask, _planTask, _memberCountTask);

        plan = Mapping.Profile.ToViewModels(_planTask.Result);
        memberCount = _memberCountTask.Result;

        var _exercice = _exerciceTask.Result;
        if (_exercice is null)
        {
            return;
        }

        currentExercice = Mapping.Profile.ToViewModel(_exercice);
        var _previous = _exercicesTask.Result
            .Where(i => i.Id != _exercice.Id && i.EndDate < _exercice.StartDate)
            .OrderByDescending(i => i.EndDate)
            .ThenByDescending(i => i.StartDate)
            .FirstOrDefault();

        var _totalsTask = Mediator.Send(new GetAmountTotalByAccountRequest(_exercice.Id));
        var _balanceTask = Mediator.Send(new GetBalanceByDayRequest(_exercice.Id));
        var _previousTotalsTask = _previous is null
            ? Task.FromResult<IEnumerable<AmountTotalByAccount>>([])
            : Mediator.Send(new GetAmountTotalByAccountRequest(_previous.Id));

        await Task.WhenAll(_totalsTask, _balanceTask, _previousTotalsTask);

        if (_previous is not null)
        {
            _previousExercice = Mapping.Profile.ToViewModel(_previous);
        }

        var _totals = _totalsTask.Result.ToDictionary(i => i.Id, i => i.Total);
        var _previousTotals = _previousTotalsTask.Result.ToDictionary(i => i.Id, i => i.Total);
        ApplyTotals(plan, _totals, _previousTotals);
        balanceByDayList = _balanceTask.Result;

        _chargesAmount = plan.Where(i => i.Direction == Enums.AccountDirection.Debit).Sum(i => i.DeepTotal) / 1000000m;
        _produitsAmount = plan.Where(i => i.Direction == Enums.AccountDirection.Credit).Sum(i => i.DeepTotal) / 1000000m;
    }

    private static void ApplyTotals(IEnumerable<ViewModels.Account> accounts,
        IReadOnlyDictionary<Guid, long> totals, IReadOnlyDictionary<Guid, long> previousTotals)
    {
        // Le plan commun conserve aussi les comptes utilisés dans un seul des deux exercices.
        foreach (var _account in accounts)
        {
            _account.Total = totals.GetValueOrDefault(_account.Id);
            _account.PreviousTotal = previousTotals.GetValueOrDefault(_account.Id);
            ApplyTotals(_account.Children, totals, previousTotals);
        }
    }
}
