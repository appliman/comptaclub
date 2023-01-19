using ComptaClub.Blazor.Extensions;
using ComptaClub.Configuration;
using ComptaClub.Handlers;
using ComptaClub.Requests;

namespace ComptaClub.Blazor.Pages;

public partial class EntryList : ComponentBase
{
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

    IEnumerable<ViewModels.Entry>? entryList;
    RadzenDataGrid<ViewModels.Entry>? grid;
    List<ViewModels.Account> creditAccountOptionList = new();
    List<ViewModels.Account> debitAccountOptionList = new();
    ViewModels.Exercice activeExercice = new();


    protected override async Task OnInitializedAsync()
    {
        var exercice = await Mediator.Send(new GetActiveExerciceRequest());
        activeExercice = Mapper.Map<ViewModels.Exercice>(exercice);

        var accountList = await Mediator.Send(new Requests.GetPlanRequest());
        accountList = accountList.GetLeafList().ToList();
        creditAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Datas.AccountDirection.Credit
                                    || i.Direction == Datas.AccountDirection.Import)
                                    .ToList());

        debitAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Datas.AccountDirection.Debit
                                    || i.Direction == Datas.AccountDirection.Import)
                                    .ToList());

    }

    async Task LoadDatas(LoadDataArgs args)
    {
        var request = new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(f =>
        {
            f.ExerciceId = activeExercice.Id;
            f.PageSize = int.MaxValue;
        });

		var dataPage = await Mediator!.Send(request);
        var list = Mapper.Map<IEnumerable<ViewModels.Entry>>(dataPage.List);
        int rowIndex = dataPage.List.Count();
        var balance = activeExercice.BalanceAmount;
        foreach (var item in list.OrderByDescending(i => i.CreationDate))
        {
            item.RowIndex = rowIndex--;
            item.Balance = balance;
            balance = balance + (item.Amount * (int)item.AccountDirection);
        }
        entryList = list;
    }

    void InsertRow(string direction)
    {
        NavigationManager.NavigateTo($"/ecriture/ajout/{direction}");
    }

    void EditRow(ViewModels.Entry entry)
    {
        NavigationManager.NavigateTo($"/ecriture/edition/{entry.Id}");
    }

    async Task DeleteRow(ViewModels.Entry entry)
    {
        var confirm = await DialogService.Confirm("Confirmez-vous la suppression de cette écriture", "Suppression");
        if (!confirm.HasValue || !confirm.Value)
        {
            return;
        }

        var deleteResult = await Mediator.Send(new DeleteEntryRequest(entry.Id));
        if (deleteResult.HasError)
        {
            NotificationService.NotifyError(deleteResult);
            return;
        }

        NotificationService.Notify(NotificationSeverity.Success, "L'ecriture est maintenant supprimée");
        await grid!.Reload();
    }

    void Import()
    {
        NavigationManager.NavigateTo("/importation-ecritures");
    }
}