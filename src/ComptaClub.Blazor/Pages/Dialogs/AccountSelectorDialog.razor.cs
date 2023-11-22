using AutoMapper;

using ComptaClub.Contracts.Models.Accounts;

using MediatR;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class AccountSelectorDialog
{
    [Parameter]
    public AccountDirection AccountDirection { get; set; } = AccountDirection.Import;

    [Parameter]
    public Guid? AccountId { get; set; }

    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    public DialogService DialogService { get; set; } = default!;

    [Inject]
    IMapper Mapper { get; set; } = default!;

    List<ViewModels.Account>? accountList;
    RadzenDataGrid<ViewModels.Account> grid = default!;
    ViewModels.Account? selectedRow;

    protected override async Task OnInitializedAsync()
    {
        await LoadDatas();
    }

    async Task LoadDatas()
    {
        var datas = await Mediator!.Send(new GetPlanRequest());
        var list = MapPlan(datas);
        accountList = list;
        ExpandAll(accountList);
    }

    void ExpandAll(IEnumerable<ViewModels.Account> accountList)
    { 
        foreach (var item in accountList)
        {
            grid.ExpandRow(item);
            ExpandAll(item.Children);
        }
    }

    List<ViewModels.Account> MapPlan(List<Datas.AccountData> list)
    {
        var result = new List<ViewModels.Account>();
        foreach (var item in list)
        {
            if (item.Direction != AccountDirection)
            {
                continue;
            }
            var account = Mapper.Map<ViewModels.Account>(item);
            account.Children = MapPlan(item.Children);
            result.Add(account);
        }
        return result;
    }

    void LoadChildData(DataGridLoadChildDataEventArgs<ViewModels.Account> args)
    {
        args.Data = args.Item.Children;
    }

    void AccountSelected(ViewModels.Account account)
    {
        if (account.Children is null
            || account.Children.Count == 0)
        {
            // Selection ok
            selectedRow = account;
        }
        else
        {
            selectedRow = null;
        }
    }

    Task Select()
    {
        if (selectedRow is not null)
        {
            DialogService.Close(selectedRow);
        }
        else
        {
            DialogService.Close();
        }
        return Task.CompletedTask;
    }

    void RowRender(RowRenderEventArgs<ViewModels.Account> args)
    {
        args.Expandable = args.Data.Children.Any();
    }

}