using AutoMapper;

using ComptaClub.Contracts.Models.Documents;
using ComptaClub.Contracts.Models.Members;
using ComptaClub.Contracts.Results;

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
    List<BrokenRule> brokenRules = new();

    protected override void OnInitialized()
	{
		filter.PageSize = 500;
	}

	async Task LoadDatas(LoadDataArgs args)
	{
		if (args.Filters.Any())
		{
			foreach (var filterItem in args.Filters)
			{
				switch ($"{filterItem.Property}".ToLower())
				{
					case "entity.name":
						filter.Name = $"{filterItem.FilterValue}";
						break;
					case "entity.email":
						filter.Email = $"{filterItem.FilterValue}";
						break;
					case "entity.licensenumber":
						filter.LicenseNumber = $"{filterItem.FilterValue}";
						break;
					case "entity.licensetypename":
						filter.LicenseTypeName = $"{filterItem.FilterValue}";
						break;
				}
			}
		}
		if (args.Sorts.Any())
		{
			foreach (var sortItem in args.Sorts)
            {
				filter.SortDirection = sortItem.SortOrder == SortOrder.Ascending ? System.ComponentModel.ListSortDirection.Ascending : System.ComponentModel.ListSortDirection.Descending;

                switch ($"{sortItem.Property}".ToLower())
                {
                    case "entity.name":
                        filter.SortByName = "Name";
						break;
                    case "entity.email":
						filter.SortByName = "Email";
                        break;
                    case "entity.licensenumber":
						filter.SortByName = "licensenumber";
                        break;
                    case "entity.licensetypename":
						filter.SortByName = "licensetypeName";
                        break;
                }
            }
		}

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

	async Task ApplyFilter()
	{
		await grid!.Reload();
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
			await grid.Reload();
		}
		catch (Exception ex)
		{
			NotificationService.Notify(NotificationSeverity.Error, $"La lecture de ce fichier a échoué pour la raison suivante : {ex.Message}");
		}
	}

    async Task EditRow(ViewModels.MemberRow item)
    {
        await grid!.EditRow(item);
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

        await grid!.UpdateRow(item);
    }

    void CancelEdit(ViewModels.MemberRow item)
    {
        grid!.CancelEditRow(item);
    }

    async Task DeleteRow(ViewModels.MemberRow item)
    {
        var dialogResult = await DialogService.Confirm("Confirmez-vous la suppression de ce membre ?", "Suppression");
        if (!dialogResult.Value)
        {
            return;
        }

        await Mediator.Send(new DeleteMemberRequest(item.Id));
        await grid!.Reload();
    }
}