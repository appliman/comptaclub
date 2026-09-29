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
	IEnumerable<BalanceByDay> balanceByDayList = new List<BalanceByDay>();
	decimal _chargesAmount;
	decimal _produitsAmount;
	IEnumerable<AmountTotalByAccount> amountTotalByAccountList = new List<AmountTotalByAccount>();
	IEnumerable<ViewModels.Account> plan = new List<ViewModels.Account>();
	System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("fr-FR");
	int memberCount = 0;

	protected override async Task OnInitializedAsync()
	{
		var tasks = new List<Task>();

		var t1 = Mediator.Send(new GetActiveExerciceRequest());
		var t2 = Mediator.Send(new GetAmountTotalByAccountRequest());
		var t3 = Mediator.Send(new GetBalanceByDayRequest());
		var t4 = Mediator.Send(new GetPlanRequest());
		var t5 = Mediator.Send(new GetMemberCountRequest());

		tasks.Add(t1);
		tasks.Add(t2);
		tasks.Add(t3);
		tasks.Add(t4);
		tasks.Add(t5);

		await Task.WhenAll(tasks);

		var exercice = t1.Result;
		if (exercice is not null)
		{
			currentExercice = Mapping.Profile.ToViewModel(exercice);
		}

		amountTotalByAccountList = t2.Result;
		balanceByDayList = t3.Result;
		var planData = t4.Result;

		plan = Mapping.Profile.ToViewModels(planData);
		foreach (var total in amountTotalByAccountList)
		{
			var account = plan.DeepFirstOrDefault(i => i.Id == total.Id);
			if (account != null)
			{
				account.Total = total.Total;
			}
		}

		_chargesAmount = plan.Where(i => i.Direction == Enums.AccountDirection.Debit).Sum(i => i.DeepTotal) / 1000000m;
		_produitsAmount = plan.Where(i => i.Direction == Enums.AccountDirection.Credit).Sum(i => i.DeepTotal) / 1000000m;

		memberCount = t5.Result;
	}
}
