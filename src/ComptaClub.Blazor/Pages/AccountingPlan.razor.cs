using ComptaClub.Models;
using ComptaClub.Blazor.Pages.Components;

using Microsoft.AspNetCore.Components;

using Radzen;
using Radzen.Blazor;
using Microsoft.Extensions.Azure;
using ComptaClub.Blazor.Extensions;

namespace ComptaClub.Blazor.Pages;

public partial class AccountingPlan : ComponentBase
{
    IEnumerable<ViewModels.Account> accounts = new List<ViewModels.Account>();
    RadzenDataGrid<ViewModels.Account>? grid;
    ViewModels.Account? accountToUpdate;
    ViewModels.Account? accountToInsert;
	List<Results.BrokenRule> brokenRules = new();

	[Inject]
	MediatR.IMediator? Mediator { get; set; }

	[Inject]
	AutoMapper.IMapper? Mapper { get; set; }

	protected override async Task OnInitializedAsync()
	{
		var request = new Requests.GetEntityPagedListRequest<Models.AccountListFilter, Datas.AccountData>(f =>
		{
			f.PageSize = int.MaxValue;
		});

        var datas = await Mediator!.Send(request);
		var list = new List<ViewModels.Account>();
        foreach (var data in datas.List)
        {
            var account = Mapper!.Map<ViewModels.Account>(data)!;
            account.Level = account.ParentAccountId == null ? 0 : -1;
            list.Add(account);
        }

        list.Levelize();
        list.Hierarchize();

		accounts = list;
    }

    void RowRender(RowRenderEventArgs<ViewModels.Account> args)
	{
		args.Expandable = args.Data.Children.Any();
	}

	void LoadChildData(DataGridLoadChildDataEventArgs<ViewModels.Account> args)
	{
		args.Data = args.Item.Children;
	}

	async Task EditRow(ViewModels.Account account)
	{
		accountToUpdate = account;
		await grid!.EditRow(account);
	}

	async Task SaveRow(ViewModels.Account account)
	{
		if (account == accountToInsert)
		{
			accountToInsert = null;
		}

		accountToUpdate = null;

		var data = Mapper!.Map<Datas.AccountData>(account);
		var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.AccountData>(data));
		if (saveResult!.HasError)
		{
			brokenRules = saveResult.ErrorBrokenRuleList;
			return;
		}

		await grid!.UpdateRow(account);
	}

	void CancelEdit(ViewModels.Account account)
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
		var data = await Mediator!.Send(new Requests.CreateAccountRequest());
        accountToInsert = Mapper!.Map<ViewModels.Account>(data);
        await grid!.InsertRow(accountToInsert);
	}

	async Task InsertRow(ViewModels.Account account)
	{
		var data = await Mediator!.Send(new Requests.CreateAccountRequest());
        accountToInsert = Mapper!.Map<ViewModels.Account>(data);
        account.Children.Add(accountToInsert);
		await grid!.SelectRow(accountToInsert);
		await grid!.EditRow(accountToInsert);
		await grid!.ExpandRow(account);
	}
}