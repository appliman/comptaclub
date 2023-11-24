using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Exercices;

public record DeleteExerciceRequest : IRequest<CommandResult>
{
	public DeleteExerciceRequest(Guid exerciceId)
	{
		ExerciceId = exerciceId;
	}

	public Guid ExerciceId { get; init; }
}
