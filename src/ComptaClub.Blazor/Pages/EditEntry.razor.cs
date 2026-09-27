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
	List<Datas.AccountData> _accountTree = new();
	List<SelectOption<Guid>> exerciceOptionList = new();
	string? _dateRangeLabel;
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
		_accountTree = FilterAccountTree(accountList);

		var exercices = await Mediator.Send(new GetAllExercicesRequest());
		exerciceOptionList = exercices.ToSelectOptionList(i => i.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.ExerciceId);
		if (exercices.Any()
			&& entry.ExerciceId == Guid.Empty)
		{
			entry.ExerciceId = exercices.Single(i => i.Active).Id;
		}
		var _selectedExercice = exercices.FirstOrDefault(i => i.Id == entry.ExerciceId);
		if (_selectedExercice is not null)
		{
			_dateRangeLabel = $"(du {_selectedExercice.StartDate.FromDayId():dd/MM/yy} au {_selectedExercice.EndDate.FromDayId():dd/MM/yy})";
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

	List<Datas.AccountData> FilterAccountTree(IEnumerable<Datas.AccountData> accounts)
	{
		var _result = new List<Datas.AccountData>();
		foreach (var _account in accounts)
		{
			var _children = FilterAccountTree(_account.Children);
			if (_account.Children.Count > 0 && _children.Count == 0)
			{
				continue;
			}

			if (_account.Children.Count == 0 &&
				((direction == Enums.AccountDirection.Debit && _account.Direction == Enums.AccountDirection.Credit) ||
				 (direction == Enums.AccountDirection.Credit && _account.Direction == Enums.AccountDirection.Debit)))
			{
				continue;
			}

			var _filteredAccount = (Datas.AccountData)_account.Clone();
			_filteredAccount.Children = _children;
			_result.Add(_filteredAccount);
		}
		return _result;
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
