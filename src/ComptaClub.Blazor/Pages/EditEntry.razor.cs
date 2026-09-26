using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Contracts.Models.Accounts;
using ComptaClub.Contracts.Models.Banks;
using ComptaClub.Contracts.Models.Entries;
using ComptaClub.Contracts.Models.Exercices;

using Microsoft.AspNetCore.WebUtilities;
using SuperBlazorComponents.Components.SuperTabs;

namespace ComptaClub.Blazor.Pages;

public partial class EditEntry : ComponentBase
{
	[CascadingParameter]
	MainLayout MainLayout { get; set; } = default!;

	[Parameter]
	public Guid? EntryId { get; set; }

	[Parameter]
	public string Direction { get; set; } = null!;


	[Inject]
	ChannelMediator.IMediator Mediator { get; set; } = default!;

	[Inject]
	NavigationManager NavigationManager { get; set; } = default!;

	[Inject]
	NotificationService NotificationService { get; set; } = default!;


	Datas.EntryData entry = new();
	CustomValidator? customValidator;
	List<SelectOption<Guid>> bankOptionList = new();
	List<SelectOption<Guid>> accountOptionList = new();
	List<SelectOption<Guid>> exerciceOptionList = new();
	Enums.AccountDirection direction;
	AssociatedMemberByEntry associatedMembers = default!;
	DocumentListByEntity associatedDocuments = default!;
	int selectedTabIndex = 0;
	SuperTabs tabs = default!;

	protected override async Task OnInitializedAsync()
	{
		if (EntryId == null
			|| EntryId == Guid.Empty)
		{
			entry = await Mediator.Send(new CreateEntryRequest());
		}
		else
		{
			var data = await Mediator.Send(new GetEntryByFilterRequest(f => f.GetById(EntryId.Value)));
			if (data != null)
			{
				entry = data;
			}
		}

		if (Direction == "charge")
		{
			direction = Enums.AccountDirection.Debit;
		}
		else if (Direction == "produit")
		{
			direction = Enums.AccountDirection.Credit;
		}
		else
		{
			direction = entry.AccountDirection;
		}

		var bankList = await Mediator.Send(new GetAllBanksRequest());
		bankOptionList = bankList.ToSelectOptionList(k => k.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.BankId);
		if (entry.BankId == Guid.Empty
			&& bankOptionList.Any())
		{
			entry.BankId = bankList.First().Id;
			bankOptionList.First().Selected = true;
		}

		var accountList = await Mediator.Send(new GetPlanRequest());
		accountList = accountList.GetLeafList().ToList();
		if (direction == Enums.AccountDirection.Debit)
		{
			accountList.RemoveAll(i => i.Direction == Enums.AccountDirection.Credit);
		}
		else
		{
			accountList.RemoveAll(i => i.Direction == Enums.AccountDirection.Debit);
		}
		accountOptionList = accountList.ToSelectOptionList(i => i.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.AccountId);

		var exercices = await Mediator.Send(new GetAllExercicesRequest());
		exerciceOptionList = exercices.ToSelectOptionList(i => i.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.ExerciceId);
		if (exercices.Any()
			&& entry.ExerciceId == Guid.Empty)
		{
			entry.ExerciceId = exercices.Single(i => i.Active).Id;
		}

		var user = MainLayout.GetCurrentUser();
		if (user != null)
		{
			entry.UserCreatorId = user.Id!;
		}
	}

	async Task ValidateAndSave()
	{
		customValidator?.Reset();
		entry.AccountDirection = direction;
		entry.ValueDate = entry.CreationDate;
		var saveResult = await Mediator!.Send(new SaveEntityRequest<Datas.EntryData>(entry));
		if (saveResult!.HasError)
		{
			customValidator!.DisplayErrors(saveResult.ErrorBrokenRuleList);
			if (selectedTabIndex != 0)
			{
				await NotificationService.NotifyError(saveResult);
			}
			return;
		}

		if (associatedMembers is not null)
		{
			await associatedMembers.SaveAssociations();
		}

		NavigationManager.NavigateTo("/ecritures");
	}

	void RefreshToolbar(int tabId)
	{
		selectedTabIndex = tabId;
	}

	string GetEntriesUrl()
	{
		var url = "/ecritures";
		if (QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query)
			.TryGetValue("filter", out var filterValue))
		{
			url = QueryHelpers.AddQueryString(url, "filter", $"{filterValue}");
		}
		return url;



	}
}
