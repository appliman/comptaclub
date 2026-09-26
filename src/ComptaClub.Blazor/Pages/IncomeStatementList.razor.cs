using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Datas;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class IncomeStatementList : ComponentBase
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


	List<ViewModels.IncomeStatement>? incomeStatementList;
	SuperDataGrid<ViewModels.IncomeStatement>? grid;

	ValueTask<GridItemsProviderResult<ViewModels.IncomeStatement>> LoadItems(GridItemsProviderRequest<ViewModels.IncomeStatement> request)
	{
		IEnumerable<ViewModels.IncomeStatement> rows = incomeStatementList ?? [];
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Description" => descending ? rows.OrderByDescending(x => x.Description) : rows.OrderBy(x => x.Description),
			"CreationDate" => descending ? rows.OrderByDescending(x => x.CreationDate) : rows.OrderBy(x => x.CreationDate),
			_ => rows.OrderByDescending(x => x.CreationDate)
		};
		var all = rows.ToList();
		return ValueTask.FromResult(GridItemsProviderResult<ViewModels.IncomeStatement>.From(
			all.Skip(request.StartIndex).Take(request.Count ?? all.Count).ToList(), all.Count));
	}

	protected override async Task OnInitializedAsync()
	{
		await LoadDatas();
	}

	async Task LoadDatas()
	{
		var filter = new IncomeStatementListFilter();
		filter.SortDirection = System.ComponentModel.ListSortDirection.Descending;
		filter.SortByName = "CreationDate";

		var page = await Mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, IncomeStatementData>(filter));
		incomeStatementList = Mapping.Profile.ToViewModels(page.List);
	}

	async Task DeleteRow(ViewModels.IncomeStatement item)
	{
		var dialog = await DialogService.Confirm("Suppression compte de résultat", "Confirmez-vous la suppression de ce compte de résultat ?", new ConfirmOptions
		{
			OkButtonText = "Supprimer",
			CancelButtonText = "Annuler"
		});

		if (dialog == false)
		{
			return;
		}

		var deleteResult = await Mediator.Send(new DeleteIncomeStatementRequest(item.Id));
		if (deleteResult.HasError)
		{
			await NotificationService.NotifyError(deleteResult);
			return;
		}

		await LoadDatas();
		if (grid is not null) await grid.ReloadAsync();
	}

	async Task CreateIncomeStatement()
	{
		var dialog = await DialogService.OpenAsync<Dialogs.ExerciceSelectorDialog>("Selection d'un exercice",
			options: new DialogOptions
			{

			});

		var exercice = dialog as ViewModels.Exercice;
		if (exercice is null)
		{
			return;
		}

		var createResult = await Mediator.Send(new CreateAndSaveIncomeStatementRequest(exercice.Id));
		if (createResult.HasError)
		{
			await NotificationService.NotifyError(createResult);
			return;
		}

		NavigationManager.NavigateTo($"/compte-de-resultat/{createResult.Id}");
	}
}
