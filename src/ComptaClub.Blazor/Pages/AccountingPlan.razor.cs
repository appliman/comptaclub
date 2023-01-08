using ComptaClub.Models;
using ComptaClub.Blazor.Pages.Components;

using Microsoft.AspNetCore.Components;

using Radzen;
using Radzen.Blazor;
using Microsoft.Extensions.Azure;
using ComptaClub.Blazor.Extensions;
using ComptaClub.Requests;
using ComptaClub.Blazor.Pages.Shared;
using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Mvc.Localization;
using System.Collections.Generic;

namespace ComptaClub.Blazor.Pages;

public partial class AccountingPlan : ComponentBase
{
	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

    [Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

    [Inject]
	NotificationService NotificationService { get; set; } = default!;

	[Inject]
	DialogService DialogService { get; set; } = default!;


    IEnumerable<ViewModels.Account> accountList = new List<ViewModels.Account>();
    RadzenDataGrid<ViewModels.Account>? grid;
    ViewModels.Account? accountToUpdate;
    ViewModels.Account? accountToInsert;
    List<Results.BrokenRule> brokenRules = new();

    protected override async Task OnInitializedAsync()
	{
		await LoadDatas();
    }

	async Task LoadDatas()
	{
        var dataPlan = await Mediator!.Send(new GetPlanRequest());
        var list = MapPlan(dataPlan);
        accountList = list;
    }

    List<ViewModels.Account> MapPlan(List<Datas.AccountData> list)
	{
        var result = new List<ViewModels.Account>();
		foreach (var item in list)
		{
			var account = Mapper.Map<ViewModels.Account>(item);
            account.Children = MapPlan(item.Children);
			result.Add(account);
		}
		return result;
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
		var data = await Mediator!.Send(new Requests.CreateAccountRequest() 
		{ 
			Direction = (Datas.AccountDirection)account.Direction,
			ParentId = account.Id
		});
        accountToInsert = Mapper!.Map<ViewModels.Account>(data);
        account.Children.Add(accountToInsert);
		await grid!.SelectRow(accountToInsert);
		await grid!.EditRow(accountToInsert);
		await grid!.ExpandRow(account);
	}

    async Task DeleteRow(ViewModels.Account account)
    {
		var confirm = await DialogService.Confirm("Confirmez vous la suppression de ce compte", "Suppression");
        if (!confirm.GetValueOrDefault(false))
		{
			return;
		}
        var result = await Mediator!.Send(new Requests.DeleteAccountRequest(account.Id));
		if (result.HasError)
		{
			NotificationService.NotifyError(result);
		}
		else
		{
            await LoadDatas();
            await grid!.Reload();
			NotificationService.Notify(new NotificationMessage
			{
				Severity = NotificationSeverity.Info,
				Summary = $"Ce compte ({account.Code}) vient d'etre supprimé"
            });
		}
    }


}