using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Datas;

namespace ComptaClub.Blazor.Pages;

public partial class EditForecastBudget
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	[Parameter]
	public Guid ForecastBudgetId { get; set; }

	ForecastBudgetData forecastBudget = new();
	int levelMax = 0;

	protected override async Task OnInitializedAsync()
	{
		forecastBudget = (await Mediator.GetForecastBudgetById(ForecastBudgetId))!;

		var filter = new ForecastBudgetItemListFilter();
		filter.IdList.Add(ForecastBudgetId);

		forecastBudget.ItemList = await Mediator.GetForecastBudgetItemList(ForecastBudgetId);
		forecastBudget.ItemList.Levelize();
		levelMax = forecastBudget.ItemList.Max(i => i.Level) + 1;
		forecastBudget.ItemList.Hierarchize();
		forecastBudget.ComputeTotal();

		StateHasChanged();
	}

	void Recompute()
	{
		forecastBudget.ComputeTotal();
		StateHasChanged();
	}

	async Task Save()
	{
		var result = await Mediator.Send(new SaveForecastBudgetRequest(forecastBudget));
		if (result != null)
		{
			await NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Le bilan pr�visionnel a �t� mis � jour avec succ�s");
		}
		else
		{
			await NotificationService.Notify(NotificationSeverity.Error, "Sauvegarde", "Une erreur est survenue lors de la mise � jour du bilan pr�visionnel");
		}
	}
}