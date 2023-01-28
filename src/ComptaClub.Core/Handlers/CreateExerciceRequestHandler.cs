using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ComptaClub.Models;
using ComptaClub.Requests;

using MediatR;

namespace ComptaClub.Handlers;

internal class CreateExerciceRequestHandler : IRequestHandler<Requests.CreateExerciceRequest, Datas.ExerciceData>
{
    public Task<Datas.ExerciceData> Handle(CreateExerciceRequest request, CancellationToken cancellationToken)
    {
        var result = new Datas.ExerciceData();
        result.Id = Guid.NewGuid();
        result.CreationDate = DateTime.Today.ToDayId();
        result.Code = request.Code;
        result.Label = request.Label;
        result.StartDate = request.StartDate;
        result.EndDate = request.EndDate;
        result.InitialAmount = request.InitialAmount;
        result.BalanceAmount = request.InitialAmount;
        result.Active = request.Active;
        return Task.FromResult(result);
    }
}
