namespace ComptaClub.Requests.Exercices;

public record CreateNextExerciceRequest(Guid ExerciceId, string Code, string Label)
	: IRequest<ExerciceData?>;