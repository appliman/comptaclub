using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Datas;
using ComptaClub.Requests.ForecastBudget;

namespace ComptaClub.Blazor.Pages;

public partial class EditForecastBudget
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

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
		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "save",
			Text = "Enregistrer",
			OnClick = Save
		}).AddItem(new ViewModels.Toolbar.ToolbarLink
		{
			IconName = "print",
			Text = "Imprimer",
			Target = "_blank",
			Description = "Imprimer le bilan prévisionnel",
			Url = $"/reports/bilan-previsionnel/{ForecastBudgetId}"
		})
		.Display();

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
			NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Le bilan prévisionnel a été mis à jour avec succès");
		}
		else
		{
			NotificationService.Notify(NotificationSeverity.Error, "Sauvegarde", "Une erreur est survenue lors de la mise à jour du bilan prévisionnel");
		}
	}
}