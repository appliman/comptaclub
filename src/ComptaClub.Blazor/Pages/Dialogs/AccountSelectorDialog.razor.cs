
using ComptaClub.Contracts.Models.Accounts;

using ChannelMediator;
using SuperBlazorComponents.Components.SuperDataGrid;

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


    List<ViewModels.Account>? accountList;
    SuperDataGrid<ViewModels.Account> grid = default!;
    ViewModels.Account? selectedRow;

    protected override async Task OnInitializedAsync()
    {
        await LoadDatas();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await grid.ExpandAllAsync();
        }
    }

    async Task LoadDatas()
    {
        var datas = await Mediator!.Send(new GetPlanRequest());
        var list = MapPlan(datas);
        accountList = list;
    }

    async ValueTask<GridItemsProviderResult<ViewModels.Account>> LoadItems(GridItemsProviderRequest<ViewModels.Account> request)
    {
        if (accountList is null)
        {
            await LoadDatas();
        }

        var source = request.ParentItem?.Children ?? accountList ?? [];
        return GridItemsProviderResult<ViewModels.Account>.From(
            source.Skip(request.StartIndex).Take(request.Count ?? source.Count).ToList(), source.Count);
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
            var account = Mapping.Profile.ToViewModel(item);
            account.Children = MapPlan(item.Children);
            result.Add(account);
        }
        return result;
    }

    void OnSelectionChanged(IEnumerable<ViewModels.Account> selected)
    {
        var account = selected.FirstOrDefault();
        if (account is null)
        {
            selectedRow = null;
            return;
        }
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

    async Task Select()
    {
        if (selectedRow is not null)
        {
            await DialogService.Close(selectedRow);
        }
        else
        {
            await DialogService.Close();
        }
    }

}
