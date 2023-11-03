using ComptaClub.Requests;
using ComptaClub.Requests.IncomeStatements;

using MediatR;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class IncomeStatementSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	List<ViewModels.IncomeStatement>? incomeStatementList;
	RadzenDataGrid<ViewModels.IncomeStatement>? grid = new();
	IList<ViewModels.IncomeStatement>? selectedRow;

	async Task LoadDatas(LoadDataArgs args)
	{
		var filter = new Requests.IncomeStatements.IncomeStatementListFilter();
		filter.SortDirection = System.ComponentModel.ListSortDirection.Descending;
		filter.SortByName = "CreationDate";
		filter.PageSize = int.MaxValue;

		var page = await Mediator.Send(new GetPagedEntityListRequest<IncomeStatementListFilter, Datas.IncomeStatementData>(filter));
        var list = new List<ViewModels.IncomeStatement>();
		foreach (var item in page.List)
		{
            list.Add(Mapper.Map<ViewModels.IncomeStatement>(item));
		}
		incomeStatementList = list;	
	}

	Task Select()
	{
		if (selectedRow != null
			&& selectedRow.Any())
		{
			DialogService.Close(selectedRow[0]);
		}
		else
		{
			DialogService.Close();
		}
		return Task.CompletedTask;
	}
}