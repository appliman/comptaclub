using ComptaClub.Contracts.Models.Exercices;
using ComptaClub.Enums;

namespace ComptaClub.Handlers.Exercices;

internal class CreateExerciceRequestHandler : IRequestHandler<CreateExerciceRequest, ExerciceData>
{
	public Task<ExerciceData> Handle(CreateExerciceRequest request, CancellationToken cancellationToken)
	{
		var result = new ExerciceData();
		result.Id = Guid.NewGuid();
		result.CreationDate = DateTime.Today.ToDayId();
		result.Code = request.Code;
		result.Label = request.Label;
		result.InitialAmount = request.InitialAmount;
		result.BalanceAmount = request.InitialAmount;
		result.Active = request.Active;
		result.ExerciceState = ExerciceState.Current;
		result.StartDate = DateTime.Today.FirstDateOfCurrentYear();
		result.EndDate = DateTime.Today.LastDateOfCurrentYear();

		return Task.FromResult(result);
	}
}
