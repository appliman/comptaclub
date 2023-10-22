
using AutoMapper;

using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class MemberList : ComponentBase
{
	[CascadingParameter]
	Shared.MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	IMapper Mapper { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	List<ViewModels.Member>? memberList;
	RadzenDataGrid<ViewModels.Member>? grid = new();
	MemberListFilter filter = new();
	bool displayUpload = false;

	protected override void OnInitialized()
	{
		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "upload_file",
			Text = "Importer",
			Title = "Importer à partir d'un fichier excel",
			OnClick = ImportFile
		}).Display();

		filter.PageSize = 100;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
		memberList = Mapper.Map<List<ViewModels.Member>>(page.List);

		var balanceByMemberList = await Mediator.Send(new Requests.Members.GetBalanceByMemberListRequest(filter));
		int rowIndex = 1;
		foreach (var item in memberList)
		{
			var balance = balanceByMemberList.SingleOrDefault(i => i.MemberId == item.Id);
			if (balance != null)
			{
				item.Amount = balance.Balance;
			}
			item.RowIndex = rowIndex++;
		}
	}

	async Task ImportFile()
	{
		var uploadDialog = await DialogService.OpenAsync<Dialogs.ImportMemberExcelFileDialog>("Importer un fichier excel des membres");
		if (uploadDialog is null)
		{
			return;
		}

		var ms = uploadDialog as MemoryStream;
		if (ms is null)
		{
			return;
		}
		ms.Seek(0, SeekOrigin.Begin);
		var dataList = await Mediator.Send(new Requests.Members.ImportExcelMemberListRequest(ms));
		memberList = Mapper.Map<List<ViewModels.Member>>(dataList);
		int rowIndex = 1;
		foreach (var item in memberList)
		{
			item.RowIndex = rowIndex++;
		}
		displayUpload = false;
	}
}