using System.Security.Claims;

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Extensions;
using ComptaClub.Requests;

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ComptaClub.Blazor.Pages;

public partial class EditEntry : ComponentBase
{
	[CascadingParameter]
	Task<AuthenticationState> AuthenticationState { get; set; } = default!;

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
	DialogService DialogService { get; set; } = default!;


	ViewModels.Entry entry = new();
	CustomValidator? customValidator;
	List<SelectOption<Guid>> bankOptionList = new();
	List<SelectOption<Guid>> accountOptionList = new();
	List<SelectOption<Guid>> exerciceOptionList = new();
	Datas.AccountDirection direction;
    RadzenDataGrid<ViewModels.Member>? grid = new();
	List<ViewModels.Member> memberList = new();
    ViewModels.Member? memberToInsert;


    protected override async Task OnInitializedAsync()
	{
		if (EntryId == null
			|| EntryId == Guid.Empty)
		{
			var data = await Mediator.Send(new Requests.CreateEntryRequest());
			entry = Mapper.Map<ViewModels.Entry>(data);
		}
		else
		{
			var data = await Mediator.Send(new Requests.GetEntryByFilterRequest(f => f.GetById(EntryId.Value)));
			if (data != null)
			{
				entry = Mapper.Map<ViewModels.Entry>(data);
			}
		}

		if (Direction == "charge")
		{
			direction = Datas.AccountDirection.Debit;
		}
		else if (Direction == "produit")
		{
			direction = Datas.AccountDirection.Credit;
		}
		else
		{
			direction = entry.AccountDirection;
		}

		var bankList = await Mediator.Send(new Requests.GetAllBanksRequest());
		bankOptionList = bankList.ToSelectOptionList(k => k.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.BankId);
		if (entry.BankId == Guid.Empty
			&& bankOptionList.Any())
		{
			entry.BankId = bankList.First().Id;
			bankOptionList.First().Selected = true;
		}

		var accountList = await Mediator.Send(new Requests.GetPlanRequest());
		accountList = accountList.GetLeafList().ToList();
		if (direction == Datas.AccountDirection.Debit)
		{
			accountList.RemoveAll(i => i.Direction == Datas.AccountDirection.Credit);
		}
		else
		{
			accountList.RemoveAll(i => i.Direction == Datas.AccountDirection.Debit);
		}
		accountOptionList = accountList.ToSelectOptionList(i => i.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.AccountId);

		var exercices = await Mediator.Send(new GetAllExercicesRequest());
		exerciceOptionList = exercices.ToSelectOptionList(i => i.Id, t => $"({t.Code}) {t.Label}", i => i.Id == entry.ExerciceId);
		if (exercices.Any()
			&& entry.ExerciceId == Guid.Empty)
		{
			entry.ExerciceId = exercices.Single(i => i.Active).Id;
		}

		var userId = (await AuthenticationState).User.GetUserId();
		if (userId != null)
		{
			entry.UserCreatorId = userId!;
        }

		var dataList = await Mediator.Send(new GetMemberListByEntryRequest(entry.Id));
		memberList = Mapper.Map<List<Member>>(dataList);

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
            OnClick = InsertRow
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
			return;
		}

		foreach (var member in memberList)
		{
			await Mediator.Send(new LinkMemberToEntryRequest(entry.Id, member.Id));
		}

		NavigationManager.NavigateTo("/ecritures");
	}

    async Task InsertRow()
    {
		var result = await DialogService.OpenAsync<Dialogs.MemberSelectorDialog>("Selection d'un membre",
			options: new DialogOptions
			{
				CloseDialogOnEsc = true,
			});

		memberToInsert = result as ViewModels.Member;
        if (memberToInsert != null)
		{
            memberList.Add(memberToInsert);
			await grid!.InsertRow(memberToInsert);
        }
    }
}