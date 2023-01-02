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

    IEnumerable<ViewModels.Entry> entryList = new List<ViewModels.Entry>();
    RadzenDataGrid<ViewModels.Entry>? grid;

    async Task LoadDatas(LoadDataArgs args)
    {
        var datas = await Mediator!.Send(new GetEntityPagedListRequest<Models.EntryListFilter, Datas.EntryData>(f =>
        {
            f.PageSize = int.MaxValue;
        }));
        var page = Mapper.Map<Models.PagedList<IEnumerable<ViewModels.Entry>>>(datas);
        entryList = page.List;
    }

    void InsertRow()
    {
        NavigationManager.NavigateTo("/ecriture/ajout");
    }

    void EditRow(ViewModels.Entry entry)
    {
        NavigationManager.NavigateTo($"/ecriture/edition/{entry.Id}");
    }

    async Task DeleteRow(ViewModels.Entry entry)
    {

    }
}