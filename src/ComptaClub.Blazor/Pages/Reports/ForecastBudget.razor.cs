using ComptaClub.Contracts.Models.Clubs;
using ComptaClub.Contracts.Models.ForecastBudget;

using MediatR;

namespace ComptaClub.Blazor.Pages.Reports;

public partial class ForecastBudget
{
	[Parameter]
	public Guid ForecastBudgetId { get; set; }

	[Inject]
	IMediator Mediator { get; set; } = default!;

	Datas.ClubData club = new();
	Datas.ForecastBudgetData forecastBudget = new();
	int levelMax = 0;

	protected override async Task OnInitializedAsync()
	{
		club = await Mediator.Send(new GetClubRequest());

		forecastBudget = (await Mediator.GetForecastBudgetById(ForecastBudgetId))!;

		var filter = new ForecastBudgetItemListFilter();
		filter.IdList.Add(ForecastBudgetId);

		forecastBudget.ItemList = await Mediator.GetForecastBudgetItemList(ForecastBudgetId);
		forecastBudget.ItemList.Levelize();
		levelMax = forecastBudget.ItemList.Max(i => i.Level) + 1;
		forecastBudget.ItemList.Hierarchize();
		forecastBudget.ComputeTotal();
	}
}