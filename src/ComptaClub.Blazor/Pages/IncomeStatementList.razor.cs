using ComptaClub.Contracts.Models.IncomeStatements;
using ComptaClub.Datas;

namespace ComptaClub.Blazor.Pages;

public partial class IncomeStatementList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;


	List<ViewModels.IncomeStatement>? incomeStatementList;
	RadzenDataGrid<ViewModels.IncomeStatement>? grid;
	IList<ViewModels.IncomeStatement>? selectedRow;

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
		incomeStatementList = Mapper.Map<List<ViewModels.IncomeStatement>>(page.List);
	}

	async Task DeleteRow(ViewModels.IncomeStatement item)
	{
		var dialog = await DialogService.Confirm("Confirmez-vous la suppression de ce compte de résultat ?", "Suppression compte de résultat", new ConfirmOptions
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
			NotificationService.NotifyError(deleteResult);
			return;
		}

		await LoadDatas();
	}

	async Task CreateIncomeStatement()
	{
		var dialog = await DialogService.OpenAsync<Dialogs.ExerciceSelectorDialog>("Selection d'un exercice",
			options: new DialogOptions
			{
				CloseDialogOnEsc = true,
			});

		var exercice = dialog as ViewModels.Exercice;
		if (exercice is null)
		{
			return;
		}

		var createResult = await Mediator.Send(new CreateAndSaveIncomeStatementRequest(exercice.Id));
		if (createResult.HasError)
		{
			NotificationService.NotifyError(createResult);
			return;
		}

		NavigationManager.NavigateTo($"/compte-de-resultat/{createResult.Id}");
	}
}
