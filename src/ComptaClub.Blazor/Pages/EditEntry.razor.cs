using System.Security.Claims;

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Dialogs;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Extensions;
using ComptaClub.Requests;

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ComptaClub.Blazor.Pages;

public partial class EditEntry : ComponentBase
{
    [CascadingParameter]
    Shared.MainLayout MainLayout { get; set; } = default!;

    [Parameter]
	public Guid? EntryId { get; set; }

	[Parameter]
	public string Direction { get; set; } = null!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	[Inject]
	MediatR.IMediator Mediator { get; set; } = default!;

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
	bool newEntry = true;
	int selectedTabIndex = 0;
	RadzenTabs tabs = default!;

	protected override async Task OnInitializedAsync()
	{
		if (EntryId == null
			|| EntryId == Guid.Empty)
		{
			entry = await Mediator.Send(new Requests.Entries.CreateEntryRequest());
			newEntry = true;
		}
		else
		{
			var data = await Mediator.Send(new Requests.Entries.GetEntryByFilterRequest(f => f.GetById(EntryId.Value)));
			if (data != null)
			{
				entry = data;
				newEntry = false;
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

		var bankList = await Mediator.Send(new Requests.Banks.GetAllBanksRequest());
		bankOptionList = bankList.ToSelectOptionList(k => k.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.BankId);
		if (entry.BankId == Guid.Empty
			&& bankOptionList.Any())
		{
			entry.BankId = bankList.First().Id;
			bankOptionList.First().Selected = true;
		}

		var accountList = await Mediator.Send(new Requests.Accounts.GetPlanRequest());
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

		var exercices = await Mediator.Send(new Requests.Exercices.GetAllExercicesRequest());
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

		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            IconName = "save",
            Text = "Sauvegarder",
            Title = "Sauvegarder les informations",
            OnClick = ValidateAndSave
        }).AddItem(new ViewModels.Toolbar.ToolbarButton
        {
            IconName = "person_add",
            Text = "Associer",
            Title = "Associer un membre à cette écriture",
			IsVisible = () => selectedTabIndex == 1,
            OnClick = async () =>
			{
				await associatedMembers.InsertRow();
			}
        }).AddItem(new ViewModels.Toolbar.ToolbarButton
		{
			IconName = "person_add",
			Text = "Associer",
			Title = "Associer un document à cette écriture",
			IsVisible = () => selectedTabIndex == 2,
			OnClick = async () =>
			{
				await associatedDocuments.InsertRow();
			}
		}).Display();
    }

    async Task ValidateAndSave()
	{
		entry.AccountDirection = direction;
		var data = Mapper.Map<Datas.EntryData>(entry);
		var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.EntryData>(data));
		if (saveResult!.HasError)
		{
			customValidator!.DisplayErrors(saveResult.ErrorBrokenRuleList);
			if (selectedTabIndex != 0)
			{
				NotificationService.NotifyError(saveResult);
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
		MainLayout.Toolbar.Refresh();
	}
}