using ComptaClub.Results;

namespace ComptaClub.Requests.Exercices;
public record CloseExerciceRequest(Guid ExerciceId)
    : IRequest<CommandResult>;
