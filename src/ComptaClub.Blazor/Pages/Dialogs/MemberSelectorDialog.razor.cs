using ComptaClub.Contracts.Models.Members;

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

	List<ViewModels.MemberRow>? memberList;
	RadzenDataGrid<ViewModels.MemberRow>? grid = default!;
	MemberListFilter filter = new();
	IList<ViewModels.MemberRow>? selectedMembers;

	async Task LoadDatas(LoadDataArgs args)
	{
		if (args is not null)
		{
			var searchFilter = args.Filters.FirstOrDefault(i => i.Property == "Entity.Name");
			if (searchFilter is not null)
			{
				filter.Search = $"{searchFilter.FilterValue}";
			}
		}
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