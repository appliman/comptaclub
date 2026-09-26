using ComptaClub.Contracts.Models.Exercices;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class ExerciceSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;


	List<ViewModels.Exercice>? exerciceList;
	SuperDataGrid<ViewModels.Exercice>? grid = default!;
	IList<ViewModels.Exercice>? selectedRow;
	void OnSelectionChanged(IEnumerable<ViewModels.Exercice> selected) => selectedRow = selected.ToList();

	async ValueTask<GridItemsProviderResult<ViewModels.Exercice>> LoadDatas(GridItemsProviderRequest<ViewModels.Exercice> request)
	{
		var datas = await Mediator!.Send(new GetAllExercicesRequest());
		datas.RemoveAll(i => i.ExerciceState != ExerciceState.Closed);
		exerciceList = Mapping.Profile.ToViewModels(datas);
		var rowIndex = 1;
		foreach (var item in exerciceList)
		{
			item.RowIndex = rowIndex++;
		}
		IEnumerable<ViewModels.Exercice> rows = exerciceList;
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Code" => descending ? rows.OrderByDescending(x => x.Code) : rows.OrderBy(x => x.Code),
			"Label" => descending ? rows.OrderByDescending(x => x.Label) : rows.OrderBy(x => x.Label),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		return GridItemsProviderResult<ViewModels.Exercice>.From(
			rows.Skip(request.StartIndex).Take(request.Count ?? exerciceList.Count).ToList(), exerciceList.Count);
	}

	async Task Select()
	{
		if (selectedRow != null
			&& selectedRow.Any())
		{
			await DialogService.Close(selectedRow[0]);
		}
		else
		{
			await DialogService.Close();
		}
	}
}
