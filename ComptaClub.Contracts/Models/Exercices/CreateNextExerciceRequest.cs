namespace ComptaClub.Contracts.Models.Exercices;

public record CreateNextExerciceRequest(Guid ExerciceId, string Code, string Label)
    : IRequest<ExerciceData?>;