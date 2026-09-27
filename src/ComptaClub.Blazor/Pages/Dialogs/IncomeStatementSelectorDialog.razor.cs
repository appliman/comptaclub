using ComptaClub.Contracts.Models.IncomeStatements;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class IncomeStatementSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;


	List<ViewModels.IncomeStatement>? incomeStatementList;
	SuperDataGrid<ViewModels.IncomeStatement>? grid = default!;
	IList<ViewModels.IncomeStatement>? selectedRow;
	void OnSelectionChanged(IEnumerable<ViewModels.IncomeStatement> selected) => selectedRow = selected.ToList();

	async ValueTask<GridItemsProviderResult<ViewModels.IncomeStatement>> LoadDatas(GridItemsProviderRequest<ViewModels.IncomeStatement> request)
	{
		var filter = new IncomeStatementListFilter();
		filter.SortDirection = System.ComponentModel.ListSortDirection.Descending;
		filter.SortByName = "CreationDate";
		filter.PageSize = int.MaxValue;

		var page = await Mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, Datas.IncomeStatementData>(filter));
		var list = new List<ViewModels.IncomeStatement>();
		list.Add(new ViewModels.IncomeStatement()
		{
			Id = Guid.Empty,
			Description = "Sans compte de résultat"
		});
		foreach (var item in page.List)
		{
			list.Add(Mapping.Profile.ToViewModel(item));
		}
		incomeStatementList = list;
		IEnumerable<ViewModels.IncomeStatement> rows = list;
		if (request.SortColumn == "Description")
		{
			rows = request.SortDirection == SortDirection.Descending
				? rows.OrderByDescending(x => x.Description)
				: rows.OrderBy(x => x.Description);
		}
		return GridItemsProviderResult<ViewModels.IncomeStatement>.From(
			rows.Skip(request.StartIndex).Take(request.Count ?? list.Count).ToList(), list.Count);
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
