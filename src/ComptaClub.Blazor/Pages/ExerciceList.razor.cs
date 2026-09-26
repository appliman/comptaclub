using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Contracts.Results;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class ExerciceList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;


	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	List<ViewModels.Exercice>? exerciceList;
	SuperDataGrid<ViewModels.Exercice>? grid;
	List<BrokenRule> brokenRules = new();
	IList<ViewModels.Exercice>? selectedExercices;

	async Task LoadDatas()
	{
		var datas = await Mediator!.Send(new GetAllExercicesRequest());
		exerciceList = Mapping.Profile.ToViewModels(datas);
	}

	void OnSelectionChanged(IEnumerable<ViewModels.Exercice> selected) => selectedExercices = selected.ToList();

	async ValueTask<GridItemsProviderResult<ViewModels.Exercice>> LoadItems(GridItemsProviderRequest<ViewModels.Exercice> request)
	{
		if (exerciceList is null)
        {
            await LoadDatas();
        }

        IEnumerable<ViewModels.Exercice> rows = exerciceList ?? [];
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Code" => descending ? rows.OrderByDescending(x => x.Code) : rows.OrderBy(x => x.Code),
			"Label" => descending ? rows.OrderByDescending(x => x.Label) : rows.OrderBy(x => x.Label),
			"InitialAmount" => descending ? rows.OrderByDescending(x => x.InitialAmount) : rows.OrderBy(x => x.InitialAmount),
			"BalanceAmount" => descending ? rows.OrderByDescending(x => x.BalanceAmount) : rows.OrderBy(x => x.BalanceAmount),
			"StartDate" => descending ? rows.OrderByDescending(x => x.StartDate) : rows.OrderBy(x => x.StartDate),
			"EndDate" => descending ? rows.OrderByDescending(x => x.EndDate) : rows.OrderBy(x => x.EndDate),
			"ExerciceState" => descending ? rows.OrderByDescending(x => x.ExerciceState) : rows.OrderBy(x => x.ExerciceState),
			_ => rows.OrderBy(x => x.Code)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.Exercice>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	async Task RefreshAsync()
	{
		await LoadDatas();
		if (grid is not null)
        {
            await grid.ReloadAsync();
        }
    }

	void EditRow(ViewModels.Exercice exercice)
	{
		NavigationManager.NavigateTo($"/exercice/edition/{exercice.Id}");
	}

	async Task DeleteRow(ViewModels.Exercice exercice)
	{
		var deleteResult = await Mediator.Send(new DeleteExerciceRequest(exercice.Id));
		if (deleteResult.HasError)
		{
			await NotificationService.NotifyError(deleteResult);
		}
		else if (deleteResult.HasWarning)
		{
			await NotificationService.NotifyWarning(deleteResult);
		}
		await RefreshAsync();
	}

	async Task ChangeActiveExercice(ChangeEventArgs args, ViewModels.Exercice exercice)
	{
		var changeResult = await Mediator.Send(new ChangeActiveExerciceRequest($"{args.Value}" == "on", exercice.Id));
		if (changeResult.HasError)
		{
			await NotificationService.NotifyError(changeResult);
		}
		else if (changeResult.HasWarning)
		{
			await NotificationService.NotifyWarning(changeResult);
		}
		await RefreshAsync();
	}

	async Task CloseExercice()
	{
		var selectedExercice = selectedExercices?.FirstOrDefault();
		if (selectedExercice is null)
		{
			await NotificationService.Notify(new NotificationMessage()
			{
				Severity = NotificationSeverity.Info,
				Summary = "Vous devez cliquer sur un exercice pour le clore"
			});
			return;
		}

		var confirm = await MainLayout.DialogService.Confirm("Cl�ture", "Confirmez vous la cl�ture de cet exercice");
		if (!confirm)
		{
			return;
		}

		var closeResult = await Mediator.Send(new CloseExerciceRequest(selectedExercice.Id));
		if (closeResult.HasError)
		{
			await NotificationService.NotifyError(closeResult);
		}
		else if (closeResult.HasWarning)
		{
			await NotificationService.NotifyWarning(closeResult);
		}
		await RefreshAsync();
	}

}
