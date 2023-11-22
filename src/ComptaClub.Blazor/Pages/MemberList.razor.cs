using ComptaClub.Contracts.Models.Members;

using MediatR;

namespace ComptaClub.Blazor.Pages;

public partial class MemberList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	ILogger<MemberList> Logger { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;

	List<ViewModels.MemberRow>? memberList;
	RadzenDataGrid<ViewModels.MemberRow> grid = default!;
	MemberListFilter filter = new();
	IList<ViewModels.MemberRow>? selectedMembers;

	protected override void OnInitialized()
	{
		filter.PageSize = 100;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		var page = await Mediator.Send(new GetPagedEntityListRequest<MemberListFilter, Datas.MemberData>(filter));
		var rowList = new List<ViewModels.MemberRow>();

		var balanceByMemberList = await Mediator.Send(new GetBalanceByMemberListRequest(filter));
		int rowIndex = 1;
		foreach (var item in page.List)
		{
			var balance = balanceByMemberList.SingleOrDefault(i => i.MemberId == item.Id);
			var row = new ViewModels.MemberRow
			{
				Entity = item,
				RowIndex = rowIndex++
			};
			if (balance != null)
			{
				row.Amount = balance.Balance;
			}
			rowList.Add(row);
		}
		memberList = rowList;
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

		// Mediator.Send(new Importmem

		await grid.Reload();
	}
}