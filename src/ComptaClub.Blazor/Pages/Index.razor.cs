using System.Security.Principal;

using AutoMapper;

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages;
public partial class Index
{
	[Inject] 
	IMediator Mediator { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	ViewModels.Exercice currentExercice = new();
	IEnumerable<Models.BalanceByDay> balanceByDayList = new List<Models.BalanceByDay>();
	IEnumerable<Models.AmountTotalByAccount> amountTotalByAccountList = new List<Models.AmountTotalByAccount>();
	IEnumerable<ViewModels.Account> plan = new List<ViewModels.Account>();
	System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("fr-FR");
	int memberCount = 0;

	protected override async Task OnInitializedAsync()
	{
		var exercice = await Mediator.Send(new Requests.Exercices.GetActiveExerciceRequest());
		currentExercice = Mapper.Map<ViewModels.Exercice>(exercice);

		balanceByDayList = await Mediator.Send(new Requests.Stats.GetBalanceByDayRequest());
		amountTotalByAccountList = await Mediator.Send(new Requests.Accounts.GetAmountTotalByAccountRequest());
		plan = (await Mediator.Send(new Requests.Accounts.GetPlanRequest())).MapToAccountList(Mapper);
		foreach (var total in amountTotalByAccountList)
		{
			var account = plan.DeepFirstOrDefault(i => i.Id == total.Id);
			if (account != null)
			{
				account.Total = total.Total;
			}
		}

		memberCount = await Mediator.Send(new Requests.Members.GetMemberCountRequest());
    }
}