using System.Linq.Dynamic.Core;

using ComptaClub.Blazor.Services;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Datas;
using ComptaClub.Models;
using ComptaClub.Requests;

using ComptaClub.Requests.Accounts;

using Microsoft.AspNetCore.WebUtilities;

namespace ComptaClub.Blazor.Pages;

public partial class EntryList : ComponentBase
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

    [Inject]
    DialogService DialogService { get; set; } = default!;

    [Inject]
    ListFilterQueryStringParametersService ListFilterQueryStringParametersService { get; set; } = default!;

    List<ViewModels.EntryRow>? entryList;
    RadzenDataGrid<ViewModels.EntryRow>? grid = new();
    List<ViewModels.Account> leafAccountList = new();
    ViewModels.Exercice activeExercice = new();
    long currentBalance = 0;
    EntryListFilter filter = new();
    bool filterFirstInitialize = false;
    PeriodFilter? selectedPeriodFilter;

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            ListFilterQueryStringParametersService.AddFilterToQueryString(new FilterInfo(filter));
        }
    }

    protected override async Task OnInitializedAsync()
    {
        var taskList = new List<Task>();

        var t1 = Mediator.Send(new Requests.Exercices.GetActiveExerciceRequest());
        taskList.Add(t1);
        var t2 = Mediator.Send(new Requests.Accounts.GetPlanRequest());
        taskList.Add(t2);

        await Task.WhenAll(taskList);

        var exercice = t1.Result;
        if (exercice == null)
        {
            return;
        }
        activeExercice = Mapper.Map<ViewModels.Exercice>(exercice);
        currentBalance = activeExercice.BalanceAmount;

        var accountList = t2.Result;
        leafAccountList = Mapper.Map<List<ViewModels.Account>>(accountList.GetLeafList().ToList());

        InitializeFilter();

        MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            OnClick = () => InsertRow("charge"),
            IconName = "remove_circle_outline",
            Text = "Ajouter une dépense"
        }).AddItem(new ViewModels.Toolbar.ToolbarButton
        {
            OnClick = () => InsertRow("produit"),
            IconName = "add_circle_outline",
            Text = "Ajouter une rentrée"
        }).AddItem(new ViewModels.Toolbar.ToolbarLink
        {
            Url = "/importation-ecritures",
            IconName = "cloud_upload",
            Text = "Import"
        }).Display();

        StateHasChanged();
    }

    void InitializeFilter(bool bypass = false)
    {
        var filterInfo = ListFilterQueryStringParametersService.GetFilterInfoFromQueryString()
                    ?? new FilterInfo(filter);
        if (bypass)
        {
            filter = new();
            filterInfo = new FilterInfo(filter);
        }

        filter = (filterInfo.Filter as EntryListFilter) ?? new();
        filter.ExerciceId = activeExercice.Id;
        filter.PageSize = int.MaxValue;
        filterFirstInitialize = true;
    }

    async Task LoadDatas(LoadDataArgs args)
    {
        if (!filterFirstInitialize)
        {
            filter.Search = null;
            if (!args.Filters.IsNullOrEmpty())
            {
                var searchFilter = args.Filters.FirstOrDefault();
                if (searchFilter is not null
                    && searchFilter.Property == nameof(EntryData.PartNumber))
                {
                    filter.Search = $"{searchFilter.FilterValue}";
                }
            }
        }
        if (selectedPeriodFilter is not null)
        {
            filter.FromDayId = selectedPeriodFilter.FromDayId;
            filter.ToDayId = selectedPeriodFilter.ToDayId;
        }
        else
        {
            filter.FromDayId = null;
            filter.ToDayId = null;
        }

        var request = new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(filter);

        var dataPage = await Mediator.Send(request);
        var list = new List<EntryRow>();
        int rowId = 1;
        foreach (var item in dataPage.List)
        {
            list.Add(new EntryRow
            {
                Entity = item,
                RowIndex = rowId++,
            });
        }
        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            entryList = list.AsQueryable().OrderBy(args.OrderBy).ToList();
        }
        else
        {
            entryList = list;
        }
        filterFirstInitialize = false;
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

    Task EditRow(ViewModels.EntryRow entry)
    {
        QueryHelpers.ParseQuery(NavigationManager.ToAbsoluteUri(NavigationManager.Uri).Query)
			.TryGetValue("filter", out var filterValue);
		var editUrl = $"/ecriture/edition/{entry.Id}";
		editUrl = QueryHelpers.AddQueryString(editUrl, "filter", $"{filterValue}");
		NavigationManager.NavigateTo(editUrl);
        return Task.CompletedTask;
    }

    async Task DeleteRow(ViewModels.EntryRow entry)
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

    long? GetProfit()
    {
        if (entryList is null)
        {
            return 0;
        }
        var debit = entryList!.Where(i => i.Entity.AccountDirection == Enums.AccountDirection.Debit).Sum(i => i.Entity.Amount);
        var credit = entryList!.Where(i => i.Entity.AccountDirection == Enums.AccountDirection.Credit).Sum(i => i.Entity.Amount);

        return credit - debit;
    }
}