using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComptaClub.Contracts.Models;

namespace ComptaClub.Contracts.Models.Exercices;
public record GetPeriodListFromExerciceRequest(Guid ExerciceId)
    : IRequest<List<PeriodFilter>>;