using ComptaClub.Models;
using ComptaClub.Pages.Shared.Components;

using Microsoft.AspNetCore.Components;

using Radzen;
using Radzen.Blazor;

namespace ComptaClub.Pages;

public partial class AccountingPlan : ComponentBase
{
    IEnumerable<Models.Account> accounts = new List<Models.Account>();
    RadzenDataGrid<Models.Account>? grid;
	Models.Account? accountToUpdate;
	Models.Account? accountToInsert;
	List<BrokenRule> brokenRules = new();

	[Inject]
	Services.IAccountingService? AccountingService { get; set; }

	[Inject]
	MediatR.IMediator? Mediator { get; set; }

	protected override async Task OnInitializedAsync()
	{
		accounts = await AccountingService!.GetAccountingPlan();
	}

	void RowRender(RowRenderEventArgs<Models.Account> args)
	{
		args.Expandable = args.Data.Children.Any();
	}

	void LoadChildData(DataGridLoadChildDataEventArgs<Models.Account> args)
	{
		args.Data = args.Item.Children;
	}

	async Task EditRow(Models.Account account)
	{
		accountToUpdate = account;
		await grid!.EditRow(account);
	}

	async Task SaveRow(Models.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		var saveResult = await Mediator!.Send(new Requests.SaveEntity<Models.Account>(account));
		if (saveResult!.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(account);
	}

	void CancelEdit(Models.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		grid!.CancelEditRow(account);
	}

	async Task InsertRow()
	{
		accountToInsert = await Mediator!.Send(new Requests.CreateAccount());
		await grid!.InsertRow(accountToInsert);
	}

	async Task InsertRow(Models.Account account)
	{
		accountToInsert = await Mediator!.Send(new Requests.CreateAccount());
		account.Children.Add(accountToInsert);
		await grid!.SelectRow(accountToInsert);
		await grid!.EditRow(accountToInsert);
		await grid!.ExpandRow(account);
	}
}