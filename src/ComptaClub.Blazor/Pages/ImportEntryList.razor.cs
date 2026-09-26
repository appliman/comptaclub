

using ComptaClub.Blazor.Extensions;
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Blazor.ViewModels;
using ComptaClub.Configuration;
using ComptaClub.Contracts.Models.Entries;

using ChannelMediator;

using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using SuperBlazorComponents.Components.SuperDataGrid;

namespace ComptaClub.Blazor.Pages;

public partial class ImportEntryList : ComponentBase
{
    [CascadingParameter]
    Task<AuthenticationState> AuthenticationState { get; set; } = default!;

    [Inject]
    IMediator Mediator { get; set; } = default!;


    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    IEnumerable<ViewModels.EntryRow>? entryList;
    SuperDataGrid<ViewModels.EntryRow> grid = default!;

    ValueTask<GridItemsProviderResult<ViewModels.EntryRow>> LoadItems(GridItemsProviderRequest<ViewModels.EntryRow> request)
    {
        IEnumerable<ViewModels.EntryRow> rows = entryList ?? [];
        if (request.SortColumn is "RowIndex" or "CreationDate" or "PartNumber")
        {
            var descending = request.SortDirection == SortDirection.Descending;
            rows = request.SortColumn switch
            {
                "RowIndex" => descending ? rows.OrderByDescending(x => x.RowIndex) : rows.OrderBy(x => x.RowIndex),
                "CreationDate" => descending ? rows.OrderByDescending(x => x.Entity.CreationDate) : rows.OrderBy(x => x.Entity.CreationDate),
                _ => descending ? rows.OrderByDescending(x => x.Entity.PartNumber) : rows.OrderBy(x => x.Entity.PartNumber)
            };
        }

        var all = rows.ToList();
        var page = all.Skip(request.StartIndex).Take(request.Count ?? all.Count).ToList();
        return ValueTask.FromResult(GridItemsProviderResult<ViewModels.EntryRow>.From(page, all.Count));
    }

    protected override async Task OnInitializedAsync()
    {
        await Task.Yield();
        entryList = new List<EntryRow>();
    }

    async Task LoadFile(InputFileChangeEventArgs args)
    {
        using var ms = new MemoryStream();
        await args.File.OpenReadStream().CopyToAsync(ms);
        var dataList = await Mediator.Send(new ImportEntryListFromStreamRequest(ms));
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
        await grid.ReloadAsync();
    }

    async Task SaveRow(ViewModels.EntryRow entry)
    {
        var currentUser = (await AuthenticationState).User.GetUserInfos();
        entry.Entity.UserCreatorId = currentUser!.Id;
        var saveResult = await Mediator!.Send(new SaveEntityRequest<Datas.EntryData>(entry.Entity));
        if (saveResult!.HasError)
        {
            await NotificationService.NotifyError(saveResult);
            return;
        }
        (entryList as List<EntryRow>)!.Remove(entry);
        await grid.ReloadAsync();
        await NotificationService.Notify(NotificationSeverity.Success, "Sauvegarde", "Cette �criture est bien import�e");
        StateHasChanged();
    }


}
