using ComptaClub.Requests;

using Radzen;
using Radzen.Blazor;

namespace ComptaClub.Blazor.Pages;

public partial class Exercices : ComponentBase
{
    [Inject]
    MediatR.IMediator Mediator { get; set; } = default!;

    [Inject]
    AutoMapper.IMapper Mapper { get; set; } = default!;

    [Inject]
    NotificationService NotificationService { get; set; } = default!;

    IEnumerable<ViewModels.Exercice> exerciceList = new List<ViewModels.Exercice>();
    RadzenDataGrid<ViewModels.Exercice>? grid;
    ViewModels.Exercice? exerciceToUpdate;
    ViewModels.Exercice? exerciceToInsert;
    List<Results.BrokenRule> brokenRules = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await LoadDatas();
    }

    async Task LoadDatas()
    {
        var datas = await Mediator!.Send(new GetAllExercicesRequest());
        exerciceList = Mapper.Map<List<ViewModels.Exercice>>(datas);
    }

    async Task InsertRow()
    {
        var data = await Mediator!.Send(new Requests.CreateExerciceRequest());
        exerciceToInsert = Mapper!.Map<ViewModels.Exercice>(data);
        await grid!.InsertRow(exerciceToInsert);
    }

    async Task EditRow(ViewModels.Exercice exercice)
    {
        exerciceToUpdate = exercice;
        await grid!.EditRow(exercice);
    }

    async Task SaveRow(ViewModels.Exercice exercice)
    {
        if (exercice == exerciceToInsert)
        {
            exerciceToInsert = null;
        }

        exerciceToUpdate = null;

        var data = Mapper!.Map<Datas.ExerciceData>(exercice);
        var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.ExerciceData>(data));
        if (saveResult!.HasError)
        {
            brokenRules = saveResult.ErrorBrokenRuleList;
            return;
        }

        await grid!.UpdateRow(exercice);
    }

    void CancelEdit(ViewModels.Exercice exercice)
    {
        if (exercice == exerciceToInsert)
        {
            exerciceToInsert = null;
        }

        exerciceToUpdate = null;

        grid!.CancelEditRow(exercice);
    }

    async Task DeleteRow(ViewModels.Exercice exercice)
    {
        
    }

}