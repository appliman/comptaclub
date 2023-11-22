using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Exercices;

public record ChangeActiveExerciceRequest : IRequest<CommandResult>
{
    public ChangeActiveExerciceRequest(bool active, Guid exerciceId)
    {
        Active = active;
        ExerciceId = exerciceId;
    }

    public bool Active { get; init; }
    public Guid ExerciceId { get; init; }
}
