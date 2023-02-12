
using AutoMapper;

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Configuration;

using MediatR;

using Microsoft.AspNetCore.Mvc.Razor.Compilation;

namespace ComptaClub.Blazor.Pages;

public partial class ImportEntryList : ComponentBase
{
    [CascadingParameter]
    Task<AuthenticationState> AuthenticationState { get; set; } = default!;

    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    IEnumerable<ViewModels.Entry> entryList = new List<ViewModels.Entry>();
    RadzenDataGrid<ViewModels.Entry>? grid = default!;
    List<Datas.AccountData> accountList = new();
    List<ViewModels.Account>? creditAccountOptionList;
    List<ViewModels.Account>? debitAccountOptionList;

    protected override async Task OnInitializedAsync()
    {
        accountList = await Mediator.Send(new Requests.Accounts.GetPlanRequest());
        accountList = accountList.GetLeafList().ToList();
        accountList.Insert(0,new Datas.AccountData()
        {
            Id = ComptaClubSettings.ImportAccount,
            Code = "Import",
            Label = "Import",
            Direction = Enums.AccountDirection.Import
        });
    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var dataList = await Mediator.Send(new Requests.Entries.ImportEntryListFromStreamRequest(ms));
        entryList = Mapper.Map<IEnumerable<ViewModels.Entry>>(dataList);
        int rowIndex = 1;
        foreach (var item in entryList)
        {
            item.RowIndex = rowIndex++;
        }
    }

    async Task SaveRow(ViewModels.Entry entry)
    {
        var currentUser = (await AuthenticationState).User.GetUserInfos();
        entry.UserCreatorId = currentUser!.Id;
        var data = Mapper.Map<Datas.EntryData>(entry);
        var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.EntryData>(data));
        if (saveResult!.HasError)
        {
            NotificationService.NotifyError(saveResult);
            return;
        }
        NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Cette écriture est bien importée");
    }

    void LoadDebitAccountList(LoadDataArgs args)
    {
        debitAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Enums.AccountDirection.Debit
                            || i.Direction == Enums.AccountDirection.Import)
                            .ToList());

        if (!string.IsNullOrWhiteSpace(args.Filter))
        {
            debitAccountOptionList = (from account in debitAccountOptionList
                                      where account.CodeAndLabel.IndexOf(args.Filter, StringComparison.InvariantCultureIgnoreCase) != -1
                                       select account).ToList();
        }
        InvokeAsync(StateHasChanged);
    }

    void LoadCreditAccountList(LoadDataArgs args)
    {
        creditAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Enums.AccountDirection.Credit
                                    || i.Direction == Enums.AccountDirection.Import)
                                    .ToList());

        if (!string.IsNullOrWhiteSpace(args.Filter))
        {
            creditAccountOptionList = (from account in creditAccountOptionList
                                       where account.CodeAndLabel.IndexOf(args.Filter, StringComparison.InvariantCultureIgnoreCase) != -1
                                      select account).ToList();
        }
        InvokeAsync(StateHasChanged);
    }
}