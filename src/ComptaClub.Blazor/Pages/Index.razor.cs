using System.Security.Principal;

using AutoMapper;

using ComptaClub.Blazor.Extensions;
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
	IEnumerable<Models.AmountTotalByAccount> amountTotalByAccount = new List<Models.AmountTotalByAccount>();
	IEnumerable<ViewModels.Account> plan = new List<ViewModels.Account>();
	System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("fr-FR");

	protected override async Task OnInitializedAsync()
	{
		var exercice = await Mediator.Send(new GetActiveExerciceRequest());
		currentExercice = Mapper.Map<ViewModels.Exercice>(exercice);

		balanceByDayList = await Mediator.Send(new GetBalanceByDayRequest());
		amountTotalByAccount = await Mediator.Send(new GetAmountTotalByAccountRequest());
		plan = (await Mediator.Send(new GetPlanRequest())).MapToAccountList(Mapper);
		foreach (var total in amountTotalByAccount)
		{
			var account = plan.DeepFirstOrDefault(i => i.Id == total.Id);
			if (account != null)
			{
				account.Total = total.Total;
			}
		}
    }
}