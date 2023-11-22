using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Results;

namespace ComptaClub.Contracts.Models.Exercices;

public record DeleteExerciceRequest : IRequest<CommandResult>
{
    public DeleteExerciceRequest(Guid exerciceId)
    {
        ExerciceId = exerciceId;
    }

    public Guid ExerciceId { get; init; }
}
