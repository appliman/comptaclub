using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.ForecastBudget;
using ComptaClub.Datas;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class ForecastBudgetList : ComponentBase
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


	List<ForecastBudgetRow>? forecastBudgetList;
	SuperDataGrid<ForecastBudgetRow>? grid;

	ValueTask<GridItemsProviderResult<ForecastBudgetRow>> LoadItems(GridItemsProviderRequest<ForecastBudgetRow> request)
	{
		IEnumerable<ForecastBudgetRow> rows = forecastBudgetList ?? [];
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Name" => descending ? rows.OrderByDescending(x => x.Entity.Name) : rows.OrderBy(x => x.Entity.Name),
			"Description" => descending ? rows.OrderByDescending(x => x.Entity.Description) : rows.OrderBy(x => x.Entity.Description),
			"CreationDate" => descending ? rows.OrderByDescending(x => x.Entity.CreationDate) : rows.OrderBy(x => x.Entity.CreationDate),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var all = rows.ToList();
		return ValueTask.FromResult(GridItemsProviderResult<ForecastBudgetRow>.From(
			all.Skip(request.StartIndex).Take(request.Count ?? all.Count).ToList(), all.Count));
	}

	protected override async Task OnInitializedAsync()
	{
		await LoadDatas();
	}

	async Task LoadDatas()
	{
		var filter = new ForecastBudgetListFilter();
		filter.SortDirection = System.ComponentModel.ListSortDirection.Descending;
		filter.SortByName = "CreationDate";

		var page = await Mediator.Send(new GetPagedEntityListRequest<ForecastBudgetListFilter, ForecastBudgetData>(filter));
		var list = new List<ForecastBudgetRow>();
		var rowIndex = 1;
		foreach (var item in page.List)
		{
			list.Add(new ForecastBudgetRow
			{
				Entity = item,
				RowIndex = rowIndex++
			});
		}
		forecastBudgetList = list;
	}

	async Task DeleteRow(ForecastBudgetData item)
	{
		var dialog = await DialogService.Confirm("Suppression bilan prévisionnel", "Confirmez-vous la suppression de ce bilan prévisionnel ?",
			new ConfirmOptions
			{
				OkButtonText = "Supprimer",
				CancelButtonText = "Annuler"
			});

		if (dialog == false)
		{
			return;
		}

		var deleteResult = await Mediator.Send(new DeleteForecastBudgetRequest(item.Id));
		if (deleteResult.HasError)
		{
			await NotificationService.NotifyError(deleteResult);
			return;
		}

		await LoadDatas();
		if (grid is not null)
        {
            await grid.ReloadAsync();
        }
    }

	async Task CreateForecastBudget()
	{
		var dialog = await DialogService.OpenAsync<Dialogs.IncomeStatementSelectorDialog>("Selection d'un compte de résultat",
			options: new DialogOptions
			{

			});

		var incomeStatement = dialog as ViewModels.IncomeStatement;
		if (incomeStatement is null)
		{
			return;
		}

		var createResult = await Mediator.Send(new CreateAndSaveForecastBudgetFromIncomeStatementRequest(incomeStatement.Id,
			"Bilan prévisionnel",
			incomeStatement.Description));

		if (createResult.HasError)
		{
			await NotificationService.NotifyError(createResult);
			return;
		}

		NavigationManager.NavigateTo($"/bilan-previsionnel/{createResult.Id}");
	}
}
