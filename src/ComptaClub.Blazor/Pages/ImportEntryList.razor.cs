
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
    [Inject]
    IMediator Mediator { get; set; } = default!;

    [Inject]
    IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    IEnumerable<ViewModels.Entry> entryList = new List<ViewModels.Entry>();
    RadzenDataGrid<ViewModels.Entry>? grid;
    List<ViewModels.Account> creditAccountOptionList = new();
    List<ViewModels.Account> debitAccountOptionList = new();

    protected override async Task OnInitializedAsync()
    {
        var accountList = await Mediator.Send(new Requests.GetPlanRequest());
        accountList = accountList.GetLeafList().ToList();
        accountList.Insert(0,new Datas.AccountData()
        {
            Id = ComptaClubSettings.ImportAccount,
            Code = "Import",
            Label = "Import",
            Direction = Datas.AccountDirection.Import
        });

        creditAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Datas.AccountDirection.Credit 
                                    || i.Direction == Datas.AccountDirection.Import)
                                    .ToList());

        debitAccountOptionList = Mapper.Map<List<ViewModels.Account>>(accountList.Where(i => i.Direction == Datas.AccountDirection.Debit 
                                    || i.Direction == Datas.AccountDirection.Import)
                                    .ToList());

    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var dataList = await Mediator.Send(new Requests.ImportEntryListFromStreamRequest(ms));
        entryList = Mapper.Map<IEnumerable<ViewModels.Entry>>(dataList);
        int rowIndex = 1;
        foreach (var item in entryList)
        {
            item.RowIndex = rowIndex++;
        }
    }

    async Task SaveRow(ViewModels.Entry entry)
    {
        var data = Mapper.Map<Datas.EntryData>(entry);
        var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.EntryData>(data));
        if (saveResult!.HasError)
        {
            NotificationService.NotifyError(saveResult);
            return;
        }
        NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Cette écriture est bien importée");
    }
}