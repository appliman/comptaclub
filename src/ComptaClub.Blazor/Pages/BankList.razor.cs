using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Results;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class BankList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;


	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	List<ViewModels.BankRow>? bankList;
	SuperDataGrid<ViewModels.BankRow> grid = default!;
	ViewModels.BankRow? pendingBank;
	List<BrokenRule> brokenRules = new();

	async Task LoadDatas()
	{
		var datas = await Mediator.Send(new GetAllBanksRequest());
		int rowIndex = 1;
		var rowList = new List<ViewModels.BankRow>();
		foreach (var item in datas)
		{
			rowList.Add(new ViewModels.BankRow
			{
				Entity = item,
				RowIndex = rowIndex++
			});
		}
		bankList = rowList;
	}

	async ValueTask<GridItemsProviderResult<ViewModels.BankRow>> LoadItems(GridItemsProviderRequest<ViewModels.BankRow> request)
	{
		if (bankList is null)
        {
            await LoadDatas();
        }

        IEnumerable<ViewModels.BankRow> rows = bankList ?? [];
		foreach (var filter in request.Filters)
		{
			if (string.IsNullOrWhiteSpace(filter.PropertyValue))
            {
                continue;
            }

            rows = filter.PropertyName switch
			{
				"Entity.Code" => rows.Where(x => x.Entity.Code?.Contains(filter.PropertyValue, StringComparison.OrdinalIgnoreCase) == true),
				"Entity.Label" => rows.Where(x => x.Entity.Label?.Contains(filter.PropertyValue, StringComparison.OrdinalIgnoreCase) == true),
				_ => rows
			};
		}
		var descending = request.SortDirection == SortDirection.Descending;
		rows = request.SortColumn switch
		{
			"Entity.Code" => descending ? rows.OrderByDescending(x => x.Entity.Code) : rows.OrderBy(x => x.Entity.Code),
			"Entity.Label" => descending ? rows.OrderByDescending(x => x.Entity.Label) : rows.OrderBy(x => x.Entity.Label),
			_ => rows.OrderBy(x => x.RowIndex)
		};
		var items = rows.ToList();
		return GridItemsProviderResult<ViewModels.BankRow>.From(items.Skip(request.StartIndex).Take(request.Count ?? items.Count).ToList(), items.Count);
	}

	async Task InsertRow()
	{
		var data = await Mediator.Send(new CreateBankRequest());
		var bankToInsert = new ViewModels.BankRow
		{
			Entity = data,
			RowIndex = (bankList?.Count ?? 0) + 1
		};
		pendingBank = bankToInsert;
		bankList ??= [];
		bankList.Insert(0, bankToInsert);
		await grid.ReloadAsync();
		await grid.BeginEditAsync(bankToInsert);
	}

	Task EditRow(ViewModels.BankRow bank)
	{
		return grid.BeginEditAsync(bank);
	}

	async Task SaveRow(ViewModels.BankRow bank)
	{
		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.BankData>(bank.Entity));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid.EndEditAsync(bank);
		pendingBank = null;
		await LoadDatas();
		await grid.ReloadAsync();
	}

	async Task CancelEdit(ViewModels.BankRow bank)
	{
		await grid.CancelEditAsync(bank);
		if (ReferenceEquals(pendingBank, bank))
		{
			bankList?.Remove(bank);
			pendingBank = null;
		}
		else
        {
            await LoadDatas();
        }

        await grid.ReloadAsync();
	}

	Task DeleteRow(ViewModels.BankRow bank)
	{
		return Task.CompletedTask;
	}

	async Task ChangeActiveBank(ChangeEventArgs args, ViewModels.BankRow bank)
	{
		var changeResult = await Mediator.Send(new ChangeActiveBankRequest($"{args.Value}" == "on", bank.Id));
		if (changeResult.HasError)
		{
			await NotificationService.NotifyError(changeResult);
		}
		else if (changeResult.HasWarning)
		{
			await NotificationService.NotifyWarning(changeResult);
		}
		await LoadDatas();
		await grid.ReloadAsync();
	}


}
