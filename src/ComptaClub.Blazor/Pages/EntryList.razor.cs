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

    protected override async Task OnInitializedAsync()
    {
        await LoadDatas();
    }

    async Task LoadDatas()
    {
        var request = new GetPagedEntityListRequest<Models.EntryListFilter, Datas.EntryData>(f =>
        {
            f.PageSize = int.MaxValue;
        });

		var dataPage = await Mediator!.Send(request);
        var list = Mapper.Map<IEnumerable<ViewModels.Entry>>(dataPage.List);
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

    }

    void Import()
    {
        NavigationManager.NavigateTo("/importation-ecritures");
    }
}