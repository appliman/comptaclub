using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Results;

namespace ComptaClub.Requests;

public record ChangeActiveExerciceRequest : IRequest<CommandResult>
{
	public ChangeActiveExerciceRequest(bool active, Guid exerciceId)
	{
		this.Active = active;
		this.ExerciceId = exerciceId;
	}

	public bool Active { get; init; }
	public Guid ExerciceId { get; init; }
}
