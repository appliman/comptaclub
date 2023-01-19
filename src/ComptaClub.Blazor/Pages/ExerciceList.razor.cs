using ComptaClub.Blazor.Extensions;
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


	List<ViewModels.Exercice>? exerciceList;
    RadzenDataGrid<ViewModels.Exercice>? grid;
    List<Results.BrokenRule> brokenRules = new();

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
        var deleteResult = await Mediator.Send(new DeleteExerciceRequest(exercice.Id));
        if (deleteResult.HasError)
        {
            NotificationService.NotifyError(deleteResult);
        }
        else if (deleteResult.HasWarning)
        {
            NotificationService.NotifyWarning(deleteResult);
        }
        await LoadDatas();
    }

	async Task ChangeActiveExercice(ChangeEventArgs args, ViewModels.Exercice exercice)
	{
        var changeResult = await Mediator.Send(new ChangeActiveExerciceRequest($"{args.Value}" == "on", exercice.Id));
        if (changeResult.HasError)
        {
            NotificationService.NotifyError(changeResult);
        }
        else if (changeResult.HasWarning)
        {
			NotificationService.NotifyWarning(changeResult);
		}
		await LoadDatas();
	}

}