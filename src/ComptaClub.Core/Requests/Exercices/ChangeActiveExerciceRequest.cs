using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests.Exercices;

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
