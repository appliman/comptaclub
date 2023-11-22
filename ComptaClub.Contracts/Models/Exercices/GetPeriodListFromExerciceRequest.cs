namespace ComptaClub.Contracts.Models.Exercices;
public record GetPeriodListFromExerciceRequest(Guid ExerciceId)
	: IRequest<List<PeriodFilter>>;