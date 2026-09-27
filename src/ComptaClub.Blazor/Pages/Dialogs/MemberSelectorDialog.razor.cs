using ComptaClub.Contracts.Models.Members;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class MemberSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;


	List<ViewModels.MemberRow>? memberList;
	SuperDataGrid<ViewModels.MemberRow>? grid = default!;
	MemberListFilter filter = new();
	IList<ViewModels.MemberRow>? selectedMembers;

	void OnSelectionChanged(IEnumerable<ViewModels.MemberRow> selected) => selectedMembers = selected.ToList();

	async Task OnNameFilterChanged(SuperDataGridFilterInfo filterInfo)
	{
		filter.Name = filterInfo.PropertyValue;
		if (grid is not null)
		{
			await grid.ReloadAsync();
		}
	}

	async Task OnEmailFilterChanged(SuperDataGridFilterInfo filterInfo)
	{
		filter.Email = filterInfo.PropertyValue;
		if (grid is not null)
		{
			await grid.ReloadAsync();
		}
	}

	async ValueTask<GridItemsProviderResult<ViewModels.MemberRow>> LoadItems(GridItemsProviderRequest<ViewModels.MemberRow> request)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
		memberList = new();
		int rowIndex = 1;
		foreach (var data in page.List.OrderBy(i => i.Name))
		{
			var item = new ViewModels.MemberRow();
			item.Entity = data;
			item.RowIndex = rowIndex++;
			memberList.Add(item);
		}
		IEnumerable<ViewModels.MemberRow> rows = memberList;
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Entity.Name" => descending ? rows.OrderByDescending(x => x.Entity.Name) : rows.OrderBy(x => x.Entity.Name),
			"Entity.Email" => descending ? rows.OrderByDescending(x => x.Entity.Email) : rows.OrderBy(x => x.Entity.Email),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.MemberRow>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	async Task Select()
	{
		if (selectedMembers != null
			&& selectedMembers.Any())
		{
			await DialogService.Close(selectedMembers.First());
		}
		else
		{
			await DialogService.Close();
		}
	}

}
