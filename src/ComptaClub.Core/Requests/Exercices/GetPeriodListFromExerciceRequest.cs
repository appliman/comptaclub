using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComptaClub.Requests.Exercices;
public record GetPeriodListFromExerciceRequest(Guid ExerciceId) 
    : IRequest<List<Models.PeriodFilter>>;