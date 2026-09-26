
using ComptaClub.Blazor.Pages.Components;
using ComptaClub.Contracts.Models.Exercices;

namespace ComptaClub.Blazor.Pages
{
    public partial class EditExercice
    {
        [Parameter]
        public Guid? ExerciceId { get; set; }


		[Inject]
        ChannelMediator.IMediator Mediator { get; set; } = default!;

		[Inject]
		NavigationManager NavigationManager { get; set; } = default!;


		ViewModels.Exercice exercice = new();
        CustomValidator customValidator = default!;

		protected override async Task OnInitializedAsync()
        {
            if (ExerciceId == null
                || ExerciceId == Guid.Empty)
            {
                var data = await Mediator.Send(new CreateExerciceRequest());
				exercice = Mapping.Profile.ToViewModel(data);
			}
            else
            {
                var data = await Mediator.Send(new GetExerciceByFilterRequest(f => f.Id == ExerciceId.Value));
                if (data != null)
                {
					exercice = Mapping.Profile.ToViewModel(data);
				}
			}
        }

        async Task ValidateAndSave()
        {
			var data = Mapping.Profile.ToData(exercice);
			var saveResult = await Mediator!.Send(new SaveEntityRequest<Datas.ExerciceData>(data));
			if (saveResult!.HasError)
			{
                customValidator.DisplayErrors(saveResult.ErrorBrokenRuleList);
				return;
			}

            NavigationManager.NavigateTo("/exercices");
        }
    }
}