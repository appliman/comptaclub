using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class MemberSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	List<ViewModels.Member>? memberList;
	RadzenDataGrid<ViewModels.Member>? grid = new();
	MemberListFilter filter = new();
	IList<ViewModels.Member>? selectedMembers;

	async Task LoadDatas(LoadDataArgs args)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
		memberList = new();
		int rowIndex = 1;
		foreach (var data in page.List.OrderBy(i => i.Name))
		{
			var item = Mapper.Map<ViewModels.Member>(data);
			item.RowIndex = rowIndex++;
			memberList.Add(item);
		}
	}

	Task Select()
	{
		if (selectedMembers != null
			&& selectedMembers.Any())
		{
			DialogService.Close(selectedMembers.First());
		}
		else
		{
			DialogService.Close();
		}
		return Task.CompletedTask;
	}

}