using MediatR;

namespace ComptaClub.Blazor.Pages.Dialogs;

public partial class ExerciceSelectorDialog
{
	[Inject]
	IMediator Mediator { get; set; } = default!;

	[Inject]
	public DialogService DialogService { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	List<ViewModels.Exercice>? exerciceList;
	RadzenDataGrid<ViewModels.Exercice>? grid = new();
	IList<ViewModels.Exercice>? selectedRow;

	async Task LoadDatas(LoadDataArgs args)
	{
		var datas = await Mediator!.Send(new Requests.Exercices.GetAllExercicesRequest());
		datas.RemoveAll(i => i.ExerciceState != ExerciceState.Closed);
		exerciceList = Mapper.Map<List<ViewModels.Exercice>>(datas);
		var rowIndex = 1;
		foreach (var item in exerciceList)
		{
			item.RowIndex = rowIndex++;
		}
	}

	Task Select()
	{
		if (selectedRow != null
			&& selectedRow.Any())
		{
			DialogService.Close(selectedRow.First());
		}
		else
		{
			DialogService.Close();
		}
		return Task.CompletedTask;
	}
}