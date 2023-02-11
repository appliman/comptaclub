using ComptaClub.Blazor.Extensions;
using ComptaClub.Configuration;
using ComptaClub.Handlers;
using ComptaClub.Requests;

using System.Linq.Dynamic.Core;

using Microsoft.AspNetCore.Components.Routing;
using ComptaClub.Models;

namespace ComptaClub.Blazor.Pages;

public partial class EntryList : ComponentBase
{
    [CascadingParameter]
    Shared.MainLayout MainLayout { get; set; } = default!;

    [Inject]
    MediatR.IMediator Mediator { get; set; } = default!;

    [Inject]
    AutoMapper.IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    DialogService DialogService { get; set; } = default!;

    List<ViewModels.Entry>? entryList;
    RadzenDataGrid<ViewModels.Entry>? grid = new();
    List<ViewModels.Account> leafAccountList = new();
    ViewModels.Exercice activeExercice = new();
    decimal currentBalance = 0;
    EntryListFilter filter = new();


    protected override async Task OnInitializedAsync()
    {
        var exercice = await Mediator.Send(new Requests.Exercices.GetActiveExerciceRequest());
        if (exercice == null)
        {
            return;
        }
        activeExercice = Mapper.Map<ViewModels.Exercice>(exercice);
        currentBalance = activeExercice.BalanceAmount;

        var accountList = await Mediator.Send(new Requests.Accounts.GetPlanRequest());
        leafAccountList = Mapper.Map<List<ViewModels.Account>>(accountList.GetLeafList().ToList());

        MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            OnClick = () => InsertRow("charge"),
            IconName = "remove_circle_outline",
            Text = "Ajouter une dépense"
        }).AddItem(new ViewModels.Toolbar.ToolbarButton
        {
            OnClick = () => InsertRow("charge"),
            IconName = "add_circle_outline",
            Text = "Ajouter une rentrée"
        }).AddItem(new ViewModels.Toolbar.ToolbarLink
        {
            Url = "/importation-ecritures",
            IconName = "cloud_upload",
            Text = "Import"
        }).Display();

        filter.ExerciceId = activeExercice.Id;
        filter.PageSize = int.MaxValue;
    }

    async Task LoadDatas(LoadDataArgs args)
    {
        var request = new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(filter);

		var dataPage = await Mediator!.Send(request);
        var list = Mapper.Map<List<ViewModels.Entry>>(dataPage.List);
        int rowIndex = dataPage.List.Count();
        var balance = currentBalance;
        foreach (var item in list.OrderByDescending(i => i.CreationDate))
        {
            item.RowIndex = rowIndex--;
            item.Balance = balance;
            balance = balance - (item.Amount * (int)item.AccountDirection);
        }
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            entryList = list.AsQueryable().OrderBy(args.OrderBy).ToList();
        }
        else
        {
            entryList = list;
        }
    }

    async Task ApplyFilter()
    {
        filter.PageIndex = 0;
        grid!.Reset(true, true);
        if (grid.CurrentPage == 0)
        {
            await grid.Reload();
        }
        else
        {
            await grid.GoToPage(0);
        }
        StateHasChanged();

    }

    Task InsertRow(string direction)
    {
        NavigationManager.NavigateTo($"/ecriture/ajout/{direction}");
        return Task.CompletedTask;
    }

    Task EditRow(ViewModels.Entry entry)
    {
        NavigationManager.NavigateTo($"/ecriture/edition/{entry.Id}");
        return Task.CompletedTask;
    }

    async Task DeleteRow(ViewModels.Entry entry)
    {
        var confirm = await DialogService.Confirm("Confirmez-vous la suppression de cette écriture", "Suppression");
        if (!confirm.HasValue || !confirm.Value)
        {
            return;
        }

        var deleteResult = await Mediator.Send(new Requests.Entries.DeleteEntryRequest(entry.Id));
        if (deleteResult.HasError)
        {
            NotificationService.NotifyError(deleteResult);
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "L'ecriture est maintenant supprimée");
        await grid!.Reload();
    }

}