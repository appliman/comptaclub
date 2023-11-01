
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

    IEnumerable<ViewModels.EntryRow>? entryList;
    RadzenDataGrid<ViewModels.EntryRow> grid = default!;

    protected override async Task OnInitializedAsync()
    {
        await Task.Yield();
        entryList = new List<EntryRow>();
    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        using var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var dataList = await Mediator.Send(new Requests.Entries.ImportEntryListFromStreamRequest(ms));
        int rowIndex = 1;
        var list = new List<EntryRow>();
        foreach (var item in dataList)
        {
            list.Add(new EntryRow
            {
                Entity = item,
				RowIndex = rowIndex++
            });
        }
        entryList = list;
    }

    async Task SaveRow(ViewModels.EntryRow entry)
    {
        var currentUser = (await AuthenticationState).User.GetUserInfos();
        entry.Entity.UserCreatorId = currentUser!.Id;
        var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.EntryData>(entry.Entity));
        if (saveResult!.HasError)
        {
            NotificationService.NotifyError(saveResult);
            return;
        }
        (entryList as List<EntryRow>)!.Remove(entry);
        await grid.Reload();
        NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Cette écriture est bien importée");
        StateHasChanged();
    }


}