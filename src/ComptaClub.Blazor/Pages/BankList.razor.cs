using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Shared;
using ComptaClub.Requests;

namespace ComptaClub.Blazor.Pages;

public partial class BankList : ComponentBase
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


    IEnumerable<ViewModels.Bank> bankList = new List<ViewModels.Bank>();
    RadzenDataGrid<ViewModels.Bank>? grid;
    ViewModels.Bank? bankToUpdate;
    ViewModels.Bank? bankToInsert;
    List<Results.BrokenRule> brokenRules = new();

    protected override async Task OnInitializedAsync()
    {
		MainLayout.AddToolbarItem(new ViewModels.Toolbar.ToolbarButton
		{
			OnClick = InsertRow,
			IconName = "add_circle_outline",
			Text = "Ajouter une banque",
			Disabled = (bankToUpdate != null || bankToInsert != null)
		}).Display();

		await LoadDatas();
    }

    async Task LoadDatas()
    {
        var datas = await Mediator.Send(new GetAllBanksRequest());
        var result = Mapper.Map<List<ViewModels.Bank>>(datas);
        bankList = result;
    }

    async Task InsertRow()
    {
        var data = await Mediator.Send(new Requests.CreateBankRequest());
        bankToInsert = Mapper.Map<ViewModels.Bank>(data);
        await grid!.InsertRow(bankToInsert);
    }

    void EditRow(ViewModels.Bank bank)
    {
        NavigationManager.NavigateTo($"/banque/edition/{bank.Id}");
    }

    async Task SaveRow(ViewModels.Bank bank)
    {
        if (bank == bankToInsert)
        {
            bankToInsert = null;
        }

        bankToUpdate = null;

        var data = Mapper!.Map<Datas.BankData>(bank);
        var saveResult = await Mediator.Send(new Requests.SaveEntityRequest<Datas.BankData>(data));
        if (saveResult.HasError)
        {
            brokenRules = saveResult.ErrorBrokenRuleList;
            return;
        }

        await grid!.UpdateRow(bank);
    }

    void CancelEdit(ViewModels.Bank bank)
    {
        if (bank == bankToInsert)
        {
            bankToInsert = null;
        }

        bankToUpdate = null;

        grid!.CancelEditRow(bank);
    }


    Task DeleteRow(ViewModels.Bank bank)
    {
        return Task.CompletedTask;
    }

    async Task ChangeActiveBank(ChangeEventArgs args, ViewModels.Bank bank)
    {
        var changeResult = await Mediator.Send(new ChangeActiveBankRequest($"{args.Value}" == "on", bank.Id));
        if (changeResult.HasError)
        {
            NotificationService.NotifyError(changeResult);
        }
        else if (changeResult.HasWarning)
        {
            NotificationService.NotifyWarning(changeResult);
        }
        await LoadDatas();
    }


}