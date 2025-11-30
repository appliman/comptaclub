using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Blazor.Pages;

public partial class BankList : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;


	IEnumerable<ViewModels.BankRow>? bankList = null;
	RadzenDataGrid<ViewModels.BankRow> grid = default!;
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

	async Task InsertRow()
	{
		var data = await Mediator.Send(new CreateBankRequest());
		var bankToInsert = new ViewModels.BankRow
		{
			Entity = data,
			RowIndex = bankList!.Count() + 1
		};
		await grid.InsertRow(bankToInsert);
	}

	void EditRow(ViewModels.BankRow bank)
	{
		grid.EditRow(bank);
	}

	async Task SaveRow(ViewModels.BankRow bank)
	{
		var saveResult = await Mediator.Send(new SaveEntityRequest<Datas.BankData>(bank.Entity));
		if (saveResult.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid.UpdateRow(bank);
	}

	void CancelEdit(ViewModels.BankRow bank)
	{
		grid!.CancelEditRow(bank);
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
			NotificationService.NotifyError(changeResult);
		}
		else if (changeResult.HasWarning)
		{
			NotificationService.NotifyWarning(changeResult);
		}
		await LoadDatas();
	}


}