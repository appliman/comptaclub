using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Contracts.Models.Exercices;

/// <summary>
/// Recupère l'exercice en cours
/// <see cref="Handlers.Exercices.GetActiveExerciceRequestHandler"/>
/// </summary>
public record GetActiveExerciceRequest
    : IRequest<ExerciceData?>;
