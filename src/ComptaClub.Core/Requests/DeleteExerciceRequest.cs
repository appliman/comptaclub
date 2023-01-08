using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record DeleteExerciceRequest : IRequest<CommandResult>
{
	public DeleteExerciceRequest(Guid exerciceId)
	{
		this.ExerciceId = exerciceId;
	}

	public Guid ExerciceId { get; init; }
}
