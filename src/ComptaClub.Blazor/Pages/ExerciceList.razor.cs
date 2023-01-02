using ComptaClub.Requests;

using Radzen;
using Radzen.Blazor;

namespace ComptaClub.Blazor.Pages;

public partial class ExerciceList : ComponentBase
{
    [Inject]
    MediatR.IMediator Mediator { get; set; } = default!;

    [Inject]
    AutoMapper.IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    [Inject]
    NavigationManager NavigationManager { get; set; } = default!;


	List<ViewModels.Exercice> exerciceList = new();
    RadzenDataGrid<ViewModels.Exercice>? grid;
    List<Results.BrokenRule> brokenRules = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadDatas();
    }

    async Task LoadDatas()
    {
        var datas = await Mediator!.Send(new GetAllExercicesRequest());
        exerciceList = Mapper.Map<List<ViewModels.Exercice>>(datas);
        StateHasChanged();
    }

    void InsertRow()
    {
        NavigationManager.NavigateTo("/exercice/ajout");
    }

    void EditRow(ViewModels.Exercice exercice)
    {
		NavigationManager.NavigateTo($"/exercice/edition/{exercice.Id}");
    }

    async Task DeleteRow(ViewModels.Exercice exercice)
    {
        
    }

}