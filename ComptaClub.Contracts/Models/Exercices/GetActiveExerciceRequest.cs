namespace ComptaClub.Contracts.Models.Exercices;

/// <summary>
/// Recupère l'exercice en cours
/// <see cref="Handlers.Exercices.GetActiveExerciceRequestHandler"/>
/// </summary>
public record GetActiveExerciceRequest
	: IRequest<ExerciceData?>;
