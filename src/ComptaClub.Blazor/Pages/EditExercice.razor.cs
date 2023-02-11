using AutoMapper;

using ComptaClub.Blazor.Pages.Components;

namespace ComptaClub.Blazor.Pages
{
    public partial class EditExercice
    {
        [Parameter]
        public Guid? ExerciceId { get; set; }

		[Inject]
		AutoMapper.IMapper Mapper { get; set; } = default!;

		[Inject]
        MediatR.IMediator Mediator { get; set; } = default!;

		[Inject]
		NavigationManager NavigationManager { get; set; } = default!;


		ViewModels.Exercice exercice = new();
        CustomValidator customValidator = default!;

		protected override async Task OnInitializedAsync()
        {
            if (ExerciceId == null
                || ExerciceId == Guid.Empty)
            {
                var data = await Mediator.Send(new Requests.Exercices.CreateExerciceRequest());
				exercice = Mapper.Map<ViewModels.Exercice>(data);
			}
            else
            {
                var data = await Mediator.Send(new Requests.Exercices.GetExerciceByFilterRequest(f => f.Id == ExerciceId.Value));
                if (data != null)
                {
					exercice = Mapper.Map<ViewModels.Exercice>(data);
				}
			}
        }

        async Task ValidateAndSave()
        {
			var data = Mapper.Map<Datas.ExerciceData>(exercice);
			var saveResult = await Mediator!.Send(new Requests.SaveEntityRequest<Datas.ExerciceData>(data));
			if (saveResult!.HasError)
			{
                customValidator.DisplayErrors(saveResult.ErrorBrokenRuleList);
				return;
			}

            NavigationManager.NavigateTo("/exercices");
        }
    }
}