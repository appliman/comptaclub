using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Requests;

/// <summary>
/// Recupère l'exercice en cours
/// <see cref="Handlers.GetActiveExerciceRequestHandler"/>
/// </summary>
public record GetActiveExerciceRequest : IRequest<Datas.ExerciceData?>
{
}
