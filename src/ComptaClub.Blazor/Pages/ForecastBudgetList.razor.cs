using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Datas;
using ComptaClub.Requests;
using ComptaClub.Requests.ForecastBudget;

namespace ComptaClub.Blazor.Pages;

public partial class ForecastBudgetList : ComponentBase
{
    [CascadingParameter]
    MainLayout MainLayout { get; set; } = default!;

    [Inject]
    MediatR.IMediator Mediator { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    DialogService DialogService { get; set; } = default!;


    List<ForecastBudgetRow>? forecastBudgetList;
    RadzenDataGrid<ForecastBudgetRow>? grid;
    IList<ForecastBudgetRow>? selectedRow;

    protected override async Task OnInitializedAsync()
    {
        MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
        {
            IconName = "add_circle_outline",
            Text = "Creer un bilan prévisionnel",
            OnClick = CreateForecastBudget
        })
        .Display();

        await LoadDatas();
    }

    async Task LoadDatas()
    {
        var filter = new ForecastBudgetListFilter();
        filter.SortDirection = System.ComponentModel.ListSortDirection.Descending;
        filter.SortByName = "CreationDate";

        var page = await Mediator.Send(new GetPagedEntityListRequest<ForecastBudgetListFilter, ForecastBudgetData>(filter));
        var list = new List<ForecastBudgetRow>();
        var rowIndex = 1;
        foreach (var item in page.List)
        {
            list.Add(new ForecastBudgetRow
            {
                Entity = item,
                RowIndex = rowIndex++
            });
        }
        forecastBudgetList = list;
    }

    async Task DeleteRow(ForecastBudgetData item)
    {
        var dialog = await DialogService.Confirm("Confirmez-vous la suppression de ce bilan prévisionnel ?", 
            "Suppression bilan prévisionnel", 
            new ConfirmOptions
        {
            OkButtonText = "Supprimer",
            CancelButtonText = "Annuler"
        });

        if (dialog == false)
        {
            return;
        }

        var deleteResult = await Mediator.Send(new DeleteForecastBudgetRequest(item.Id));
        if (deleteResult.HasError)
        {
            NotificationService.NotifyError(deleteResult);
            return;
        }

        await LoadDatas();
    }

    async Task CreateForecastBudget()
    {
        var dialog = await DialogService.OpenAsync<Dialogs.IncomeStatementSelectorDialog>("Selection d'un compte de résultat",
            options: new DialogOptions
            {
                CloseDialogOnEsc = true,
            });

        var incomeStatement = dialog as ViewModels.IncomeStatement;
        if (incomeStatement is null)
        {
            return;
        }

        var createResult = await Mediator.Send(new CreateAndSaveForecastBudgetRequest(incomeStatement.Id,
            "Bilan prévisionnel",
            incomeStatement.Description));

        if (createResult.HasError)
        {
            NotificationService.NotifyError(createResult);
            return;
        }

        NavigationManager.NavigateTo($"/bilan-previsionnel/{createResult.Id}");
    }
}
