using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Exercices;
public record CloseExerciceRequest(Guid ExerciceId)
    : IRequest<CommandResult>;
