using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Enums;
using ComptaClub.Models;
using ComptaClub.Requests.Exercices;
using MediatR;

namespace ComptaClub.Handlers.Exercices;

internal class CreateExerciceRequestHandler : IRequestHandler<CreateExerciceRequest, ExerciceData>
{
    public Task<ExerciceData> Handle(CreateExerciceRequest request, CancellationToken cancellationToken)
    {
        var result = new ExerciceData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();
        result.Code = request.Code;
        result.Label = request.Label;
        result.StartDate = request.StartDate;
        result.EndDate = request.EndDate;
        result.InitialAmount = request.InitialAmount;
        result.BalanceAmount = request.InitialAmount;
        result.Active = request.Active;
        result.ExerciceState = ExerciceState.Current;
        return Task.FromResult(result);
    }
}
