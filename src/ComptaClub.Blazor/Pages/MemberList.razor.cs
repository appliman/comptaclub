
using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Results;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

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
	SuperDataGrid<ViewModels.MemberRow> grid = default!;
	MemberListFilter filter = new();
	IList<ViewModels.MemberRow>? selectedMembers;
    List<BrokenRule> brokenRules = new();

    protected override void OnInitialized()
	{
		filter.PageSize = 500;
	}

	void OnSelectionChanged(IEnumerable<ViewModels.MemberRow> selected) => selectedMembers = selected.ToList();

	async ValueTask<GridItemsProviderResult<ViewModels.MemberRow>> LoadItems(GridItemsProviderRequest<ViewModels.MemberRow> request)
	{
		filter.Name = null;
		filter.Email = null;
		filter.LicenseNumber = null;
		filter.LicenseTypeName = null;
		foreach (var filterItem in request.Filters)
		{
			switch (filterItem.PropertyName.ToLowerInvariant())
			{
				case "entity.name": filter.Name = filterItem.PropertyValue; break;
				case "entity.email": filter.Email = filterItem.PropertyValue; break;
				case "entity.licensenumber": filter.LicenseNumber = filterItem.PropertyValue; break;
				case "entity.licensetypename": filter.LicenseTypeName = filterItem.PropertyValue; break;
			}
		}
		filter.SortByName = request.SortColumn switch
		{
			"Entity.Name" => "Name",
			"Entity.Email" => "Email",
			"Entity.LicenseNumber" => "licensenumber",
			"Entity.LicenseTypeName" => "licensetypeName",
			_ => null
		};
		filter.SortDirection = request.SortDirection == SortDirection.Descending
			? System.ComponentModel.ListSortDirection.Descending
			: System.ComponentModel.ListSortDirection.Ascending;

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
		return GridItemsProviderResult<ViewModels.MemberRow>.From(
			rowList.Skip(request.StartIndex).Take(request.Count ?? rowList.Count).ToList(), rowList.Count);
	}

	async Task ApplyFilter()
	{
		await grid.ReloadAsync();
    }

	async Task ImportFile()
	{
		var uploadDialog = await DialogService.OpenAsync<Dialogs.ImportMemberExcelFileDialog>("Importer un fichier excel des membres");
		if (uploadDialog is null)
		{
			return;
		}

		var tempFileName = uploadDialog as string;
		if (string.IsNullOrWhiteSpace(tempFileName))
		{
			return;
		}

		try
		{
			await Mediator.Send(new ImportExcelMemberListRequest(tempFileName));
			await grid.ReloadAsync();
		}
		catch (Exception ex)
		{
			await NotificationService.Notify(NotificationSeverity.Error, $"La lecture de ce fichier a échoué pour la raison suivante : {ex.Message}");
		}
	}

    async Task EditRow(ViewModels.MemberRow item)
    {
		await grid.BeginEditAsync(item);
    }

    async Task SaveRow(ViewModels.MemberRow item)
    {
		brokenRules.Clear();

		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.MemberData>(item.Entity));
        if (saveResult.HasError)
        {
            brokenRules = saveResult.ErrorBrokenRuleList;
            return;
        }

		await grid.EndEditAsync(item);
		await grid.ReloadAsync();
	}

	async Task CancelEdit(ViewModels.MemberRow item)
	{
		await grid.CancelEditAsync(item);
		await grid.ReloadAsync();
    }

    async Task DeleteRow(ViewModels.MemberRow item)
    {
        var dialogResult = await DialogService.Confirm("Suppression", "Confirmez-vous la suppression de ce membre ?");
        if (!dialogResult)
        {
            return;
        }

        await Mediator.Send(new DeleteMemberRequest(item.Id));
		await grid.ReloadAsync();
    }
}
