using AutoMapper;

using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Blazor.Pages;
public partial class Index
{
	[Inject] 
	IMediator Mediator { get; set; } = default!;

	[Inject]
	AutoMapper.IMapper Mapper { get; set; } = default!;

	ViewModels.Exercice currentExercice = new();

	protected override async Task OnInitializedAsync()
	{
		var exercice = await Mediator.Send(new GetActiveExerciceRequest());
		currentExercice = Mapper.Map<ViewModels.Exercice>(exercice);
	}
}